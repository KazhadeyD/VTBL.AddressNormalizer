namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Результат одного элемента batch (только в HTTP 200).
    /// </summary>
    public class BatchItemResultDto
    {
        /// <summary>
        /// Статус элемента: <c>ok</c> или <c>error</c>.
        /// </summary>
        /// <example>ok</example>
        public string Status { get; set; }

        /// <summary>
        /// Исходная строка элемента (<c>null</c> во входе сериализуется как пустая строка).
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Value полной нормализации; заполняется только при <c>status=ok</c>.
        /// </summary>
        public NormalizeValueDto Value { get; set; }

        /// <summary>
        /// Текст ошибки; заполняется только при <c>status=error</c>.
        /// </summary>
        public string Error { get; set; }
    }
}
