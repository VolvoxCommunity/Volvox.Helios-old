using Microsoft.EntityFrameworkCore.Migrations;

namespace Volvox.Helios.Service.Migrations
{
    public partial class DbSetWhiteListedRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WhiteListedRole_StreamerSettings_StreamerSettingsGuildId",
                table: "WhiteListedRole");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WhiteListedRole",
                table: "WhiteListedRole");

            migrationBuilder.RenameTable(
                name: "WhiteListedRole",
                newName: "WhiteListedRoles");

            migrationBuilder.RenameIndex(
                name: "IX_WhiteListedRole_StreamerSettingsGuildId",
                table: "WhiteListedRoles",
                newName: "IX_WhiteListedRoles_StreamerSettingsGuildId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WhiteListedRoles",
                table: "WhiteListedRoles",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_WhiteListedRoles_StreamerSettings_StreamerSettingsGuildId",
                table: "WhiteListedRoles",
                column: "StreamerSettingsGuildId",
                principalTable: "StreamerSettings",
                principalColumn: "GuildId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WhiteListedRoles_StreamerSettings_StreamerSettingsGuildId",
                table: "WhiteListedRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WhiteListedRoles",
                table: "WhiteListedRoles");

            migrationBuilder.RenameTable(
                name: "WhiteListedRoles",
                newName: "WhiteListedRole");

            migrationBuilder.RenameIndex(
                name: "IX_WhiteListedRoles_StreamerSettingsGuildId",
                table: "WhiteListedRole",
                newName: "IX_WhiteListedRole_StreamerSettingsGuildId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WhiteListedRole",
                table: "WhiteListedRole",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_WhiteListedRole_StreamerSettings_StreamerSettingsGuildId",
                table: "WhiteListedRole",
                column: "StreamerSettingsGuildId",
                principalTable: "StreamerSettings",
                principalColumn: "GuildId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
