namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Ответ <c>GET /health</c>.
    /// </summary>
    public class HealthResponse
    {
        /// <summary>
        /// Статус сервиса; при успехе всегда <c>Healthy</c>.
        /// </summary>
        /// <example>Healthy</example>
        public string Status { get; set; }
    }
}
