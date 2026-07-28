using System.Text.Json.Serialization;

namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Одна подсказка из ответа DaData suggest/address.
    /// </summary>
    public class DadataSuggestAddressSuggestionDto
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("unrestricted_value")]
        public string UnrestrictedValue { get; set; }

        [JsonPropertyName("data")]
        public DadataAddressDataDto Data { get; set; }
    }
}