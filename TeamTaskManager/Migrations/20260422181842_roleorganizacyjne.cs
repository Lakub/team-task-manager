using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TeamTaskManager.Models;

#nullable disable

namespace TeamTaskManager.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260422181842_roleorganizacyjne")]
    public partial class roleorganizacyjne : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrgRole",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrgRole",
                table: "Users");
        }
    }
}
