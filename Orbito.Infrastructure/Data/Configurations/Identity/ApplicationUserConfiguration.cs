using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orbito.Domain.Identity;

namespace Orbito.Infrastructure.Data.Configurations.Identity
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.ToTable("Users");

            // Basic properties
            builder.Property(u => u.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(u => u.LastName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(u => u.IsActive)
                .HasDefaultValue(true);

            builder.Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Multi-tenant indexes for performance
            builder.HasIndex(u => new { u.TenantId, u.NormalizedEmail })
                .HasDatabaseName("IX_Users_TenantId_Email")
                .IsUnique(false);

            builder.HasIndex(u => new { u.TenantId, u.NormalizedUserName })
                .HasDatabaseName("IX_Users_TenantId_UserName")
                .IsUnique(false);

            builder.HasIndex(u => u.TenantId)
                .HasDatabaseName("IX_Users_TenantId");

            // Additional indexes
            builder.HasIndex(u => u.CreatedAt)
                .HasDatabaseName("IX_Users_CreatedAt");

            builder.HasIndex(u => u.LastLoginAt)
                .HasDatabaseName("IX_Users_LastLoginAt");
        }
    }
}
