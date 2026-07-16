using System.Globalization;
using System.Xml;
using Fias.Application.Interfaces;
using Fias.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Fias.Infrastructure.Services;

public sealed class FiasAddressObjectImportService
    : IFiasAddressObjectImportService
{
    private const int FileBufferSize = 1024 * 1024;

    private const string ColumnList =
        "id, object_id, object_guid, parent_object_id, " +
        "name, type_name, level_id, is_active, region_code";

    private readonly FiasDbContext _context;
    private readonly ILogger<FiasAddressObjectImportService> _logger;

    public FiasAddressObjectImportService(
        FiasDbContext context,
        ILogger<FiasAddressObjectImportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<long> ImportDirectoryAsync(
        string directoryPath,
        bool clearTableBeforeImport,
        CancellationToken cancellationToken = default)
    {
        var files = GetAddressFiles(directoryPath);

        await using var connection = await OpenConnectionAsync(cancellationToken);

        if (clearTableBeforeImport)
        {
            await using var truncate = new NpgsqlCommand(
                "TRUNCATE TABLE fias_address_objects;",
                connection);

            await truncate.ExecuteNonQueryAsync(cancellationToken);
        }

        _logger.LogInformation(
            "Начинается импорт AS_ADDR_OBJ. " +
            "Каталог: {Directory}. Найдено файлов: {Count}.",
            directoryPath,
            files.Length);

        long total = 0;

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var regionCode = GetRegionCode(file);

            _logger.LogInformation(
                "Импорт {File}. Регион: {RegionCode}.",
                Path.GetFileName(file),
                regionCode ?? "не определён");

            total += await ImportFileAsync(
                connection,
                file,
                regionCode,
                cancellationToken);
        }

        _logger.LogInformation(
            "Импорт AS_ADDR_OBJ завершён. Всего загружено {Count:N0} записей.",
            total);

        return total;
    }

    public async Task<long> ApplyDeltaDirectoryAsync(
        string directoryPath,
        CancellationToken cancellationToken = default)
    {
        var files = GetAddressFiles(directoryPath);

        await using var connection = await OpenConnectionAsync(cancellationToken);

        _logger.LogInformation(
            "Применение дельты AS_ADDR_OBJ. " +
            "Каталог: {Directory}. Найдено файлов: {Count}.",
            directoryPath,
            files.Length);

        long total = 0;

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var regionCode = GetRegionCode(file);

            total += await ApplyDeltaFileAsync(
                connection,
                file,
                regionCode,
                cancellationToken);
        }

        _logger.LogInformation(
            "Дельта AS_ADDR_OBJ применена. Затронуто записей: {Count:N0}.",
            total);

        return total;
    }

    private async Task<long> ImportFileAsync(
        NpgsqlConnection connection,
        string file,
        string? regionCode,
        CancellationToken cancellationToken)
    {
        using var reader = CreateReader(file, out var stream);
        await using (stream)
        {
            await using var importer =
                await connection.BeginBinaryImportAsync(
                    $"COPY fias_address_objects ({ColumnList}) " +
                    "FROM STDIN (FORMAT BINARY)",
                    cancellationToken);

            var count = await CopyObjectsAsync(
                importer, reader, regionCode, file, cancellationToken);

            await importer.CompleteAsync(cancellationToken);

            return count;
        }
    }

    private async Task<long> ApplyDeltaFileAsync(
        NpgsqlConnection connection,
        string file,
        string? regionCode,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            await connection.BeginTransactionAsync(cancellationToken);

        await using (var createTemp = new NpgsqlCommand(
            "CREATE TEMP TABLE tmp_addr " +
            "(LIKE fias_address_objects INCLUDING DEFAULTS) ON COMMIT DROP;",
            connection,
            transaction))
        {
            await createTemp.ExecuteNonQueryAsync(cancellationToken);
        }

        long count;

        using (var reader = CreateReader(file, out var stream))
        await using (stream)
        {
            await using var importer =
                await connection.BeginBinaryImportAsync(
                    $"COPY tmp_addr ({ColumnList}) FROM STDIN (FORMAT BINARY)",
                    cancellationToken);

            count = await CopyObjectsAsync(
                importer, reader, regionCode, file, cancellationToken);

            await importer.CompleteAsync(cancellationToken);
        }

        if (count == 0)
        {
            await transaction.RollbackAsync(cancellationToken);
            return 0;
        }

        await using (var delete = new NpgsqlCommand(
            "DELETE FROM fias_address_objects a " +
            "USING tmp_addr t WHERE a.object_id = t.object_id;",
            connection,
            transaction))
        {
            await delete.ExecuteNonQueryAsync(cancellationToken);
        }

        await using (var insert = new NpgsqlCommand(
            $"INSERT INTO fias_address_objects ({ColumnList}) " +
            $"SELECT DISTINCT ON (object_id) {ColumnList} " +
            "FROM tmp_addr ORDER BY object_id, id DESC;",
            connection,
            transaction))
        {
            await insert.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);

        _logger.LogInformation(
            "Дельта {File}: применено {Count:N0} записей.",
            Path.GetFileName(file),
            count);

        return count;
    }

    private async Task<long> CopyObjectsAsync(
        NpgsqlBinaryImporter importer,
        XmlReader reader,
        string? regionCode,
        string file,
        CancellationToken cancellationToken)
    {
        long count = 0;

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

            if (reader.GetAttribute("ISACTUAL") != "1")
            {
                continue;
            }

            var id = ParseRequiredLong(reader.GetAttribute("ID"), "ID", file);
            var objectId = ParseRequiredLong(
                reader.GetAttribute("OBJECTID"), "OBJECTID", file);
            var objectGuid = ParseRequiredGuid(
                reader.GetAttribute("OBJECTGUID"), "OBJECTGUID", file);
            var name = reader.GetAttribute("NAME") ?? string.Empty;
            var typeName = reader.GetAttribute("TYPENAME") ?? string.Empty;
            var levelId = ParseRequiredInt(
                reader.GetAttribute("LEVEL"), "LEVEL", file);
            var isActive = reader.GetAttribute("ISACTIVE") == "1";

            await importer.StartRowAsync(cancellationToken);

            await importer.WriteAsync(id, NpgsqlDbType.Bigint, cancellationToken);
            await importer.WriteAsync(objectId, NpgsqlDbType.Bigint, cancellationToken);
            await importer.WriteAsync(objectGuid, NpgsqlDbType.Uuid, cancellationToken);

            await importer.WriteNullAsync(cancellationToken);

            await importer.WriteAsync(name, NpgsqlDbType.Varchar, cancellationToken);
            await importer.WriteAsync(typeName, NpgsqlDbType.Varchar, cancellationToken);
            await importer.WriteAsync(levelId, NpgsqlDbType.Integer, cancellationToken);
            await importer.WriteAsync(isActive, NpgsqlDbType.Boolean, cancellationToken);

            if (string.IsNullOrWhiteSpace(regionCode))
            {
                await importer.WriteNullAsync(cancellationToken);
            }
            else
            {
                await importer.WriteAsync(
                    regionCode, NpgsqlDbType.Varchar, cancellationToken);
            }

            count++;

            if (count % 100_000 == 0)
            {
                _logger.LogInformation(
                    "{File}: обработано {Count:N0} записей.",
                    Path.GetFileName(file),
                    count);
            }
        }

        return count;
    }

    private async Task<NpgsqlConnection> OpenConnectionAsync(
        CancellationToken cancellationToken)
    {
        var connectionString = _context.Database.GetConnectionString();

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Не удалось получить строку подключения PostgreSQL.");
        }

        var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        return connection;
    }

    private static XmlReader CreateReader(string file, out FileStream stream)
    {
        stream = new FileStream(
            file,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            FileBufferSize,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        var settings = new XmlReaderSettings
        {
            Async = true,
            IgnoreComments = true,
            IgnoreWhitespace = true,
            DtdProcessing = DtdProcessing.Prohibit,
            CloseInput = true
        };

        return XmlReader.Create(stream, settings);
    }

    private static string[] GetAddressFiles(string directoryPath)
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
                "AS_ADDR_OBJ_*.XML",
                SearchOption.AllDirectories)
            .Where(path =>
            {
                var name = Path.GetFileName(path);
                return !name.Contains("PARAMS", StringComparison.OrdinalIgnoreCase)
                    && !name.Contains("DIVISION", StringComparison.OrdinalIgnoreCase);
            })
            .OrderBy(path => path)
            .ToArray();

        if (files.Length == 0)
        {
            throw new FileNotFoundException(
                "Файлы AS_ADDR_OBJ_*.XML не найдены.",
                directoryPath);
        }

        return files;
    }

    private static string? GetRegionCode(string filePath)
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
        string? value, string attributeName, string filePath)
    {
        if (long.TryParse(
                value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var result))
        {
            return result;
        }

        throw CreateInvalidAttributeException(attributeName, value, filePath);
    }

    private static int ParseRequiredInt(
        string? value, string attributeName, string filePath)
    {
        if (int.TryParse(
                value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var result))
        {
            return result;
        }

        throw CreateInvalidAttributeException(attributeName, value, filePath);
    }

    private static Guid ParseRequiredGuid(
        string? value, string attributeName, string filePath)
    {
        if (Guid.TryParse(value, out var result))
        {
            return result;
        }

        throw CreateInvalidAttributeException(attributeName, value, filePath);
    }

    private static InvalidDataException CreateInvalidAttributeException(
        string attributeName, string? value, string filePath)
    {
        return new InvalidDataException(
            $"Некорректный атрибут {attributeName}='{value}' " +
            $"в файле '{filePath}'.");
    }
}
