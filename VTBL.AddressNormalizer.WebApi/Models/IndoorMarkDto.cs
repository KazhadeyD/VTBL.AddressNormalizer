using System;
using System.Text.Json.Serialization;

namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Одна категория внутренней адресации.
    /// </summary>
    public class IndoorMarkDto
    {
        /// <summary>
        /// Стабильный идентификатор категории, удобный для обработки на клиенте.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Человекочитаемое название категории.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Значения, найденные для этой категории в исходной строке.
        /// </summary>
        [JsonPropertyName("values")]
        public string[] Values { get; set; } = Array.Empty<string>();

        /// <inheritdoc />
        public override string ToString()
        {
            var label = !string.IsNullOrEmpty(Name) ? Name : (Id ?? string.Empty);
            var values = Values == null || Values.Length == 0
                ? string.Empty
                : string.Join(",", Values);
            return label + "=" + values;
        }
    }
}