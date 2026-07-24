using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using VTBL.AddressNormalizer.Infrastructure.BuildingUnit;
using VTBL.AddressNormalizer.Infrastructure.Shared;
using Xunit;

namespace VTBL.AddressNormalizer.UnitTests.Canonicalization.BuildingUnit
{
    /// <summary>
    /// Слой E: реестр Known Gaps и Characterization / DocumentationOnly.
    /// </summary>
    public class BuildingUnitParserKnownGapTests
    {
        [Fact]
        public void KnownGaps_Registry_ContainsG01ThroughG06()
        {
            var ids = BuildingUnitKnownGaps.Ids;

            Assert.Equal(6, ids.Count);
            Assert.Equal(
                new[]
                {
                    BuildingUnitKnownGaps.G01,
                    BuildingUnitKnownGaps.G02,
                    BuildingUnitKnownGaps.G03,
                    BuildingUnitKnownGaps.G04,
                    BuildingUnitKnownGaps.G05,
                    BuildingUnitKnownGaps.G06,
                },
                ids.ToArray());
        }

        /// <summary>
        /// DocumentationOnly G05: Litera есть в early-маркерах парсера, но отсутствует
        /// в outdoor <see cref="IndoorMarkerPatterns.All"/> (15 kinds; property Litera нет).
        /// Без <c>IndoorMarkerKind.Litera</c> (символа в enum нет).
        /// </summary>
        [Fact]
        public void Gap_G05_Doc_LiteraAbsentFromOutdoorPatterns()
        {
            Assert.Equal(15, IndoorMarkerPatterns.All.Count);
            Assert.Null(
                typeof(IndoorMarkerPatterns).GetProperty(
                    "Litera",
                    BindingFlags.Public | BindingFlags.Static));
            Assert.Contains(BuildingUnitKnownGaps.G05, BuildingUnitKnownGaps.Ids);
        }

        /// <summary>
        /// DocumentationOnly G06: <c>SlashTypeHeaderRegex</c> не включает КАБ/РАБ
        /// (dot-slash ограничен ЭТ|ПОМЕЩ|КОМ|ОФИС).
        /// </summary>
        [Fact]
        public void Gap_G06_Doc_DotSlashExcludesCabRab()
        {
            var pattern = GetPrivateStaticRegexPattern(
                typeof(BuildingUnitParser),
                "SlashTypeHeaderRegex");

            Assert.DoesNotContain("КАБ", pattern, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("РАБ", pattern, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(BuildingUnitKnownGaps.G06, BuildingUnitKnownGaps.Ids);
        }

        /// <summary>
        /// Characterization G01–G04 (UC-06 / F08, P09, N03, R05).
        /// Assert = actual Host Parse→ToCanonical; Desired (ТЗ §6) — только в комментариях.
        /// </summary>
        public static IEnumerable<object[]> GapCharacterizationCases()
        {
            // G01 / F08 — Desired: эт:цокол
            yield return new object[] { BuildingUnitKnownGaps.G01, "ЦОКОЛ", "code:цокол" };
            // G02 / P09 — Desired: неж.пом:5 (typed); оба написания маркера
            yield return new object[] { BuildingUnitKnownGaps.G02, "НЕЖ.ПОМ 5", "пом:5|code:неж" };
            yield return new object[] { BuildingUnitKnownGaps.G02, "НЕЖ ПОМ 5", "пом:5|code:неж" };
            // G03 / N03 — Desired: секц:1
            yield return new object[] { BuildingUnitKnownGaps.G03, "СЕКЦ 1", "code:1|code:секц" };
            // G04 / R05 — Desired: ком:3|ком:4; FullTests quirk не удалять
            yield return new object[] { BuildingUnitKnownGaps.G04, "КОМ. 3,4", "ком:3|code:4" };
        }

        public static IEnumerable<object[]> GapG01Cases() => CasesFor(BuildingUnitKnownGaps.G01);

        public static IEnumerable<object[]> GapG02Cases() => CasesFor(BuildingUnitKnownGaps.G02);

        public static IEnumerable<object[]> GapG03Cases() => CasesFor(BuildingUnitKnownGaps.G03);

        public static IEnumerable<object[]> GapG04Cases() => CasesFor(BuildingUnitKnownGaps.G04);

        private static IEnumerable<object[]> CasesFor(string gapId) =>
            GapCharacterizationCases().Where(row => (string)row[0] == gapId);

        [Theory]
        [MemberData(nameof(GapG01Cases))]
        public void Gap_G01_Cokol_MapsToRawCode(
            string gapId,
            string input,
            string actualCanonical)
        {
            Assert.Equal(BuildingUnitKnownGaps.G01, gapId);
            BuildingUnitTestAsserts.AssertCanonical(input, actualCanonical);
        }

        [Theory]
        [MemberData(nameof(GapG02Cases))]
        public void Gap_G02_NezhPom_LeavesNezhAsCode(
            string gapId,
            string input,
            string actualCanonical)
        {
            Assert.Equal(BuildingUnitKnownGaps.G02, gapId);
            BuildingUnitTestAsserts.AssertCanonical(input, actualCanonical);
        }

        [Theory]
        [MemberData(nameof(GapG03Cases))]
        public void Gap_G03_Sekc_MapsToRawCodes(
            string gapId,
            string input,
            string actualCanonical)
        {
            Assert.Equal(BuildingUnitKnownGaps.G03, gapId);
            BuildingUnitTestAsserts.AssertCanonical(input, actualCanonical);
        }

        [Theory]
        [MemberData(nameof(GapG04Cases))]
        public void Gap_G04_KomComma_SecondValueIsRawCode(
            string gapId,
            string input,
            string actualCanonical)
        {
            Assert.Equal(BuildingUnitKnownGaps.G04, gapId);
            BuildingUnitTestAsserts.AssertCanonical(input, actualCanonical);
        }

        private static string GetPrivateStaticRegexPattern(Type declaringType, string fieldName)
        {
            var field = declaringType.GetField(
                fieldName,
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(field);

            var regex = field.GetValue(null) as Regex;
            Assert.NotNull(regex);

            return regex.ToString();
        }
    }
}
