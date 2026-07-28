using System.Text.Json.Serialization;

namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Результат нормализации building-части адреса (<c>buildingValue</c>).
    /// </summary>
    public class BuildingValueDto
    {
        /// <summary>
        /// Фрагмент исходной строки, распознанный как building-часть адреса.
        /// </summary>
        [JsonPropertyName("extracted")]
        public string Extracted { get; set; }

        /// <summary>
        /// Нормализованный building-адрес в читаемом виде.
        /// </summary>
        [JsonPropertyName("normalizedAddress")]
        public string NormalizedAddress { get; set; }

        /// <summary>
        /// Hash нормализованного building-адреса. Подходит для сравнения и дедупликации.
        /// </summary>
        [JsonPropertyName("hash")]
        public string Hash { get; set; }

        /// <summary>
        /// FIAS ID дома. Берётся из <c>dadata.suggest</c>, а если его нет — из <c>dadata.clean</c>.
        /// </summary>
        [JsonPropertyName("fiasId")]
        public string FiasId { get; set; }

        /// <summary>
        /// Данные DaData для building-части адреса.
        /// </summary>
        [JsonPropertyName("dadata")]
        public DadataDto Dadata { get; set; }
    }
}
