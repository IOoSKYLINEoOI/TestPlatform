using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestPlatform.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAttempts2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Score",
                table: "attempts",
                newName: "MaxPoints");

            migrationBuilder.AddColumn<decimal>(
                name: "EarnedPoints",
                table: "attempts",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EarnedPoints",
                table: "attempts");

            migrationBuilder.RenameColumn(
                name: "MaxPoints",
                table: "attempts",
                newName: "Score");
        }
    }
}
