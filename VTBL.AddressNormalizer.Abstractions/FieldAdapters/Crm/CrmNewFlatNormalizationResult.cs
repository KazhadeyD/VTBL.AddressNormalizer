using VTBL.AddressNormalizer.Abstractions.BuildingUnit;

namespace VTBL.AddressNormalizer.Abstractions.FieldAdapters.Crm
{
    /// <summary>
    /// Результат нормализации CRM-поля <c>new_flat</c>: категория, структура, канон, JSON и hash.
    /// </summary>
    public sealed class CrmNewFlatNormalizationResult
    {
        /// <summary>
        /// Создаёт результат нормализации CRM-поля <c>new_flat</c>.
        /// </summary>
        /// <param name="original">Исходное значение поля.</param>
        /// <param name="category">Категория по маркерам.</param>
        /// <param name="location">Разобранная модель локации.</param>
        /// <param name="canonical">Каноническая строка.</param>
        /// <param name="json">JSON-представление модели (camelCase).</param>
        /// <param name="hash">SHA256 от canonical.</param>
        public CrmNewFlatNormalizationResult(
            string original,
            BuildingUnitCategory category,
            BuildingUnitLocation location,
            string canonical,
            string json,
            string hash)
        {
            Original = original;
            Category = category;
            Location = location;
            Canonical = canonical;
            Json = json;
            Hash = hash;
        }

        /// <summary>Исходное значение <c>new_flat</c>.</summary>
        public string Original { get; }

        /// <summary>Категория строки по доминирующим маркерам.</summary>
        public BuildingUnitCategory Category { get; }

        /// <summary>Разобранная структура локации.</summary>
        public BuildingUnitLocation Location { get; }

        /// <summary>Каноническая строка для matching.</summary>
        public string Canonical { get; }

        /// <summary>JSON модели (<see cref="BuildingUnitLocation"/>).</summary>
        public string Json { get; }

        /// <summary>SHA256-хеш канонической строки (hex, lowercase).</summary>
        public string Hash { get; }
    }
}
