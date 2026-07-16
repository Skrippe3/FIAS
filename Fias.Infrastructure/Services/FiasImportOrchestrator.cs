using Fias.Application.Interfaces;
using Fias.Domain.Constants;
using Fias.Domain.Entities;
using Fias.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fias.Infrastructure.Services;

public sealed class FiasImportOrchestrator : IFiasImportOrchestrator
{
    private readonly FiasDbContext _context;
    private readonly IFiasArchiveExtractService _extractService;
    private readonly IFiasReestrObjectImportService _reestrImportService;
    private readonly IFiasAddressObjectImportService _addressImportService;
    private readonly IFiasHierarchyImportService _hierarchyImportService;
    private readonly ILogger<FiasImportOrchestrator> _logger;

    public FiasImportOrchestrator(
        FiasDbContext context,
        IFiasArchiveExtractService extractService,
        IFiasReestrObjectImportService reestrImportService,
        IFiasAddressObjectImportService addressImportService,
        IFiasHierarchyImportService hierarchyImportService,
        ILogger<FiasImportOrchestrator> logger)
    {
        _context = context;
        _extractService = extractService;
        _reestrImportService = reestrImportService;
        _addressImportService = addressImportService;
        _hierarchyImportService = hierarchyImportService;
        _logger = logger;
    }

    public async Task<bool> ProcessNextCompletedAsync(
        CancellationToken cancellationToken = default)
    {
        var installedVersionIds = await _context.FiasVersions
            .AsNoTracking()
            .Select(x => x.VersionId)
            .ToListAsync(cancellationToken);

        var download = await _context.FiasDownloads
            .Where(x =>
                x.Status == FiasDownloadStatuses.Completed &&
                !installedVersionIds.Contains(x.VersionId))
            .OrderBy(x => x.VersionId)
            .FirstOrDefaultAsync(cancellationToken);

        if (download is null)
        {
            return false;
        }

        var isFullImport = IsFullArchive(download.DownloadUrl);

        _logger.LogInformation(
            "Разворачиваем версию {VersionId} ({Kind}). Архив: {File}.",
            download.VersionId,
            isFullImport ? "полная" : "дельта",
            download.FilePath);

        await _extractService.ExtractAsync(download.FilePath, cancellationToken);

        var xmlDirectory = Path.Combine(
            Path.GetDirectoryName(download.FilePath)!,
            "xml");

        await ImportXmlDirectoryAsync(
            xmlDirectory,
            download.VersionId,
            sourceUrl: download.DownloadUrl,
            isFullImport: isFullImport,
            includeReestr: isFullImport,
            cancellationToken);

        return true;
    }

    public async Task<long> ImportLocalDirectoryAsync(
        string xmlDirectory,
        int versionId,
        bool includeReestr,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(xmlDirectory))
        {
            throw new ArgumentException(
                "Не указан каталог XML.",
                nameof(xmlDirectory));
        }

        var alreadyInstalled = await _context.FiasVersions
            .AsNoTracking()
            .AnyAsync(x => x.VersionId == versionId, cancellationToken);

        if (alreadyInstalled)
        {
            _logger.LogInformation(
                "Версия {VersionId} уже установлена — локальный импорт пропущен.",
                versionId);

            return 0;
        }

        _logger.LogInformation(
            "Локальный импорт версии {VersionId} из каталога {Directory}.",
            versionId,
            xmlDirectory);

        return await ImportXmlDirectoryAsync(
            xmlDirectory,
            versionId,
            sourceUrl: xmlDirectory,
            isFullImport: true,
            includeReestr: includeReestr,
            cancellationToken);
    }

    private async Task<long> ImportXmlDirectoryAsync(
        string xmlDirectory,
        int versionId,
        string sourceUrl,
        bool isFullImport,
        bool includeReestr,
        CancellationToken cancellationToken)
    {
        long imported;

        if (isFullImport)
        {
            if (includeReestr)
            {
                await _reestrImportService.ImportDirectoryAsync(
                    xmlDirectory,
                    clearTableBeforeImport: true,
                    cancellationToken);
            }

            imported = await _addressImportService.ImportDirectoryAsync(
                xmlDirectory,
                clearTableBeforeImport: true,
                cancellationToken);

            await _hierarchyImportService.ImportDirectoryAsync(
                xmlDirectory,
                cancellationToken);
        }
        else
        {
            imported = await _addressImportService.ApplyDeltaDirectoryAsync(
                xmlDirectory,
                cancellationToken);
        }

        _context.FiasVersions.Add(new FiasVersion
        {
            VersionId = versionId,
            TextVersion = null,
            InstalledAtUtc = DateTime.UtcNow,
            IsFullImport = isFullImport,
            SourceUrl = sourceUrl
        });

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Версия {VersionId} установлена. Импортировано адресных объектов: {Count:N0}.",
            versionId,
            imported);

        return imported;
    }

    private static bool IsFullArchive(string downloadUrl)
    {
        return downloadUrl.IndexOf("delta", StringComparison.OrdinalIgnoreCase) < 0;
    }
}
