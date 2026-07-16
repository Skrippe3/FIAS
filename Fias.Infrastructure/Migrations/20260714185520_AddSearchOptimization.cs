using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fias.Infrastructure.Migrations
{
    public partial class AddSearchOptimization : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "CREATE EXTENSION IF NOT EXISTS pg_trgm;");

            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS ix_fias_address_objects_name_trgm
                ON fias_address_objects
                USING gin (name gin_trgm_ops);
                """);

            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS ix_fias_objects_name_trgm
                ON fias_objects
                USING gin (name gin_trgm_ops);
                """);

            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS ix_fias_address_objects_active_name
                ON fias_address_objects (name)
                WHERE is_active = true;
                """);

            migrationBuilder.Sql(
                """
                CREATE OR REPLACE VIEW v_fias_active_addresses AS
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS v_fias_active_addresses;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS ix_fias_address_objects_active_name;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS ix_fias_objects_name_trgm;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS ix_fias_address_objects_name_trgm;");
        }
    }
}
