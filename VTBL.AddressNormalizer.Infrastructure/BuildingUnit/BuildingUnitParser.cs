using VTBL.AddressNormalizer.Abstractions.BuildingUnit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VTBL.AddressNormalizer.Infrastructure.BuildingUnit
{
    /// <summary>
    /// Разбор локации внутри здания в структурированную модель.
    /// </summary>
    /// <remarks>
    /// Partial-файлы: <see cref="BuildingUnitParser.Patterns.cs"/> (regex),
    /// <see cref="BuildingUnitParser.Stages.cs"/> (indoor-стадии).
    /// </remarks>
    public sealed partial class BuildingUnitParser : IBuildingUnitParser
    {
        /// <summary>
        /// Разбирает сырую строку в <see cref="BuildingUnitLocation"/>.
        /// </summary>
        /// <param name="input">Значение поля или произвольная строка локации.</param>
        /// <returns>Структурированная модель; для пустого ввода — пустая модель.</returns>
        public BuildingUnitLocation Parse(string input)
        {
            var location = new BuildingUnitLocation();
            if (string.IsNullOrWhiteSpace(input))
                return location;

            var working = Preprocess(input);
            ExtractBusinessCenterNotes(location, ref working);
            ExtractSlashChains(location, ref working);

            ExtractTyped(location, ApartmentRegex, location.Apartments, ref working, splitValues: true);
            ExtractTyped(location, CabinetRegex, location.Cabinets, ref working, splitValues: true);
            ExtractTyped(location, EntranceRegex, location.Entrances, ref working);
            ExtractBlockSection(location, ref working);
            ExtractTyped(location, BlockRegex, location.Blocks, ref working);
            ExtractTyped(location, SectionRegex, location.Sections, ref working);
            ExtractTyped(location, MailboxRegex, location.Mailboxes, ref working);
            ExtractTyped(location, LiteraRegex, location.Literas, ref working);

            working = PreprocessIndoorRemainder(working);
            ExtractNotes(location, ref working);
            ExtractSlashFormat(location, ref working);
            ExtractSpecialFloors(location, ref working);
            ExtractTypedSegments(location, ref working);
            ExtractRemainingRawCodes(location, working);
            ExtractRanges(location);

            return location;
        }

        /// <summary>
        /// Начальный препроцессинг: кавычки, №, обратный слэш, дефисы, пробелы.
        /// </summary>
        private static string Preprocess(string input)
        {
            var text = input.Trim();
            if (text.Length >= 2 && text[0] == '"' && text[text.Length - 1] == '"')
                text = text.Substring(1, text.Length - 2).Trim();

            text = text.Replace('№', ' ');
            text = text.Replace('\\', '/');
            text = DashGapRegex.Replace(text, "-");
            text = WhitespaceCollapseRegex.Replace(text, " ").Trim();
            text = text.TrimEnd('.', ',', ';');
            return text;
        }

        /// <summary>
        /// Препроцессинг остатка перед indoor-стадиями: upper case, порядковые суффиксы.
        /// </summary>
        private static string PreprocessIndoorRemainder(string working)
        {
            var text = working.Trim().ToUpperInvariant();
            text = OrdinalSuffixStripRegex.Replace(text, "$1");
            text = WhitespaceCollapseRegex.Replace(text, " ").Trim();
            return text;
        }

        /// <summary>
        /// Извлекает slash-цепочки заголовков («ЭТ/ПОМ 1/40», «ЭТАЖ/ПОМЕЩ. АНТРЕСОЛЬ 2/I КОМ./ОФИС …»).
        /// </summary>
        private static void ExtractSlashChains(BuildingUnitLocation location, ref string working)
        {
            var consumed = new bool[working.Length];
            var match = SlashChainHeaderChainRegex.Match(working);
            while (match.Success)
            {
                if (IsConsumed(consumed, match.Index, match.Length))
                {
                    match = match.NextMatch();
                    continue;
                }

                var headers = ParseSlashHeaders(match.Groups["headers"].Value);
                if (headers.Count == 0)
                {
                    match = match.NextMatch();
                    continue;
                }

                MarkConsumed(consumed, match.Index, match.Length);

                var valueStart = match.Index + match.Length;
                while (valueStart < working.Length && char.IsWhiteSpace(working[valueStart]))
                {
                    MarkConsumed(consumed, valueStart, 1);
                    valueStart++;
                }

                var boundary = NextSlashHeaderClusterBoundaryRegex.Match(working, valueStart);
                var valueEnd = boundary.Success ? boundary.Index : working.Length;

                if (valueStart < valueEnd)
                {
                    var valuesRaw = working.Substring(valueStart, valueEnd - valueStart);
                    var valueParts = valuesRaw
                        .Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(NormalizeValue)
                        .Where(part => !string.IsNullOrWhiteSpace(part))
                        .ToList();

                    MapSlashChainValues(location, headers, valueParts);
                    MarkConsumed(consumed, valueStart, valueEnd - valueStart);
                }

                match = SlashChainHeaderChainRegex.Match(working, valueEnd);
            }

            working = BuildRemaining(working, consumed);
            working = WhitespaceCollapseRegex.Replace(working, " ").Trim(' ', ',');
        }

        /// <summary>
        /// Сопоставляет заголовки slash-цепочки со значениями; лишние значения — в <see cref="BuildingUnitLocation.RawCodes"/>.
        /// </summary>
        private static void MapSlashChainValues(
            BuildingUnitLocation location,
            IReadOnlyList<string> headers,
            IReadOnlyList<string> valueParts)
        {
            var headerIndex = 0;
            var valueIndex = 0;

            while (headerIndex < headers.Count && valueIndex < valueParts.Count)
            {
                if (headerIndex + 1 < headers.Count &&
                    headers[headerIndex] == "ПОМЕЩ" &&
                    headers[headerIndex + 1] == "КОМ" &&
                    TrySplitPremiseRoom(valueParts[valueIndex], out var premise, out var room))
                {
                    ApplySlashChainValue(location, "ПОМЕЩ", premise);
                    ApplySlashChainValue(location, "КОМ", room);
                    headerIndex += 2;
                    valueIndex += 1;
                    continue;
                }

                ApplySlashChainValue(location, headers[headerIndex], valueParts[valueIndex]);
                headerIndex += 1;
                valueIndex += 1;
            }

            for (var i = valueIndex; i < valueParts.Count; i++)
                location.RawCodes.Add(valueParts[i]);
        }

        /// <summary>
        /// Разбивает составное значение «XII-8» на помещение и комнату для пары заголовков ПОМЕЩ+КОМ.
        /// </summary>
        private static bool TrySplitPremiseRoom(string value, out string premise, out string room)
        {
            premise = string.Empty;
            room = string.Empty;

            var match = PremiseRoomCompoundRegex.Match(value ?? string.Empty);
            if (!match.Success)
                return false;

            premise = NormalizeValue(match.Groups["premise"].Value);
            room = NormalizeValue(match.Groups["room"].Value);
            return premise.Length > 0 && room.Length > 0;
        }

        /// <summary>
        /// Извлекает составной маркер «БЛОК-СЕКЦИЯ» с дублированием номера в блок и секцию.
        /// </summary>
        private static void ExtractBlockSection(BuildingUnitLocation location, ref string working)
        {
            working = BlockSectionRegex.Replace(working, match =>
            {
                var value = match.Groups["v"].Success ? NormalizeValue(match.Groups["v"].Value) : string.Empty;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    location.Blocks.Add(value);
                    location.Sections.Add(value);
                }

                return " ";
            });

            working = WhitespaceCollapseRegex.Replace(working, " ").Trim(' ', ',');
        }

        /// <summary>
        /// Разбирает строку заголовков slash-цепочки на нормализованные типы.
        /// </summary>
        private static List<string> ParseSlashHeaders(string raw)
        {
            var headers = new List<string>();
            var match = SlashChainHeaderTokenRegex.Match(raw);
            while (match.Success)
            {
                headers.Add(NormalizeSlashHeader(match.Value));
                match = match.NextMatch();
            }

            return headers;
        }

        /// <summary>
        /// Приводит токен заголовка slash-цепочки к каноническому типу (ЭТ, ПОМЕЩ, КОМ, …).
        /// </summary>
        private static string NormalizeSlashHeader(string raw)
        {
            var token = WhitespaceCollapseRegex.Replace(raw.ToUpperInvariant(), string.Empty).TrimEnd('.');
            if (token.StartsWith("ЭТАЖ", StringComparison.Ordinal) || token == "ЭТ")
                return "ЭТ";
            if (token.StartsWith("ПОМЕЩ", StringComparison.Ordinal) || token.StartsWith("ПОМ", StringComparison.Ordinal))
                return "ПОМЕЩ";
            if (token.StartsWith("КОМН", StringComparison.Ordinal) || token.StartsWith("КОМ", StringComparison.Ordinal))
                return "КОМ";
            if (token.StartsWith("ОФИС", StringComparison.Ordinal) || token.StartsWith("ОФ", StringComparison.Ordinal))
                return "ОФИС";
            if (token.StartsWith("КАБИНЕТ", StringComparison.Ordinal) || token.StartsWith("КАБ", StringComparison.Ordinal))
                return "КАБ";
            if (token.StartsWith("РАБ", StringComparison.Ordinal))
                return "РАБ";

            return token;
        }

        /// <summary>
        /// Записывает одно значение slash-цепочки в соответствующую коллекцию модели.
        /// </summary>
        private static void ApplySlashChainValue(BuildingUnitLocation location, string type, string value)
        {
            switch (type)
            {
                case "ЭТ":
                    AddSingleValue(location.Floors, value);
                    break;
                case "ПОМЕЩ":
                    AddMultiValue(location.Premises, value);
                    break;
                case "КОМ":
                    AddMultiValue(location.Rooms, value);
                    break;
                case "ОФИС":
                    AddSingleValue(location.Offices, value);
                    break;
                case "КАБ":
                    AddSingleValue(location.Cabinets, value);
                    break;
                case "РАБ":
                    AddSingleValue(location.Workplaces, value);
                    break;
            }
        }

        /// <summary>
        /// Извлекает примечания бизнес-центра («БЦ …», скобки) в <see cref="BuildingUnitLocation.Notes"/>.
        /// </summary>
        private static void ExtractBusinessCenterNotes(BuildingUnitLocation location, ref string working)
        {
            working = BusinessCenterNoteRegex.Replace(working, match =>
            {
                var value = match.Groups["v"].Success ? NormalizeValue(match.Groups["v"].Value) : string.Empty;
                if (!string.IsNullOrWhiteSpace(value))
                    location.Notes.Add(value);

                return " ";
            });

            working = WhitespaceCollapseRegex.Replace(working, " ").Trim(' ', ',');
        }

        /// <summary>
        /// Извлекает типизированный сегмент по regex в целевую коллекцию модели.
        /// </summary>
        /// <param name="splitValues">При <c>true</c> дробит списки через «,»/«;».</param>
        private static void ExtractTyped(
            BuildingUnitLocation location,
            System.Text.RegularExpressions.Regex regex,
            IList<string> target,
            ref string working,
            bool splitValues = false)
        {
            working = regex.Replace(working, match =>
            {
                var value = match.Groups["v"].Success ? NormalizeValue(match.Groups["v"].Value) : string.Empty;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (splitValues)
                    {
                        foreach (var part in SplitValues(value))
                            target.Add(part);
                    }
                    else
                    {
                        target.Add(value);
                    }
                }

                return " ";
            });

            working = WhitespaceCollapseRegex.Replace(working, " ").Trim(' ', ',');
        }

        /// <summary>
        /// Схлопывает пробелы внутри извлечённого значения.
        /// </summary>
        private static string NormalizeValue(string raw)
        {
            return WhitespaceCollapseRegex.Replace(raw.Trim(), " ");
        }

        /// <summary>
        /// Дробит список значений через «,» или «;».
        /// </summary>
        private static IEnumerable<string> SplitValues(string value)
        {
            return value
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(part => NormalizeValue(part))
                .Where(part => !string.IsNullOrWhiteSpace(part));
        }

        /// <summary>
        /// Переносит диапазоны из <see cref="BuildingUnitLocation.RawCodes"/> в <see cref="BuildingUnitLocation.Ranges"/>.
        /// </summary>
        private static void ExtractRanges(BuildingUnitLocation location)
        {
            if (location.RawCodes.Count == 0)
                return;

            var remaining = new List<string>();
            foreach (var rawCode in location.RawCodes)
            {
                var match = RangeRegex.Match(rawCode ?? string.Empty);
                if (!match.Success)
                {
                    remaining.Add(rawCode);
                    continue;
                }

                location.Ranges.Add(NormalizeRange(match.Groups["v"].Value));
            }

            location.RawCodes.Clear();
            foreach (var value in remaining)
            {
                if (!string.IsNullOrWhiteSpace(value))
                    location.RawCodes.Add(value);
            }
        }

        /// <summary>
        /// Убирает пробелы вокруг дефиса диапазона («74 - 82» → «74-82»).
        /// </summary>
        private static string NormalizeRange(string value)
        {
            return value.Replace(" ", string.Empty);
        }

        /// <summary>
        /// Добавляет одно нормализованное значение в коллекцию, если оно непустое.
        /// </summary>
        private static void AddSingleValue(IList<string> target, string raw)
        {
            var value = NormalizeValue(raw);
            if (value.Length > 0)
                target.Add(value);
        }

        /// <summary>
        /// Добавляет несколько значений из строки с разделителями «;»/«,».
        /// </summary>
        private static void AddMultiValue(IList<string> target, string raw)
        {
            foreach (var part in raw.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries))
                AddSingleValue(target, part);
        }
    }
}
