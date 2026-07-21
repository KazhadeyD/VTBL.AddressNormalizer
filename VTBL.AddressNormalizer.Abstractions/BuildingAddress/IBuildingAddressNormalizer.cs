namespace VTBL.AddressNormalizer.Abstractions.BuildingAddress
{
    /// <summary>
    /// End-to-end нормализация полного адреса: extract building location → readable canonical.
    /// </summary>
    public interface IBuildingAddressNormalizer
    {
        /// <summary>
        /// Нормализует полный адрес: отсечение indoor + читаемая канонизация.
        /// </summary>
        /// <param name="input">Полная адресная строка (возможно с indoor-частью).</param>
        /// <returns>Результат с Original, Extracted и Canonical (без hash).</returns>
        BuildingAddressNormalizationResult Normalize(string input);
    }
}
