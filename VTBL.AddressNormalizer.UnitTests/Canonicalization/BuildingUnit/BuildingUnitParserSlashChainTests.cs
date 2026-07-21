using System.Linq;
using VTBL.AddressNormalizer.Abstractions.BuildingUnit;
using VTBL.AddressNormalizer.UnitTests;
using Xunit;

namespace VTBL.AddressNormalizer.UnitTests.Canonicalization.BuildingUnit
{
    /// <summary>
    /// Тесты slash-цепочек (<see cref="BuildingUnitParser"/> / ExtractSlashChains).
    /// </summary>
    public class BuildingUnitParserSlashChainTests
    {
        [Theory]
        [InlineData("ЭТ/ПОМ 1/40", "1", "40")]
        [InlineData("ЭТАЖ/ПОМЕЩ. 11/97", "11", "97")]
        [InlineData("ЭТ./ПОМЕЩ. 1/VIII", "1", "VIII")]
        [InlineData("ЭТАЖ/КОМ. 5/118", "5", "118")]
        public void Parse_TwoHeaderChain_MapsFloorAndSecondType(
            string input,
            string floor,
            string secondValue)
        {
            var location = AddressNormalizerTestHost.Parser.Parse(input);

            Assert.Contains(floor, location.Floors);
            if (input.Contains("КОМ"))
                Assert.Contains(secondValue, location.Rooms);
            else
                Assert.Contains(secondValue, location.Premises);
        }

        [Theory]
        [InlineData("эт/пом/ком 4/I/30", "4", "I", "30")]
        [InlineData("ЭТ/ПОМ/КОМ 13/XXI/13", "13", "XXI", "13")]
        public void Parse_ThreeHeaderChain_MapsFloorPremiseAndRoom(
            string input,
            string floor,
            string premise,
            string room)
        {
            var location = AddressNormalizerTestHost.Parser.Parse(input);

            Assert.Contains(floor, location.Floors);
            Assert.Contains(premise, location.Premises);
            Assert.Contains(room, location.Rooms);
            Assert.Empty(location.RawCodes);
        }

        [Fact]
        public void Parse_FourHeaderChainWithCompoundPremiseRoom_SplitsXii8()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("ЭТАЖ/ПОМЕЩ.-КОМ./ОФИС 5/XII-8/34");

            Assert.Equal(new[] { "5" }, location.Floors);
            Assert.Equal(new[] { "XII" }, location.Premises);
            Assert.Equal(new[] { "8" }, location.Rooms);
            Assert.Equal(new[] { "34" }, location.Offices);
            Assert.Empty(location.RawCodes);
            Assert.Empty(location.Unparsed);
        }

        [Fact]
        public void Parse_MultiWordFloorValue_MapsAntresolWithNumber()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("ЭТАЖ/ПОМЕЩ. АНТРЕСОЛЬ 2/I КОМ./ОФИС 17/Е9Е");

