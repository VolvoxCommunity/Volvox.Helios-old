using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Volvox.Helios.Service.Migrations
{
    public partial class AddGuildIdAsKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_StreamAnnouncerSettings",
                table: "StreamAnnouncerSettings");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "StreamAnnouncerSettings");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StreamAnnouncerSettings",
                table: "StreamAnnouncerSettings",
                column: "GuildId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_StreamAnnouncerSettings",
                table: "StreamAnnouncerSettings");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "StreamAnnouncerSettings",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_StreamAnnouncerSettings",
                table: "StreamAnnouncerSettings",
                column: "Id");
        }
    }
}
