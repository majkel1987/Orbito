using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orbito.Domain.Entities;
using Orbito.Domain.ValueObjects;

namespace Orbito.Infrastructure.Data.Configurations.Entity
{
    public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
    {
        public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
        {
            // Primary Key
            builder.HasKey(p => p.Id);

            // TenantId - required for multi-tenancy
            builder.Property(p => p.TenantId)
                .HasConversion(
                    tenantId => tenantId.Value,
                    guid => TenantId.Create(guid))
                .IsRequired()
                .HasColumnName("TenantId");

            // Basic Properties
            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Description)
                .HasMaxLength(1000);

            builder.Property(p => p.FeaturesJson)
                .HasColumnType("nvarchar(max)");

            builder.Property(p => p.LimitationsJson)
                .HasColumnType("nvarchar(max)");

            builder.Property(p => p.TrialPeriodDays)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(p => p.IsPublic)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(p => p.SortOrder)
                .IsRequired()
                .HasDefaultValue(0);

            // Money Value Object
            builder.OwnsOne(p => p.Price, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("PriceAmount")
                    .HasPrecision(18, 2);
                money.Property(m => m.Currency)
                    .HasColumnName("PriceCurrency")
                    .HasMaxLength(3)
                    .HasConversion(
                        currency => currency.Code,
                        code => Currency.Create(code, GetSymbolForCode(code), 2));
            });

            // BillingPeriod Value Object
            builder.OwnsOne(p => p.BillingPeriod, period =>
            {
                period.Property(bp => bp.Value)
                    .HasColumnName("BillingPeriodValue");
                // NOTE: BillingPeriodType conversion is configured globally in ValueObjectsConfiguration.ConfigureEnumConverters()
            });

            // Timestamps
            builder.Property(p => p.CreatedAt)
                .IsRequired();

            builder.Property(p => p.UpdatedAt);

            // Indexes
            builder.HasIndex(p => p.TenantId)
                .HasDatabaseName("IX_SubscriptionPlans_TenantId");

            builder.HasIndex(p => new { p.TenantId, p.IsActive })
                .HasDatabaseName("IX_SubscriptionPlans_TenantId_IsActive");

            builder.HasIndex(p => new { p.TenantId, p.SortOrder })
                .HasDatabaseName("IX_SubscriptionPlans_TenantId_SortOrder");

            builder.HasIndex(p => new { p.TenantId, p.IsPublic })
                .HasDatabaseName("IX_SubscriptionPlans_TenantId_IsPublic");

            builder.HasIndex(p => new { p.TenantId, p.IsActive, p.IsPublic })
                .HasDatabaseName("IX_SubscriptionPlans_TenantId_IsActive_IsPublic");

            // Relationships
            builder.HasOne(p => p.Provider)
                .WithMany(pr => pr.Plans)
                .HasForeignKey(p => p.TenantId)
                .HasPrincipalKey(pr => pr.TenantId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(p => p.Subscriptions)
                .WithOne(s => s.Plan)
                .HasForeignKey(s => s.PlanId)
                .OnDelete(DeleteBehavior.Restrict);

            // Table name
            builder.ToTable("SubscriptionPlans");
        }

        /// <summary>
        /// Helper method to get symbol for currency code
        /// </summary>
        /// <param name="code">Currency code</param>
        /// <returns>Currency symbol</returns>
        private static string GetSymbolForCode(string code) => code.ToUpperInvariant() switch
        {
            "PLN" => "zł",
            "USD" => "$",
            "EUR" => "€",
            "GBP" => "£",
            _ => code
        };
    }
}
