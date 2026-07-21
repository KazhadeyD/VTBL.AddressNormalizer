namespace VTBL.AddressNormalizer.Abstractions.BuildingAddress
{
    /// <summary>
    /// Извлечение локации здания из полного адреса (отсечение indoor-хвоста).
    /// </summary>
    public interface IBuildingLocationExtractor
    {
        /// <summary>
        /// Извлекает географическую/строительную часть адреса без indoor-сегментов.
        /// </summary>
        /// <param name="input">Полный или частичный адрес.</param>
        /// <returns>Локация здания или пустая строка.</returns>
        string Extract(string input);
    }
}
