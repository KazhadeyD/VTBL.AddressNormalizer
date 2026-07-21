using VTBL.AddressNormalizer.Abstractions.BuildingAddress;
using VTBL.AddressNormalizer.Abstractions.BuildingUnit;
using VTBL.AddressNormalizer.Abstractions.FieldAdapters.Crm;
using VTBL.AddressNormalizer.Abstractions.Shared;
using VTBL.AddressNormalizer.Infrastructure.Composition;

namespace VTBL.AddressNormalizer.UnitTests
{
    /// <summary>
    /// Общий хост для unit-тестов (singleton-сервисы AddressNormalizer).
    /// </summary>
    internal static class AddressNormalizerTestHost
    {
        public static IBuildingUnitParser Parser => AddressNormalizerFactory.BuildingUnitParser;

        public static IBuildingUnitCanonicalizer Canonicalizer => AddressNormalizerFactory.BuildingUnitCanonicalizer;

        public static IBuildingUnitNormalizer Normalizer => AddressNormalizerFactory.BuildingUnitNormalizer;

        public static IBuildingUnitClassifier Classifier => AddressNormalizerFactory.BuildingUnitClassifier;

        public static ICanonicalHash Hash => AddressNormalizerFactory.CanonicalHash;

        public static ICrmNewFlatNormalizer CrmNewFlat => AddressNormalizerFactory.CrmNewFlatNormalizer;

        public static IBuildingAddressNormalizer BuildingAddress => AddressNormalizerFactory.BuildingAddressNormalizer;
    }
}
