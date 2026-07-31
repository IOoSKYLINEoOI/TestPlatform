using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestPlatform.Infrastructure.Postgres.Migrations
{
    public partial class StrengthenAssessmentModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuestionIds",
                table: "exam_sections");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "tests",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "ReviewPolicy",
                table: "exams",
                type: "text",
                nullable: false,
                defaultValue: "AfterExamClosed");

            migrationBuilder.AddColumn<int>(
                name: "AttemptNumber",
                table: "attempts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LatestFinishAt",
                table: "attempts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RequestId",
                table: "attempts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewAvailableAt",
                table: "attempts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "exam_section_questions",
                columns: table => new
                {
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamId = table.Column<Guid>(type: "uuid", nullable: false),
                    SectionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_section_questions", x => new { x.ExamId, x.SectionId, x.QuestionId });
                    table.ForeignKey(
                        name: "FK_exam_section_questions_exam_sections_ExamId_SectionId",
                        columns: x => new { x.ExamId, x.SectionId },
                        principalTable: "exam_sections",
                        principalColumns: new[] { "ExamId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_exam_section_questions_questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_attempts_UserId_RequestId",
                table: "attempts",
                columns: new[] { "UserId", "RequestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_attempts_UserId_Type_SourceId_AttemptNumber",
                table: "attempts",
                columns: new[] { "UserId", "Type", "SourceId", "AttemptNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_section_questions_QuestionId",
                table: "exam_section_questions",
                column: "QuestionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exam_section_questions");

            migrationBuilder.DropIndex(
                name: "IX_attempts_UserId_RequestId",
                table: "attempts");

            migrationBuilder.DropIndex(
                name: "IX_attempts_UserId_Type_SourceId_AttemptNumber",
                table: "attempts");

            migrationBuilder.DropColumn(
                name: "ReviewPolicy",
                table: "exams");

            migrationBuilder.DropColumn(
                name: "AttemptNumber",
                table: "attempts");

            migrationBuilder.DropColumn(
                name: "LatestFinishAt",
                table: "attempts");

            migrationBuilder.DropColumn(
                name: "RequestId",
                table: "attempts");

            migrationBuilder.DropColumn(
                name: "ReviewAvailableAt",
                table: "attempts");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "tests",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<Guid[]>(
                name: "QuestionIds",
                table: "exam_sections",
                type: "uuid[]",
                nullable: false,
                defaultValue: new Guid[0]);
        }
    }
}
