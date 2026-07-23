using System;

namespace VTBL.AddressNormalizer.Abstractions.Logging
{
    /// <summary>
    /// No-op логгер для тестов и in-process хостов без sink.
    /// </summary>
    public sealed class NullLogger : ILogger
    {
        /// <summary>
        /// Единственный экземпляр.
        /// </summary>
        public static NullLogger Instance { get; } = new NullLogger();

        private NullLogger()
        {
        }

        /// <inheritdoc />
        public void Debug(string message)
        {
        }

        /// <inheritdoc />
        public void Information(string message)
        {
        }

        /// <inheritdoc />
        public void Warning(string message)
        {
        }

        /// <inheritdoc />
        public void Error(string message)
        {
        }

        /// <inheritdoc />
        public void Error(Exception exception, string message)
        {
        }
    }
}
