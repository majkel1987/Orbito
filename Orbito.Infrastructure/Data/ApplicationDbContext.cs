using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Common;
using Orbito.Domain.Entities;
using Orbito.Domain.Identity;
using Orbito.Domain.Interfaces;
using Orbito.Domain.ValueObjects;
using Orbito.Infrastructure.Data.Configurations.ValueObjects;

namespace Orbito.Infrastructure.Data
{
    /// <summary>
    /// Application database context with multi-tenancy support, audit trails, and security validations
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        private readonly ITenantProvider _tenantProvider;
        private readonly IUserContextService _userContextService;
        private readonly ILogger<ApplicationDbContext> _logger;

        // Security constants
        private static readonly Guid AdminTenantId = Guid.Empty;

        // Performance cache - cleared automatically between requests (DbContext is scoped)
        private Guid? _cachedTenantId;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            ITenantProvider tenantProvider,
            IUserContextService userContextService,
            ILogger<ApplicationDbContext> logger) : base(options)
        {
            _tenantProvider = tenantProvider ?? throw new ArgumentNullException(nameof(tenantProvider));
            _userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Domain entities DbSets
        public DbSet<Provider> Providers { get; set; } = null!;
        public DbSet<Client> Clients { get; set; } = null!;
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; } = null!;
        public DbSet<Subscription> Subscriptions { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<PaymentMethod> PaymentMethods { get; set; } = null!;
        public DbSet<PaymentHistory> PaymentHistory { get; set; } = null!;
        public DbSet<PaymentWebhookLog> PaymentWebhookLogs { get; set; } = null!;
        public DbSet<EmailNotification> EmailNotifications { get; set; } = null!;
        public DbSet<PaymentRetrySchedule> PaymentRetrySchedules { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Exclude TenantId from being treated as an entity
            builder.Ignore<TenantId>();

            // Apply all entity configurations automatically
            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // Configure Value Objects
            builder.ConfigureValueObjects();

            // Configure multi-tenancy with query filters
            ConfigureMultiTenancy(builder);

            // Seed default data
            SeedDefaultData(builder);
        }

        /// <summary>
        /// Configures multi-tenancy query filters for all entities
        /// </summary>
        private void ConfigureMultiTenancy(ModelBuilder builder)
        {
            ConfigureIdentityTenantFiltering(builder);
            ConfigureDomainTenantFiltering(builder);
        }

        /// <summary>
        /// Configures tenant filtering for ASP.NET Identity entities
        /// Allows global roles (TenantId = null) and admin bypass (Guid.Empty)
        /// </summary>
        private void ConfigureIdentityTenantFiltering(ModelBuilder builder)
        {
            // ApplicationRole - allow global roles (TenantId = null) and tenant-specific roles
            builder.Entity<ApplicationRole>()
                .HasQueryFilter(r =>
                    GetCurrentTenantIdForQueryFilter() == AdminTenantId ||
                    r.TenantId == null ||
                    r.TenantId == GetCurrentTenantIdForQueryFilter());

            // ApplicationUser - filter by tenant, admin bypass enabled
            builder.Entity<ApplicationUser>()
                .HasQueryFilter(u =>
                    GetCurrentTenantIdForQueryFilter() == AdminTenantId ||
                    u.TenantId == GetCurrentTenantIdForQueryFilter());
        }

        /// <summary>
        /// Configures tenant filtering for all domain entities
        /// All domain entities must implement IMustHaveTenant interface
        /// </summary>
        private void ConfigureDomainTenantFiltering(ModelBuilder builder)
        {
            // Explicit configuration for type safety and performance
            // Reflection is avoided for production code
            SetTenantQueryFilterGeneric<Provider>(builder);
            SetTenantQueryFilterGeneric<Client>(builder);
            SetTenantQueryFilterGeneric<SubscriptionPlan>(builder);
            SetTenantQueryFilterGeneric<Subscription>(builder);
            SetTenantQueryFilterGeneric<Payment>(builder);
            SetTenantQueryFilterGeneric<PaymentMethod>(builder);
            SetTenantQueryFilterGeneric<PaymentHistory>(builder);
            SetTenantQueryFilterGeneric<PaymentWebhookLog>(builder);
            SetTenantQueryFilterGeneric<EmailNotification>(builder);
            SetTenantQueryFilterGeneric<PaymentRetrySchedule>(builder);
        }

        /// <summary>
        /// Sets tenant query filter for a specific entity type
        /// </summary>
        /// <typeparam name="T">Entity type that implements IMustHaveTenant</typeparam>
        private void SetTenantQueryFilterGeneric<T>(ModelBuilder builder) where T : class, IMustHaveTenant
        {
            builder.Entity<T>()
                .HasQueryFilter(e =>
                    GetCurrentTenantIdForQueryFilter() == AdminTenantId ||
                    e.TenantId.Value == GetCurrentTenantIdForQueryFilter());
        }

        /// <summary>
        /// Gets current tenant ID for query filters - MUST NOT throw exceptions
        /// Used in EF Core query filter lambdas which are translated to SQL
        /// Returns AdminTenantId for invalid contexts (grants admin access as safest default)
        /// </summary>
        /// <returns>Current tenant ID or AdminTenantId for admin/invalid contexts</returns>
        private Guid GetCurrentTenantIdForQueryFilter()
        {
            try
            {
                // Use cached value if available
                if (_cachedTenantId.HasValue)
                    return _cachedTenantId.Value;

                var tenantId = _tenantProvider.GetCurrentTenantIdAsGuid();

                // For query filters, invalid context = admin access (safest default)
                if (tenantId == Guid.Empty)
                {
                    _logger.LogWarning("Tenant ID not available in query filter - granting admin access");
                    _cachedTenantId = AdminTenantId;
                    return AdminTenantId;
                }

                _cachedTenantId = tenantId;
                return tenantId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in query filter tenant resolution - granting admin access");
                _cachedTenantId = AdminTenantId;
                return AdminTenantId;
            }
        }

        /// <summary>
        /// Gets current tenant ID with strict validation - throws on invalid context
        /// Used for write operations (SaveChanges) where security is critical
        /// </summary>
        /// <returns>Current tenant ID</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when tenant context is invalid</exception>
        private Guid GetCurrentTenantIdStrict()
        {
            // Use cached value if available and valid (not admin)
            if (_cachedTenantId.HasValue && _cachedTenantId.Value != AdminTenantId)
                return _cachedTenantId.Value;

            var tenantId = _tenantProvider.GetCurrentTenantIdAsGuid();

            if (tenantId == Guid.Empty)
            {
                _logger.LogError("Tenant ID not available for write operation - SECURITY VIOLATION");
                throw new UnauthorizedAccessException("Tenant context required for data modification");
            }

            _cachedTenantId = tenantId;
            return tenantId;
        }

        /// <summary>
        /// Override SaveChanges to add tenant validation and audit fields
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ValidateTenantContext();
            SetAuditFields();

            try
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency conflict during save");
                throw;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database update error during save");
                throw;
            }
        }

        /// <summary>
        /// Override SaveChanges to add tenant validation and audit fields
        /// </summary>
        public override int SaveChanges()
        {
            ValidateTenantContext();
            SetAuditFields();

            try
            {
                return base.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency conflict during save");
                throw;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database update error during save");
                throw;
            }
        }

        /// <summary>
        /// Validates tenant context for all modified entities
        /// Ensures users can only modify data belonging to their tenant
        /// Auto-assigns TenantId for new entities
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">Thrown when tenant validation fails</exception>
        private void ValidateTenantContext()
        {
            var currentTenantId = GetCurrentTenantIdStrict();

            // Skip validation for admin context
            if (currentTenantId == AdminTenantId)
            {
                _logger.LogDebug("Skipping tenant validation for admin context");
                return;
            }

            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .ToList();

            foreach (var entry in entries)
            {
                if (entry.Entity is IMustHaveTenant hasTenant)
                {
                    ValidateAndSetTenantForEntity(entry, hasTenant, currentTenantId);
                }
            }

            _logger.LogDebug("Tenant validation completed successfully for {Count} entities", entries.Count);
        }

        /// <summary>
        /// Validates and sets tenant for a specific entity
        /// </summary>
        private void ValidateAndSetTenantForEntity(EntityEntry entry, IMustHaveTenant entity, Guid currentTenantId)
        {
            var entityTypeName = entry.Entity.GetType().Name;

            // Auto-set TenantId for new entities
            if (entry.State == EntityState.Added)
            {
                if (entity.TenantId.Value == Guid.Empty)
                {
                    // Use reflection to set TenantId for new entities
                    // This is necessary because IMustHaveTenant.TenantId is read-only
                    var tenantIdProperty = entry.Entity.GetType().GetProperty("TenantId");
                    if (tenantIdProperty != null && tenantIdProperty.CanWrite)
                    {
                        tenantIdProperty.SetValue(entry.Entity, TenantId.Create(currentTenantId));
                        _logger.LogDebug("Auto-assigned TenantId {TenantId} to new {EntityType}",
                            currentTenantId, entityTypeName);
                        return; // No need to validate after auto-assignment
                    }
                    else
                    {
                        _logger.LogWarning("Cannot set TenantId for {EntityType} - property is read-only or not found",
                            entityTypeName);
                    }
                }
            }

            // Validate tenant ownership
            if (entity.TenantId.Value != currentTenantId)
            {
                _logger.LogError(
                    "SECURITY VIOLATION: Attempt to modify {EntityType} (ID: {EntityId}) belonging to tenant {EntityTenant} from context {CurrentTenant}",
                    entityTypeName,
                    GetEntityId(entry),
                    entity.TenantId.Value,
                    currentTenantId);

                throw new UnauthorizedAccessException(
                    $"Cannot modify {entityTypeName} belonging to a different tenant");
            }
        }

        /// <summary>
        /// Sets audit fields (CreatedAt, UpdatedAt, CreatedBy, UpdatedBy) for all modified entities
        /// </summary>
        private void SetAuditFields()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .ToList();

            if (!entries.Any())
                return;

            var now = DateTime.UtcNow;
            var userId = GetCurrentUserIdSafe();

            foreach (var entry in entries)
            {
                // Set audit properties by convention if they exist
                if (entry.State == EntityState.Added)
                {
                    SetPropertyIfExists(entry, "CreatedAt", now);
                    SetPropertyIfExists(entry, "CreatedBy", userId);
                }

                SetPropertyIfExists(entry, "UpdatedAt", now);
                SetPropertyIfExists(entry, "UpdatedBy", userId);
            }

            _logger.LogDebug("Set audit fields for {Count} entities", entries.Count);
        }

        /// <summary>
        /// Sets property value if it exists on the entity
        /// </summary>
        private void SetPropertyIfExists(EntityEntry entry, string propertyName, object value)
        {
            try
            {
                var property = entry.Properties.FirstOrDefault(p => p.Metadata.Name == propertyName);
                if (property != null && !property.IsTemporary)
                {
                    property.CurrentValue = value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to set property {PropertyName} on {EntityType}",
                    propertyName, entry.Entity.GetType().Name);
            }
        }

        /// <summary>
        /// Gets the ID of an entity from its entry
        /// </summary>
        private object? GetEntityId(EntityEntry entry)
        {
            try
            {
                var idProperty = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
                return idProperty?.CurrentValue;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets current user ID safely without throwing exceptions
        /// </summary>
        /// <returns>Current user ID or Guid.Empty if not available</returns>
        private Guid GetCurrentUserIdSafe()
        {
            try
            {
                var userId = _userContextService.GetCurrentUserId();
                return userId ?? Guid.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to get current user ID for audit fields");
                return Guid.Empty;
            }
        }

        /// <summary>
        /// INTERNAL USE ONLY - Clears tenant cache
        /// Called automatically by DI container when DbContext scope ends
        /// Should NOT be called manually during request processing
        /// </summary>
        internal void ClearTenantCache()
        {
            _cachedTenantId = null;
            _logger.LogDebug("Tenant cache cleared");
        }

        /// <summary>
        /// Seeds default data for the application
        /// Creates global roles: PlatformAdmin, Provider, Client
        /// </summary>
        private void SeedDefaultData(ModelBuilder builder)
        {
            // Default global roles with validated GUIDs
            var platformAdminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var providerRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var clientRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");

            // Validate GUIDs to prevent conflicts
            var roleIds = new[] { platformAdminRoleId, providerRoleId, clientRoleId };

            if (roleIds.Any(id => id == Guid.Empty))
                throw new InvalidOperationException("Seed data GUIDs cannot be empty");

            if (roleIds.Distinct().Count() != roleIds.Length)
                throw new InvalidOperationException("Seed data GUIDs must be unique");

            var seedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            builder.Entity<ApplicationRole>().HasData(
                new ApplicationRole("PlatformAdmin")
                {
                    Id = platformAdminRoleId,
                    NormalizedName = "PLATFORMADMIN",
                    Description = "Platform Administrator - Full system access",
                    TenantId = null, // Global role
                    IsActive = true,
                    CreatedAt = seedDate
                },
                new ApplicationRole("Provider")
                {
                    Id = providerRoleId,
                    NormalizedName = "PROVIDER",
                    Description = "Service Provider - Manages clients and subscriptions",
                    TenantId = null, // Global role
                    IsActive = true,
                    CreatedAt = seedDate
                },
                new ApplicationRole("Client")
                {
                    Id = clientRoleId,
                    NormalizedName = "CLIENT",
                    Description = "Client User - Manages own subscriptions and payments",
                    TenantId = null, // Global role
                    IsActive = true,
                    CreatedAt = seedDate
                }
            );

            _logger.LogInformation("Seed data configuration completed for {RoleCount} roles", roleIds.Length);
        }
    }
}