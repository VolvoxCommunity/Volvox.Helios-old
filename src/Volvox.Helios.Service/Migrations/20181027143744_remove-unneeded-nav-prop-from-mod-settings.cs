using Microsoft.EntityFrameworkCore.Migrations;

namespace Volvox.Helios.Service.Migrations
{
    public partial class removeunneedednavpropfrommodsettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_mod_ActivePunishments_mod_ModerationSettings_ModerationSettingsGuildId",
                table: "mod_ActivePunishments");

            migrationBuilder.DropIndex(
                name: "IX_mod_ActivePunishments_ModerationSettingsGuildId",
                table: "mod_ActivePunishments");

            migrationBuilder.DropColumn(
                name: "ModerationSettingsGuildId",
                table: "mod_ActivePunishments");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ModerationSettingsGuildId",
                table: "mod_ActivePunishments",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_mod_ActivePunishments_ModerationSettingsGuildId",
                table: "mod_ActivePunishments",
                column: "ModerationSettingsGuildId");

            migrationBuilder.AddForeignKey(
                name: "FK_mod_ActivePunishments_mod_ModerationSettings_ModerationSettingsGuildId",
                table: "mod_ActivePunishments",
                column: "ModerationSettingsGuildId",
                principalTable: "mod_ModerationSettings",
                principalColumn: "GuildId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
