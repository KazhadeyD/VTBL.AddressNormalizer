using System.Text.Json.Serialization;

namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Value полной нормализации: buildingValue и indoorValue.
    /// </summary>
    public class NormalizeValueDto
    {
        /// <summary>
        /// Результат нормализации outdoor-части с payload DaData.
        /// </summary>
        [JsonPropertyName("buildingValue")]
        public DadataOutdoorDto BuildingValue { get; set; }

        /// <summary>
        /// Результат indoor-нормализации: extracted, hash и sparse-массив units.
        /// </summary>
        [JsonPropertyName("indoorValue")]
        public IndoorValueDto IndoorValue { get; set; }
    }
}