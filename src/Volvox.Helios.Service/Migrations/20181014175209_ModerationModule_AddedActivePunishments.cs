using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Volvox.Helios.Service.Migrations
{
    public partial class ModerationModule_AddedActivePunishments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mod_ActivePunishments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    GuildId = table.Column<decimal>(nullable: false),
                    UserId = table.Column<decimal>(nullable: false),
                    RoleId = table.Column<decimal>(nullable: true),
                    PunishType = table.Column<int>(nullable: false),
                    PunishmentExpires = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mod_ActivePunishments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_mod_ActivePunishments_mod_ModerationSettings_GuildId",
                        column: x => x.GuildId,
                        principalTable: "mod_ModerationSettings",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_mod_ActivePunishments_GuildId",
                table: "mod_ActivePunishments",
                column: "GuildId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mod_ActivePunishments");
        }
    }
}
