using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Ответ метода пакетной нормализации адресов.
    /// </summary>
    public class BatchNormalizeResponse
    {
        /// <summary>
        /// Результаты обработки по каждому адресу из batch-запроса.
        /// </summary>
        [JsonPropertyName("items")]
        public IList<BatchItemResultDto> Items { get; set; }
    }
}