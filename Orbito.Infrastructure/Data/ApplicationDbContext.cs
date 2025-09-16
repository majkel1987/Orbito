using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Orbito.Domain.Entities;
using Orbito.Domain.Identity;
using Orbito.Infrastructure.Data.Configurations.ValueObjects;

namespace Orbito.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IMultiTenantDbContext
    {
        public ITenantInfo TenantInfo { get; }
        public TenantMismatchMode TenantMismatchMode { get; set; } = TenantMismatchMode.Throw;
        public TenantNotSetMode TenantNotSetMode { get; set; } = TenantNotSetMode.Throw;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            ITenantInfo tenantInfo) : base(options)
        {
            TenantInfo = tenantInfo;
        }

        // Domain entities DbSets
        public DbSet<Provider> Providers { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Apply all entity configurations automatically
            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // Configure Value Objects
            builder.ConfigureValueObjects();

            // Configure multi-tenancy
            ConfigureMultiTenancy(builder);

            // Seed default data
            SeedDefaultData(builder);
        }

        private void ConfigureMultiTenancy(ModelBuilder builder)
        {
            // Multi-tenant configuration using Finbuckle
            builder.Entity<Client>().IsMultiTenant();
            builder.Entity<SubscriptionPlan>().IsMultiTenant();
            builder.Entity<Subscription>().IsMultiTenant();
            builder.Entity<Payment>().IsMultiTenant();

            // Providers are not multi-tenant as they ARE the tenants
            // ApplicationUsers are not multi-tenant globally filtered

            // Global query filters for ApplicationRole
            builder.Entity<ApplicationRole>()
                .HasQueryFilter(r => TenantInfo.Id == null ||
                                     r.TenantId == null ||
                                     r.TenantId == Guid.Parse(TenantInfo.Id!));
        }

        private void SeedDefaultData(ModelBuilder builder)
        {
            // Default roles
            var platformAdminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var providerRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var clientRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");

            builder.Entity<ApplicationRole>().HasData(
                new ApplicationRole("PlatformAdmin")
                {
                    Id = platformAdminRoleId,
                    NormalizedName = "PLATFORMADMIN",
                    Description = "Platform Administrator",
                    TenantId = null,
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new ApplicationRole("Provider")
                {
                    Id = providerRoleId,
                    NormalizedName = "PROVIDER",
                    Description = "Service Provider",
                    TenantId = null,
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new ApplicationRole("Client")
                {
                    Id = clientRoleId,
                    NormalizedName = "CLIENT",
                    Description = "Client User",
                    TenantId = null,
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Auto-set TenantId for new multi-tenant entities
            if (TenantInfo?.Id != null && Guid.TryParse(TenantInfo.Id, out var tenantGuid))
            {
                foreach (var entry in ChangeTracker.Entries()
                    .Where(e => e.State == EntityState.Added))
                {
                    if (entry.Entity is Client client && client.TenantId.Value == Guid.Empty)
                    {
                        // Używamy reflection żeby ustawić prywatne pole
                        var tenantIdProperty = typeof(Client).GetProperty(nameof(Client.TenantId));
                        tenantIdProperty?.SetValue(client, Domain.ValueObjects.TenantId.Create(tenantGuid));
                    }
                    else if (entry.Entity is SubscriptionPlan plan && plan.TenantId.Value == Guid.Empty)
                    {
                        var tenantIdProperty = typeof(SubscriptionPlan).GetProperty(nameof(SubscriptionPlan.TenantId));
                        tenantIdProperty?.SetValue(plan, Domain.ValueObjects.TenantId.Create(tenantGuid));
                    }
                    else if (entry.Entity is Subscription subscription && subscription.TenantId.Value == Guid.Empty)
                    {
                        var tenantIdProperty = typeof(Subscription).GetProperty(nameof(Subscription.TenantId));
                        tenantIdProperty?.SetValue(subscription, Domain.ValueObjects.TenantId.Create(tenantGuid));
                    }
                    else if (entry.Entity is Payment payment && payment.TenantId.Value == Guid.Empty)
                    {
                        var tenantIdProperty = typeof(Payment).GetProperty(nameof(Payment.TenantId));
                        tenantIdProperty?.SetValue(payment, Domain.ValueObjects.TenantId.Create(tenantGuid));
                    }
                }
            }

            // Set timestamps for new entities
            SetTimestamps();

            return await base.SaveChangesAsync(cancellationToken);
        }

        private void SetTimestamps()
        {
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added))
            {
                if (entry.Entity.GetType().GetProperty("CreatedAt") != null)
                {
                    entry.Property("CreatedAt").CurrentValue = now;
                }
            }

            foreach (var entry in ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified))
            {
                if (entry.Entity.GetType().GetProperty("UpdatedAt") != null)
                {
                    entry.Property("UpdatedAt").CurrentValue = now;
                }
            }
        }
    }
}
