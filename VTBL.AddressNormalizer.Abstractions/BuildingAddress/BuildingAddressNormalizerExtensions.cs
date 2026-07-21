namespace VTBL.AddressNormalizer.Abstractions.BuildingAddress
{
    /// <summary>
    /// Расширения для <see cref="IBuildingAddressNormalizer"/>.
    /// </summary>
    public static class BuildingAddressNormalizerExtensions
    {
        /// <summary>
        /// Возвращает только каноническую строку.
        /// </summary>
        /// <param name="normalizer">Нормализатор.</param>
        /// <param name="input">Полный адрес.</param>
        /// <returns>Канон локации здания.</returns>
        public static string NormalizeToCanonical(this IBuildingAddressNormalizer normalizer, string input)
        {
            return normalizer.Normalize(input).Canonical;
        }
    }
}
