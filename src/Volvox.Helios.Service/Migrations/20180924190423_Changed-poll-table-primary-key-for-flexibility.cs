using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Volvox.Helios.Service.Migrations
{
    public partial class Changedpolltableprimarykeyforflexibility : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Poll",
                table: "Poll");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Poll",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Poll",
                table: "Poll",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Poll",
                table: "Poll");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Poll");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Poll",
                table: "Poll",
                column: "MessageId");
        }
    }
}
