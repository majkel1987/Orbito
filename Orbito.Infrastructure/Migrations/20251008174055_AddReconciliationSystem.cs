using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orbito.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReconciliationSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop existing indexes and constraints that depend on Status column
            migrationBuilder.DropIndex(
                name: "IX_PaymentRetrySchedule_Payment_Active",
                table: "PaymentRetrySchedules");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRetrySchedules_PaymentId_Status",
                table: "PaymentRetrySchedules");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRetrySchedules_TenantId_ClientId_Status_NextAttemptAt",
                table: "PaymentRetrySchedules");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "PaymentRetrySchedules",
                type: "datetime2(3)",
                precision: 3,
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "PaymentRetrySchedules",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "NextAttemptAt",
                table: "PaymentRetrySchedules",
                type: "datetime2(3)",
                precision: 3,
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "PaymentRetrySchedules",
                type: "datetime2(3)",
                precision: 3,
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "EmailNotifications",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ReconciliationReports",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    run_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    period_start = table.Column<DateTime>(type: "datetime2", nullable: false),
                    period_end = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    total_payments = table.Column<int>(type: "int", nullable: false),
                    matched_payments = table.Column<int>(type: "int", nullable: false),
                    mismatched_payments = table.Column<int>(type: "int", nullable: false),
                    discrepancies_count = table.Column<int>(type: "int", nullable: false),
                    auto_resolved_count = table.Column<int>(type: "int", nullable: false),
                    manual_review_count = table.Column<int>(type: "int", nullable: false),
                    started_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    completed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    duration = table.Column<TimeSpan>(type: "time", nullable: true),
                    error_message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReconciliationReports", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentDiscrepancies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    reconciliation_report_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    payment_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    external_payment_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    type = table.Column<int>(type: "int", nullable: false),
                    resolution = table.Column<int>(type: "int", nullable: false),
                    orbito_status = table.Column<int>(type: "int", nullable: true),
                    stripe_status = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    orbito_amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    orbito_currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    stripe_amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    stripe_currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    resolution_notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    resolved_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    resolved_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    additional_data = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    detected_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentDiscrepancies", x => x.id);
                    table.ForeignKey(
                        name: "FK_PaymentDiscrepancies_Payments_payment_id",
                        column: x => x.payment_id,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PaymentDiscrepancies_ReconciliationReports_reconciliation_report_id",
                        column: x => x.reconciliation_report_id,
                        principalTable: "ReconciliationReports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "Description",
                value: "Platform Administrator - Full system access");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "Description",
                value: "Service Provider - Manages clients and subscriptions");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "Description",
                value: "Client User - Manages own subscriptions and payments");

            // Re-create indexes after column type change
            migrationBuilder.CreateIndex(
                name: "IX_PaymentRetrySchedule_Payment_Active",
                table: "PaymentRetrySchedules",
                columns: new[] { "PaymentId", "Status" },
                unique: true,
                filter: "[Status] = 'Scheduled'");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRetrySchedules_PaymentId_Status",
                table: "PaymentRetrySchedules",
                columns: new[] { "PaymentId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRetrySchedules_TenantId_ClientId_Status_NextAttemptAt",
                table: "PaymentRetrySchedules",
                columns: new[] { "TenantId", "ClientId", "Status", "NextAttemptAt" },
                filter: "[Status] = 'Scheduled'");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRetrySchedules_TenantId_CreatedAt",
                table: "PaymentRetrySchedules",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRetrySchedules_TenantId_Status_NextAttemptAt",
                table: "PaymentRetrySchedules",
                columns: new[] { "TenantId", "Status", "NextAttemptAt" },
                filter: "[Status] IN ('Scheduled', 'InProgress')");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRetrySchedules_TenantId_UpdatedAt",
                table: "PaymentRetrySchedules",
                columns: new[] { "TenantId", "UpdatedAt" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_PaymentRetrySchedules_AttemptNumber_NotExceedMax",
                table: "PaymentRetrySchedules",
                sql: "[AttemptNumber] <= [MaxAttempts]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PaymentRetrySchedules_AttemptNumber_Range",
                table: "PaymentRetrySchedules",
                sql: "[AttemptNumber] >= 1 AND [AttemptNumber] <= 10");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PaymentRetrySchedules_MaxAttempts_Range",
                table: "PaymentRetrySchedules",
                sql: "[MaxAttempts] >= 1 AND [MaxAttempts] <= 10");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PaymentRetrySchedules_NextAttemptAt_Future",
                table: "PaymentRetrySchedules",
                sql: "[NextAttemptAt] >= [CreatedAt]");

            migrationBuilder.CreateIndex(
                name: "ix_payment_discrepancies_detected_at",
                table: "PaymentDiscrepancies",
                column: "detected_at");

            migrationBuilder.CreateIndex(
                name: "ix_payment_discrepancies_external_payment_id",
                table: "PaymentDiscrepancies",
                column: "external_payment_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_discrepancies_payment_id",
                table: "PaymentDiscrepancies",
                column: "payment_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_discrepancies_report_id",
                table: "PaymentDiscrepancies",
                column: "reconciliation_report_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_discrepancies_tenant_id",
                table: "PaymentDiscrepancies",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_discrepancies_tenant_resolution",
                table: "PaymentDiscrepancies",
                columns: new[] { "tenant_id", "resolution" });

            migrationBuilder.CreateIndex(
                name: "ix_payment_discrepancies_tenant_type",
                table: "PaymentDiscrepancies",
                columns: new[] { "tenant_id", "type" });

            migrationBuilder.CreateIndex(
                name: "ix_reconciliation_reports_period",
                table: "ReconciliationReports",
                columns: new[] { "period_start", "period_end" });

            migrationBuilder.CreateIndex(
                name: "ix_reconciliation_reports_tenant_id",
                table: "ReconciliationReports",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_reconciliation_reports_tenant_run_date",
                table: "ReconciliationReports",
                columns: new[] { "tenant_id", "run_date" });

            migrationBuilder.CreateIndex(
                name: "ix_reconciliation_reports_tenant_status",
                table: "ReconciliationReports",
                columns: new[] { "tenant_id", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentDiscrepancies");

            migrationBuilder.DropTable(
                name: "ReconciliationReports");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRetrySchedules_TenantId_CreatedAt",
                table: "PaymentRetrySchedules");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRetrySchedules_TenantId_Status_NextAttemptAt",
                table: "PaymentRetrySchedules");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRetrySchedules_TenantId_UpdatedAt",
                table: "PaymentRetrySchedules");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PaymentRetrySchedules_AttemptNumber_NotExceedMax",
                table: "PaymentRetrySchedules");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PaymentRetrySchedules_AttemptNumber_Range",
                table: "PaymentRetrySchedules");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PaymentRetrySchedules_MaxAttempts_Range",
                table: "PaymentRetrySchedules");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PaymentRetrySchedules_NextAttemptAt_Future",
                table: "PaymentRetrySchedules");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "EmailNotifications");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "PaymentRetrySchedules",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)",
                oldPrecision: 3);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "PaymentRetrySchedules",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<DateTime>(
                name: "NextAttemptAt",
                table: "PaymentRetrySchedules",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)",
                oldPrecision: 3);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "PaymentRetrySchedules",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)",
                oldPrecision: 3);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "Description",
                value: "Platform Administrator");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "Description",
                value: "Service Provider");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "Description",
                value: "Client User");
        }
    }
}
