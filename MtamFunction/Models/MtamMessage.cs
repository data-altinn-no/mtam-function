using System.Text.Json.Serialization;

namespace MtamFunction.Models;

[Serializable]
public class MtamSourceMessage
{
    [JsonPropertyName("identifikator")]
    public string? Id { get; set; }

    [JsonPropertyName("mottaker")]
    public string? Recipient { get; set; }

    [JsonPropertyName("meldingOmTildaenhet")]
    public string? Subject { get; set; }

    [JsonPropertyName("datoForMeldingTilAnnenMyndighet")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("meldingsinnholdTilAnnenMyndighet")]
    public MtamSourceMessageContent? MessageContent { get; set; }
}

[Serializable]
public class MtamSourceMessageContent
{
    [JsonPropertyName("meldingsType")]
    public string? MessageType { get; set; }

    [JsonPropertyName("fritekst")]
    public string? FreeText { get; set; }
}