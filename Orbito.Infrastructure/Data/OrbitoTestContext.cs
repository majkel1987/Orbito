using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Orbito.Infrastructure.Models;

namespace Orbito.Infrastructure.Data;

public partial class OrbitoTestContext : DbContext
{
    public OrbitoTestContext()
    {
    }

    public OrbitoTestContext(DbContextOptions<OrbitoTestContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<EmailNotification> EmailNotifications { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PaymentDiscrepancy> PaymentDiscrepancies { get; set; }

    public virtual DbSet<PaymentHistory> PaymentHistories { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    public virtual DbSet<PaymentRetrySchedule> PaymentRetrySchedules { get; set; }

    public virtual DbSet<PaymentWebhookLog> PaymentWebhookLogs { get; set; }

    public virtual DbSet<Provider> Providers { get; set; }

    public virtual DbSet<ReconciliationReport> ReconciliationReports { get; set; }

    public virtual DbSet<Subscription> Subscriptions { get; set; }

    public virtual DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }

    public virtual DbSet<TeamMember> TeamMembers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "IX_AspNetRoles_NormalizedName")
                .IsUnique()
                .HasFilter("([NormalizedName] IS NOT NULL)");

            entity.HasIndex(e => e.TenantId, "IX_AspNetRoles_TenantId");

            entity.HasIndex(e => new { e.TenantId, e.NormalizedName }, "IX_AspNetRoles_TenantId_NormalizedName");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);

