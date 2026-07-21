using VTBL.AddressNormalizer.Abstractions.BuildingAddress;
using VTBL.AddressNormalizer.Infrastructure.Composition;

namespace VTBL.AddressNormalizer.Console
{
    /// <summary>
    /// Демо нормализации полного адреса до уровня строения (BuildingAddress).
    /// </summary>
    internal static class BuildingAddressDemo
    {
        private static readonly string[] Samples =
        {
            "600001, Владимирская область, г. Владимир, ул. Студеная Гора, д.44 А, офис 318",
            "430016, Республика Мордовия, г. Саранск, ул. Полежаева, д. 103",
            "354000, Краснодарский край, г. Сочи, ул. Кубанская (Центральный р-н), д. 15, оф. 303",
            "423815, Республика Татарстан, г. Набережные Челны, проспект Московский, территория рынка \"Автозаводский\" (53-й Комплекс)",
            "603087, Нижегородская область, г. Нижний Новгород, ул. Верхне-Печерская, д. 14/1",
            "115487, г. Москва, ул. Нагатинская, д. 16, помещ. 11/10/1",
            "129128, г. Москва, плат. Северянин, д. 1, стр. 1, эт. 2, пом. 18",
            "191317, г. Санкт-Петербург, пл. Александра Невского, д. 2, лит. Е,  пом. 35-Н, Ч.П. 1 (№ 410).",
            "630048, Новосибирская область, г. Новосибирск, ул. Титова, д. 1 этаж второй",
            "630048, Новосибирская область, г. Новосибирск, ул. Титова, д. 1 этаж 2",
        };

        /// <summary>
        /// Запускает демо BuildingAddress для набора sample-строк или одной произвольной.
        /// </summary>
        /// <param name="customInput">Произвольный адрес; если задан — обрабатывается только он.</param>
        public static void Run(string customInput)
        {
            DemoConsoleWriter.WriteTitle("BuildingAddress — адрес строения");

            var normalizer = AddressNormalizerFactory.BuildingAddressNormalizer;
            var inputs = string.IsNullOrWhiteSpace(customInput) ? Samples : new[] { customInput };

            for (var i = 0; i < inputs.Length; i++)
            {
                PrintResult(normalizer, inputs[i], i + 1, inputs.Length);
            }
        }

        private static void PrintResult(
            IBuildingAddressNormalizer normalizer,
            string input,
            int index,
            int total)
        {
            var result = normalizer.Normalize(input);

            DemoConsoleWriter.WriteSampleHeader(index, total);
            DemoConsoleWriter.WriteField("IN", result.Original);
            DemoConsoleWriter.WriteField("EXTRACTED", result.Extracted);
            DemoConsoleWriter.WriteField("CANONICAL", result.Canonical);
        }
    }
}
