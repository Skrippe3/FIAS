using Fias.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fias.Infrastructure.Persistence.Configurations;

public sealed class FiasObjectConfiguration
    : IEntityTypeConfiguration<FiasObject>
{
    public void Configure(EntityTypeBuilder<FiasObject> builder)
    {
        builder.ToTable("fias_objects");

        builder.HasKey(x => x.ObjectId)
            .HasName("pk_fias_objects");

        builder.Property(x => x.ObjectId)
            .HasColumnName("object_id")
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(x => x.ObjectGuid)
            .HasColumnName("object_guid")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(x => x.ChangeId)
            .HasColumnName("change_id")
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(x => x.LevelId)
            .HasColumnName("level_id")
            .IsRequired();

        builder.Property(x => x.CreateDate)
            .HasColumnName("create_date")
            .HasColumnType("date");

        builder.Property(x => x.UpdateDate)
            .HasColumnName("update_date")
            .HasColumnType("date");

        builder.Property(x => x.RegionCode)
            .HasColumnName("region_code")
            .HasMaxLength(10);

        builder.HasIndex(x => x.ObjectGuid)
            .HasDatabaseName("ix_fias_objects_object_guid");

        builder.HasIndex(x => x.LevelId)
            .HasDatabaseName("ix_fias_objects_level_id");

        builder.HasIndex(x => new { x.IsActive, x.LevelId })
            .HasDatabaseName("ix_fias_objects_is_active_level_id");

        builder.HasIndex(x => x.RegionCode)
            .HasDatabaseName("ix_fias_objects_region_code");
    }
}
