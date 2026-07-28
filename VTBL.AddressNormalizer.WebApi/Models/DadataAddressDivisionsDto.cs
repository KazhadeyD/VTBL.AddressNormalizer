using System.Text.Json.Serialization;

namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Сведения о делении адреса по административной и муниципальной структуре.
    /// </summary>
    public class DadataAddressDivisionsDto
    {
        [JsonPropertyName("administrative")]
        public DadataAddressDivisionBlockDto Administrative { get; set; }

        [JsonPropertyName("municipal")]
        public DadataAddressDivisionBlockDto Municipal { get; set; }
    }
}