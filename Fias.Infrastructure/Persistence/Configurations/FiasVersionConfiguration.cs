using Fias.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fias.Infrastructure.Persistence.Configurations;

public sealed class FiasVersionConfiguration
    : IEntityTypeConfiguration<FiasVersion>
{
    public void Configure(EntityTypeBuilder<FiasVersion> entity)
    {
        entity.ToTable("fias_versions");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        entity.Property(x => x.VersionId)
            .HasColumnName("version_id")
            .IsRequired();

        entity.Property(x => x.TextVersion)
            .HasColumnName("text_version")
            .HasMaxLength(250);

        entity.Property(x => x.InstalledAtUtc)
            .HasColumnName("installed_at_utc")
            .IsRequired();

        entity.Property(x => x.IsFullImport)
            .HasColumnName("is_full_import")
            .IsRequired();

        entity.Property(x => x.SourceUrl)
            .HasColumnName("source_url")
            .HasMaxLength(2000);

        entity.HasIndex(x => x.VersionId)
            .IsUnique()
            .HasDatabaseName("ux_fias_versions_version_id");
    }
}
