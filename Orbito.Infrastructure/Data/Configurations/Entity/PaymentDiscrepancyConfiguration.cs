using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;

namespace Orbito.Infrastructure.Data.Configurations.Entity;

/// <summary>
/// EF Core configuration for PaymentDiscrepancy entity
/// </summary>
public class PaymentDiscrepancyConfiguration : IEntityTypeConfiguration<PaymentDiscrepancy>
{
    public void Configure(EntityTypeBuilder<PaymentDiscrepancy> builder)
    {
        builder.ToTable("PaymentDiscrepancies");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .HasColumnName("id")
            .IsRequired();

        // TenantId configuration
        builder.Property(d => d.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(
                tenantId => tenantId.Value,
                value => TenantId.Create(value))
            .IsRequired();

        // Reconciliation report reference
        builder.Property(d => d.ReconciliationReportId)
            .HasColumnName("reconciliation_report_id")
            .IsRequired();

        // Payment reference
        builder.Property(d => d.PaymentId)
            .HasColumnName("payment_id");

        builder.Property(d => d.ExternalPaymentId)
            .HasColumnName("external_payment_id")
            .HasMaxLength(255);

        // Discrepancy details
        builder.Property(d => d.Type)
            .HasColumnName("type")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(d => d.Resolution)
            .HasColumnName("resolution")
            .HasConversion<int>()
            .IsRequired();

        // Status comparison
        builder.Property(d => d.OrbitoStatus)
            .HasColumnName("orbito_status")
            .HasConversion<int>();

        builder.Property(d => d.StripeStatus)
            .HasColumnName("stripe_status")
            .HasMaxLength(100);

        // Amount comparison
        builder.Property(d => d.OrbitoAmount)
            .HasColumnName("orbito_amount")
            .HasPrecision(18, 2);

        builder.Property(d => d.OrbitoCurrency)
            .HasColumnName("orbito_currency")
            .HasMaxLength(3);

        builder.Property(d => d.StripeAmount)
            .HasColumnName("stripe_amount")
            .HasPrecision(18, 2);

        builder.Property(d => d.StripeCurrency)
            .HasColumnName("stripe_currency")
            .HasMaxLength(3);

        // Resolution details
        builder.Property(d => d.ResolutionNotes)
            .HasColumnName("resolution_notes")
            .HasMaxLength(2000);

        builder.Property(d => d.ResolvedAt)
            .HasColumnName("resolved_at");

        builder.Property(d => d.ResolvedBy)
            .HasColumnName("resolved_by")
            .HasMaxLength(255);

        // Additional metadata
        builder.Property(d => d.AdditionalData)
            .HasColumnName("additional_data")
            .HasMaxLength(4000);

        builder.Property(d => d.DetectedAt)
            .HasColumnName("detected_at")
            .IsRequired();

        // Navigation properties
        builder.HasOne(d => d.ReconciliationReport)
            .WithMany(r => r.Discrepancies)
            .HasForeignKey(d => d.ReconciliationReportId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Payment)
            .WithMany()
            .HasForeignKey(d => d.PaymentId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes for performance
        builder.HasIndex(d => d.TenantId)
            .HasDatabaseName("ix_payment_discrepancies_tenant_id");

        builder.HasIndex(d => d.ReconciliationReportId)
            .HasDatabaseName("ix_payment_discrepancies_report_id");

        builder.HasIndex(d => new { d.TenantId, d.Resolution })
            .HasDatabaseName("ix_payment_discrepancies_tenant_resolution");

        builder.HasIndex(d => new { d.TenantId, d.Type })
            .HasDatabaseName("ix_payment_discrepancies_tenant_type");

        builder.HasIndex(d => d.PaymentId)
            .HasDatabaseName("ix_payment_discrepancies_payment_id");

        builder.HasIndex(d => d.ExternalPaymentId)
            .HasDatabaseName("ix_payment_discrepancies_external_payment_id");

        builder.HasIndex(d => d.DetectedAt)
            .HasDatabaseName("ix_payment_discrepancies_detected_at");

        // Query filter for multi-tenancy (will be configured in DbContext)
        // builder.HasQueryFilter(d => d.TenantId == currentTenantId);
    }
}
