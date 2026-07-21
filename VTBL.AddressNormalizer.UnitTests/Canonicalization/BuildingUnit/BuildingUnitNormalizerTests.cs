using System.Text.RegularExpressions;
using VTBL.AddressNormalizer.Abstractions.Shared;
using VTBL.AddressNormalizer.UnitTests;
using VTBL.AddressNormalizer.Abstractions.BuildingUnit;
using Xunit;

namespace VTBL.AddressNormalizer.UnitTests.Canonicalization.BuildingUnit
{
    public class BuildingUnitNormalizerTests
    {
        [Theory]
        [MemberData(nameof(BuildingUnitSampleCases.NormalizeCases), MemberType = typeof(BuildingUnitSampleCases))]
        public void Normalize_ReturnsExpectedCanonical(string input, string expectedCanonical)
        {
            var result = AddressNormalizerTestHost.Normalizer.Normalize(input);

            Assert.Equal(input, result.Original);
            Assert.Equal(expectedCanonical, result.Canonical);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Normalize_EmptyInput_ReturnsEmptyCanonical(string input)
        {
            var result = AddressNormalizerTestHost.Normalizer.Normalize(input);

            Assert.Equal(input, result.Original);
            Assert.Equal(string.Empty, result.Canonical);
            Assert.Empty(result.Location.Floors);
            Assert.Empty(result.Location.Premises);
            Assert.Empty(result.Location.Rooms);
            Assert.Empty(result.Location.Offices);
            Assert.Empty(result.Location.RawCodes);
        }

        [Fact]
        public void Normalize_Hash_IsSha256OfCanonical()
        {
            var result = AddressNormalizerTestHost.Normalizer.Normalize("ЭТАЖ 4 ПОМЕЩЕНИЕ 2");

            Assert.Equal(AddressNormalizerTestHost.Hash.ComputeSha256(result.Canonical), result.Hash);
            Assert.Matches(new Regex("^[0-9a-f]{64}$"), result.Hash);
        }

        [Fact]
        public void Normalize_SameInput_ProducesStableHash()
        {
            const string input = "ОФИС №18С";

            var first = AddressNormalizerTestHost.Normalizer.Normalize(input);
            var second = AddressNormalizerTestHost.Normalizer.Normalize(input);

            Assert.Equal(first.Canonical, second.Canonical);
            Assert.Equal(first.Hash, second.Hash);
            Assert.Equal(first.Json, second.Json);
        }

        [Fact]
        public void Normalize_Json_ContainsParsedStructure()
        {
            var result = AddressNormalizerTestHost.Normalizer.Normalize("ЭТАЖ 4 ПОМЕЩЕНИЕ 2");

            Assert.Contains("\"floors\":[\"4\"]", result.Json);
            Assert.Contains("\"premises\":[\"2\"]", result.Json);
            Assert.Single(result.Location.Floors);
            Assert.Single(result.Location.Premises);
        }

        [Fact]
        public void Normalize_SlashFormat_PopulatesAllSegments()
        {
            var result = AddressNormalizerTestHost.Normalizer.Normalize("ЭТ./ПОМЕЩ. 0/II КОМ./ОФИС 1/24");

            Assert.Equal("0", result.Location.Floors[0]);
            Assert.Equal("II", result.Location.Premises[0]);
            Assert.Equal("1", result.Location.Rooms[0]);
            Assert.Equal("24", result.Location.Offices[0]);
        }

        [Fact]
        public void Normalize_Note_ExtractedSeparately()
        {
            var result = AddressNormalizerTestHost.Normalizer.Normalize("ЭТАЖ ЦОКОЛЬНЫЙ, ВХОД С ТОРЦА ОФИС 1");

            Assert.Contains("вход с торца", result.Location.Notes);
            Assert.Contains("note:вход с торца", result.Canonical);
        }
    }
}
