using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;

namespace Orbito.Infrastructure.Data.Configurations.Entity;

/// <summary>
/// EF Core configuration for ReconciliationReport entity
/// </summary>
public class ReconciliationReportConfiguration : IEntityTypeConfiguration<ReconciliationReport>
{
    public void Configure(EntityTypeBuilder<ReconciliationReport> builder)
    {
        builder.ToTable("ReconciliationReports");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .IsRequired();

        // TenantId configuration
        builder.Property(r => r.TenantId)
            .HasConversion(
                tenantId => tenantId.Value,
                value => TenantId.Create(value))
            .IsRequired()
            .HasColumnName("TenantId");

        // Report metadata
        builder.Property(r => r.RunDate)
            .IsRequired();

        builder.Property(r => r.PeriodStart)
            .IsRequired();

        builder.Property(r => r.PeriodEnd)
            .IsRequired();

        builder.Property(r => r.Status)
            .HasConversion<int>()
            .IsRequired();

        // Statistics
        builder.Property(r => r.TotalPayments)
            .IsRequired();

        builder.Property(r => r.MatchedPayments)
            .IsRequired();

        builder.Property(r => r.MismatchedPayments)
            .IsRequired();

        builder.Property(r => r.DiscrepanciesCount)
            .IsRequired();

        builder.Property(r => r.AutoResolvedCount)
            .IsRequired();

        builder.Property(r => r.ManualReviewCount)
            .IsRequired();

        // Execution details
        builder.Property(r => r.StartedAt);

        builder.Property(r => r.CompletedAt);

        builder.Property(r => r.Duration);

        builder.Property(r => r.ErrorMessage)
            .HasMaxLength(2000);

        // Navigation properties
        builder.HasMany(r => r.Discrepancies)
            .WithOne(d => d.ReconciliationReport)
            .HasForeignKey(d => d.ReconciliationReportId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for performance
        builder.HasIndex(r => r.TenantId)
            .HasDatabaseName("IX_ReconciliationReports_TenantId");

        builder.HasIndex(r => new { r.TenantId, r.RunDate })
            .HasDatabaseName("IX_ReconciliationReports_TenantId_RunDate");

        builder.HasIndex(r => new { r.TenantId, r.Status })
            .HasDatabaseName("IX_ReconciliationReports_TenantId_Status");

        builder.HasIndex(r => new { r.PeriodStart, r.PeriodEnd })
            .HasDatabaseName("IX_ReconciliationReports_PeriodStartEnd");

        // NOTE: Multi-tenancy query filter configured in ApplicationDbContext.OnModelCreating()
        // Filter: WHERE TenantId == CurrentTenantId (applied to all queries automatically)
    }
}
