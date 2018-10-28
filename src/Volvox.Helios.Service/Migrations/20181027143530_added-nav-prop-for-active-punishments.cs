using Microsoft.EntityFrameworkCore.Migrations;

namespace Volvox.Helios.Service.Migrations
{
    public partial class addednavpropforactivepunishments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_mod_ActivePunishments_mod_ModerationSettings_GuildId",
                table: "mod_ActivePunishments");

            migrationBuilder.DropIndex(
                name: "IX_mod_ActivePunishments_GuildId",
                table: "mod_ActivePunishments");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "mod_ActivePunishments");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "mod_ActivePunishments",
                nullable: false,
                oldClrType: typeof(decimal));

            migrationBuilder.AddColumn<decimal>(
                name: "ModerationSettingsGuildId",
                table: "mod_ActivePunishments",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_mod_ActivePunishments_ModerationSettingsGuildId",
                table: "mod_ActivePunishments",
                column: "ModerationSettingsGuildId");

            migrationBuilder.CreateIndex(
                name: "IX_mod_ActivePunishments_UserId",
                table: "mod_ActivePunishments",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_mod_ActivePunishments_mod_ModerationSettings_ModerationSettingsGuildId",
                table: "mod_ActivePunishments",
                column: "ModerationSettingsGuildId",
                principalTable: "mod_ModerationSettings",
                principalColumn: "GuildId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_mod_ActivePunishments_mod_users_UserId",
                table: "mod_ActivePunishments",
                column: "UserId",
                principalTable: "mod_users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_mod_ActivePunishments_mod_ModerationSettings_ModerationSettingsGuildId",
                table: "mod_ActivePunishments");

            migrationBuilder.DropForeignKey(
                name: "FK_mod_ActivePunishments_mod_users_UserId",
                table: "mod_ActivePunishments");

            migrationBuilder.DropIndex(
                name: "IX_mod_ActivePunishments_ModerationSettingsGuildId",
                table: "mod_ActivePunishments");

            migrationBuilder.DropIndex(
                name: "IX_mod_ActivePunishments_UserId",
                table: "mod_ActivePunishments");

            migrationBuilder.DropColumn(
                name: "ModerationSettingsGuildId",
                table: "mod_ActivePunishments");

            migrationBuilder.AlterColumn<decimal>(
                name: "UserId",
                table: "mod_ActivePunishments",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<decimal>(
                name: "GuildId",
                table: "mod_ActivePunishments",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_mod_ActivePunishments_GuildId",
                table: "mod_ActivePunishments",
                column: "GuildId");

            migrationBuilder.AddForeignKey(
                name: "FK_mod_ActivePunishments_mod_ModerationSettings_GuildId",
                table: "mod_ActivePunishments",
                column: "GuildId",
                principalTable: "mod_ModerationSettings",
                principalColumn: "GuildId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
