using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orbito.Domain.Entities;

namespace Orbito.Infrastructure.Data.Configurations.Entity
{
    public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
    {
        public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
        {
            builder.ToTable("SubscriptionPlans");

            // Primary Key
            builder.HasKey(p => p.Id);

            // TenantId
            builder.Property(p => p.TenantId)
                .HasConversion(
                    tenantId => tenantId.Value,
                    guid => Domain.ValueObjects.TenantId.Create(guid))
                .IsRequired()
                .HasColumnName("TenantId");

            // Basic Properties
            builder.Property(p => p.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(p => p.Description)
                .HasMaxLength(2000);

            builder.Property(p => p.FeaturesJson)
                .HasColumnType("nvarchar(max)");

            builder.Property(p => p.TrialDays)
                .HasDefaultValue(0);

            builder.Property(p => p.IsActive)
                .HasDefaultValue(true);

            builder.Property(p => p.IsPublic)
                .HasDefaultValue(true);

            builder.Property(p => p.SortOrder)
                .HasDefaultValue(0);

            builder.Property(p => p.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Money Value Object - Price
            builder.OwnsOne(p => p.Price, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("PriceAmount")
                    .HasPrecision(18, 2)
                    .IsRequired();

                money.Property(m => m.Currency)
                    .HasColumnName("PriceCurrency")
                    .HasMaxLength(3)
                    .IsRequired();
            });

            // BillingPeriod Value Object
            builder.OwnsOne(p => p.BillingPeriod, period =>
            {
                period.Property(bp => bp.Value)
                    .HasColumnName("BillingPeriodValue")
                    .IsRequired();

                period.Property(bp => bp.Type)
                    .HasColumnName("BillingPeriodType")
                    .HasConversion<int>()
                    .IsRequired();
            });

            // Computed properties (not mapped)
            builder.Ignore(p => p.BillingPeriod.DaysCount);

            // Indexes
            builder.HasIndex(p => p.TenantId)
                .HasDatabaseName("IX_SubscriptionPlans_TenantId");

            builder.HasIndex(p => new { p.TenantId, p.IsActive, p.IsPublic })
                .HasDatabaseName("IX_SubscriptionPlans_TenantId_Active_Public");

            builder.HasIndex(p => new { p.TenantId, p.SortOrder })
                .HasDatabaseName("IX_SubscriptionPlans_TenantId_SortOrder");

            // Relationships - POPRAWIONE!
            builder.HasOne(p => p.Provider)
                .WithMany(pr => pr.Plans)
                .HasForeignKey("TenantId") // Używamy nazwy kolumny
                .HasPrincipalKey(pr => pr.Id)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Subscriptions)
                .WithOne(s => s.Plan)
                .HasForeignKey(s => s.PlanId)
                .OnDelete(DeleteBehavior.Restrict); // Don't delete plans with active subscriptions
        }
    }
}