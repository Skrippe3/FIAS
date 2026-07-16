namespace Fias.Application.DTO;

public sealed class FiasSearchResponseDto
{
    public int Page { get; set; }

    public int PageSize { get; set; }

    public long Total { get; set; }

    public IReadOnlyCollection<FiasAddressSearchResultDto> Items { get; set; }
        = [];
}
