using Microsoft.EntityFrameworkCore.Migrations;

namespace Volvox.Helios.Service.Migrations
{
    public partial class FixWhiteListedRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_WhiteListedRole",
                table: "WhiteListedRole");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "WhiteListedRole");

            migrationBuilder.AddColumn<decimal>(
                name: "RoleId",
                table: "WhiteListedRole",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "PK_WhiteListedRole",
                table: "WhiteListedRole",
                column: "RoleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_WhiteListedRole",
                table: "WhiteListedRole");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "WhiteListedRole");

            migrationBuilder.AddColumn<decimal>(
                name: "Id",
                table: "WhiteListedRole",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "PK_WhiteListedRole",
                table: "WhiteListedRole",
                column: "Id");
        }
    }
}
