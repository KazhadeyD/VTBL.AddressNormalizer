using Microsoft.Extensions.DependencyInjection;
using CoreLogger = VTBL.AddressNormalizer.Abstractions.Logging.ILogger;

namespace VTBL.AddressNormalizer.WebApi.Logging
{
    /// <summary>
    /// Регистрация ядрового логгера для WebApi host.
    /// </summary>
    public static class AddressNormalizerLoggingServiceCollectionExtensions
    {
        /// <summary>
        /// Регистрирует <see cref="MicrosoftExtensionsAddressNormalizerLogger"/> (MEL → NLog).
        /// Вызывать до <c>AddAddressNormalizer</c>, когда ядро начнёт зависеть от <see cref="CoreLogger"/>.
        /// </summary>
        /// <param name="services">Коллекция DI.</param>
        /// <returns>Та же коллекция для цепочки вызовов.</returns>
        public static IServiceCollection AddAddressNormalizerLogging(this IServiceCollection services)
        {
            services.AddSingleton<CoreLogger, MicrosoftExtensionsAddressNormalizerLogger>();
            return services;
        }
    }
}
