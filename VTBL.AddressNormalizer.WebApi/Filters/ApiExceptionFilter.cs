using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using VTBL.AddressNormalizer.WebApi.Models;

namespace VTBL.AddressNormalizer.WebApi.Filters
{
    /// <summary>
    /// Global exception filter: unhandled → HTTP 500 + JSON <c>{ "error": "..." }</c>, лог Error.
    /// Correlation Id подставляется в layout NLog из MDLC (ключ <c>CorrelationId</c>).
    /// Не перехватывает per-item ошибки batch — они не выбрасываются из сервиса как unhandled.
    /// </summary>
    public class ApiExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<ApiExceptionFilter> _logger;

        /// <summary>
        /// Создаёт фильтр необработанных исключений.
        /// </summary>
        public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Обрабатывает необработанное исключение: лог Error и ответ 500 <c>{ error }</c>.
        /// </summary>
        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "Unhandled exception");

            context.Result = new ObjectResult(new ErrorResponse
            {
                Error = context.Exception.Message
            })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };

            context.ExceptionHandled = true;
        }
    }
}
