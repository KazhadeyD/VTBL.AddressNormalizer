using VTBL.AddressNormalizer.Abstractions.Shared;
using VTBL.AddressNormalizer.Infrastructure.Shared;

namespace VTBL.AddressNormalizer.Infrastructure.BuildingAddress.Steps
{
    /// <summary>
    /// Замена полных форм географических типов на канонические сокращения.
    /// </summary>
    internal sealed class GeographicTypeNormalizer : ITextNormalizer
    {
        private readonly ITextNormalizer _inner = new RegexSynonymNormalizer(GeographicTypeSynonymRules.All);

        /// <inheritdoc />
        public string Normalize(string input) => _inner.Normalize(input);
    }
}
