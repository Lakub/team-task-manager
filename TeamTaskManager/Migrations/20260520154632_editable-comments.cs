using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamTaskManager.Migrations
{
    /// <inheritdoc />
    public partial class editablecomments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "WikiArticles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Comments",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_WikiArticles_ProjectId",
                table: "WikiArticles",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_WikiArticles_Projects_ProjectId",
                table: "WikiArticles",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WikiArticles_Projects_ProjectId",
                table: "WikiArticles");

            migrationBuilder.DropIndex(
                name: "IX_WikiArticles_ProjectId",
                table: "WikiArticles");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "WikiArticles");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Comments");
        }
    }
}