            entity.HasOne(d => d.Tenant).WithMany(p => p.AspNetRoles)
                .HasPrincipalKey(p => p.TenantId)
                .HasForeignKey(d => d.TenantId);
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.Email, "IX_AspNetUsers_Email").IsUnique();

            entity.HasIndex(e => e.NormalizedEmail, "IX_AspNetUsers_NormalizedEmail");

            entity.HasIndex(e => e.NormalizedUserName, "IX_AspNetUsers_NormalizedUserName")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.HasIndex(e => e.TenantId, "IX_AspNetUsers_TenantId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                    });
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasIndex(e => e.TenantId, "IX_Clients_TenantId");

            entity.HasIndex(e => new { e.TenantId, e.DirectEmail }, "IX_Clients_TenantId_DirectEmail").HasFilter("([DirectEmail] IS NOT NULL)");

            entity.HasIndex(e => e.UserId, "IX_Clients_UserId")
                .IsUnique()
                .HasFilter("([UserId] IS NOT NULL)");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CompanyName).HasMaxLength(200);
            entity.Property(e => e.DirectEmail).HasMaxLength(255);
            entity.Property(e => e.DirectFirstName).HasMaxLength(100);
            entity.Property(e => e.DirectLastName).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);

            entity.HasOne(d => d.Tenant).WithMany(p => p.Clients)
                .HasPrincipalKey(p => p.TenantId)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.User).WithOne(p => p.Client)
                .HasForeignKey<Client>(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<EmailNotification>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasIndex(e => e.ClientId, "IX_Payments_ClientId");

            entity.HasIndex(e => e.CreatedAt, "IX_Payments_CreatedAt");

            entity.HasIndex(e => e.ExternalPaymentId, "IX_Payments_ExternalPaymentId").HasFilter("([ExternalPaymentId] IS NOT NULL)");

            entity.HasIndex(e => e.ExternalTransactionId, "IX_Payments_ExternalTransactionId")
                .IsUnique()
                .HasFilter("([ExternalTransactionId] IS NOT NULL)");

            entity.HasIndex(e => e.SubscriptionId, "IX_Payments_SubscriptionId");

            entity.HasIndex(e => new { e.SubscriptionId, e.Status }, "IX_Payments_SubscriptionId_Status_Unique")
                .IsUnique()
                .HasFilter("([Status] IN ('Pending', 'Processing'))");

            entity.HasIndex(e => e.TenantId, "IX_Payments_TenantId");

            entity.HasIndex(e => new { e.TenantId, e.Status }, "IX_Payments_TenantId_Status");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.ExternalPaymentId).HasMaxLength(255);
            entity.Property(e => e.ExternalTransactionId).HasMaxLength(255);
            entity.Property(e => e.FailureReason).HasMaxLength(1000);
            entity.Property(e => e.IdempotencyKey).HasMaxLength(100);
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.PaymentMethodId).HasMaxLength(255);
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Client).WithMany(p => p.Payments)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Subscription).WithMany(p => p.Payments)
                .HasForeignKey(d => d.SubscriptionId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<PaymentDiscrepancy>(entity =>
        {
            entity.HasIndex(e => e.DetectedAt, "ix_payment_discrepancies_detected_at");

            entity.HasIndex(e => e.ExternalPaymentId, "ix_payment_discrepancies_external_payment_id");

            entity.HasIndex(e => e.PaymentId, "ix_payment_discrepancies_payment_id");

            entity.HasIndex(e => e.ReconciliationReportId, "ix_payment_discrepancies_report_id");

            entity.HasIndex(e => e.TenantId, "ix_payment_discrepancies_tenant_id");

            entity.HasIndex(e => new { e.TenantId, e.Resolution }, "ix_payment_discrepancies_tenant_resolution");

            entity.HasIndex(e => new { e.TenantId, e.Type }, "ix_payment_discrepancies_tenant_type");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AdditionalData)
                .HasMaxLength(4000)
                .HasColumnName("additional_data");
            entity.Property(e => e.DetectedAt).HasColumnName("detected_at");
            entity.Property(e => e.ExternalPaymentId)
                .HasMaxLength(255)
                .HasColumnName("external_payment_id");
            entity.Property(e => e.OrbitoAmount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("orbito_amount");
            entity.Property(e => e.OrbitoCurrency)
                .HasMaxLength(3)
                .HasColumnName("orbito_currency");
            entity.Property(e => e.OrbitoStatus).HasColumnName("orbito_status");
            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.ReconciliationReportId).HasColumnName("reconciliation_report_id");
            entity.Property(e => e.Resolution).HasColumnName("resolution");
            entity.Property(e => e.ResolutionNotes)
                .HasMaxLength(2000)
                .HasColumnName("resolution_notes");
            entity.Property(e => e.ResolvedAt).HasColumnName("resolved_at");
            entity.Property(e => e.ResolvedBy)
                .HasMaxLength(255)
                .HasColumnName("resolved_by");
            entity.Property(e => e.StripeAmount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("stripe_amount");
            entity.Property(e => e.StripeCurrency)
                .HasMaxLength(3)
                .HasColumnName("stripe_currency");
            entity.Property(e => e.StripeStatus)
                .HasMaxLength(100)
                .HasColumnName("stripe_status");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.Type).HasColumnName("type");

            entity.HasOne(d => d.Payment).WithMany(p => p.PaymentDiscrepancies)
                .HasForeignKey(d => d.PaymentId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.ReconciliationReport).WithMany(p => p.PaymentDiscrepancies).HasForeignKey(d => d.ReconciliationReportId);
        });

        modelBuilder.Entity<PaymentHistory>(entity =>
        {
            entity.ToTable("PaymentHistory");

            entity.HasIndex(e => e.OccurredAt, "IX_PaymentHistory_OccurredAt");

            entity.HasIndex(e => e.PaymentId, "IX_PaymentHistory_PaymentId");

            entity.HasIndex(e => new { e.PaymentId, e.OccurredAt }, "IX_PaymentHistory_PaymentId_OccurredAt");

            entity.HasIndex(e => e.Status, "IX_PaymentHistory_Status");

            entity.HasIndex(e => e.TenantId, "IX_PaymentHistory_TenantId");

            entity.HasIndex(e => new { e.TenantId, e.PaymentId }, "IX_PaymentHistory_TenantId_PaymentId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Action).HasMaxLength(100);
            entity.Property(e => e.Details).HasMaxLength(2000);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);

            entity.HasOne(d => d.Payment).WithMany(p => p.PaymentHistories).HasForeignKey(d => d.PaymentId);
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasIndex(e => new { e.ClientId, e.IsDefault }, "IX_PaymentMethods_ClientId_IsDefault").HasFilter("([IsDefault]=(1))");

            entity.HasIndex(e => e.ExpiryDate, "IX_PaymentMethods_ExpiryDate");

            entity.HasIndex(e => new { e.TenantId, e.ClientId }, "IX_PaymentMethods_TenantId_ClientId");

            entity.HasIndex(e => new { e.Type, e.CreatedAt }, "IX_PaymentMethods_Type_CreatedAt");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.LastFourDigits).HasMaxLength(4);
            entity.Property(e => e.Token).HasMaxLength(500);
            entity.Property(e => e.Type).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Client).WithMany(p => p.PaymentMethods).HasForeignKey(d => d.ClientId);
        });

        modelBuilder.Entity<PaymentRetrySchedule>(entity =>
        {
            entity.HasIndex(e => e.PaymentId, "IX_PaymentRetrySchedule_Payment_Active")
                .IsUnique()
                .HasFilter("([Status] IN ('Scheduled', 'InProgress'))");

            entity.HasIndex(e => e.ClientId, "IX_PaymentRetrySchedules_ClientId");

            entity.HasIndex(e => new { e.PaymentId, e.Status }, "IX_PaymentRetrySchedules_PaymentId_Status");

            entity.HasIndex(e => new { e.TenantId, e.ClientId, e.Status, e.NextAttemptAt }, "IX_PaymentRetrySchedules_TenantId_ClientId_Status_NextAttemptAt").HasFilter("([Status]='Scheduled')");

            entity.HasIndex(e => new { e.TenantId, e.CreatedAt }, "IX_PaymentRetrySchedules_TenantId_CreatedAt");

            entity.HasIndex(e => new { e.TenantId, e.Status, e.NextAttemptAt }, "IX_PaymentRetrySchedules_TenantId_Status_NextAttemptAt").HasFilter("([Status] IN ('Scheduled', 'InProgress'))");

            entity.HasIndex(e => new { e.TenantId, e.UpdatedAt }, "IX_PaymentRetrySchedules_TenantId_UpdatedAt");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasPrecision(3);
            entity.Property(e => e.LastError).HasMaxLength(2000);
            entity.Property(e => e.MaxAttempts).HasDefaultValue(5);
            entity.Property(e => e.NextAttemptAt).HasPrecision(3);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.UpdatedAt).HasPrecision(3);

            entity.HasOne(d => d.Client).WithMany(p => p.PaymentRetrySchedules)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Payment).WithOne(p => p.PaymentRetrySchedule).HasForeignKey<PaymentRetrySchedule>(d => d.PaymentId);
        });

        modelBuilder.Entity<PaymentWebhookLog>(entity =>
        {
            entity.HasIndex(e => new { e.Status, e.ReceivedAt }, "IX_PaymentWebhookLogs_Status_ReceivedAt").HasFilter("([Status]='Failed')");

            entity.HasIndex(e => new { e.TenantId, e.EventId }, "IX_PaymentWebhookLogs_TenantId_EventId").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
            entity.Property(e => e.EventId).HasMaxLength(255);
            entity.Property(e => e.EventType).HasMaxLength(100);
            entity.Property(e => e.Metadata).HasMaxLength(2000);
            entity.Property(e => e.Provider).HasMaxLength(100);
            entity.Property(e => e.ReceivedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Status).HasMaxLength(20);
        });

        modelBuilder.Entity<Provider>(entity =>
        {
            entity.HasIndex(e => e.TenantId, "AK_Providers_TenantId").IsUnique();

            entity.HasIndex(e => e.SubdomainSlug, "IX_Providers_SubdomainSlug").IsUnique();

            entity.HasIndex(e => e.TenantId, "IX_Providers_TenantId").IsUnique();

            entity.HasIndex(e => e.UserId, "IX_Providers_UserId")
                .IsUnique()
                .HasFilter("([UserId] IS NOT NULL)");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Avatar).HasMaxLength(500);
            entity.Property(e => e.BusinessName).HasMaxLength(200);
            entity.Property(e => e.CustomDomain).HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.MonthlyRevenueAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MonthlyRevenueCurrency).HasMaxLength(3);
            entity.Property(e => e.SubdomainSlug).HasMaxLength(100);

            entity.HasOne(d => d.User).WithOne(p => p.Provider).HasForeignKey<Provider>(d => d.UserId);
        });

        modelBuilder.Entity<ReconciliationReport>(entity =>
        {
            entity.HasIndex(e => new { e.PeriodStart, e.PeriodEnd }, "ix_reconciliation_reports_period");

            entity.HasIndex(e => e.TenantId, "ix_reconciliation_reports_tenant_id");

            entity.HasIndex(e => new { e.TenantId, e.RunDate }, "ix_reconciliation_reports_tenant_run_date");

            entity.HasIndex(e => new { e.TenantId, e.Status }, "ix_reconciliation_reports_tenant_status");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AutoResolvedCount).HasColumnName("auto_resolved_count");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.DiscrepanciesCount).HasColumnName("discrepancies_count");
            entity.Property(e => e.Duration).HasColumnName("duration");
            entity.Property(e => e.ErrorMessage)
                .HasMaxLength(2000)
                .HasColumnName("error_message");
            entity.Property(e => e.ManualReviewCount).HasColumnName("manual_review_count");
            entity.Property(e => e.MatchedPayments).HasColumnName("matched_payments");
            entity.Property(e => e.MismatchedPayments).HasColumnName("mismatched_payments");
            entity.Property(e => e.PeriodEnd).HasColumnName("period_end");
            entity.Property(e => e.PeriodStart).HasColumnName("period_start");
            entity.Property(e => e.RunDate).HasColumnName("run_date");
            entity.Property(e => e.StartedAt).HasColumnName("started_at");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.TotalPayments).HasColumnName("total_payments");
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasIndex(e => e.ClientId, "IX_Subscriptions_ClientId");

            entity.HasIndex(e => e.NextBillingDate, "IX_Subscriptions_NextBillingDate");

            entity.HasIndex(e => e.PlanId, "IX_Subscriptions_PlanId");

            entity.HasIndex(e => e.TenantId, "IX_Subscriptions_TenantId");

            entity.HasIndex(e => new { e.TenantId, e.Status }, "IX_Subscriptions_TenantId_Status");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.BillingPeriodType).HasMaxLength(20);
            entity.Property(e => e.CurrentPriceAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CurrentPriceCurrency).HasMaxLength(3);
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Client).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Plan).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.PlanId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Tenant).WithMany(p => p.Subscriptions)
                .HasPrincipalKey(p => p.TenantId)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<SubscriptionPlan>(entity =>
        {
            entity.HasIndex(e => e.TenantId, "IX_SubscriptionPlans_TenantId");

            entity.HasIndex(e => new { e.TenantId, e.IsActive }, "IX_SubscriptionPlans_TenantId_IsActive");

            entity.HasIndex(e => new { e.TenantId, e.IsActive, e.IsPublic }, "IX_SubscriptionPlans_TenantId_IsActive_IsPublic");

            entity.HasIndex(e => new { e.TenantId, e.IsPublic }, "IX_SubscriptionPlans_TenantId_IsPublic");

            entity.HasIndex(e => new { e.TenantId, e.SortOrder }, "IX_SubscriptionPlans_TenantId_SortOrder");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.BillingPeriodType).HasMaxLength(20);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsPublic).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.PriceAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PriceCurrency).HasMaxLength(3);

            entity.HasOne(d => d.Tenant).WithMany(p => p.SubscriptionPlans)
                .HasPrincipalKey(p => p.TenantId)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<TeamMember>(entity =>
        {
            entity.HasIndex(e => e.InvitationToken, "IX_TeamMembers_InvitationToken")
                .IsUnique()
                .HasFilter("([invitation_token] IS NOT NULL)");

            entity.HasIndex(e => e.InvitedAt, "IX_TeamMembers_InvitedAt");

            entity.HasIndex(e => e.IsActive, "IX_TeamMembers_IsActive");

            entity.HasIndex(e => e.Role, "IX_TeamMembers_Role");

            entity.HasIndex(e => e.TenantId, "IX_TeamMembers_TenantId");

            entity.HasIndex(e => new { e.TenantId, e.Email }, "IX_TeamMembers_TenantId_Email").IsUnique();

            entity.HasIndex(e => new { e.TenantId, e.UserId }, "IX_TeamMembers_TenantId_UserId").IsUnique();

            entity.HasIndex(e => e.UserId, "IX_TeamMembers_UserId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AcceptedAt).HasColumnName("accepted_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .HasColumnName("first_name");
            entity.Property(e => e.InvitationExpiresAt).HasColumnName("invitation_expires_at");
            entity.Property(e => e.InvitationToken)
                .HasMaxLength(100)
                .HasColumnName("invitation_token");
            entity.Property(e => e.InvitedAt).HasColumnName("invited_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.LastActiveAt).HasColumnName("last_active_at");
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .HasColumnName("last_name");
            entity.Property(e => e.RemovedAt).HasColumnName("removed_at");
            entity.Property(e => e.Role).HasColumnName("role");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TeamMembers)
                .HasPrincipalKey(p => p.TenantId)
                .HasForeignKey(d => d.TenantId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
