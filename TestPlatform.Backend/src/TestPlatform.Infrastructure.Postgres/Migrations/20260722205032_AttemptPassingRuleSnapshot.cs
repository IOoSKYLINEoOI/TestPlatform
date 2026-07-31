using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestPlatform.Infrastructure.Postgres.Migrations
{
    public partial class AttemptPassingRuleSnapshot : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "MinPassingPercent",
                table: "attempts",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinPassingScore",
                table: "attempts",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinPassingPercent",
                table: "attempts");

            migrationBuilder.DropColumn(
                name: "MinPassingScore",
                table: "attempts");
        }
    }
}
