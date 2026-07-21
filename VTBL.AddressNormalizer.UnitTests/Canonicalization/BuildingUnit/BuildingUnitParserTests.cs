using VTBL.AddressNormalizer.Abstractions.BuildingUnit;
using VTBL.AddressNormalizer.UnitTests;
using Xunit;

namespace VTBL.AddressNormalizer.UnitTests.Canonicalization.BuildingUnit
{
    public class BuildingUnitParserTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Parse_EmptyInput_ReturnsEmptyLocation(string input)
        {
            var location = AddressNormalizerTestHost.Parser.Parse(input);

            Assert.Empty(location.Floors);
            Assert.Empty(location.Premises);
            Assert.Empty(location.Rooms);
            Assert.Empty(location.Offices);
            Assert.Empty(location.RawCodes);
        }

        [Fact]
        public void Parse_RawCodesOnly_AddsToRawCodes()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("305,307");

            Assert.Equal(new[] { "305", "307" }, location.RawCodes);
        }

        [Fact]
        public void Parse_MultiRoomSemicolon_SplitsRooms()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("ЭТ 1 ПОМ XIБ КОМ 1;2");

            Assert.Equal(new[] { "1", "2" }, location.Rooms);
        }

        [Fact]
        public void Parse_SpecialFloor_AddsCanonicalFloorName()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("ЭТАЖ 2 ПОДВАЛЬНЫЙ, ПОМ. 173");

            Assert.Contains("2", location.Floors);
            Assert.Contains("подвальный", location.Floors);
            Assert.Single(location.Premises);
            Assert.Equal("173", location.Premises[0]);
        }
    }
}
