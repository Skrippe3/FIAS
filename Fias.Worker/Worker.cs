using Fias.Application.Interfaces;
using Fias.Domain.Entities;
using Fias.Infrastructure.Options;
using Fias.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Fias.Worker;

public sealed class Worker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly FiasOptions _options;
    private readonly ILogger<Worker> _logger;
    private readonly TimeSpan _pollInterval;

    public Worker(
        IServiceScopeFactory scopeFactory,
        IOptions<FiasOptions> options,
        IConfiguration configuration,
        ILogger<Worker> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;

        var seconds = configuration.GetValue("Fias:PollIntervalSeconds", 30);
        _pollInterval = TimeSpan.FromSeconds(seconds <= 0 ? 30 : seconds);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Служба ФИАС запущена. Интервал опроса: {Interval}.",
            _pollInterval);

        await TryInitialImportAsync(stoppingToken);
        await TryHierarchyImportAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var didWork = await RunCycleAsync(stoppingToken);

                if (!didWork)
                {
                    await Task.Delay(_pollInterval, stoppingToken);
                }
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Ошибка в цикле службы ФИАС.");
                await Task.Delay(_pollInterval, stoppingToken);
            }
        }

        _logger.LogInformation("Служба ФИАС остановлена.");
    }

    private async Task TryInitialImportAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.InitialImportPath))
        {
            return;
        }

        if (!Directory.Exists(_options.InitialImportPath))
        {
            _logger.LogWarning(
                "Каталог первоначального импорта не найден: {Path}.",
                _options.InitialImportPath);

            return;
        }

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var orchestrator = scope.ServiceProvider
                .GetRequiredService<IFiasImportOrchestrator>();

            _logger.LogInformation(
                "Запуск первоначального импорта из {Path} (версия {Version}).",
                _options.InitialImportPath,
                _options.InitialImportVersionId);

            await orchestrator.ImportLocalDirectoryAsync(
                _options.InitialImportPath,
                _options.InitialImportVersionId,
                _options.InitialImportIncludeReestr,
                cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (FileNotFoundException)
        {
            _logger.LogInformation(
                "Файлы AS_ADDR_OBJ в {Path} не найдены — импорт адресов пропущен.",
                _options.InitialImportPath);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Ошибка первоначального импорта.");
        }
    }

    private async Task TryHierarchyImportAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.InitialImportPath) ||
            !Directory.Exists(_options.InitialImportPath))
        {
            return;
        }

        var marker = $"HIERARCHY_{_options.InitialImportVersionId}";

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var context = scope.ServiceProvider
                .GetRequiredService<FiasDbContext>();

            var alreadyDone = await context.FiasImportLogs
                .AsNoTracking()
                .AnyAsync(x => x.FileName == marker, cancellationToken);

            if (alreadyDone)
            {
                _logger.LogInformation(
                    "Иерархия версии {Version} уже импортирована — пропуск.",
                    _options.InitialImportVersionId);

                return;
            }

            var hierarchyImport = scope.ServiceProvider
                .GetRequiredService<IFiasHierarchyImportService>();

            _logger.LogInformation(
                "Запуск импорта иерархии из {Path}.",
                _options.InitialImportPath);

            await hierarchyImport.ImportDirectoryAsync(
                _options.InitialImportPath,
                cancellationToken);

            context.FiasImportLogs.Add(new FiasImportLog
            {
                FileName = marker,
                ImportedAtUtc = DateTime.UtcNow
            });

            await context.SaveChangesAsync(cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Ошибка импорта иерархии.");
        }
    }

    private async Task<bool> RunCycleAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();

        var downloadProcessor = scope.ServiceProvider
            .GetRequiredService<IFiasArchiveDownloadProcessor>();

        var importOrchestrator = scope.ServiceProvider
            .GetRequiredService<IFiasImportOrchestrator>();

        var downloaded = await downloadProcessor
            .ProcessNextPendingAsync(cancellationToken);

        var imported = await importOrchestrator
            .ProcessNextCompletedAsync(cancellationToken);

        if (downloaded || imported)
        {
            return true;
        }

        if (_options.AutoUpdateEnabled)
        {
            var planner = scope.ServiceProvider
                .GetRequiredService<IFiasUpdatePlannerService>();

            var queued = await planner.QueuePendingUpdatesAsync(cancellationToken);

            return queued > 0;
        }

        return false;
    }
}
