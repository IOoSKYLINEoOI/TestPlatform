using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestPlatform.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddExam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exams_questions");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "exams");

            migrationBuilder.AddColumn<Guid>(
                name: "AuthorId",
                table: "exams",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CoverImageName",
                table: "exams",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "exams",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<double>(
                name: "PassingRule_MinPercent",
                table: "exams",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PassingRule_MinScore",
                table: "exams",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAt",
                table: "exams",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Schedule_AvailableFrom",
                table: "exams",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Schedule_AvailableTo",
                table: "exams",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "exams",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "exams",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "exam_questions",
                columns: table => new
                {
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamId = table.Column<Guid>(type: "uuid", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    KeycloakId = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    TabNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tests_AuthorId",
                table: "tests",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_exams_AuthorId",
                table: "exams",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_exams_Status",
                table: "exams",
                column: "Status");

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

            migrationBuilder.AddForeignKey(
                name: "FK_attempts_users_UserId",
                table: "attempts",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tests_users_AuthorId",
                table: "tests",
                column: "AuthorId",
                principalTable: "users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_attempts_users_UserId",
                table: "attempts");

            migrationBuilder.DropForeignKey(
                name: "FK_tests_users_AuthorId",
                table: "tests");

            migrationBuilder.DropTable(
                name: "exam_questions");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropIndex(
                name: "IX_tests_AuthorId",
                table: "tests");

            migrationBuilder.DropIndex(
                name: "IX_exams_AuthorId",
                table: "exams");

            migrationBuilder.DropIndex(
                name: "IX_exams_Status",
                table: "exams");

            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "exams");

            migrationBuilder.DropColumn(
                name: "CoverImageName",
                table: "exams");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "exams");

            migrationBuilder.DropColumn(
                name: "PassingRule_MinPercent",
                table: "exams");

            migrationBuilder.DropColumn(
                name: "PassingRule_MinScore",
                table: "exams");

            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "exams");

            migrationBuilder.DropColumn(
                name: "Schedule_AvailableFrom",
                table: "exams");

            migrationBuilder.DropColumn(
                name: "Schedule_AvailableTo",
                table: "exams");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "exams");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "exams");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "exams",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "exams_questions",
                columns: table => new
                {
                    ExamsId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exams_questions", x => new { x.ExamsId, x.QuestionsId });
                    table.ForeignKey(
                        name: "FK_exams_questions_exams_ExamsId",
                        column: x => x.ExamsId,
                        principalTable: "exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_exams_questions_questions_QuestionsId",
                        column: x => x.QuestionsId,
                        principalTable: "questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_exams_questions_QuestionsId",
                table: "exams_questions",
                column: "QuestionsId");
        }
    }
}
