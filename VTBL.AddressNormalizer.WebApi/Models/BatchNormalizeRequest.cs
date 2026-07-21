using System.Collections.Generic;

namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Запрос batch-нормализации (<c>POST /api/v1/normalize/batch</c>).
    /// </summary>
    public class BatchNormalizeRequest
    {
        /// <summary>
        /// Элементы batch; каждый содержит свой <c>source</c>. Не пустой; размер ≤ <c>Batch:MaxItems</c>.
        /// </summary>
        public IList<SourceRequest> Items { get; set; }
    }
}
