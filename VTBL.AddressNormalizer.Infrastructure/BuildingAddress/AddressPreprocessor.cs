using System.Text.RegularExpressions;

namespace VTBL.AddressNormalizer.Infrastructure.BuildingAddress
{
    /// <summary>
    /// Результат препроцессинга адресной строки.
    /// </summary>
    internal readonly struct PreprocessResult
    {
        /// <summary>
        /// Создаёт результат препроцессинга.
        /// </summary>
        public PreprocessResult(string text, bool hadExplicitDelimiters)
        {
            Text = text ?? string.Empty;
            HadExplicitDelimiters = hadExplicitDelimiters;
        }

        /// <summary>Нормализованный текст.</summary>
        public string Text { get; }

        /// <summary>
        /// <c>true</c>, если исходник содержал <c>,</c> или <c>;</c> до мутаций.
        /// </summary>
        public bool HadExplicitDelimiters { get; }
    }

    /// <summary>
    /// Единый препроцессинг для Extract и Canonical.
    /// </summary>
    internal static class AddressPreprocessor
    {
        private static readonly Regex WhitespaceRegex = new Regex(
            @"\s+",
            RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        /// Препроцессит входную строку.
        /// </summary>
        /// <param name="input">Сырая строка.</param>
        /// <returns>Текст и флаг явных разделителей.</returns>
        internal static PreprocessResult Preprocess(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return new PreprocessResult(string.Empty, false);

            var hadExplicitDelimiters = input.IndexOf(',') >= 0 || input.IndexOf(';') >= 0;
            var text = input.Trim();
            text = WhitespaceRegex.Replace(text, " ");
            text = text.Replace("№", "номер");
            text = text.Replace(';', ',');

            return new PreprocessResult(text, hadExplicitDelimiters);
        }
    }
}
