namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Ближайшая станция метро из ответа DaData.
    /// </summary>
    public class DadataMetroDto
    {
        public double? Distance { get; set; }
        public string Line { get; set; }
        public string Name { get; set; }
    }
}
