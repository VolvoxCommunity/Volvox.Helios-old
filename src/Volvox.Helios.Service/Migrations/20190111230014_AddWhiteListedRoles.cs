using Microsoft.EntityFrameworkCore.Migrations;

namespace Volvox.Helios.Service.Migrations
{
    public partial class AddWhiteListedRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WhiteListedRole",
                columns: table => new
                {
                    Id = table.Column<decimal>(nullable: false),
                    StreamerSettingsGuildId = table.Column<decimal>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhiteListedRole", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WhiteListedRole_StreamerSettings_StreamerSettingsGuildId",
                        column: x => x.StreamerSettingsGuildId,
                        principalTable: "StreamerSettings",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WhiteListedRole_StreamerSettingsGuildId",
                table: "WhiteListedRole",
                column: "StreamerSettingsGuildId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WhiteListedRole");
        }
    }
}
