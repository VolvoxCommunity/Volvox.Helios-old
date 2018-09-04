using Microsoft.EntityFrameworkCore.Migrations;

namespace Volvox.Helios.Service.Migrations
{
    public partial class Announcerindividualchannelsettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnnouncementChannelId",
                table: "StreamAnnouncerSettings");

            migrationBuilder.DropColumn(
                name: "RemoveMessages",
                table: "StreamAnnouncerSettings");

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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StreamAnnouncerChannelSettings");

            migrationBuilder.AddColumn<decimal>(
                name: "AnnouncementChannelId",
                table: "StreamAnnouncerSettings",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "RemoveMessages",
                table: "StreamAnnouncerSettings",
                nullable: false,
                defaultValue: false);
        }
    }
}
