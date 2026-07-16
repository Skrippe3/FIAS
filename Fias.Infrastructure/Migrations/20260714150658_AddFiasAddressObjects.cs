using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Fias.Infrastructure.Migrations
{
    public partial class AddFiasAddressObjects : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fias_address_objects",
                columns: table => new
                {
                    ObjectId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ObjectGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentObjectId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TypeName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LevelId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    RegionCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fias_address_objects", x => x.ObjectId);
                });

            migrationBuilder.CreateIndex(
                name: "ix_fias_address_objects_name",
                table: "fias_address_objects",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_fias_address_objects_ObjectGuid",
                table: "fias_address_objects",
                column: "ObjectGuid");

            migrationBuilder.CreateIndex(
                name: "IX_fias_address_objects_ParentObjectId",
                table: "fias_address_objects",
                column: "ParentObjectId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fias_address_objects");
        }
    }
}
