using System;
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
    /// Composition root: singleton-экземпляры сервисов нормализации адресов без DI-контейнера.
    /// </summary>
    public static class AddressNormalizerFactory
    {
        private static readonly Lazy<IBuildingUnitParser> BuildingUnitParserLazy =
            new Lazy<IBuildingUnitParser>(() => new BuildingUnitParser());

        private static readonly Lazy<IBuildingUnitCanonicalizer> BuildingUnitCanonicalizerLazy =
            new Lazy<IBuildingUnitCanonicalizer>(() => new BuildingUnitCanonicalizer());

        private static readonly Lazy<IBuildingUnitClassifier> BuildingUnitClassifierLazy =
            new Lazy<IBuildingUnitClassifier>(() => new BuildingUnitClassifier());

        private static readonly Lazy<ICanonicalHash> CanonicalHashLazy =
            new Lazy<ICanonicalHash>(() => new CanonicalHash());

        private static readonly Lazy<IBuildingUnitNormalizer> BuildingUnitNormalizerLazy =
            new Lazy<IBuildingUnitNormalizer>(() => new BuildingUnitNormalizer(
                BuildingUnitParserLazy.Value,
                BuildingUnitCanonicalizerLazy.Value,
                CanonicalHashLazy.Value));

        private static readonly Lazy<ICrmNewFlatNormalizer> CrmNewFlatNormalizerLazy =
            new Lazy<ICrmNewFlatNormalizer>(() => new CrmNewFlatNormalizer(
                BuildingUnitClassifierLazy.Value,
                BuildingUnitParserLazy.Value,
                BuildingUnitCanonicalizerLazy.Value,
                CanonicalHashLazy.Value));

        private static readonly Lazy<IBuildingLocationExtractor> BuildingLocationExtractorLazy =
            new Lazy<IBuildingLocationExtractor>(() => new BuildingLocationExtractor());

        private static readonly Lazy<IBuildingAddressCanonicalizer> BuildingAddressCanonicalizerLazy =
            new Lazy<IBuildingAddressCanonicalizer>(() => new BuildingAddressCanonicalizer());

        private static readonly Lazy<IBuildingAddressNormalizer> BuildingAddressNormalizerLazy =
            new Lazy<IBuildingAddressNormalizer>(() => new BuildingAddressNormalizer(
                BuildingLocationExtractorLazy.Value,
                BuildingAddressCanonicalizerLazy.Value));

        private static readonly Lazy<ICrmNewAddressNormalizer> CrmNewAddressNormalizerLazy =
            new Lazy<ICrmNewAddressNormalizer>(() => new CrmNewAddressNormalizer(
                BuildingAddressNormalizerLazy.Value));

        /// <summary>Парсер локации внутри здания.</summary>
        public static IBuildingUnitParser BuildingUnitParser => BuildingUnitParserLazy.Value;

        /// <summary>Канонизатор BuildingUnit.</summary>
        public static IBuildingUnitCanonicalizer BuildingUnitCanonicalizer => BuildingUnitCanonicalizerLazy.Value;

        /// <summary>Классификатор BuildingUnit.</summary>
        public static IBuildingUnitClassifier BuildingUnitClassifier => BuildingUnitClassifierLazy.Value;

        /// <summary>SHA256-хеш канонической строки.</summary>
        public static ICanonicalHash CanonicalHash => CanonicalHashLazy.Value;

        /// <summary>Core-нормализатор BuildingUnit.</summary>
        public static IBuildingUnitNormalizer BuildingUnitNormalizer => BuildingUnitNormalizerLazy.Value;

        /// <summary>CRM-адаптер для <c>new_flat</c>.</summary>
        public static ICrmNewFlatNormalizer CrmNewFlatNormalizer => CrmNewFlatNormalizerLazy.Value;

        /// <summary>Extract outdoor/indoor (<see cref="IBuildingLocationExtractor.ExtractSplit"/>) и outdoor-only Extract.</summary>
        public static IBuildingLocationExtractor BuildingLocationExtractor => BuildingLocationExtractorLazy.Value;

        /// <summary>Канонизатор building location.</summary>
        public static IBuildingAddressCanonicalizer BuildingAddressCanonicalizer => BuildingAddressCanonicalizerLazy.Value;

        /// <summary>End-to-end нормализация полного адреса.</summary>
        public static IBuildingAddressNormalizer BuildingAddressNormalizer => BuildingAddressNormalizerLazy.Value;

        /// <summary>CRM-адаптер для <c>new_address</c> (stub, фаза 2).</summary>
        public static ICrmNewAddressNormalizer CrmNewAddressNormalizer => CrmNewAddressNormalizerLazy.Value;
    }
}
