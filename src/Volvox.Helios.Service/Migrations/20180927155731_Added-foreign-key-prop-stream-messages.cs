using Microsoft.EntityFrameworkCore.Migrations;

namespace Volvox.Helios.Service.Migrations
{
    public partial class Addedforeignkeypropstreammessages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StreamAnnouncerMessages_StreamAnnouncerSettings_StreamAnnouncerSettingsGuildId",
                table: "StreamAnnouncerMessages");

            migrationBuilder.DropIndex(
                name: "IX_StreamAnnouncerMessages_StreamAnnouncerSettingsGuildId",
                table: "StreamAnnouncerMessages");

            migrationBuilder.DropColumn(
                name: "StreamAnnouncerSettingsGuildId",
                table: "StreamAnnouncerMessages");

            migrationBuilder.AddColumn<decimal>(
                name: "GuildId",
                table: "StreamAnnouncerMessages",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_StreamAnnouncerMessages_GuildId",
                table: "StreamAnnouncerMessages",
                column: "GuildId");

            migrationBuilder.AddForeignKey(
                name: "FK_StreamAnnouncerMessages_StreamAnnouncerSettings_GuildId",
                table: "StreamAnnouncerMessages",
                column: "GuildId",
                principalTable: "StreamAnnouncerSettings",
                principalColumn: "GuildId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StreamAnnouncerMessages_StreamAnnouncerSettings_GuildId",
                table: "StreamAnnouncerMessages");

            migrationBuilder.DropIndex(
                name: "IX_StreamAnnouncerMessages_GuildId",
                table: "StreamAnnouncerMessages");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "StreamAnnouncerMessages");

            migrationBuilder.AddColumn<decimal>(
                name: "StreamAnnouncerSettingsGuildId",
                table: "StreamAnnouncerMessages",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StreamAnnouncerMessages_StreamAnnouncerSettingsGuildId",
                table: "StreamAnnouncerMessages",
                column: "StreamAnnouncerSettingsGuildId");

            migrationBuilder.AddForeignKey(
                name: "FK_StreamAnnouncerMessages_StreamAnnouncerSettings_StreamAnnouncerSettingsGuildId",
                table: "StreamAnnouncerMessages",
                column: "StreamAnnouncerSettingsGuildId",
                principalTable: "StreamAnnouncerSettings",
                principalColumn: "GuildId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
