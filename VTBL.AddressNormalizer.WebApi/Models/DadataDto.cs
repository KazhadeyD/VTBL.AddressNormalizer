using System.Text.Json.Serialization;

namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Данные DaData для building-части адреса: подсказки и результат очистки.
    /// </summary>
    public class DadataDto
    {
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
