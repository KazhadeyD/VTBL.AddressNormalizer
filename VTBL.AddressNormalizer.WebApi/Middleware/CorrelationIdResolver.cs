using System;

namespace VTBL.AddressNormalizer.WebApi.Middleware
{
    /// <summary>
    /// Чистая функция выбора Correlation Id по алгоритму F-API-07 / UC-07.
    /// </summary>
    public static class CorrelationIdResolver
    {
        /// <summary>Имя response/request заголовка Correlation Id.</summary>
        public const string CorrelationIdHeaderName = "X-Correlation-Id";

        /// <summary>Имя request заголовка Request Id (fallback).</summary>
        public const string RequestIdHeaderName = "X-Request-Id";

        /// <summary>Ключ NLog MDLC / ScopeContext для Correlation Id.</summary>
        public const string MdlcKey = "CorrelationId";

        /// <summary>
        /// Выбирает Correlation Id: non-whitespace <paramref name="correlationIdHeader"/>,
        /// иначе non-whitespace <paramref name="requestIdHeader"/>, иначе новый GUID ("D").
        /// Whitespace и пустая строка считаются отсутствием заголовка.
        /// </summary>
        public static string Resolve(string correlationIdHeader, string requestIdHeader)
        {
            if (!string.IsNullOrWhiteSpace(correlationIdHeader))
                return correlationIdHeader;

            if (!string.IsNullOrWhiteSpace(requestIdHeader))
                return requestIdHeader;

            return Guid.NewGuid().ToString("D");
        }
    }
}
