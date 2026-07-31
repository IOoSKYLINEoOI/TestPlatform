using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestPlatform.Infrastructure.Postgres.Migrations
{
    public partial class NormalizeTagNames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_tags_name_lower_unique",
                table: "tags");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedName",
                table: "tags",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE tags SET \"NormalizedName\" = upper(btrim(\"Name\"));");

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedName",
                table: "tags",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ux_tags_normalized_name",
                table: "tags",
                column: "NormalizedName",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_tags_normalized_name",
                table: "tags");

            migrationBuilder.DropColumn(
                name: "NormalizedName",
                table: "tags");

            migrationBuilder.CreateIndex(
                name: "ix_tags_name_lower_unique",
                table: "tags",
                column: "Name",
                unique: true);
        }
    }
}
