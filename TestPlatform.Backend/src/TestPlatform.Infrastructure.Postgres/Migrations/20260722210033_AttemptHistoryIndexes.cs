using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestPlatform.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AttemptHistoryIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_attempts_Type_SourceId",
                table: "attempts");

            migrationBuilder.DropIndex(
                name: "IX_attempts_UserId",
                table: "attempts");

            migrationBuilder.DropIndex(
                name: "IX_attempts_UserId_Status",
                table: "attempts");

            migrationBuilder.CreateIndex(
                name: "IX_attempts_Type_SourceId_StartedAt",
                table: "attempts",
                columns: new[] { "Type", "SourceId", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_attempts_UserId_StartedAt",
                table: "attempts",
                columns: new[] { "UserId", "StartedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_attempts_Type_SourceId_StartedAt",
                table: "attempts");

            migrationBuilder.DropIndex(
                name: "IX_attempts_UserId_StartedAt",
                table: "attempts");

            migrationBuilder.CreateIndex(
                name: "IX_attempts_Type_SourceId",
                table: "attempts",
                columns: new[] { "Type", "SourceId" });

            migrationBuilder.CreateIndex(
                name: "IX_attempts_UserId",
                table: "attempts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_attempts_UserId_Status",
                table: "attempts",
                columns: new[] { "UserId", "Status" });
        }
    }
}
