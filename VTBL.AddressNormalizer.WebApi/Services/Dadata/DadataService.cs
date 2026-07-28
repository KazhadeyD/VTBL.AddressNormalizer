using System.Threading;
using System.Threading.Tasks;
using VTBL.AddressNormalizer.WebApi.Models;

namespace VTBL.AddressNormalizer.WebApi.Services.Dadata
{
    /// <summary>
    /// Реализация обёртки DaData. HTTP-вызовы заполняются отдельно.
    /// </summary>
    public sealed class DadataService : IDadataService
    {
        /// <inheritdoc />
        public Task<DadataSuggestAddressDto> SuggestAddressAsync(string address, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<DadataSuggestAddressDto>(null);
        }

        /// <inheritdoc />
        public Task<DadataCleanAddressDto> CleanAddressAsync(string address, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<DadataCleanAddressDto>(null);
        }
    }
}
