using System.Collections.Generic;
using VTBL.AddressNormalizer.WebApi.Models;

namespace VTBL.AddressNormalizer.WebApi.Services
{
    /// <summary>
    /// Классификация исхода batch-нормализации.
    /// </summary>
    public enum BatchOutcomeKind
    {
        /// <summary>Есть хотя бы один ok — ответ с items.</summary>
        PartialOrSuccess,

        /// <summary>Все элементы — ошибки валидации.</summary>
        AllFailValidation,

        /// <summary>Все элементы — исключения ядра.</summary>
        AllFailException,

        /// <summary>Все элементы error, смесь validation и exception.</summary>
        AllFailMixed,

        /// <summary>Некорректный запрос (пустой / null / сверх MaxItems).</summary>
        RequestInvalid
    }

    /// <summary>
    /// Исход NormalizeBatch до HTTP-маппинга.
    /// </summary>
    public sealed class BatchOutcome
    {
        /// <summary>
        /// Вид исхода.
        /// </summary>
        public BatchOutcomeKind Kind { get; set; }

        /// <summary>
        /// Per-item результаты (для PartialOrSuccess; для all-fail может быть заполнено для классификации).
        /// </summary>
        public IList<BatchItemResultDto> Items { get; set; }

        /// <summary>
        /// Общее сообщение ошибки (RequestInvalid / all-fail).
        /// </summary>
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// Результат NormalizeUnit на уровне сервиса.
    /// </summary>
    public sealed class UnitNormalizeResult
    {
        public string Source { get; set; }
        public IndoorValueDto IndoorValue { get; set; }
        public string Canonical { get; set; }
        public string Hash { get; set; }
    }
}
