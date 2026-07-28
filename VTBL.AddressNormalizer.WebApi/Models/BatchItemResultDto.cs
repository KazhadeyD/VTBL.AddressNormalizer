using System.Text.Json.Serialization;

namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Результат одного элемента batch-запроса.
    /// </summary>
    public class BatchItemResultDto
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; }

        [JsonPropertyName("value")]
        public NormalizeValueDto Value { get; set; }

        [JsonPropertyName("error")]
        public string Error { get; set; }
    }
}