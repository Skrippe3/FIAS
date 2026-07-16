using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Fias.Infrastructure.Migrations
{
    public partial class AddFiasObjects : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fias_objects",
                columns: table => new
                {
                    object_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    object_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    change_id = table.Column<long>(type: "bigint", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    level_id = table.Column<int>(type: "integer", nullable: false),
                    create_date = table.Column<DateOnly>(type: "date", nullable: true),
                    update_date = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fias_objects", x => x.object_id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_fias_objects_level_id",
                table: "fias_objects",
                column: "level_id");

            migrationBuilder.CreateIndex(
                name: "ix_fias_objects_object_guid",
                table: "fias_objects",
                column: "object_guid");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fias_objects");
        }
    }
}
