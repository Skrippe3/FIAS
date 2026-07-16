namespace Fias.Application.DTO;

public sealed class FiasUpdatePlanDto
{
    public int? InstalledVersionId { get; set; }

    public int? LatestAvailableVersionId { get; set; }

    public bool IsUpToDate { get; set; }

    public bool RequiresFullImport { get; set; }

    public IReadOnlyCollection<FiasUpdateStepDto> Steps { get; set; } = [];
}

public sealed class FiasUpdateStepDto
{
    public int VersionId { get; set; }

    public string? TextVersion { get; set; }

    public string? GarXmlDeltaUrl { get; set; }
}
