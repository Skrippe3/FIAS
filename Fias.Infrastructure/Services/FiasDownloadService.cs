using System.Net.Http.Json;
using Fias.Application.DTO;
using Fias.Application.Interfaces;
using Fias.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fias.Infrastructure.Services;

public sealed class FiasDownloadService : IFiasDownloadService
{
    private readonly HttpClient _httpClient;
    private readonly FiasOptions _options;
    private readonly ILogger<FiasDownloadService> _logger;

    public FiasDownloadService(
        HttpClient httpClient,
        IOptions<FiasOptions> options,
        ILogger<FiasDownloadService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<FiasDownloadInfoDto> GetLatestDownloadInfoAsync(
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.LastDownloadInfoUrl))
        {
            throw new InvalidOperationException(
                "Не указан параметр Fias:LastDownloadInfoUrl.");
        }

        _logger.LogInformation(
            "Запрос последней версии ФИАС по адресу {Url}.",
            _options.LastDownloadInfoUrl);

        using var response = await _httpClient.GetAsync(
            _options.LastDownloadInfoUrl,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        var responseText = await response.Content.ReadAsStringAsync(
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"ФИАС вернул HTTP {(int)response.StatusCode} " +
                $"({response.StatusCode}). Ответ: {responseText}");
        }

        FiasDownloadInfoDto? result;

        try
        {
            result = await response.Content
                .ReadFromJsonAsync<FiasDownloadInfoDto>(
                    cancellationToken: cancellationToken);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                $"Не удалось прочитать JSON ФИАС. Ответ: {responseText}",
                exception);
        }

        if (result is null)
        {
            throw new InvalidOperationException(
                "Сервис ФИАС вернул пустой ответ.");
        }

        if (result.VersionId <= 0)
        {
            throw new InvalidOperationException(
                $"ФИАС вернул некорректный VersionId. Ответ: {responseText}");
        }

        _logger.LogInformation(
            "Получена последняя версия ФИАС: {VersionId}, {TextVersion}.",
            result.VersionId,
            result.TextVersion);

        return result;
    }
}
