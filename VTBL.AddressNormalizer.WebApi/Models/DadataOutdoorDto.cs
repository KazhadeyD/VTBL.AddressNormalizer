namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Блок outdoor-результата (<c>dadataOutdoor</c>): extract + canonical + hash + заглушки DaData/FIAS.
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

        /// <summary>
        /// Идентификатор FIAS. В v1 всегда <c>null</c> (заглушка под будущую интеграцию).
        /// </summary>
        public string FiasId { get; set; }

        /// <summary>
        /// Сырой ответ / строка DaData. В v1 всегда <c>null</c> (заглушка).
        /// </summary>
        public string Dadata { get; set; }
    }
}
