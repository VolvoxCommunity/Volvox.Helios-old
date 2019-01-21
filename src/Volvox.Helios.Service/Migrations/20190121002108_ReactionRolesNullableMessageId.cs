using Microsoft.EntityFrameworkCore.Migrations;

namespace Volvox.Helios.Service.Migrations
{
    public partial class ReactionRolesNullableMessageId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "MessageId",
                table: "ReactionRoleMessages",
                nullable: true,
                oldClrType: typeof(decimal));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "MessageId",
                table: "ReactionRoleMessages",
                nullable: false,
                oldClrType: typeof(decimal),
                oldNullable: true);
        }
    }
}
