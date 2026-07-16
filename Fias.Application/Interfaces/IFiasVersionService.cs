using Fias.Application.DTO;

namespace Fias.Application.Interfaces;

public interface IFiasVersionService
{
    Task<IReadOnlyCollection<FiasVersionDto>> GetInstalledVersionsAsync(
        CancellationToken cancellationToken = default);

    Task<FiasVersionDto?> GetLatestInstalledVersionAsync(
        CancellationToken cancellationToken = default);
}
