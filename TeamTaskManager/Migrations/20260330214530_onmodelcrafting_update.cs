using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamTaskManager.Migrations
{
    /// <inheritdoc />
    public partial class onmodelcrafting_update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_Users_UploaderId",
                table: "Attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Users_CommenterId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Users_OwnerId",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_Sprints_Users_CreatorId",
                table: "Sprints");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_Users_UploaderId",
                table: "Attachments",
                column: "UploaderId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Users_CommenterId",
                table: "Comments",
                column: "CommenterId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Users_OwnerId",
                table: "Projects",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sprints_Users_CreatorId",
                table: "Sprints",
                column: "CreatorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_Users_UploaderId",
                table: "Attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Users_CommenterId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Users_OwnerId",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_Sprints_Users_CreatorId",
                table: "Sprints");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_Users_UploaderId",
                table: "Attachments",
                column: "UploaderId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Users_CommenterId",
                table: "Comments",
                column: "CommenterId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Users_OwnerId",
                table: "Projects",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sprints_Users_CreatorId",
                table: "Sprints",
                column: "CreatorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
