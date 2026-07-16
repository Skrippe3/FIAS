using Fias.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fias.Infrastructure.Persistence.Configurations;

public sealed class FiasAddressObjectConfiguration
    : IEntityTypeConfiguration<FiasAddressObject>
{
    public void Configure(EntityTypeBuilder<FiasAddressObject> builder)
    {
        builder.ToTable("fias_address_objects");

        builder.HasKey(x => x.Id)
            .HasName("pk_fias_address_objects");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(x => x.ObjectId)
            .HasColumnName("object_id")
            .IsRequired();

        builder.Property(x => x.ObjectGuid)
            .HasColumnName("object_guid")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(x => x.ParentObjectId)
            .HasColumnName("parent_object_id");

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.TypeName)
            .HasColumnName("type_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.LevelId)
            .HasColumnName("level_id")
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(x => x.RegionCode)
            .HasColumnName("region_code")
            .HasMaxLength(10);

        builder.Property(x => x.Path)
            .HasColumnName("path")
            .HasMaxLength(500);

        builder.HasIndex(x => x.Name)
            .HasDatabaseName("ix_fias_address_objects_name");

        builder.HasIndex(x => x.ObjectId)
            .HasDatabaseName("ix_fias_address_objects_object_id");

        builder.HasIndex(x => x.ObjectGuid)
            .HasDatabaseName("ix_fias_address_objects_object_guid");

        builder.HasIndex(x => x.ParentObjectId)
            .HasDatabaseName("ix_fias_address_objects_parent_object_id");

        builder.HasIndex(x => x.RegionCode)
            .HasDatabaseName("ix_fias_address_objects_region_code");
    }
}
