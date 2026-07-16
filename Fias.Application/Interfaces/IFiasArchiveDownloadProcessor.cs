namespace Fias.Application.Interfaces;

public interface IFiasArchiveDownloadProcessor
{
    Task<bool> ProcessNextPendingAsync(
        CancellationToken cancellationToken = default);
}
