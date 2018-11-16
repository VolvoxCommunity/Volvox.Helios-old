using Microsoft.EntityFrameworkCore.Migrations;

namespace Volvox.Helios.Service.Migrations
{
    public partial class ChangeTypeExpirePeriodIntFromShort : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "WarningExpirePeriod",
                table: "mod_profanity_filters",
                nullable: false,
                oldClrType: typeof(short));

            migrationBuilder.AlterColumn<int>(
                name: "WarningExpirePeriod",
                table: "mod_link_filters",
                nullable: false,
                oldClrType: typeof(short));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<short>(
                name: "WarningExpirePeriod",
                table: "mod_profanity_filters",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<short>(
                name: "WarningExpirePeriod",
                table: "mod_link_filters",
                nullable: false,
                oldClrType: typeof(int));
        }
    }
}
