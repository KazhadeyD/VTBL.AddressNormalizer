using System.Collections.Generic;
using VTBL.AddressNormalizer.Abstractions.BuildingUnit;
using Xunit;

namespace VTBL.AddressNormalizer.UnitTests.Canonicalization.BuildingUnit
{
    /// <summary>
    /// Слой B: матричное покрытие категорий BuildingUnitParser (Theory + MemberData).
    /// </summary>
    public class BuildingUnitParserCategoryTests
    {
        // MemberData-регионы (architecture §3.5): FloorCases, PremiseCases, RoomCases,
        // OfficeCases, WorkplaceCases, PartCases, ApartmentCases, CabinetCases,
        // EntranceCases, PassageHoldingStorageCases, BlockSectionCases, MailboxCases,
        // LiteraCases, RangeRawNoteUnparsedCases, PreprocessMixedCases, ExpandRangeCases.
        // Наполнение — задачи 2.x.

        [Theory]
        [InlineData("F02", "ЭТ 3", "эт:3")]
        public void Parse_CategorySmoke_ReturnsExpectedCanonical(
            string matrixId,
            string input,
            string expectedCanonical)
        {
            Assert.Equal("F02", matrixId);
            BuildingUnitTestAsserts.AssertCanonical(input, expectedCanonical);
        }

        #region Floors

        /// <summary>
        /// M1 Floors (architecture §3.5 F04/F06/F06b/F12). F01–F03/F05/F07/F09–F10 уже C;
        /// F11 expand — <see cref="ExpandRangeCases"/>; F08 (ЦОКОЛ) — G01 → task 3.1, не здесь.
        /// </summary>
        public static IEnumerable<object[]> FloorCases()
        {
            yield return new object[] { "F04", "ЭТАЖ 4-Я", "эт:4" };
            yield return new object[] { "F06", "ПОДВАЛ", "эт:подвал" };
            yield return new object[] { "F06b", "ЭТАЖ 1 ПОДВАЛ", "эт:1|эт:подвал" };
            // BareFloorWordRegex снимает голое «ЭТАЖ» перед ОФИС.
            yield return new object[] { "F12", "ЭТАЖ, ОФИС 1", "оф:1" };
        }

        // DisplayName в отчёте: "{MatrixId}: {input}" (аргументы Theory).
        [Theory]
        [MemberData(nameof(FloorCases))]
        public void Parse_FloorCases_ReturnsExpectedCanonical(
            string matrixId,
            string input,
            string expectedCanonical)
        {
            Assert.False(string.IsNullOrWhiteSpace(matrixId));
            BuildingUnitTestAsserts.AssertCanonical(input, expectedCanonical);
        }

        /// <summary>
        /// F12: голое «ЭТАЖ» снято, Floors пуст, остаётся Office.
        /// </summary>
        [Fact]
        public void Parse_F12_BareFloorWord_LeavesOfficeOnly()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("ЭТАЖ, ОФИС 1");
            var canonical = AddressNormalizerTestHost.Canonicalizer.ToCanonical(location);

            Assert.Equal("оф:1", canonical);
            Assert.Empty(location.Floors);
            Assert.Equal(new[] { "1" }, location.Offices);
        }

        #endregion

        #region Rooms

        /// <summary>
        /// M3 Rooms: R01–R07 уже C; R08 expand — <see cref="ExpandRangeCases"/> (не ShortRoom R03).
        /// </summary>
        public static IEnumerable<object[]> RoomCases() => System.Array.Empty<object[]>();

        #endregion

        #region Offices

        /// <summary>
        /// M4 Offices: O01–O04 уже C; O05 expand — <see cref="ExpandRangeCases"/>.
        /// </summary>
        public static IEnumerable<object[]> OfficeCases() => System.Array.Empty<object[]>();

        #endregion

        #region Apartments

        /// <summary>
        /// A01–A02 уже C; A04 — pipeline SampleCases; A03 expand — <see cref="ExpandRangeCases"/>.
        /// </summary>
        public static IEnumerable<object[]> ApartmentCases() => System.Array.Empty<object[]>();

