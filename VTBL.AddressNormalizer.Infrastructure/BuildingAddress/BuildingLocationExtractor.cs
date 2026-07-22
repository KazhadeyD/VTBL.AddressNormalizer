using System.Text.RegularExpressions;
using VTBL.AddressNormalizer.Abstractions.BuildingAddress;
using VTBL.AddressNormalizer.Abstractions.Logging;

namespace VTBL.AddressNormalizer.Infrastructure.BuildingAddress
{
    /// <summary>
    /// Извлечение локации здания из полного адреса (отсечение indoor-хвоста).
    /// Поддерживает <see cref="ExtractSplit"/> (outdoor + indoor) и <see cref="Extract"/> (= Outdoor).
    /// </summary>
    public sealed class BuildingLocationExtractor : IBuildingLocationExtractor
    {
        private static readonly Regex HouseNumberMarkerRegex = new Regex(
            @"(?<!\p{L})(?:ДОМ|Д)(?!\p{L})\.?\s*\d",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private readonly ILogger _logger;

        /// <summary>
        /// Создаёт extractor с внедрённым логгером ядра.
        /// </summary>
        public BuildingLocationExtractor(ILogger logger)
        {
            _logger = logger ?? NullLogger.Instance;
        }

        /// <inheritdoc />
        public BuildingLocationExtractionResult ExtractSplit(string input)
        {
            var preprocessed = AddressPreprocessor.Preprocess(input);
            var text = preprocessed.Text;
            if (string.IsNullOrEmpty(text))
            {
                _logger.Debug("ExtractSplit: пусто после preprocess, длина входа=" + (input?.Length ?? 0));
                return new BuildingLocationExtractionResult(string.Empty, string.Empty);
            }

            var housePos = FindHouseMarkerIndex(text);
            var indoorMatch = IndoorMarkerRegistry.FindFirstMatch(text);

            BuildingLocationExtractionResult result;
            if (indoorMatch == null)
            {
                result = new BuildingLocationExtractionResult(TrimTrailingDelimiters(text), string.Empty);
            }
            else
            {
                // Indoor всегда от индекса маркера, не от cutIndex (иначе ведущая ','/';'/ws).
                var indoor = text.Substring(indoorMatch.Index);

                if (housePos >= 0 && indoorMatch.Index < housePos)
                {
                    result = new BuildingLocationExtractionResult(string.Empty, indoor);
                }
                else
                {
                    var cutIndex = ComputeCutIndex(text, indoorMatch);
                    var outdoorRaw = cutIndex <= 0 ? string.Empty : text.Substring(0, cutIndex);
                    result = new BuildingLocationExtractionResult(TrimTrailingDelimiters(outdoorRaw), indoor);
                }
            }

            _logger.Debug(
                "ExtractSplit: длина входа=" + text.Length +
                ", длина outdoor=" + result.Outdoor.Length +
                ", длина indoor=" + result.Indoor.Length +
                ", есть маркер дома=" + (housePos >= 0 ? "да" : "нет"));

            return result;
        }

        /// <inheritdoc />
        public string Extract(string input) => ExtractSplit(input).Outdoor;

        private static int FindHouseMarkerIndex(string text)
        {
            var match = HouseNumberMarkerRegex.Match(text);
            return match.Success ? match.Index : -1;
        }

        private static int ComputeCutIndex(string text, IndoorMarkerMatch indoorMatch)
        {
            var cut = indoorMatch.Index;
            var i = cut - 1;
            while (i >= 0 && char.IsWhiteSpace(text[i]))
                i--;

            if (i >= 0 && (text[i] == ',' || text[i] == ';'))
                cut = i;

            return cut;
        }

        private static string TrimTrailingDelimiters(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return text.TrimEnd(' ', '\t', ',', ';');
        }
    }
}
