using VTBL.AddressNormalizer.Abstractions.BuildingUnit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VTBL.AddressNormalizer.Infrastructure.BuildingUnit
{
    /// <summary>
    /// Indoor-стадии парсера: dot-slash, typed segments, подвал/цоколь, остаточные коды.
    /// </summary>
    public sealed partial class BuildingUnitParser
    {
        /// <summary>
        /// Извлекает текстовые примечания («ВХОД С ТОРЦА», «ВХОД С ФАСАДА») в <see cref="BuildingUnitLocation.Notes"/>.
        /// </summary>
        private static void ExtractNotes(BuildingUnitLocation location, ref string working)
        {
            working = NoteRegex.Replace(working, match =>
            {
                location.Notes.Add(NormalizeNote(match.Value));
                return ", ";
            });
            CollapseWorking(ref working);
        }

        /// <summary>
        /// Извлекает dot-slash формат («ЭТ./ПОМЕЩ. 0/II КОМ./ОФИС 1/24»).
        /// </summary>
        private static void ExtractSlashFormat(BuildingUnitLocation location, ref string working)
        {
            if (working.IndexOf("./", StringComparison.Ordinal) < 0 &&
                working.IndexOf(". /", StringComparison.Ordinal) < 0)
            {
                return;
            }

            var consumed = new bool[working.Length];
            var match = SlashTypeHeaderRegex.Match(working);
            while (match.Success)
            {
                if (IsConsumed(consumed, match.Index, match.Length))
                {
                    match = match.NextMatch();
                    continue;
                }

                var types = new List<string>();
                var index = match.Index;

                while (index < working.Length)
                {
                    var header = SlashTypeHeaderRegex.Match(working, index);
                    if (!header.Success || header.Index != index)
                        break;

                    var token = SlashTypeTokenRegex.Match(header.Value).Value.ToUpperInvariant();
                    if (token.Length > 0)
                        types.Add(token);

                    MarkConsumed(consumed, header.Index, header.Length);
                    index = header.Index + header.Length;
                }

                while (index < working.Length && char.IsWhiteSpace(working[index]))
                {
                    MarkConsumed(consumed, index, 1);
                    index++;
                }

                var valueStart = index;
                while (index < working.Length && !char.IsWhiteSpace(working[index]))
                    index++;

                if (types.Count > 0 && valueStart < index)
                {
                    var values = working.Substring(valueStart, index - valueStart)
                        .Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(v => v.Trim())
                        .Where(v => v.Length > 0)
                        .ToList();

                    MarkConsumed(consumed, valueStart, index - valueStart);

                    for (var i = 0; i < types.Count && i < values.Count; i++)
                        ApplySlashTypeValue(location, types[i], values[i], SlashValueMode.DotSlash);
                }

                match = SlashTypeHeaderRegex.Match(working, index);
            }

            working = BuildRemaining(working, consumed);
        }

        /// <summary>
        /// Проверяет, перекрывается ли диапазон символов уже обработанным фрагментом.
        /// </summary>
        private static bool IsConsumed(bool[] consumed, int index, int length)
        {
            for (var i = index; i < index + length; i++)
            {
                if (consumed[i])
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Помечает диапазон символов строки как обработанный.
        /// </summary>
        private static void MarkConsumed(bool[] consumed, int index, int length)
        {
            for (var i = index; i < index + length && i < consumed.Length; i++)
                consumed[i] = true;
        }

        /// <summary>
        /// Собирает необработанный остаток строки после пошагового извлечения.
        /// </summary>
        private static string BuildRemaining(string working, bool[] consumed)
        {
            var chars = new char[working.Length];
            var length = 0;
            for (var i = 0; i < working.Length; i++)
            {
                if (consumed[i])
                    continue;

                chars[length++] = working[i];
            }

            return WhitespaceCollapseRegex.Replace(new string(chars, 0, length), " ").Trim(' ', ',');
        }

        /// <summary>
        /// Извлекает специальные этажи: цоколь, подвал, подвальный.
        /// </summary>
        private static void ExtractSpecialFloors(BuildingUnitLocation location, ref string working)
        {
            var floorPatterns = new Dictionary<string, string>
            {
                { "ЦОКОЛЬНЫЙ", "цокольный" },
                { "ПОДВАЛЬНЫЙ", "подвальный" },
                { "ПОДВАЛ", "подвал" },
            };

            foreach (var pair in floorPatterns)
            {
                var pattern = pair.Key;
                var canonical = pair.Value;

                if (working.IndexOf(pattern, StringComparison.Ordinal) < 0)
                    continue;

                location.Floors.Add(canonical);
                working = working.Replace(pattern, " ");
            }

            working = WhitespaceCollapseRegex.Replace(working, " ").Trim(' ', ',');
            working = BareFloorWordRegex.Replace(working, " ");
            CollapseWorking(ref working);
        }

        /// <summary>
        /// Извлекает типизированные сегменты эт/пом/ком/оф/раб.м/ч.п по regex-шаблонам.
        /// </summary>
        private static void ExtractTypedSegments(BuildingUnitLocation location, ref string working)
        {
            foreach (var pattern in TypedPatterns)
            {
                working = pattern.Regex.Replace(working, match =>
                {
                    pattern.Apply(location, match.Groups["v"].Value);
                    return " ";
                });
            }

            CollapseWorking(ref working);
        }

        /// <summary>
        /// Разбирает необработанный остаток на <see cref="BuildingUnitLocation.RawCodes"/> и <see cref="BuildingUnitLocation.Unparsed"/>.
        /// </summary>
        private static void ExtractRemainingRawCodes(BuildingUnitLocation location, string working)
        {
            if (string.IsNullOrWhiteSpace(working))
                return;

            var tokens = working.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .SelectMany(token => token.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                .Select(NormalizeResidualToken)
                .Where(token => token.Length > 0)
                .ToList();

            foreach (var token in tokens)
            {
                if (IsIgnorableToken(token))
                    continue;

                if (RawCodeTokenRegex.IsMatch(token))
                    location.RawCodes.Add(token);
                else
                    location.Unparsed.Add(token);
            }
        }

        /// <summary>
        /// Нормализует остаточный токен: скобки и хвостовая пунктуация («410).» → «410»).
        /// </summary>
        private static string NormalizeResidualToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return string.Empty;

            token = token.Trim();
            token = token.Trim('(', ')');
            token = token.TrimEnd('.', ',', ';');
            token = token.Trim('(', ')');
            return token;
        }

        /// <summary>
        /// Проверяет, является ли токен «хвостом» маркера без значения (ЭТ, ПОМ, …).
        /// </summary>
        private static bool IsIgnorableToken(string token)
        {
            return token == "ЭТ"
                || token == "ПОМ"
                || token == "КОМ"
                || token == "ОФ"
                || token == "ОФИС"
                || token == "ПОМЕЩ"
                || token == "ЭТАЖ"
                || token == "ПОМЕЩЕНИЕ"
                || token == "КОМНАТА"
                || token == "КОМН";
        }

        /// <summary>
        /// Нормализует текст примечания: lower case, схлопывание пробелов.
        /// </summary>
        private static string NormalizeNote(string value)
        {
            return WhitespaceCollapseRegex.Replace(value.Trim().ToLowerInvariant(), " ");
        }
    }
}
