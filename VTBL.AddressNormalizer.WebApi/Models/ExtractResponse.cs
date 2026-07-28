using System.Text.Json.Serialization;

namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Ответ метода извлечения building-части адреса.
    /// </summary>
    public class ExtractResponse
    {
        /// <summary>
        /// Исходная строка, переданная в запросе.
        /// </summary>
        [JsonPropertyName("source")]
        public string Source { get; set; }

        /// <summary>
        /// Фрагмент строки, распознанный как building-часть адреса.
        /// </summary>
        [JsonPropertyName("extracted")]
        public string Extracted { get; set; }
    }
}