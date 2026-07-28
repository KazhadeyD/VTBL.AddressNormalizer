using System.Text.Json.Serialization;

namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Тело ошибки с единственным полем <c>error</c>.
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Человекочитаемый текст ошибки.
        /// </summary>
        /// <example>source должен быть непустой строкой</example>
        [JsonPropertyName("error")]
        public string Error { get; set; }
    }
}