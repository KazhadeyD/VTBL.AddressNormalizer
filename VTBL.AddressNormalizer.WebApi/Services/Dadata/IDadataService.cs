using System.Threading;
using System.Threading.Tasks;
using VTBL.AddressNormalizer.WebApi.Models;

namespace VTBL.AddressNormalizer.WebApi.Services.Dadata
{
    /// <summary>
    /// Обёртка над HTTP API DaData для building-части адреса.
    /// </summary>
    public interface IDadataService
    {
        /// <summary>
        /// Вызов DaData suggest/address.
        /// </summary>
        /// <param name="address">Строка building-части адреса для подсказки.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        Task<DadataSuggestAddressDto> SuggestAddressAsync(string address, CancellationToken cancellationToken = default);

        /// <summary>
        /// Вызов DaData clean/address.
        /// </summary>
        /// <param name="address">Строка building-части адреса для очистки и нормализации.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        Task<DadataCleanAddressDto> CleanAddressAsync(string address, CancellationToken cancellationToken = default);
    }
}
