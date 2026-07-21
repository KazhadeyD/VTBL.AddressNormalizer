using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace VTBL.AddressNormalizer.Infrastructure.BuildingUnit
{
    /// <summary>
    /// Раскрытие чисто числовых диапазонов «35-38» → 35, 36, 37, 38 для типизированных маркеров.
    /// </summary>
    /// <remarks>
    /// Не применяется к идентификаторам с буквенным суффиксом («35-Н») и к краткой записи комнаты «К. 5-20».
    /// Нетипизированные диапазоны в остатке по-прежнему идут в <c>диап:</c>.
    /// </remarks>
    internal static class BuildingUnitNumericRangeExpander
    {
        /// <summary>Максимальная длина раскрываемого диапазона (включительно).</summary>
        public const int DefaultMaxSpan = 500;

        private static readonly Regex PureNumericRangeRegex = new Regex(
            @"^(?<start>\d+)\s*-\s*(?<end>\d+)$",
            RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        /// Раскрывает значение в набор токенов, если это чистый числовой диапазон.
        /// </summary>
        public static IEnumerable<string> Expand(string value, bool enabled, int maxSpan = DefaultMaxSpan)
        {
            if (string.IsNullOrWhiteSpace(value))
                yield break;

            var trimmed = value.Trim();
            if (!enabled)
            {
                yield return trimmed;
                yield break;
            }

            var match = PureNumericRangeRegex.Match(trimmed);
            // start == end — не диапазон, а идентификатор вида «5-5» (корпус: пом:5-5).
            if (!match.Success
                || !int.TryParse(match.Groups["start"].Value, out var start)
                || !int.TryParse(match.Groups["end"].Value, out var end)
                || end <= start
                || end - start > maxSpan)
            {
                yield return trimmed;
                yield break;
            }

            for (var current = start; current <= end; current++)
                yield return current.ToString();
        }
    }
}
