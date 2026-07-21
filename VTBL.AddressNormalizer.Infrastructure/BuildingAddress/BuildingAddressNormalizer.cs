using VTBL.AddressNormalizer.Abstractions.BuildingAddress;

namespace VTBL.AddressNormalizer.Infrastructure.BuildingAddress
{
    /// <summary>
    /// End-to-end нормализация полного адреса: extract → readable canonical.
    /// </summary>
    public sealed class BuildingAddressNormalizer : IBuildingAddressNormalizer
    {
        private readonly IBuildingLocationExtractor _extractor;
        private readonly IBuildingAddressCanonicalizer _canonicalizer;

        /// <summary>
        /// Создаёт фасад с внедрёнными зависимостями.
        /// </summary>
        public BuildingAddressNormalizer(
            IBuildingLocationExtractor extractor,
            IBuildingAddressCanonicalizer canonicalizer)
        {
            _extractor = extractor;
            _canonicalizer = canonicalizer;
        }

        /// <inheritdoc />
        public BuildingAddressNormalizationResult Normalize(string input)
        {
            var original = input ?? string.Empty;
            var extracted = _extractor.Extract(input);
            var canonical = _canonicalizer.ToCanonical(extracted);
            return new BuildingAddressNormalizationResult(original, extracted, canonical);
        }
    }
}
