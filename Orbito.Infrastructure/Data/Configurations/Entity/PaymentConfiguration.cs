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

            // Status - Configured globally in ValueObjectsConfiguration.ConfigureEnumConverters()
            // NOTE: HasConversion, HasMaxLength, and IsRequired are set in the global configuration

            // Money Value Object
            builder.OwnsOne(p => p.Amount, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("Amount")
                    .HasPrecision(18, 2);
                money.Property(m => m.Currency)
                    .HasColumnName("Currency")
                    .HasMaxLength(3)
                    .HasConversion(
                        currency => currency.Code,
                        code => Currency.Create(code, GetSymbolForCode(code), 2));
            });

            // External Payment Data
            builder.Property(p => p.ExternalTransactionId)
                .HasMaxLength(255);

            builder.Property(p => p.PaymentMethod)
                .HasMaxLength(50);

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

            builder.Property(p => p.RefundedAt);

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

            // Unique constraint for ExternalTransactionId to prevent duplicates
            builder.HasIndex(p => p.ExternalTransactionId)
                .HasDatabaseName("IX_Payments_ExternalTransactionId")
                .IsUnique()
                .HasFilter("ExternalTransactionId IS NOT NULL");

            builder.HasIndex(p => new { p.TenantId, p.Status })
                .HasDatabaseName("IX_Payments_TenantId_Status");

            builder.HasIndex(p => p.CreatedAt)
                .HasDatabaseName("IX_Payments_CreatedAt");

            // SECURITY: Unique constraint to prevent race condition on concurrent payment processing
            // Only one active payment (Pending or Processing) per subscription at a time
            // This prevents duplicate charges when multiple requests arrive simultaneously
            builder.HasIndex(p => new { p.SubscriptionId, p.Status })
                .HasDatabaseName("IX_Payments_SubscriptionId_Status_Unique")
                .IsUnique()
                .HasFilter("Status IN ('Pending', 'Processing')");

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

        /// <summary>
        /// Helper method to get symbol for currency code
        /// </summary>
        /// <param name="code">Currency code</param>
        /// <returns>Currency symbol</returns>
        private static string GetSymbolForCode(string code) => code.ToUpperInvariant() switch
        {
            "PLN" => "zł",
            "USD" => "$",
            "EUR" => "€",
            "GBP" => "£",
            _ => code
        };
    }
}
