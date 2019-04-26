using Microsoft.EntityFrameworkCore.Migrations;

namespace GuildTools.Migrations
{
    public partial class ProfileName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfileName",
                table: "GuildProfile",
                maxLength: 150,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfileName",
                table: "GuildProfile");
        }
    }
}
