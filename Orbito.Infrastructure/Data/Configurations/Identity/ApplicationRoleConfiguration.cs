using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orbito.Domain.Identity;

namespace Orbito.Infrastructure.Data.Configurations.Identity
{
    public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
    {
        public void Configure(EntityTypeBuilder<ApplicationRole> builder)
        {
            builder.ToTable("Roles");

            builder.Property(r => r.Description)
            .HasMaxLength(500);

            builder.Property(r => r.IsActive)
                .HasDefaultValue(true);

            builder.Property(r => r.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            builder.HasIndex(r => r.TenantId)
                .HasDatabaseName("IX_Roles_TenantId");

            builder.HasIndex(r => new { r.TenantId, r.NormalizedName })
                .HasDatabaseName("IX_Roles_TenantId_Name")
                .IsUnique();

            // Relationships
            builder.HasOne(r => r.Provider)
                .WithMany()
                .HasForeignKey(r => r.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            // Global query filter for multi-tenancy
            // Note: This will be configured in ApplicationDbContext with ITenantInfo
        }
    }
}
