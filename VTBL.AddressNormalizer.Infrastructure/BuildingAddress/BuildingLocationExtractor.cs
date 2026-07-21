using System.Text.RegularExpressions;
using VTBL.AddressNormalizer.Abstractions.BuildingAddress;

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

        /// <inheritdoc />
        public BuildingLocationExtractionResult ExtractSplit(string input)
        {
            var preprocessed = AddressPreprocessor.Preprocess(input);
            var text = preprocessed.Text;
            if (string.IsNullOrEmpty(text))
                return new BuildingLocationExtractionResult(string.Empty, string.Empty);

            var housePos = FindHouseMarkerIndex(text);
            var indoorMatch = IndoorMarkerRegistry.FindFirstMatch(text);

            if (indoorMatch == null)
                return new BuildingLocationExtractionResult(TrimTrailingDelimiters(text), string.Empty);

            // Indoor всегда от индекса маркера, не от cutIndex (иначе ведущая ','/';'/ws).
            var indoor = text.Substring(indoorMatch.Index);

            if (housePos >= 0 && indoorMatch.Index < housePos)
                return new BuildingLocationExtractionResult(string.Empty, indoor);

            var cutIndex = ComputeCutIndex(text, indoorMatch);
            var outdoorRaw = cutIndex <= 0 ? string.Empty : text.Substring(0, cutIndex);
            return new BuildingLocationExtractionResult(TrimTrailingDelimiters(outdoorRaw), indoor);
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
