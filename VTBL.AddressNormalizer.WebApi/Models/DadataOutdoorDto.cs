namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Блок outdoor-результата (<c>dadataOutdoor</c>): extract + canonical + hash.
    /// </summary>
    public class DadataOutdoorDto
    {
        /// <summary>
        /// Outdoor-часть после extract (без indoor-хвоста).
        /// </summary>
        /// <example>г Москва, ул Сухонская, д 11</example>
        public string Extracted { get; set; }

        /// <summary>
        /// Читаемый канон outdoor (building location).
        /// </summary>
        /// <example>г Москва, ул Сухонская, д 11</example>
        public string OutdoorCanonical { get; set; }

        /// <summary>
        /// SHA256 (hex, lowercase) от <see cref="OutdoorCanonical"/>.
        /// </summary>
        public string Hash { get; set; }
    }
}
