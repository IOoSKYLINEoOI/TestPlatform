using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestPlatform.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestAttemptEntity_Tests_TestId",
                table: "TestAttemptEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TestAttemptEntity",
                table: "TestAttemptEntity");

            migrationBuilder.RenameTable(
                name: "TestAttemptEntity",
                newName: "TestAttempts");

            migrationBuilder.RenameIndex(
                name: "IX_TestAttemptEntity_TestId",
                table: "TestAttempts",
                newName: "IX_TestAttempts_TestId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TestAttempts",
                table: "TestAttempts",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TestAttempts_Tests_TestId",
                table: "TestAttempts",
                column: "TestId",
                principalTable: "Tests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestAttempts_Tests_TestId",
                table: "TestAttempts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TestAttempts",
                table: "TestAttempts");

            migrationBuilder.RenameTable(
                name: "TestAttempts",
                newName: "TestAttemptEntity");

            migrationBuilder.RenameIndex(
                name: "IX_TestAttempts_TestId",
                table: "TestAttemptEntity",
                newName: "IX_TestAttemptEntity_TestId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TestAttemptEntity",
                table: "TestAttemptEntity",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TestAttemptEntity_Tests_TestId",
                table: "TestAttemptEntity",
                column: "TestId",
                principalTable: "Tests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
