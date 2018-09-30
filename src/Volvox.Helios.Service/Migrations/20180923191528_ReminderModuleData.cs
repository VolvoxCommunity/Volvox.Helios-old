using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Volvox.Helios.Service.Migrations
{
    public partial class ReminderModuleData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReminderSettings",
                columns: table => new
                {
                    GuildId = table.Column<decimal>(nullable: false),
                    Enabled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReminderSettings", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "RecurringReminderMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    GuildId = table.Column<decimal>(nullable: false),
                    Enabled = table.Column<bool>(nullable: false),
                    Message = table.Column<string>(nullable: false),
                    ChannelId = table.Column<decimal>(nullable: false),
                    MinutesExpression = table.Column<string>(nullable: false),
                    HoursExpression = table.Column<string>(nullable: false),
                    DayOfMonthExpression = table.Column<string>(nullable: false),
                    MonthExpression = table.Column<string>(nullable: false),
                    DayOfWeekExpression = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecurringReminderMessages", x => x.Id)
                        .Annotation("SqlServer:Clustered", true);
                    table.ForeignKey(
                        name: "FK_RecurringReminderMessages_ReminderSettings_GuildId",
                        column: x => x.GuildId,
                        principalTable: "ReminderSettings",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecurringReminderMessages_GuildId",
                table: "RecurringReminderMessages",
                column: "GuildId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecurringReminderMessages");

            migrationBuilder.DropTable(
                name: "ReminderSettings");
        }
    }
}
