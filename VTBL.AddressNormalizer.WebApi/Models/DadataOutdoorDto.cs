namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Блок outdoor-результата (<c>buildingValue</c>): extract + canonical + hash + заглушки DaData/FIAS.
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
        public string NormalizedAddress { get; set; }

        /// <summary>
        /// SHA256 (hex, lowercase) от <see cref="NormalizedAddress"/>.
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        /// Идентификатор FIAS. В v1 всегда <c>null</c> (заглушка под будущую интеграцию).
        /// </summary>
        public string FiasId { get; set; }

        /// <summary>
        /// Заглушка структуры ответа DaData suggest/address.
        /// </summary>
        public DadataSuggestAddressDto Suggest { get; set; }

        /// <summary>
        /// Заглушка структуры ответа DaData clean/address.
        /// </summary>
        public DadataCleanAddressDto Clean { get; set; }
    }
}
