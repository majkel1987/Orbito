using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;

namespace Orbito.Infrastructure.Data.Configurations.Entity
{
    public class PaymentHistoryConfiguration : IEntityTypeConfiguration<PaymentHistory>
    {
        public void Configure(EntityTypeBuilder<PaymentHistory> builder)
        {
            // Primary Key
            builder.HasKey(ph => ph.Id);

            // TenantId - required for multi-tenancy
            builder.Property(ph => ph.TenantId)
                .HasConversion(
                    tenantId => tenantId.Value,
                    guid => TenantId.Create(guid))
                .IsRequired()
                .HasColumnName("TenantId");

            // Foreign Keys
            builder.Property(ph => ph.PaymentId)
                .IsRequired();

            // History Details
            builder.Property(ph => ph.Action)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(ph => ph.Status)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(ph => ph.OccurredAt)
                .IsRequired();

            builder.Property(ph => ph.Details)
                .HasMaxLength(2000);

            builder.Property(ph => ph.ErrorMessage)
                .HasMaxLength(1000);

            // Indexes
            builder.HasIndex(ph => ph.TenantId)
                .HasDatabaseName("IX_PaymentHistory_TenantId");

            builder.HasIndex(ph => ph.PaymentId)
                .HasDatabaseName("IX_PaymentHistory_PaymentId");

            builder.HasIndex(ph => new { ph.TenantId, ph.PaymentId })
                .HasDatabaseName("IX_PaymentHistory_TenantId_PaymentId");

            builder.HasIndex(ph => ph.Status)
                .HasDatabaseName("IX_PaymentHistory_Status");

            builder.HasIndex(ph => ph.OccurredAt)
                .HasDatabaseName("IX_PaymentHistory_OccurredAt");

            builder.HasIndex(ph => new { ph.PaymentId, ph.OccurredAt })
                .HasDatabaseName("IX_PaymentHistory_PaymentId_OccurredAt");

            // NOTE: Multi-tenancy query filter configured in ApplicationDbContext.OnModelCreating()
            // Filter: WHERE TenantId == CurrentTenantId (applied to all queries automatically)

            // Relationships
            builder.HasOne(ph => ph.Payment)
                .WithMany()
                .HasForeignKey(ph => ph.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Table name
            builder.ToTable("PaymentHistory");
        }
    }
}
