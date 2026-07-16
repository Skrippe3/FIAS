using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Fias.Infrastructure.Migrations
{
    public partial class AddFiasDownloads : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_fias_versions_version_id",
                table: "fias_versions",
                newName: "ux_fias_versions_version_id");

            migrationBuilder.AlterColumn<string>(
                name: "text_version",
                table: "fias_versions",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "source_url",
                table: "fias_versions",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "fias_downloads",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    version_id = table.Column<int>(type: "integer", nullable: false),
                    download_url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    file_path = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    file_size = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    error_message = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fias_downloads", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_fias_downloads_version_id",
                table: "fias_downloads",
                column: "version_id");

            migrationBuilder.CreateIndex(
                name: "ix_fias_downloads_version_id_status",
                table: "fias_downloads",
                columns: new[] { "version_id", "status" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fias_downloads");

            migrationBuilder.RenameIndex(
                name: "ux_fias_versions_version_id",
                table: "fias_versions",
                newName: "IX_fias_versions_version_id");

            migrationBuilder.AlterColumn<string>(
                name: "text_version",
                table: "fias_versions",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "source_url",
                table: "fias_versions",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);
        }
    }
}
