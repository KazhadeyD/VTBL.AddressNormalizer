using System;
using System.Text.Json.Serialization;

namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Одна indoor-категория в массиве <c>units</c>.
    /// </summary>
    public class IndoorMarkDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("values")]
        public string[] Values { get; set; } = Array.Empty<string>();
    }
}