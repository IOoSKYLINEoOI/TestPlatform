using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestPlatform.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class FileAssetsStorageRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoverImageName",
                table: "tests");

            migrationBuilder.DropColumn(
                name: "ImageName",
                table: "questions");

            migrationBuilder.DropColumn(
                name: "CoverImageName",
                table: "exams");

            migrationBuilder.AddColumn<Guid>(
                name: "CoverImageId",
                table: "tests",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ImageId",
                table: "questions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CoverImageId",
                table: "exams",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "file_assets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AttachedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_file_assets", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_file_assets_ObjectKey",
                table: "file_assets",
                column: "ObjectKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_file_assets_UploadedByUserId_Status",
                table: "file_assets",
                columns: new[] { "UploadedByUserId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "file_assets");

            migrationBuilder.DropColumn(
                name: "CoverImageId",
                table: "tests");

            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "questions");

            migrationBuilder.DropColumn(
                name: "CoverImageId",
                table: "exams");

            migrationBuilder.AddColumn<string>(
                name: "CoverImageName",
                table: "tests",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageName",
                table: "questions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoverImageName",
                table: "exams",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
