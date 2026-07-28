using System.Text.Json.Serialization;

namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Результат полной нормализации адреса.
    /// </summary>
    public class NormalizeValueDto
    {
        /// <summary>
        /// Результат для building-части адреса: извлечённый фрагмент, нормализованный адрес, hash и данные DaData.
        /// </summary>
        [JsonPropertyName("buildingValue")]
        public BuildingValueDto BuildingValue { get; set; }

        /// <summary>
        /// Результат для indoor-части адреса: извлечённый фрагмент, hash и категории внутренней адресации.
        /// </summary>
        [JsonPropertyName("indoorValue")]
        public IndoorValueDto IndoorValue { get; set; }
    }
}