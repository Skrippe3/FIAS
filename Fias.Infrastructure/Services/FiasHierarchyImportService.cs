using System.Globalization;
using System.Xml;
using Fias.Application.Interfaces;
using Fias.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Fias.Infrastructure.Services;

public sealed class FiasHierarchyImportService : IFiasHierarchyImportService
{
    private const int FileBufferSize = 1024 * 1024;

    private readonly FiasDbContext _context;
    private readonly ILogger<FiasHierarchyImportService> _logger;

    public FiasHierarchyImportService(
        FiasDbContext context,
        ILogger<FiasHierarchyImportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<long> ImportDirectoryAsync(
        string directoryPath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            throw new ArgumentException(
                "Не указан каталог XML.",
                nameof(directoryPath));
        }

        if (!Directory.Exists(directoryPath))
        {
            throw new DirectoryNotFoundException(
                $"Каталог не найден: {directoryPath}");
        }

        var files = Directory
            .EnumerateFiles(
                directoryPath,
                "AS_ADM_HIERARCHY_*.XML",
                SearchOption.AllDirectories)
            .OrderBy(path => path)
            .ToArray();

        if (files.Length == 0)
        {
            throw new FileNotFoundException(
                "Файлы AS_ADM_HIERARCHY_*.XML не найдены.",
                directoryPath);
        }

        var connectionString = _context.Database.GetConnectionString();

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Не удалось получить строку подключения PostgreSQL.");
        }

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        _logger.LogInformation(
            "Начинается импорт AS_ADM_HIERARCHY. " +
            "Каталог: {Directory}. Найдено файлов: {Count}.",
            directoryPath,
            files.Length);

        long totalUpdated = 0;

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation(
                "Иерархия: обрабатывается {File}.",
                Path.GetFileName(file));

            totalUpdated += await ImportFileAsync(
                connection,
                file,
                cancellationToken);
        }

        _logger.LogInformation(
            "Импорт AS_ADM_HIERARCHY завершён. Обновлено объектов: {Count:N0}.",
            totalUpdated);

        return totalUpdated;
    }

    private async Task<long> ImportFileAsync(
        NpgsqlConnection connection,
        string file,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            await connection.BeginTransactionAsync(cancellationToken);

        await using (var createTemp = new NpgsqlCommand(
            """
            CREATE TEMP TABLE tmp_hierarchy
            (
                object_id bigint NOT NULL,
                parent_object_id bigint,
                path character varying(500)
            ) ON COMMIT DROP;
            """,
            connection,
            transaction))
        {
            await createTemp.ExecuteNonQueryAsync(cancellationToken);
        }

        long parsed;

        var settings = new XmlReaderSettings
        {
            Async = true,
            IgnoreComments = true,
            IgnoreWhitespace = true,
            DtdProcessing = DtdProcessing.Prohibit,
            CloseInput = true
        };

        await using (var stream = new FileStream(
            file,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            FileBufferSize,
            FileOptions.Asynchronous | FileOptions.SequentialScan))
        using (var reader = XmlReader.Create(stream, settings))
        {
            await using var importer =
                await connection.BeginBinaryImportAsync(
                    "COPY tmp_hierarchy (object_id, parent_object_id, path) " +
                    "FROM STDIN (FORMAT BINARY)",
                    cancellationToken);

            parsed = await CopyItemsAsync(importer, reader, cancellationToken);

            await importer.CompleteAsync(cancellationToken);
        }

        if (parsed == 0)
        {
            await transaction.RollbackAsync(cancellationToken);
            return 0;
        }

        await using (var update = new NpgsqlCommand(
            """
            UPDATE fias_address_objects a
            SET parent_object_id = t.parent_object_id,
                path = t.path
            FROM (
                SELECT DISTINCT ON (object_id)
                    object_id, parent_object_id, path
                FROM tmp_hierarchy
                ORDER BY object_id
            ) t
            WHERE a.object_id = t.object_id;
            """,
            connection,
            transaction))
        {
            update.CommandTimeout = 0;

            var updated = await update.ExecuteNonQueryAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Иерархия {File}: прочитано {Parsed:N0}, обновлено {Updated:N0}.",
                Path.GetFileName(file),
                parsed,
                updated);

            return updated;
        }
    }

    private async Task<long> CopyItemsAsync(
        NpgsqlBinaryImporter importer,
        XmlReader reader,
        CancellationToken cancellationToken)
    {
        long count = 0;

        while (await reader.ReadAsync())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (reader.NodeType != XmlNodeType.Element ||
                !string.Equals(
                    reader.LocalName,
                    "ITEM",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (reader.GetAttribute("ISACTIVE") != "1")
            {
                continue;
            }

            if (!long.TryParse(
                    reader.GetAttribute("OBJECTID"),
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out var objectId))
            {
                continue;
            }

            var parentText = reader.GetAttribute("PARENTOBJID");
            var path = reader.GetAttribute("PATH");

            await importer.StartRowAsync(cancellationToken);

            await importer.WriteAsync(
                objectId, NpgsqlDbType.Bigint, cancellationToken);

            if (long.TryParse(
                    parentText,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out var parentObjectId) &&
                parentObjectId > 0)
            {
                await importer.WriteAsync(
                    parentObjectId, NpgsqlDbType.Bigint, cancellationToken);
            }
            else
            {
                await importer.WriteNullAsync(cancellationToken);
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                await importer.WriteNullAsync(cancellationToken);
            }
            else
            {
                await importer.WriteAsync(
                    path, NpgsqlDbType.Varchar, cancellationToken);
            }

            count++;

            if (count % 500_000 == 0)
            {
                _logger.LogInformation(
                    "Иерархия: прочитано {Count:N0} связей.",
                    count);
            }
        }

        return count;
    }
}
