using System.Text.RegularExpressions;
using VTBL.AddressNormalizer.Abstractions.BuildingAddress;

namespace VTBL.AddressNormalizer.Infrastructure.BuildingAddress
{
    /// <summary>
    /// Извлечение локации здания из полного адреса (отсечение indoor-хвоста).
    /// </summary>
    public sealed class BuildingLocationExtractor : IBuildingLocationExtractor
    {
        private static readonly Regex HouseNumberMarkerRegex = new Regex(
            @"(?<!\p{L})(?:ДОМ|Д)(?!\p{L})\.?\s*\d",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <inheritdoc />
        public string Extract(string input)
        {
            var preprocessed = AddressPreprocessor.Preprocess(input);
            var text = preprocessed.Text;
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var housePos = FindHouseMarkerIndex(text);
            var indoorMatch = IndoorMarkerRegistry.FindFirstMatch(text);

            if (indoorMatch == null)
                return TrimTrailingDelimiters(text);

            if (housePos >= 0 && indoorMatch.Index < housePos)
                return string.Empty;

            var cutIndex = ComputeCutIndex(text, indoorMatch);
            var result = cutIndex <= 0 ? string.Empty : text.Substring(0, cutIndex);
            return TrimTrailingDelimiters(result);
        }

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
