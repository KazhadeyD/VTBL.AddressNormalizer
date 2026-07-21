using VTBL.AddressNormalizer.Abstractions.Shared;
using System.Collections.Generic;
using System.Linq;

namespace VTBL.AddressNormalizer.Infrastructure.Shared
{
    /// <summary>
    /// Последовательное применение шагов нормализации.
    /// </summary>
    public sealed class CanonicalizationPipeline : ITextNormalizer
    {
        private readonly IReadOnlyList<ITextNormalizer> _steps;

        /// <summary>
        /// Создаёт пайплайн из переданных шагов.
        /// </summary>
        /// <param name="steps">Шаги нормализации в порядке применения.</param>
        public CanonicalizationPipeline(IEnumerable<ITextNormalizer> steps)
        {
            _steps = steps.ToList();
        }

        /// <inheritdoc />
        public string Normalize(string input)
        {
            var result = input ?? string.Empty;
            foreach (var step in _steps)
                result = step.Normalize(result);

            return result;
        }
    }
}
