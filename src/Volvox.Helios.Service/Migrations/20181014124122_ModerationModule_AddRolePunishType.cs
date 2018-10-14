using Microsoft.EntityFrameworkCore.Migrations;

namespace Volvox.Helios.Service.Migrations
{
    public partial class ModerationModule_AddRolePunishType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<short>(
                name: "PunishDuration",
                table: "mod_punishments",
                nullable: true,
                oldClrType: typeof(short));

            migrationBuilder.AddColumn<decimal>(
                name: "RoleId",
                table: "mod_punishments",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "mod_punishments");

            migrationBuilder.AlterColumn<short>(
                name: "PunishDuration",
                table: "mod_punishments",
                nullable: false,
                oldClrType: typeof(short),
                oldNullable: true);
        }
    }
}
