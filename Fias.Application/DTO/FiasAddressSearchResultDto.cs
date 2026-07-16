namespace Fias.Application.DTO;

public sealed class FiasAddressSearchResultDto
{
    public long ObjectId { get; set; }

    public Guid ObjectGuid { get; set; }

    public long? ParentObjectId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string TypeName { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string FullAddress { get; set; } = string.Empty;

    public int LevelId { get; set; }

    public bool IsActive { get; set; }

    public string? RegionCode { get; set; }
}
