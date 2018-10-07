using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Volvox.Helios.Service.Migrations
{
    public partial class Persiststreammessages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StreamAnnouncerMessages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<decimal>(nullable: false),
                    MessageId = table.Column<decimal>(nullable: false),
                    ChannelId = table.Column<decimal>(nullable: false),
                    StreamAnnouncerSettingsGuildId = table.Column<decimal>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreamAnnouncerMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StreamAnnouncerMessages_StreamAnnouncerSettings_StreamAnnouncerSettingsGuildId",
                        column: x => x.StreamAnnouncerSettingsGuildId,
                        principalTable: "StreamAnnouncerSettings",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StreamAnnouncerMessages_StreamAnnouncerSettingsGuildId",
                table: "StreamAnnouncerMessages",
                column: "StreamAnnouncerSettingsGuildId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StreamAnnouncerMessages");
        }
    }
}
