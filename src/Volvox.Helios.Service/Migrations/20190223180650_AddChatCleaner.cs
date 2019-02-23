using Microsoft.EntityFrameworkCore.Migrations;

namespace Volvox.Helios.Service.Migrations
{
    public partial class AddChatCleaner : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CleanChatSettings",
                columns: table => new
                {
                    GuildId = table.Column<decimal>(nullable: false),
                    Enabled = table.Column<bool>(nullable: false),
                    MessageDuration = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CleanChatSettings", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "CleanChatChannel",
                columns: table => new
                {
                    Id = table.Column<decimal>(nullable: false),
                    CleanChatSettingsGuildId = table.Column<decimal>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CleanChatChannel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CleanChatChannel_CleanChatSettings_CleanChatSettingsGuildId",
                        column: x => x.CleanChatSettingsGuildId,
                        principalTable: "CleanChatSettings",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CleanChatChannel_CleanChatSettingsGuildId",
                table: "CleanChatChannel",
                column: "CleanChatSettingsGuildId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CleanChatChannel");

            migrationBuilder.DropTable(
                name: "CleanChatSettings");
        }
    }
}
