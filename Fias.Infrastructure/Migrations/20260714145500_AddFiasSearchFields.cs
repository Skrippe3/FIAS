using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fias.Infrastructure.Migrations
{
    public partial class AddFiasSearchFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "fias_objects",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "region_code",
                table: "fias_objects",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "type_name",
                table: "fias_objects",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_fias_objects_name",
                table: "fias_objects",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_fias_objects_region_code",
                table: "fias_objects",
                column: "region_code");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_fias_objects_name",
                table: "fias_objects");

            migrationBuilder.DropIndex(
                name: "ix_fias_objects_region_code",
                table: "fias_objects");

            migrationBuilder.DropColumn(
                name: "name",
                table: "fias_objects");

            migrationBuilder.DropColumn(
                name: "region_code",
                table: "fias_objects");

            migrationBuilder.DropColumn(
                name: "type_name",
                table: "fias_objects");
        }
    }
}
