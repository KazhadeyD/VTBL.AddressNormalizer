using System.Collections.Generic;

namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Ответ batch при частичном/полном успехе (HTTP 200).
    /// </summary>
    public class BatchNormalizeResponse
    {
        /// <summary>
        /// Per-item результаты в порядке входа (<c>ok</c> / <c>error</c>).
        /// </summary>
        public IList<BatchItemResultDto> Items { get; set; }
    }
}
