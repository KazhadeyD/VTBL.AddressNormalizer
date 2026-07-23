using VTBL.AddressNormalizer.Abstractions.Shared;

namespace VTBL.AddressNormalizer.Infrastructure.BuildingAddress
{
    /// <summary>
    /// Совпадение indoor-маркера в строке.
    /// </summary>
    internal sealed class IndoorMarkerMatch
    {
        /// <summary>
        /// Создаёт описание совпадения.
        /// </summary>
        public IndoorMarkerMatch(int index, int length, IndoorMarkerKind kind)
        {
            Index = index;
            Length = length;
            Kind = kind;
        }

        /// <summary>
        /// Позиция начала маркера.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Длина совпадения.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Категория маркера.
        /// </summary>
        public IndoorMarkerKind Kind { get; }
    }
}
