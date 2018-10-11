using Microsoft.EntityFrameworkCore.Migrations;

namespace Volvox.Helios.Service.Migrations
{
    public partial class ModerationModule_AddedDefaultBannedWordOption : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_mod_link_filters_mod_settings_GuildId",
                table: "mod_link_filters");

            migrationBuilder.DropForeignKey(
                name: "FK_mod_profanity_filters_mod_settings_GuildId",
                table: "mod_profanity_filters");

            migrationBuilder.DropForeignKey(
                name: "FK_mod_punishments_mod_settings_GuildId",
                table: "mod_punishments");

            migrationBuilder.DropForeignKey(
                name: "FK_mod_users_mod_settings_GuildId",
                table: "mod_users");

            migrationBuilder.DropForeignKey(
                name: "FK_mod_whitelisted_channels_mod_settings_GuildId",
                table: "mod_whitelisted_channels");

            migrationBuilder.DropForeignKey(
                name: "FK_mod_whitelisted_roles_mod_settings_GuildId",
                table: "mod_whitelisted_roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_mod_settings",
                table: "mod_settings");

            migrationBuilder.RenameTable(
                name: "mod_settings",
                newName: "mod_ModerationSettings");

            migrationBuilder.AddColumn<bool>(
                name: "UseDefaultList",
                table: "mod_profanity_filters",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Word",
                table: "mod_banned_words",
                maxLength: 26,
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AddPrimaryKey(
                name: "PK_mod_ModerationSettings",
                table: "mod_ModerationSettings",
                column: "GuildId");

            migrationBuilder.AddForeignKey(
                name: "FK_mod_link_filters_mod_ModerationSettings_GuildId",
                table: "mod_link_filters",
                column: "GuildId",
                principalTable: "mod_ModerationSettings",
                principalColumn: "GuildId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_mod_profanity_filters_mod_ModerationSettings_GuildId",
                table: "mod_profanity_filters",
                column: "GuildId",
                principalTable: "mod_ModerationSettings",
                principalColumn: "GuildId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_mod_punishments_mod_ModerationSettings_GuildId",
                table: "mod_punishments",
                column: "GuildId",
                principalTable: "mod_ModerationSettings",
                principalColumn: "GuildId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_mod_users_mod_ModerationSettings_GuildId",
                table: "mod_users",
                column: "GuildId",
                principalTable: "mod_ModerationSettings",
                principalColumn: "GuildId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_mod_whitelisted_channels_mod_ModerationSettings_GuildId",
                table: "mod_whitelisted_channels",
                column: "GuildId",
                principalTable: "mod_ModerationSettings",
                principalColumn: "GuildId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_mod_whitelisted_roles_mod_ModerationSettings_GuildId",
                table: "mod_whitelisted_roles",
                column: "GuildId",
                principalTable: "mod_ModerationSettings",
                principalColumn: "GuildId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_mod_link_filters_mod_ModerationSettings_GuildId",
                table: "mod_link_filters");

            migrationBuilder.DropForeignKey(
                name: "FK_mod_profanity_filters_mod_ModerationSettings_GuildId",
                table: "mod_profanity_filters");

            migrationBuilder.DropForeignKey(
                name: "FK_mod_punishments_mod_ModerationSettings_GuildId",
                table: "mod_punishments");

            migrationBuilder.DropForeignKey(
                name: "FK_mod_users_mod_ModerationSettings_GuildId",
                table: "mod_users");

            migrationBuilder.DropForeignKey(
                name: "FK_mod_whitelisted_channels_mod_ModerationSettings_GuildId",
                table: "mod_whitelisted_channels");

            migrationBuilder.DropForeignKey(
                name: "FK_mod_whitelisted_roles_mod_ModerationSettings_GuildId",
                table: "mod_whitelisted_roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_mod_ModerationSettings",
                table: "mod_ModerationSettings");

            migrationBuilder.DropColumn(
                name: "UseDefaultList",
                table: "mod_profanity_filters");

            migrationBuilder.RenameTable(
                name: "mod_ModerationSettings",
                newName: "mod_settings");

            migrationBuilder.AlterColumn<string>(
                name: "Word",
                table: "mod_banned_words",
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 26);

            migrationBuilder.AddPrimaryKey(
                name: "PK_mod_settings",
                table: "mod_settings",
                column: "GuildId");

            migrationBuilder.AddForeignKey(
                name: "FK_mod_link_filters_mod_settings_GuildId",
                table: "mod_link_filters",
                column: "GuildId",
                principalTable: "mod_settings",
                principalColumn: "GuildId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_mod_profanity_filters_mod_settings_GuildId",
                table: "mod_profanity_filters",
                column: "GuildId",
                principalTable: "mod_settings",
                principalColumn: "GuildId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_mod_punishments_mod_settings_GuildId",
                table: "mod_punishments",
                column: "GuildId",
                principalTable: "mod_settings",
                principalColumn: "GuildId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_mod_users_mod_settings_GuildId",
                table: "mod_users",
                column: "GuildId",
                principalTable: "mod_settings",
                principalColumn: "GuildId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_mod_whitelisted_channels_mod_settings_GuildId",
                table: "mod_whitelisted_channels",
                column: "GuildId",
                principalTable: "mod_settings",
                principalColumn: "GuildId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_mod_whitelisted_roles_mod_settings_GuildId",
                table: "mod_whitelisted_roles",
                column: "GuildId",
                principalTable: "mod_settings",
                principalColumn: "GuildId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
