using Microsoft.EntityFrameworkCore.Migrations;

namespace GuildTools.Migrations
{
    public partial class GuildAbbreviations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Abbreviation",
                table: "FriendGuilds");

            migrationBuilder.AddColumn<string>(
                name: "Abbreviation",
                table: "StoredGuilds",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Abbreviation",
                table: "StoredGuilds");

            migrationBuilder.AddColumn<string>(
                name: "Abbreviation",
                table: "FriendGuilds",
                nullable: true);
        }
    }
}
