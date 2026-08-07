using System.Text.RegularExpressions;
using VTBL.AddressNormalizer.Abstractions.BuildingAddress;
using VTBL.AddressNormalizer.Abstractions.Logging;
using VTBL.AddressNormalizer.Infrastructure.Shared;

namespace VTBL.AddressNormalizer.Infrastructure.BuildingAddress
{
    /// <summary>
    /// Извлечение локации здания из полного адреса (отсечение indoor-хвоста).
    /// Поддерживает <see cref="ExtractSplit"/> (outdoor + indoor) и <see cref="Extract"/> (= Outdoor).
    /// </summary>
    /// <remarks>
    /// Правила после маркера дома:
    /// <list type="bullet">
    /// <item><c>к</c>/<c>к.</c> сразу после дома — корпус (outdoor), не точка cut;</item>
    /// <item><c>кв</c>/<c>кв.</c> или <c>к</c>/<c>к.</c> после промежуточных токенов — cut сразу после дома (хвост целиком indoor);</item>
    /// <item>прочие indoor-маркеры (<c>эт</c>, <c>пом</c>, <c>ком</c>, …) — cut по самому левому маркеру.</item>
    /// </list>
    /// </remarks>
    public sealed class BuildingLocationExtractor : IBuildingLocationExtractor
    {
        private static readonly Regex HouseNumberMarkerRegex = new Regex(
            @"(?<!\p{L})(?:ДОМ|Д)(?!\p{L})\.?\s*\d",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        /// Короткое «к» / «к.» (не «кв», не «ком»…): корпус сразу после дома
        /// или триггер cut после дома, если после дома есть промежуточные токены.
        /// </summary>
        private static readonly Regex ShortKMarkerRegex = new Regex(
            @"(?<!\p{L})К\.?(?!\p{L})",
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

            var houseMatch = HouseNumberMarkerRegex.Match(text);
            var housePos = houseMatch.Success ? houseMatch.Index : -1;
            var houseEnd = houseMatch.Success ? FindHouseNumberEnd(text, houseMatch) : -1;

            var decision = DecideSplit(text, housePos, houseEnd);

            BuildingLocationExtractionResult result;
            if (decision == null)
            {
                result = new BuildingLocationExtractionResult(TrimTrailingDelimiters(text), string.Empty);
            }
            else if (decision.CutAfterHouse)
            {
                var outdoorRaw = houseEnd <= 0 ? string.Empty : text.Substring(0, houseEnd);
                var indoor = TrimLeadingDelimiters(text.Substring(houseEnd));
                result = new BuildingLocationExtractionResult(TrimTrailingDelimiters(outdoorRaw), indoor);
            }
            else if (housePos >= 0 && decision.IndoorStart < housePos)
            {
                result = new BuildingLocationExtractionResult(
                    string.Empty,
                    text.Substring(decision.IndoorStart));
            }
            else
            {
                var cutIndex = ComputeCutIndex(text, decision.IndoorStart);
                var outdoorRaw = cutIndex <= 0 ? string.Empty : text.Substring(0, cutIndex);
                result = new BuildingLocationExtractionResult(
                    TrimTrailingDelimiters(outdoorRaw),
                    text.Substring(decision.IndoorStart));
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

        private static SplitDecision DecideSplit(string text, int housePos, int houseEnd)
        {
            if (housePos < 0)
                return DecideWithoutHouse(text);

            var searchFrom = houseEnd >= 0 ? houseEnd : housePos;

            var indoorBeforeHouse = FindLeftmostIndoorMarker(text, 0, housePos);
            if (indoorBeforeHouse != null)
            {
                return SplitDecision.AtMarker(indoorBeforeHouse.Index);
            }

            var cutAfterHouse = TryCutAfterHouseWhenKOrApartment(text, houseEnd, searchFrom);
            if (cutAfterHouse != null)
                return cutAfterHouse;

            var indoorAfterHouse = FindLeftmostIndoorMarker(text, searchFrom, text.Length);
            if (indoorAfterHouse != null)
            {
                return SplitDecision.AtMarker(indoorAfterHouse.Index);
            }

            return null;
        }

        private static SplitDecision DecideWithoutHouse(string text)
        {
            var indoor = FindLeftmostIndoorMarker(text, 0, text.Length);
            if (indoor != null)
                return SplitDecision.AtMarker(indoor.Index);

            // Без дома короткое «к.» / «к» считаем indoor (комната), как прежний ShortRoom.
            var k = ShortKMarkerRegex.Match(text);
            if (k.Success)
                return SplitDecision.AtMarker(k.Index);

            return null;
        }

        /// <summary>
        /// Cut сразу после дома, если после дома есть <c>кв</c> или <c>к</c>/<c>к.</c> за промежуточными токенами
        /// (хвост целиком уходит в indoor). Если сразу после дома корпус, а дальше <c>кв</c> —
        /// корпус остаётся в outdoor, cut на <c>кв</c>.
        /// </summary>
        private static SplitDecision TryCutAfterHouseWhenKOrApartment(string text, int houseEnd, int searchFrom)
        {
            if (houseEnd < 0)
                return null;

            var hasKorpusAtHouse = false;
            for (var m = ShortKMarkerRegex.Match(text, searchFrom); m.Success; m = m.NextMatch())
            {
                if (IsImmediatelyAfterHouse(text, houseEnd, m.Index))
                {
                    hasKorpusAtHouse = true;
                    continue;
                }

                return SplitDecision.AfterHouse();
            }

            var apartment = IndoorMarkerPatterns.Apartment.Match(text, searchFrom);
            if (!apartment.Success)
                return null;

            if (hasKorpusAtHouse)
                return SplitDecision.AtMarker(apartment.Index);

            return SplitDecision.AfterHouse();
        }

        private static IndoorMarkerMatch FindLeftmostIndoorMarker(string text, int minIndex, int maxIndexExclusive)
        {
            if (string.IsNullOrEmpty(text) || minIndex >= maxIndexExclusive)
                return null;

            IndoorMarkerMatch best = null;

            foreach (var definition in IndoorMarkerPatterns.All)
            {
                var match = definition.Pattern.Match(text, minIndex);
                if (!match.Success || match.Index >= maxIndexExclusive)
                    continue;

                if (best == null || match.Index < best.Index)
                {
                    best = new IndoorMarkerMatch(match.Index, match.Length, definition.Kind);
                }
            }

            return best;
        }

        private static int FindHouseNumberEnd(string text, Match houseMatch)
        {
            // Паттерн заканчивается на первой цифре — добираем остальные цифры и опциональную литеру дома.
            var i = houseMatch.Index + houseMatch.Length;
            while (i < text.Length && char.IsDigit(text[i]))
                i++;

            var afterDigits = i;
            var j = afterDigits;
            while (j < text.Length && char.IsWhiteSpace(text[j]))
                j++;

            if (j < text.Length && IsSingleHouseLetter(text, j))
                return j + 1;

            if (afterDigits < text.Length && IsSingleHouseLetter(text, afterDigits))
                return afterDigits + 1;

            return afterDigits;
        }

        private static bool IsSingleHouseLetter(string text, int index)
        {
            if (index >= text.Length || !char.IsLetter(text[index]))
                return false;

            var next = index + 1;
            return next >= text.Length || !char.IsLetter(text[next]);
        }

        private static bool IsImmediatelyAfterHouse(string text, int houseEnd, int markerIndex)
        {
            if (houseEnd < 0 || markerIndex < houseEnd)
                return false;

            for (var i = houseEnd; i < markerIndex; i++)
            {
                var c = text[i];
                if (char.IsWhiteSpace(c) || c == ',' || c == ';')
                    continue;
                return false;
            }

            return true;
        }

        private static int ComputeCutIndex(string text, int indoorStart)
        {
            var cut = indoorStart;
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

        private static string TrimLeadingDelimiters(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var i = 0;
            while (i < text.Length)
            {
                var c = text[i];
                if (char.IsWhiteSpace(c) || c == ',' || c == ';')
                {
                    i++;
                    continue;
                }

                break;
            }

            return i == 0 ? text : text.Substring(i);
        }

        private sealed class SplitDecision
        {
            public int IndoorStart { get; private set; }
            public bool CutAfterHouse { get; private set; }

            public static SplitDecision AtMarker(int indoorStart) =>
                new SplitDecision { IndoorStart = indoorStart, CutAfterHouse = false };

            public static SplitDecision AfterHouse() =>
                new SplitDecision { IndoorStart = -1, CutAfterHouse = true };
        }
    }
}
