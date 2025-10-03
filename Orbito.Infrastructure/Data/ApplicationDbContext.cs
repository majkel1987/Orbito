using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.Identity;
using Orbito.Infrastructure.Data.Configurations.ValueObjects;
using System.Linq.Expressions;

namespace Orbito.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        private readonly ITenantProvider _tenantProvider;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            ITenantProvider tenantProvider) : base(options)
        {
            _tenantProvider = tenantProvider;
        }

        // Domain entities DbSets
        public DbSet<Provider> Providers { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<PaymentHistory> PaymentHistory { get; set; }
        public DbSet<PaymentWebhookLog> PaymentWebhookLogs { get; set; }

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
            // Global query filters for multi-tenancy using ITenantProvider
            // CRITICAL: Each lambda expression is evaluated per query, not once during model building
            
            // For ApplicationRole - allow global roles (TenantId = null) and tenant-specific roles
            builder.Entity<ApplicationRole>()
                .HasQueryFilter(r => r.TenantId == null || r.TenantId == _tenantProvider.GetCurrentTenantIdAsGuid());

            // For ApplicationUser - filter by tenant
            builder.Entity<ApplicationUser>()
                .HasQueryFilter(u => u.TenantId == _tenantProvider.GetCurrentTenantIdAsGuid());

            // For domain entities with TenantId value object
            // Each method call is evaluated per query, ensuring current tenant context
            builder.Entity<Provider>()
                .HasQueryFilter(p => p.TenantId.Value == _tenantProvider.GetCurrentTenantIdAsGuid());

            builder.Entity<Client>()
                .HasQueryFilter(c => c.TenantId.Value == _tenantProvider.GetCurrentTenantIdAsGuid());

            builder.Entity<SubscriptionPlan>()
                .HasQueryFilter(p => p.TenantId.Value == _tenantProvider.GetCurrentTenantIdAsGuid());

            builder.Entity<Subscription>()
                .HasQueryFilter(s => s.TenantId.Value == _tenantProvider.GetCurrentTenantIdAsGuid());

            builder.Entity<Payment>()
                .HasQueryFilter(p => p.TenantId.Value == _tenantProvider.GetCurrentTenantIdAsGuid());

            builder.Entity<PaymentMethod>()
                .HasQueryFilter(pm => pm.TenantId.Value == _tenantProvider.GetCurrentTenantIdAsGuid());

            builder.Entity<PaymentHistory>()
                .HasQueryFilter(ph => ph.TenantId.Value == _tenantProvider.GetCurrentTenantIdAsGuid());

            builder.Entity<PaymentWebhookLog>()
                .HasQueryFilter(pwl => pwl.TenantId.Value == _tenantProvider.GetCurrentTenantIdAsGuid());
        }

        private void SeedDefaultData(ModelBuilder builder)
        {
            // Default global roles
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

    }
}
