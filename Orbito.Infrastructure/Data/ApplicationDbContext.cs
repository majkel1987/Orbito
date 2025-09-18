using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Orbito.Domain.Entities;
using Orbito.Domain.Identity;
using Orbito.Infrastructure.Data.Configurations.ValueObjects;

namespace Orbito.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        { }

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
            // Global query filters for multi-tenancy
            // Note: These will be applied automatically when TenantInfo is available at runtime

            // For ApplicationRole - allow global roles (TenantId = null) and tenant-specific roles
            builder.Entity<ApplicationRole>()
                .HasQueryFilter(r => r.TenantId == null); // Will be overridden at runtime with actual tenant context

            // For ApplicationUser - filter by tenant
            builder.Entity<ApplicationUser>()
                .HasQueryFilter(u => u.TenantId == null); // Will be overridden at runtime with actual tenant context

            // For domain entities - all must have tenant
            // Note: We use EF.Property<Guid> to access the underlying Guid value for query filters
            builder.Entity<Provider>()
                .HasQueryFilter(p => EF.Property<Guid>(p, "TenantId") == Guid.Empty); // Will be overridden at runtime

            builder.Entity<Client>()
                .HasQueryFilter(c => EF.Property<Guid>(c, "TenantId") == Guid.Empty); // Will be overridden at runtime

            builder.Entity<SubscriptionPlan>()
                .HasQueryFilter(p => EF.Property<Guid>(p, "TenantId") == Guid.Empty); // Will be overridden at runtime

            builder.Entity<Subscription>()
                .HasQueryFilter(s => EF.Property<Guid>(s, "TenantId") == Guid.Empty); // Will be overridden at runtime

            builder.Entity<Payment>()
                .HasQueryFilter(p => EF.Property<Guid>(p, "TenantId") == Guid.Empty); // Will be overridden at runtime
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
