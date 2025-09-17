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

            // Money Value Object
            builder.OwnsOne(p => p.Price, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("PriceAmount")
                    .HasPrecision(18, 2);
                money.Property(m => m.Currency)
                    .HasColumnName("PriceCurrency")
                    .HasMaxLength(3);
            });

            // BillingPeriod Value Object
            builder.OwnsOne(p => p.BillingPeriod, period =>
            {
                period.Property(bp => bp.Value)
                    .HasColumnName("BillingPeriodValue");
                period.Property(bp => bp.Type)
                    .HasColumnName("BillingPeriodType")
                    .HasConversion<string>();
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
    }
}
