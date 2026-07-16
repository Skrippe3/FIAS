using Fias.Application.DTO;
using Fias.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Fias.Api.Controllers;

[ApiController]
[Route("api/fias/downloads")]
[Produces("application/json")]
public sealed class FiasDownloadsController : ControllerBase
{
    private readonly IFiasDownloadService _downloadInfoService;
    private readonly IFiasDownloadQueueService _downloadQueueService;
    private readonly ILogger<FiasDownloadsController> _logger;

    public FiasDownloadsController(
        IFiasDownloadService downloadInfoService,
        IFiasDownloadQueueService downloadQueueService,
        ILogger<FiasDownloadsController> logger)
    {
        _downloadInfoService = downloadInfoService;
        _downloadQueueService = downloadQueueService;
        _logger = logger;
    }

    /// <summary>
    /// Возвращает информацию о последней выгрузке ФИАС.
    /// </summary>
    [HttpGet("latest")]
    [ProducesResponseType(
        typeof(FiasDownloadInfoDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<FiasDownloadInfoDto>> GetLatest(
        CancellationToken cancellationToken)
    {
        try
        {
            var result =
                await _downloadInfoService.GetLatestDownloadInfoAsync(
                    cancellationToken);

            return Ok(result);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Не удалось получить последнюю версию ФИАС.");

            return Problem(
                title: "Ошибка обращения к сервису ФИАС",
                detail: exception.Message,
                statusCode: StatusCodes.Status502BadGateway);
        }
    }

    /// <summary>
    /// Возвращает журнал заданий на скачивание.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyCollection<FiasDownloadDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<FiasDownloadDto>>>
        GetDownloads(CancellationToken cancellationToken)
    {
        var downloads =
            await _downloadQueueService.GetDownloadsAsync(
                cancellationToken);

        return Ok(downloads);
    }

    /// <summary>
    /// Создаёт задание на скачивание последней полной выгрузки ГАР.
    /// Сам файл будет скачан Worker-службой.
    /// </summary>
    [HttpPost("latest/full/queue")]
    [ProducesResponseType(
        typeof(FiasDownloadDto),
        StatusCodes.Status202Accepted)]
    [ProducesResponseType(
        StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<FiasDownloadDto>>
        QueueLatestFullArchive(CancellationToken cancellationToken)
    {
        try
        {
            var download =
                await _downloadQueueService.QueueLatestFullArchiveAsync(
                    cancellationToken);

            return Accepted(download);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Не удалось создать задание на скачивание ФИАС.");

            return Problem(
                title: "Не удалось создать задание",
                detail: exception.Message,
                statusCode: StatusCodes.Status502BadGateway);
        }
    }
}
