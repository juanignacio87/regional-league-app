using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegionalLeagueApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AllowDuplicateInactivePlayerShirtNumbers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_players_TeamId_ShirtNumber",
                table: "players");

            migrationBuilder.CreateIndex(
                name: "IX_players_TeamId_ShirtNumber",
                table: "players",
                columns: new[] { "TeamId", "ShirtNumber" },
                unique: true,
                filter: "\"IsActive\" = true AND \"ShirtNumber\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_players_TeamId_ShirtNumber",
                table: "players");

            migrationBuilder.CreateIndex(
                name: "IX_players_TeamId_ShirtNumber",
                table: "players",
                columns: new[] { "TeamId", "ShirtNumber" },
                unique: true);
        }
    }
}
