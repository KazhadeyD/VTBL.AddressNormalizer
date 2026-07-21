namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Входной запрос с исходной адресной / unit-строкой.
    /// </summary>
    /// <example>
    /// { "source": "г Москва, ул Сухонская, д 11, кв 89" }
    /// </example>
    public class SourceRequest
    {
        /// <summary>
        /// Исходная строка. Не должна быть null, пустой или состоять только из пробелов (иначе HTTP 400).
        /// </summary>
        /// <example>г Москва, ул Сухонская, д 11, кв 89</example>
        public string Source { get; set; }
    }
}
