using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;

namespace Orbito.Infrastructure.Data.Configurations.Entity
{
    /// <summary>
    /// Entity configuration for PaymentMethod
    /// </summary>
    public class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
    {
        public void Configure(EntityTypeBuilder<PaymentMethod> builder)
        {
            // Primary key
            builder.HasKey(pm => pm.Id);

            // Table name
            builder.ToTable("PaymentMethods");

            // Properties
            builder.Property(pm => pm.Id)
                .IsRequired();

            builder.Property(pm => pm.TenantId)
                .IsRequired()
                .HasConversion(
                    v => v.Value,
                    v => TenantId.Create(v));

            builder.Property(pm => pm.ClientId)
                .IsRequired();

            builder.Property(pm => pm.Type)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(pm => pm.Token)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(pm => pm.LastFourDigits)
                .HasMaxLength(4)
                .IsFixedLength(false);

            builder.Property(pm => pm.ExpiryDate)
                .HasColumnType("datetime2");

            builder.Property(pm => pm.IsDefault)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(pm => pm.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(pm => pm.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            builder.HasIndex(pm => new { pm.TenantId, pm.ClientId })
                .HasDatabaseName("IX_PaymentMethods_TenantId_ClientId");

            builder.HasIndex(pm => new { pm.ClientId, pm.IsDefault })
                .HasDatabaseName("IX_PaymentMethods_ClientId_IsDefault")
                .HasFilter("[IsDefault] = 1"); // Only for default = true

            builder.HasIndex(pm => new { pm.Type, pm.CreatedAt })
                .HasDatabaseName("IX_PaymentMethods_Type_CreatedAt");

            builder.HasIndex(pm => pm.ExpiryDate)
                .HasDatabaseName("IX_PaymentMethods_ExpiryDate");

            // Multi-tenancy query filter is handled globally in ApplicationDbContext

            // Relationships
            builder.HasOne(pm => pm.Client)
                .WithMany()
                .HasForeignKey(pm => pm.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}