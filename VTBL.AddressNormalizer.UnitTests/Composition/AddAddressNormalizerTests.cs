using Microsoft.Extensions.DependencyInjection;
using VTBL.AddressNormalizer.Abstractions.BuildingAddress;
using VTBL.AddressNormalizer.Abstractions.BuildingUnit;
using VTBL.AddressNormalizer.Abstractions.Shared;
using VTBL.AddressNormalizer.Infrastructure.Composition;
using Xunit;

namespace VTBL.AddressNormalizer.UnitTests.Composition
{
    /// <summary>
    /// Проверка DI-регистрации ядра через <see cref="AddressNormalizerServiceCollectionExtensions.AddAddressNormalizer"/>.
    /// </summary>
    public class AddAddressNormalizerTests
    {
        [Fact]
        public void AddAddressNormalizer_ResolvesIBuildingUnitParser()
        {
            var provider = BuildProvider();
            Assert.NotNull(provider.GetRequiredService<IBuildingUnitParser>());
        }

        [Fact]
        public void AddAddressNormalizer_ResolvesFullCoreGraph()
        {
            var provider = BuildProvider();

            Assert.NotNull(provider.GetRequiredService<IBuildingUnitParser>());
            Assert.NotNull(provider.GetRequiredService<IBuildingUnitCanonicalizer>());
            Assert.NotNull(provider.GetRequiredService<IBuildingUnitNormalizer>());
            Assert.NotNull(provider.GetRequiredService<IBuildingUnitClassifier>());
            Assert.NotNull(provider.GetRequiredService<ICanonicalHash>());
            Assert.NotNull(provider.GetRequiredService<IBuildingLocationExtractor>());
            Assert.NotNull(provider.GetRequiredService<IBuildingAddressCanonicalizer>());
            Assert.NotNull(provider.GetRequiredService<IBuildingAddressNormalizer>());
        }

        private static ServiceProvider BuildProvider()
        {
            var services = new ServiceCollection();
            services.AddAddressNormalizer();
            return services.BuildServiceProvider();
        }
    }
}
