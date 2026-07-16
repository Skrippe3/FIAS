using System.Text.Json.Serialization;

namespace Fias.Application.DTO;

public sealed class FiasDownloadInfoDto
{
    [JsonPropertyName("VersionId")]
    public int VersionId { get; set; }

    [JsonPropertyName("TextVersion")]
    public string? TextVersion { get; set; }

    [JsonPropertyName("GarXMLFullURL")]
    public string? GarXmlFullUrl { get; set; }

    [JsonPropertyName("GarXMLDeltaURL")]
    public string? GarXmlDeltaUrl { get; set; }
}
