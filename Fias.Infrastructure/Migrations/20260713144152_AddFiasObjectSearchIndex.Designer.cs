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
    [Migration("20260713144152_AddFiasObjectSearchIndex")]
    partial class AddFiasObjectSearchIndex
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Fias.Domain.Entities.FiasDownload", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<DateTime?>("CompletedAtUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("completed_at_utc");

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at_utc");

                    b.Property<string>("DownloadUrl")
                        .IsRequired()
                        .HasMaxLength(2000)
                        .HasColumnType("character varying(2000)")
                        .HasColumnName("download_url");

                    b.Property<string>("ErrorMessage")
                        .HasMaxLength(4000)
                        .HasColumnType("character varying(4000)")
                        .HasColumnName("error_message");

                    b.Property<string>("FilePath")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)")
                        .HasColumnName("file_path");

                    b.Property<long>("FileSize")
                        .HasColumnType("bigint")
                        .HasColumnName("file_size");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("status");

                    b.Property<int>("VersionId")
                        .HasColumnType("integer")
                        .HasColumnName("version_id");

                    b.HasKey("Id");

                    b.HasIndex("VersionId")
                        .HasDatabaseName("ix_fias_downloads_version_id");

                    b.HasIndex("VersionId", "Status")
                        .HasDatabaseName("ix_fias_downloads_version_id_status");

                    b.ToTable("fias_downloads", (string)null);
                });

            modelBuilder.Entity("Fias.Domain.Entities.FiasObject", b =>
                {
                    b.Property<long>("ObjectId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("object_id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("ObjectId"));

                    b.Property<long>("ChangeId")
                        .HasColumnType("bigint")
                        .HasColumnName("change_id");

                    b.Property<DateOnly?>("CreateDate")
                        .HasColumnType("date")
                        .HasColumnName("create_date");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean")
                        .HasColumnName("is_active");

                    b.Property<int>("LevelId")
                        .HasColumnType("integer")
                        .HasColumnName("level_id");

                    b.Property<Guid>("ObjectGuid")
                        .HasColumnType("uuid")
                        .HasColumnName("object_guid");

                    b.Property<DateOnly?>("UpdateDate")
                        .HasColumnType("date")
                        .HasColumnName("update_date");

                    b.HasKey("ObjectId")
                        .HasName("pk_fias_objects");

                    b.HasIndex("LevelId")
                        .HasDatabaseName("ix_fias_objects_level_id");

                    b.HasIndex("ObjectGuid")
                        .HasDatabaseName("ix_fias_objects_object_guid");

                    b.HasIndex("IsActive", "LevelId")
                        .HasDatabaseName("ix_fias_objects_is_active_level_id");

                    b.ToTable("fias_objects", (string)null);
                });

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
                        .HasMaxLength(2000)
                        .HasColumnType("character varying(2000)")
                        .HasColumnName("source_url");

                    b.Property<string>("TextVersion")
                        .HasMaxLength(250)
                        .HasColumnType("character varying(250)")
                        .HasColumnName("text_version");

                    b.Property<int>("VersionId")
                        .HasColumnType("integer")
                        .HasColumnName("version_id");

                    b.HasKey("Id");

                    b.HasIndex("VersionId")
                        .IsUnique()
                        .HasDatabaseName("ux_fias_versions_version_id");

                    b.ToTable("fias_versions", (string)null);
                });
#pragma warning restore 612, 618
        }
    }
}
