using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamTaskManager.Migrations
{
    /// <inheritdoc />
    public partial class removedeprecated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SprintTasks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "SprintTasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
