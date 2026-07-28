using System.Text.Json.Serialization;

namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Результат для buildingValue: extracted, normalizedAddress, hash и payload DaData.
    /// </summary>
    public class DadataOutdoorDto
    {
        [JsonPropertyName("extracted")]
        public string Extracted { get; set; }

        [JsonPropertyName("normalizedAddress")]
        public string NormalizedAddress { get; set; }

        [JsonPropertyName("hash")]
        public string Hash { get; set; }

        /// <summary>
        /// Заполняется по правилу: suggest.house_fias_id, затем clean.house_fias_id.
        /// </summary>
        [JsonPropertyName("fiasId")]
        public string FiasId { get; set; }

        [JsonPropertyName("suggest")]
        public DadataSuggestAddressDto Suggest { get; set; }

        [JsonPropertyName("clean")]
        public DadataCleanAddressDto Clean { get; set; }
    }
}