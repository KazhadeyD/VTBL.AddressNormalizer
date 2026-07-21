using VTBL.AddressNormalizer.Infrastructure.Shared;

namespace VTBL.AddressNormalizer.Infrastructure.BuildingAddress
{
    /// <summary>
    /// Поиск самого левого indoor-маркера по <see cref="IndoorMarkerPatterns"/>.
    /// </summary>
    internal static class IndoorMarkerRegistry
    {
        /// <summary>
        /// Находит самое левое совпадение indoor-маркера.
        /// </summary>
        /// <param name="text">Препроцессированная строка.</param>
        /// <returns>Match или <c>null</c>.</returns>
        internal static IndoorMarkerMatch FindFirstMatch(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            IndoorMarkerMatch best = null;

            foreach (var definition in IndoorMarkerPatterns.All)
            {
                var match = definition.Pattern.Match(text);
                if (!match.Success)
                    continue;

                if (best == null || match.Index < best.Index)
                {
                    best = new IndoorMarkerMatch(match.Index, match.Length, definition.Kind);
                }
            }

            return best;
        }
    }
}
