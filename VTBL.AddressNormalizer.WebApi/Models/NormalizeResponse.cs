using System.Text.Json.Serialization;

namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Ответ полной нормализации адреса.
    /// </summary>
    public class NormalizeResponse
    {
        /// <summary>
        /// Исходная строка запроса.
        /// </summary>
        [JsonPropertyName("source")]
        public string Source { get; set; }

        /// <summary>
        /// Результат нормализации.
        /// </summary>
        [JsonPropertyName("value")]
        public NormalizeValueDto Value { get; set; }
    }
}