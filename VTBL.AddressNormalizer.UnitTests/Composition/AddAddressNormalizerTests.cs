using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VTBL.AddressNormalizer.Abstractions.BuildingAddress;
using VTBL.AddressNormalizer.Abstractions.BuildingUnit;
using VTBL.AddressNormalizer.Abstractions.Shared;
using VTBL.AddressNormalizer.Infrastructure.Composition;
using Xunit;
using CoreLogger = VTBL.AddressNormalizer.Abstractions.Logging.ILogger;
using ConsoleAddressNormalizerLogger = VTBL.AddressNormalizer.Console.Logging.ConsoleAddressNormalizerLogger;
using WebApiAddressNormalizerLogger = VTBL.AddressNormalizer.WebApi.Logging.MicrosoftExtensionsAddressNormalizerLogger;

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
            Assert.NotNull(provider.GetRequiredService<IBuildingUnitClassifier>());
            Assert.NotNull(provider.GetRequiredService<ICanonicalHash>());
            Assert.NotNull(provider.GetRequiredService<IBuildingLocationExtractor>());
            Assert.NotNull(provider.GetRequiredService<IBuildingAddressCanonicalizer>());
            Assert.NotNull(provider.GetRequiredService<IBuildingAddressNormalizer>());
        }

        [Fact]
        public void AddAddressNormalizerLogging_WebApi_ResolvesAdapter()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            VTBL.AddressNormalizer.WebApi.Logging.AddressNormalizerLoggingServiceCollectionExtensions
                .AddAddressNormalizerLogging(services);
            var logger = services.BuildServiceProvider().GetRequiredService<CoreLogger>();
            Assert.IsType<WebApiAddressNormalizerLogger>(logger);
        }

        [Fact]
        public void AddAddressNormalizerLogging_Console_ResolvesAdapter()
        {
            var services = new ServiceCollection();
            VTBL.AddressNormalizer.Console.Logging.AddressNormalizerLoggingServiceCollectionExtensions
                .AddAddressNormalizerLogging(services);
            var logger = services.BuildServiceProvider().GetRequiredService<CoreLogger>();
            Assert.IsType<ConsoleAddressNormalizerLogger>(logger);
        }

        private static ServiceProvider BuildProvider()
        {
            var services = new ServiceCollection();
            services.AddAddressNormalizer();
            return services.BuildServiceProvider();
        }
    }
}
