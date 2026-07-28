using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Ответ batch-нормализации.
    /// </summary>
    public class BatchNormalizeResponse
    {
        [JsonPropertyName("items")]
        public IList<BatchItemResultDto> Items { get; set; }
    }
}