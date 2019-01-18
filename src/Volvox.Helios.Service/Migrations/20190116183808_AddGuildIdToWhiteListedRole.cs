using Microsoft.EntityFrameworkCore.Migrations;

namespace Volvox.Helios.Service.Migrations
{
    public partial class AddGuildIdToWhiteListedRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WhiteListedRoles_StreamerSettings_StreamerSettingsGuildId",
                table: "WhiteListedRoles");

            migrationBuilder.DropIndex(
                name: "IX_WhiteListedRoles_StreamerSettingsGuildId",
                table: "WhiteListedRoles");

            migrationBuilder.DropColumn(
                name: "StreamerSettingsGuildId",
                table: "WhiteListedRoles");

            migrationBuilder.AddColumn<decimal>(
                name: "GuildId",
                table: "WhiteListedRoles",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_WhiteListedRoles_GuildId",
                table: "WhiteListedRoles",
                column: "GuildId");

            migrationBuilder.AddForeignKey(
                name: "FK_WhiteListedRoles_StreamerSettings_GuildId",
                table: "WhiteListedRoles",
                column: "GuildId",
                principalTable: "StreamerSettings",
                principalColumn: "GuildId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WhiteListedRoles_StreamerSettings_GuildId",
                table: "WhiteListedRoles");

            migrationBuilder.DropIndex(
                name: "IX_WhiteListedRoles_GuildId",
                table: "WhiteListedRoles");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "WhiteListedRoles");

            migrationBuilder.AddColumn<decimal>(
                name: "StreamerSettingsGuildId",
                table: "WhiteListedRoles",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WhiteListedRoles_StreamerSettingsGuildId",
                table: "WhiteListedRoles",
                column: "StreamerSettingsGuildId");

            migrationBuilder.AddForeignKey(
                name: "FK_WhiteListedRoles_StreamerSettings_StreamerSettingsGuildId",
                table: "WhiteListedRoles",
                column: "StreamerSettingsGuildId",
                principalTable: "StreamerSettings",
                principalColumn: "GuildId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
