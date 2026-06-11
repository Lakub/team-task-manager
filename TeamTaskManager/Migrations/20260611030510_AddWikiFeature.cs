using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamTaskManager.Migrations
{
    /// <inheritdoc />
    public partial class AddWikiFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "WikiArticles",
                nullable: false,
                defaultValue: false);

            
            migrationBuilder.AddColumn<bool>(
                name: "IsFavorite",
                table: "WikiArticles",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            
            migrationBuilder.AddColumn<int>(
                name: "ParentArticleId",
                table: "WikiArticles",
                type: "INTEGER",
                nullable: true); 
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "IsDraft", table: "WikiArticles");
            migrationBuilder.DropColumn(name: "IsFavorite", table: "WikiArticles");
            migrationBuilder.DropColumn(name: "ParentArticleId", table: "WikiArticles");
        }
    }
}
