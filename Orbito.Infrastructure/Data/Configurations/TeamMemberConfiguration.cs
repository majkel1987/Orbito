using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;

namespace Orbito.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for TeamMember entity.
/// </summary>
public class TeamMemberConfiguration : IEntityTypeConfiguration<TeamMember>
{
    public void Configure(EntityTypeBuilder<TeamMember> builder)
    {
        // Table name
        builder.ToTable("TeamMembers");

        // Primary key
        builder.HasKey(tm => tm.Id);

        // Properties
        builder.Property(tm => tm.TenantId)
            .HasConversion(
                v => v.Value,
                v => TenantId.Create(v))
            .IsRequired()
            .HasColumnName("tenant_id");

        builder.Property(tm => tm.UserId)
            .IsRequired()
            .HasColumnName("user_id");

        builder.Property(tm => tm.Role)
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<TeamMemberRole>(v))
            .IsRequired()
            .HasColumnName("role");

        builder.Property(tm => tm.Email)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("email");

        builder.Property(tm => tm.FirstName)
            .HasMaxLength(100)
            .HasColumnName("first_name");

        builder.Property(tm => tm.LastName)
            .HasMaxLength(100)
            .HasColumnName("last_name");

        builder.Property(tm => tm.IsActive)
            .IsRequired()
            .HasDefaultValue(true)
            .HasColumnName("is_active");

        builder.Property(tm => tm.InvitedAt)
            .IsRequired()
            .HasColumnName("invited_at");

        builder.Property(tm => tm.LastActiveAt)
            .IsRequired()
            .HasColumnName("last_active_at");

        builder.Property(tm => tm.AcceptedAt)
            .HasColumnName("accepted_at");

        builder.Property(tm => tm.RemovedAt)
            .HasColumnName("removed_at");

        builder.Property(tm => tm.InvitationToken)
            .HasMaxLength(100)
            .HasColumnName("invitation_token");

        builder.Property(tm => tm.InvitationExpiresAt)
            .HasColumnName("invitation_expires_at");

        // Indexes
        builder.HasIndex(tm => new { tm.TenantId, tm.UserId })
            .IsUnique()
            .HasDatabaseName("IX_TeamMembers_TenantId_UserId");

        builder.HasIndex(tm => new { tm.TenantId, tm.Email })
            .IsUnique()
            .HasDatabaseName("IX_TeamMembers_TenantId_Email");

        builder.HasIndex(tm => tm.TenantId)
            .HasDatabaseName("IX_TeamMembers_TenantId");

        builder.HasIndex(tm => tm.UserId)
            .HasDatabaseName("IX_TeamMembers_UserId");

        builder.HasIndex(tm => tm.Role)
            .HasDatabaseName("IX_TeamMembers_Role");

        builder.HasIndex(tm => tm.IsActive)
            .HasDatabaseName("IX_TeamMembers_IsActive");

        builder.HasIndex(tm => tm.InvitedAt)
            .HasDatabaseName("IX_TeamMembers_InvitedAt");

        builder.HasIndex(tm => tm.InvitationToken)
            .IsUnique()
            .HasFilter("[invitation_token] IS NOT NULL")
            .HasDatabaseName("IX_TeamMembers_InvitationToken");

        // Query filter for multi-tenancy is configured in ApplicationDbContext.ConfigureDomainTenantFiltering

        // Relationships
        builder.HasOne<Provider>()
            .WithMany()
            .HasForeignKey(tm => tm.TenantId)
            .HasPrincipalKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
