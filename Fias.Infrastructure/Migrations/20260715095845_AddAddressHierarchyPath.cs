using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fias.Infrastructure.Migrations
{
    public partial class AddAddressHierarchyPath : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "path",
                table: "fias_address_objects",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "path",
                table: "fias_address_objects");
        }
    }
}
