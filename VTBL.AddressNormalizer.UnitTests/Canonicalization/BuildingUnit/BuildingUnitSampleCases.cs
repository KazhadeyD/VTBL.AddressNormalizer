using System.Collections.Generic;

namespace VTBL.AddressNormalizer.UnitTests.Canonicalization.BuildingUnit
{
    internal static class BuildingUnitSampleCases
    {
        public static IEnumerable<object[]> NormalizeCases => new List<object[]>
        {
            new object[] { "ЭТАЖ 4 ПОМЕЩЕНИЕ 2", "эт:4|пом:2" },
            new object[] { "ЭТАЖ 1-Й", "эт:1" },
            new object[] { "ОФИС №18С", "оф:18с" },
            new object[] { "ЭТ 3 ПОМ IA КОМ 35", "эт:3|пом:ia|ком:35" },
            new object[] { "К. 5-20", "ком:5-20" },
            new object[] { "27-Н (ч.п. 666), офис 613-11", "оф:613-11|ч.п:666|code:27-н" },
            new object[] { "ЭТ./ПОМЕЩ. 0/II КОМ./ОФИС 1/24", "эт:0|пом:ii|ком:1|оф:24" },
            new object[] { "6-Н, ОФИС 104, РАБ.М.1", "оф:104|раб.м:1|code:6-н" },
            new object[] { "305,307", "code:305|code:307" },
            new object[] { "14, помещ. 15", "пом:15|code:14" },
            new object[] { "II, комн.9; 12", "ком:9|code:12|code:ii" },
            new object[] { "659318", "code:659318" },
            new object[] { "ПОМЕЩ. №410", "пом:410" },
            new object[] { "ПОМЕЩ. 5/1А", "пом:5/1а" },
            new object[] { "ПОМЕЩЕНИЕ 5-5", "пом:5-5" },
            new object[] { "60, помещение № 1", "пом:1|code:60" },
            new object[] { "41X1Д, ОФ 79", "оф:79|code:41x1д" },
            new object[] { "61,62", "code:61|code:62" },
            new object[] { "КОМНАТА 136", "ком:136" },
            new object[] { "ПОМЕЩ. I, ОФИС 307", "пом:i|оф:307" },
            new object[] { "ЭТАЖ 3 ПОМ 183", "эт:3|пом:183" },
            new object[] { "ЭТ 1 ПОМ XIБ КОМ 1;2", "эт:1|пом:xiб|ком:1|ком:2" },
            new object[] { "ПОМЕЩ. 9А/1", "пом:9а/1" },
            new object[] { "ЭТАЖ 2 ПОДВАЛЬНЫЙ, ПОМ. 173", "эт:2|эт:подвальный|пом:173" },
            new object[] { "ЭТАЖ ЦОКОЛЬНЫЙ, ВХОД С ТОРЦА ОФИС 1", "эт:цокольный|оф:1|note:вход с торца" },
        };
    }
}
