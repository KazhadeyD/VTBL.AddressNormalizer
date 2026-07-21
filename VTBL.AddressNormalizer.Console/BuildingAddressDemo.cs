using VTBL.AddressNormalizer.Abstractions.BuildingAddress;

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
            "630048, Новосибирская область, г. Новосибирск , ул. Титова, д. 1 пом/эт 2/второй",
            "630048, Новосибирская область, г. Новосибирск, ул. Титова, д. 1 этаж 2",
            "630048, Новосибирская область, г. Новосибирск, ул. Титова, дом. 1 этаж 2",
            "630048, Новосибирская область, г. Новосибирск, ул. Титова, корп. 2 этаж 2",
            "630048, Новосибирская область, г. Новосибирск, ул. Титова, корп. 2 этаж 2",
            "300025, обл Тульская, г Тула, пр-кт Ленина, д. 102Б, офис /ЭТАЖ 309/ТРЕТИЙ",
            "300025, обл Тульская, г Тула, пр-кт Ленина, д. 102Б, этаж 4 кв. 31-Р,35-200",
            "300025, | обл Тульская, г Тула, пр-кт Ленина, д. 102Б | этаж 4 кв. 31-Р,35-200",
            //"РФ, 141580, Солнечногорский, д. Черная Грязь, квартира при д. 2 строительный рынок",
            //"РФ, 143370, обл Московская, Г НАРО-ФОМИНСК, КАЛИНИНЕЦ, РЫНОК ДУБКИ 2, ПОМЕЩ. 29 ОФИС 22А",
            "обл Московская, Г НАРО-ФОМИНСК, КАЛИНИНЕЦ, РЫНОК ДУБКИ 2, ПОМЕЩ. 29 ОФИС 22А",
            "456604, Челябинская обл., г. Копейск, ул. 4 Пятилетки,64 (рынок «Народный двор»)",
            //300025, обл Тульская, г Тула, пр-кт Ленина, д. 102Б, офис 309 эт 3
            //"РФ, 300025, обл Тульская, г Тула, пр-кт Ленина, д. 102Б, офис /ЭТАЖ 309/ТРЕТИЙ",
        };

        /// <summary>
        /// Запускает демо BuildingAddress для набора sample-строк или одной произвольной.
        /// </summary>
        /// <param name="customInput">Произвольный адрес; если задан — обрабатывается только он.</param>
        public static void Run(string customInput)
        {
            DemoConsoleWriter.WriteTitle("BuildingAddress — адрес строения");

            var normalizer = DemoServices.BuildingAddressNormalizer;
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
