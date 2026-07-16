namespace Fias.Application.Interfaces;

public interface IFiasAddressObjectImportService
{
    Task<long> ImportDirectoryAsync(
        string directoryPath,
        bool clearTableBeforeImport,
        CancellationToken cancellationToken = default);

    Task<long> ApplyDeltaDirectoryAsync(
        string directoryPath,
        CancellationToken cancellationToken = default);
}
