namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Один компонент деления DaData.
    /// </summary>
    public class DadataAddressDivisionItemDto
    {
        public string FiasId { get; set; }
        public string KladrId { get; set; }
        public string Type { get; set; }
        public string TypeFull { get; set; }
        public string Name { get; set; }
        public string NameWithType { get; set; }
    }
}
