namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Value полной нормализации: <c>fiasId</c>, <c>dadataOutdoor</c>, <c>indoorValue</c>.
    /// </summary>
    public class NormalizeValueDto
    {
        /// <summary>
        /// Идентификатор FIAS. В v1 всегда <c>null</c> (заглушка под будущую интеграцию).
        /// </summary>
        public string FiasId { get; set; }

        /// <summary>
        /// Outdoor-результат: extracted, канон и SHA256 от канона (локальный блок, не вызов DaData).
        /// </summary>
        public DadataOutdoorDto DadataOutdoor { get; set; }

        /// <summary>
        /// Indoor: все категории <c>BuildingUnitLocation</c> как { name, values }.
        /// </summary>
        public IndoorValueDto IndoorValue { get; set; }
    }
}
