namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Одна категория indoor в массиве <c>marks</c>: стабильный id, русское имя и значения.
    /// </summary>
    public class IndoorMarkDto
    {
        /// <summary>
        /// Стабильный машинный идентификатор категории (camelCase, единственное число).
        /// </summary>
        /// <example>apartment</example>
        public string Id { get; set; }

        /// <summary>
        /// Русское отображаемое имя категории.
        /// </summary>
        /// <example>квартира</example>
        public string Name { get; set; }

        /// <summary>
        /// Значения категории.
        /// </summary>
        /// <example>["89"]</example>
        public string[] Values { get; set; } = System.Array.Empty<string>();
    }
}
