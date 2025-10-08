using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orbito.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePaymentRetrySchedule_AddClientIdAndFailedStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentRetrySchedules_NextAttemptAt_Status",
                table: "PaymentRetrySchedules");

            migrationBuilder.AlterColumn<string>(
                name: "LastError",
                table: "PaymentRetrySchedules",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClientId",
                table: "PaymentRetrySchedules",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRetrySchedules_ClientId",
                table: "PaymentRetrySchedules",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRetrySchedules_TenantId_ClientId_Status_NextAttemptAt",
                table: "PaymentRetrySchedules",
                columns: new[] { "TenantId", "ClientId", "Status", "NextAttemptAt" },
                filter: "[Status] = 'Scheduled'");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentRetrySchedules_Clients_ClientId",
                table: "PaymentRetrySchedules",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentRetrySchedules_Clients_ClientId",
                table: "PaymentRetrySchedules");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRetrySchedules_ClientId",
                table: "PaymentRetrySchedules");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRetrySchedules_TenantId_ClientId_Status_NextAttemptAt",
                table: "PaymentRetrySchedules");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "PaymentRetrySchedules");

            migrationBuilder.AlterColumn<string>(
                name: "LastError",
                table: "PaymentRetrySchedules",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRetrySchedules_NextAttemptAt_Status",
                table: "PaymentRetrySchedules",
                columns: new[] { "NextAttemptAt", "Status" },
                filter: "[Status] = 'Scheduled'");
        }
    }
}
