using Fias.Application.Interfaces;
using Fias.Domain.Constants;
using Fias.Domain.Entities;
using Fias.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fias.Infrastructure.Services;

public sealed class FiasArchiveDownloadProcessor
    : IFiasArchiveDownloadProcessor
{
    private const int BufferSize = 1024 * 1024;

    private readonly FiasDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly ILogger<FiasArchiveDownloadProcessor> _logger;

    public FiasArchiveDownloadProcessor(
        FiasDbContext context,
        HttpClient httpClient,
        ILogger<FiasArchiveDownloadProcessor> logger)
    {
        _context = context;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> ProcessNextPendingAsync(
        CancellationToken cancellationToken = default)
    {
        var download = await _context.FiasDownloads
            .Where(x => x.Status == FiasDownloadStatuses.Pending)
            .OrderBy(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (download is null)
        {
            return false;
        }

        download.Status = FiasDownloadStatuses.Running;
        download.ErrorMessage = null;

        await _context.SaveChangesAsync(cancellationToken);

        try
        {
            await DownloadFileAsync(
                download,
                cancellationToken);

            download.Status =
                FiasDownloadStatuses.Completed;

            download.CompletedAtUtc =
                DateTime.UtcNow;

            download.ErrorMessage = null;

            await _context.SaveChangesAsync(
                cancellationToken);

            _logger.LogInformation(
                "Архив ФИАС версии {VersionId} скачан. Файл {FilePath}. Размер {Size} байт",
                download.VersionId,
                download.FilePath,
                download.FileSize);

            return true;
        }

        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            download.Status =
                FiasDownloadStatuses.Pending;

            download.ErrorMessage =
                "Скачивание остановлено";

            await SaveChangesWithoutCancellationAsync();

            throw;
        }

        catch (Exception exception)
        {
            download.Status =
                FiasDownloadStatuses.Failed;

            download.ErrorMessage =
                LimitErrorMessage(
                    exception.ToString());

            download.CompletedAtUtc =
                DateTime.UtcNow;

            await SaveChangesWithoutCancellationAsync();

            _logger.LogError(
                exception,
                "Ошибка скачивания версии {VersionId}",
                download.VersionId);

            return true;
        }
    }

    private async Task DownloadFileAsync(
        FiasDownload download,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(download.DownloadUrl))
            throw new InvalidOperationException(
                "Отсутствует URL");

        if (string.IsNullOrWhiteSpace(download.FilePath))
            throw new InvalidOperationException(
                "Отсутствует путь файла");

        var directory =
            Path.GetDirectoryName(download.FilePath);

        if (string.IsNullOrWhiteSpace(directory))
            throw new InvalidOperationException(
                "Не найден каталог");

        Directory.CreateDirectory(directory);

        var tempFile =
            download.FilePath + ".part";

        long existingBytes = 0;

        if (File.Exists(tempFile))
        {
            existingBytes =
                new FileInfo(tempFile).Length;
        }

        _logger.LogInformation(
            "Начало загрузки версии {VersionId}. Уже есть {MB} МБ",
            download.VersionId,
            existingBytes / 1024 / 1024);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                download.DownloadUrl);

        if (existingBytes > 0)
        {
            request.Headers.Range =
                new System.Net.Http.Headers.RangeHeaderValue(
                    existingBytes,
                    null);
        }

        using var response =
            await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

        response.EnsureSuccessStatusCode();

        var contentLength =
            response.Content.Headers.ContentLength;

        long totalBytes =
            existingBytes +
            (contentLength ?? 0);

        await using var source =
            await response.Content
                .ReadAsStreamAsync(
                    cancellationToken);

        await using var target =
            new FileStream(
                tempFile,
                FileMode.OpenOrCreate,
                FileAccess.Write,
                FileShare.None,
                BufferSize,
                FileOptions.Asynchronous |
                FileOptions.SequentialScan);

        target.Seek(
            existingBytes,
            SeekOrigin.Begin);

        var buffer =
            new byte[BufferSize];

        long downloaded =
            existingBytes;

        int lastPercent = -1;

        while (true)
        {
            var read =
                await source.ReadAsync(
                    buffer,
                    cancellationToken);

            if (read == 0)
                break;

            await target.WriteAsync(
                buffer.AsMemory(0, read),
                cancellationToken);

            downloaded += read;

            LogProgress(
                download.VersionId,
                downloaded,
                totalBytes,
                ref lastPercent);
        }

        await target.FlushAsync(
            cancellationToken);

        download.FileSize =
            downloaded;

        if (File.Exists(download.FilePath))
        {
            File.Delete(download.FilePath);
        }

        File.Move(
            tempFile,
            download.FilePath,
            true);
    }

    private void LogProgress(
        int versionId,
        long downloadedBytes,
        long totalBytes,
        ref int lastPercent)
    {
        if (totalBytes <= 0)
            return;

        var percent =
            (int)(
                downloadedBytes * 100 /
                totalBytes);

        if (percent < lastPercent + 5 &&
            percent != 100)
        {
            return;
        }

        lastPercent =
            percent;

        _logger.LogInformation(
            "Скачивание версии {VersionId}: {Percent}% ({MB}/{Total} МБ)",
            versionId,
            percent,
            downloadedBytes / 1024 / 1024,
            totalBytes / 1024 / 1024);
    }

    private async Task SaveChangesWithoutCancellationAsync()
    {
        try
        {
            await _context.SaveChangesAsync(
                CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Ошибка сохранения статуса");
        }
    }

    private static string LimitErrorMessage(
        string message)
    {
        const int maxLength = 4000;

        return message.Length <= maxLength
            ? message
            : message[..maxLength];
    }
}
