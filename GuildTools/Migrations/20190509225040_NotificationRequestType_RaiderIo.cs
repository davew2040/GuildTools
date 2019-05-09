using Microsoft.EntityFrameworkCore.Migrations;

namespace GuildTools.Migrations
{
    public partial class NotificationRequestType_RaiderIo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "NotificationRequestTypes",
                columns: new[] { "Id", "RequestTypeName" },
                values: new object[] { 2, "RaiderIoStatsRequestComplete" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "NotificationRequestTypes",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
