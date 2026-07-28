using System.Text.Json.Serialization;

namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Ответ метода полной нормализации адреса.
    /// </summary>
    public class NormalizeResponse
    {
        /// <summary>
        /// Исходная адресная строка, переданная в запросе.
        /// </summary>
        [JsonPropertyName("source")]
        public string Source { get; set; }

        /// <summary>
        /// Результат нормализации по building- и indoor-частям адреса.
        /// </summary>
        [JsonPropertyName("value")]
        public NormalizeValueDto Value { get; set; }
    }
}