namespace Fias.Application.Interfaces;

public interface IFiasHierarchyImportService
{
    Task<long> ImportDirectoryAsync(
        string directoryPath,
        CancellationToken cancellationToken = default);
}
