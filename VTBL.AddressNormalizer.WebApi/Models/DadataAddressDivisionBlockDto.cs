namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Один блок деления DaData: набор компонентов адреса в рамках деления.
    /// </summary>
    public class DadataAddressDivisionBlockDto
    {
        public DadataAddressDivisionItemDto Area { get; set; }
        public DadataAddressDivisionItemDto City { get; set; }
        public DadataAddressDivisionItemDto CityDistrict { get; set; }
        public DadataAddressDivisionItemDto Settlement { get; set; }
        public DadataAddressDivisionItemDto PlanningStructure { get; set; }
    }
}
