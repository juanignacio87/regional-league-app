using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegionalLeagueApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddContributorTeamAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_club_contributors_ClubId_UserId",
                table: "club_contributors");

            migrationBuilder.AddColumn<Guid>(
                name: "TeamId",
                table: "club_contributors",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_club_contributors_ClubId",
                table: "club_contributors",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_club_contributors_TeamId",
                table: "club_contributors",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_club_contributors_UserId_TeamId",
                table: "club_contributors",
                columns: new[] { "UserId", "TeamId" },
                unique: true,
                filter: "\"TeamId\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_club_contributors_teams_TeamId",
                table: "club_contributors",
                column: "TeamId",
                principalTable: "teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_club_contributors_teams_TeamId",
                table: "club_contributors");

            migrationBuilder.DropIndex(
                name: "IX_club_contributors_ClubId",
                table: "club_contributors");

            migrationBuilder.DropIndex(
                name: "IX_club_contributors_TeamId",
                table: "club_contributors");

            migrationBuilder.DropIndex(
                name: "IX_club_contributors_UserId_TeamId",
                table: "club_contributors");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "club_contributors");

            migrationBuilder.CreateIndex(
                name: "IX_club_contributors_ClubId_UserId",
                table: "club_contributors",
                columns: new[] { "ClubId", "UserId" },
                unique: true);
        }
    }
}
