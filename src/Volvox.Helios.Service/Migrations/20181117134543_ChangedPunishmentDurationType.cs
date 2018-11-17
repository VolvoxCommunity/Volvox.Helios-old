using Microsoft.EntityFrameworkCore.Migrations;

namespace Volvox.Helios.Service.Migrations
{
    public partial class ChangedPunishmentDurationType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "PunishDuration",
                table: "mod_punishments",
                nullable: true,
                oldClrType: typeof(short),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<short>(
                name: "PunishDuration",
                table: "mod_punishments",
                nullable: true,
                oldClrType: typeof(int),
                oldNullable: true);
        }
    }
}
