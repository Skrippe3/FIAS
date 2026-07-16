using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Fias.Infrastructure.Migrations
{
    public partial class AddFiasReestrObjects : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fias_reestr_objects",
                columns: table => new
                {
                    ObjectId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ObjectGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangeId = table.Column<long>(type: "bigint", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LevelId = table.Column<int>(type: "integer", nullable: false),
                    ObjectName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreateDate = table.Column<DateOnly>(type: "date", nullable: true),
                    UpdateDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fias_reestr_objects", x => x.ObjectId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_fias_reestr_objects_LevelId",
                table: "fias_reestr_objects",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "IX_fias_reestr_objects_ObjectGuid",
                table: "fias_reestr_objects",
                column: "ObjectGuid");

            migrationBuilder.CreateIndex(
                name: "IX_fias_reestr_objects_ObjectName",
                table: "fias_reestr_objects",
                column: "ObjectName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fias_reestr_objects");
        }
    }
}
