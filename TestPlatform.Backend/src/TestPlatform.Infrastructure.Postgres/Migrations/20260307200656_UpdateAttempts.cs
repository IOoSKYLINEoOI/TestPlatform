using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestPlatform.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAttempts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_attempts_ParentType_ParentId",
                table: "attempts");

            migrationBuilder.DropColumn(
                name: "ParentType",
                table: "attempts");

            migrationBuilder.RenameColumn(
                name: "ParentId",
                table: "attempts",
                newName: "UserId");

            migrationBuilder.AlterColumn<decimal>(
                name: "Score",
                table: "attempts",
                type: "numeric(5,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

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

            migrationBuilder.CreateIndex(
                name: "IX_attempts_UserId",
                table: "attempts",
                column: "UserId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Attempt_Parent",
                table: "attempts",
                sql: "(\"TestId\" IS NOT NULL AND \"ExamId\" IS NULL) OR (\"TestId\" IS NULL AND \"ExamId\" IS NOT NULL)");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropIndex(
                name: "IX_attempts_UserId",
                table: "attempts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Attempt_Parent",
                table: "attempts");

            migrationBuilder.DropColumn(
                name: "ExamId",
                table: "attempts");

            migrationBuilder.DropColumn(
                name: "TestId",
                table: "attempts");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "attempts",
                newName: "ParentId");

            migrationBuilder.AlterColumn<double>(
                name: "Score",
                table: "attempts",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)");

            migrationBuilder.AddColumn<int>(
                name: "ParentType",
                table: "attempts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_attempts_ParentType_ParentId",
                table: "attempts",
                columns: new[] { "ParentType", "ParentId" });
        }
    }
}
