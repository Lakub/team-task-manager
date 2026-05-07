using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamTaskManager.Migrations
{
    /// <inheritdoc />
    public partial class sprinttaskid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SprintTasks",
                table: "SprintTasks");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "SprintTasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SprintTasks",
                table: "SprintTasks",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_SprintTasks_SprintId",
                table: "SprintTasks",
                column: "SprintId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SprintTasks",
                table: "SprintTasks");

            migrationBuilder.DropIndex(
                name: "IX_SprintTasks_SprintId",
                table: "SprintTasks");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "SprintTasks");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SprintTasks",
                table: "SprintTasks",
                columns: new[] { "SprintId", "TaskId" });
        }
    }
}
