using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace VTBL.AddressNormalizer.WebApi.Health
{
    /// <summary>
    /// Регистрация health checks WebApi.
    /// </summary>
    public static class HealthServiceCollectionExtensions
    {
        /// <summary>
        /// Подключает liveness и readiness проверки сервиса.
        /// </summary>
        /// <param name="services">Коллекция DI.</param>
        /// <returns>Та же коллекция для цепочки вызовов.</returns>
        public static IServiceCollection AddWebApiHealthChecks(this IServiceCollection services)
        {
            services
                .AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy("HTTP host запущен."), tags: new[] { "live" })
                .AddCheck<AddressNormalizerReadinessHealthCheck>(
                    "address_normalizer_readiness",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { "ready" });

            return services;
        }
    }
}
