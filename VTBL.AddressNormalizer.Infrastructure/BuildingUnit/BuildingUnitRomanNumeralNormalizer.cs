using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using VTBL.AddressNormalizer.Abstractions.BuildingUnit;

namespace VTBL.AddressNormalizer.Infrastructure.BuildingUnit
{
    /// <summary>
    /// Замена чистых римских токенов на арабские после разбора локации.
    /// </summary>
    /// <remarks>
    /// Конвертируется только значение, целиком состоящее из <c>I V X L C D M</c>
    /// (без учёта регистра). Смеси вроде <c>X-10</c>, <c>2X</c>, <c>IA</c>, <c>XIБ</c> не трогаются.
    /// </remarks>
    internal static class BuildingUnitRomanNumeralNormalizer
    {
        private static readonly Regex PureRomanRegex = new Regex(
            @"^[IVXLCDM]+$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        /// Проходит числовые/кодовые коллекции локации и заменяет чистые римские токены.
        /// </summary>
        public static void Normalize(BuildingUnitLocation location)
        {
            if (location == null)
                return;

            NormalizeList(location.Floors);
            NormalizeList(location.Premises);
            NormalizeList(location.Rooms);
            NormalizeList(location.Offices);
            NormalizeList(location.Workplaces);
            NormalizeList(location.Parts);
            NormalizeList(location.Apartments);
            NormalizeList(location.Cabinets);
            NormalizeList(location.Entrances);
            NormalizeList(location.Passages);
            NormalizeList(location.Holdings);
            NormalizeList(location.Storages);
            NormalizeList(location.Blocks);
            NormalizeList(location.Sections);
            NormalizeList(location.Mailboxes);
            NormalizeList(location.RawCodes);
            // Literas / Notes / Unparsed / Ranges — буквенные и текстовые метки, не номера.
        }

        /// <summary>
        /// Если <paramref name="value"/> — чистая валидная римская запись, возвращает арабскую строку;
        /// иначе исходное значение.
        /// </summary>
        public static string ConvertIfPureRoman(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            var trimmed = value.Trim();
            if (!PureRomanRegex.IsMatch(trimmed))
                return value;

            if (!TryParseRoman(trimmed, out var arabic) || arabic <= 0)
                return value;

            return arabic.ToString();
        }

        private static void NormalizeList(IList<string> values)
        {
            for (var i = 0; i < values.Count; i++)
                values[i] = ConvertIfPureRoman(values[i]);
        }

        private static bool TryParseRoman(string roman, out int result)
        {
            result = 0;
            var upper = roman.ToUpperInvariant();
            var total = 0;
            var previous = 0;

            for (var i = upper.Length - 1; i >= 0; i--)
            {
                if (!TryMapRomanDigit(upper[i], out var current))
                    return false;

                if (current < previous)
                    total -= current;
                else
                    total += current;

                previous = current;
            }

            if (total <= 0)
                return false;

            result = total;
            return true;
        }

        private static bool TryMapRomanDigit(char digit, out int value)
        {
            switch (digit)
            {
                case 'I':
                    value = 1;
                    return true;
                case 'V':
                    value = 5;
                    return true;
                case 'X':
                    value = 10;
                    return true;
                case 'L':
                    value = 50;
                    return true;
                case 'C':
                    value = 100;
                    return true;
                case 'D':
                    value = 500;
                    return true;
                case 'M':
                    value = 1000;
                    return true;
                default:
                    value = 0;
                    return false;
            }
        }
    }
}
