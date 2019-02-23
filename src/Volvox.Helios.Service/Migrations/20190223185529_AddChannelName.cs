using Microsoft.EntityFrameworkCore.Migrations;

namespace Volvox.Helios.Service.Migrations
{
    public partial class AddChannelName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "CleanChatChannel",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "CleanChatChannel");
        }
    }
}
