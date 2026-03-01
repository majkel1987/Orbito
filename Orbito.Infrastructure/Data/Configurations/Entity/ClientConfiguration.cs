using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;

namespace Orbito.Infrastructure.Data.Configurations.Entity
{
    public class ClientConfiguration : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            // Primary Key
            builder.HasKey(c => c.Id);

            // TenantId - required for multi-tenancy
            builder.Property(c => c.TenantId)
                .HasConversion(
                    tenantId => tenantId.Value,
                    guid => TenantId.Create(guid))
                .IsRequired()
                .HasColumnName("TenantId");

            // Basic Properties
            builder.Property(c => c.CompanyName)
                .HasMaxLength(200);

            builder.Property(c => c.Phone)
                .HasMaxLength(20);

            builder.Property(c => c.DirectEmail)
                .HasMaxLength(255);

            builder.Property(c => c.DirectFirstName)
                .HasMaxLength(100);

            builder.Property(c => c.DirectLastName)
                .HasMaxLength(100);

            // Invitation Flow
            builder.Property(c => c.Status)
                .IsRequired()
                .HasDefaultValue(ClientStatus.Inactive);

            builder.Property(c => c.InvitationToken)
                .HasMaxLength(200);

            builder.Property(c => c.InvitationTokenExpiresAt);

            builder.Property(c => c.ConfirmedAt);

            // Timestamps
            builder.Property(c => c.CreatedAt)
                .IsRequired();

            // Indexes
            builder.HasIndex(c => c.TenantId)
                .HasDatabaseName("IX_Clients_TenantId");

            builder.HasIndex(c => c.UserId)
                .HasDatabaseName("IX_Clients_UserId");

            builder.HasIndex(c => new { c.TenantId, c.DirectEmail })
                .HasDatabaseName("IX_Clients_TenantId_DirectEmail")
                .HasFilter("DirectEmail IS NOT NULL");

            // Relationships
            builder.HasOne(c => c.User)
                .WithOne(u => u.ClientProfile)
                .HasForeignKey<Client>(c => c.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(c => c.Provider)
                .WithMany(p => p.Clients)
                .HasForeignKey(c => c.TenantId)
                .HasPrincipalKey(p => p.TenantId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(c => c.Subscriptions)
                .WithOne(s => s.Client)
                .HasForeignKey(s => s.ClientId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(c => c.Payments)
                .WithOne(p => p.Client)
                .HasForeignKey(p => p.ClientId)
                .OnDelete(DeleteBehavior.NoAction);

            // Table name
            builder.ToTable("Clients");
        }
    }
}
