using System.Text.Json.Serialization;

namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Один блок деления DaData.
    /// </summary>
    public class DadataAddressDivisionBlockDto
    {
        [JsonPropertyName("area")]
        public DadataAddressDivisionItemDto Area { get; set; }

        [JsonPropertyName("city")]
        public DadataAddressDivisionItemDto City { get; set; }

        [JsonPropertyName("city_district")]
        public DadataAddressDivisionItemDto CityDistrict { get; set; }

        [JsonPropertyName("settlement")]
        public DadataAddressDivisionItemDto Settlement { get; set; }

        [JsonPropertyName("planning_structure")]
        public DadataAddressDivisionItemDto PlanningStructure { get; set; }
    }
}