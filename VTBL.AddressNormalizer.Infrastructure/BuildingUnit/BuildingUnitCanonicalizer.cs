using VTBL.AddressNormalizer.Abstractions.BuildingUnit;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace VTBL.AddressNormalizer.Infrastructure.BuildingUnit
{
    /// <summary>
    /// Сборка канонической строки из структуры локации внутри здания.
    /// </summary>
    public sealed class BuildingUnitCanonicalizer : IBuildingUnitCanonicalizer
    {
        /// <summary>
        /// Нормализация пробелов внутри значения перед записью в канон.
        /// </summary>
        private static readonly Regex ValueWhitespaceRegex = new Regex(
            @"\s+",
            RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        /// Собирает каноническую строку из заполненной модели локации.
        /// </summary>
        /// <param name="location">Структурированная локация; <c>null</c> → пустая строка.</param>
        /// <returns>Канон вида «эт:4|пом:2|code:659318», сегменты отсортированы.</returns>
        public string ToCanonical(BuildingUnitLocation location)
        {
            if (location == null)
                return string.Empty;

            var parts = new List<string>();
            Append(parts, "эт", location.Floors);
            Append(parts, "пом", location.Premises);
            Append(parts, "ком", location.Rooms);
            Append(parts, "оф", location.Offices);
            Append(parts, "раб.м", location.Workplaces);
            Append(parts, "ч.п", location.Parts);
            Append(parts, "кв", location.Apartments);
            Append(parts, "каб", location.Cabinets);
            Append(parts, "под", location.Entrances);
            Append(parts, "проезд", location.Passages);
            Append(parts, "влад", location.Holdings);
            Append(parts, "склад", location.Storages);
            Append(parts, "блок", location.Blocks);
            Append(parts, "секц", location.Sections);
            Append(parts, "а/я", location.Mailboxes);
            Append(parts, "лит", location.Literas);
            Append(parts, "диап", location.Ranges);
            Append(parts, "code", location.RawCodes);
            Append(parts, "note", location.Notes);
            Append(parts, "unparsed", location.Unparsed);

            return string.Join("|", parts);
        }

        /// <summary>
        /// Добавляет нормализованные значения коллекции в канон с заданным префиксом.
        /// </summary>
        private static void Append(List<string> parts, string prefix, IEnumerable<string> values)
        {
            foreach (var value in values
                         .Select(NormalizeValue)
                         .Where(v => !string.IsNullOrEmpty(v))
                         .Distinct()
                         .OrderBy(v => v, System.StringComparer.Ordinal))
            {
                parts.Add($"{prefix}:{value}");
            }
        }

        /// <summary>
        /// Нормализует значение сегмента канона: trim, lower case, схлопывание пробелов.
        /// </summary>
        internal static string NormalizeValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return ValueWhitespaceRegex.Replace(value.Trim().ToLowerInvariant(), " ");
        }
    }
}