            Assert.Contains("АНТРЕСОЛЬ 2", location.Floors);
            Assert.Contains("I", location.Premises);
            Assert.Contains("17", location.Rooms);
            Assert.Contains("Е9Е", location.Offices);
            Assert.Empty(location.RawCodes);
        }

        [Fact]
        public void Parse_SecondChainAfterComma_MapsCorpusAntresolPattern()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("ЭТАЖ/ПОМЕЩ АНТРЕСОЛЬ 2/I, КОМ./ОФИС 17/Д7Н");

            Assert.Contains("АНТРЕСОЛЬ 2", location.Floors);
            Assert.Contains("I", location.Premises);
            Assert.Contains("17", location.Rooms);
            Assert.Contains("Д7Н", location.Offices);
            Assert.Empty(location.RawCodes);
        }

        [Fact]
        public void Parse_DotSlashTwoChainsInOneString_MapsAllFourTypes()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("ЭТ./ПОМЕЩ. 0/II КОМ./ОФИС 1/24");

            Assert.Equal(new[] { "0" }, location.Floors);
            Assert.Equal(new[] { "II" }, location.Premises);
            Assert.Equal(new[] { "1" }, location.Rooms);
            Assert.Equal(new[] { "24" }, location.Offices);
            Assert.Empty(location.RawCodes);
        }

        [Fact]
        public void Parse_TextFloorValue_MapsPodvalAndSecondChain()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("ЭТАЖ/ПОМЕЩ. ПОДВАЛ/I, КОМ./ОФИС 6/151");

            Assert.Contains("ПОДВАЛ", location.Floors);
            Assert.Contains("I", location.Premises);
            Assert.Contains("6", location.Rooms);
            Assert.Contains("151", location.Offices);
        }

        [Fact]
        public void Parse_FloorOfficeChain_ExtraValuesGoToRawCodes()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("ЭТАЖ/ОФИС 3/314/5/WP");

            Assert.Equal(new[] { "3" }, location.Floors);
            Assert.Equal(new[] { "314" }, location.Offices);
            Assert.Contains("5", location.RawCodes);
            Assert.Contains("WP", location.RawCodes);
        }

        [Fact]
        public void Parse_FloorRoomChain_CommaInSecondValueSplitsRooms()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("ЭТАЖ/КОМН 7/709,710");

            Assert.Equal(new[] { "7" }, location.Floors);
            Assert.Contains("709", location.Rooms);
            Assert.Contains("710", location.Rooms);
        }

        [Fact]
        public void Parse_FloorPremisesPlural_CommaListAddsMultiplePremises()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("ЭТАЖ/ПОМЕЩЕНИЯ 2/297, 298");

            Assert.Equal(new[] { "2" }, location.Floors);
            Assert.Contains("297", location.Premises);
            Assert.Contains("298", location.Premises);
            Assert.Empty(location.RawCodes);
        }

        [Fact]
        public void Parse_ThreeHeaderChainWithSemicolonRooms_SplitsAllRooms()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("эт/пом/ком 4/I/30,31,32");

            Assert.Contains("4", location.Floors);
            Assert.Contains("I", location.Premises);
            Assert.Equal(new[] { "30", "31", "32" }, location.Rooms.OrderBy(r => r).ToArray());
        }

        [Fact]
        public void Parse_PremiseRoomOfficeWithoutFloorPrefix_MapsThreeTypes()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("АНТРЕСОЛЬ 2 ПОМ/КОМ/ОФ I/17/Е5П");

            Assert.Contains("I", location.Premises);
            Assert.Contains("17", location.Rooms);
            Assert.Contains("Е5П", location.Offices);
            Assert.Contains("2", location.RawCodes);
            Assert.Contains("АНТРЕСОЛЬ", location.RawCodes);
        }

        [Theory]
        [InlineData("ПОМЕЩ./КАБ. 20/4", "20", "4")]
        [InlineData("ЭТАЖ 2 КАБИНЕТ/РАБ.МЕСТО 5/2", "5", "2")]
        public void Parse_TwoHeaderPremiseOrCabinetChain_MapsBothValues(
            string input,
            string first,
            string second)
        {
            var location = AddressNormalizerTestHost.Parser.Parse(input);

            if (input.Contains("ПОМЕЩ"))
            {
                Assert.Contains(first, location.Premises);
                Assert.Contains(second, location.Cabinets);
            }
            else
            {
                Assert.Contains(first, location.Cabinets);
                Assert.Contains(second, location.Workplaces);
            }
        }

        [Theory]
        [InlineData("ЭТАЖ/ПОМЕЩ. АНТРЕСОЛЬ 2/I КОМ./ОФИС 17/Е9Е", "эт:антресоль 2|пом:i|ком:17|оф:е9е")]
        [InlineData("ЭТ./ПОМЕЩ. 0/II КОМ./ОФИС 1/24", "эт:0|пом:ii|ком:1|оф:24")]
        [InlineData("ЭТАЖ/ПОМЕЩ.-КОМ./ОФИС 5/XII-8/34", "эт:5|пом:xii|ком:8|оф:34")]
        [InlineData("ЭТ/ПОМ 1/40", "эт:1|пом:40")]
        public void Parse_SlashChainCases_ProduceExpectedCanonical(string input, string expectedCanonical)
        {
            var location = AddressNormalizerTestHost.Parser.Parse(input);
            var canonical = AddressNormalizerTestHost.Canonicalizer.ToCanonical(location);

            Assert.Equal(expectedCanonical, canonical);
        }
    }
}
