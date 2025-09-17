using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;

namespace Orbito.Infrastructure.Data.Configurations.Entity
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            // Primary Key
            builder.HasKey(p => p.Id);

            // TenantId - required for multi-tenancy
            builder.Property(p => p.TenantId)
                .HasConversion(
                    tenantId => tenantId.Value,
                    guid => TenantId.Create(guid))
                .IsRequired()
                .HasColumnName("TenantId");

            // Foreign Keys
            builder.Property(p => p.SubscriptionId)
                .IsRequired();

            builder.Property(p => p.ClientId)
                .IsRequired();

            // Status
            builder.Property(p => p.Status)
                .HasConversion<string>()
                .IsRequired();

            // Money Value Object
            builder.OwnsOne(p => p.Amount, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("Amount")
                    .HasPrecision(18, 2);
                money.Property(m => m.Currency)
                    .HasColumnName("Currency")
                    .HasMaxLength(3);
            });

            // External Payment Data
            builder.Property(p => p.ExternalPaymentId)
                .HasMaxLength(255);

            builder.Property(p => p.PaymentMethodId)
                .HasMaxLength(255);

            builder.Property(p => p.FailureReason)
                .HasMaxLength(1000);

            // Timestamps
            builder.Property(p => p.CreatedAt)
                .IsRequired();

            builder.Property(p => p.ProcessedAt);

            builder.Property(p => p.FailedAt);

            // Indexes
            builder.HasIndex(p => p.TenantId)
                .HasDatabaseName("IX_Payments_TenantId");

            builder.HasIndex(p => p.SubscriptionId)
                .HasDatabaseName("IX_Payments_SubscriptionId");

            builder.HasIndex(p => p.ClientId)
                .HasDatabaseName("IX_Payments_ClientId");

            builder.HasIndex(p => p.ExternalPaymentId)
                .HasDatabaseName("IX_Payments_ExternalPaymentId")
                .HasFilter("ExternalPaymentId IS NOT NULL");

            builder.HasIndex(p => new { p.TenantId, p.Status })
                .HasDatabaseName("IX_Payments_TenantId_Status");

            builder.HasIndex(p => p.CreatedAt)
                .HasDatabaseName("IX_Payments_CreatedAt");

            // Relationships
            builder.HasOne(p => p.Subscription)
                .WithMany(s => s.Payments)
                .HasForeignKey(p => p.SubscriptionId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(p => p.Client)
                .WithMany(c => c.Payments)
                .HasForeignKey(p => p.ClientId)
                .OnDelete(DeleteBehavior.NoAction);

            // Table name
            builder.ToTable("Payments");
        }
    }
}
