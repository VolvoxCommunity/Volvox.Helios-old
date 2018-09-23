using Microsoft.EntityFrameworkCore.Migrations;

namespace Volvox.Helios.Service.Migrations
{
    public partial class RecurringReminderMessageAddReadableCron : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "JobId",
                table: "RecurringReminderMessages",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CronExpression",
                table: "RecurringReminderMessages",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReadableCronExpression",
                table: "RecurringReminderMessages",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReadableCronExpression",
                table: "RecurringReminderMessages");

            migrationBuilder.AlterColumn<string>(
                name: "JobId",
                table: "RecurringReminderMessages",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "CronExpression",
                table: "RecurringReminderMessages",
                nullable: true,
                oldClrType: typeof(string));
        }
    }
}
