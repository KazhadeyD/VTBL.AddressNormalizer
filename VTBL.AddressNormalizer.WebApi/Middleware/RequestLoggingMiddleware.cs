using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace VTBL.AddressNormalizer.WebApi.Middleware
{
    /// <summary>
    /// Логирует исход HTTP-запроса: method, path, status code, duration.
    /// Correlation Id подставляется из NLog MDLC (middleware Correlation должен идти раньше).
    /// </summary>
    public sealed class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        /// <summary>
        /// Создаёт middleware логирования HTTP-ответов.
        /// </summary>
        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Выполняет pipeline и пишет лог с HTTP-статусом.
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var skip = ShouldSkip(context);
            var sw = skip ? null : Stopwatch.StartNew();

            try
            {
                await _next(context);
            }
            finally
            {
                if (!skip && sw != null)
                {
                    sw.Stop();
                    LogCompleted(
                        context.Request.Method,
                        context.Request.Path.Value ?? string.Empty,
                        context.Response.StatusCode,
                        sw.ElapsedMilliseconds);
                }
            }
        }

        private void LogCompleted(string method, string path, int statusCode, long elapsedMs)
        {
            const string message = "HTTP {Method} {Path} → {StatusCode} за {ElapsedMs} ms";

            if (statusCode >= 500)
            {
                _logger.LogError(message, method, path, statusCode, elapsedMs);
                return;
            }

            if (statusCode >= 400)
            {
                _logger.LogWarning(message, method, path, statusCode, elapsedMs);
                return;
            }

            _logger.LogInformation(message, method, path, statusCode, elapsedMs);
        }

        /// <summary>
        /// Health и Swagger не логируем — шум без пользы для бизнес-трафика.
        /// </summary>
        private static bool ShouldSkip(HttpContext context)
        {
            var path = context.Request.Path.Value;
            if (string.IsNullOrEmpty(path))
                return false;

            return path.StartsWith("/health", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase);
        }
    }
}
