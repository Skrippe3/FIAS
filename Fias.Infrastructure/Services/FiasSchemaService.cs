using System.Security.Cryptography;
using System.Xml.Linq;
using Fias.Application.DTO;
using Fias.Application.Interfaces;
using Fias.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fias.Infrastructure.Services;

public sealed class FiasSchemaService : IFiasSchemaService
{
    private static readonly XNamespace Xs =
        "http://www.w3.org/2001/XMLSchema";

    private static readonly IReadOnlyDictionary<string, string[]> ExpectedAttributes =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["AS_ADDR_OBJ"] = ["ID", "OBJECTID", "OBJECTGUID", "NAME", "TYPENAME", "LEVEL", "ISACTIVE", "ISACTUAL"],
            ["AS_REESTR_OBJECTS"] = ["OBJECTID", "OBJECTGUID", "CHANGEID", "ISACTIVE", "LEVELID"],
        };

    private readonly FiasOptions _options;
    private readonly ILogger<FiasSchemaService> _logger;

    public FiasSchemaService(
        IOptions<FiasOptions> options,
        ILogger<FiasSchemaService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<FiasSchemaCheckResultDto> CheckSchemaAsync(
        string? xsdDirectory,
        CancellationToken cancellationToken = default)
    {
        var directory = string.IsNullOrWhiteSpace(xsdDirectory)
            ? _options.XsdPath
            : xsdDirectory;

        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new InvalidOperationException(
                "Не указан каталог XSD (параметр Fias:XsdPath).");
        }

        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException(
                $"Каталог XSD не найден: {directory}");
        }

        var files = Directory
            .EnumerateFiles(directory, "*.xsd", SearchOption.AllDirectories)
            .OrderBy(path => path)
            .ToArray();

        if (files.Length == 0)
        {
            throw new FileNotFoundException(
                "XSD-файлы не найдены.",
                directory);
        }

        var results = new List<FiasSchemaFileCheckDto>(files.Length);

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            results.Add(await CheckFileAsync(file, cancellationToken));
        }

        var known = results.Where(x => x.IsKnown).ToList();

        var isCompatible = known.Count > 0 && known.All(x => x.IsCompatible);

        _logger.LogInformation(
            "Проверка XSD завершена. Файлов: {Total}, известных: {Known}, " +
            "совместимо: {Compatible}.",
            results.Count,
            known.Count,
            isCompatible);

        return new FiasSchemaCheckResultDto
        {
            IsCompatible = isCompatible,
            Files = results
        };
    }

    private async Task<FiasSchemaFileCheckDto> CheckFileAsync(
        string file,
        CancellationToken cancellationToken)
    {
        var fileName = Path.GetFileName(file);

        var bytes = await File.ReadAllBytesAsync(file, cancellationToken);
        var hash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

        var expectedKey = ExpectedAttributes.Keys
            .FirstOrDefault(key =>
                fileName.Contains(key, StringComparison.OrdinalIgnoreCase));

        if (expectedKey is null)
        {
            return new FiasSchemaFileCheckDto
            {
                FileName = fileName,
                Hash = hash,
                IsKnown = false,
                IsCompatible = true
            };
        }

        var actualAttributes = ReadAttributeNames(bytes);
        var expected = ExpectedAttributes[expectedKey];

        var missing = expected
            .Where(attr => !actualAttributes.Contains(attr))
            .ToArray();

        var newAttributes = actualAttributes
            .Where(attr => !expected.Contains(attr, StringComparer.OrdinalIgnoreCase))
            .OrderBy(x => x)
            .ToArray();

        return new FiasSchemaFileCheckDto
        {
            FileName = fileName,
            Hash = hash,
            IsKnown = true,
            IsCompatible = missing.Length == 0,
            MissingAttributes = missing,
            NewAttributes = newAttributes
        };
    }

    public async Task<FiasSchemaDdlDto> GenerateTableDdlAsync(
        string xsdFile,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(xsdFile))
        {
            throw new ArgumentException(
                "Не указан путь к XSD-файлу.",
                nameof(xsdFile));
        }

        if (!File.Exists(xsdFile))
        {
            throw new FileNotFoundException("XSD-файл не найден.", xsdFile);
        }

        var bytes = await File.ReadAllBytesAsync(xsdFile, cancellationToken);

        using var stream = new MemoryStream(bytes);
        var document = XDocument.Load(stream);

        var columns = document
            .Descendants(Xs + "attribute")
            .Where(attr => attr.Attribute("name") is not null)
            .Select(MapColumn)
            .ToList();

        if (columns.Count == 0)
        {
            throw new InvalidOperationException(
                "В XSD-схеме не найдено ни одного атрибута.");
        }

        var tableName = BuildTableName(xsdFile);
        var ddl = BuildCreateTable(tableName, columns);

        _logger.LogInformation(
            "Сгенерирован DDL по схеме {File}: таблица {Table}, колонок {Count}.",
            Path.GetFileName(xsdFile),
            tableName,
            columns.Count);

        return new FiasSchemaDdlDto
        {
            FileName = Path.GetFileName(xsdFile),
            TableName = tableName,
            Ddl = ddl,
            Columns = columns
        };
    }

    private static FiasSchemaColumnDto MapColumn(XElement attribute)
    {
        var name = attribute.Attribute("name")!.Value;
        var isRequired = attribute.Attribute("use")?.Value == "required";

        var restriction = attribute
            .Descendants(Xs + "restriction")
            .FirstOrDefault();

        var xsdType =
            attribute.Attribute("type")?.Value
            ?? restriction?.Attribute("base")?.Value
            ?? "xs:string";

        int? maxLength = null;

        var maxLengthValue = restriction?
            .Elements(Xs + "maxLength")
            .FirstOrDefault()?
            .Attribute("value")?.Value;

        if (int.TryParse(maxLengthValue, out var parsed))
        {
            maxLength = parsed;
        }

        return new FiasSchemaColumnDto
        {
            Name = ToSnakeCase(name),
            SqlType = MapSqlType(xsdType, maxLength),
            IsRequired = isRequired
        };
    }

    private static string MapSqlType(string xsdType, int? maxLength)
    {
        var type = xsdType.ToLowerInvariant();

        if (type.Contains("long"))
            return "bigint";
        if (type.Contains("int"))
            return "integer";
        if (type.Contains("boolean"))
            return "boolean";
        if (type.Contains("datetime"))
            return "timestamp";
        if (type.Contains("date"))
            return "date";
        if (type.Contains("decimal") || type.Contains("double") || type.Contains("float"))
            return "numeric";

        return maxLength is > 0 ? $"character varying({maxLength})" : "text";
    }

    private static string BuildCreateTable(
        string tableName,
        IReadOnlyList<FiasSchemaColumnDto> columns)
    {
        var lines = columns.Select(column =>
            $"    {column.Name} {column.SqlType}" +
            (column.IsRequired ? " NOT NULL" : string.Empty));

        return $"CREATE TABLE {tableName}\n(\n"
            + string.Join(",\n", lines)
            + "\n);";
    }

    private static string BuildTableName(string xsdFile)
    {
        var raw = Path.GetFileNameWithoutExtension(xsdFile);

        var cleaned = new string(raw
            .TakeWhile(ch => !char.IsDigit(ch) || ch == '_')
            .ToArray())
            .Trim('_');

        if (string.IsNullOrWhiteSpace(cleaned))
        {
            cleaned = raw;
        }

        return "gar_" + ToSnakeCase(cleaned);
    }

    private static string ToSnakeCase(string value)
    {
        var lowered = value.Trim().ToLowerInvariant();

        var chars = lowered
            .Select(ch => char.IsLetterOrDigit(ch) ? ch : '_')
            .ToArray();

        return new string(chars).Trim('_');
    }

    private static HashSet<string> ReadAttributeNames(byte[] xsdContent)
    {
        using var stream = new MemoryStream(xsdContent);

        var document = XDocument.Load(stream);

        var names = document
            .Descendants(Xs + "attribute")
            .Select(element => element.Attribute("name")?.Value)
            .Where(name => !string.IsNullOrEmpty(name))
            .Select(name => name!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return names;
    }
}
