namespace Fias.Domain.Entities;

public sealed class FiasDownload
{
    public long Id { get; set; }

    public int VersionId { get; set; }

    public string DownloadUrl { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? CompletedAtUtc { get; set; }
}
