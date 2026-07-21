using VTBL.AddressNormalizer.Abstractions.BuildingUnit;
using VTBL.AddressNormalizer.Abstractions.FieldAdapters.Crm;
using VTBL.AddressNormalizer.Abstractions.Shared;
using VTBL.AddressNormalizer.Infrastructure.BuildingUnit;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace VTBL.AddressNormalizer.Infrastructure.FieldAdapters.Crm
{
    /// <summary>
    /// Нормализация CRM-поля <c>new_flat</c>: classify → parse → canonical + JSON + SHA256.
    /// </summary>
    public sealed class CrmNewFlatNormalizer : ICrmNewFlatNormalizer
    {
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.None,
        };

        private readonly IBuildingUnitClassifier _classifier;
        private readonly IBuildingUnitParser _parser;
        private readonly IBuildingUnitCanonicalizer _canonicalizer;
        private readonly ICanonicalHash _hash;

        /// <summary>
        /// Создаёт CRM-адаптер для <c>new_flat</c>.
        /// </summary>
        public CrmNewFlatNormalizer(
            IBuildingUnitClassifier classifier,
            IBuildingUnitParser parser,
            IBuildingUnitCanonicalizer canonicalizer,
            ICanonicalHash hash)
        {
            _classifier = classifier;
            _parser = parser;
            _canonicalizer = canonicalizer;
            _hash = hash;
        }

        /// <inheritdoc />
        public CrmNewFlatNormalizationResult Normalize(string input)
        {
            var category = _classifier.Classify(input);
            var location = _parser.Parse(input);
            var canonical = _canonicalizer.ToCanonical(location);

            if (string.IsNullOrEmpty(canonical) && category == BuildingUnitCategory.Garbage && !string.IsNullOrWhiteSpace(input))
            {
                location.Unparsed.Add(input.Trim());
                canonical = _canonicalizer.ToCanonical(location);
            }

            var json = JsonConvert.SerializeObject(location, JsonSettings);
            var hash = _hash.ComputeSha256(canonical);

            return new CrmNewFlatNormalizationResult(input, category, location, canonical, json, hash);
        }
    }
}
