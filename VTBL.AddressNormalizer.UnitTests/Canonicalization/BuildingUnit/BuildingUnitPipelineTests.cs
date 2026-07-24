using System.Text.RegularExpressions;
using VTBL.AddressNormalizer.UnitTests;
using Xunit;

namespace VTBL.AddressNormalizer.UnitTests.Canonicalization.BuildingUnit
{
    /// <summary>
    /// Pipeline indoor: Parse → ToCanonical → SHA256.
    /// </summary>
    public class BuildingUnitPipelineTests
    {
        [Theory]
        [MemberData(nameof(BuildingUnitSampleCases.NormalizeCases), MemberType = typeof(BuildingUnitSampleCases))]
        public void ParseAndCanonicalize_ReturnsExpectedCanonical(string input, string expectedCanonical)
        {
            var location = AddressNormalizerTestHost.Parser.Parse(input);
            var canonical = AddressNormalizerTestHost.Canonicalizer.ToCanonical(location);

            Assert.Equal(expectedCanonical, canonical);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Parse_EmptyInput_ReturnsEmptyCanonical(string input)
        {
            var location = AddressNormalizerTestHost.Parser.Parse(input);
            var canonical = AddressNormalizerTestHost.Canonicalizer.ToCanonical(location);

            Assert.Equal(string.Empty, canonical);
            Assert.Empty(location.Floors);
            Assert.Empty(location.Premises);
            Assert.Empty(location.Rooms);
            Assert.Empty(location.Offices);
            Assert.Empty(location.RawCodes);
        }

        [Fact]
        public void Hash_IsSha256OfCanonical()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("ЭТАЖ 4 ПОМЕЩЕНИЕ 2");
            var canonical = AddressNormalizerTestHost.Canonicalizer.ToCanonical(location);
            var hash = AddressNormalizerTestHost.Hash.ComputeSha256(canonical);

            Assert.Equal(AddressNormalizerTestHost.Hash.ComputeSha256(canonical), hash);
            Assert.Matches(new Regex("^[0-9a-f]{64}$"), hash);
        }

        [Fact]
        public void SameInput_ProducesStableCanonicalAndHash()
        {
            const string input = "ОФИС №18С";

            var first = Canonicalize(input);
            var second = Canonicalize(input);

            Assert.Equal(first.Canonical, second.Canonical);
            Assert.Equal(first.Hash, second.Hash);
        }

        [Fact]
        public void SlashFormat_PopulatesAllSegments()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("ЭТ./ПОМЕЩ. 0/II КОМ./ОФИС 1/24");

            Assert.Equal("0", location.Floors[0]);
            Assert.Equal("II", location.Premises[0]);
            Assert.Equal("1", location.Rooms[0]);
            Assert.Equal("24", location.Offices[0]);
        }

        [Fact]
        public void Note_ExtractedSeparately()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("ЭТАЖ ЦОКОЛЬНЫЙ, ВХОД С ТОРЦА ОФИС 1");
            var canonical = AddressNormalizerTestHost.Canonicalizer.ToCanonical(location);

            Assert.Contains("вход с торца", location.Notes);
            Assert.Contains("note:вход с торца", canonical);
        }

        [Fact]
        public void NoteFacade_ExtractedSeparately()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("ЭТАЖ 2, Вход с фасада, ОФИС 5");
            var canonical = AddressNormalizerTestHost.Canonicalizer.ToCanonical(location);

            Assert.Contains("вход с фасада", location.Notes);
            Assert.Contains("note:вход с фасада", canonical);
        }

        [Theory]
        [InlineData("проезд 1")]
        [InlineData("1-й проезд")]
        [InlineData("пр-д 1")]
        [InlineData("1-й пр-д")]
        public void Passage_ParsesToPassageCategory(string input)
        {
            var location = AddressNormalizerTestHost.Parser.Parse(input);
            var canonical = AddressNormalizerTestHost.Canonicalizer.ToCanonical(location);

            Assert.Contains("1", location.Passages);
            Assert.Equal("проезд:1", canonical);
        }

        [Theory]
        [InlineData("владение 1")]
        [InlineData("влад 1")]
        [InlineData("вл. 1")]
        public void Holding_ParsesToHoldingCategory(string input)
        {
            var location = AddressNormalizerTestHost.Parser.Parse(input);
            var canonical = AddressNormalizerTestHost.Canonicalizer.ToCanonical(location);

            Assert.Contains("1", location.Holdings);
            Assert.Equal("влад:1", canonical);
        }

        [Theory]
        [InlineData("склад 1")]
        [InlineData("скл. 1")]
        public void Storage_ParsesToStorageCategory(string input)
        {
            var location = AddressNormalizerTestHost.Parser.Parse(input);
            var canonical = AddressNormalizerTestHost.Canonicalizer.ToCanonical(location);

            Assert.Contains("1", location.Storages);
            Assert.Equal("склад:1", canonical);
        }

        private static (string Canonical, string Hash) Canonicalize(string input)
        {
            var location = AddressNormalizerTestHost.Parser.Parse(input);
            var canonical = AddressNormalizerTestHost.Canonicalizer.ToCanonical(location);
            var hash = AddressNormalizerTestHost.Hash.ComputeSha256(canonical);
            return (canonical, hash);
        }
    }
}
