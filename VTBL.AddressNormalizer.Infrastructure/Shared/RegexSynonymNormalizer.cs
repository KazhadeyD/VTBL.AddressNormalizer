using VTBL.AddressNormalizer.Abstractions.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace VTBL.AddressNormalizer.Infrastructure.Shared
{
    /// <summary>
    /// Заменяет целые токены по списку regex-правил (с границами слова и опциональной точкой).
    /// </summary>
    public sealed class RegexSynonymNormalizer : ITextNormalizer
    {
        private sealed class CompiledRule
        {
            public Regex Regex { get; set; }

            public string Replacement { get; set; }
        }

        private readonly CompiledRule[] _rules;

        /// <summary>
        /// Создаёт нормализатор с набором правил замены синонимов.
        /// </summary>
        /// <param name="rules">Правила; применяются по порядку.</param>
        public RegexSynonymNormalizer(IEnumerable<SynonymRule> rules)
        {
            _rules = rules
                .Select(rule => new CompiledRule
                {
                    Regex = new Regex(
                        $@"(?<!\p{{L}})(?:{rule.Pattern})\.?(?!\p{{L}})",
                        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled),
                    Replacement = rule.Replacement,
                })
                .ToArray();
        }

        /// <inheritdoc />
        public string Normalize(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var result = input;
            foreach (var rule in _rules)
                result = rule.Regex.Replace(result, rule.Replacement);

            return result;
        }
    }
}
