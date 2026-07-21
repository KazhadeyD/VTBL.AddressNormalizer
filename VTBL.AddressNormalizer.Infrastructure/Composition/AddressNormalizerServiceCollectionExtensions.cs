using Microsoft.Extensions.DependencyInjection;
using VTBL.AddressNormalizer.Abstractions.BuildingAddress;
using VTBL.AddressNormalizer.Abstractions.BuildingUnit;
using VTBL.AddressNormalizer.Abstractions.FieldAdapters.Crm;
using VTBL.AddressNormalizer.Abstractions.Shared;
using VTBL.AddressNormalizer.Infrastructure.BuildingAddress;
using VTBL.AddressNormalizer.Infrastructure.BuildingUnit;
using VTBL.AddressNormalizer.Infrastructure.FieldAdapters.Crm;
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
        /// </summary>
        /// <param name="services">Коллекция DI.</param>
        /// <returns>Та же коллекция для цепочки вызовов.</returns>
        public static IServiceCollection AddAddressNormalizer(this IServiceCollection services)
        {
            services.AddSingleton<IBuildingUnitParser, BuildingUnitParser>();
            services.AddSingleton<IBuildingUnitCanonicalizer, BuildingUnitCanonicalizer>();
            services.AddSingleton<IBuildingUnitClassifier, BuildingUnitClassifier>();
            services.AddSingleton<ICanonicalHash, CanonicalHash>();
            services.AddSingleton<IBuildingUnitNormalizer, BuildingUnitNormalizer>();
            services.AddSingleton<ICrmNewFlatNormalizer, CrmNewFlatNormalizer>();
            services.AddSingleton<IBuildingLocationExtractor, BuildingLocationExtractor>();
            services.AddSingleton<IBuildingAddressCanonicalizer, BuildingAddressCanonicalizer>();
            services.AddSingleton<IBuildingAddressNormalizer, BuildingAddressNormalizer>();
            services.AddSingleton<ICrmNewAddressNormalizer, CrmNewAddressNormalizer>();
            return services;
        }
    }
}
