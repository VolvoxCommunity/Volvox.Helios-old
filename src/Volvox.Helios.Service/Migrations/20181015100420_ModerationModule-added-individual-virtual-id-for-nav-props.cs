using Microsoft.EntityFrameworkCore.Migrations;

namespace Volvox.Helios.Service.Migrations
{
    public partial class ModerationModuleaddedindividualvirtualidfornavprops : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_mod_warnings_mod_users_UserId",
                table: "mod_warnings");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "mod_warnings",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_mod_warnings_mod_users_UserId",
                table: "mod_warnings",
                column: "UserId",
                principalTable: "mod_users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_mod_warnings_mod_users_UserId",
                table: "mod_warnings");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "mod_warnings",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_mod_warnings_mod_users_UserId",
                table: "mod_warnings",
                column: "UserId",
                principalTable: "mod_users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
