namespace Fias.Application.Interfaces;

public interface IFiasReestrObjectImportService
{
    Task<long> ImportDirectoryAsync(
        string directoryPath,
        bool clearTableBeforeImport,
        CancellationToken cancellationToken = default);
}
