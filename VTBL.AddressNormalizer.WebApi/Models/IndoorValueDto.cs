using System.Collections.Generic;
using System.Linq;
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

        /// <inheritdoc />
        public override string ToString()
        {
            var hashPreview = string.IsNullOrEmpty(Hash)
                ? "«»"
                : Hash.Length <= 8
                    ? "«" + Hash + "»"
                    : "«" + Hash.Substring(0, 8) + "…»";

            var units = Units == null || Units.Count == 0
                ? "[]"
                : "[" + string.Join(", ", Units.Select(u => u?.ToString() ?? string.Empty)) + "]";

            return "extracted: «" + (Extracted ?? string.Empty) + "»; hash: " + hashPreview + "; units: " + units;
        }
    }
}
