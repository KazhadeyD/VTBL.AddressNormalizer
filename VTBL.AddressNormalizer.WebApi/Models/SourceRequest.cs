using System.Text.Json.Serialization;

namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Входной запрос с исходной адресной или unit-строкой.
    /// </summary>
    /// <example>
    /// { "source": "г Москва, ул Сухонская, д 11, кв 89" }
    /// </example>
    public class SourceRequest
    {
        /// <summary>
        /// Исходная строка. Не должна быть null, пустой или состоять только из пробелов.
        /// </summary>
        /// <example>г Москва, ул Сухонская, д 11, кв 89</example>
        [JsonPropertyName("source")]
        public string Source { get; set; }
    }
}