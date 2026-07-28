using System.Text.Json.Serialization;

namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Одна станция метро из ответа DaData.
    /// </summary>
    public class DadataMetroDto
    {
        [JsonPropertyName("distance")]
        public double? Distance { get; set; }

        [JsonPropertyName("line")]
        public string Line { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}