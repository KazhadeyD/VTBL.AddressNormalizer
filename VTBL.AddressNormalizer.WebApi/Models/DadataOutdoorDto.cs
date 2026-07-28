using System.Text.Json.Serialization;

namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Результат нормализации building-части адреса.
    /// </summary>
    public class DadataOutdoorDto
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
        /// FIAS ID дома. Берётся из <c>suggest.house_fias_id</c>, а если его нет — из <c>clean.house_fias_id</c>.
        /// </summary>
        [JsonPropertyName("fiasId")]
        public string FiasId { get; set; }

        /// <summary>
        /// Структура ответа DaData suggest/address. Пока заполняется заглушкой.
        /// </summary>
        [JsonPropertyName("suggest")]
        public DadataSuggestAddressDto Suggest { get; set; }

        /// <summary>
        /// Структура ответа DaData clean/address. Пока заполняется заглушкой.
        /// </summary>
        [JsonPropertyName("clean")]
        public DadataCleanAddressDto Clean { get; set; }
    }
}