using System.Text.Json.Serialization;

namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Ответ нормализации indoor или unit-строки.
    /// </summary>
    public class UnitNormalizeResponse
    {
        [JsonPropertyName("source")]
        public string Source { get; set; }

        [JsonPropertyName("indoorValue")]
        public IndoorValueDto IndoorValue { get; set; }

        [JsonPropertyName("canonical")]
        public string Canonical { get; set; }

        [JsonPropertyName("hash")]
        public string Hash { get; set; }
    }
}