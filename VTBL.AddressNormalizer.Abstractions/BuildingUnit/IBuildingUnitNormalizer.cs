namespace VTBL.AddressNormalizer.Abstractions.BuildingUnit
{
    /// <summary>
    /// Нормализация локации внутри здания: parse → canonical + JSON + SHA256.
    /// </summary>
    public interface IBuildingUnitNormalizer
    {
        /// <summary>
        /// Нормализует строку локации: parse → canonical + JSON + SHA256.
        /// </summary>
        /// <param name="input">Сырая строка локации внутри здания.</param>
        /// <returns>Результат Core-нормализации.</returns>
        BuildingUnitNormalizationResult Normalize(string input);
    }
}
