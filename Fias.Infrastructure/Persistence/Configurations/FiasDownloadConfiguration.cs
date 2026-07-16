using Fias.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fias.Infrastructure.Persistence.Configurations;

public sealed class FiasDownloadConfiguration
    : IEntityTypeConfiguration<FiasDownload>
{
    public void Configure(EntityTypeBuilder<FiasDownload> entity)
    {
        entity.ToTable("fias_downloads");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        entity.Property(x => x.VersionId)
            .HasColumnName("version_id")
            .IsRequired();

        entity.Property(x => x.DownloadUrl)
            .HasColumnName("download_url")
            .HasMaxLength(2000)
            .IsRequired();

        entity.Property(x => x.FilePath)
            .HasColumnName("file_path")
            .HasMaxLength(1000)
            .IsRequired();

        entity.Property(x => x.FileSize)
            .HasColumnName("file_size")
            .IsRequired();

        entity.Property(x => x.Status)
            .HasColumnName("status")
            .HasMaxLength(50)
            .IsRequired();

        entity.Property(x => x.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(4000);

        entity.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        entity.Property(x => x.CompletedAtUtc)
            .HasColumnName("completed_at_utc");

        entity.HasIndex(x => x.VersionId)
            .HasDatabaseName("ix_fias_downloads_version_id");

        entity.HasIndex(x => new { x.VersionId, x.Status })
            .HasDatabaseName("ix_fias_downloads_version_id_status");
    }
}
