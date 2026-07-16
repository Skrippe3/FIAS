using System.IO.Compression;
using Fias.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Fias.Infrastructure.Services;

public sealed class FiasArchiveExtractService
    : IFiasArchiveExtractService
{
    private readonly ILogger<FiasArchiveExtractService> _logger;

    public FiasArchiveExtractService(
        ILogger<FiasArchiveExtractService> logger)
    {
        _logger = logger;
    }

    public async Task ExtractAsync(
        string archivePath,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(archivePath))
        {
            throw new FileNotFoundException(
                "Архив ФИАС не найден",
                archivePath);
        }

        var directory =
            Path.GetDirectoryName(archivePath);

        if (directory == null)
        {
            throw new InvalidOperationException(
                "Не найден каталог архива");
        }

        var extractDirectory =
            Path.Combine(
                directory,
                "xml");

        Directory.CreateDirectory(
            extractDirectory);

        _logger.LogInformation(
            "Распаковка архива {Archive}",
            archivePath);

        await Task.Run(() =>
        {
            ZipFile.ExtractToDirectory(
                archivePath,
                extractDirectory,
                true);

        }, cancellationToken);

        _logger.LogInformation(
            "Архив распакован: {Directory}",
            extractDirectory);
    }
}
