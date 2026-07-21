namespace VTBL.AddressNormalizer.Abstractions.Shared
{
    /// <summary>
    /// Правило замены синонимов: альтернативы в <see cref="Pattern"/> (через |) → <see cref="Replacement"/>.
    /// </summary>
    /// <remarks>Длинные формы указывайте первыми в <see cref="Pattern"/>.</remarks>
    public sealed class SynonymRule
    {
        /// <summary>
        /// Создаёт правило замены синонимов.
        /// </summary>
        /// <param name="pattern">Regex-альтернативы через |, например «комната|комн|ком|ко».</param>
        /// <param name="replacement">Каноническое значение, например «ком».</param>
        public SynonymRule(string pattern, string replacement)
        {
            Pattern = pattern;
            Replacement = replacement;
        }

        /// <summary>
        /// Regex-альтернативы синонимов (без границ слова — они добавляются в нормализаторе).
        /// </summary>
        public string Pattern { get; }

        /// <summary>
        /// Каноническое значение для замены.
        /// </summary>
        public string Replacement { get; }
    }
}
