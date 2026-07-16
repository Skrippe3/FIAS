using Fias.Application.DTO;
using Fias.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Fias.Api.Controllers;

/// <summary>
/// Поиск по адресным объектам ФИАС.
/// </summary>
[ApiController]
[Route("api/fias/search")]
[Produces("application/json")]
public sealed class FiasSearchController : ControllerBase
{
    private readonly IFiasSearchService _searchService;

    public FiasSearchController(IFiasSearchService searchService)
    {
        _searchService = searchService;
    }

    /// <summary>
    /// Полнотекстовый поиск и поиск по полям (GET, удобно для веб-интерфейса).
    /// </summary>
    /// <param name="query">Строка поиска по наименованию.</param>
    /// <param name="typeName">Тип объекта (ул, г, обл, …).</param>
    /// <param name="levelId">Уровень адресного объекта.</param>
    /// <param name="regionCode">Код региона (2 знака).</param>
    /// <param name="onlyActive">Только актуальные объекты.</param>
    /// <param name="page">Номер страницы (с 1).</param>
    /// <param name="pageSize">Размер страницы (1..100).</param>
    [HttpGet]
    [ProducesResponseType(
        typeof(FiasSearchResponseDto),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<FiasSearchResponseDto>> Search(
        [FromQuery] string? query,
        [FromQuery] string? typeName,
        [FromQuery] int? levelId,
        [FromQuery] string? regionCode,
        [FromQuery] bool onlyActive = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var request = new FiasSearchRequestDto
        {
            Query = query,
            TypeName = typeName,
            LevelId = levelId,
            RegionCode = regionCode,
            OnlyActive = onlyActive,
            Page = page,
            PageSize = pageSize
        };

        var result = await _searchService.SearchAsync(
            request,
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Расширенный поиск по телу запроса (POST).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(
        typeof(FiasSearchResponseDto),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<FiasSearchResponseDto>> SearchByBody(
        [FromBody] FiasSearchRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _searchService.SearchAsync(
            request,
            cancellationToken);

        return Ok(result);
    }
}
