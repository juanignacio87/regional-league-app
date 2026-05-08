using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegionalLeagueApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBasicMatchEventFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PlayerName",
                table: "match_events",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddCheckConstraint(
                name: "ck_match_events_minute_range",
                table: "match_events",
                sql: "\"Minute\" >= 0 AND \"Minute\" <= 130");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_match_events_minute_range",
                table: "match_events");

            migrationBuilder.DropColumn(
                name: "PlayerName",
                table: "match_events");
        }
    }
}
