using System.Text.Json.Serialization;

namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Ответ метода нормализации indoor-строки.
    /// </summary>
    public class UnitNormalizeResponse
    {
        /// <summary>
        /// Исходная строка, переданная в запросе.
        /// </summary>
        [JsonPropertyName("source")]
        public string Source { get; set; }

        /// <summary>
        /// Структурированный результат разбора indoor-части.
        /// </summary>
        [JsonPropertyName("indoorValue")]
        public IndoorValueDto IndoorValue { get; set; }

        /// <summary>
        /// Каноническое представление indoor-строки.
        /// </summary>
        [JsonPropertyName("canonical")]
        public string Canonical { get; set; }

        /// <summary>
        /// Hash канонической indoor-строки.
        /// </summary>
        [JsonPropertyName("hash")]
        public string Hash { get; set; }
    }
}