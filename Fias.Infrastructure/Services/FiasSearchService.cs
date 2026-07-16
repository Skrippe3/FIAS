using Fias.Application.DTO;
using Fias.Application.Interfaces;
using Fias.Domain.Entities;
using Fias.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fias.Infrastructure.Services;

public sealed class FiasSearchService : IFiasSearchService
{
    private const int MaxPageSize = 100;

    private readonly FiasDbContext _context;

    public FiasSearchService(FiasDbContext context)
    {
        _context = context;
    }

    public async Task<FiasSearchResponseDto> SearchAsync(
        FiasSearchRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var page = request.Page < 1 ? 1 : request.Page;

        var pageSize = request.PageSize switch
        {
            < 1 => 20,
            > MaxPageSize => MaxPageSize,
            _ => request.PageSize
        };

        IQueryable<FiasAddressObject> query =
            _context.FiasAddressObjects.AsNoTracking();

        if (request.OnlyActive)
        {
            query = query.Where(x => x.IsActive);
        }

        var text = request.Query?.Trim();

        if (!string.IsNullOrEmpty(text))
        {
            var pattern = $"%{EscapeLike(text)}%";
            query = query.Where(x => EF.Functions.ILike(x.Name, pattern));
        }

        if (!string.IsNullOrWhiteSpace(request.TypeName))
        {
            var typeName = request.TypeName.Trim();
            query = query.Where(x => x.TypeName == typeName);
        }

        if (request.LevelId is { } levelId)
        {
            query = query.Where(x => x.LevelId == levelId);
        }

        if (!string.IsNullOrWhiteSpace(request.RegionCode))
        {
            var regionCode = request.RegionCode.Trim();
            query = query.Where(x => x.RegionCode == regionCode);
        }

        var total = await query.LongCountAsync(cancellationToken);

        query = string.IsNullOrEmpty(text)
            ? query.OrderBy(x => x.Name).ThenBy(x => x.ObjectId)
            : query
                .OrderByDescending(x =>
                    EF.Functions.TrigramsSimilarity(x.Name, text))
                .ThenBy(x => x.Name)
                .ThenBy(x => x.ObjectId);

        var rows = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.ObjectId,
                x.ObjectGuid,
                x.ParentObjectId,
                x.Name,
                x.TypeName,
                x.LevelId,
                x.IsActive,
                x.RegionCode,
                x.Path
            })
            .ToListAsync(cancellationToken);

        var names = await ResolvePathNamesAsync(
            rows.Select(x => x.Path),
            cancellationToken);

        var items = rows
            .Select(x => new FiasAddressSearchResultDto
            {
                ObjectId = x.ObjectId,
                ObjectGuid = x.ObjectGuid,
                ParentObjectId = x.ParentObjectId,
                Name = x.Name,
                TypeName = x.TypeName,
                FullName = x.TypeName + " " + x.Name,
                FullAddress = BuildFullAddress(x.Path, names),
                LevelId = x.LevelId,
                IsActive = x.IsActive,
                RegionCode = x.RegionCode
            })
            .ToList();

        return new FiasSearchResponseDto
        {
            Page = page,
            PageSize = pageSize,
            Total = total,
            Items = items
        };
    }

    private async Task<Dictionary<long, string>> ResolvePathNamesAsync(
        IEnumerable<string?> paths,
        CancellationToken cancellationToken)
    {
        var ids = paths
            .SelectMany(ParsePath)
            .Distinct()
            .ToArray();

        if (ids.Length == 0)
        {
            return [];
        }

        return await _context.FiasAddressObjects
            .AsNoTracking()
            .Where(x => ids.Contains(x.ObjectId))
            .Select(x => new { x.ObjectId, Title = x.TypeName + " " + x.Name })
            .ToDictionaryAsync(x => x.ObjectId, x => x.Title, cancellationToken);
    }

    private static string BuildFullAddress(
        string? path,
        IReadOnlyDictionary<long, string> names)
    {
        var titles = ParsePath(path)
            .Select(id => names.GetValueOrDefault(id))
            .Where(title => !string.IsNullOrWhiteSpace(title))
            .ToArray();

        return string.Join(", ", titles);
    }

    private static IEnumerable<long> ParsePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            yield break;
        }

        foreach (var part in path.Split(
            '.',
            StringSplitOptions.RemoveEmptyEntries |
            StringSplitOptions.TrimEntries))
        {
            if (long.TryParse(part, out var id))
            {
                yield return id;
            }
        }
    }

    private static string EscapeLike(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("%", "\\%")
            .Replace("_", "\\_");
    }
}
