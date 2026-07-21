namespace VTBL.AddressNormalizer.Abstractions.FieldAdapters.Crm
{
    /// <summary>
    /// Нормализация CRM-поля <c>new_flat</c>: classify → parse → canonical + JSON + SHA256.
    /// </summary>
    public interface ICrmNewFlatNormalizer
    {
        /// <summary>
        /// Нормализует CRM-поле <c>new_flat</c>.
        /// </summary>
        /// <param name="input">Значение столбца <c>new_flat</c>.</param>
        /// <returns>Результат с категорией и Core-данными.</returns>
        CrmNewFlatNormalizationResult Normalize(string input);
    }
}
