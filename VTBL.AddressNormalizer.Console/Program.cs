using System;
using VTBL.AddressNormalizer.Abstractions.FieldAdapters.Crm;
using VTBL.AddressNormalizer.Infrastructure.Composition;

namespace VTBL.AddressNormalizer.Console
{
    /// <summary>
    /// Точка входа консольного приложения: демо нормализации CRM-поля <c>new_flat</c>.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Запускает демо-нормализацию sample-строк через <see cref="ICrmNewFlatNormalizer"/>.
        /// </summary>
        /// <param name="args">Аргументы командной строки (не используются).</param>
        static void Main(string[] args)
        {
            var normalizer = AddressNormalizerFactory.CrmNewFlatNormalizer;

            var samples = new[]
            {
                "ЭТАЖ/ПОМЕЩ. АНТРЕСОЛЬ 2/I КОМ./ОФИС 17/Е9Е",
            };

            foreach (var sample in samples)
            {
                var result = normalizer.Normalize(sample);
                System.Console.WriteLine("---");
                System.Console.WriteLine($"IN:  {result.Original}");
                System.Console.WriteLine($"CAN: {result.Canonical}");
                System.Console.WriteLine($"JSON:{result.Json}");
                var hashPrefix = result.Hash != null && result.Hash.Length > 16
                    ? result.Hash.Substring(0, 16)
                    : result.Hash;
                System.Console.WriteLine($"HASH:{hashPrefix}...");
            }
        }
    }
}
