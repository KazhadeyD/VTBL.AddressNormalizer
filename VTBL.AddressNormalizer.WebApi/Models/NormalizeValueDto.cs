namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Value полной нормализации: <c>buildingValue</c>, <c>indoorValue</c>.
    /// </summary>
    public class NormalizeValueDto
    {
        /// <summary>
        /// Outdoor-результат: extracted, канон, hash, заглушки <c>fiasId</c>/<c>dadata</c>.
        /// </summary>
        public DadataOutdoorDto BuildingValue { get; set; }

        /// <summary>
        /// Indoor: extracted-фрагмент, hash канона unit и sparse-массив категорий { id, name, values }.
        /// </summary>
        public IndoorValueDto IndoorValue { get; set; }
    }
}
