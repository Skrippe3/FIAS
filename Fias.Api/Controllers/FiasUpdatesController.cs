using Fias.Application.DTO;
using Fias.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Fias.Api.Controllers;

/// <summary>
/// Планирование обновлений локальной базы ФИАС.
/// </summary>
[ApiController]
[Route("api/fias/updates")]
[Produces("application/json")]
public sealed class FiasUpdatesController : ControllerBase
{
    private readonly IFiasUpdatePlannerService _updatePlanner;
    private readonly ILogger<FiasUpdatesController> _logger;

    public FiasUpdatesController(
        IFiasUpdatePlannerService updatePlanner,
        ILogger<FiasUpdatesController> logger)
    {
        _updatePlanner = updatePlanner;
        _logger = logger;
    }

    /// <summary>
    /// Возвращает план обновления: установленную версию, последнюю доступную
    /// и упорядоченный список дифов к применению.
    /// </summary>
    [HttpGet("plan")]
    [ProducesResponseType(typeof(FiasUpdatePlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<FiasUpdatePlanDto>> GetPlan(
        CancellationToken cancellationToken)
    {
        try
        {
            var plan = await _updatePlanner.GetUpdatePlanAsync(cancellationToken);

            return Ok(plan);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Не удалось построить план обновления.");

            return Problem(
                title: "Ошибка обращения к сервису ФИАС",
                detail: exception.Message,
                statusCode: StatusCodes.Status502BadGateway);
        }
    }

    /// <summary>
    /// Ставит в очередь на скачивание недостающие дельты по порядку.
    /// Скачивание и применение выполняет фоновая служба (Fias.Worker).
    /// </summary>
    [HttpPost("queue")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> QueueUpdates(
        CancellationToken cancellationToken)
    {
        try
        {
            var queued = await _updatePlanner.QueuePendingUpdatesAsync(
                cancellationToken);

            return Accepted(new { queued });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Не удалось поставить обновления в очередь.");

            return Problem(
                title: "Ошибка постановки обновлений в очередь",
                detail: exception.Message,
                statusCode: StatusCodes.Status502BadGateway);
        }
    }
}
