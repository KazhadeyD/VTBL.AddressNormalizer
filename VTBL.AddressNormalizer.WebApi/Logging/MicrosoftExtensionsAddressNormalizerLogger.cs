using System;
using Microsoft.Extensions.Logging;
using CoreLogger = VTBL.AddressNormalizer.Abstractions.Logging.ILogger;

namespace VTBL.AddressNormalizer.WebApi.Logging
{
    /// <summary>
    /// Адаптер ядрового <see cref="CoreLogger"/> к Microsoft.Extensions.Logging (NLog через host).
    /// </summary>
    public sealed class MicrosoftExtensionsAddressNormalizerLogger : CoreLogger
    {
        /// <summary>Категория для правил nlog.config / appsettings Logging:LogLevel.</summary>
        public const string CategoryName = "VTBL.AddressNormalizer";

        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        /// <summary>
        /// Создаёт адаптер с фиксированной категорией <see cref="CategoryName"/>.
        /// </summary>
        /// <param name="loggerFactory">Фабрика логгеров ASP.NET Core host.</param>
        public MicrosoftExtensionsAddressNormalizerLogger(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));

            _logger = loggerFactory.CreateLogger(CategoryName);
        }

        /// <inheritdoc />
        public void Debug(string message) => _logger.LogDebug(message);

        /// <inheritdoc />
        public void Information(string message) => _logger.LogInformation(message);

        /// <inheritdoc />
        public void Warning(string message) => _logger.LogWarning(message);

        /// <inheritdoc />
        public void Error(string message) => _logger.LogError(message);

        /// <inheritdoc />
        public void Error(Exception exception, string message) => _logger.LogError(exception, message);
    }
}
