using System.Text.Json.Serialization;

namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Ответ canonicalize для building-адреса.
    /// </summary>
    public class CanonicalizeResponse
    {
        [JsonPropertyName("source")]
        public string Source { get; set; }

        [JsonPropertyName("canonical")]
        public string Canonical { get; set; }
    }
}