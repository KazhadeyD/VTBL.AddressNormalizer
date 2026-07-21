using VTBL.AddressNormalizer.Abstractions.Shared;

namespace VTBL.AddressNormalizer.Infrastructure.BuildingAddress.Steps
{
    /// <summary>
    /// Обёртка над <see cref="AddressPreprocessor"/> (для тестов/reuse; не в production pipeline).
    /// </summary>
    internal sealed class AddressPreprocessStep : ITextNormalizer
    {
        /// <inheritdoc />
        public string Normalize(string input) => AddressPreprocessor.Preprocess(input).Text;
    }
}
