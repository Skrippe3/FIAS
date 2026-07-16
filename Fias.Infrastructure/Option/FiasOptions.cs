namespace Fias.Infrastructure.Options;

public sealed class FiasOptions
{
    public const string SectionName = "Fias";

    public string LastDownloadInfoUrl { get; set; } = string.Empty;

    public string AllDownloadInfoUrl { get; set; } = string.Empty;

    public string StoragePath { get; set; } = string.Empty;

    public string XsdPath { get; set; } = string.Empty;

    public string InitialImportPath { get; set; } = string.Empty;

    public int InitialImportVersionId { get; set; }

    public bool InitialImportIncludeReestr { get; set; }

    public bool AutoUpdateEnabled { get; set; }
}
