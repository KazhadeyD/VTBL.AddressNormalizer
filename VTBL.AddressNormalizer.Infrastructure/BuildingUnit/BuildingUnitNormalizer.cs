using VTBL.AddressNormalizer.Abstractions.BuildingUnit;
using VTBL.AddressNormalizer.Abstractions.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace VTBL.AddressNormalizer.Infrastructure.BuildingUnit
{
    /// <summary>
    /// Нормализация локации внутри здания: parse → canonical + JSON + SHA256.
    /// </summary>
    public sealed class BuildingUnitNormalizer : IBuildingUnitNormalizer
    {
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.None,
        };

        private readonly IBuildingUnitParser _parser;
        private readonly IBuildingUnitCanonicalizer _canonicalizer;
        private readonly ICanonicalHash _hash;

        /// <summary>
        /// Создаёт нормализатор с внедрёнными зависимостями.
        /// </summary>
        public BuildingUnitNormalizer(
            IBuildingUnitParser parser,
            IBuildingUnitCanonicalizer canonicalizer,
            ICanonicalHash hash)
        {
            _parser = parser;
            _canonicalizer = canonicalizer;
            _hash = hash;
        }

        /// <inheritdoc />
        public BuildingUnitNormalizationResult Normalize(string input)
        {
            var location = _parser.Parse(input);
            var canonical = _canonicalizer.ToCanonical(location);
            var json = JsonConvert.SerializeObject(location, JsonSettings);
            var hash = _hash.ComputeSha256(canonical);

            return new BuildingUnitNormalizationResult(input, location, canonical, json, hash);
        }
    }
}
