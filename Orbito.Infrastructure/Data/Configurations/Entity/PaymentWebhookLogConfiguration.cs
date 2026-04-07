using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orbito.Domain.Entities;
using Orbito.Domain.ValueObjects;

namespace Orbito.Infrastructure.Data.Configurations.Entity
{
    /// <summary>
    /// Entity configuration for PaymentWebhookLog
    /// </summary>
    public class PaymentWebhookLogConfiguration : IEntityTypeConfiguration<PaymentWebhookLog>
    {
        public void Configure(EntityTypeBuilder<PaymentWebhookLog> builder)
        {
            // Primary key
            builder.HasKey(w => w.Id);

            // Table name
            builder.ToTable("PaymentWebhookLogs");

            // Properties
            builder.Property(w => w.Id)
                .IsRequired();

            builder.Property(w => w.TenantId)
                .IsRequired()
                .HasConversion(
                    v => v.Value,
                    v => TenantId.Create(v));

            builder.Property(w => w.EventId)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(w => w.Provider)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(w => w.EventType)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(w => w.Payload)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.Property(w => w.ProcessedAt);

            builder.Property(w => w.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(w => w.ErrorMessage)
                .HasMaxLength(1000);

            builder.Property(w => w.Attempts)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(w => w.ReceivedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(w => w.Metadata)
                .HasMaxLength(2000);

            // Indexes - optimized for common queries
            builder.HasIndex(w => new { w.TenantId, w.EventId })
                .IsUnique()
                .HasDatabaseName("IX_PaymentWebhookLogs_TenantId_EventId");

            builder.HasIndex(w => new { w.Status, w.ReceivedAt })
                .HasDatabaseName("IX_PaymentWebhookLogs_Status_ReceivedAt")
                .HasFilter("[Status] = 'Failed'"); // For retry logic

            // NOTE: Multi-tenancy query filter configured in ApplicationDbContext.OnModelCreating()
            // Filter: WHERE TenantId == CurrentTenantId (applied to all queries automatically)

            // Relationships
            // No foreign key relationships for webhook logs as they are independent entities
        }
    }
}
