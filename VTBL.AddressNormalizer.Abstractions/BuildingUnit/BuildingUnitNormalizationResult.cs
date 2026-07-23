namespace VTBL.AddressNormalizer.Abstractions.BuildingUnit
{
    /// <summary>
    /// Результат нормализации локации внутри здания: структура, канон, JSON и hash.
    /// </summary>
    public sealed class BuildingUnitNormalizationResult
    {
        /// <summary>
        /// Создаёт результат Core-нормализации.
        /// </summary>
        /// <param name="original">Исходная строка.</param>
        /// <param name="location">Разобранная модель.</param>
        /// <param name="canonical">Каноническая строка.</param>
        /// <param name="json">JSON-представление модели (camelCase).</param>
        /// <param name="hash">SHA256 от canonical.</param>
        public BuildingUnitNormalizationResult(
            string original,
            BuildingUnitLocation location,
            string canonical,
            string json,
            string hash)
        {
            Original = original;
            Location = location;
            Canonical = canonical;
            Json = json;
            Hash = hash;
        }

        /// <summary>
        /// Исходная строка до нормализации.
        /// </summary>
        public string Original { get; }

        /// <summary>
        /// Разобранная структура локации.
        /// </summary>
        public BuildingUnitLocation Location { get; }

        /// <summary>
        /// Каноническая строка для matching.
        /// </summary>
        public string Canonical { get; }

        /// <summary>
        /// JSON модели (<see cref="BuildingUnitLocation"/>).
        /// </summary>
        public string Json { get; }

        /// <summary>
        /// SHA256-хеш канонической строки (hex, lowercase).
        /// </summary>
        public string Hash { get; }
    }
}
