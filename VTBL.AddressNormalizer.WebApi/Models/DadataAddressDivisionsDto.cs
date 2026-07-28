using System.Text.Json.Serialization;

namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Административное и муниципальное деление DaData.
    /// </summary>
    public class DadataAddressDivisionsDto
    {
        [JsonPropertyName("administrative")]
        public DadataAddressDivisionBlockDto Administrative { get; set; }

        [JsonPropertyName("municipal")]
        public DadataAddressDivisionBlockDto Municipal { get; set; }
    }
}