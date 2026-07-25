using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestPlatform.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class RenameTabNumberToEmployeeNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TabNumber",
                table: "users",
                newName: "EmployeeNumber");

            migrationBuilder.RenameIndex(
                name: "IX_users_TabNumber",
                table: "users",
                newName: "IX_users_EmployeeNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EmployeeNumber",
                table: "users",
                newName: "TabNumber");

            migrationBuilder.RenameIndex(
                name: "IX_users_EmployeeNumber",
                table: "users",
                newName: "IX_users_TabNumber");
        }
    }
}
