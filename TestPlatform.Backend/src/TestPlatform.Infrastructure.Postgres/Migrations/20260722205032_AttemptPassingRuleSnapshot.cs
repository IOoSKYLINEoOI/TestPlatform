using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestPlatform.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AttemptPassingRuleSnapshot : Migration
    {
        /// <inheritdoc />
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

        /// <inheritdoc />
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
