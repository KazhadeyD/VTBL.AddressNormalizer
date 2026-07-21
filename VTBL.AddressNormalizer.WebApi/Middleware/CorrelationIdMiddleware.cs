using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NLog;

namespace VTBL.AddressNormalizer.WebApi.Middleware
{
    /// <summary>
    /// Correlation Id (UC-07 / F-API-07): выбор Id, NLog MDLC, echo response header.
    /// </summary>
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Создаёт middleware Correlation Id.
        /// </summary>
        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        /// <summary>
        /// Разрешает Id, устанавливает MDLC до следующего middleware и пишет <c>X-Correlation-Id</c>.
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            var correlationHeader = GetHeader(context, CorrelationIdResolver.CorrelationIdHeaderName);
            var requestHeader = GetHeader(context, CorrelationIdResolver.RequestIdHeaderName);
            var correlationId = CorrelationIdResolver.Resolve(correlationHeader, requestHeader);

            // ScopeContext питает ${mdlc:item=CorrelationId} в NLog 5 (эквивалент MDLC).
            using (ScopeContext.PushProperty(CorrelationIdResolver.MdlcKey, correlationId))
            {
                context.Response.OnStarting(() =>
                {
                    context.Response.Headers[CorrelationIdResolver.CorrelationIdHeaderName] = correlationId;
                    return Task.CompletedTask;
                });

                await _next(context);
            }
        }

        private static string GetHeader(HttpContext context, string name)
        {
            if (!context.Request.Headers.TryGetValue(name, out var values) || values.Count == 0)
                return null;

            return values[0];
        }
    }
}
