using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Запрос пакетной нормализации адресов.
    /// </summary>
    public class BatchNormalizeRequest
    {
        /// <summary>
        /// Список адресов для обработки.
        /// </summary>
        [JsonPropertyName("items")]
        public IList<SourceRequest> Items { get; set; }
    }
}