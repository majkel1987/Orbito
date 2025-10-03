using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orbito.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWebhooksAndPaymentMethods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_PaymentMethods_PaymentMethodId1",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_PaymentMethodId1",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_PaymentMethods_ClientId",
                table: "PaymentMethods");

            migrationBuilder.DropIndex(
                name: "IX_PaymentMethods_CreatedAt",
                table: "PaymentMethods");

            migrationBuilder.DropIndex(
                name: "IX_PaymentMethods_IsDefault",
                table: "PaymentMethods");

            migrationBuilder.DropIndex(
                name: "IX_PaymentMethods_TenantId",
                table: "PaymentMethods");

            migrationBuilder.DropIndex(
                name: "IX_PaymentMethods_Type",
                table: "PaymentMethods");

            migrationBuilder.DropColumn(
                name: "PaymentMethodId1",
                table: "Payments");

            migrationBuilder.AddColumn<string>(
                name: "ExternalSubscriptionId",
                table: "Subscriptions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "PaymentMethods",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "PaymentMethods",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "ClientId1",
                table: "PaymentMethods",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PaymentWebhookLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Attempts = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Metadata = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentWebhookLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_ClientId_IsDefault",
                table: "PaymentMethods",
                columns: new[] { "ClientId", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_ClientId1",
                table: "PaymentMethods",
                column: "ClientId1");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_ExpiryDate",
                table: "PaymentMethods",
                column: "ExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_Type_CreatedAt",
                table: "PaymentMethods",
                columns: new[] { "Type", "CreatedAt" });

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
                name: "IX_PaymentWebhookLogs_TenantId_EventId",
                table: "PaymentWebhookLogs",
                columns: new[] { "TenantId", "EventId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentMethods_Clients_ClientId1",
                table: "PaymentMethods",
                column: "ClientId1",
                principalTable: "Clients",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentMethods_Clients_ClientId1",
                table: "PaymentMethods");

            migrationBuilder.DropTable(
                name: "PaymentWebhookLogs");

            migrationBuilder.DropIndex(
                name: "IX_PaymentMethods_ClientId_IsDefault",
                table: "PaymentMethods");

            migrationBuilder.DropIndex(
                name: "IX_PaymentMethods_ClientId1",
                table: "PaymentMethods");

            migrationBuilder.DropIndex(
                name: "IX_PaymentMethods_ExpiryDate",
                table: "PaymentMethods");

            migrationBuilder.DropIndex(
                name: "IX_PaymentMethods_Type_CreatedAt",
                table: "PaymentMethods");

            migrationBuilder.DropColumn(
                name: "ExternalSubscriptionId",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "ClientId1",
                table: "PaymentMethods");

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentMethodId1",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "PaymentMethods",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "PaymentMethods",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentMethodId1",
                table: "Payments",
                column: "PaymentMethodId1");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_ClientId",
                table: "PaymentMethods",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_CreatedAt",
                table: "PaymentMethods",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_IsDefault",
                table: "PaymentMethods",
                column: "IsDefault");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_TenantId",
                table: "PaymentMethods",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_Type",
                table: "PaymentMethods",
                column: "Type");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_PaymentMethods_PaymentMethodId1",
                table: "Payments",
                column: "PaymentMethodId1",
                principalTable: "PaymentMethods",
                principalColumn: "Id");
        }
    }
}
