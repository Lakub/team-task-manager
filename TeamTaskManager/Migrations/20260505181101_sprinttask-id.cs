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

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WikiArticles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WikiArticles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TagWikiArticle",
                columns: table => new
                {
                    ArticlesId = table.Column<int>(type: "INTEGER", nullable: false),
                    TagsId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagWikiArticle", x => new { x.ArticlesId, x.TagsId });
                    table.ForeignKey(
                        name: "FK_TagWikiArticle_Tags_TagsId",
                        column: x => x.TagsId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TagWikiArticle_WikiArticles_ArticlesId",
                        column: x => x.ArticlesId,
                        principalTable: "WikiArticles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SprintTasks_SprintId",
                table: "SprintTasks",
                column: "SprintId");

            migrationBuilder.CreateIndex(
                name: "IX_TagWikiArticle_TagsId",
                table: "TagWikiArticle",
                column: "TagsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TagWikiArticle");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "WikiArticles");

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
