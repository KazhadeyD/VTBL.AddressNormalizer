namespace VTBL.AddressNormalizer.Abstractions.FieldAdapters.Crm
{
    /// <summary>
    /// CRM-адаптер для полного адреса (<c>new_address</c>). Stub — фаза 2.
    /// </summary>
    public interface ICrmNewAddressNormalizer
    {
        /// <summary>
        /// Нормализует готовую строку полного адреса через Core BuildingAddress.
        /// </summary>
        /// <param name="input">Готовая адресная строка.</param>
        /// <returns>Результат без hash.</returns>
        CrmNewAddressNormalizationResult Normalize(string input);

        /// <summary>
        /// Нормализует адрес, собранный из столбцов CRM (фаза 2).
        /// </summary>
        /// <param name="input">Набор полей <c>new_address</c>.</param>
        /// <returns>Результат без hash.</returns>
        CrmNewAddressNormalizationResult Normalize(CrmNewAddressInput input);
    }
}
