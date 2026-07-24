using VTBL.AddressNormalizer.Infrastructure.BuildingUnit;
using Xunit;

namespace VTBL.AddressNormalizer.UnitTests.Canonicalization.BuildingUnit
{
    /// <summary>
    /// Конвертация чистых римских токенов → арабские (после Parse).
    /// </summary>
    public class BuildingUnitRomanNumeralNormalizerTests
    {
        [Theory]
        [InlineData("I", "1")]
        [InlineData("II", "2")]
        [InlineData("III", "3")]
        [InlineData("IV", "4")]
        [InlineData("V", "5")]
        [InlineData("VIII", "8")]
        [InlineData("IX", "9")]
        [InlineData("X", "10")]
        [InlineData("XII", "12")]
        [InlineData("XXI", "21")]
        [InlineData("xl", "40")]
        [InlineData("MCMXC", "1990")]
        public void ConvertIfPureRoman_PureToken_ReturnsArabic(string input, string expected)
        {
            Assert.Equal(expected, BuildingUnitRomanNumeralNormalizer.ConvertIfPureRoman(input));
        }

        [Theory]
        [InlineData("X-10")]
        [InlineData("2X-2X")]
        [InlineData("2X")]
        [InlineData("IA")]
        [InlineData("XIБ")]
        [InlineData("18С")]
        [InlineData("")]
        [InlineData("   ")]
        public void ConvertIfPureRoman_MixedOrEmpty_LeavesUnchanged(string input)
        {
            Assert.Equal(input, BuildingUnitRomanNumeralNormalizer.ConvertIfPureRoman(input));
        }

        [Theory]
        [InlineData("ПОМ II", "пом:2")]
        [InlineData("ПОМЕЩ. XII", "пом:12")]
        [InlineData("ОФИС II", "оф:2")]
        [InlineData("ОФ VIII", "оф:8")]
        [InlineData("ПОМ IA", "пом:ia")]
        [InlineData("ПОМ XIБ", "пом:xiб")]
        public void Parse_PureRomanInPremiseOrOffice_ConvertsToArabic(
            string input,
            string expectedCanonical)
        {
            BuildingUnitTestAsserts.AssertCanonical(input, expectedCanonical);
        }
    }
}
