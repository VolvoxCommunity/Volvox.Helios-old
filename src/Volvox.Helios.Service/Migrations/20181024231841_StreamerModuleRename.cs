using Microsoft.EntityFrameworkCore.Migrations;

namespace Volvox.Helios.Service.Migrations
{
    public partial class StreamerModuleRename : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StreamAnnouncerMessages_StreamAnnouncerSettings_GuildId",
                table: "StreamAnnouncerMessages");

            migrationBuilder.DropTable(
                name: "StreamAnnouncerChannelSettings");

            migrationBuilder.DropTable(
                name: "StreamerRoleSettings");

            migrationBuilder.DropTable(
                name: "StreamAnnouncerSettings");

            migrationBuilder.CreateTable(
                name: "StreamerSettings",
                columns: table => new
                {
                    GuildId = table.Column<decimal>(nullable: false),
                    Enabled = table.Column<bool>(nullable: false),
                    RoleId = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreamerSettings", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "StreamerChannelSettings",
                columns: table => new
                {
                    ChannelId = table.Column<decimal>(nullable: false),
                    GuildId = table.Column<decimal>(nullable: false),
                    RemoveMessage = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreamerChannelSettings", x => x.ChannelId);
                    table.ForeignKey(
                        name: "FK_StreamerChannelSettings_StreamerSettings_GuildId",
                        column: x => x.GuildId,
                        principalTable: "StreamerSettings",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StreamerChannelSettings_GuildId",
                table: "StreamerChannelSettings",
                column: "GuildId");

            migrationBuilder.AddForeignKey(
                name: "FK_StreamAnnouncerMessages_StreamerSettings_GuildId",
                table: "StreamAnnouncerMessages",
                column: "GuildId",
                principalTable: "StreamerSettings",
                principalColumn: "GuildId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StreamAnnouncerMessages_StreamerSettings_GuildId",
                table: "StreamAnnouncerMessages");

            migrationBuilder.DropTable(
                name: "StreamerChannelSettings");

            migrationBuilder.DropTable(
                name: "StreamerSettings");

            migrationBuilder.CreateTable(
                name: "StreamAnnouncerSettings",
                columns: table => new
                {
                    GuildId = table.Column<decimal>(nullable: false),
                    Enabled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreamAnnouncerSettings", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "StreamerRoleSettings",
                columns: table => new
                {
                    GuildId = table.Column<decimal>(nullable: false),
                    Enabled = table.Column<bool>(nullable: false),
                    RoleId = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreamerRoleSettings", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "StreamAnnouncerChannelSettings",
                columns: table => new
                {
                    ChannelId = table.Column<decimal>(nullable: false),
                    GuildId = table.Column<decimal>(nullable: false),
                    RemoveMessage = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreamAnnouncerChannelSettings", x => x.ChannelId);
                    table.ForeignKey(
                        name: "FK_StreamAnnouncerChannelSettings_StreamAnnouncerSettings_GuildId",
                        column: x => x.GuildId,
                        principalTable: "StreamAnnouncerSettings",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StreamAnnouncerChannelSettings_GuildId",
                table: "StreamAnnouncerChannelSettings",
                column: "GuildId");

            migrationBuilder.AddForeignKey(
                name: "FK_StreamAnnouncerMessages_StreamAnnouncerSettings_GuildId",
                table: "StreamAnnouncerMessages",
                column: "GuildId",
                principalTable: "StreamAnnouncerSettings",
                principalColumn: "GuildId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
