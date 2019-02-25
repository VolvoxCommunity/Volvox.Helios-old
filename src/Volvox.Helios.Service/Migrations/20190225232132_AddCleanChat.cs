using Microsoft.EntityFrameworkCore.Migrations;

namespace Volvox.Helios.Service.Migrations
{
    public partial class AddCleanChat : Migration
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
                    GuildId = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CleanChatChannel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CleanChatChannel_CleanChatSettings_GuildId",
                        column: x => x.GuildId,
                        principalTable: "CleanChatSettings",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CleanChatChannel_GuildId",
                table: "CleanChatChannel",
                column: "GuildId");
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
