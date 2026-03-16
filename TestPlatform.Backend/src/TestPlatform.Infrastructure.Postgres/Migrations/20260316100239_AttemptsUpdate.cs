using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestPlatform.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AttemptsUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_attempts_exams_ExamId",
                table: "attempts");

            migrationBuilder.DropForeignKey(
                name: "FK_attempts_tests_TestId",
                table: "attempts");

            migrationBuilder.DropIndex(
                name: "IX_attempts_ExamId",
                table: "attempts");

            migrationBuilder.DropIndex(
                name: "IX_attempts_TestId",
                table: "attempts");

            migrationBuilder.DropColumn(
                name: "ExamId",
                table: "attempts");

            migrationBuilder.DropColumn(
                name: "TestId",
                table: "attempts");

            migrationBuilder.AddColumn<Guid>(
                name: "SourceId",
                table: "attempts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "attempts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "attempts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_attempts_Type_SourceId",
                table: "attempts",
                columns: new[] { "Type", "SourceId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_attempts_Type_SourceId",
                table: "attempts");

            migrationBuilder.DropColumn(
                name: "SourceId",
                table: "attempts");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "attempts");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "attempts");

            migrationBuilder.AddColumn<Guid>(
                name: "ExamId",
                table: "attempts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TestId",
                table: "attempts",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_attempts_ExamId",
                table: "attempts",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_attempts_TestId",
                table: "attempts",
                column: "TestId");

            migrationBuilder.AddForeignKey(
                name: "FK_attempts_exams_ExamId",
                table: "attempts",
                column: "ExamId",
                principalTable: "exams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_attempts_tests_TestId",
                table: "attempts",
                column: "TestId",
                principalTable: "tests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
