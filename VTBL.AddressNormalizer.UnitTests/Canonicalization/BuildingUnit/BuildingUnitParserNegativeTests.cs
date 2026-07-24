using System.Collections.Generic;
using VTBL.AddressNormalizer.Abstractions.BuildingUnit;
using Xunit;

namespace VTBL.AddressNormalizer.UnitTests.Canonicalization.BuildingUnit
{
    /// <summary>
    /// Слой D: соседство маркеров и bare/optional на изолированной unit-строке.
    /// UC-02 / architecture §2.1 N-01…N-11; bare §3.5 H02/ST02/B04/M02.
    /// </summary>
    public class BuildingUnitParserNegativeTests
    {
        /// <summary>
        /// NeighborCases: (matrixId, input, expectedCanonical, emptyNeighborCollections).
        /// N-09 (slash unknown → RawCodes) не дублируется: прецедент
        /// <c>ЭТАЖ/ОФИС 3/314/5/WP</c> — см. <see cref="BuildingUnitParserSlashChainTests.Parse_FloorOfficeChain_ExtraValuesGoToRawCodes"/>
        /// (лишние values в RawCodes); не повторять здесь.
        /// </summary>
        public static IEnumerable<object[]> NeighborCases()
        {
            // N-01: ОФ vs ОФИС → Offices; сосед Rooms пуст
            yield return new object[]
            {
                "N-01",
                "ОФ 79",
                "оф:79",
                new[] { nameof(BuildingUnitLocation.Rooms) },
            };
            yield return new object[]
            {
                "N-01b",
                "ОФИС 104",
                "оф:104",
                new[] { nameof(BuildingUnitLocation.Rooms) },
            };

            // N-02: КОМ / КОМНАТА / К. → Rooms; ShortRoom «К. 5-20» без expand
            // (якорь канона также в SampleCases; здесь — neighbor Apartments)
            yield return new object[]
            {
                "N-02",
                "КОМ 10",
                "ком:10",
                new[] { nameof(BuildingUnitLocation.Apartments) },
            };
            yield return new object[]
            {
                "N-02b",
                "КОМНАТА 136",
                "ком:136",
                new[] { nameof(BuildingUnitLocation.Apartments) },
            };
            yield return new object[]
            {
                "N-02c",
                "К. 5-20",
                "ком:5-20",
                new[] { nameof(BuildingUnitLocation.Apartments) },
            };

            // N-03: КВ vs К. — Apartments vs Rooms; соседняя коллекция пуста
            yield return new object[]
            {
                "N-03",
                "КВ 89",
                "кв:89",
                new[] { nameof(BuildingUnitLocation.Rooms) },
            };
            yield return new object[]
            {
                "N-03b",
                "К. 7",
                "ком:7",
                new[] { nameof(BuildingUnitLocation.Apartments) },
            };

            // N-04: ПОМ vs ПОМЕЩЕНИЕ → Premises
            yield return new object[]
            {
                "N-04",
                "ПОМ 183",
                "пом:183",
                new[] { nameof(BuildingUnitLocation.Rooms) },
            };
            yield return new object[]
            {
                "N-04b",
                "ПОМЕЩЕНИЕ 5-5",
                "пом:5-5",
                new[] { nameof(BuildingUnitLocation.Rooms) },
            };

            // N-05: ВЛАД vs ВЛАДЕНИЕ → Holdings
            yield return new object[]
            {
                "N-05",
                "ВЛАД 1",
                "влад:1",
                new[] { nameof(BuildingUnitLocation.Storages) },
            };
            yield return new object[]
            {
                "N-05b",
                "ВЛАДЕНИЕ 1",
                "влад:1",
                new[] { nameof(BuildingUnitLocation.Storages) },
            };

            // N-06: СКЛ vs СКЛАД → Storages
            yield return new object[]
            {
                "N-06",
                "СКЛ 1",
                "склад:1",
                new[] { nameof(BuildingUnitLocation.Holdings) },
            };
            yield return new object[]
            {
                "N-06b",
                "СКЛАД 1",
                "склад:1",
                new[] { nameof(BuildingUnitLocation.Holdings) },
            };

            // N-07: ПР-Д / ПРОЕЗД на unit-строке → Passages (без уличной оболочки)
            yield return new object[]
            {
                "N-07",
                "пр-д 1",
                "проезд:1",
                new[] { nameof(BuildingUnitLocation.Holdings), nameof(BuildingUnitLocation.Storages) },
            };
            yield return new object[]
            {
                "N-07b",
                "проезд 1",
                "проезд:1",
                new[] { nameof(BuildingUnitLocation.Holdings), nameof(BuildingUnitLocation.Storages) },
            };

            // N-08: типовые «X не заполняет Y» (расширенный empty-neighbor assert)
            yield return new object[]
            {
                "N-08",
                "ОФИС 104",
                "оф:104",
                new[]
                {
                    nameof(BuildingUnitLocation.Rooms),
                    nameof(BuildingUnitLocation.Premises),
                    nameof(BuildingUnitLocation.Apartments),
                },
            };
            yield return new object[]
            {
                "N-08b",
                "КВ 89",
                "кв:89",
                new[]
                {
                    nameof(BuildingUnitLocation.Rooms),
                    nameof(BuildingUnitLocation.Offices),
                    nameof(BuildingUnitLocation.Premises),
                },
            };

            // N-10 bare / optional (§3.5 H02, ST02, B04, M02) — actual-канон
            yield return new object[]
            {
                "N-10-H02",
                "владение",
                "code:владение",
                new[] { nameof(BuildingUnitLocation.Holdings) },
            };
            yield return new object[]
            {
                "N-10-ST02",
                "склад",
                "code:склад",
                new[] { nameof(BuildingUnitLocation.Storages) },
            };
            yield return new object[]
            {
                "N-10-B04",
                "БЛОК",
                string.Empty,
                new[] { nameof(BuildingUnitLocation.Blocks) },
            };
            yield return new object[]
            {
                "N-10-M02",
                "А/Я",
                string.Empty,
                new[] { nameof(BuildingUnitLocation.Mailboxes) },
            };

            // N-11: ложный префикс в слове на изолированной unit-строке.
            // Actual: ShortRoomTypedRegex (К\.?) съедает «К» → ком:вартирный; Apartments пуст.
            yield return new object[]
            {
                "N-11",
                "КВАРТИРНЫЙ",
                "ком:вартирный",
                new[] { nameof(BuildingUnitLocation.Apartments) },
            };
        }

        [Theory]
        [MemberData(nameof(NeighborCases))]
        public void Parse_NeighborMarkers_ReturnsExpectedCanonicalAndEmptyNeighbors(
            string matrixId,
            string input,
            string expectedCanonical,
            string[] emptyNeighborCollections)
        {
            Assert.False(string.IsNullOrWhiteSpace(matrixId));
            BuildingUnitTestAsserts.AssertCanonicalAndFields(
                input,
                expectedCanonical,
                emptyNeighborCollections);
        }
    }
}
