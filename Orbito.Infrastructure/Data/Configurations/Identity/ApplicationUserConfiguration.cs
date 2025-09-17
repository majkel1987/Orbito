using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orbito.Domain.Identity;
using Orbito.Domain.ValueObjects;

namespace Orbito.Infrastructure.Data.Configurations.Identity
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            // Primary Key
            builder.HasKey(u => u.Id);

            // TenantId - nullable for multi-tenancy
            builder.Property(u => u.TenantId)
                .HasConversion(
                    tenantId => tenantId != null ? tenantId.Value : (Guid?)null,
                    guid => guid.HasValue ? TenantId.Create(guid.Value) : null)
                .HasColumnName("TenantId");

            // Basic Properties
            builder.Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(u => u.NormalizedEmail)
                .HasMaxLength(256);

            builder.Property(u => u.UserName)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(u => u.NormalizedUserName)
                .HasMaxLength(256);

            builder.Property(u => u.PhoneNumber)
                .HasMaxLength(50);

            // Timestamps
            builder.Property(u => u.CreatedAt)
                .IsRequired();

            builder.Property(u => u.LastLoginAt);

            // Indexes
            builder.HasIndex(u => u.TenantId)
                .HasDatabaseName("IX_AspNetUsers_TenantId");

            builder.HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("IX_AspNetUsers_Email");

            builder.HasIndex(u => u.NormalizedEmail)
                .HasDatabaseName("IX_AspNetUsers_NormalizedEmail");

            builder.HasIndex(u => u.NormalizedUserName)
                .IsUnique()
                .HasDatabaseName("IX_AspNetUsers_NormalizedUserName");

            // Relationships
            builder.HasOne(u => u.Provider)
                .WithOne(p => p.User)
                .HasForeignKey<ApplicationUser>(u => u.Id)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(u => u.ClientProfile)
                .WithOne(c => c.User)
                .HasForeignKey<ApplicationUser>(u => u.Id)
                .OnDelete(DeleteBehavior.NoAction);

            // Table name
            builder.ToTable("AspNetUsers");
        }
    }
}
