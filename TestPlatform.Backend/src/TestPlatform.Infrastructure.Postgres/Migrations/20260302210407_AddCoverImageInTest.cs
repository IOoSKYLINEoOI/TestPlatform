using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestPlatform.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddCoverImageInTest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "tests",
                newName: "AuthorId");

            migrationBuilder.AddColumn<string>(
                name: "CoverImageUrl",
                table: "tests",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoverImageUrl",
                table: "tests");

            migrationBuilder.RenameColumn(
                name: "AuthorId",
                table: "tests",
                newName: "UserId");
        }
    }
}
