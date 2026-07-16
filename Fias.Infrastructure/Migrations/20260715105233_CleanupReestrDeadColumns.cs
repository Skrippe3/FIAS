using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Fias.Infrastructure.Migrations
{
    public partial class CleanupReestrDeadColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DROP INDEX IF EXISTS ix_fias_objects_name_trgm;");

            migrationBuilder.DropIndex(
                name: "ix_fias_objects_name",
                table: "fias_objects");

            migrationBuilder.DropColumn(
                name: "name",
                table: "fias_objects");

            migrationBuilder.DropColumn(
                name: "type_name",
                table: "fias_objects");

            migrationBuilder.AlterColumn<long>(
                name: "object_id",
                table: "fias_objects",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.Sql("DROP VIEW IF EXISTS v_fias_active_addresses;");

            migrationBuilder.Sql(
                """
                CREATE VIEW v_fias_active_addresses AS
                SELECT
                    id,
                    object_id,
                    object_guid,
                    parent_object_id,
                    name,
                    type_name,
                    type_name || ' ' || name AS full_name,
                    level_id,
                    region_code,
                    path
                FROM fias_address_objects
                WHERE is_active = true;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "object_id",
                table: "fias_objects",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "fias_objects",
                type: "character varying(250)",
                maxLength: 250,
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

            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS ix_fias_objects_name_trgm
                ON fias_objects USING gin (name gin_trgm_ops);
                """);

            migrationBuilder.Sql("DROP VIEW IF EXISTS v_fias_active_addresses;");

            migrationBuilder.Sql(
                """
                CREATE VIEW v_fias_active_addresses AS
                SELECT
                    object_id,
                    object_guid,
                    parent_object_id,
                    name,
                    type_name,
                    level_id,
                    region_code
                FROM fias_address_objects
                WHERE is_active = true;
                """);
        }
    }
}
