using Fias.Application.DTO;

namespace Fias.Application.Interfaces;

public interface IFiasSearchService
{
    Task<FiasSearchResponseDto> SearchAsync(
        FiasSearchRequestDto request,
        CancellationToken cancellationToken = default);
}
