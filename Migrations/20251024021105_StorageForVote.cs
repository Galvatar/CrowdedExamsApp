using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace netventure.Migrations
{
    /// <inheritdoc />
    public partial class StorageForVote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserVote_Users_UserId",
                table: "UserVote");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserVote",
                table: "UserVote");

            migrationBuilder.RenameTable(
                name: "UserVote",
                newName: "UserVotes");

            migrationBuilder.RenameIndex(
                name: "IX_UserVote_UserId",
                table: "UserVotes",
                newName: "IX_UserVotes_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserVotes",
                table: "UserVotes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserVotes_Users_UserId",
                table: "UserVotes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserVotes_Users_UserId",
                table: "UserVotes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserVotes",
                table: "UserVotes");

            migrationBuilder.RenameTable(
                name: "UserVotes",
                newName: "UserVote");

            migrationBuilder.RenameIndex(
                name: "IX_UserVotes_UserId",
                table: "UserVote",
                newName: "IX_UserVote_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserVote",
                table: "UserVote",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserVote_Users_UserId",
                table: "UserVote",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
