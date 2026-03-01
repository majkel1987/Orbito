using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orbito.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClientInvitationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmedAt",
                table: "Clients",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvitationToken",
                table: "Clients",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InvitationTokenExpiresAt",
                table: "Clients",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Clients",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfirmedAt",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "InvitationToken",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "InvitationTokenExpiresAt",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Clients");
        }
    }
}
