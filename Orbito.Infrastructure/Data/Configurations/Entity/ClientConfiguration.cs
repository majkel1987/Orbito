using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orbito.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orbito.Infrastructure.Data.Configurations.Entity
{
    public class ClientConfiguration : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            builder.ToTable("Clients");

            // Primary Key
            builder.HasKey(c => c.Id);

            // TenantId - required for multi-tenancy
            builder.Property(c => c.TenantId)
                .HasConversion(
                    tenantId => tenantId.Value,
                    guid => Domain.ValueObjects.TenantId.Create(guid))
                .IsRequired();

            // Optional Identity User relationship
            builder.Property(c => c.UserId);

            // Direct contact fields (for clients without Identity account)
            builder.Property(c => c.DirectEmail)
                .HasMaxLength(255);

            builder.Property(c => c.DirectFirstName)
                .HasMaxLength(100);

            builder.Property(c => c.DirectLastName)
                .HasMaxLength(100);

            builder.Property(c => c.CompanyName)
                .HasMaxLength(200);

            builder.Property(c => c.Phone)
                .HasMaxLength(20);

            builder.Property(c => c.IsActive)
                .HasDefaultValue(true);

            builder.Property(c => c.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Computed columns (not mapped)
            builder.Ignore(c => c.Email);
            builder.Ignore(c => c.FirstName);
            builder.Ignore(c => c.LastName);
            builder.Ignore(c => c.FullName);
            builder.Ignore(c => c.ActiveSubscription);

            // Indexes
            builder.HasIndex(c => c.TenantId)
                .HasDatabaseName("IX_Clients_TenantId");

            builder.HasIndex(c => new { c.TenantId, c.DirectEmail })
                .HasDatabaseName("IX_Clients_TenantId_DirectEmail")
                .HasFilter("DirectEmail IS NOT NULL");

            builder.HasIndex(c => new { c.TenantId, c.UserId })
                .HasDatabaseName("IX_Clients_TenantId_UserId")
                .HasFilter("UserId IS NOT NULL");

            builder.HasIndex(c => c.CreatedAt)
                .HasDatabaseName("IX_Clients_CreatedAt");

            // Relationships
            builder.HasOne(c => c.Provider)
                .WithMany(p => p.Clients)
                .HasForeignKey(c => c.TenantId)
                .HasPrincipalKey(p => p.Id)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(c => c.User)
                .WithOne(u => u.ClientProfile)
                .HasForeignKey<Client>(c => c.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(c => c.Subscriptions)
                .WithOne(s => s.Client)
                .HasForeignKey(s => s.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.Payments)
                .WithOne(p => p.Client)
                .HasForeignKey(p => p.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
