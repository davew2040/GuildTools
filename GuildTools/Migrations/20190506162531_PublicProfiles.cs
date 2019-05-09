using Microsoft.EntityFrameworkCore.Migrations;

namespace GuildTools.Migrations
{
    public partial class PublicProfiles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CreatorId",
                table: "GuildProfile",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "GuildProfile",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "GuildProfile");

            migrationBuilder.AlterColumn<string>(
                name: "CreatorId",
                table: "GuildProfile",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
