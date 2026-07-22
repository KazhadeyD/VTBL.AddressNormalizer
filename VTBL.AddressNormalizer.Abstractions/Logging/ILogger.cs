using System;

namespace VTBL.AddressNormalizer.Abstractions.Logging
{
    /// <summary>
    /// Логгер ядра AddressNormalizer. Реализация и sink задаются хостом (WebApi, Console).
    /// </summary>
    public interface ILogger
    {
        /// <summary>Диагностика для разработки и отладки парсера.</summary>
        void Debug(string message);

        /// <summary>Штатные события без детализации на каждый шаг.</summary>
        void Information(string message);

        /// <summary>Аномалии, не прерывающие обработку.</summary>
        void Warning(string message);

        /// <summary>Ошибка без исключения.</summary>
        void Error(string message);

        /// <summary>Ошибка с исключением.</summary>
        void Error(Exception exception, string message);
    }
}
