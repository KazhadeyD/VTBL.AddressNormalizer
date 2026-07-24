using Microsoft.Extensions.DependencyInjection;
using VTBL.AddressNormalizer.Abstractions.BuildingAddress;
using VTBL.AddressNormalizer.Abstractions.Logging;
using VTBL.AddressNormalizer.Infrastructure.Composition;
using Xunit;
using CoreLogger = VTBL.AddressNormalizer.Abstractions.Logging.ILogger;

namespace VTBL.AddressNormalizer.UnitTests.Logging
{
    /// <summary>
    /// Debug-логи границ Infrastructure (длины / counts, без полного адреса).
    /// </summary>
    public class InfrastructureLoggingTests
    {
        [Fact]
        public void AddAddressNormalizer_WithoutHostLogger_RegistersNullLogger()
        {
            var services = new ServiceCollection();
            services.AddAddressNormalizer();
            var logger = services.BuildServiceProvider().GetRequiredService<CoreLogger>();
            Assert.Same(NullLogger.Instance, logger);
        }

        [Fact]
        public void AddAddressNormalizer_DoesNotOverrideHostLogger()
        {
            var capturing = new CapturingLogger();
            var services = new ServiceCollection();
            services.AddSingleton<CoreLogger>(capturing);
            services.AddAddressNormalizer();

            var logger = services.BuildServiceProvider().GetRequiredService<CoreLogger>();
            Assert.Same(capturing, logger);
        }

        [Fact]
        public void ExtractSplit_LogsDebugSummaryWithoutAddressText()
        {
            var capturing = new CapturingLogger();
            var provider = BuildWithLogger(capturing);
            var extractor = provider.GetRequiredService<IBuildingLocationExtractor>();

            extractor.ExtractSplit("г Москва, ул Сухонская, д 11, кв 89");

            Assert.Single(capturing.DebugMessages);
            var msg = capturing.DebugMessages[0];
            Assert.Contains("ExtractSplit:", msg);
            Assert.Contains("длина входа=", msg);
            Assert.Contains("длина outdoor=", msg);
            Assert.Contains("длина indoor=", msg);
            Assert.Contains("есть маркер дома=да", msg);
            Assert.DoesNotContain("Сухонская", msg);
            Assert.DoesNotContain("кв 89", msg);
        }

        private static ServiceProvider BuildWithLogger(CoreLogger logger)
        {
            var services = new ServiceCollection();
            services.AddSingleton(logger);
            services.AddAddressNormalizer();
            return services.BuildServiceProvider();
        }
    }
}
