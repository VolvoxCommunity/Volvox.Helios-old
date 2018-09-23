using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Volvox.Helios.Service.Migrations
{
    public partial class RecurringReminderMessageUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "RecurringReminderMessages");

            migrationBuilder.DropColumn(
                name: "PostDate",
                table: "RecurringReminderMessages");

            migrationBuilder.AddColumn<string>(
                name: "CronExpression",
                table: "RecurringReminderMessages",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JobId",
                table: "RecurringReminderMessages",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CronExpression",
                table: "RecurringReminderMessages");

            migrationBuilder.DropColumn(
                name: "JobId",
                table: "RecurringReminderMessages");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "RecurringReminderMessages",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PostDate",
                table: "RecurringReminderMessages",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }
    }
}
