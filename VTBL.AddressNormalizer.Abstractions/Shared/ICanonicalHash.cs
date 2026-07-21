namespace VTBL.AddressNormalizer.Abstractions.Shared
{
    /// <summary>
    /// Вычисление хеша канонической строки.
    /// </summary>
    public interface ICanonicalHash
    {
        /// <summary>
        /// Вычисляет SHA256-хеш канонической строки в hex (lowercase, без дефисов).
        /// </summary>
        /// <param name="input">Каноническая строка после нормализации.</param>
        /// <returns>Hex-представление SHA256.</returns>
        string ComputeSha256(string input);
    }
}
