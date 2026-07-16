using Fias.Application.DTO;

namespace Fias.Application.Interfaces;

public interface IFiasSchemaService
{
    Task<FiasSchemaCheckResultDto> CheckSchemaAsync(
        string? xsdDirectory,
        CancellationToken cancellationToken = default);

    Task<FiasSchemaDdlDto> GenerateTableDdlAsync(
        string xsdFile,
        CancellationToken cancellationToken = default);
}
