using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegionalLeagueApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerDisplayNameAndActiveStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "players",
                type: "character varying(160)",
                maxLength: 160,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE players
                SET "DisplayName" = btrim(concat_ws(' ', "FirstName", "LastName"))
                WHERE "DisplayName" = ''
                """);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "players",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_players_TeamId_IsActive",
                table: "players",
                columns: new[] { "TeamId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_players_TeamId_IsActive",
                table: "players");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "players");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "players");
        }
    }
}
