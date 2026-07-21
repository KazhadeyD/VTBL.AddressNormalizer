using System;
using Microsoft.Extensions.DependencyInjection;
using VTBL.AddressNormalizer.Abstractions.BuildingAddress;
using VTBL.AddressNormalizer.Abstractions.BuildingUnit;
using VTBL.AddressNormalizer.Abstractions.FieldAdapters.Crm;
using VTBL.AddressNormalizer.Abstractions.Shared;
using VTBL.AddressNormalizer.Infrastructure.Composition;

namespace VTBL.AddressNormalizer.UnitTests
{
    /// <summary>
    /// Общий хост для unit-тестов: ядро через <see cref="AddressNormalizerServiceCollectionExtensions.AddAddressNormalizer"/>.
    /// </summary>
    internal static class AddressNormalizerTestHost
    {
        private static readonly Lazy<IServiceProvider> ProviderLazy =
            new Lazy<IServiceProvider>(BuildProvider);

        private static IServiceProvider Provider => ProviderLazy.Value;

        public static IBuildingUnitParser Parser =>
            Provider.GetRequiredService<IBuildingUnitParser>();

        public static IBuildingUnitCanonicalizer Canonicalizer =>
            Provider.GetRequiredService<IBuildingUnitCanonicalizer>();

        public static IBuildingUnitNormalizer Normalizer =>
            Provider.GetRequiredService<IBuildingUnitNormalizer>();

        public static IBuildingUnitClassifier Classifier =>
            Provider.GetRequiredService<IBuildingUnitClassifier>();

        public static ICanonicalHash Hash =>
            Provider.GetRequiredService<ICanonicalHash>();

        public static ICrmNewFlatNormalizer CrmNewFlat =>
            Provider.GetRequiredService<ICrmNewFlatNormalizer>();

        public static IBuildingAddressNormalizer BuildingAddress =>
            Provider.GetRequiredService<IBuildingAddressNormalizer>();

        public static IBuildingLocationExtractor BuildingLocationExtractor =>
            Provider.GetRequiredService<IBuildingLocationExtractor>();

        public static IBuildingAddressCanonicalizer BuildingAddressCanonicalizer =>
            Provider.GetRequiredService<IBuildingAddressCanonicalizer>();

        private static IServiceProvider BuildProvider()
        {
            var services = new ServiceCollection();
            services.AddAddressNormalizer();
            return services.BuildServiceProvider();
        }
    }
}
