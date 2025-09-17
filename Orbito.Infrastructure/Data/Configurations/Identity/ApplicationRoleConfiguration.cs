using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orbito.Domain.Identity;
using Orbito.Domain.ValueObjects;

namespace Orbito.Infrastructure.Data.Configurations.Identity
{
    public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
    {
        public void Configure(EntityTypeBuilder<ApplicationRole> builder)
        {
            // Primary Key
            builder.HasKey(r => r.Id);

            // TenantId - nullable for multi-tenancy (global roles vs tenant-specific roles)
            builder.Property(r => r.TenantId)
                .HasConversion(
                    tenantId => tenantId != null ? tenantId.Value : (Guid?)null,
                    guid => guid.HasValue ? TenantId.Create(guid.Value) : null)
                .HasColumnName("TenantId");

            // Basic Properties
            builder.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(r => r.NormalizedName)
                .HasMaxLength(256);

            builder.Property(r => r.Description)
                .HasMaxLength(500);

            // Timestamps
            builder.Property(r => r.CreatedAt)
                .IsRequired();

            // Indexes
            builder.HasIndex(r => r.TenantId)
                .HasDatabaseName("IX_AspNetRoles_TenantId");

            builder.HasIndex(r => r.NormalizedName)
                .IsUnique()
                .HasDatabaseName("IX_AspNetRoles_NormalizedName");

            builder.HasIndex(r => new { r.TenantId, r.NormalizedName })
                .HasDatabaseName("IX_AspNetRoles_TenantId_NormalizedName");

            // Relationships
            builder.HasOne(r => r.Provider)
                .WithMany()
                .HasForeignKey(r => r.TenantId)
                .HasPrincipalKey(p => p.TenantId)
                .OnDelete(DeleteBehavior.NoAction);

            // Table name
            builder.ToTable("AspNetRoles");
        }
    }
}
