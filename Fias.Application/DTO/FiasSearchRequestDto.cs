namespace Fias.Application.DTO;

public sealed class FiasSearchRequestDto
{
    public string? Query { get; set; }

    public string? TypeName { get; set; }

    public int? LevelId { get; set; }

    public string? RegionCode { get; set; }

    public bool OnlyActive { get; set; } = true;

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
