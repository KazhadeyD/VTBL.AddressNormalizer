using VTBL.AddressNormalizer.Abstractions.BuildingAddress;
using VTBL.AddressNormalizer.Abstractions.BuildingUnit;
using VTBL.AddressNormalizer.Abstractions.FieldAdapters.Crm;
using VTBL.AddressNormalizer.Abstractions.Shared;
using VTBL.AddressNormalizer.Infrastructure.BuildingAddress;
using VTBL.AddressNormalizer.Infrastructure.BuildingUnit;
using VTBL.AddressNormalizer.Infrastructure.FieldAdapters.Crm;
using VTBL.AddressNormalizer.Infrastructure.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace VTBL.AddressNormalizer.Infrastructure.DependencyInjection
{
    /// <summary>
    /// Регистрация сервисов нормализации адресов в DI-контейнере.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Регистрирует BuildingUnit, BuildingAddress, Shared и CRM-адаптеры как Singleton.
        /// </summary>
        /// <param name="services">Коллекция сервисов.</param>
        /// <returns>Та же коллекция для цепочки вызовов.</returns>
        public static IServiceCollection AddAddressNormalizer(this IServiceCollection services)
        {
            services.AddSingleton<IBuildingUnitParser, BuildingUnitParser>();
            services.AddSingleton<IBuildingUnitCanonicalizer, BuildingUnitCanonicalizer>();
            services.AddSingleton<IBuildingUnitNormalizer, BuildingUnitNormalizer>();
            services.AddSingleton<IBuildingUnitClassifier, BuildingUnitClassifier>();
            services.AddSingleton<ICanonicalHash, CanonicalHash>();
            services.AddSingleton<ICrmNewFlatNormalizer, CrmNewFlatNormalizer>();

            services.AddSingleton<IBuildingLocationExtractor, BuildingLocationExtractor>();
            services.AddSingleton<IBuildingAddressCanonicalizer, BuildingAddressCanonicalizer>();
            services.AddSingleton<IBuildingAddressNormalizer, BuildingAddressNormalizer>();
            services.AddSingleton<ICrmNewAddressNormalizer, CrmNewAddressNormalizer>();

            return services;
        }
    }
}
