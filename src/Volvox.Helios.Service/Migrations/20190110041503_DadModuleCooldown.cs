using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Volvox.Helios.Service.Migrations
{
    public partial class DadModuleCooldown : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DadResponseCooldownMinutes",
                table: "DadModuleSettings",
                nullable: false,
                defaultValue: 15);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastDadResponseUtc",
                table: "DadModuleSettings",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DadResponseCooldownMinutes",
                table: "DadModuleSettings");

            migrationBuilder.DropColumn(
                name: "LastDadResponseUtc",
                table: "DadModuleSettings");
        }
    }
}
