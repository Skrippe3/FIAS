using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Fias.Infrastructure.Migrations
{
    public partial class RefactorSchemaCleanup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fias_import_regions");

            migrationBuilder.DropTable(
                name: "fias_reestr_objects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_fias_address_objects",
                table: "fias_address_objects");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "fias_address_objects",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "TypeName",
                table: "fias_address_objects",
                newName: "type_name");

            migrationBuilder.RenameColumn(
                name: "RegionCode",
                table: "fias_address_objects",
                newName: "region_code");

            migrationBuilder.RenameColumn(
                name: "ParentObjectId",
                table: "fias_address_objects",
                newName: "parent_object_id");

            migrationBuilder.RenameColumn(
                name: "ObjectGuid",
                table: "fias_address_objects",
                newName: "object_guid");

            migrationBuilder.RenameColumn(
                name: "LevelId",
                table: "fias_address_objects",
                newName: "level_id");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "fias_address_objects",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "ObjectId",
                table: "fias_address_objects",
                newName: "object_id");

            migrationBuilder.RenameIndex(
                name: "IX_fias_address_objects_ParentObjectId",
                table: "fias_address_objects",
                newName: "ix_fias_address_objects_parent_object_id");

            migrationBuilder.RenameIndex(
                name: "IX_fias_address_objects_ObjectGuid",
                table: "fias_address_objects",
                newName: "ix_fias_address_objects_object_guid");

            migrationBuilder.AlterColumn<string>(
                name: "region_code",
                table: "fias_address_objects",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<long>(
                name: "parent_object_id",
                table: "fias_address_objects",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "object_id",
                table: "fias_address_objects",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "pk_fias_address_objects",
                table: "fias_address_objects",
                column: "object_id");

            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS fias_import_logs
                (
                    file_name character varying(500) NOT NULL,
                    imported_at_utc timestamp with time zone NOT NULL,
                    CONSTRAINT pk_fias_import_logs PRIMARY KEY (file_name)
                );
                """);

            migrationBuilder.CreateIndex(
                name: "ix_fias_address_objects_region_code",
                table: "fias_address_objects",
                column: "region_code");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fias_import_logs");

            migrationBuilder.DropPrimaryKey(
                name: "pk_fias_address_objects",
                table: "fias_address_objects");

            migrationBuilder.DropIndex(
                name: "ix_fias_address_objects_region_code",
                table: "fias_address_objects");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "fias_address_objects",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "type_name",
                table: "fias_address_objects",
                newName: "TypeName");

            migrationBuilder.RenameColumn(
                name: "region_code",
                table: "fias_address_objects",
                newName: "RegionCode");

            migrationBuilder.RenameColumn(
                name: "parent_object_id",
                table: "fias_address_objects",
                newName: "ParentObjectId");

            migrationBuilder.RenameColumn(
                name: "object_guid",
                table: "fias_address_objects",
                newName: "ObjectGuid");

            migrationBuilder.RenameColumn(
                name: "level_id",
                table: "fias_address_objects",
                newName: "LevelId");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "fias_address_objects",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "object_id",
                table: "fias_address_objects",
                newName: "ObjectId");

            migrationBuilder.RenameIndex(
                name: "ix_fias_address_objects_parent_object_id",
                table: "fias_address_objects",
                newName: "IX_fias_address_objects_ParentObjectId");

            migrationBuilder.RenameIndex(
                name: "ix_fias_address_objects_object_guid",
                table: "fias_address_objects",
                newName: "IX_fias_address_objects_ObjectGuid");

            migrationBuilder.AlterColumn<string>(
                name: "RegionCode",
                table: "fias_address_objects",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ParentObjectId",
                table: "fias_address_objects",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ObjectId",
                table: "fias_address_objects",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_fias_address_objects",
                table: "fias_address_objects",
                column: "ObjectId");

            migrationBuilder.CreateTable(
                name: "fias_import_regions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FinishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ObjectsCount = table.Column<long>(type: "bigint", nullable: false),
                    RegionCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fias_import_regions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "fias_reestr_objects",
                columns: table => new
                {
                    ObjectId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChangeId = table.Column<long>(type: "bigint", nullable: false),
                    CreateDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LevelId = table.Column<int>(type: "integer", nullable: false),
                    ObjectGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    UpdateDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fias_reestr_objects", x => x.ObjectId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_fias_import_regions_RegionCode",
                table: "fias_import_regions",
                column: "RegionCode",
                unique: true);

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
    }
}
