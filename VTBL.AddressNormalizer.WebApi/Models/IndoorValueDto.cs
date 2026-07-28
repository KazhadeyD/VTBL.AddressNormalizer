using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Результат нормализации indoor-части адреса.
    /// </summary>
    public class IndoorValueDto
    {
        /// <summary>
        /// Фрагмент исходной строки, распознанный как indoor-часть адреса.
        /// </summary>
        [JsonPropertyName("extracted")]
        public string Extracted { get; set; }

        /// <summary>
        /// Hash канонической indoor-строки. Подходит для сравнения и дедупликации.
        /// </summary>
        [JsonPropertyName("hash")]
        public string Hash { get; set; }

        /// <summary>
        /// Категории внутренней адресации, в которых удалось распознать значения:
        /// например этаж, квартира, офис или помещение.
        /// </summary>
        [JsonPropertyName("units")]
        public IList<IndoorMarkDto> Units { get; set; } = new List<IndoorMarkDto>();
    }
}