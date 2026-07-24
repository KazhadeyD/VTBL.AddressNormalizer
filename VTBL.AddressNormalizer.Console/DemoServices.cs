using System;
using Microsoft.Extensions.DependencyInjection;
using VTBL.AddressNormalizer.Abstractions.BuildingAddress;
using VTBL.AddressNormalizer.Abstractions.BuildingUnit;
using VTBL.AddressNormalizer.Abstractions.Shared;
using VTBL.AddressNormalizer.Console.Logging;
using VTBL.AddressNormalizer.Infrastructure.Composition;

namespace VTBL.AddressNormalizer.Console
{
    /// <summary>
    /// DI-хост демо: ядро через <see cref="AddressNormalizerServiceCollectionExtensions.AddAddressNormalizer"/>.
    /// </summary>
    internal static class DemoServices
    {
        private static readonly Lazy<IServiceProvider> ProviderLazy =
            new Lazy<IServiceProvider>(BuildProvider);

        private static IServiceProvider Provider => ProviderLazy.Value;

        public static IBuildingAddressNormalizer BuildingAddressNormalizer =>
            Provider.GetRequiredService<IBuildingAddressNormalizer>();

        public static IBuildingUnitParser BuildingUnitParser =>
            Provider.GetRequiredService<IBuildingUnitParser>();

        public static IBuildingUnitCanonicalizer BuildingUnitCanonicalizer =>
            Provider.GetRequiredService<IBuildingUnitCanonicalizer>();

        public static ICanonicalHash CanonicalHash =>
            Provider.GetRequiredService<ICanonicalHash>();

        public static IBuildingUnitClassifier BuildingUnitClassifier =>
            Provider.GetRequiredService<IBuildingUnitClassifier>();

        private static IServiceProvider BuildProvider()
        {
            var services = new ServiceCollection();
            services.AddAddressNormalizerLogging();
            services.AddAddressNormalizer();
            return services.BuildServiceProvider();
        }
    }
}
