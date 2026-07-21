namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Ответ нормализации indoor/unit (<c>POST /api/v1/unit/normalize</c>).
    /// </summary>
    public class UnitNormalizeResponse
    {
        /// <summary>
        /// Исходная строка запроса.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Indoor в формате варианта B (все 17 категорий).
        /// </summary>
        public IndoorValueDto IndoorValue { get; set; }

        /// <summary>
        /// Каноническая строка unit (для matching).
        /// </summary>
        public string Canonical { get; set; }

        /// <summary>
        /// SHA256 hex (lowercase) от <see cref="Canonical"/>.
        /// </summary>
        public string Hash { get; set; }
    }
}
