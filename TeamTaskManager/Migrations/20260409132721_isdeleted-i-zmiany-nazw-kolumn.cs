using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamTaskManager.Migrations
{
    /// <inheritdoc />
    public partial class isdeletedizmianynazwkolumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SprintTasks_Users_LastAssigneeId",
                table: "SprintTasks");

            migrationBuilder.RenameColumn(
                name: "LastStatus",
                table: "SprintTasks",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "LastAssigneeId",
                table: "SprintTasks",
                newName: "AssigneeId");

            migrationBuilder.RenameIndex(
                name: "IX_SprintTasks_LastAssigneeId",
                table: "SprintTasks",
                newName: "IX_SprintTasks_AssigneeId");

            migrationBuilder.AlterColumn<int>(
                name: "AddedById",
                table: "SprintTasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "SprintTasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ProjectUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_SprintTasks_Users_AssigneeId",
                table: "SprintTasks",
                column: "AssigneeId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SprintTasks_Users_AssigneeId",
                table: "SprintTasks");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SprintTasks");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ProjectUsers");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "SprintTasks",
                newName: "LastStatus");

            migrationBuilder.RenameColumn(
                name: "AssigneeId",
                table: "SprintTasks",
                newName: "LastAssigneeId");

            migrationBuilder.RenameIndex(
                name: "IX_SprintTasks_AssigneeId",
                table: "SprintTasks",
                newName: "IX_SprintTasks_LastAssigneeId");

            migrationBuilder.AlterColumn<int>(
                name: "AddedById",
                table: "SprintTasks",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_SprintTasks_Users_LastAssigneeId",
                table: "SprintTasks",
                column: "LastAssigneeId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
