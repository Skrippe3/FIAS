using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Fias.Infrastructure.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fias_versions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    version_id = table.Column<int>(type: "integer", nullable: false),
                    text_version = table.Column<string>(type: "text", nullable: true),
                    installed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_full_import = table.Column<bool>(type: "boolean", nullable: false),
                    source_url = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fias_versions", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_fias_versions_version_id",
                table: "fias_versions",
                column: "version_id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fias_versions");
        }
    }
}
