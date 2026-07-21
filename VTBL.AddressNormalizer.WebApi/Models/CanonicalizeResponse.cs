namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Ответ canonicalize building location (<c>POST /api/v1/address/canonicalize</c>).
    /// Hash не возвращается.
    /// </summary>
    public class CanonicalizeResponse
    {
        /// <summary>
        /// Исходная строка запроса.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Читаемая каноническая строка building location.
        /// </summary>
        public string Canonical { get; set; }
    }
}