        #endregion

        #region Cabinets

        /// <summary>
        /// M8 Cabinets (architecture §3.5 C02/C02b). C05 — pipeline SampleCases, не дублировать.
        /// </summary>
        public static IEnumerable<object[]> CabinetCases()
        {
            yield return new object[] { "C02", "КАБ. 12", "каб:12" };
            yield return new object[] { "C02b", "КАБ 12", "каб:12" };
        }

        // DisplayName в отчёте: "{MatrixId}: {input}" (аргументы Theory).
        [Theory]
        [MemberData(nameof(CabinetCases))]
        public void Parse_CabinetCases_ReturnsExpectedCanonical(
            string matrixId,
            string input,
            string expectedCanonical)
        {
            Assert.False(string.IsNullOrWhiteSpace(matrixId));
            BuildingUnitTestAsserts.AssertCanonical(input, expectedCanonical);
        }

        #endregion

        #region Entrances

        /// <summary>
        /// M9 Entrances (architecture §3.5 E02/E02b). Slash ПОДЪЕЗД/ЭТ → Entrances, Floors пуст.
        /// E03 — pipeline SampleCases, не дублировать. Range Entrances не требуется (§2.M.0).
        /// </summary>
        public static IEnumerable<object[]> EntranceCases()
        {
            yield return new object[] { "E02", "ПОДЪЕЗД/ЭТ 2", "под:2" };
            yield return new object[] { "E02b", "ПОДЪЕЗД/ЭТАЖ 3", "под:3" };
        }

        [Theory]
        [MemberData(nameof(EntranceCases))]
        public void Parse_EntranceCases_ReturnsExpectedCanonical(
            string matrixId,
            string input,
            string expectedCanonical)
        {
            Assert.False(string.IsNullOrWhiteSpace(matrixId));
            BuildingUnitTestAsserts.AssertCanonicalAndFields(
                input,
                expectedCanonical,
                new[] { nameof(BuildingUnitLocation.Floors) });
        }

        /// <summary>
        /// E02: slash-цепочка даёт Entrances, не Floors.
        /// </summary>
        [Fact]
        public void Parse_E02_SlashEntrance_FillsEntrancesNotFloors()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("ПОДЪЕЗД/ЭТ 2");
            var canonical = AddressNormalizerTestHost.Canonicalizer.ToCanonical(location);

