namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Ответ extract outdoor (<c>POST /api/v1/address/extract</c>).
    /// </summary>
    public class ExtractResponse
    {
        /// <summary>
        /// Исходная строка запроса.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Извлечённая outdoor-часть (без indoor-хвоста).
        /// </summary>
        public string Extracted { get; set; }
    }
}
