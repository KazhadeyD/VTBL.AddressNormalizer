using System.Text.Json.Serialization;

namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Ответ метода канонизации building-части адреса.
    /// </summary>
    public class CanonicalizeResponse
    {
        /// <summary>
        /// Исходная строка, переданная в запросе.
        /// </summary>
        [JsonPropertyName("source")]
        public string Source { get; set; }

        /// <summary>
        /// Нормализованное представление building-части адреса.
        /// </summary>
        [JsonPropertyName("canonical")]
        public string Canonical { get; set; }
    }
}