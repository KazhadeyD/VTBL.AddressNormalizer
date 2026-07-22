using System;
using System.Collections.Generic;
using VTBL.AddressNormalizer.Abstractions.Logging;

namespace VTBL.AddressNormalizer.UnitTests.Logging
{
    /// <summary>
    /// Логгер для тестов: собирает сообщения по уровням.
    /// </summary>
    internal sealed class CapturingLogger : ILogger
    {
        public List<string> DebugMessages { get; } = new List<string>();
        public List<string> InformationMessages { get; } = new List<string>();
        public List<string> WarningMessages { get; } = new List<string>();
        public List<string> ErrorMessages { get; } = new List<string>();

        public void Debug(string message) => DebugMessages.Add(message);

        public void Information(string message) => InformationMessages.Add(message);

        public void Warning(string message) => WarningMessages.Add(message);

        public void Error(string message) => ErrorMessages.Add(message);

        public void Error(Exception exception, string message) =>
            ErrorMessages.Add(message + " | " + exception?.GetType().Name);
    }
}
