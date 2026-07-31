using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestPlatform.Infrastructure.Postgres.Migrations
{
    public partial class AssessmentQuestionForeignKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_test_questions_QuestionId",
                table: "test_questions",
                column: "QuestionId");

            migrationBuilder.AddForeignKey(
                name: "FK_test_questions_questions_QuestionId",
                table: "test_questions",
                column: "QuestionId",
                principalTable: "questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_test_questions_questions_QuestionId",
                table: "test_questions");

            migrationBuilder.DropIndex(
                name: "IX_test_questions_QuestionId",
                table: "test_questions");
        }
    }
}
