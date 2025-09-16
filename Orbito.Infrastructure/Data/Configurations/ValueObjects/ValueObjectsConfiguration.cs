using Microsoft.EntityFrameworkCore;
using Orbito.Domain.Entities;
using Orbito.Domain.ValueObjects;

namespace Orbito.Infrastructure.Data.Configurations.ValueObjects
{
    public static class ValueObjectsExtensions
    {
        public static void ConfigureValueObjects(this ModelBuilder modelBuilder)
        {
            // TenantId Value Object conversions
            modelBuilder.Entity<Client>()
                .Property(c => c.TenantId)
                .HasConversion(
                    tenantId => tenantId.Value,
                    guid => TenantId.Create(guid));

            modelBuilder.Entity<SubscriptionPlan>()
                .Property(sp => sp.TenantId)
                .HasConversion(
                    tenantId => tenantId.Value,
                    guid => TenantId.Create(guid));

            modelBuilder.Entity<Subscription>()
                .Property(s => s.TenantId)
                .HasConversion(
                    tenantId => tenantId.Value,
                    guid => TenantId.Create(guid));

            modelBuilder.Entity<Payment>()
                .Property(p => p.TenantId)
                .HasConversion(
                    tenantId => tenantId.Value,
                    guid => TenantId.Create(guid));

        }

    }
}
