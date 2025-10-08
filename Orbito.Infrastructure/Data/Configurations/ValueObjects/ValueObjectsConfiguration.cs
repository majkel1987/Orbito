using Microsoft.EntityFrameworkCore;
using Orbito.Domain.ValueObjects;

namespace Orbito.Infrastructure.Data.Configurations.ValueObjects
{
    /// <summary>
    /// Configuration for Value Objects in EF Core
    /// </summary>
    public static class ValueObjectsConfiguration
    {
        /// <summary>
        /// Configures all value objects in the model
        /// </summary>
        /// <param name="modelBuilder">EF Core ModelBuilder</param>
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

            modelBuilder.Entity<Domain.Entities.PaymentMethod>()
                .Property(pm => pm.TenantId)
                .HasConversion(
                    tenantId => tenantId.Value,
                    guid => TenantId.Create(guid));

            modelBuilder.Entity<Domain.Entities.PaymentHistory>()
                .Property(ph => ph.TenantId)
                .HasConversion(
                    tenantId => tenantId.Value,
                    guid => TenantId.Create(guid));

            modelBuilder.Entity<Domain.Entities.PaymentWebhookLog>()
                .Property(pwl => pwl.TenantId)
                .HasConversion(
                    tenantId => tenantId.Value,
                    guid => TenantId.Create(guid));

            modelBuilder.Entity<Domain.Entities.EmailNotification>()
                .Property(en => en.TenantId)
                .HasConversion(
                    tenantId => tenantId.Value,
                    guid => TenantId.Create(guid));

            modelBuilder.Entity<Domain.Entities.PaymentRetrySchedule>()
                .Property(prs => prs.TenantId)
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
