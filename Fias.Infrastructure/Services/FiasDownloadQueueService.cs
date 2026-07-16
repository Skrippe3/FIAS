using Fias.Application.DTO;
using Fias.Application.Interfaces;
using Fias.Domain.Constants;
using Fias.Domain.Entities;
using Fias.Infrastructure.Options;
using Fias.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fias.Infrastructure.Services;

public sealed class FiasDownloadQueueService : IFiasDownloadQueueService
{
    private readonly FiasDbContext _context;
    private readonly IFiasDownloadService _downloadInfoService;
    private readonly FiasOptions _options;
    private readonly ILogger<FiasDownloadQueueService> _logger;

    public FiasDownloadQueueService(
        FiasDbContext context,
        IFiasDownloadService downloadInfoService,
        IOptions<FiasOptions> options,
        ILogger<FiasDownloadQueueService> logger)
    {
        _context = context;
        _downloadInfoService = downloadInfoService;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<FiasDownloadDto> QueueLatestFullArchiveAsync(
        CancellationToken cancellationToken = default)
    {
        var downloadInfo =
            await _downloadInfoService.GetLatestDownloadInfoAsync(
                cancellationToken);

        if (string.IsNullOrWhiteSpace(downloadInfo.GarXmlFullUrl))
        {
            throw new InvalidOperationException(
                "Сервис ФИАС не вернул ссылку GarXMLFullURL.");
        }

        if (string.IsNullOrWhiteSpace(_options.StoragePath))
        {
            throw new InvalidOperationException(
                "В конфигурации не указан Fias:StoragePath.");
        }

        var existingDownload = await _context.FiasDownloads
            .AsNoTracking()
            .Where(x =>
                x.VersionId == downloadInfo.VersionId &&
                x.DownloadUrl == downloadInfo.GarXmlFullUrl &&
                (
                    x.Status == FiasDownloadStatuses.Pending ||
                    x.Status == FiasDownloadStatuses.Running ||
                    x.Status == FiasDownloadStatuses.Completed
                ))
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingDownload is not null)
        {
            _logger.LogInformation(
                "Задание для версии ФИАС {VersionId} уже существует. " +
                "Статус: {Status}.",
                existingDownload.VersionId,
                existingDownload.Status);

            return MapToDto(existingDownload);
        }

        var versionDirectory = Path.Combine(
            _options.StoragePath,
            downloadInfo.VersionId.ToString());

        var archiveFileName = GetFileName(downloadInfo.GarXmlFullUrl);

        var archiveFilePath = Path.Combine(
            versionDirectory,
            archiveFileName);

        var download = new FiasDownload
        {
            VersionId = downloadInfo.VersionId,
            DownloadUrl = downloadInfo.GarXmlFullUrl,
            FilePath = archiveFilePath,
            FileSize = 0,
            Status = FiasDownloadStatuses.Pending,
            ErrorMessage = null,
            CreatedAtUtc = DateTime.UtcNow,
            CompletedAtUtc = null
        };

        _context.FiasDownloads.Add(download);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Создано задание {DownloadId} на скачивание полной версии ФИАС " +
            "{VersionId}.",
            download.Id,
            download.VersionId);

        return MapToDto(download);
    }

    public async Task<IReadOnlyCollection<FiasDownloadDto>> GetDownloadsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.FiasDownloads
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new FiasDownloadDto
            {
                Id = x.Id,
                VersionId = x.VersionId,
                DownloadUrl = x.DownloadUrl,
                FilePath = x.FilePath,
                FileSize = x.FileSize,
                Status = x.Status,
                ErrorMessage = x.ErrorMessage,
                CreatedAtUtc = x.CreatedAtUtc,
                CompletedAtUtc = x.CompletedAtUtc
            })
            .ToListAsync(cancellationToken);
    }

    private static string GetFileName(string downloadUrl)
    {
        if (!Uri.TryCreate(
                downloadUrl,
                UriKind.Absolute,
                out var uri))
        {
            throw new InvalidOperationException(
                "ФИАС вернул некорректный URL архива.");
        }

        var fileName = Path.GetFileName(uri.LocalPath);

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return "gar_xml.zip";
        }

        return fileName;
    }

    private static FiasDownloadDto MapToDto(FiasDownload download)
    {
        return new FiasDownloadDto
        {
            Id = download.Id,
            VersionId = download.VersionId,
            DownloadUrl = download.DownloadUrl,
            FilePath = download.FilePath,
            FileSize = download.FileSize,
            Status = download.Status,
            ErrorMessage = download.ErrorMessage,
            CreatedAtUtc = download.CreatedAtUtc,
            CompletedAtUtc = download.CompletedAtUtc
        };
    }
}
