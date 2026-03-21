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
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Orbito.Infrastructure.Data
{
    /// <summary>
    /// Application database context with multi-tenancy support, audit trails, and security validations
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, ITenantValidationBypass
    {
        private readonly ITenantProvider _tenantProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ApplicationDbContext> _logger;

        // Security constants
        private static readonly Guid AdminTenantId = Guid.Empty;

        // Performance cache - cleared automatically between requests (DbContext is scoped)
        // Thread-safe cache using Lazy<T> to prevent race conditions
        private readonly Lazy<Guid?> _cachedTenantId;

        // Flag to skip tenant validation for admin setup operations
        // Used during initial admin seeding when no tenant context exists yet
        private bool _skipTenantValidation = false;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            ITenantProvider tenantProvider,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ApplicationDbContext> logger) : base(options)
        {
            _tenantProvider = tenantProvider ?? throw new ArgumentNullException(nameof(tenantProvider));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Initialize thread-safe cache
            _cachedTenantId = new Lazy<Guid?>(() => GetCurrentTenantIdSafe());
        }

        /// <summary>
        /// Temporarily disables tenant validation for admin setup operations
        /// Should only be used during initial admin seeding when no tenant context exists
        /// </summary>
        public void SkipTenantValidation()
        {
            _skipTenantValidation = true;
            _logger.LogWarning("Tenant validation disabled for admin setup operation");
        }

        /// <summary>
        /// Re-enables tenant validation after admin setup operations
        /// Should be called after admin setup is complete to restore security checks
        /// </summary>
        public void ResetTenantValidation()
        {
            _skipTenantValidation = false;
            _logger.LogDebug("Tenant validation re-enabled after admin setup operation");
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
        public DbSet<ReconciliationReport> ReconciliationReports { get; set; } = null!;
        public DbSet<PaymentDiscrepancy> PaymentDiscrepancies { get; set; } = null!;
        public DbSet<TeamMember> TeamMembers { get; set; } = null!;

        // Platform subscription entities (Provider pays Orbito)
        public DbSet<PlatformPlan> PlatformPlans { get; set; } = null!;
        public DbSet<ProviderSubscription> ProviderSubscriptions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Exclude TenantId from being treated as an entity
            builder.Ignore<TenantId>();

            // Apply all entity configurations automatically
            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // Configure Value Objects
            builder.ConfigureValueObjects();

            // Configure Enum Converters (must be called AFTER ApplyConfigurationsFromAssembly)
            builder.ConfigureEnumConverters();

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
        /// IMPORTANT: Identity entities (ApplicationUser, ApplicationRole) should NOT have query filters
        /// UserManager and RoleManager already handle security and tenant isolation
        /// Query filters cause EF Core translation issues with nullable TenantId value objects
        /// </summary>
        private void ConfigureIdentityTenantFiltering(ModelBuilder builder)
        {
            // NO QUERY FILTERS FOR IDENTITY ENTITIES
            // Reasons:
            // 1. UserManager/RoleManager already provide security layer
            // 2. EF.Property<Guid?> with value converters cannot be translated to SQL
            // 3. Identity operations need to work during registration (no tenant context)
            // 4. Manual tenant filtering in repositories is more explicit and maintainable

            // Security is handled by:
            // - TenantMiddleware sets tenant context from JWT token
            // - Controllers validate user's tenant matches requested resources
            // - SaveChanges validates tenant context for write operations
        }

        /// <summary>
        /// Configures tenant filtering for all domain entities
        /// IMPORTANT: Query filters are NOT used due to EF Core translation issues with TenantId value converters
        /// </summary>
        private void ConfigureDomainTenantFiltering(ModelBuilder builder)
        {
            // NO QUERY FILTERS FOR DOMAIN ENTITIES
            //
            // Reasons:
            // 1. TenantId is a value object with ValueConverter (TenantId.Create())
            // 2. EF.Property<Guid>(entity, "TenantId") with value converters causes InvalidCastException
            // 3. EF Core cannot properly translate value converter expressions to SQL
            // 4. Manual tenant filtering in repositories is more explicit, testable, and maintainable
            //
            // Security is handled by:
            // - Each repository manually filters by TenantId (explicit WHERE clause)
            // - TenantMiddleware validates tenant context from JWT
            // - SaveChanges validates TenantId for all write operations
            // - RBAC at controller level
            //
            // Affected entities (ALL have manual filtering in repositories):
            // - Provider: IS a tenant, no filtering needed
            // - Client: Manual filtering in ClientRepository
            // - SubscriptionPlan: Manual filtering in SubscriptionPlanRepository
            // - Subscription: Manual filtering in SubscriptionRepository
            // - Payment: Manual filtering in PaymentRepository
            // - PaymentMethod: Manual filtering in PaymentMethodRepository
            // - PaymentHistory: Manual filtering in PaymentHistoryRepository
            // - PaymentWebhookLog: Manual filtering in PaymentWebhookLogRepository
            // - EmailNotification: Manual filtering in EmailNotificationRepository
            // - PaymentRetrySchedule: Manual filtering in PaymentRetryScheduleRepository
            // - ReconciliationReport: Manual filtering in ReconciliationReportRepository
            // - PaymentDiscrepancy: Manual filtering in PaymentDiscrepancyRepository
            // - TeamMember: Manual filtering in TeamMemberRepository
        }

        /// <summary>
        /// DEPRECATED: SetTenantQueryFilterGeneric is no longer used
        /// Query filters with EF.Property and value converters cause InvalidCastException
        /// All tenant filtering is now done manually in repositories
        /// </summary>
        [Obsolete("Query filters are disabled. Use manual filtering in repositories.", error: true)]
        private void SetTenantQueryFilterGeneric<T>(ModelBuilder builder) where T : class, IMustHaveTenant
        {
            throw new NotSupportedException(
                "Query filters are not supported due to EF Core translation issues with value converters. " +
                "Use manual TenantId filtering in repositories instead.");
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
                // Use thread-safe cached value
                var cachedValue = _cachedTenantId.Value;
                if (cachedValue.HasValue)
                    return cachedValue.Value;

                var tenantId = _tenantProvider.GetCurrentTenantIdAsGuid();

                // For query filters, invalid context = admin access (safest default)
                if (tenantId == Guid.Empty)
                {
                    _logger.LogWarning("Tenant ID not available in query filter - granting admin access");
                    return AdminTenantId;
                }

                return tenantId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in query filter tenant resolution - granting admin access");
                return AdminTenantId;
            }
        }

        /// <summary>
        /// Gets current tenant ID safely without throwing exceptions
        /// </summary>
        /// <returns>Current tenant ID or null if not available</returns>
        private Guid? GetCurrentTenantIdSafe()
        {
            try
            {
                var tenantId = _tenantProvider.GetCurrentTenantIdAsGuid();
                return tenantId == Guid.Empty ? null : tenantId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current tenant ID safely");
                return null;
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
            // Use thread-safe cached value if available and valid (not admin)
            var cachedValue = _cachedTenantId.Value;
            if (cachedValue.HasValue && cachedValue.Value != AdminTenantId)
                return cachedValue.Value;

            var tenantId = _tenantProvider.GetCurrentTenantIdAsGuid();

            if (tenantId == Guid.Empty)
            {
                _logger.LogError("Tenant ID not available for write operation - SECURITY VIOLATION");
                throw new UnauthorizedAccessException("Tenant context required for data modification");
            }

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
            // Skip validation if explicitly disabled (for admin setup operations)
            if (_skipTenantValidation)
            {
                _logger.LogDebug("Skipping tenant validation - explicitly disabled for admin setup");
                return;
            }

            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
                .ToList();

            // Nothing to validate - empty changeset
            if (!entries.Any())
            {
                _logger.LogDebug("Skipping tenant validation - no changes to save");
                return;
            }

            // Check if we're creating a Provider (Provider creates its own tenant)
            var creatingProvider = entries.Any(e => e.Entity is Provider && e.State == EntityState.Added);

            // Skip tenant validation when creating a Provider (Provider IS a tenant)
            if (creatingProvider)
            {
                _logger.LogDebug("Skipping tenant validation - creating Provider entity");
                return;
            }

            // Check if we're managing Identity entities during registration flow
            var hasIdentityEntities = entries.Any(e => e.Entity is ApplicationUser ||
                                                       e.Entity is ApplicationRole ||
                                                       e.Entity.GetType().Namespace == "Microsoft.AspNetCore.Identity.EntityFrameworkCore");

            // Check if we're in a registration flow (Provider + Identity entities)
            var hasProvider = entries.Any(e => e.Entity is Provider);
            var allEntitiesAreRegistrationRelated = entries.All(e =>
                e.Entity is Provider ||
                e.Entity is ApplicationUser ||
                e.Entity is ApplicationRole ||
                e.Entity.GetType().Namespace == "Microsoft.AspNetCore.Identity.EntityFrameworkCore");

            // Skip tenant validation during registration/rollback flow
            if (allEntitiesAreRegistrationRelated && (hasProvider || hasIdentityEntities))
            {
                _logger.LogDebug("Skipping tenant validation - registration flow detected (Provider + Identity operations)");
                return;
            }

            // CRITICAL FIX: Allow ALL Identity operations regardless of tenant context
            // This includes user registration, role assignment, login, profile updates, etc.
            // Identity operations are self-contained and don't need tenant validation
            // even if the User entity has a TenantId assigned
            if (hasIdentityEntities && !hasProvider)
            {
                // Check if ALL entities are Identity-related
                // IMPORTANT: IdentityUserRole<T> and other Identity join tables are also Identity entities
                var onlyIdentityEntities = entries.All(e =>
                    e.Entity is ApplicationUser ||
                    e.Entity is ApplicationRole ||
                    e.Entity.GetType().Namespace == "Microsoft.AspNetCore.Identity.EntityFrameworkCore" ||
                    e.Entity.GetType().Name.StartsWith("Identity", StringComparison.OrdinalIgnoreCase));

                if (onlyIdentityEntities)
                {
                    _logger.LogDebug("Skipping tenant validation - pure Identity operations (User: {UserCount}, Role: {RoleCount}, Other: {OtherCount})",
                        entries.Count(e => e.Entity is ApplicationUser),
                        entries.Count(e => e.Entity is ApplicationRole),
                        entries.Count(e => e.Entity.GetType().Namespace == "Microsoft.AspNetCore.Identity.EntityFrameworkCore" || 
                                          e.Entity.GetType().Name.StartsWith("Identity", StringComparison.OrdinalIgnoreCase)));
                    return;
                }
            }

            // Try to get tenant ID safely first
            var tenantId = GetCurrentTenantIdSafe();
            if (!tenantId.HasValue)
            {
                // No tenant context and we have non-Identity entities - this is an error
                _logger.LogError("Tenant context required for non-Identity operations");
                throw new UnauthorizedAccessException("Tenant context required for data modification");
            }

            var currentTenantId = tenantId.Value;

            // Skip validation for admin context
            if (currentTenantId == AdminTenantId)
            {
                _logger.LogDebug("Skipping tenant validation for admin context");
                return;
            }

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
                var user = _httpContextAccessor.HttpContext?.User;
                if (user?.Identity?.IsAuthenticated != true)
                {
                    return Guid.Empty;
                }

                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Guid.Empty;
                }

                return userId;
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
        /// Note: With Lazy<T>, cache is automatically cleared when DbContext is disposed
        /// </summary>
        internal void ClearTenantCache()
        {
            // With Lazy<T>, the cache is automatically cleared when DbContext is disposed
            // No manual clearing needed - this method is kept for compatibility
            _logger.LogDebug("Tenant cache will be cleared on DbContext disposal");
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