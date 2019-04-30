using Microsoft.EntityFrameworkCore.Migrations;

namespace GuildTools.Migrations
{
    public partial class Initial2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoredPlayers_PlayerAlts_Id",
                table: "StoredPlayers");

            migrationBuilder.DropIndex(
                name: "IX_StoredPlayers_Id",
                table: "StoredPlayers");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_PlayerAlts_PlayerId",
                table: "PlayerAlts");

            migrationBuilder.CreateIndex(
                name: "IX_StoredPlayers_Id",
                table: "StoredPlayers",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerAlts_PlayerId",
                table: "PlayerAlts",
                column: "PlayerId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerAlts_StoredPlayers_PlayerId",
                table: "PlayerAlts",
                column: "PlayerId",
                principalTable: "StoredPlayers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayerAlts_StoredPlayers_PlayerId",
                table: "PlayerAlts");

            migrationBuilder.DropIndex(
                name: "IX_StoredPlayers_Id",
                table: "StoredPlayers");

            migrationBuilder.DropIndex(
                name: "IX_PlayerAlts_PlayerId",
                table: "PlayerAlts");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_PlayerAlts_PlayerId",
                table: "PlayerAlts",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_StoredPlayers_Id",
                table: "StoredPlayers",
                column: "Id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StoredPlayers_PlayerAlts_Id",
                table: "StoredPlayers",
                column: "Id",
                principalTable: "PlayerAlts",
                principalColumn: "PlayerId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
