namespace VTBL.AddressNormalizer.Abstractions.BuildingUnit
{
    /// <summary>
    /// Сборка канонической строки из структуры локации внутри здания.
    /// </summary>
    public interface IBuildingUnitCanonicalizer
    {
        /// <summary>
        /// Собирает каноническую строку из заполненной модели локации.
        /// </summary>
        /// <param name="location">Структурированная локация; <c>null</c> → пустая строка.</param>
        /// <returns>Канон вида «эт:4|пом:2|code:659318», сегменты отсортированы.</returns>
        string ToCanonical(BuildingUnitLocation location);
    }
}
