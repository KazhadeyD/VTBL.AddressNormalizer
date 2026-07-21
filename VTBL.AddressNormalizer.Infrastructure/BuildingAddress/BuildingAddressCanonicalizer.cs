using VTBL.AddressNormalizer.Abstractions.BuildingAddress;
using VTBL.AddressNormalizer.Abstractions.Shared;
using VTBL.AddressNormalizer.Infrastructure.BuildingAddress.Steps;
using VTBL.AddressNormalizer.Infrastructure.Shared;

namespace VTBL.AddressNormalizer.Infrastructure.BuildingAddress
{
    /// <summary>
    /// Читаемая канонизация локации здания.
    /// </summary>
    public sealed class BuildingAddressCanonicalizer : IBuildingAddressCanonicalizer
    {
        /// <inheritdoc />
        public string ToCanonical(string input)
        {
            var preprocessed = AddressPreprocessor.Preprocess(input);
            if (string.IsNullOrEmpty(preprocessed.Text))
                return string.Empty;

            var pipeline = new CanonicalizationPipeline(new ITextNormalizer[]
            {
                new GeographicTypeNormalizer(),
                new AbbreviationPunctuationStep(),
                new TitleCaseStep(),
                new ComponentDelimiterStep(preprocessed.HadExplicitDelimiters),
            });

            return pipeline.Normalize(preprocessed.Text);
        }
    }
}
