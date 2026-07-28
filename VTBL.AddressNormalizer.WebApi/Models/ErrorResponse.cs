using System.Text.Json.Serialization;

namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Ответ с описанием ошибки.
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Краткое пояснение, почему запрос не удалось обработать.
        /// </summary>
        /// <example>source должен быть непустой строкой</example>
        [JsonPropertyName("error")]
        public string Error { get; set; }
    }
}