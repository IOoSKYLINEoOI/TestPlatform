using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestPlatform.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PassingRule_MinPercent",
                table: "exams");

            migrationBuilder.DropColumn(
                name: "PassingRule_MinScore",
                table: "exams");

            migrationBuilder.DropColumn(
                name: "Schedule_AvailableFrom",
                table: "exams");

            migrationBuilder.DropColumn(
                name: "Schedule_AvailableTo",
                table: "exams");

            migrationBuilder.AddColumn<string>(
                name: "PassingRule",
                table: "exams",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Schedule",
                table: "exams",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PassingRule",
                table: "exams");

            migrationBuilder.DropColumn(
                name: "Schedule",
                table: "exams");

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
                name: "Schedule_AvailableFrom",
                table: "exams",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Schedule_AvailableTo",
                table: "exams",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
