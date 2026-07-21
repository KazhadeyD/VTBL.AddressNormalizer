using System.Collections.Generic;
using VTBL.AddressNormalizer.WebApi.Models;

namespace VTBL.AddressNormalizer.WebApi.Services
{
    /// <summary>
    /// Оркестрация бизнес-endpoint’ов нормализации адреса.
    /// Контроллеры вызывают только этот интерфейс (без прямого доступа к Factory).
    /// </summary>
    public interface IAddressNormalizationService
    {
        /// <summary>
        /// Полная нормализация адреса.
        /// </summary>
        /// <exception cref="System.ArgumentException">source null/whitespace.</exception>
        NormalizeValueDto NormalizeFull(string source);

        /// <summary>
        /// Нормализация indoor/unit.
        /// </summary>
        /// <exception cref="System.ArgumentException">source null/whitespace.</exception>
        UnitNormalizeResult NormalizeUnit(string source);

        /// <summary>
        /// Extract outdoor-части.
        /// </summary>
        /// <exception cref="System.ArgumentException">source null/whitespace.</exception>
        string ExtractOutdoor(string source);

        /// <summary>
        /// Канонизация building location (без предварительного extract).
        /// </summary>
        /// <exception cref="System.ArgumentException">source null/whitespace.</exception>
        string Canonicalize(string source);

        /// <summary>
        /// Batch полной нормализации.
        /// </summary>
        BatchOutcome NormalizeBatch(IReadOnlyList<string> sources, int maxItems);
    }
}
