using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VTBL.AddressNormalizer.Abstractions.BuildingAddress;
using VTBL.AddressNormalizer.Abstractions.BuildingUnit;
using VTBL.AddressNormalizer.Abstractions.Logging;
using VTBL.AddressNormalizer.Abstractions.Shared;
using VTBL.AddressNormalizer.Infrastructure.BuildingAddress;
using VTBL.AddressNormalizer.Infrastructure.BuildingUnit;
using VTBL.AddressNormalizer.Infrastructure.Shared;

namespace VTBL.AddressNormalizer.Infrastructure.Composition
{
    /// <summary>
    /// Регистрация сервисов ядра нормализации в <see cref="IServiceCollection"/>.
    /// </summary>
    public static class AddressNormalizerServiceCollectionExtensions
    {
        /// <summary>
        /// Регистрирует singleton-реализации ядра AddressNormalizer.
        /// Единый composition root для WebApi, Console и тестов.
        /// Если <see cref="ILogger"/> ещё не зарегистрирован — подставляет <see cref="NullLogger"/>
        /// (хост может вызвать <c>AddAddressNormalizerLogging</c> раньше).
        /// </summary>
        /// <param name="services">Коллекция DI.</param>
        /// <returns>Та же коллекция для цепочки вызовов.</returns>
        public static IServiceCollection AddAddressNormalizer(this IServiceCollection services)
        {
            services.TryAddSingleton<ILogger>(NullLogger.Instance);

            services.AddSingleton<IBuildingUnitParser, BuildingUnitParser>();
            services.AddSingleton<IBuildingUnitCanonicalizer, BuildingUnitCanonicalizer>();
            services.AddSingleton<ICanonicalHash, CanonicalHash>();
            services.AddSingleton<IBuildingLocationExtractor, BuildingLocationExtractor>();
            services.AddSingleton<IBuildingAddressCanonicalizer, BuildingAddressCanonicalizer>();
            services.AddSingleton<IBuildingAddressNormalizer, BuildingAddressNormalizer>();
            return services;
        }
    }
}
