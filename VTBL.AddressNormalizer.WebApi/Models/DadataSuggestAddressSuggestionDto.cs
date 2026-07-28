namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Одна подсказка DaData suggest/address.
    /// </summary>
    public class DadataSuggestAddressSuggestionDto
    {
        public string Value { get; set; }
        public string UnrestrictedValue { get; set; }
        public DadataAddressDataDto Data { get; set; }
    }
}
