using System.Text.RegularExpressions;
using VTBL.AddressNormalizer.Abstractions.Shared;

namespace VTBL.AddressNormalizer.Infrastructure.BuildingAddress.Steps
{
    /// <summary>
    /// Удаляет точки после сокращений типов; дефисы в топонимах сохраняет.
    /// </summary>
    internal sealed class AbbreviationPunctuationStep : ITextNormalizer
    {
        private static readonly Regex TrailingDotAfterType = new Regex(
            @"(?<!\p{L})(г|ул|д|корп|стр|лит|пр-кт|пер|б-р|ш|обл|р-н|п|с)\.(?!\p{L})",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <inheritdoc />
        public string Normalize(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return TrailingDotAfterType.Replace(input, "$1");
        }
    }
}
