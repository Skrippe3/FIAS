namespace Fias.Application.Interfaces;

public interface IFiasImportOrchestrator
{
    Task<bool> ProcessNextCompletedAsync(
        CancellationToken cancellationToken = default);

    Task<long> ImportLocalDirectoryAsync(
        string xmlDirectory,
        int versionId,
        bool includeReestr,
        CancellationToken cancellationToken = default);
}
