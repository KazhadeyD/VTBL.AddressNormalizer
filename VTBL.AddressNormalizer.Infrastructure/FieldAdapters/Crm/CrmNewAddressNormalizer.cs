using VTBL.AddressNormalizer.Abstractions.BuildingAddress;
using VTBL.AddressNormalizer.Abstractions.FieldAdapters.Crm;
using VTBL.AddressNormalizer.Infrastructure.BuildingAddress;

namespace VTBL.AddressNormalizer.Infrastructure.FieldAdapters.Crm
{
    /// <summary>
    /// Stub CRM-адаптера для полного адреса (<c>new_address</c>). UC-04 — фаза 2.
    /// </summary>
    public sealed class CrmNewAddressNormalizer : ICrmNewAddressNormalizer
    {
        private readonly IBuildingAddressNormalizer _buildingAddress;

        /// <summary>
        /// Создаёт stub-адаптер.
        /// </summary>
        public CrmNewAddressNormalizer(IBuildingAddressNormalizer buildingAddress)
        {
            _buildingAddress = buildingAddress;
        }

        /// <inheritdoc />
        public CrmNewAddressNormalizationResult Normalize(string input)
        {
            var result = _buildingAddress.Normalize(input);
            return new CrmNewAddressNormalizationResult(result.Original, result.Extracted, result.Canonical);
        }

        /// <inheritdoc />
        public CrmNewAddressNormalizationResult Normalize(CrmNewAddressInput input)
        {
            throw new System.NotImplementedException(
                "Сборка адреса из столбцов new_address — фаза 2 (UC-04). Используйте Normalize(string).");
        }
    }
}
