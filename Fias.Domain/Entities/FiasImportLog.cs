namespace Fias.Domain.Entities;

public sealed class FiasImportLog
{
    public string FileName { get; set; } = string.Empty;

    public DateTime ImportedAtUtc { get; set; }
}
