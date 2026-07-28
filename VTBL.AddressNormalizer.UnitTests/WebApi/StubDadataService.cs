using System.Threading;
using System.Threading.Tasks;
using VTBL.AddressNormalizer.WebApi.Models;
using VTBL.AddressNormalizer.WebApi.Services.Dadata;

namespace VTBL.AddressNormalizer.UnitTests.WebApi
{
    /// <summary>
    /// Заглушка DaData для unit/E2E-тестов WebApi.
    /// </summary>
    internal sealed class StubDadataService : IDadataService
    {
        public Task<DadataSuggestAddressDto> SuggestAddressAsync(string address, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new DadataSuggestAddressDto
            {
                Suggestions = new[]
                {
                    new DadataSuggestAddressSuggestionDto
                    {
                        Value = address,
                        UnrestrictedValue = address,
                        Data = new DadataAddressDataDto
                        {
                            Source = address,
                            Result = address,
                            Country = "Россия",
                            CountryIsoCode = "RU",
                            HouseFiasId = "suggest-house-fias-id"
                        }
                    }
                }
            });
        }

        public Task<DadataCleanAddressDto> CleanAddressAsync(string address, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new DadataCleanAddressDto
            {
                Source = address,
                Result = address,
                Country = "Россия",
                CountryIsoCode = "RU",
                HouseFiasId = "clean-house-fias-id"
            });
        }
    }
}
