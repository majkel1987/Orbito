using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;

namespace Orbito.Infrastructure.Data.Configurations.Entity;

public class ProviderSubscriptionConfiguration : IEntityTypeConfiguration<ProviderSubscription>
{
    public void Configure(EntityTypeBuilder<ProviderSubscription> builder)
    {
        builder.ToTable("ProviderSubscriptions");

        builder.HasKey(ps => ps.Id);

        builder.Property(ps => ps.ProviderId)
            .IsRequired();

        builder.Property(ps => ps.PlatformPlanId)
            .IsRequired();

        builder.Property(ps => ps.Status)
            .IsRequired()
            .HasDefaultValue(ProviderSubscriptionStatus.Trial);

        builder.Property(ps => ps.StartDate)
            .IsRequired();

        builder.Property(ps => ps.TrialEndDate)
            .IsRequired();

        builder.Property(ps => ps.PaidUntil);

        builder.Property(ps => ps.LastNotificationSentAt);

        builder.Property(ps => ps.LastNotificationTier)
            .IsRequired()
            .HasDefaultValue(TrialNotificationTier.None);

        builder.Property(ps => ps.CreatedAt)
            .IsRequired();

        builder.Property(ps => ps.UpdatedAt);

        // Relationships
        builder.HasOne(ps => ps.Provider)
            .WithMany()
            .HasForeignKey(ps => ps.ProviderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ps => ps.PlatformPlan)
            .WithMany(pp => pp.ProviderSubscriptions)
            .HasForeignKey(ps => ps.PlatformPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(ps => ps.ProviderId)
            .IsUnique()
            .HasDatabaseName("IX_ProviderSubscriptions_ProviderId");

        builder.HasIndex(ps => ps.PlatformPlanId)
            .HasDatabaseName("IX_ProviderSubscriptions_PlatformPlanId");

        builder.HasIndex(ps => ps.Status)
            .HasDatabaseName("IX_ProviderSubscriptions_Status");

        builder.HasIndex(ps => ps.TrialEndDate)
            .HasDatabaseName("IX_ProviderSubscriptions_TrialEndDate");

        builder.HasIndex(ps => new { ps.Status, ps.TrialEndDate })
            .HasDatabaseName("IX_ProviderSubscriptions_Status_TrialEndDate");
    }
}
