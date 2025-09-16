using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orbito.Domain.Entities;

namespace Orbito.Infrastructure.Data.Configurations.Entity
{
    public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
    {
        public void Configure(EntityTypeBuilder<Subscription> builder)
        {
            builder.ToTable("Subscriptions");

            // Primary Key
            builder.HasKey(s => s.Id);

            // TenantId
            builder.Property(s => s.TenantId)
                .HasConversion(
                    tenantId => tenantId.Value,
                    guid => Domain.ValueObjects.TenantId.Create(guid))
                .IsRequired()
                .HasColumnName("TenantId");

            // Foreign Keys
            builder.Property(s => s.ClientId).IsRequired();
            builder.Property(s => s.PlanId).IsRequired();

            // Status
            builder.Property(s => s.Status)
                .HasConversion<int>()
                .IsRequired();

            // Dates
            builder.Property(s => s.StartDate)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(s => s.NextBillingDate)
                .IsRequired();

            // Trial
            builder.Property(s => s.IsInTrial)
                .HasDefaultValue(false);

            builder.Property(s => s.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Money Value Object - CurrentPrice
            builder.OwnsOne(s => s.CurrentPrice, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("CurrentPriceAmount")
                    .HasPrecision(18, 2)
                    .IsRequired();

                money.Property(m => m.Currency)
                    .HasColumnName("CurrentPriceCurrency")
                    .HasMaxLength(3)
                    .IsRequired();
            });

            // BillingPeriod Value Object
            builder.OwnsOne(s => s.BillingPeriod, period =>
            {
                period.Property(bp => bp.Value)
                    .HasColumnName("BillingPeriodValue")
                    .IsRequired();

                period.Property(bp => bp.Type)
                    .HasColumnName("BillingPeriodType")
                    .HasConversion<int>()
                    .IsRequired();
            });

            // Indexes
            builder.HasIndex(s => s.TenantId)
                .HasDatabaseName("IX_Subscriptions_TenantId");

            builder.HasIndex(s => new { s.TenantId, s.Status })
                .HasDatabaseName("IX_Subscriptions_TenantId_Status");

            builder.HasIndex(s => s.ClientId)
                .HasDatabaseName("IX_Subscriptions_ClientId");

            builder.HasIndex(s => s.NextBillingDate)
                .HasDatabaseName("IX_Subscriptions_NextBillingDate");

            builder.HasIndex(s => new { s.IsInTrial, s.TrialEndDate })
                .HasDatabaseName("IX_Subscriptions_Trial")
                .HasFilter("IsInTrial = 1");

            // Relationships
            builder.HasOne(s => s.Client)
                .WithMany(c => c.Subscriptions)
                .HasForeignKey(s => s.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(s => s.Plan)
                .WithMany(p => p.Subscriptions)
                .HasForeignKey(s => s.PlanId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(s => s.Payments)
                .WithOne(p => p.Subscription)
                .HasForeignKey(p => p.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}