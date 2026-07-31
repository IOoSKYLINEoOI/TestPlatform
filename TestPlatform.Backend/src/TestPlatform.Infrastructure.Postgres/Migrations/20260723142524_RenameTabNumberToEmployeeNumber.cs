using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestPlatform.Infrastructure.Postgres.Migrations
{
    public partial class RenameTabNumberToEmployeeNumber : Migration
    {
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
