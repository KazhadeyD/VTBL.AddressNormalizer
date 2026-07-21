using VTBL.AddressNormalizer.Abstractions.BuildingUnit;
using VTBL.AddressNormalizer.Infrastructure.Composition;
using Xunit;

namespace VTBL.AddressNormalizer.UnitTests.Composition
{
    /// <summary>
    /// Проверка composition root <see cref="AddressNormalizerFactory"/>.
    /// </summary>
    public class AddressNormalizerFactoryTests
    {
        [Fact]
        public void Factory_ExposesIBuildingUnitParser()
        {
            Assert.NotNull(AddressNormalizerFactory.BuildingUnitParser);
        }

        [Fact]
        public void Factory_ExposesFullBuildingUnitChain()
        {
            Assert.NotNull(AddressNormalizerFactory.BuildingUnitParser);
            Assert.NotNull(AddressNormalizerFactory.BuildingUnitCanonicalizer);
            Assert.NotNull(AddressNormalizerFactory.BuildingUnitNormalizer);
            Assert.NotNull(AddressNormalizerFactory.BuildingUnitClassifier);
        }
    }
}
