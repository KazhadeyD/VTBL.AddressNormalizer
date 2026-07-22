namespace VTBL.AddressNormalizer.Abstractions.BuildingUnit
{
    /// <summary>
    /// Классификация строки локации внутри здания по маркерам.
    /// </summary>
    public interface IBuildingUnitClassifier
    {
        /// <summary>
        /// Определяет категорию строки по наличию маркеров типов.
        /// </summary>
        /// <param name="input">Сырая строка локации внутри здания.</param>
        /// <returns>Категория по доминирующему формату.</returns>
        BuildingUnitCategory Classify(string input);
    }
}
