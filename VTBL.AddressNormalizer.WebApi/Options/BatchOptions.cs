namespace VTBL.AddressNormalizer.WebApi.Options
{
    /// <summary>
    /// Параметры batch-обработки. Секция конфигурации: <c>Batch</c>.
    /// </summary>
    public class BatchOptions
    {
        /// <summary>
        /// Максимальное число элементов в одном batch-запросе.
        /// </summary>
        public int MaxItems { get; set; } = 100;
    }
}
