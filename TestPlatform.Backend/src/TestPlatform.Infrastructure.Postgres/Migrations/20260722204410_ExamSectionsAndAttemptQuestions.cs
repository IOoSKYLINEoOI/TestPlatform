using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestPlatform.Infrastructure.Postgres.Migrations
{
    public partial class ExamSectionsAndAttemptQuestions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exam_questions");

            migrationBuilder.DropColumn(
                name: "Score",
                table: "test_questions");

            migrationBuilder.AddColumn<int>(
                name: "AttemptsLimit",
                table: "exams",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "attempt_questions",
                columns: table => new
                {
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Score = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attempt_questions", x => new { x.AttemptId, x.QuestionId });
                    table.ForeignKey(
                        name: "FK_attempt_questions_attempts_AttemptId",
                        column: x => x.AttemptId,
                        principalTable: "attempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exam_sections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    QuestionsToSelect = table.Column<int>(type: "integer", nullable: false),
                    ScorePerQuestion = table.Column<int>(type: "integer", nullable: false),
                    QuestionIds = table.Column<Guid[]>(type: "uuid[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_sections", x => new { x.ExamId, x.Id });
                    table.ForeignKey(
                        name: "FK_exam_sections_exams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "attempt_questions");

            migrationBuilder.DropTable(
                name: "exam_sections");

            migrationBuilder.DropColumn(
                name: "AttemptsLimit",
                table: "exams");

            migrationBuilder.AddColumn<int>(
                name: "Score",
                table: "test_questions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "exam_questions",
                columns: table => new
                {
                    ExamId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_questions", x => new { x.ExamId, x.QuestionId });
                    table.ForeignKey(
                        name: "FK_exam_questions_exams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_exam_questions_ExamId_Order",
                table: "exam_questions",
                columns: new[] { "ExamId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_questions_ExamId_QuestionId",
                table: "exam_questions",
                columns: new[] { "ExamId", "QuestionId" },
                unique: true);
        }
    }
}
