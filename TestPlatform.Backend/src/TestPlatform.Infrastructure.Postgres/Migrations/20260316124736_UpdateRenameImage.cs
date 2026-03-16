using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestPlatform.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRenameImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CoverImageUrl",
                table: "tests",
                newName: "CoverImageName");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "questions",
                newName: "ImageName");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "answer_options",
                newName: "ImageName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CoverImageName",
                table: "tests",
                newName: "CoverImageUrl");

            migrationBuilder.RenameColumn(
                name: "ImageName",
                table: "questions",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "ImageName",
                table: "answer_options",
                newName: "ImageUrl");
        }
    }
}
