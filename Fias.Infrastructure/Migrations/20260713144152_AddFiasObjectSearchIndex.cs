using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fias.Infrastructure.Migrations
{
    public partial class AddFiasObjectSearchIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_fias_objects",
                table: "fias_objects");

            migrationBuilder.AddPrimaryKey(
                name: "pk_fias_objects",
                table: "fias_objects",
                column: "object_id");

            migrationBuilder.CreateIndex(
                name: "ix_fias_objects_is_active_level_id",
                table: "fias_objects",
                columns: new[] { "is_active", "level_id" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_fias_objects",
                table: "fias_objects");

            migrationBuilder.DropIndex(
                name: "ix_fias_objects_is_active_level_id",
                table: "fias_objects");

            migrationBuilder.AddPrimaryKey(
                name: "PK_fias_objects",
                table: "fias_objects",
                column: "object_id");
        }
    }
}
