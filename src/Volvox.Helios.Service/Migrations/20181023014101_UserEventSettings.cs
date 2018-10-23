using Microsoft.EntityFrameworkCore.Migrations;

namespace Volvox.Helios.Service.Migrations
{
    public partial class UserEventSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserEventSettings",
                columns: table => new
                {
                    GuildId = table.Column<decimal>(nullable: false),
                    Enabled = table.Column<bool>(nullable: false),
                    EnableUserLeftEvent = table.Column<bool>(nullable: false),
                    UserLeftEventChannelId = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserEventSettings", x => x.GuildId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserEventSettings");
        }
    }
}
