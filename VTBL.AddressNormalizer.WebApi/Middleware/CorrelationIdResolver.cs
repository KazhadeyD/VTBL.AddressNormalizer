using System;

namespace VTBL.AddressNormalizer.WebApi.Middleware
{
    /// <summary>
    /// Чистая функция выбора Correlation Id.
    /// </summary>
    public static class CorrelationIdResolver
    {
        /// <summary>
        /// Имя request/response заголовка идентификатора запроса VTBL.
        /// </summary>
        public const string RequestIdHeaderName = "X-VTBL-Request-ID";

        /// <summary>
        /// Ключ NLog MDLC / ScopeContext для Correlation Id.
        /// </summary>
        public const string MdlcKey = "CorrelationId";

        /// <summary>
        /// Выбирает Correlation Id: non-whitespace <paramref name="requestIdHeader"/>,
        /// иначе новый GUID ("D").
        /// Whitespace и пустая строка считаются отсутствием заголовка.
        /// </summary>
        public static string Resolve(string requestIdHeader)
        {
            if (!string.IsNullOrWhiteSpace(requestIdHeader))
                return requestIdHeader;

            return Guid.NewGuid().ToString("D");
        }
    }
}
