using Fias.Application.DTO;

namespace Fias.Application.Interfaces;

public interface IFiasDownloadQueueService
{
    Task<FiasDownloadDto> QueueLatestFullArchiveAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<FiasDownloadDto>> GetDownloadsAsync(
        CancellationToken cancellationToken = default);
}
