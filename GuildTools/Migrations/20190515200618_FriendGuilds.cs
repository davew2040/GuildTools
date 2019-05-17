using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GuildTools.Migrations
{
    public partial class FriendGuilds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FriendGuilds",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    StoredGuildId = table.Column<int>(nullable: false),
                    Abbreviation = table.Column<string>(nullable: true),
                    ProfileId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FriendGuilds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FriendGuilds_GuildProfile_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "GuildProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FriendGuilds_StoredGuilds_StoredGuildId",
                        column: x => x.StoredGuildId,
                        principalTable: "StoredGuilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FriendGuilds_Id",
                table: "FriendGuilds",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_FriendGuilds_ProfileId",
                table: "FriendGuilds",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_FriendGuilds_StoredGuildId",
                table: "FriendGuilds",
                column: "StoredGuildId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FriendGuilds");
        }
    }
}
