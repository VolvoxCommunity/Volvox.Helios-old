using Microsoft.EntityFrameworkCore.Migrations;

namespace Volvox.Helios.Service.Migrations
{
    public partial class Addedpollmoduletables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PollSettings",
                columns: table => new
                {
                    GuildId = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PollSettings", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "Poll",
                columns: table => new
                {
                    MessageId = table.Column<decimal>(nullable: false),
                    ChannelId = table.Column<decimal>(nullable: false),
                    GuildId = table.Column<decimal>(nullable: false),
                    PollTitle = table.Column<string>(maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Poll", x => x.MessageId);
                    table.ForeignKey(
                        name: "FK_Poll_PollSettings_GuildId",
                        column: x => x.GuildId,
                        principalTable: "PollSettings",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Poll_GuildId",
                table: "Poll",
                column: "GuildId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Poll");

            migrationBuilder.DropTable(
                name: "PollSettings");
        }
    }
}
