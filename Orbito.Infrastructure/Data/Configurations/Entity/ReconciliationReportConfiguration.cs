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
            .HasColumnName("id")
            .IsRequired();

        // TenantId configuration
        builder.Property(r => r.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(
                tenantId => tenantId.Value,
                value => TenantId.Create(value))
            .IsRequired();

        // Report metadata
        builder.Property(r => r.RunDate)
            .HasColumnName("run_date")
            .IsRequired();

        builder.Property(r => r.PeriodStart)
            .HasColumnName("period_start")
            .IsRequired();

        builder.Property(r => r.PeriodEnd)
            .HasColumnName("period_end")
            .IsRequired();

        builder.Property(r => r.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        // Statistics
        builder.Property(r => r.TotalPayments)
            .HasColumnName("total_payments")
            .IsRequired();

        builder.Property(r => r.MatchedPayments)
            .HasColumnName("matched_payments")
            .IsRequired();

        builder.Property(r => r.MismatchedPayments)
            .HasColumnName("mismatched_payments")
            .IsRequired();

        builder.Property(r => r.DiscrepanciesCount)
            .HasColumnName("discrepancies_count")
            .IsRequired();

        builder.Property(r => r.AutoResolvedCount)
            .HasColumnName("auto_resolved_count")
            .IsRequired();

        builder.Property(r => r.ManualReviewCount)
            .HasColumnName("manual_review_count")
            .IsRequired();

        // Execution details
        builder.Property(r => r.StartedAt)
            .HasColumnName("started_at");

        builder.Property(r => r.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(r => r.Duration)
            .HasColumnName("duration");

        builder.Property(r => r.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(2000);

        // Navigation properties
        builder.HasMany(r => r.Discrepancies)
            .WithOne(d => d.ReconciliationReport)
            .HasForeignKey(d => d.ReconciliationReportId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for performance
        builder.HasIndex(r => r.TenantId)
            .HasDatabaseName("ix_reconciliation_reports_tenant_id");

        builder.HasIndex(r => new { r.TenantId, r.RunDate })
            .HasDatabaseName("ix_reconciliation_reports_tenant_run_date");

        builder.HasIndex(r => new { r.TenantId, r.Status })
            .HasDatabaseName("ix_reconciliation_reports_tenant_status");

        builder.HasIndex(r => new { r.PeriodStart, r.PeriodEnd })
            .HasDatabaseName("ix_reconciliation_reports_period");

        // Query filter for multi-tenancy (will be configured in DbContext)
        // builder.HasQueryFilter(r => r.TenantId == currentTenantId);
    }
}
