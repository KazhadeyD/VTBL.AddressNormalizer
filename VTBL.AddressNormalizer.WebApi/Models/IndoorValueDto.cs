using System.Collections.Generic;

namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Structured <c>indoorValue</c>: <c>hash</c> от unit-канона и sparse-массив <c>marks</c>
    /// (только категории с непустыми <c>values</c>).
    /// </summary>
    public class IndoorValueDto
    {
        /// <summary>
        /// Indoor-часть после extract (хвост адреса после outdoor).
        /// </summary>
        public string Extracted { get; set; }

        /// <summary>
        /// SHA256 (hex, lowercase) от канонической строки unit (<c>ToCanonical</c>).
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        /// Категории indoor с данными; пустые категории не включаются.
        /// </summary>
        public IList<IndoorMarkDto> Marks { get; set; } = new List<IndoorMarkDto>();
    }
}
