using Fias.Application.DTO;
using Fias.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Fias.Api.Controllers;

/// <summary>
/// Проверка соответствия XSD-схемы ГАР структуре базы данных.
/// </summary>
[ApiController]
[Route("api/fias/schema")]
[Produces("application/json")]
public sealed class FiasSchemaController : ControllerBase
{
    private readonly IFiasSchemaService _schemaService;
    private readonly ILogger<FiasSchemaController> _logger;

    public FiasSchemaController(
        IFiasSchemaService schemaService,
        ILogger<FiasSchemaController> logger)
    {
        _schemaService = schemaService;
        _logger = logger;
    }

    /// <summary>
    /// Проверяет XSD-файлы в каталоге (или из конфигурации) на совместимость
    /// с текущей моделью данных.
    /// </summary>
    /// <param name="xsdDirectory">
    /// Путь к каталогу со схемами. Если не задан — используется Fias:XsdPath.
    /// </param>
    [HttpGet("check")]
    [ProducesResponseType(typeof(FiasSchemaCheckResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FiasSchemaCheckResultDto>> Check(
        [FromQuery] string? xsdDirectory,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _schemaService.CheckSchemaAsync(
                xsdDirectory,
                cancellationToken);

            return Ok(result);
        }
        catch (Exception exception)
            when (exception is InvalidOperationException
                or DirectoryNotFoundException
                or FileNotFoundException)
        {
            _logger.LogWarning(exception, "Проверка XSD не выполнена.");

            return Problem(
                title: "Не удалось проверить XSD-схему",
                detail: exception.Message,
                statusCode: StatusCodes.Status400BadRequest);
        }
    }

    /// <summary>
    /// Генерирует по XSD-схеме DDL таблицы PostgreSQL
    /// (создание структуры базы данных на основе документации).
    /// </summary>
    /// <param name="xsdFile">Путь к XSD-файлу схемы.</param>
    [HttpGet("ddl")]
    [ProducesResponseType(typeof(FiasSchemaDdlDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FiasSchemaDdlDto>> GenerateDdl(
        [FromQuery] string xsdFile,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _schemaService.GenerateTableDdlAsync(
                xsdFile,
                cancellationToken);

            return Ok(result);
        }
        catch (Exception exception)
            when (exception is ArgumentException
                or InvalidOperationException
                or FileNotFoundException)
        {
            _logger.LogWarning(exception, "Генерация DDL не выполнена.");

            return Problem(
                title: "Не удалось сгенерировать DDL по XSD",
                detail: exception.Message,
                statusCode: StatusCodes.Status400BadRequest);
        }
    }
}
