using System;
using CoreLogger = VTBL.AddressNormalizer.Abstractions.Logging.ILogger;

namespace VTBL.AddressNormalizer.Console.Logging
{
    /// <summary>
    /// Простой sink в stdout для CLI-демо.
    /// </summary>
    public sealed class ConsoleAddressNormalizerLogger : CoreLogger
    {
        /// <inheritdoc />
        public void Debug(string message) => Write("DBG", message);

        /// <inheritdoc />
        public void Information(string message) => Write("INF", message);

        /// <inheritdoc />
        public void Warning(string message) => Write("WRN", message);

        /// <inheritdoc />
        public void Error(string message) => Write("ERR", message);

        /// <inheritdoc />
        public void Error(Exception exception, string message) =>
            Write("ERR", message + Environment.NewLine + exception);

        private static void Write(string level, string message)
        {
            System.Console.WriteLine($"[{level}] AddressNormalizer: {message}");
        }
    }
}
