namespace Fias.Domain.Entities;

public class FiasVersion
{
    public long Id { get; set; }

    public int VersionId { get; set; }

    public string? TextVersion { get; set; }

    public DateTime InstalledAtUtc { get; set; }

    public bool IsFullImport { get; set; }

    public string? SourceUrl { get; set; }
}
