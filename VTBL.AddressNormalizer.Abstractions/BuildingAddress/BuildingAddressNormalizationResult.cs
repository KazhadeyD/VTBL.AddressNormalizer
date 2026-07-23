namespace VTBL.AddressNormalizer.Abstractions.BuildingAddress
{
    /// <summary>
    /// Результат нормализации полного адреса до уровня здания.
    /// </summary>
    public sealed class BuildingAddressNormalizationResult
    {
        /// <summary>
        /// Создаёт результат extract + canonical.
        /// </summary>
        /// <param name="original">Исходная строка (или пустая при <c>null</c>).</param>
        /// <param name="extracted">Локация здания без indoor-хвоста.</param>
        /// <param name="canonical">Читаемый канон извлечённой локации.</param>
        public BuildingAddressNormalizationResult(string original, string extracted, string canonical)
        {
            Original = original ?? string.Empty;
            Extracted = extracted ?? string.Empty;
            Canonical = canonical ?? string.Empty;
        }

        /// <summary>
        /// Исходная строка до нормализации.
        /// </summary>
        public string Original { get; }

        /// <summary>
        /// Извлечённая локация здания (без indoor).
        /// </summary>
        public string Extracted { get; }

        /// <summary>
        /// Читаемый канонический вид локации здания.
        /// </summary>
        public string Canonical { get; }
    }
}
