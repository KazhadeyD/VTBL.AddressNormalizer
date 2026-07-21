using System.Text.RegularExpressions;
using VTBL.AddressNormalizer.Abstractions.Shared;

namespace VTBL.AddressNormalizer.Infrastructure.Shared
{
    /// <summary>
    /// Определение regex indoor-маркера и его категории.
    /// </summary>
    internal readonly struct IndoorMarkerPatternDefinition
    {
        /// <summary>
        /// Создаёт определение маркера.
        /// </summary>
        public IndoorMarkerPatternDefinition(Regex pattern, IndoorMarkerKind kind)
        {
            Pattern = pattern;
            Kind = kind;
        }

        /// <summary>Скомпилированный regex маркера.</summary>
        public Regex Pattern { get; }

        /// <summary>Категория маркера.</summary>
        public IndoorMarkerKind Kind { get; }
    }
}
