using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using VTBL.AddressNormalizer.Abstractions.Shared;

namespace VTBL.AddressNormalizer.Infrastructure.BuildingAddress.Steps
{
    /// <summary>
    /// Title case для топонимов; geographic types остаются в нижнем регистре.
    /// </summary>
    internal sealed class TitleCaseStep : ITextNormalizer
    {
        private static readonly HashSet<string> TypeTokens = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "г", "ул", "д", "корп", "стр", "лит", "пр-кт", "пер", "б-р", "ш", "обл", "р-н", "п", "с",
        };

        /// <inheritdoc />
        public string Normalize(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var parts = new List<string>();
            var current = new StringBuilder();

            void FlushToken()
            {
                if (current.Length == 0)
                    return;
                parts.Add(FormatToken(current.ToString()));
                current.Clear();
            }

            foreach (var ch in input)
            {
                if (ch == ',' || char.IsWhiteSpace(ch))
                {
                    FlushToken();
                    parts.Add(ch.ToString());
                }
                else
                {
                    current.Append(ch);
                }
            }

            FlushToken();
            return string.Concat(parts);
        }

        private static string FormatToken(string token)
        {
            if (TypeTokens.Contains(token))
                return token.ToLowerInvariant();

            if (token.Contains('-'))
            {
                return string.Join("-", token.Split('-').Select(TitleCaseWord));
            }

            return TitleCaseWord(token);
        }

        private static string TitleCaseWord(string word)
        {
            if (string.IsNullOrEmpty(word))
                return word;

            var lower = word.ToLowerInvariant();
            if (lower.Length == 1)
                return char.ToUpper(lower[0], CultureInfo.InvariantCulture).ToString();

            return char.ToUpper(lower[0], CultureInfo.InvariantCulture) + lower.Substring(1);
        }
    }
}
