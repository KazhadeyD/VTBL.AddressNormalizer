using System.Collections.Generic;
using VTBL.AddressNormalizer.Abstractions.BuildingUnit;
using Xunit;

namespace VTBL.AddressNormalizer.UnitTests.Canonicalization.BuildingUnit
{
    /// <summary>
    /// Категорийное покрытие <see cref="BuildingUnitParser"/> (Theory + MemberData).
    /// </summary>
    public class BuildingUnitParserCategoryTests
    {
        [Theory]
        [InlineData("ЭТ 3", "эт:3")]
        [InlineData("Эт 3", "эт:3")]
        [InlineData("Э 3", "эт:3")]
        public void Parse_CategorySmoke_ReturnsExpectedCanonical(
            string input,
            string expectedCanonical)
        {
            BuildingUnitTestAsserts.AssertCanonical(input, expectedCanonical);
        }

        #region Floors

        public static IEnumerable<object[]> FloorCases()
        {
            yield return new object[] { "ЭТАЖ 4-Я", "эт:4" };
            yield return new object[] { "Э 4", "эт:4" };
            yield return new object[] { "э. 2", "эт:2" };
            yield return new object[] { "ПОДВАЛ", "эт:подвал" };
            yield return new object[] { "ЭТАЖ 1 ПОДВАЛ", "эт:1|эт:подвал" };
            // BareFloorWordRegex снимает голое «ЭТАЖ» перед ОФИС.
            yield return new object[] { "ЭТАЖ, ОФИС 1", "оф:1" };
        }

        [Theory]
        [MemberData(nameof(FloorCases))]
        public void Parse_FloorCases_ReturnsExpectedCanonical(
            string input,
            string expectedCanonical)
        {
            BuildingUnitTestAsserts.AssertCanonical(input, expectedCanonical);
        }

        /// <summary>
        /// Голое «ЭТАЖ» снято, Floors пуст, остаётся Office.
        /// </summary>
        [Fact]
        public void Parse_BareFloorWord_LeavesOfficeOnly()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("ЭТАЖ, ОФИС 1");
            var canonical = AddressNormalizerTestHost.Canonicalizer.ToCanonical(location);

            Assert.Equal("оф:1", canonical);
            Assert.Empty(location.Floors);
            Assert.Equal(new[] { "1" }, location.Offices);
        }

        #endregion

        #region Cabinets

        public static IEnumerable<object[]> CabinetCases()
        {
            yield return new object[] { "КАБ. 12", "каб:12" };
            yield return new object[] { "КАБ 12", "каб:12" };
        }

        [Theory]
        [MemberData(nameof(CabinetCases))]
        public void Parse_CabinetCases_ReturnsExpectedCanonical(
            string input,
            string expectedCanonical)
        {
            BuildingUnitTestAsserts.AssertCanonical(input, expectedCanonical);
        }

        #endregion

        #region Entrances

        public static IEnumerable<object[]> EntranceCases()
        {
            yield return new object[] { "ПОДЪЕЗД/ЭТ 2", "под:2" };
            yield return new object[] { "ПОДЪЕЗД/ЭТАЖ 3", "под:3" };
        }

        [Theory]
        [MemberData(nameof(EntranceCases))]
        public void Parse_EntranceCases_ReturnsExpectedCanonical(
            string input,
            string expectedCanonical)
        {
            BuildingUnitTestAsserts.AssertCanonicalAndFields(
                input,
                expectedCanonical,
                new[] { nameof(BuildingUnitLocation.Floors) });
        }

        /// <summary>
        /// Slash «ПОДЪЕЗД/ЭТ» даёт Entrances, не Floors.
        /// </summary>
        [Fact]
        public void Parse_SlashEntranceEt_FillsEntrancesNotFloors()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("ПОДЪЕЗД/ЭТ 2");
            var canonical = AddressNormalizerTestHost.Canonicalizer.ToCanonical(location);

