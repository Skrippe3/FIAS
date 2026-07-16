using System.Globalization;
using System.Xml;
using Fias.Application.Interfaces;
using Fias.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Fias.Infrastructure.Services;

public sealed class FiasReestrObjectImportService
    : IFiasReestrObjectImportService
{
    private const int FileBufferSize = 1024 * 1024;

    private readonly FiasDbContext _context;
    private readonly ILogger<FiasReestrObjectImportService> _logger;

    public FiasReestrObjectImportService(
        FiasDbContext context,
        ILogger<FiasReestrObjectImportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<long> ImportDirectoryAsync(
        string directoryPath,
        bool clearTableBeforeImport,
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
                "AS_REESTR_OBJECTS_*.XML",
                SearchOption.AllDirectories)
            .OrderBy(path => path)
            .ToArray();

        if (files.Length == 0)
        {
            throw new FileNotFoundException(
                "Файлы AS_REESTR_OBJECTS_*.XML не найдены.",
                directoryPath);
        }

        var connectionString =
            _context.Database.GetConnectionString();

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Не удалось получить строку подключения PostgreSQL.");
        }

        await using var connection =
            new NpgsqlConnection(connectionString);

        await connection.OpenAsync(cancellationToken);

        if (clearTableBeforeImport)
        {
            await ClearImportDataAsync(
                connection,
                cancellationToken);
        }

        _logger.LogInformation(
            "Начинается импорт AS_REESTR_OBJECTS. " +
            "Каталог: {Directory}. Найдено файлов: {Count}.",
            directoryPath,
            files.Length);

        long totalImported = 0;

        foreach (var filePath in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var relativePath = Path.GetRelativePath(
                directoryPath,
                filePath);

            if (await IsFileImportedAsync(
                    relativePath,
                    connection,
                    cancellationToken))
            {
                _logger.LogInformation(
                    "Файл уже импортирован, пропускаем: {File}.",
                    relativePath);

                continue;
            }

            var regionCode = GetRegionCode(filePath);

            _logger.LogInformation(
                "Начинается импорт файла {File}. Регион: {RegionCode}.",
                relativePath,
                regionCode ?? "не определён");

            var importedCount = await ImportFileAsync(
                connection,
                filePath,
                regionCode,
                cancellationToken);

            await SaveImportLogAsync(
                relativePath,
                connection,
                cancellationToken);

            totalImported += importedCount;

            _logger.LogInformation(
                "Файл {File} импортирован. " +
                "Записей: {Count:N0}. Всего за запуск: {Total:N0}.",
                relativePath,
                importedCount,
                totalImported);
        }

        _logger.LogInformation(
            "Импорт AS_REESTR_OBJECTS завершён. " +
            "Всего за этот запуск загружено {Count:N0} записей.",
            totalImported);

        return totalImported;
    }

    private static async Task ClearImportDataAsync(
        NpgsqlConnection connection,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await using (var clearObjectsCommand =
                new NpgsqlCommand(
                    "TRUNCATE TABLE fias_objects;",
                    connection,
                    transaction))
            {
                await clearObjectsCommand.ExecuteNonQueryAsync(
                    cancellationToken);
            }

            await using (var clearLogsCommand =
                new NpgsqlCommand(
                    "TRUNCATE TABLE fias_import_logs RESTART IDENTITY;",
                    connection,
                    transaction))
            {
                await clearLogsCommand.ExecuteNonQueryAsync(
                    cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(
                CancellationToken.None);

            throw;
        }
    }

    private static async Task<bool> IsFileImportedAsync(
        string filePath,
        NpgsqlConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(
            """
            SELECT EXISTS
            (
                SELECT 1
                FROM fias_import_logs
                WHERE file_name = @file_name
            );
            """,
            connection);

        command.Parameters.AddWithValue(
            "file_name",
            NpgsqlDbType.Varchar,
            filePath);

        var result = await command.ExecuteScalarAsync(
            cancellationToken);

        return result is true;
    }

    private static async Task SaveImportLogAsync(
        string filePath,
        NpgsqlConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(
            """
            INSERT INTO fias_import_logs
            (
                file_name,
                imported_at_utc
            )
            VALUES
            (
                @file_name,
                CURRENT_TIMESTAMP
            )
            ON CONFLICT (file_name)
            DO NOTHING;
            """,
            connection);

        command.Parameters.AddWithValue(
            "file_name",
            NpgsqlDbType.Varchar,
            filePath);

        await command.ExecuteNonQueryAsync(
            cancellationToken);
    }

    private async Task<long> ImportFileAsync(
        NpgsqlConnection connection,
        string filePath,
        string? regionCode,
        CancellationToken cancellationToken)
    {
        var settings = new XmlReaderSettings
        {
            Async = true,
            IgnoreComments = true,
            IgnoreWhitespace = true,
            DtdProcessing = DtdProcessing.Prohibit,
            CloseInput = true
        };

        await using var stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            FileBufferSize,
            FileOptions.Asynchronous |
            FileOptions.SequentialScan);

        using var reader = XmlReader.Create(
            stream,
            settings);

        await using var importer =
            await connection.BeginBinaryImportAsync(
                """
                COPY fias_objects
                (
                    object_id,
                    object_guid,
                    change_id,
                    is_active,
                    level_id,
                    create_date,
                    update_date,
                    region_code
                )
                FROM STDIN (FORMAT BINARY)
                """,
                cancellationToken);

        long importedCount = 0;

        while (await reader.ReadAsync())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (reader.NodeType != XmlNodeType.Element ||
                !string.Equals(
                    reader.LocalName,
                    "OBJECT",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var objectId = ParseRequiredLong(
                reader.GetAttribute("OBJECTID"),
                "OBJECTID",
                filePath);

            var objectGuid = ParseRequiredGuid(
                reader.GetAttribute("OBJECTGUID"),
                "OBJECTGUID",
                filePath);

            var changeId = ParseRequiredLong(
                reader.GetAttribute("CHANGEID"),
                "CHANGEID",
                filePath);

            var isActive =
                reader.GetAttribute("ISACTIVE") == "1";

            var levelId = ParseRequiredInt(
                reader.GetAttribute("LEVELID"),
                "LEVELID",
                filePath);

            var createDate = ParseNullableDate(
                reader.GetAttribute("CREATEDATE"));

            var updateDate = ParseNullableDate(
                reader.GetAttribute("UPDATEDATE"));

            await importer.StartRowAsync(
                cancellationToken);

            await importer.WriteAsync(
                objectId,
                NpgsqlDbType.Bigint,
                cancellationToken);

            await importer.WriteAsync(
                objectGuid,
                NpgsqlDbType.Uuid,
                cancellationToken);

            await importer.WriteAsync(
                changeId,
                NpgsqlDbType.Bigint,
                cancellationToken);

            await importer.WriteAsync(
                isActive,
                NpgsqlDbType.Boolean,
                cancellationToken);

            await importer.WriteAsync(
                levelId,
                NpgsqlDbType.Integer,
                cancellationToken);

            await WriteNullableDateAsync(
                importer,
                createDate,
                cancellationToken);

            await WriteNullableDateAsync(
                importer,
                updateDate,
                cancellationToken);

            if (string.IsNullOrWhiteSpace(regionCode))
            {
                await importer.WriteNullAsync(
                    cancellationToken);
            }
            else
            {
                await importer.WriteAsync(
                    regionCode,
                    NpgsqlDbType.Varchar,
                    cancellationToken);
            }

            importedCount++;

            if (importedCount % 100_000 == 0)
            {
                _logger.LogInformation(
                    "Файл {File}: обработано {Count:N0} записей.",
                    Path.GetFileName(filePath),
                    importedCount);
            }
        }

        await importer.CompleteAsync(
            cancellationToken);

        return importedCount;
    }

    private static async Task WriteNullableDateAsync(
        NpgsqlBinaryImporter importer,
        DateOnly? value,
        CancellationToken cancellationToken)
    {
        if (value.HasValue)
        {
            await importer.WriteAsync(
                value.Value,
                NpgsqlDbType.Date,
                cancellationToken);
        }
        else
        {
            await importer.WriteNullAsync(
                cancellationToken);
        }
    }

    private static string? GetRegionCode(
        string filePath)
    {
        var directory = Directory.GetParent(filePath);

        while (directory is not null)
        {
            var name = directory.Name;

            if (name.Length == 2 &&
                int.TryParse(
                    name,
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out _))
            {
                return name;
            }

            directory = directory.Parent;
        }

        return null;
    }

    private static long ParseRequiredLong(
        string? value,
        string attributeName,
        string filePath)
    {
        if (long.TryParse(
            value,
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out var result))
        {
            return result;
        }

        throw CreateInvalidAttributeException(
            attributeName,
            value,
            filePath);
    }

    private static int ParseRequiredInt(
        string? value,
        string attributeName,
        string filePath)
    {
        if (int.TryParse(
            value,
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out var result))
        {
            return result;
        }

        throw CreateInvalidAttributeException(
            attributeName,
            value,
            filePath);
    }

    private static Guid ParseRequiredGuid(
        string? value,
        string attributeName,
        string filePath)
    {
        if (Guid.TryParse(
            value,
            out var result))
        {
            return result;
        }

        throw CreateInvalidAttributeException(
            attributeName,
            value,
            filePath);
    }

    private static DateOnly? ParseNullableDate(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return DateOnly.TryParseExact(
            value,
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var result)
            ? result
            : null;
    }

    private static InvalidDataException
        CreateInvalidAttributeException(
            string attributeName,
            string? value,
            string filePath)
    {
        return new InvalidDataException(
            $"Некорректный атрибут " +
            $"{attributeName}='{value}' " +
            $"в файле '{filePath}'.");
    }
}
