using Fias.Application.DTO;

namespace Fias.Application.Interfaces;

public interface IFiasDownloadService
{
    Task<FiasDownloadInfoDto> GetLatestDownloadInfoAsync(
        CancellationToken cancellationToken = default);
}
