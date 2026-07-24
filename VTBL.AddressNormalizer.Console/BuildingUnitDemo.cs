using VTBL.AddressNormalizer.Abstractions.BuildingUnit;
using VTBL.AddressNormalizer.Abstractions.Shared;

namespace VTBL.AddressNormalizer.Console
{
    /// <summary>
    /// Демо парсинга внутренних помещений (BuildingUnit).
    /// </summary>
    internal static class BuildingUnitDemo
    {
        private static readonly string[] Samples =
        {
            "этаж 4 кв. 31-Р,35-38, 40-42",
            "этаж 4 кв. 31-Р,[35,38], 40-42",
            "Ч.П. 1 ( 410",
            "Ч.П. 1 (№410)",
            "ком 5, этаж 3, помещ 2, кв 8",
            "этаж второй",
            "этаж 2",
            "второй",
            "2",
            "ЭТАЖ ЦОКОЛЬНЫЙ, ВХОД С ТОРЦА ОФИС 1",
            "ЭТАЖ ЦОКОЛЬНЫЙ, ВХОД С Парадно ОФИС 1",
            "офис /ЭТАЖ 309/ТРЕТИЙ",
            "ТЕРРИТОРИЯ РЫНКА, Й КОМПЛЕКС",
        };

        /// <summary>
        /// Запускает демо BuildingUnit для набора sample-строк или одной произвольной.
        /// </summary>
        /// <param name="customInput">Произвольная unit-строка; если задана — обрабатывается только она.</param>
        public static void Run(string customInput)
        {
            DemoConsoleWriter.WriteTitle("BuildingUnit — внутренние помещения");

            var parser = DemoServices.BuildingUnitParser;
            var canonicalizer = DemoServices.BuildingUnitCanonicalizer;
            var hash = DemoServices.CanonicalHash;
            var classifier = DemoServices.BuildingUnitClassifier;
            var inputs = string.IsNullOrWhiteSpace(customInput) ? Samples : new[] { customInput };

            for (var i = 0; i < inputs.Length; i++)
            {
                PrintResult(parser, canonicalizer, hash, classifier, inputs[i], i + 1, inputs.Length);
            }
        }

        private static void PrintResult(
            IBuildingUnitParser parser,
            IBuildingUnitCanonicalizer canonicalizer,
            ICanonicalHash hash,
            IBuildingUnitClassifier classifier,
            string input,
            int index,
            int total)
        {
            var location = parser.Parse(input);
            var canonical = canonicalizer.ToCanonical(location);
            var digest = hash.ComputeSha256(canonical);
            var category = classifier.Classify(input);

            DemoConsoleWriter.WriteSampleHeader(index, total);
            DemoConsoleWriter.WriteField("IN", input);
            DemoConsoleWriter.WriteField("CATEGORY", category.ToString());
            DemoConsoleWriter.WriteField("CANONICAL", canonical);
            DemoConsoleWriter.WriteHashPreview(digest);
        }
    }
}
