using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Запрос batch-нормализации.
    /// </summary>
    public class BatchNormalizeRequest
    {
        [JsonPropertyName("items")]
        public IList<SourceRequest> Items { get; set; }
    }
}