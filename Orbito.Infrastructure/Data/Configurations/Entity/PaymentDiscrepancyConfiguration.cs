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
            .IsRequired();

        // TenantId configuration
        builder.Property(d => d.TenantId)
            .HasConversion(
                tenantId => tenantId.Value,
                value => TenantId.Create(value))
            .IsRequired()
            .HasColumnName("TenantId");

        // Reconciliation report reference
        builder.Property(d => d.ReconciliationReportId)
            .IsRequired();

        // Payment reference
        builder.Property(d => d.PaymentId);

        builder.Property(d => d.ExternalPaymentId)
            .HasMaxLength(255);

        // Discrepancy details
        builder.Property(d => d.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(d => d.Resolution)
            .HasConversion<int>()
            .IsRequired();

        // Status comparison
        builder.Property(d => d.OrbitoStatus)
            .HasConversion<int>();

        builder.Property(d => d.StripeStatus)
            .HasMaxLength(100);

        // Amount comparison
        builder.Property(d => d.OrbitoAmount)
            .HasPrecision(18, 2);

        builder.Property(d => d.OrbitoCurrency)
            .HasMaxLength(3);

        builder.Property(d => d.StripeAmount)
            .HasPrecision(18, 2);

        builder.Property(d => d.StripeCurrency)
            .HasMaxLength(3);

        // Resolution details
        builder.Property(d => d.ResolutionNotes)
            .HasMaxLength(2000);

        builder.Property(d => d.ResolvedAt);

        builder.Property(d => d.ResolvedBy)
            .HasMaxLength(255);

        // Additional metadata
        builder.Property(d => d.AdditionalData)
            .HasMaxLength(4000);

        builder.Property(d => d.DetectedAt)
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
            .HasDatabaseName("IX_PaymentDiscrepancies_TenantId");

        builder.HasIndex(d => d.ReconciliationReportId)
            .HasDatabaseName("IX_PaymentDiscrepancies_ReconciliationReportId");

        builder.HasIndex(d => new { d.TenantId, d.Resolution })
            .HasDatabaseName("IX_PaymentDiscrepancies_TenantId_Resolution");

        builder.HasIndex(d => new { d.TenantId, d.Type })
            .HasDatabaseName("IX_PaymentDiscrepancies_TenantId_Type");

        builder.HasIndex(d => d.PaymentId)
            .HasDatabaseName("IX_PaymentDiscrepancies_PaymentId");

        builder.HasIndex(d => d.ExternalPaymentId)
            .HasDatabaseName("IX_PaymentDiscrepancies_ExternalPaymentId");

        builder.HasIndex(d => d.DetectedAt)
            .HasDatabaseName("IX_PaymentDiscrepancies_DetectedAt");

        // NOTE: Multi-tenancy query filter configured in ApplicationDbContext.OnModelCreating()
        // Filter: WHERE TenantId == CurrentTenantId (applied to all queries automatically)
    }
}
