using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orbito.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintForActiveRetries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add unique constraint to prevent race conditions
            // Only one active retry (Scheduled or InProgress) allowed per payment
            migrationBuilder.CreateIndex(
                name: "IX_PaymentRetrySchedule_Payment_Active",
                table: "PaymentRetrySchedules",
                column: "PaymentId",
                unique: true,
                filter: "[Status] IN ('Scheduled', 'InProgress')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove unique constraint
            migrationBuilder.DropIndex(
                name: "IX_PaymentRetrySchedule_Payment_Active",
                table: "PaymentRetrySchedules");
        }
    }
}
