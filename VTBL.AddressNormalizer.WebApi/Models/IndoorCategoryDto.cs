namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Одна категория indoor: русское отображаемое имя и список значений.
    /// </summary>
    public class IndoorCategoryDto
    {
        /// <summary>
        /// Русское имя категории (например, «квартира», «этаж»).
        /// </summary>
        /// <example>квартира</example>
        public string Name { get; set; }

        /// <summary>
        /// Значения категории; пустой массив, если данных нет.
        /// </summary>
        /// <example>["89"]</example>
        public string[] Values { get; set; } = System.Array.Empty<string>();
    }
}
