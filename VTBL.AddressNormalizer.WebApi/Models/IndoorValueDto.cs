using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Структурированное indoorValue: extracted, hash канона и sparse-массив units.
    /// </summary>
    public class IndoorValueDto
    {
        [JsonPropertyName("extracted")]
        public string Extracted { get; set; }

        [JsonPropertyName("hash")]
        public string Hash { get; set; }

        [JsonPropertyName("units")]
        public IList<IndoorMarkDto> Units { get; set; } = new List<IndoorMarkDto>();
    }
}