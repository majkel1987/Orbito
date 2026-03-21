using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orbito.Domain.Entities;
using Orbito.Domain.ValueObjects;

namespace Orbito.Infrastructure.Data.Configurations.Entity;

public class PlatformPlanConfiguration : IEntityTypeConfiguration<PlatformPlan>
{
    public void Configure(EntityTypeBuilder<PlatformPlan> builder)
    {
        builder.ToTable("PlatformPlans");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.TrialDays)
            .IsRequired()
            .HasDefaultValue(14);

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.FeaturesJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(p => p.SortOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt);

        // Money Value Object
        builder.OwnsOne(p => p.Price, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("PriceAmount")
                .HasPrecision(18, 2)
                .IsRequired();
            money.Property(m => m.Currency)
                .HasColumnName("PriceCurrency")
                .HasMaxLength(3)
                .IsRequired()
                .HasConversion(
                    currency => currency.Code,
                    code => Currency.Create(code, GetSymbolForCode(code), 2));
        });

        // BillingPeriod Value Object
        builder.OwnsOne(p => p.BillingPeriod, period =>
        {
            period.Property(bp => bp.Value)
                .HasColumnName("BillingPeriodValue")
                .IsRequired();
            period.Property(bp => bp.Type)
                .HasColumnName("BillingPeriodType")
                .IsRequired();
        });

        // Indexes
        builder.HasIndex(p => p.Name)
            .IsUnique()
            .HasDatabaseName("IX_PlatformPlans_Name");

        builder.HasIndex(p => p.IsActive)
            .HasDatabaseName("IX_PlatformPlans_IsActive");

        builder.HasIndex(p => p.SortOrder)
            .HasDatabaseName("IX_PlatformPlans_SortOrder");
    }

    private static string GetSymbolForCode(string code) => code.ToUpperInvariant() switch
    {
        "PLN" => "zł",
        "USD" => "$",
        "EUR" => "€",
        "GBP" => "£",
        _ => code
    };
}
