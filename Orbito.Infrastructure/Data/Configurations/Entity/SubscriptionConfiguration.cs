using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;

namespace Orbito.Infrastructure.Data.Configurations.Entity
{
    public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
    {
        public void Configure(EntityTypeBuilder<Subscription> builder)
        {
            // Primary Key
            builder.HasKey(s => s.Id);

            // TenantId - required for multi-tenancy
            builder.Property(s => s.TenantId)
                .HasConversion(
                    tenantId => tenantId.Value,
                    guid => TenantId.Create(guid))
                .IsRequired()
                .HasColumnName("TenantId");

            // Foreign Keys
            builder.Property(s => s.ClientId)
                .IsRequired();

            builder.Property(s => s.PlanId)
                .IsRequired();

            // Status
            builder.Property(s => s.Status)
                .HasConversion<string>()
                .IsRequired();

            // Money Value Object
            builder.OwnsOne(s => s.CurrentPrice, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("CurrentPriceAmount")
                    .HasPrecision(18, 2);
                money.Property(m => m.Currency)
                    .HasColumnName("CurrentPriceCurrency")
                    .HasMaxLength(3);
            });

            // BillingPeriod Value Object
            builder.OwnsOne(s => s.BillingPeriod, period =>
            {
                period.Property(bp => bp.Value)
                    .HasColumnName("BillingPeriodValue");
                period.Property(bp => bp.Type)
                    .HasColumnName("BillingPeriodType")
                    .HasConversion<string>();
            });

            // Dates
            builder.Property(s => s.StartDate)
                .IsRequired();

            builder.Property(s => s.EndDate);

            builder.Property(s => s.NextBillingDate)
                .IsRequired();

            builder.Property(s => s.TrialEndDate);

            builder.Property(s => s.CreatedAt)
                .IsRequired();

            builder.Property(s => s.CancelledAt);

            builder.Property(s => s.UpdatedAt);

            // Indexes
            builder.HasIndex(s => s.TenantId)
                .HasDatabaseName("IX_Subscriptions_TenantId");

            builder.HasIndex(s => s.ClientId)
                .HasDatabaseName("IX_Subscriptions_ClientId");

            builder.HasIndex(s => s.PlanId)
                .HasDatabaseName("IX_Subscriptions_PlanId");

            builder.HasIndex(s => new { s.TenantId, s.Status })
                .HasDatabaseName("IX_Subscriptions_TenantId_Status");

            builder.HasIndex(s => s.NextBillingDate)
                .HasDatabaseName("IX_Subscriptions_NextBillingDate");

            // Relationships
            builder.HasOne(s => s.Client)
                .WithMany(c => c.Subscriptions)
                .HasForeignKey(s => s.ClientId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(s => s.Plan)
                .WithMany(p => p.Subscriptions)
                .HasForeignKey(s => s.PlanId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(s => s.Payments)
                .WithOne(p => p.Subscription)
                .HasForeignKey(p => p.SubscriptionId)
                .OnDelete(DeleteBehavior.NoAction);

            // Table name
            builder.ToTable("Subscriptions");
        }
    }
}
