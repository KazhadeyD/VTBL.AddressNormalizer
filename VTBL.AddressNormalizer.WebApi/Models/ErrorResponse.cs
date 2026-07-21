namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Тело ошибки (HTTP 400 / 500): одно поле <c>error</c>.
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Человекочитаемый текст ошибки.
        /// </summary>
        /// <example>source is required</example>
        public string Error { get; set; }
    }
}
