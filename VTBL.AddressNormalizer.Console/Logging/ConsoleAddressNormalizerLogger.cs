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
        public void Debug(string message) => Write("ДОП", message);

        /// <inheritdoc />
        public void Information(string message) => Write("ИНФ", message);

        /// <inheritdoc />
        public void Warning(string message) => Write("ПРД", message);

        /// <inheritdoc />
        public void Error(string message) => Write("ОШБ", message);

        /// <inheritdoc />
        public void Error(Exception exception, string message) =>
            Write("ОШБ", message + Environment.NewLine + exception);

        private static void Write(string level, string message)
        {
            System.Console.WriteLine($"[{level}] AddressNormalizer: {message}");
        }
    }
}
