using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegionalLeagueApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftArchiveFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedAt",
                table: "teams",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "teams",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedAt",
                table: "players",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedAt",
                table: "leagues",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "leagues",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedAt",
                table: "competitions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "competitions",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedAt",
                table: "clubs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "clubs",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "teams");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "teams");

            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "players");

            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "leagues");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "leagues");

            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "competitions");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "competitions");

            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "clubs");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "clubs");
        }
    }
}