            Assert.Equal("под:2", canonical);
            Assert.Equal(new[] { "2" }, location.Entrances);
            Assert.Empty(location.Floors);
        }

        /// <summary>
        /// E02b: полная форма ЭТАЖ в slash — тот же контракт Entrances.
        /// </summary>
        [Fact]
        public void Parse_E02b_SlashEntrance_FillsEntrancesNotFloors()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("ПОДЪЕЗД/ЭТАЖ 3");
            var canonical = AddressNormalizerTestHost.Canonicalizer.ToCanonical(location);

            Assert.Equal("под:3", canonical);
            Assert.Equal(new[] { "3" }, location.Entrances);
            Assert.Empty(location.Floors);
        }

        #endregion

        #region Workplaces

        /// <summary>
        /// M5 Workplaces (architecture §3.5 W02). W01 — pipeline SampleCases; W04 expand — <see cref="ExpandRangeCases"/>.
        /// </summary>
        public static IEnumerable<object[]> WorkplaceCases()
        {
            yield return new object[] { "W02", "РАБ М 2", "раб.м:2" };
        }

        // DisplayName в отчёте: "{MatrixId}: {input}" (аргументы Theory).
        [Theory]
        [MemberData(nameof(WorkplaceCases))]
        public void Parse_WorkplaceCases_ReturnsExpectedCanonical(
            string matrixId,
            string input,
            string expectedCanonical)
        {
            Assert.False(string.IsNullOrWhiteSpace(matrixId));
            BuildingUnitTestAsserts.AssertCanonical(input, expectedCanonical);
        }

        #endregion

        #region Parts

        /// <summary>
        /// M6 Parts (architecture §3.5 T02). T01* — pipeline/Full; T03 expand — <see cref="ExpandRangeCases"/>.
        /// </summary>
        public static IEnumerable<object[]> PartCases()
        {
            yield return new object[] { "T02", "Ч П 12", "ч.п:12" };
        }

        [Theory]
        [MemberData(nameof(PartCases))]
        public void Parse_PartCases_ReturnsExpectedCanonical(
            string matrixId,
            string input,
            string expectedCanonical)
        {
            Assert.False(string.IsNullOrWhiteSpace(matrixId));
            BuildingUnitTestAsserts.AssertCanonical(input, expectedCanonical);
        }

        #endregion

        #region Literas

        public static IEnumerable<object[]> LiteraCases()
        {
            yield return new object[] { "L01", "ЛИТЕРА А", "лит:а" };
            // Characterization: лексема ЛИТЕ?РА? не матчит голое «ЛИТ» → RawCodes.
            yield return new object[] { "L02", "ЛИТ Б", "code:б|code:лит" };
            yield return new object[] { "L02b", "ЛИТРА А", "лит:а" };
            yield return new object[] { "L02b", "ЛИТР А", "лит:а" };
            yield return new object[] { "L03", "ЛИТЕРА А, ОФИС 1", "оф:1|лит:а" };
        }

        // DisplayName в отчёте: "{MatrixId}: {input}" (аргументы Theory).
        [Theory]
        [MemberData(nameof(LiteraCases))]
        public void Parse_LiteraCases_ReturnsExpectedCanonical(
            string matrixId,
            string input,
            string expectedCanonical)
        {
            Assert.False(string.IsNullOrWhiteSpace(matrixId));
            BuildingUnitTestAsserts.AssertCanonical(input, expectedCanonical);
        }

        #endregion

        #region Passages / Holdings / Storages

        /// <summary>
        /// M10–M14 Passages/Holdings/Storages (architecture §3.5 S03/S04).
        /// S04 закрывает mixed early Passages+Holdings+Storages (X08 / H03 / ST03).
        /// H02/ST02/B04/M02 (bare) → task 4.1, не здесь.
        /// </summary>
        public static IEnumerable<object[]> PassageHoldingStorageCases()
        {
            yield return new object[] { "S03", "проезд 12А", "проезд:12а" };
            // Один кейс: S04 + X08 (kitchen-sink early markers).
            yield return new object[] { "S04", "склад 2 влад 3 проезд 1", "проезд:1|влад:3|склад:2" };
        }

        // DisplayName в отчёте: "{MatrixId}: {input}" (аргументы Theory).
        [Theory]
        [MemberData(nameof(PassageHoldingStorageCases))]
        public void Parse_PassageHoldingStorageCases_ReturnsExpectedCanonical(
            string matrixId,
            string input,
            string expectedCanonical)
        {
            Assert.False(string.IsNullOrWhiteSpace(matrixId));
            BuildingUnitTestAsserts.AssertCanonical(input, expectedCanonical);
        }

        #endregion

        #region Blocks / Sections

        /// <summary>
        /// M17 Blocks / M19 Sections (architecture §3.5 B02/N02). Без «НОМЕР» у секции.
        /// N03/B04 (gaps/bare) → task 3.1 / 4.1. G03 «СЕКЦ 1» не путать с N02.
        /// </summary>
        public static IEnumerable<object[]> BlockSectionCases()
        {
            yield return new object[] { "N02", "СЕКЦИЯ 1", "секц:1" };
            yield return new object[] { "B02", "БЛОК 1", "блок:1" };
        }

        [Theory]
        [MemberData(nameof(BlockSectionCases))]
        public void Parse_BlockSectionCases_ReturnsExpectedCanonical(
            string matrixId,
            string input,
            string expectedCanonical)
        {
            Assert.False(string.IsNullOrWhiteSpace(matrixId));
            BuildingUnitTestAsserts.AssertCanonical(input, expectedCanonical);
        }

        #endregion

        #region Range / Raw / Note / Unparsed

        /// <summary>
        /// M20 Unparsed / Notes / Ranges (architecture §3.5 U01–U02, NT02/NT02b, D02).
        /// NT01/NT03 — pipeline SampleCases, не дублировать. F08/N03/P09 → task 3.1.
        /// </summary>
        public static IEnumerable<object[]> RangeRawNoteUnparsedCases()
        {
            yield return new object[] { "U01", "foo+bar", "unparsed:foo+bar" };
            yield return new object[] { "U02", "ОФИС 5 foo+bar", "оф:5|unparsed:foo+bar" };
            yield return new object[] { "NT02", "БЦ Речной Вокзал", "note:бц речной вокзал" };
            yield return new object[] { "NT02b", "210 БЦ Речной Вокзал", "code:210|note:бц речной вокзал" };
            // letter-suffix range; Host actual = desired диап:1а-2б.
            yield return new object[] { "D02", "1А-2Б", "диап:1а-2б" };
        }

        [Theory]
        [MemberData(nameof(RangeRawNoteUnparsedCases))]
        public void Parse_RangeRawNoteUnparsedCases_ReturnsExpectedCanonical(
            string matrixId,
            string input,
            string expectedCanonical)
        {
            Assert.False(string.IsNullOrWhiteSpace(matrixId));
            BuildingUnitTestAsserts.AssertCanonical(input, expectedCanonical);
        }

        /// <summary>
        /// U02: typed Office + остаток Unparsed в одном Parse.
        /// </summary>
        [Fact]
        public void Parse_U02_TypedPlusUnparsed_FillsOfficesAndUnparsed()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("ОФИС 5 foo+bar");
            var canonical = AddressNormalizerTestHost.Canonicalizer.ToCanonical(location);

            Assert.Equal("оф:5|unparsed:foo+bar", canonical);
            Assert.Equal(new[] { "5" }, location.Offices);
            Assert.NotEmpty(location.Unparsed);
        }

        #endregion

        #region Preprocess / Mixed

        /// <summary>
        /// M21 Preprocess/Mixed (architecture §3.5 X01 quotes, X07 kitchen-sink).
        /// X01: кавычки вокруг строки снимаются в Preprocess (UC-03).
        /// </summary>
        public static IEnumerable<object[]> PreprocessMixedCases()
        {
            yield return new object[] { "X01", "\"ЭТАЖ 4 ПОМЕЩЕНИЕ 2\"", "эт:4|пом:2" };
            yield return new object[] { "X07", "пом. 1 оф. 2 кв. 3 каб. 4", "пом:1|оф:2|кв:3|каб:4" };
        }

        [Theory]
        [MemberData(nameof(PreprocessMixedCases))]
        public void Parse_PreprocessMixedCases_ReturnsExpectedCanonical(
            string matrixId,
            string input,
            string expectedCanonical)
        {
            Assert.False(string.IsNullOrWhiteSpace(matrixId));
            BuildingUnitTestAsserts.AssertCanonical(input, expectedCanonical);
        }

        #endregion

        #region ExpandRange

        /// <summary>
        /// §2.M.0 expand (architecture §3.5 F11/R08/O05/C03/A03/W04/T03). UC-05: expand → field-assert состава.
        /// </summary>
        public static IEnumerable<object[]> ExpandRangeCases()
        {
            yield return new object[] { "F11", "ЭТ 1-3", "эт:1|эт:2|эт:3" };
            yield return new object[] { "R08", "КОМ 1-3", "ком:1|ком:2|ком:3" };
            yield return new object[] { "O05", "ОФИС 1-3", "оф:1|оф:2|оф:3" };
            yield return new object[] { "C03", "КАБ 1-3", "каб:1|каб:2|каб:3" };
            yield return new object[] { "A03", "КВ 1-3", "кв:1|кв:2|кв:3" };
            yield return new object[] { "W04", "РАБ.М.1-3", "раб.м:1|раб.м:2|раб.м:3" };
            yield return new object[] { "T03", "Ч.П.1-2", "ч.п:1|ч.п:2" };
        }

        [Theory]
        [MemberData(nameof(ExpandRangeCases))]
        public void Parse_ExpandRangeCases_ReturnsExpectedCanonical(
            string matrixId,
            string input,
            string expectedCanonical)
        {
            Assert.False(string.IsNullOrWhiteSpace(matrixId));
            BuildingUnitTestAsserts.AssertCanonical(input, expectedCanonical);
        }

        /// <summary>
        /// F11: numeric range expand → Floors 1,2,3 по порядку.
        /// </summary>
        [Fact]
        public void Parse_F11_Expand_FillsFloorsInOrder()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("ЭТ 1-3");
            var canonical = AddressNormalizerTestHost.Canonicalizer.ToCanonical(location);

            Assert.Equal("эт:1|эт:2|эт:3", canonical);
            Assert.Equal(new[] { "1", "2", "3" }, location.Floors);
        }

        /// <summary>
        /// R08: numeric range expand → Rooms 1,2,3 по порядку (не ShortRoom R03).
        /// </summary>
        [Fact]
        public void Parse_R08_Expand_FillsRoomsInOrder()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("КОМ 1-3");
            var canonical = AddressNormalizerTestHost.Canonicalizer.ToCanonical(location);

            Assert.Equal("ком:1|ком:2|ком:3", canonical);
            Assert.Equal(new[] { "1", "2", "3" }, location.Rooms);
        }

        /// <summary>
        /// O05: numeric range expand → Offices 1,2,3 по порядку.
        /// </summary>
        [Fact]
        public void Parse_O05_Expand_FillsOfficesInOrder()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("ОФИС 1-3");
            var canonical = AddressNormalizerTestHost.Canonicalizer.ToCanonical(location);

            Assert.Equal("оф:1|оф:2|оф:3", canonical);
            Assert.Equal(new[] { "1", "2", "3" }, location.Offices);
        }

        /// <summary>
        /// C03: numeric range expand → Cabinets 1,2,3 по порядку.
        /// </summary>
        [Fact]
        public void Parse_C03_Expand_FillsCabinetsInOrder()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("КАБ 1-3");
            var canonical = AddressNormalizerTestHost.Canonicalizer.ToCanonical(location);

            Assert.Equal("каб:1|каб:2|каб:3", canonical);
            Assert.Equal(new[] { "1", "2", "3" }, location.Cabinets);
        }

        /// <summary>
        /// A03: numeric range expand → Apartments 1,2,3 по порядку.
        /// </summary>
        [Fact]
        public void Parse_A03_Expand_FillsApartmentsInOrder()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("КВ 1-3");
            var canonical = AddressNormalizerTestHost.Canonicalizer.ToCanonical(location);

            Assert.Equal("кв:1|кв:2|кв:3", canonical);
            Assert.Equal(new[] { "1", "2", "3" }, location.Apartments);
        }

        /// <summary>
        /// W04: numeric range expand → Workplaces 1,2,3 по порядку.
        /// </summary>
        [Fact]
        public void Parse_W04_Expand_FillsWorkplacesInOrder()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("РАБ.М.1-3");
            var canonical = AddressNormalizerTestHost.Canonicalizer.ToCanonical(location);

            Assert.Equal("раб.м:1|раб.м:2|раб.м:3", canonical);
            Assert.Equal(new[] { "1", "2", "3" }, location.Workplaces);
        }

        /// <summary>
        /// T03: numeric range expand → Parts 1,2 по порядку.
        /// </summary>
        [Fact]
        public void Parse_T03_Expand_FillsPartsInOrder()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("Ч.П.1-2");
            var canonical = AddressNormalizerTestHost.Canonicalizer.ToCanonical(location);

            Assert.Equal("ч.п:1|ч.п:2", canonical);
            Assert.Equal(new[] { "1", "2" }, location.Parts);
        }

        #endregion
    }
}
