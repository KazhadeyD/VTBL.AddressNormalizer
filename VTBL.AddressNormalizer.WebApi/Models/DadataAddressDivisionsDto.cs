namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Гранулярные поля административного и муниципального делений DaData.
    /// </summary>
    public class DadataAddressDivisionsDto
    {
        public DadataAddressDivisionBlockDto Administrative { get; set; }
        public DadataAddressDivisionBlockDto Municipal { get; set; }
    }
}
