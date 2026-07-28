using System.Text.Json.Serialization;

namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Ответ extract для building-части адреса.
    /// </summary>
    public class ExtractResponse
    {
        [JsonPropertyName("source")]
        public string Source { get; set; }

        [JsonPropertyName("extracted")]
        public string Extracted { get; set; }
    }
}