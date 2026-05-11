using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegionalLeagueApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProposalChangePayload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChangeType",
                table: "match_update_proposals",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "MatchStatusAndResult");

            migrationBuilder.AddColumn<string>(
                name: "PayloadJson",
                table: "match_update_proposals",
                type: "jsonb",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.CreateIndex(
                name: "IX_match_update_proposals_Status_ChangeType",
                table: "match_update_proposals",
                columns: new[] { "Status", "ChangeType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_match_update_proposals_Status_ChangeType",
                table: "match_update_proposals");

            migrationBuilder.DropColumn(
                name: "ChangeType",
                table: "match_update_proposals");

            migrationBuilder.DropColumn(
                name: "PayloadJson",
                table: "match_update_proposals");
        }
    }
}
