using System;
using System.Reflection;
using System.Text.RegularExpressions;
using VTBL.AddressNormalizer.Infrastructure.BuildingUnit;
using VTBL.AddressNormalizer.Infrastructure.Shared;
using Xunit;

namespace VTBL.AddressNormalizer.UnitTests.Canonicalization.BuildingUnit
{
    /// <summary>
    /// Фиксация известных пробелов контракта (текущее поведение Host, без правок прода).
    /// </summary>
    public class BuildingUnitParserKnownGapTests
    {
        /// <summary>
        /// Литера есть в early-маркерах парсера, но отсутствует в outdoor
        /// <see cref="IndoorMarkerPatterns.All"/> (15 kinds; property Litera нет).
        /// </summary>
        [Fact]
        public void Litera_IsAbsentFromOutdoorMarkerPatterns()
        {
            Assert.Equal(15, IndoorMarkerPatterns.All.Count);
            Assert.Null(
                typeof(IndoorMarkerPatterns).GetProperty(
                    "Litera",
                    BindingFlags.Public | BindingFlags.Static));
        }

        /// <summary>
        /// Dot-slash заголовки не включают КАБ/РАБ (только ЭТ|ПОМЕЩ|КОМ|ОФИС).
        /// </summary>
        [Fact]
        public void DotSlashHeader_ExcludesCabinetAndWorkplace()
        {
            var pattern = GetPrivateStaticRegexPattern(
                typeof(BuildingUnitParser),
                "SlashTypeHeaderRegex");

            Assert.DoesNotContain("КАБ", pattern, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("РАБ", pattern, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Желаемо: эт:цокол — сейчас уходит в RawCodes.
        /// </summary>
        [Fact]
        public void Parse_Cokol_MapsToRawCode()
        {
            BuildingUnitTestAsserts.AssertCanonical("ЦОКОЛ", "code:цокол");
        }

        /// <summary>
        /// Желаемо: отдельная категория нежилого помещения — сейчас «неж» в RawCodes.
        /// </summary>
        [Theory]
        [InlineData("НЕЖ.ПОМ 5")]
        [InlineData("НЕЖ ПОМ 5")]
        public void Parse_NezhPom_LeavesNezhAsCode(string input)
        {
            BuildingUnitTestAsserts.AssertCanonical(input, "пом:5|code:неж");
        }

        /// <summary>
        /// Желаемо: секц:1 — сокращение «СЕКЦ» сейчас не матчит SectionRegex.
        /// </summary>
        [Fact]
        public void Parse_SekcAbbreviation_MapsToRawCodes()
        {
            BuildingUnitTestAsserts.AssertCanonical("СЕКЦ 1", "code:1|code:секц");
        }

        /// <summary>
        /// Желаемо: ком:3|ком:4 — второй номер после запятой уходит в RawCodes.
        /// </summary>
        [Fact]
        public void Parse_KomCommaList_SecondValueIsRawCode()
        {
            BuildingUnitTestAsserts.AssertCanonical("КОМ. 3,4", "ком:3|code:4");
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
