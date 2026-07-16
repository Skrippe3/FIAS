namespace Fias.Domain.Entities;

public class FiasObject
{
    public long ObjectId { get; set; }

    public Guid ObjectGuid { get; set; }

    public long ChangeId { get; set; }

    public bool IsActive { get; set; }

    public int LevelId { get; set; }

    public DateOnly? CreateDate { get; set; }

    public DateOnly? UpdateDate { get; set; }

    public string? RegionCode { get; set; }
}
