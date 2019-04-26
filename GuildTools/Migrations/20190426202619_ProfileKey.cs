using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GuildTools.Migrations
{
    public partial class ProfileKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("User_GuildProfilePermissions", "dbo");
            migrationBuilder.DropTable("GuildProfile", "dbo");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
