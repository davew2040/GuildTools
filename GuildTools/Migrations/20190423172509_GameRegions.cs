using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GuildTools.Migrations
{
    public partial class GameRegions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameRegions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    RegionName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameRegions", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "GameRegions",
                columns: new[] { "Id", "RegionName" },
                values: new object[] { 1, "US" });

            migrationBuilder.InsertData(
                table: "GameRegions",
                columns: new[] { "Id", "RegionName" },
                values: new object[] { 2, "EU" });

            migrationBuilder.AddColumn<int>(
                name: "RegionId",
                table: "GuildProfile",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_GuildProfile_RegionId",
                table: "GuildProfile",
                column: "RegionId");

            migrationBuilder.AddForeignKey(
                name: "FK_GuildProfile_GameRegion",
                table: "GuildProfile",
                column: "RegionId",
                principalTable: "GameRegions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuildProfile_GameRegion",
                table: "GuildProfile");

            migrationBuilder.DropTable(
                name: "GameRegions");

            migrationBuilder.DropIndex(
                name: "IX_GuildProfile_RegionId",
                table: "GuildProfile");

            migrationBuilder.DropColumn(
                name: "RegionId",
                table: "GuildProfile");
        }
    }
}
