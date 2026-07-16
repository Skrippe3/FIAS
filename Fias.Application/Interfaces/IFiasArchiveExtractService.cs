namespace Fias.Application.Interfaces;

public interface IFiasArchiveExtractService
{
    Task ExtractAsync(
        string archivePath,
        CancellationToken cancellationToken = default);
}
