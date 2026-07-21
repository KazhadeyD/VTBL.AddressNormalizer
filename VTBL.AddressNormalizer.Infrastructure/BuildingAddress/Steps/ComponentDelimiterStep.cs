using System;
using System.Linq;
using System.Text.RegularExpressions;
using VTBL.AddressNormalizer.Abstractions.Shared;

namespace VTBL.AddressNormalizer.Infrastructure.BuildingAddress.Steps
{
    /// <summary>
    /// Нормализация разделителей компонентов: <c>", "</c> если в исходнике были явные разделители.
    /// </summary>
    internal sealed class ComponentDelimiterStep : ITextNormalizer
    {
        private static readonly Regex MultiCommaSpace = new Regex(
            @"\s*,\s*",
            RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private readonly bool _hadExplicitDelimiters;

        /// <summary>
        /// Создаёт шаг с флагом явных разделителей из preprocess.
        /// </summary>
        internal ComponentDelimiterStep(bool hadExplicitDelimiters)
        {
            _hadExplicitDelimiters = hadExplicitDelimiters;
        }

        /// <inheritdoc />
        public string Normalize(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            if (!_hadExplicitDelimiters)
                return input;

            var segments = MultiCommaSpace.Split(input)
                .Select(s => s.Trim())
                .Where(s => s.Length > 0);

            return string.Join(", ", segments);
        }
    }
}
