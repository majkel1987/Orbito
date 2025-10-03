using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orbito.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixEntityFrameworkConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentMethods_Clients_ClientId1",
                table: "PaymentMethods");

            migrationBuilder.DropIndex(
                name: "IX_PaymentWebhookLogs_EventType_ProcessedAt",
                table: "PaymentWebhookLogs");

            migrationBuilder.DropIndex(
                name: "IX_PaymentWebhookLogs_ProcessedAt",
                table: "PaymentWebhookLogs");

            migrationBuilder.DropIndex(
                name: "IX_PaymentWebhookLogs_Provider_Status",
                table: "PaymentWebhookLogs");

            migrationBuilder.DropIndex(
                name: "IX_PaymentWebhookLogs_Status",
                table: "PaymentWebhookLogs");

            migrationBuilder.DropIndex(
                name: "IX_PaymentMethods_ClientId_IsDefault",
                table: "PaymentMethods");

            migrationBuilder.DropIndex(
                name: "IX_PaymentMethods_ClientId1",
                table: "PaymentMethods");

            migrationBuilder.DropColumn(
                name: "ClientId1",
                table: "PaymentMethods");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "PaymentWebhookLogs",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ProcessedAt",
                table: "PaymentWebhookLogs",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<int>(
                name: "Attempts",
                table: "PaymentWebhookLogs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "PaymentMethods",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentWebhookLogs_Status_ReceivedAt",
                table: "PaymentWebhookLogs",
                columns: new[] { "Status", "ReceivedAt" },
                filter: "[Status] = 'Failed'");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_ClientId_IsDefault",
                table: "PaymentMethods",
                columns: new[] { "ClientId", "IsDefault" },
                filter: "[IsDefault] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentWebhookLogs_Status_ReceivedAt",
                table: "PaymentWebhookLogs");

            migrationBuilder.DropIndex(
                name: "IX_PaymentMethods_ClientId_IsDefault",
                table: "PaymentMethods");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "PaymentWebhookLogs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ProcessedAt",
                table: "PaymentWebhookLogs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Attempts",
                table: "PaymentWebhookLogs",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "PaymentMethods",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<Guid>(
                name: "ClientId1",
                table: "PaymentMethods",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentWebhookLogs_EventType_ProcessedAt",
                table: "PaymentWebhookLogs",
                columns: new[] { "EventType", "ProcessedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentWebhookLogs_ProcessedAt",
                table: "PaymentWebhookLogs",
                column: "ProcessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentWebhookLogs_Provider_Status",
                table: "PaymentWebhookLogs",
                columns: new[] { "Provider", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentWebhookLogs_Status",
                table: "PaymentWebhookLogs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_ClientId_IsDefault",
                table: "PaymentMethods",
                columns: new[] { "ClientId", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_ClientId1",
                table: "PaymentMethods",
                column: "ClientId1");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentMethods_Clients_ClientId1",
                table: "PaymentMethods",
                column: "ClientId1",
                principalTable: "Clients",
                principalColumn: "Id");
        }
    }
}
