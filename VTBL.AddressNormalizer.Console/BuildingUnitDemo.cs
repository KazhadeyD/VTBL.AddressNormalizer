using VTBL.AddressNormalizer.Abstractions.BuildingUnit;
using VTBL.AddressNormalizer.Infrastructure.Composition;

namespace VTBL.AddressNormalizer.Console
{
    /// <summary>
    /// Демо парсинга внутренних помещений (BuildingUnit).
    /// </summary>
    internal static class BuildingUnitDemo
    {
        private static readonly string[] Samples =
        {
            //"пом. 35-Н, Ч.П. 1 (№ 410).",
            //"пом. 35-Н, Ч.П. 1 (№ 410)",
            "этаж 4 кв. 31-Р,35-38, 40-42",
            "этаж 4 кв. 31-Р,[35,38], 40-42",
            "Ч.П. 1 ( 410",
            "Ч.П. 1 (№410)",
            "ком 5, этаж 3, помещ 2, кв 8",
            "этаж второй", // учесть что этажи могут быть строками/цифрами
            "этаж 2",
            "второй",
            "2",
            "ЭТАЖ ЦОКОЛЬНЫЙ, ВХОД С ТОРЦА ОФИС 1",
            "офис /ЭТАЖ 309/ТРЕТИЙ",
            "ТЕРРИТОРИЯ РЫНКА, Й КОМПЛЕКС",

        };

        /// <summary>
        /// Запускает демо BuildingUnit для набора sample-строк или одной произвольной.
        /// </summary>
        /// <param name="customInput">Произвольная строка new_flat; если задана — обрабатывается только она.</param>
        public static void Run(string customInput)
        {
            DemoConsoleWriter.WriteTitle("BuildingUnit — внутренние помещения");

            var normalizer = AddressNormalizerFactory.BuildingUnitNormalizer;
            var classifier = AddressNormalizerFactory.BuildingUnitClassifier;
            var inputs = string.IsNullOrWhiteSpace(customInput) ? Samples : new[] { customInput };

            for (var i = 0; i < inputs.Length; i++)
            {
                PrintResult(normalizer, classifier, inputs[i], i + 1, inputs.Length);
            }
        }

        private static void PrintResult(
            IBuildingUnitNormalizer normalizer,
            IBuildingUnitClassifier classifier,
            string input,
            int index,
            int total)
        {
            var result = normalizer.Normalize(input);
            var category = classifier.Classify(input);

            DemoConsoleWriter.WriteSampleHeader(index, total);
            DemoConsoleWriter.WriteField("IN", result.Original);
            DemoConsoleWriter.WriteField("CATEGORY", category.ToString());
            DemoConsoleWriter.WriteField("CANONICAL", result.Canonical);
            DemoConsoleWriter.WriteField("JSON", result.Json);
            DemoConsoleWriter.WriteHashPreview(result.Hash);
        }
    }
}
