namespace Fias.Application.DTO;

public sealed class FiasSchemaDdlDto
{
    public string FileName { get; set; } = string.Empty;

    public string TableName { get; set; } = string.Empty;

    public string Ddl { get; set; } = string.Empty;

    public IReadOnlyCollection<FiasSchemaColumnDto> Columns { get; set; } = [];
}

public sealed class FiasSchemaColumnDto
{
    public string Name { get; set; } = string.Empty;

    public string SqlType { get; set; } = string.Empty;

    public bool IsRequired { get; set; }
}
