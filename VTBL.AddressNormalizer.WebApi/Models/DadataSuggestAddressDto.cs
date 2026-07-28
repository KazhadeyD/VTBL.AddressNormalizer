using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Результат подсказок DaData для building-части адреса.
    /// </summary>
    public class DadataSuggestAddressDto
    {
        /// <summary>
        /// Подсказки DaData, которые могут соответствовать building-части адреса.
        /// </summary>
        [JsonPropertyName("suggestions")]
        public IList<DadataSuggestAddressSuggestionDto> Suggestions { get; set; } = new List<DadataSuggestAddressSuggestionDto>();
    }
}