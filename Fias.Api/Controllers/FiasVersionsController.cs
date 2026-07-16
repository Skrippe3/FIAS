using Fias.Application.DTO;
using Fias.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Fias.Api.Controllers;

[ApiController]
[Route("api/fias/versions")]
[Produces("application/json")]
public sealed class FiasVersionsController : ControllerBase
{
    private readonly IFiasVersionService _fiasVersionService;

    public FiasVersionsController(
        IFiasVersionService fiasVersionService)
    {
        _fiasVersionService = fiasVersionService;
    }

    /// <summary>
    /// Возвращает журнал установленных версий ФИАС.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyCollection<FiasVersionDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<FiasVersionDto>>>
        GetVersions(CancellationToken cancellationToken)
    {
        var versions =
            await _fiasVersionService.GetInstalledVersionsAsync(
                cancellationToken);

        return Ok(versions);
    }

    /// <summary>
    /// Возвращает последнюю установленную версию ФИАС.
    /// </summary>
    [HttpGet("latest")]
    [ProducesResponseType(
        typeof(FiasVersionDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FiasVersionDto>> GetLatestVersion(
        CancellationToken cancellationToken)
    {
        var version =
            await _fiasVersionService.GetLatestInstalledVersionAsync(
                cancellationToken);

        if (version is null)
        {
            return NotFound(new
            {
                message = "Версии ФИАС пока не установлены."
            });
        }

        return Ok(version);
    }
}
