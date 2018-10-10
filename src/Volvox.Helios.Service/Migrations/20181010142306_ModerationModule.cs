using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Volvox.Helios.Service.Migrations
{
    public partial class ModerationModule : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mod_settings",
                columns: table => new
                {
                    GuildId = table.Column<decimal>(nullable: false),
                    Enabled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mod_settings", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "mod_link_filters",
                columns: table => new
                {
                    Enabled = table.Column<bool>(nullable: false),
                    GuildId = table.Column<decimal>(nullable: false),
                    WarningExpirePeriod = table.Column<short>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mod_link_filters", x => x.GuildId);
                    table.ForeignKey(
                        name: "FK_mod_link_filters_mod_settings_GuildId",
                        column: x => x.GuildId,
                        principalTable: "mod_settings",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mod_profanity_filters",
                columns: table => new
                {
                    Enabled = table.Column<bool>(nullable: false),
                    GuildId = table.Column<decimal>(nullable: false),
                    WarningExpirePeriod = table.Column<short>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mod_profanity_filters", x => x.GuildId);
                    table.ForeignKey(
                        name: "FK_mod_profanity_filters_mod_settings_GuildId",
                        column: x => x.GuildId,
                        principalTable: "mod_settings",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mod_punishments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    GuildId = table.Column<decimal>(nullable: false),
                    WarningThreshold = table.Column<short>(nullable: false),
                    PunishType = table.Column<int>(nullable: false),
                    WarningType = table.Column<int>(nullable: false),
                    PunishDuration = table.Column<short>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mod_punishments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_mod_punishments_mod_settings_GuildId",
                        column: x => x.GuildId,
                        principalTable: "mod_settings",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mod_users",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<decimal>(nullable: false),
                    GuildId = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mod_users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_mod_users_mod_settings_GuildId",
                        column: x => x.GuildId,
                        principalTable: "mod_settings",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mod_whitelisted_channels",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ChannelId = table.Column<decimal>(nullable: false),
                    GuildId = table.Column<decimal>(nullable: false),
                    WhitelistType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mod_whitelisted_channels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_mod_whitelisted_channels_mod_settings_GuildId",
                        column: x => x.GuildId,
                        principalTable: "mod_settings",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mod_whitelisted_roles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    RoleId = table.Column<decimal>(nullable: false),
                    GuildId = table.Column<decimal>(nullable: false),
                    WhitelistType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mod_whitelisted_roles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_mod_whitelisted_roles_mod_settings_GuildId",
                        column: x => x.GuildId,
                        principalTable: "mod_settings",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mod_whitelisted_links",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    GuildId = table.Column<decimal>(nullable: false),
                    ChannelId = table.Column<decimal>(nullable: true),
                    Link = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mod_whitelisted_links", x => x.Id);
                    table.ForeignKey(
                        name: "FK_mod_whitelisted_links_mod_link_filters_GuildId",
                        column: x => x.GuildId,
                        principalTable: "mod_link_filters",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mod_banned_words",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    GuildId = table.Column<decimal>(nullable: false),
                    Word = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mod_banned_words", x => x.Id);
                    table.ForeignKey(
                        name: "FK_mod_banned_words_mod_profanity_filters_GuildId",
                        column: x => x.GuildId,
                        principalTable: "mod_profanity_filters",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mod_warnings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(nullable: true),
                    WarningType = table.Column<int>(nullable: false),
                    WarningRecieved = table.Column<DateTimeOffset>(nullable: false),
                    WarningExpires = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mod_warnings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_mod_warnings_mod_users_UserId",
                        column: x => x.UserId,
                        principalTable: "mod_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_mod_banned_words_GuildId",
                table: "mod_banned_words",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_mod_punishments_GuildId",
                table: "mod_punishments",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_mod_users_GuildId",
                table: "mod_users",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_mod_warnings_UserId",
                table: "mod_warnings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_mod_whitelisted_channels_GuildId",
                table: "mod_whitelisted_channels",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_mod_whitelisted_links_GuildId",
                table: "mod_whitelisted_links",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_mod_whitelisted_roles_GuildId",
                table: "mod_whitelisted_roles",
                column: "GuildId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mod_banned_words");

            migrationBuilder.DropTable(
                name: "mod_punishments");

            migrationBuilder.DropTable(
                name: "mod_warnings");

            migrationBuilder.DropTable(
                name: "mod_whitelisted_channels");

            migrationBuilder.DropTable(
                name: "mod_whitelisted_links");

            migrationBuilder.DropTable(
                name: "mod_whitelisted_roles");

            migrationBuilder.DropTable(
                name: "mod_profanity_filters");

            migrationBuilder.DropTable(
                name: "mod_users");

            migrationBuilder.DropTable(
                name: "mod_link_filters");

            migrationBuilder.DropTable(
                name: "mod_settings");
        }
    }
}
