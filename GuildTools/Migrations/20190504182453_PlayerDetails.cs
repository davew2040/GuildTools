using Microsoft.EntityFrameworkCore.Migrations;

namespace GuildTools.Migrations
{
    public partial class PlayerDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserRealm",
                table: "AspNetUsers",
                newName: "PlayerRealm");

            migrationBuilder.AddColumn<string>(
                name: "PlayerName",
                table: "AspNetUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlayerName",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "PlayerRealm",
                table: "AspNetUsers",
                newName: "UserRealm");
        }
    }
}
