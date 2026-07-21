namespace VTBL.AddressNormalizer.Abstractions.BuildingAddress
{
    /// <summary>
    /// Читаемая канонизация локации здания (типы, пробелы, регистр).
    /// </summary>
    public interface IBuildingAddressCanonicalizer
    {
        /// <summary>
        /// Приводит строку локации здания к читаемому каноническому виду.
        /// </summary>
        /// <param name="input">Извлечённая локация здания или адрес без indoor.</param>
        /// <returns>Каноническая читаемая строка.</returns>
        string ToCanonical(string input);
    }
}
