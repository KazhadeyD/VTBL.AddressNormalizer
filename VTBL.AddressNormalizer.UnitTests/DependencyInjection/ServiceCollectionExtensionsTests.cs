using VTBL.AddressNormalizer.Abstractions.BuildingUnit;
using VTBL.AddressNormalizer.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace VTBL.AddressNormalizer.UnitTests.DependencyInjection
{
    /// <summary>
    /// Проверка регистрации сервисов в <c>AddAddressNormalizer</c>.
    /// </summary>
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAddressNormalizer_ResolvesIBuildingUnitParser()
        {
            using (var provider = new ServiceCollection()
                .AddAddressNormalizer()
                .BuildServiceProvider())
            {
                var parser = provider.GetRequiredService<IBuildingUnitParser>();

                Assert.NotNull(parser);
            }
        }

        [Fact]
        public void AddAddressNormalizer_ResolvesFullBuildingUnitChain()
        {
            using (var provider = new ServiceCollection()
                .AddAddressNormalizer()
                .BuildServiceProvider())
            {
                Assert.NotNull(provider.GetRequiredService<IBuildingUnitParser>());
                Assert.NotNull(provider.GetRequiredService<IBuildingUnitCanonicalizer>());
                Assert.NotNull(provider.GetRequiredService<IBuildingUnitNormalizer>());
                Assert.NotNull(provider.GetRequiredService<IBuildingUnitClassifier>());
            }
        }
    }
}
