using Fias.Application.DTO;
using Fias.Application.Interfaces;
using Fias.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fias.Infrastructure.Services;

public sealed class FiasVersionService : IFiasVersionService
{
    private readonly FiasDbContext _context;

    public FiasVersionService(FiasDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<FiasVersionDto>>
        GetInstalledVersionsAsync(
            CancellationToken cancellationToken = default)
    {
        return await _context.FiasVersions
            .AsNoTracking()
            .OrderByDescending(x => x.VersionId)
            .Select(x => new FiasVersionDto
            {
                Id = x.Id,
                VersionId = x.VersionId,
                TextVersion = x.TextVersion,
                InstalledAtUtc = x.InstalledAtUtc,
                IsFullImport = x.IsFullImport,
                SourceUrl = x.SourceUrl
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<FiasVersionDto?> GetLatestInstalledVersionAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.FiasVersions
            .AsNoTracking()
            .OrderByDescending(x => x.VersionId)
            .Select(x => new FiasVersionDto
            {
                Id = x.Id,
                VersionId = x.VersionId,
                TextVersion = x.TextVersion,
                InstalledAtUtc = x.InstalledAtUtc,
                IsFullImport = x.IsFullImport,
                SourceUrl = x.SourceUrl
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
