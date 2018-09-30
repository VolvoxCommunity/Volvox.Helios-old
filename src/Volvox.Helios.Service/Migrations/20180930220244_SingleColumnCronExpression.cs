using Microsoft.EntityFrameworkCore.Migrations;

namespace Volvox.Helios.Service.Migrations
{
    public partial class SingleColumnCronExpression : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DayOfMonthExpression",
                table: "RecurringReminderMessages");

            migrationBuilder.DropColumn(
                name: "DayOfWeekExpression",
                table: "RecurringReminderMessages");

            migrationBuilder.DropColumn(
                name: "HoursExpression",
                table: "RecurringReminderMessages");

            migrationBuilder.DropColumn(
                name: "MinutesExpression",
                table: "RecurringReminderMessages");

            migrationBuilder.DropColumn(
                name: "MonthExpression",
                table: "RecurringReminderMessages");

            migrationBuilder.AddColumn<string>(
                name: "CronExpression",
                table: "RecurringReminderMessages",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Fault",
                table: "RecurringReminderMessages",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CronExpression",
                table: "RecurringReminderMessages");

            migrationBuilder.DropColumn(
                name: "Fault",
                table: "RecurringReminderMessages");

            migrationBuilder.AddColumn<string>(
                name: "DayOfMonthExpression",
                table: "RecurringReminderMessages",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DayOfWeekExpression",
                table: "RecurringReminderMessages",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HoursExpression",
                table: "RecurringReminderMessages",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MinutesExpression",
                table: "RecurringReminderMessages",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MonthExpression",
                table: "RecurringReminderMessages",
                nullable: false,
                defaultValue: "");
        }
    }
}
