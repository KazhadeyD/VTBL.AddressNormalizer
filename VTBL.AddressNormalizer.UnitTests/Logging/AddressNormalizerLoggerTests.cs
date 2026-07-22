using System;
using System.IO;
using VTBL.AddressNormalizer.Abstractions.Logging;
using VTBL.AddressNormalizer.Console.Logging;
using Xunit;

namespace VTBL.AddressNormalizer.UnitTests.Logging
{
    /// <summary>
    /// Контракт ядрового <see cref="ILogger"/> и Console-реализации.
    /// </summary>
    public class AddressNormalizerLoggerTests
    {
        [Fact]
        public void NullLogger_AllMethods_DoNotThrow()
        {
            var logger = NullLogger.Instance;

            logger.Debug("debug");
            logger.Information("info");
            logger.Warning("warn");
            logger.Error("error");
            logger.Error(new InvalidOperationException("x"), "with ex");
        }

        [Fact]
        public void ConsoleAddressNormalizerLogger_WritesPrefixedLine()
        {
            var original = System.Console.Out;
            try
            {
                using var writer = new StringWriter();
                System.Console.SetOut(writer);

                var logger = new ConsoleAddressNormalizerLogger();
                logger.Information("проверка");

                var line = writer.ToString().TrimEnd();
                Assert.Contains("[ИНФ] AddressNormalizer: проверка", line);
            }
            finally
            {
                System.Console.SetOut(original);
            }
        }
    }
}
