namespace VTBL.AddressNormalizer.Abstractions.BuildingAddress
{
    /// <summary>
    /// Результат разбиения адреса на outdoor- и indoor-части (после preprocess extractor’а).
    /// </summary>
    public sealed class BuildingLocationExtractionResult
    {
        /// <summary>
        /// Создаёт результат extract split.
        /// </summary>
        /// <param name="outdoor">Часть до точки отсечения indoor-маркера; null нормализуется в пустую строку.</param>
        /// <param name="indoor">Хвост начиная с первого indoor-маркера; null нормализуется в пустую строку.</param>
        public BuildingLocationExtractionResult(string outdoor, string indoor)
        {
            Outdoor = outdoor ?? string.Empty;
            Indoor = indoor ?? string.Empty;
        }

        /// <summary>
        /// Outdoor-часть адреса (никогда не null).
        /// </summary>
        public string Outdoor { get; }

        /// <summary>
        /// Indoor-часть адреса (никогда не null; при отсутствии маркеров — пустая строка).
        /// </summary>
        public string Indoor { get; }
    }
}
