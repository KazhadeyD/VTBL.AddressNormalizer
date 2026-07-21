namespace VTBL.AddressNormalizer.Abstractions.FieldAdapters.Crm
{
    /// <summary>
    /// Результат нормализации полного адреса CRM (без hash).
    /// </summary>
    public sealed class CrmNewAddressNormalizationResult
    {
        /// <summary>
        /// Создаёт результат нормализации полного адреса.
        /// </summary>
        /// <param name="original">Исходная собранная или переданная строка.</param>
        /// <param name="extracted">Локация здания без indoor.</param>
        /// <param name="canonical">Читаемый канон.</param>
        public CrmNewAddressNormalizationResult(string original, string extracted, string canonical)
        {
            Original = original ?? string.Empty;
            Extracted = extracted ?? string.Empty;
            Canonical = canonical ?? string.Empty;
        }

        /// <summary>Исходная строка.</summary>
        public string Original { get; }

        /// <summary>Извлечённая локация здания.</summary>
        public string Extracted { get; }

        /// <summary>Читаемый канонический вид.</summary>
        public string Canonical { get; }
    }
}
