using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestPlatform.Infrastructure.Postgres.Migrations
{
    public partial class AttemptQuestionForeignKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_attempt_questions_QuestionId",
                table: "attempt_questions",
                column: "QuestionId");

            migrationBuilder.AddForeignKey(
                name: "FK_attempt_questions_questions_QuestionId",
                table: "attempt_questions",
                column: "QuestionId",
                principalTable: "questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_attempt_questions_questions_QuestionId",
                table: "attempt_questions");

            migrationBuilder.DropIndex(
                name: "IX_attempt_questions_QuestionId",
                table: "attempt_questions");
        }
    }
}
