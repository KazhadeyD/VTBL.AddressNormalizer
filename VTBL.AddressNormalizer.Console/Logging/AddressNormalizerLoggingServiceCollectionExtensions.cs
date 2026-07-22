using Microsoft.Extensions.DependencyInjection;
using CoreLogger = VTBL.AddressNormalizer.Abstractions.Logging.ILogger;

namespace VTBL.AddressNormalizer.Console.Logging
{
    /// <summary>
    /// Регистрация ядрового логгера для Console host.
    /// </summary>
    public static class AddressNormalizerLoggingServiceCollectionExtensions
    {
        /// <summary>
        /// Регистрирует <see cref="ConsoleAddressNormalizerLogger"/> (stdout).
        /// </summary>
        /// <param name="services">Коллекция DI.</param>
        /// <returns>Та же коллекция для цепочки вызовов.</returns>
        public static IServiceCollection AddAddressNormalizerLogging(this IServiceCollection services)
        {
            services.AddSingleton<CoreLogger, ConsoleAddressNormalizerLogger>();
            return services;
        }
    }
}
