using System.Text.RegularExpressions;

namespace VTBL.AddressNormalizer.Infrastructure.Shared
{
    /// <summary>
    /// Сборка regex indoor-маркеров из <see cref="IndoorMarkerLexemes"/>.
    /// </summary>
    internal static class IndoorMarkerRegexFactory
    {
        public static readonly RegexOptions MarkerOptions =
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled;

        /// <summary>
        /// Только маркер с границами слова (для outdoor/indoor extract).
        /// </summary>
        public static Regex MarkerOnly(string lexeme) =>
            new Regex($@"(?<!\p{{L}})(?:{lexeme})(?!\p{{L}})", MarkerOptions);

        /// <summary>
        /// Маркер без правой границы (для префиксов вроде «СЕКЦ», «РАБ.М»).
        /// </summary>
        public static Regex MarkerPrefix(string lexeme) =>
            new Regex($@"(?<!\p{{L}})(?:{lexeme})", MarkerOptions);

        /// <summary>
        /// Маркер + опциональная точка + обязательное значение с цифры.
        /// </summary>
        public static Regex MarkerThenDigitValue(string lexeme, bool optionalTrailingDot = true)
        {
            var dot = optionalTrailingDot ? @"\.?" : string.Empty;
            return new Regex(
                $@"(?<!\p{{L}})(?:{lexeme}){dot}\s*(?<v>\d[\d\w\-/]*)",
                MarkerOptions);
        }

        /// <summary>
        /// Маркер + опциональная точка + список значений с цифры через «,»/«;».
        /// </summary>
        public static Regex MarkerThenDigitValueList(string lexeme, bool optionalTrailingDot = true)
        {
            var dot = optionalTrailingDot ? @"\.?" : string.Empty;
            return new Regex(
                $@"(?<!\p{{L}})(?:{lexeme}){dot}\s*(?<v>\d[\d\w\-/]*(?:\s*[,;]\s*\d[\d\w\-/]*)*)",
                MarkerOptions);
        }

        /// <summary>
        /// Маркер + опциональное значение <c>[\d\w\-]+</c>.
        /// </summary>
        public static Regex MarkerThenOptionalToken(string lexeme, string valuePattern = @"[\d\w\-]+") =>
            new Regex(
                $@"(?<!\p{{L}})(?:{lexeme})\s*(?<v>{valuePattern})?",
                MarkerOptions);

        /// <summary>
        /// Маркер + обязательное значение <c>[\d\w\-]+</c>.
        /// </summary>
        public static Regex MarkerThenRequiredToken(string lexeme, string valuePattern = @"[\d\w\-]+") =>
            new Regex(
                $@"(?<!\p{{L}})(?:{lexeme})\s*(?<v>{valuePattern})",
                MarkerOptions);
    }
}
