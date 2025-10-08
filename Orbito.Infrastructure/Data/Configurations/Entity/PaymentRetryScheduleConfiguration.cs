using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;

namespace Orbito.Infrastructure.Data.Configurations.Entity
{
    /// <summary>
    /// EF Core configuration for PaymentRetrySchedule entity
    /// </summary>
    public class PaymentRetryScheduleConfiguration : IEntityTypeConfiguration<PaymentRetrySchedule>
    {
        public void Configure(EntityTypeBuilder<PaymentRetrySchedule> builder)
        {
            // Primary Key
            builder.HasKey(prs => prs.Id);

            // TenantId - required for multi-tenancy
            builder.Property(prs => prs.TenantId)
                .HasConversion(
                    tenantId => tenantId.Value,
                    guid => TenantId.Create(guid))
                .IsRequired()
                .HasColumnName("TenantId");

            // Foreign Keys with explicit column names
            builder.Property(prs => prs.ClientId)
                .IsRequired()
                .HasColumnName("ClientId");

            builder.Property(prs => prs.PaymentId)
                .IsRequired()
                .HasColumnName("PaymentId");

            // Status with validation
            builder.Property(prs => prs.Status)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(20);

            // Timestamps with precision
            builder.Property(prs => prs.NextAttemptAt)
                .IsRequired()
                .HasPrecision(3);

            builder.Property(prs => prs.AttemptNumber)
                .IsRequired()
                .HasAnnotation("MinValue", 1)
                .HasAnnotation("MaxValue", 10);

            builder.Property(prs => prs.MaxAttempts)
                .IsRequired()
                .HasDefaultValue(5)
                .HasAnnotation("MinValue", 1)
                .HasAnnotation("MaxValue", 10);

            builder.Property(prs => prs.LastError)
                .HasMaxLength(2000)
                .IsUnicode(true);

            builder.Property(prs => prs.CreatedAt)
                .IsRequired()
                .HasPrecision(3);

            builder.Property(prs => prs.UpdatedAt)
                .IsRequired()
                .HasPrecision(3);

            // Relationships
            builder.HasOne(prs => prs.Payment)
                .WithMany()
                .HasForeignKey(prs => prs.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(prs => prs.Client)
                .WithMany()
                .HasForeignKey(prs => prs.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes for performance and security filtering
            builder.HasIndex(prs => new { prs.TenantId, prs.ClientId, prs.Status, prs.NextAttemptAt })
                .HasDatabaseName("IX_PaymentRetrySchedules_TenantId_ClientId_Status_NextAttemptAt")
                .HasFilter("[Status] = 'Scheduled'");

            builder.HasIndex(prs => new { prs.PaymentId, prs.Status })
                .HasDatabaseName("IX_PaymentRetrySchedules_PaymentId_Status");

            // Additional performance indexes
            builder.HasIndex(prs => new { prs.TenantId, prs.CreatedAt })
                .HasDatabaseName("IX_PaymentRetrySchedules_TenantId_CreatedAt");

            builder.HasIndex(prs => new { prs.TenantId, prs.UpdatedAt })
                .HasDatabaseName("IX_PaymentRetrySchedules_TenantId_UpdatedAt");

            // Index for overdue retries detection
            builder.HasIndex(prs => new { prs.TenantId, prs.Status, prs.NextAttemptAt })
                .HasDatabaseName("IX_PaymentRetrySchedules_TenantId_Status_NextAttemptAt")
                .HasFilter("[Status] IN ('Scheduled', 'InProgress')");

            // RACE CONDITION PREVENTION: Unique constraint to prevent duplicate active retries
            // Only one active retry (Scheduled or InProgress) allowed per payment
            builder.HasIndex(prs => prs.PaymentId)
                .IsUnique()
                .HasDatabaseName("IX_PaymentRetrySchedule_Payment_Active")
                .HasFilter("[Status] IN ('Scheduled', 'InProgress')");

            // Data validation constraints
            builder.HasCheckConstraint("CK_PaymentRetrySchedules_AttemptNumber_Range", 
                "[AttemptNumber] >= 1 AND [AttemptNumber] <= 10");
            
            builder.HasCheckConstraint("CK_PaymentRetrySchedules_MaxAttempts_Range", 
                "[MaxAttempts] >= 1 AND [MaxAttempts] <= 10");
            
            builder.HasCheckConstraint("CK_PaymentRetrySchedules_AttemptNumber_NotExceedMax", 
                "[AttemptNumber] <= [MaxAttempts]");
            
            builder.HasCheckConstraint("CK_PaymentRetrySchedules_NextAttemptAt_Future", 
                "[NextAttemptAt] >= [CreatedAt]");

            // Table name
            builder.ToTable("PaymentRetrySchedules");
        }
    }
}
