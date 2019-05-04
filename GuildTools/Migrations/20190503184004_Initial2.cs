using Microsoft.EntityFrameworkCore.Migrations;

namespace GuildTools.Migrations
{
    public partial class Initial2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_User_GuildProfilePermissions",
                table: "User_GuildProfilePermissions");

            migrationBuilder.AddPrimaryKey(
                name: "PK_User_GuildProfilePermissions",
                table: "User_GuildProfilePermissions",
                columns: new[] { "UserId", "ProfileId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_User_GuildProfilePermissions",
                table: "User_GuildProfilePermissions");

            migrationBuilder.AddPrimaryKey(
                name: "PK_User_GuildProfilePermissions",
                table: "User_GuildProfilePermissions",
                columns: new[] { "UserId", "PermissionLevelId", "ProfileId" });
        }
    }
}
