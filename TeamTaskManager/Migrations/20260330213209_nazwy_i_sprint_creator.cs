using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamTaskManager.Migrations
{
    /// <inheritdoc />
    public partial class nazwy_i_sprint_creator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_Users_UserId",
                table: "Attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Users_UserId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Sprints_Users_UserId",
                table: "Sprints");

            migrationBuilder.DropIndex(
                name: "IX_Sprints_UserId",
                table: "Sprints");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Sprints");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Comments",
                newName: "CommenterId");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_UserId",
                table: "Comments",
                newName: "IX_Comments_CommenterId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Attachments",
                newName: "UploaderId");

            migrationBuilder.RenameIndex(
                name: "IX_Attachments_UserId",
                table: "Attachments",
                newName: "IX_Attachments_UploaderId");

            migrationBuilder.AddColumn<int>(
                name: "CreatorId",
                table: "Sprints",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Sprints_CreatorId",
                table: "Sprints",
                column: "CreatorId");

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
                name: "FK_Sprints_Users_CreatorId",
                table: "Sprints",
                column: "CreatorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
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
                name: "FK_Sprints_Users_CreatorId",
                table: "Sprints");

            migrationBuilder.DropIndex(
                name: "IX_Sprints_CreatorId",
                table: "Sprints");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Sprints");

            migrationBuilder.RenameColumn(
                name: "CommenterId",
                table: "Comments",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_CommenterId",
                table: "Comments",
                newName: "IX_Comments_UserId");

            migrationBuilder.RenameColumn(
                name: "UploaderId",
                table: "Attachments",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Attachments_UploaderId",
                table: "Attachments",
                newName: "IX_Attachments_UserId");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Sprints",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sprints_UserId",
                table: "Sprints",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_Users_UserId",
                table: "Attachments",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Users_UserId",
                table: "Comments",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sprints_Users_UserId",
                table: "Sprints",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
