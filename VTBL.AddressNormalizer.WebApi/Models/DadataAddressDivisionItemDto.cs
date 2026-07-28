using System.Text.Json.Serialization;

namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Один компонент деления DaData.
    /// </summary>
    public class DadataAddressDivisionItemDto
    {
        [JsonPropertyName("fias_id")]
        public string FiasId { get; set; }

        [JsonPropertyName("kladr_id")]
        public string KladrId { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("type_full")]
        public string TypeFull { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("name_with_type")]
        public string NameWithType { get; set; }
    }
}