using System.Collections.Generic;
using VTBL.AddressNormalizer.Abstractions.BuildingUnit;
using Xunit;

namespace VTBL.AddressNormalizer.UnitTests.Canonicalization.BuildingUnit
{
    /// <summary>
    /// Соседство маркеров и bare/optional на изолированной unit-строке.
    /// </summary>
    public class BuildingUnitParserNegativeTests
    {
        /// <summary>
        /// NeighborCases: (input, expectedCanonical, emptyNeighborCollections).
        /// Лишние values slash → RawCodes: см. SlashChain
        /// <c>ЭТАЖ/ОФИС 3/314/5/WP</c> — не дублируем здесь.
        /// </summary>
        public static IEnumerable<object[]> NeighborCases()
        {
            // ОФ / ОФИС → Offices; сосед Rooms пуст
            yield return new object[]
            {
                "ОФ 79",
                "оф:79",
                new[] { nameof(BuildingUnitLocation.Rooms) },
            };
            yield return new object[]
            {
                "ОФИС 104",
                "оф:104",
                new[] { nameof(BuildingUnitLocation.Rooms) },
            };

            // КОМ / КОМНАТА / КО / К. → Rooms; ShortRoom «К. 5-20» без expand
            yield return new object[]
            {
                "КОМ 10",
                "ком:10",
                new[] { nameof(BuildingUnitLocation.Apartments) },
            };
            yield return new object[]
            {
                "КОМНАТА 136",
                "ком:136",
                new[] { nameof(BuildingUnitLocation.Apartments) },
            };
            yield return new object[]
            {
                "КО 10",
                "ком:10",
                new[] { nameof(BuildingUnitLocation.Apartments) },
            };
            yield return new object[]
            {
                "К. 5-20",
                "ком:5-20",
                new[] { nameof(BuildingUnitLocation.Apartments) },
            };

            // КВ vs К. — Apartments vs Rooms
            yield return new object[]
            {
                "КВ 89",
                "кв:89",
                new[] { nameof(BuildingUnitLocation.Rooms) },
            };
            yield return new object[]
            {
                "К. 7",
                "ком:7",
                new[] { nameof(BuildingUnitLocation.Apartments) },
            };

            // ПОМ / ПОМЕЩЕНИЕ → Premises
            yield return new object[]
            {
                "ПОМ 183",
                "пом:183",
                new[] { nameof(BuildingUnitLocation.Rooms) },
            };
            yield return new object[]
            {
                "ПОМЕЩЕНИЕ 5-5",
                "пом:5-5",
                new[] { nameof(BuildingUnitLocation.Rooms) },
            };

            // ВЛАД / ВЛАДЕНИЕ → Holdings
            yield return new object[]
            {
                "ВЛАД 1",
                "влад:1",
                new[] { nameof(BuildingUnitLocation.Storages) },
            };
            yield return new object[]
            {
                "ВЛАДЕНИЕ 1",
                "влад:1",
                new[] { nameof(BuildingUnitLocation.Storages) },
            };

            // СКЛ / СКЛАД → Storages
            yield return new object[]
            {
                "СКЛ 1",
                "склад:1",
                new[] { nameof(BuildingUnitLocation.Holdings) },
            };
            yield return new object[]
            {
                "СКЛАД 1",
                "склад:1",
                new[] { nameof(BuildingUnitLocation.Holdings) },
            };

            // ПР-Д / ПРОЕЗД → Passages
            yield return new object[]
            {
                "пр-д 1",
                "проезд:1",
                new[] { nameof(BuildingUnitLocation.Holdings), nameof(BuildingUnitLocation.Storages) },
            };
            yield return new object[]
            {
                "проезд 1",
                "проезд:1",
                new[] { nameof(BuildingUnitLocation.Holdings), nameof(BuildingUnitLocation.Storages) },
            };

            // Маркер X не заполняет соседние категории
            yield return new object[]
            {
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
                "КВ 89",
                "кв:89",
                new[]
                {
                    nameof(BuildingUnitLocation.Rooms),
                    nameof(BuildingUnitLocation.Offices),
                    nameof(BuildingUnitLocation.Premises),
                },
            };

            // Bare / optional без номера — текущее поведение
            yield return new object[]
            {
                "владение",
                "code:владение",
                new[] { nameof(BuildingUnitLocation.Holdings) },
            };
            yield return new object[]
            {
                "склад",
                "code:склад",
                new[] { nameof(BuildingUnitLocation.Storages) },
            };
            yield return new object[]
            {
                "БЛОК",
                string.Empty,
                new[] { nameof(BuildingUnitLocation.Blocks) },
            };
            yield return new object[]
            {
                "А/Я",
                string.Empty,
                new[] { nameof(BuildingUnitLocation.Mailboxes) },
            };

            // Ложный префикс: ShortRoom (К\.?) съедает «К» → ком:вартирный; Apartments пуст
            yield return new object[]
            {
                "КВАРТИРНЫЙ",
                "ком:вартирный",
                new[] { nameof(BuildingUnitLocation.Apartments) },
            };
        }

        [Theory]
        [MemberData(nameof(NeighborCases))]
        public void Parse_NeighborMarkers_ReturnsExpectedCanonicalAndEmptyNeighbors(
            string input,
            string expectedCanonical,
            string[] emptyNeighborCollections)
        {
            BuildingUnitTestAsserts.AssertCanonicalAndFields(
                input,
                expectedCanonical,
                emptyNeighborCollections);
        }
    }
}
