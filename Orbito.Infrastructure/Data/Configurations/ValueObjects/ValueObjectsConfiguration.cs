using Microsoft.EntityFrameworkCore;
using Orbito.Domain.ValueObjects;

namespace Orbito.Infrastructure.Data.Configurations.ValueObjects
{
    public static class ValueObjectsConfiguration
    {
        public static void ConfigureValueObjects(this ModelBuilder modelBuilder)
        {
            // TenantId Value Object Configuration
            modelBuilder.Entity<Domain.Entities.Provider>()
                .Property(p => p.TenantId)
                .HasConversion(
                    tenantId => tenantId.Value,
                    guid => TenantId.Create(guid));

            modelBuilder.Entity<Domain.Entities.Client>()
                .Property(c => c.TenantId)
                .HasConversion(
                    tenantId => tenantId.Value,
                    guid => TenantId.Create(guid));

            modelBuilder.Entity<Domain.Entities.SubscriptionPlan>()
                .Property(p => p.TenantId)
                .HasConversion(
                    tenantId => tenantId.Value,
                    guid => TenantId.Create(guid));

            modelBuilder.Entity<Domain.Entities.Subscription>()
                .Property(s => s.TenantId)
                .HasConversion(
                    tenantId => tenantId.Value,
                    guid => TenantId.Create(guid));

            modelBuilder.Entity<Domain.Entities.Payment>()
                .Property(p => p.TenantId)
                .HasConversion(
                    tenantId => tenantId.Value,
                    guid => TenantId.Create(guid));

            modelBuilder.Entity<Domain.Identity.ApplicationUser>()
                .Property(u => u.TenantId)
                .HasConversion(
                    tenantId => tenantId != null ? tenantId.Value : (Guid?)null,
                    guid => guid.HasValue ? TenantId.Create(guid.Value) : null);

            modelBuilder.Entity<Domain.Identity.ApplicationRole>()
                .Property(r => r.TenantId)
                .HasConversion(
                    tenantId => tenantId != null ? tenantId.Value : (Guid?)null,
                    guid => guid.HasValue ? TenantId.Create(guid.Value) : null);
        }
    }
}
