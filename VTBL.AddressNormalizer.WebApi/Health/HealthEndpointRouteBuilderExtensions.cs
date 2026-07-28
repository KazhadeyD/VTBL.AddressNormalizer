using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;

namespace VTBL.AddressNormalizer.WebApi.Health
{
    /// <summary>
    /// Маршруты health endpoints WebApi.
    /// </summary>
    public static class HealthEndpointRouteBuilderExtensions
    {
        /// <summary>
        /// Публикует <c>/health</c> и <c>/health/live</c> с JSON-ответом.
        /// </summary>
        /// <param name="endpoints">Построитель маршрутов.</param>
        /// <returns>Тот же построитель для цепочки вызовов.</returns>
        public static IEndpointRouteBuilder MapWebApiHealthChecks(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapHealthChecks("/health", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = HealthCheckResponseWriter.WriteAsync
            });

            endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("live"),
                ResponseWriter = HealthCheckResponseWriter.WriteAsync
            });

            return endpoints;
        }
    }
}
