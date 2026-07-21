using VTBL.AddressNormalizer.Abstractions.BuildingAddress;
using VTBL.AddressNormalizer.Abstractions.BuildingUnit;
using VTBL.AddressNormalizer.Abstractions.FieldAdapters.Crm;
using VTBL.AddressNormalizer.Abstractions.Shared;
using VTBL.AddressNormalizer.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace VTBL.AddressNormalizer.UnitTests
{
    /// <summary>
    /// Общий DI-хост для unit-тестов (Singleton-сервисы AddressNormalizer).
    /// </summary>
    internal static class AddressNormalizerTestHost
    {
        private static readonly ServiceProvider Provider =
            new ServiceCollection().AddAddressNormalizer().BuildServiceProvider();

        public static IBuildingUnitParser Parser => Provider.GetRequiredService<IBuildingUnitParser>();

        public static IBuildingUnitCanonicalizer Canonicalizer => Provider.GetRequiredService<IBuildingUnitCanonicalizer>();

        public static IBuildingUnitNormalizer Normalizer => Provider.GetRequiredService<IBuildingUnitNormalizer>();

        public static IBuildingUnitClassifier Classifier => Provider.GetRequiredService<IBuildingUnitClassifier>();

        public static ICanonicalHash Hash => Provider.GetRequiredService<ICanonicalHash>();

        public static ICrmNewFlatNormalizer CrmNewFlat => Provider.GetRequiredService<ICrmNewFlatNormalizer>();

        public static IBuildingAddressNormalizer BuildingAddress =>
            Provider.GetRequiredService<IBuildingAddressNormalizer>();
    }
}
