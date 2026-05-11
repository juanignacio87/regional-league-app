using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegionalLeagueApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClubLogoPath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoPath",
                table: "clubs",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoPath",
                table: "clubs");
        }
    }
}
