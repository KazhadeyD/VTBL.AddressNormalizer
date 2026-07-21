namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Ответ полной нормализации адреса (<c>POST /api/v1/normalize</c>).
    /// </summary>
    public class NormalizeResponse
    {
        /// <summary>
        /// Исходная строка запроса (как пришла в <c>source</c>).
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Результат: заглушка FIAS, outdoor-блок и structured indoor.
        /// </summary>
        public NormalizeValueDto Value { get; set; }
    }
}
