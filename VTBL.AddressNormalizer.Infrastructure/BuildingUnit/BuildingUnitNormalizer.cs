using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using VTBL.AddressNormalizer.Abstractions.BuildingUnit;
using VTBL.AddressNormalizer.Abstractions.Logging;
using VTBL.AddressNormalizer.Abstractions.Shared;

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
        private readonly ILogger _logger;

        /// <summary>
        /// Создаёт нормализатор с внедрёнными зависимостями.
        /// </summary>
        public BuildingUnitNormalizer(
            IBuildingUnitParser parser,
            IBuildingUnitCanonicalizer canonicalizer,
            ICanonicalHash hash,
            ILogger logger)
        {
            _parser = parser;
            _canonicalizer = canonicalizer;
            _hash = hash;
            _logger = logger ?? NullLogger.Instance;
        }

        /// <inheritdoc />
        public BuildingUnitNormalizationResult Normalize(string input)
        {
            var location = _parser.Parse(input);
            var canonical = _canonicalizer.ToCanonical(location);
            var json = JsonConvert.SerializeObject(location, JsonSettings);
            var hash = _hash.ComputeSha256(canonical);

            _logger.Debug(
                "BuildingUnit.Normalize: длина входа=" + (input?.Length ?? 0) +
                ", длина канона=" + (canonical?.Length ?? 0) +
                ", заполнено категорий=" + CountFilledCategories(location) +
                ", неразобрано=" + location.Unparsed.Count);

            return new BuildingUnitNormalizationResult(input, location, canonical, json, hash);
        }

        private static int CountFilledCategories(BuildingUnitLocation location)
        {
            var n = 0;
            if (HasItems(location.Floors)) n++;
            if (HasItems(location.Premises)) n++;
            if (HasItems(location.Rooms)) n++;
            if (HasItems(location.Offices)) n++;
            if (HasItems(location.Workplaces)) n++;
            if (HasItems(location.Parts)) n++;
            if (HasItems(location.Apartments)) n++;
            if (HasItems(location.Cabinets)) n++;
            if (HasItems(location.Entrances)) n++;
            if (HasItems(location.Blocks)) n++;
            if (HasItems(location.Sections)) n++;
            if (HasItems(location.Mailboxes)) n++;
            if (HasItems(location.Literas)) n++;
            if (HasItems(location.Ranges)) n++;
            if (HasItems(location.RawCodes)) n++;
            if (HasItems(location.Notes)) n++;
            return n;
        }

        private static bool HasItems(IList<string> values) => values != null && values.Count > 0;
    }
}
