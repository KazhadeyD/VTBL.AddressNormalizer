using System.Text.Json.Serialization;

namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Результат обработки одного адреса в batch-запросе.
    /// </summary>
    public class BatchItemResultDto
    {
        /// <summary>
        /// Статус обработки элемента: успешно или с ошибкой.
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; }

        /// <summary>
        /// Исходная строка адреса для этого элемента.
        /// </summary>
        [JsonPropertyName("source")]
        public string Source { get; set; }

        /// <summary>
        /// Результат нормализации. Заполняется только для успешно обработанных элементов.
        /// </summary>
        [JsonPropertyName("value")]
        public NormalizeValueDto Value { get; set; }

        /// <summary>
        /// Текст ошибки. Заполняется только для элементов, которые не удалось обработать.
        /// </summary>
        [JsonPropertyName("error")]
        public string Error { get; set; }
    }
}