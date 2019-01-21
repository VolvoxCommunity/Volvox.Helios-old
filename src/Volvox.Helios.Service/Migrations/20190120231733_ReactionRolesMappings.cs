using Microsoft.EntityFrameworkCore.Migrations;

namespace Volvox.Helios.Service.Migrations
{
    public partial class ReactionRolesMappings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReactionRoleEmoteMappings_ReactionRoleMessages_ReactionRoleMessageId",
                table: "ReactionRoleEmoteMappings");

            migrationBuilder.AddColumn<long>(
                name: "ReactionRolesMessageId",
                table: "ReactionRoleEmoteMappings",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReactionRoleEmoteMappings_ReactionRolesMessageId",
                table: "ReactionRoleEmoteMappings",
                column: "ReactionRolesMessageId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReactionRoleEmoteMappings_ReactionRoleMessages_ReactionRolesMessageId",
                table: "ReactionRoleEmoteMappings",
                column: "ReactionRolesMessageId",
                principalTable: "ReactionRoleMessages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReactionRoleEmoteMappings_ReactionRoleMessages_ReactionRolesMessageId",
                table: "ReactionRoleEmoteMappings");

            migrationBuilder.DropIndex(
                name: "IX_ReactionRoleEmoteMappings_ReactionRolesMessageId",
                table: "ReactionRoleEmoteMappings");

            migrationBuilder.DropColumn(
                name: "ReactionRolesMessageId",
                table: "ReactionRoleEmoteMappings");

            migrationBuilder.AddForeignKey(
                name: "FK_ReactionRoleEmoteMappings_ReactionRoleMessages_ReactionRoleMessageId",
                table: "ReactionRoleEmoteMappings",
                column: "ReactionRoleMessageId",
                principalTable: "ReactionRoleMessages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
