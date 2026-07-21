namespace VTBL.AddressNormalizer.Abstractions.Shared
{
    /// <summary>
    /// Шаг нормализации текстовой строки.
    /// </summary>
    public interface ITextNormalizer
    {
        /// <summary>
        /// Применяет шаг нормализации к входной строке.
        /// </summary>
        /// <param name="input">Исходная строка.</param>
        /// <returns>Нормализованная строка.</returns>
        string Normalize(string input);
    }
}
