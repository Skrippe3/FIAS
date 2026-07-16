using Fias.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fias.Infrastructure.Persistence.Configurations;

public sealed class FiasImportLogConfiguration
    : IEntityTypeConfiguration<FiasImportLog>
{
    public void Configure(EntityTypeBuilder<FiasImportLog> entity)
    {
        entity.ToTable("fias_import_logs");

        entity.HasKey(x => x.FileName)
            .HasName("pk_fias_import_logs");

        entity.Property(x => x.FileName)
            .HasColumnName("file_name")
            .HasMaxLength(500)
            .IsRequired();

        entity.Property(x => x.ImportedAtUtc)
            .HasColumnName("imported_at_utc")
            .IsRequired();
    }
}
