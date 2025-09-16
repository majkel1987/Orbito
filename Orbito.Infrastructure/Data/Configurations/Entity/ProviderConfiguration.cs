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
    public class ProviderConfiguration : IEntityTypeConfiguration<Provider>
    {
        public void Configure(EntityTypeBuilder<Provider> builder)
        {
            builder.ToTable("Providers");

            // Primary Key
            builder.HasKey(p => p.Id);

            // TenantId jako computed property - ignorujemy przy mapowaniu
            builder.Ignore(p => p.TenantId);

            // Basic Properties
            builder.Property(p => p.BusinessName)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(p => p.Description)
                .HasMaxLength(2000);

            builder.Property(p => p.Avatar)
                .HasMaxLength(500);

            builder.Property(p => p.SubdomainSlug)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(p => p.CustomDomain)
                .HasMaxLength(255);

            builder.Property(p => p.IsActive)
                .HasDefaultValue(true);

            builder.Property(p => p.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(p => p.ActiveClientsCount)
                .HasDefaultValue(0);

            // Money Value Object - MonthlyRevenue
            builder.OwnsOne(p => p.MonthlyRevenue, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("MonthlyRevenueAmount")
                    .HasPrecision(18, 2)
                    .HasDefaultValue(0);

                money.Property(m => m.Currency)
                    .HasColumnName("MonthlyRevenueCurrency")
                    .HasMaxLength(3)
                    .HasDefaultValue("PLN");
            });

            // Unique Constraints
            builder.HasIndex(p => p.SubdomainSlug)
                .IsUnique()
                .HasDatabaseName("IX_Providers_SubdomainSlug");

            builder.HasIndex(p => p.CustomDomain)
                .IsUnique()
                .HasDatabaseName("IX_Providers_CustomDomain")
                .HasFilter("CustomDomain IS NOT NULL");

            // Other Indexes
            builder.HasIndex(p => p.IsActive)
                .HasDatabaseName("IX_Providers_IsActive");

            builder.HasIndex(p => p.CreatedAt)
                .HasDatabaseName("IX_Providers_CreatedAt");

            // Relationships
            builder.HasOne(p => p.User)
                .WithOne(u => u.Provider)
                .HasForeignKey<Provider>(p => p.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // POPRAWIONE relacje - używamy Provider.Id jako principal key
            builder.HasMany(p => p.Plans)
                .WithOne(plan => plan.Provider)
                .HasForeignKey("TenantId")
                .HasPrincipalKey(p => p.Id)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Clients)
                .WithOne(c => c.Provider)
                .HasForeignKey("TenantId")
                .HasPrincipalKey(p => p.Id)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Subscriptions)
                .WithOne()
                .HasForeignKey("TenantId")
                .HasPrincipalKey(p => p.Id)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}