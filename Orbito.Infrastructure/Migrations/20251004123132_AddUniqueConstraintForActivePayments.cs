using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orbito.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintForActivePayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Payments_SubscriptionId_Status_Unique",
                table: "Payments",
                columns: new[] { "SubscriptionId", "Status" },
                unique: true,
                filter: "Status IN ('Pending', 'Processing')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_SubscriptionId_Status_Unique",
                table: "Payments");
        }
    }
}
