using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fias.Infrastructure.Migrations
{
    public partial class AddAddressObjectRecordId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_fias_address_objects",
                table: "fias_address_objects");

            migrationBuilder.AddColumn<long>(
                name: "id",
                table: "fias_address_objects",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddPrimaryKey(
                name: "pk_fias_address_objects",
                table: "fias_address_objects",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_fias_address_objects_object_id",
                table: "fias_address_objects",
                column: "object_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_fias_address_objects",
                table: "fias_address_objects");

            migrationBuilder.DropIndex(
                name: "ix_fias_address_objects_object_id",
                table: "fias_address_objects");

            migrationBuilder.DropColumn(
                name: "id",
                table: "fias_address_objects");

            migrationBuilder.AddPrimaryKey(
                name: "pk_fias_address_objects",
                table: "fias_address_objects",
                column: "object_id");
        }
    }
}
