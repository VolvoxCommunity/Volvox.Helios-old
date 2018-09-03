using Microsoft.EntityFrameworkCore.Migrations;

namespace Volvox.Helios.Service.Migrations
{
    public partial class Addsettingforremovingstreamingmessagesonstreamconclusion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShouldRemoveMessagesOnStreamConclusion",
                table: "StreamAnnouncerSettings",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShouldRemoveMessagesOnStreamConclusion",
                table: "StreamAnnouncerSettings");
        }
    }
}
