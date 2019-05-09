using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GuildTools.Migrations
{
    public partial class NotificationRequests : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotificationRequestTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    RequestTypeName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationRequestTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationRequests",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Email = table.Column<string>(nullable: false),
                    OperationKey = table.Column<string>(nullable: true),
                    NotificationRequestTypeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationRequests_NotificationRequestTypes_NotificationRequestTypeId",
                        column: x => x.NotificationRequestTypeId,
                        principalTable: "NotificationRequestTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "NotificationRequestTypes",
                columns: new[] { "Id", "RequestTypeName" },
                values: new object[] { 1, "StatsRequestComplete" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRequests_Id",
                table: "NotificationRequests",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRequests_NotificationRequestTypeId",
                table: "NotificationRequests",
                column: "NotificationRequestTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRequests_OperationKey",
                table: "NotificationRequests",
                column: "OperationKey");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationRequests");

            migrationBuilder.DropTable(
                name: "NotificationRequestTypes");
        }
    }
}