            Assert.Equal("под:2", canonical);
            Assert.Equal(new[] { "2" }, location.Entrances);
            Assert.Empty(location.Floors);
        }

        /// <summary>
        /// Slash «ПОДЪЕЗД/ЭТАЖ» — тот же контракт Entrances.
        /// </summary>
        [Fact]
        public void Parse_SlashEntranceEtazh_FillsEntrancesNotFloors()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("ПОДЪЕЗД/ЭТАЖ 3");
            var canonical = AddressNormalizerTestHost.Canonicalizer.ToCanonical(location);

            Assert.Equal("под:3", canonical);
            Assert.Equal(new[] { "3" }, location.Entrances);
            Assert.Empty(location.Floors);
        }

        #endregion

        #region Workplaces

        public static IEnumerable<object[]> WorkplaceCases()
        {
            yield return new object[] { "РАБ М 2", "раб.м:2" };
            yield return new object[] { "РМ 1", "раб.м:1" };
            yield return new object[] { "Р.М. 3", "раб.м:3" };
            yield return new object[] { "Р.М.5", "раб.м:5" };
            yield return new object[] { "Раб. место 4", "раб.м:4" };
            yield return new object[] { "Раб. м. 6", "раб.м:6" };
            yield return new object[] { "Раб. мес 7", "раб.м:7" };
            yield return new object[] { "РАБ.МЕСТ 8", "раб.м:8" };
            yield return new object[] { "РАБ.М.9", "раб.м:9" };
            yield return new object[] { "РМ 9", "раб.м:9" };
        }

        [Theory]
        [MemberData(nameof(WorkplaceCases))]
        public void Parse_WorkplaceCases_ReturnsExpectedCanonical(
            string input,
            string expectedCanonical)
        {
            BuildingUnitTestAsserts.AssertCanonical(input, expectedCanonical);
        }

        #endregion

        #region Parts

        public static IEnumerable<object[]> PartCases()
        {
            yield return new object[] { "Ч П 12", "ч.п:12" };
        }

        [Theory]
        [MemberData(nameof(PartCases))]
        public void Parse_PartCases_ReturnsExpectedCanonical(
            string input,
            string expectedCanonical)
        {
            BuildingUnitTestAsserts.AssertCanonical(input, expectedCanonical);
        }

        #endregion

        #region Literas

        public static IEnumerable<object[]> LiteraCases()
        {
            yield return new object[] { "ЛИТЕРА А", "лит:а" };
            // Голое «ЛИТ» лексема ЛИТЕ?РА? не матчит → RawCodes.
            yield return new object[] { "ЛИТ Б", "code:б|note:лит" };
            yield return new object[] { "Курьяновски", "note:курьяновски" };
            yield return new object[] { "III Курьяновски", "code:3|note:курьяновски" };
            yield return new object[] { "ОФИС 5 Курьяновский", "оф:5|note:курьяновский" };
            yield return new object[] { "ЛИТРА А", "лит:а" };
            yield return new object[] { "ЛИТР А", "лит:а" };
            yield return new object[] { "ЛИТЕРА А, ОФИС 1", "оф:1|лит:а" };
        }

        [Theory]
        [MemberData(nameof(LiteraCases))]
        public void Parse_LiteraCases_ReturnsExpectedCanonical(
            string input,
            string expectedCanonical)
        {
            BuildingUnitTestAsserts.AssertCanonical(input, expectedCanonical);
        }

        #endregion

        #region Passages / Holdings / Storages

        public static IEnumerable<object[]> PassageHoldingStorageCases()
        {
            yield return new object[] { "проезд 12А", "проезд:12а" };
            yield return new object[] { "склад 2 влад 3 проезд 1", "проезд:1|влад:3|склад:2" };
        }

        [Theory]
        [MemberData(nameof(PassageHoldingStorageCases))]
        public void Parse_PassageHoldingStorageCases_ReturnsExpectedCanonical(
            string input,
            string expectedCanonical)
        {
            BuildingUnitTestAsserts.AssertCanonical(input, expectedCanonical);
        }

        #endregion

        #region Blocks / Sections

        public static IEnumerable<object[]> BlockSectionCases()
        {
            yield return new object[] { "СЕКЦИЯ 1", "секц:1" };
            yield return new object[] { "БЛОК 1", "блок:1" };
        }

        [Theory]
        [MemberData(nameof(BlockSectionCases))]
        public void Parse_BlockSectionCases_ReturnsExpectedCanonical(
            string input,
            string expectedCanonical)
        {
            BuildingUnitTestAsserts.AssertCanonical(input, expectedCanonical);
        }

        #endregion

        #region Range / Raw / Note / Unparsed

        public static IEnumerable<object[]> RangeRawNoteUnparsedCases()
        {
            yield return new object[] { "foo+bar", "unparsed:foo+bar" };
            yield return new object[] { "ОФИС 5 foo+bar", "оф:5|unparsed:foo+bar" };
            yield return new object[] { "БЦ Речной Вокзал", "note:бц речной вокзал" };
            yield return new object[] { "210 БЦ Речной Вокзал", "code:210|note:бц речной вокзал" };
            yield return new object[] { "1А-2Б", "диап:1а-2б" };
        }

        [Theory]
        [MemberData(nameof(RangeRawNoteUnparsedCases))]
        public void Parse_RangeRawNoteUnparsedCases_ReturnsExpectedCanonical(
            string input,
            string expectedCanonical)
        {
            BuildingUnitTestAsserts.AssertCanonical(input, expectedCanonical);
        }

        /// <summary>
        /// Typed Office + остаток Unparsed в одном Parse.
        /// </summary>
        [Fact]
        public void Parse_TypedPlusUnparsed_FillsOfficesAndUnparsed()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("ОФИС 5 foo+bar");
            var canonical = AddressNormalizerTestHost.Canonicalizer.ToCanonical(location);

            Assert.Equal("оф:5|unparsed:foo+bar", canonical);
            Assert.Equal(new[] { "5" }, location.Offices);
            Assert.NotEmpty(location.Unparsed);
        }

        #endregion

        #region Preprocess / Mixed

        public static IEnumerable<object[]> PreprocessMixedCases()
        {
            yield return new object[] { "\"ЭТАЖ 4 ПОМЕЩЕНИЕ 2\"", "эт:4|пом:2" };
            yield return new object[] { "пом. 1 оф. 2 кв. 3 каб. 4", "пом:1|оф:2|кв:3|каб:4" };
        }

        [Theory]
        [MemberData(nameof(PreprocessMixedCases))]
        public void Parse_PreprocessMixedCases_ReturnsExpectedCanonical(
            string input,
            string expectedCanonical)
        {
            BuildingUnitTestAsserts.AssertCanonical(input, expectedCanonical);
        }

        #endregion

        #region ExpandRange

        public static IEnumerable<object[]> ExpandRangeCases()
        {
            yield return new object[] { "ЭТ 1-3", "эт:1|эт:2|эт:3" };
            yield return new object[] { "КОМ 1-3", "ком:1|ком:2|ком:3" };
            yield return new object[] { "ОФИС 1-3", "оф:1|оф:2|оф:3" };
            yield return new object[] { "КАБ 1-3", "каб:1|каб:2|каб:3" };
            yield return new object[] { "КВ 1-3", "кв:1|кв:2|кв:3" };
            yield return new object[] { "РАБ.М.1-3", "раб.м:1|раб.м:2|раб.м:3" };
            yield return new object[] { "Ч.П.1-2", "ч.п:1|ч.п:2" };
        }

        [Theory]
        [MemberData(nameof(ExpandRangeCases))]
        public void Parse_ExpandRangeCases_ReturnsExpectedCanonical(
            string input,
            string expectedCanonical)
        {
            BuildingUnitTestAsserts.AssertCanonical(input, expectedCanonical);
        }

        [Fact]
        public void Parse_Expand_FillsFloorsInOrder()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("ЭТ 1-3");
            var canonical = AddressNormalizerTestHost.Canonicalizer.ToCanonical(location);

            Assert.Equal("эт:1|эт:2|эт:3", canonical);
            Assert.Equal(new[] { "1", "2", "3" }, location.Floors);
        }

        [Fact]
        public void Parse_Expand_FillsRoomsInOrder()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("КОМ 1-3");
            var canonical = AddressNormalizerTestHost.Canonicalizer.ToCanonical(location);

            Assert.Equal("ком:1|ком:2|ком:3", canonical);
            Assert.Equal(new[] { "1", "2", "3" }, location.Rooms);
        }

        [Fact]
        public void Parse_Expand_FillsOfficesInOrder()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("ОФИС 1-3");
            var canonical = AddressNormalizerTestHost.Canonicalizer.ToCanonical(location);

            Assert.Equal("оф:1|оф:2|оф:3", canonical);
            Assert.Equal(new[] { "1", "2", "3" }, location.Offices);
        }

        [Fact]
        public void Parse_Expand_FillsCabinetsInOrder()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("КАБ 1-3");
            var canonical = AddressNormalizerTestHost.Canonicalizer.ToCanonical(location);

            Assert.Equal("каб:1|каб:2|каб:3", canonical);
            Assert.Equal(new[] { "1", "2", "3" }, location.Cabinets);
        }

        [Fact]
        public void Parse_Expand_FillsApartmentsInOrder()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("КВ 1-3");
            var canonical = AddressNormalizerTestHost.Canonicalizer.ToCanonical(location);

            Assert.Equal("кв:1|кв:2|кв:3", canonical);
            Assert.Equal(new[] { "1", "2", "3" }, location.Apartments);
        }

        [Fact]
        public void Parse_Expand_FillsWorkplacesInOrder()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("РАБ.М.1-3");
            var canonical = AddressNormalizerTestHost.Canonicalizer.ToCanonical(location);

            Assert.Equal("раб.м:1|раб.м:2|раб.м:3", canonical);
            Assert.Equal(new[] { "1", "2", "3" }, location.Workplaces);
        }

        [Fact]
        public void Parse_Expand_FillsPartsInOrder()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("Ч.П.1-2");
            var canonical = AddressNormalizerTestHost.Canonicalizer.ToCanonical(location);

            Assert.Equal("ч.п:1|ч.п:2", canonical);
            Assert.Equal(new[] { "1", "2" }, location.Parts);
        }

        #endregion
    }
}
