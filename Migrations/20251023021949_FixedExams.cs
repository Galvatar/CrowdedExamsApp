using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace netventure.Migrations
{
    /// <inheritdoc />
    public partial class FixedExams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Question_Exams_ExamId",
                table: "Question");

            migrationBuilder.DropForeignKey(
                name: "FK_Reply_Solution_SolutionId",
                table: "Reply");

            migrationBuilder.DropForeignKey(
                name: "FK_Solution_Question_QuestionId",
                table: "Solution");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Solution",
                table: "Solution");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Reply",
                table: "Reply");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Question",
                table: "Question");

            migrationBuilder.RenameTable(
                name: "Solution",
                newName: "Solutions");

            migrationBuilder.RenameTable(
                name: "Reply",
                newName: "Replies");

            migrationBuilder.RenameTable(
                name: "Question",
                newName: "Questions");

            migrationBuilder.RenameIndex(
                name: "IX_Solution_QuestionId",
                table: "Solutions",
                newName: "IX_Solutions_QuestionId");

            migrationBuilder.RenameIndex(
                name: "IX_Reply_SolutionId",
                table: "Replies",
                newName: "IX_Replies_SolutionId");

            migrationBuilder.RenameIndex(
                name: "IX_Question_ExamId",
                table: "Questions",
                newName: "IX_Questions_ExamId");

            migrationBuilder.AddColumn<string>(
                name: "Difficulty",
                table: "Exams",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Institution",
                table: "Exams",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Solutions",
                table: "Solutions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Replies",
                table: "Replies",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Questions",
                table: "Questions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Exams_ExamId",
                table: "Questions",
                column: "ExamId",
                principalTable: "Exams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Replies_Solutions_SolutionId",
                table: "Replies",
                column: "SolutionId",
                principalTable: "Solutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Solutions_Questions_QuestionId",
                table: "Solutions",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Exams_ExamId",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_Replies_Solutions_SolutionId",
                table: "Replies");

            migrationBuilder.DropForeignKey(
                name: "FK_Solutions_Questions_QuestionId",
                table: "Solutions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Solutions",
                table: "Solutions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Replies",
                table: "Replies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Questions",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Difficulty",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "Institution",
                table: "Exams");

            migrationBuilder.RenameTable(
                name: "Solutions",
                newName: "Solution");

            migrationBuilder.RenameTable(
                name: "Replies",
                newName: "Reply");

            migrationBuilder.RenameTable(
                name: "Questions",
                newName: "Question");

            migrationBuilder.RenameIndex(
                name: "IX_Solutions_QuestionId",
                table: "Solution",
                newName: "IX_Solution_QuestionId");

            migrationBuilder.RenameIndex(
                name: "IX_Replies_SolutionId",
                table: "Reply",
                newName: "IX_Reply_SolutionId");

            migrationBuilder.RenameIndex(
                name: "IX_Questions_ExamId",
                table: "Question",
                newName: "IX_Question_ExamId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Solution",
                table: "Solution",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Reply",
                table: "Reply",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Question",
                table: "Question",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Question_Exams_ExamId",
                table: "Question",
                column: "ExamId",
                principalTable: "Exams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reply_Solution_SolutionId",
                table: "Reply",
                column: "SolutionId",
                principalTable: "Solution",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Solution_Question_QuestionId",
                table: "Solution",
                column: "QuestionId",
                principalTable: "Question",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
