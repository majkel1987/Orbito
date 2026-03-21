using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orbito.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformPlansAndProviderSubscriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlatformPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PriceAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PriceCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    BillingPeriodValue = table.Column<int>(type: "int", nullable: false),
                    BillingPeriodType = table.Column<int>(type: "int", nullable: false),
                    TrialDays = table.Column<int>(type: "int", nullable: false, defaultValue: 14),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FeaturesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProviderSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProviderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlatformPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrialEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaidUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastNotificationSentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastNotificationTier = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProviderSubscriptions_PlatformPlans_PlatformPlanId",
                        column: x => x.PlatformPlanId,
                        principalTable: "PlatformPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProviderSubscriptions_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlatformPlans_IsActive",
                table: "PlatformPlans",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformPlans_Name",
                table: "PlatformPlans",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlatformPlans_SortOrder",
                table: "PlatformPlans",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderSubscriptions_PlatformPlanId",
                table: "ProviderSubscriptions",
                column: "PlatformPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderSubscriptions_ProviderId",
                table: "ProviderSubscriptions",
                column: "ProviderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProviderSubscriptions_Status",
                table: "ProviderSubscriptions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderSubscriptions_Status_TrialEndDate",
                table: "ProviderSubscriptions",
                columns: new[] { "Status", "TrialEndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ProviderSubscriptions_TrialEndDate",
                table: "ProviderSubscriptions",
                column: "TrialEndDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProviderSubscriptions");

            migrationBuilder.DropTable(
                name: "PlatformPlans");
        }
    }
}
