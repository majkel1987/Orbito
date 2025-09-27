using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;

namespace Orbito.Infrastructure.Data.Configurations.Entity
{
    public class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
    {
        public void Configure(EntityTypeBuilder<PaymentMethod> builder)
        {
            // Primary Key
            builder.HasKey(pm => pm.Id);

            // TenantId - required for multi-tenancy
            builder.Property(pm => pm.TenantId)
                .HasConversion(
                    tenantId => tenantId.Value,
                    guid => TenantId.Create(guid))
                .IsRequired()
                .HasColumnName("TenantId");

            // Foreign Keys
            builder.Property(pm => pm.ClientId)
                .IsRequired();

            // Payment Method Details
            builder.Property(pm => pm.Type)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(pm => pm.Token)
                .IsRequired()
                .HasMaxLength(500); // Encrypted token can be longer

            builder.Property(pm => pm.LastFourDigits)
                .HasMaxLength(4);

            builder.Property(pm => pm.ExpiryDate);

            builder.Property(pm => pm.IsDefault)
                .IsRequired()
                .HasDefaultValue(false);

            // Timestamps
            builder.Property(pm => pm.CreatedAt)
                .IsRequired();

            builder.Property(pm => pm.UpdatedAt);

            // Indexes
            builder.HasIndex(pm => pm.TenantId)
                .HasDatabaseName("IX_PaymentMethods_TenantId");

            builder.HasIndex(pm => pm.ClientId)
                .HasDatabaseName("IX_PaymentMethods_ClientId");

            builder.HasIndex(pm => new { pm.TenantId, pm.ClientId })
                .HasDatabaseName("IX_PaymentMethods_TenantId_ClientId");

            builder.HasIndex(pm => pm.Type)
                .HasDatabaseName("IX_PaymentMethods_Type");

            builder.HasIndex(pm => pm.IsDefault)
                .HasDatabaseName("IX_PaymentMethods_IsDefault");

            builder.HasIndex(pm => pm.CreatedAt)
                .HasDatabaseName("IX_PaymentMethods_CreatedAt");

            // Relationships
            builder.HasOne(pm => pm.Client)
                .WithMany(c => c.PaymentMethods)
                .HasForeignKey(pm => pm.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Note: PaymentMethod relationship will be configured in PaymentConfiguration

            // Table name
            builder.ToTable("PaymentMethods");
        }
    }
}
