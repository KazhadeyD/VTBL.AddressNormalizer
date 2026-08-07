namespace VTBL.AddressNormalizer.Abstractions.BuildingAddress
{
    /// <summary>
    /// Извлечение локации здания из полного адреса (отсечение indoor-хвоста).
    /// </summary>
    public interface IBuildingLocationExtractor
    {
        /// <summary>
        /// Разбивает адрес на outdoor- и indoor-части.
        /// </summary>
        /// <remarks>
        /// После маркера дома: <c>к</c>/<c>к.</c> сразу после дома — корпус (остаётся в outdoor);
        /// <c>кв</c> или <c>к</c>/<c>к.</c> после промежуточных токенов — outdoor обрезается сразу после дома,
        /// весь хвост уходит в indoor; иначе cut перед самым левым прочим indoor-маркером
        /// (<c>эт</c>, <c>пом</c>, <c>ком</c>, …). Indoor-строка начинается с маркера cut либо с первого
        /// токена хвоста после cut сразу за домом. Без хвостовых <c>,</c>/<c>;</c>/пробелов в outdoor.
        /// </remarks>
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
