using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orbito.Domain.Entities;
using Orbito.Domain.ValueObjects;

namespace Orbito.Infrastructure.Data.Configurations.Entity
{
    public class ProviderConfiguration : IEntityTypeConfiguration<Provider>
    {
        public void Configure(EntityTypeBuilder<Provider> builder)
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

            // Basic Properties
            builder.Property(p => p.BusinessName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Description)
                .HasMaxLength(1000);

            builder.Property(p => p.Avatar)
                .HasMaxLength(500);

            builder.Property(p => p.SubdomainSlug)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.CustomDomain)
                .HasMaxLength(255);

            // Money Value Object
            builder.OwnsOne(p => p.MonthlyRevenue, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("MonthlyRevenueAmount")
                    .HasPrecision(18, 2);
                money.Property(m => m.Currency)
                    .HasColumnName("MonthlyRevenueCurrency")
                    .HasMaxLength(3);
            });

            // Timestamps
            builder.Property(p => p.CreatedAt)
                .IsRequired();

            // Indexes
            builder.HasIndex(p => p.TenantId)
                .IsUnique()
                .HasDatabaseName("IX_Providers_TenantId");

            builder.HasIndex(p => p.SubdomainSlug)
                .IsUnique()
                .HasDatabaseName("IX_Providers_SubdomainSlug");

            builder.HasIndex(p => p.UserId)
                .HasDatabaseName("IX_Providers_UserId");

            // Relationships
            builder.HasOne(p => p.User)
                .WithOne(u => u.Provider)
                .HasForeignKey<Provider>(p => p.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(p => p.Plans)
                .WithOne(plan => plan.Provider)
                .HasForeignKey(plan => plan.TenantId)
                .HasPrincipalKey(p => p.TenantId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(p => p.Clients)
                .WithOne(c => c.Provider)
                .HasForeignKey(c => c.TenantId)
                .HasPrincipalKey(p => p.TenantId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(p => p.Subscriptions)
                .WithOne()
                .HasForeignKey("TenantId")
                .HasPrincipalKey(p => p.TenantId)
                .OnDelete(DeleteBehavior.NoAction);

            // Table name
            builder.ToTable("Providers");
        }
    }
}
