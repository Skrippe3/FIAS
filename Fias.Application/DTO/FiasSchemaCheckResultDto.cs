namespace Fias.Application.DTO;

public sealed class FiasSchemaCheckResultDto
{
    public bool IsCompatible { get; set; }

    public IReadOnlyCollection<FiasSchemaFileCheckDto> Files { get; set; } = [];
}

public sealed class FiasSchemaFileCheckDto
{
    public string FileName { get; set; } = string.Empty;

    public string Hash { get; set; } = string.Empty;

    public bool IsKnown { get; set; }

    public bool IsCompatible { get; set; }

    public IReadOnlyCollection<string> MissingAttributes { get; set; } = [];

    public IReadOnlyCollection<string> NewAttributes { get; set; } = [];
}
