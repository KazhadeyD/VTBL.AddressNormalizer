namespace VTBL.AddressNormalizer.Abstractions.BuildingAddress
{
    /// <summary>
    /// Извлечение локации здания из полного адреса (отсечение indoor-хвоста).
    /// </summary>
    public interface IBuildingLocationExtractor
    {
        /// <summary>
        /// Разбивает адрес на outdoor- и indoor-части:
        /// outdoor — до точки отсечения indoor-маркера (без хвостовых разделителей);
        /// indoor — хвост начиная с первого indoor-маркера (маркер входит в строку).
        /// </summary>
        /// <param name="input">Сырая адресная строка (может быть null/empty).</param>
        /// <returns>
        /// Результат с <see cref="BuildingLocationExtractionResult.Outdoor"/> и
        /// <see cref="BuildingLocationExtractionResult.Indoor"/>; оба свойства never-null
        /// (<c>Indoor</c> при отсутствии маркеров — пустая строка).
        /// </returns>
        BuildingLocationExtractionResult ExtractSplit(string input);

        /// <summary>
        /// Извлекает географическую/строительную часть адреса без indoor-сегментов.
        /// Эквивалентно <c>ExtractSplit(input).Outdoor</c>.
        /// </summary>
        /// <param name="input">Полный или частичный адрес.</param>
        /// <returns>Локация здания или пустая строка.</returns>
        string Extract(string input);
    }
}
