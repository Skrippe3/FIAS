using System;
using Fias.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Fias.Infrastructure.Migrations
{
    [DbContext(typeof(FiasDbContext))]
    [Migration("20260709191830_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Fias.Domain.Entities.FiasVersion", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<DateTime>("InstalledAtUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("installed_at_utc");

                    b.Property<bool>("IsFullImport")
                        .HasColumnType("boolean")
                        .HasColumnName("is_full_import");

                    b.Property<string>("SourceUrl")
                        .HasColumnType("text")
                        .HasColumnName("source_url");

                    b.Property<string>("TextVersion")
                        .HasColumnType("text")
                        .HasColumnName("text_version");

                    b.Property<int>("VersionId")
                        .HasColumnType("integer")
                        .HasColumnName("version_id");

                    b.HasKey("Id");

                    b.HasIndex("VersionId")
                        .IsUnique();

                    b.ToTable("fias_versions", (string)null);
                });
#pragma warning restore 612, 618
        }
    }
}
