using System;
using System.Security.Cryptography;
using System.Text;
using VTBL.AddressNormalizer.Abstractions.Shared;

namespace VTBL.AddressNormalizer.Infrastructure.Shared
{
    /// <summary>
    /// Вычисление хеша канонической строки.
    /// </summary>
    public sealed class CanonicalHash : ICanonicalHash
    {
        /// <inheritdoc />
        public string ComputeSha256(string input)
        {
            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input ?? string.Empty));
                return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
            }
        }
    }
}
