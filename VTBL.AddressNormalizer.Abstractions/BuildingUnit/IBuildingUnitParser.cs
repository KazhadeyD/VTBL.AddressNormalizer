namespace VTBL.AddressNormalizer.Abstractions.BuildingUnit
{
    /// <summary>
    /// Разбор локации внутри здания в структурированную модель.
    /// </summary>
    public interface IBuildingUnitParser
    {
        /// <summary>
        /// Разбирает строку локации внутри здания.
        /// </summary>
        /// <param name="input">Значение поля или произвольная строка локации.</param>
        /// <returns>Заполненная модель локации.</returns>
        BuildingUnitLocation Parse(string input);
    }
}
