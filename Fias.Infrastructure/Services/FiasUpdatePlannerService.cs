using System.Net.Http.Json;
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

public sealed class FiasUpdatePlannerService : IFiasUpdatePlannerService
{
    private readonly HttpClient _httpClient;
    private readonly FiasDbContext _context;
    private readonly FiasOptions _options;
    private readonly ILogger<FiasUpdatePlannerService> _logger;

    public FiasUpdatePlannerService(
        HttpClient httpClient,
        FiasDbContext context,
        IOptions<FiasOptions> options,
        ILogger<FiasUpdatePlannerService> logger)
    {
        _httpClient = httpClient;
        _context = context;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<FiasUpdatePlanDto> GetUpdatePlanAsync(
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.AllDownloadInfoUrl))
        {
            throw new InvalidOperationException(
                "Не указан параметр Fias:AllDownloadInfoUrl.");
        }

        var available = await _httpClient
            .GetFromJsonAsync<List<FiasDownloadInfoDto>>(
                _options.AllDownloadInfoUrl,
                cancellationToken)
            ?? [];

        var ordered = available
            .Where(x => x.VersionId > 0)
            .OrderBy(x => x.VersionId)
            .ToList();

        var latestAvailable = ordered.Count > 0
            ? ordered[^1].VersionId
            : (int?)null;

        var installed = await _context.FiasVersions
            .AsNoTracking()
            .OrderByDescending(x => x.VersionId)
            .Select(x => (int?)x.VersionId)
            .FirstOrDefaultAsync(cancellationToken);

        if (installed is null)
        {
            _logger.LogInformation(
                "Установленных версий нет — требуется полная загрузка.");

            return new FiasUpdatePlanDto
            {
                InstalledVersionId = null,
                LatestAvailableVersionId = latestAvailable,
                RequiresFullImport = true,
                IsUpToDate = false,
                Steps = []
            };
        }

        var steps = ordered
            .Where(x => x.VersionId > installed.Value)
            .Select(x => new FiasUpdateStepDto
            {
                VersionId = x.VersionId,
                TextVersion = x.TextVersion,
                GarXmlDeltaUrl = x.GarXmlDeltaUrl
            })
            .ToList();

        _logger.LogInformation(
            "Установлена версия {Installed}. Доступно обновлений: {Count}.",
            installed.Value,
            steps.Count);

        return new FiasUpdatePlanDto
        {
            InstalledVersionId = installed.Value,
            LatestAvailableVersionId = latestAvailable,
            RequiresFullImport = false,
            IsUpToDate = steps.Count == 0,
            Steps = steps
        };
    }

    public async Task<int> QueuePendingUpdatesAsync(
        CancellationToken cancellationToken = default)
    {
        var plan = await GetUpdatePlanAsync(cancellationToken);

        if (plan.RequiresFullImport)
        {
            _logger.LogWarning(
                "Сначала нужна полная загрузка — дельты не ставятся в очередь.");

            return 0;
        }

        if (string.IsNullOrWhiteSpace(_options.StoragePath))
        {
            throw new InvalidOperationException(
                "Не указан параметр Fias:StoragePath.");
        }

        var queued = 0;

        foreach (var step in plan.Steps.OrderBy(x => x.VersionId))
        {
            if (string.IsNullOrWhiteSpace(step.GarXmlDeltaUrl))
            {
                _logger.LogWarning(
                    "У версии {VersionId} нет ссылки на дельту — пропуск.",
                    step.VersionId);

                continue;
            }

            var exists = await _context.FiasDownloads
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        x.VersionId == step.VersionId &&
                        x.DownloadUrl == step.GarXmlDeltaUrl &&
                        (x.Status == FiasDownloadStatuses.Pending ||
                         x.Status == FiasDownloadStatuses.Running ||
                         x.Status == FiasDownloadStatuses.Completed),
                    cancellationToken);

            if (exists)
            {
                continue;
            }

            var fileName = GetFileName(step.GarXmlDeltaUrl);

            var filePath = Path.Combine(
                _options.StoragePath,
                step.VersionId.ToString(),
                fileName);

            _context.FiasDownloads.Add(new FiasDownload
            {
                VersionId = step.VersionId,
                DownloadUrl = step.GarXmlDeltaUrl,
                FilePath = filePath,
                FileSize = 0,
                Status = FiasDownloadStatuses.Pending,
                CreatedAtUtc = DateTime.UtcNow
            });

            queued++;
        }

        if (queued > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Поставлено в очередь дельт: {Count}.",
                queued);
        }

        return queued;
    }

    private static string GetFileName(string downloadUrl)
    {
        if (Uri.TryCreate(downloadUrl, UriKind.Absolute, out var uri))
        {
            var fileName = Path.GetFileName(uri.LocalPath);

            if (!string.IsNullOrWhiteSpace(fileName))
            {
                return fileName;
            }
        }

        return "gar_delta_xml.zip";
    }
}
