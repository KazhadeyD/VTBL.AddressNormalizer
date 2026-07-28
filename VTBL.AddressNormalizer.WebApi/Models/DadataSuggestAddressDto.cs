using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Ответ DaData suggest/address.
    /// </summary>
    public class DadataSuggestAddressDto
    {
        [JsonPropertyName("suggestions")]
        public IList<DadataSuggestAddressSuggestionDto> Suggestions { get; set; } = new List<DadataSuggestAddressSuggestionDto>();
    }
}