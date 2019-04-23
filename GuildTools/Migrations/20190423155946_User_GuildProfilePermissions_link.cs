using Microsoft.EntityFrameworkCore.Migrations;

namespace GuildTools.Migrations
{
    public partial class User_GuildProfilePermissions_link : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GuildPermissionsPermissionLevelId",
                table: "UserData",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GuildPermissionsProfileId",
                table: "UserData",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuildPermissionsUserId",
                table: "UserData",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "User_GuildProfilePermissions",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    PermissionLevelId = table.Column<int>(nullable: false),
                    ProfileId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User_GuildProfilePermissions", x => new { x.UserId, x.PermissionLevelId, x.ProfileId });
                    table.ForeignKey(
                        name: "FK__GuildProfilePermissions_GuildPermissions",
                        column: x => x.PermissionLevelId,
                        principalTable: "GuildProfilePermissionLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK__GuildProfilePermissions_GuildProfiles",
                        column: x => x.ProfileId,
                        principalTable: "GuildProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserData_GuildPermissionsUserId_GuildPermissionsPermissionLevelId_GuildPermissionsProfileId",
                table: "UserData",
                columns: new[] { "GuildPermissionsUserId", "GuildPermissionsPermissionLevelId", "GuildPermissionsProfileId" });

            migrationBuilder.CreateIndex(
                name: "IX_User_GuildProfilePermissions_PermissionLevelId",
                table: "User_GuildProfilePermissions",
                column: "PermissionLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_User_GuildProfilePermissions_ProfileId",
                table: "User_GuildProfilePermissions",
                column: "ProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserData_User_GuildProfilePermissions_GuildPermissionsUserId_GuildPermissionsPermissionLevelId_GuildPermissionsProfileId",
                table: "UserData",
                columns: new[] { "GuildPermissionsUserId", "GuildPermissionsPermissionLevelId", "GuildPermissionsProfileId" },
                principalTable: "User_GuildProfilePermissions",
                principalColumns: new[] { "UserId", "PermissionLevelId", "ProfileId" },
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserData_User_GuildProfilePermissions_GuildPermissionsUserId_GuildPermissionsPermissionLevelId_GuildPermissionsProfileId",
                table: "UserData");

            migrationBuilder.DropTable(
                name: "User_GuildProfilePermissions");

            migrationBuilder.DropIndex(
                name: "IX_UserData_GuildPermissionsUserId_GuildPermissionsPermissionLevelId_GuildPermissionsProfileId",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "GuildPermissionsPermissionLevelId",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "GuildPermissionsProfileId",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "GuildPermissionsUserId",
                table: "UserData");
        }
    }
}
