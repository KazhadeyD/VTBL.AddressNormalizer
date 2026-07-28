using System.Collections.Generic;

namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Ответ DaData suggest/address.
    /// </summary>
    public class DadataSuggestAddressDto
    {
        public IList<DadataSuggestAddressSuggestionDto> Suggestions { get; set; } = new List<DadataSuggestAddressSuggestionDto>();
    }
}
