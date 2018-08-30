using Microsoft.EntityFrameworkCore.Migrations;

namespace Volvox.Helios.Service.Migrations
{
    public partial class Ammendedpercodereview : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ShouldRemoveMessagesOnStreamConclusion",
                table: "StreamAnnouncerSettings",
                newName: "RemoveMessages");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RemoveMessages",
                table: "StreamAnnouncerSettings",
                newName: "ShouldRemoveMessagesOnStreamConclusion");
        }
    }
}
