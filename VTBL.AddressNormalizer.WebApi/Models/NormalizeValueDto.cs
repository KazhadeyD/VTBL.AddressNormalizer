namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Value полной нормализации: <c>dadataOutdoor</c>, <c>indoorValue</c>.
    /// </summary>
    public class NormalizeValueDto
    {
        /// <summary>
        /// Outdoor-результат: extracted, канон, hash, заглушки <c>fiasId</c>/<c>dadata</c>.
        /// </summary>
        public DadataOutdoorDto DadataOutdoor { get; set; }

        /// <summary>
        /// Indoor: все категории <c>BuildingUnitLocation</c> как { name, values } + hash канона unit.
        /// </summary>
        public IndoorValueDto IndoorValue { get; set; }
    }
}
