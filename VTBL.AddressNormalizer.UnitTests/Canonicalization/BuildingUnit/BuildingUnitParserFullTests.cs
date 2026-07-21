using System.Linq;
using VTBL.AddressNormalizer.Abstractions.BuildingUnit;
using VTBL.AddressNormalizer.UnitTests;
using Xunit;

namespace VTBL.AddressNormalizer.UnitTests.Canonicalization.BuildingUnit
{
    public class BuildingUnitParserFullTests
    {
        [Fact]
        public void Parse_PremisesString_FillsBasePremisesFields()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("ЭТ 3 ПОМ IA КОМ 35");

            Assert.Equal("3", location.Floors[0]);
            Assert.Equal("IA", location.Premises[0]);
            Assert.Equal("35", location.Rooms[0]);
        }

        [Fact]
        public void Parse_ApartmentAndRoom_ExtractsApartment()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("КВ. 10, КОМ. 3,4");

            Assert.Contains("10", location.Apartments);
            Assert.Contains("3", location.Rooms);
            Assert.Contains("4", location.RawCodes);
        }

        [Fact]
        public void Parse_ApartmentList_SplitsApartmentValues()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("КВАРТИРА 13К1, 3, 6");

            Assert.Contains("13К1", location.Apartments);
            Assert.Contains("3", location.Apartments);
            Assert.Contains("6", location.Apartments);
        }

        [Fact]
        public void Parse_Cabinet_ExtractsCabinetValue()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("КАБИНЕТ 69");

            Assert.Contains("69", location.Cabinets);
            Assert.Empty(location.Unparsed);
        }

        [Fact]
        public void Parse_EntranceAndOffice_ExtractsEntranceAndOffice()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("ПОДЪЕЗД 5, ОФИС 1");

            Assert.Contains("5", location.Entrances);
            Assert.Contains("1", location.Offices);
        }

        [Fact]
        public void Parse_BlockAndOffice_ExtractsBlock()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("БЛОК Е, ОФИС 406Е-9");

            Assert.Contains("Е", location.Blocks);
            Assert.Contains("406Е-9", location.Offices);
        }

        [Fact]
        public void Parse_Section_ExtractsSection()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("ПОМЕЩ. 6 СЕКЦИЯ НОМЕР 2");

            Assert.Contains("2", location.Sections);
            Assert.Contains("6", location.Premises);
        }

        [Fact]
        public void Parse_Mailbox_ExtractsMailbox()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("А/Я 165");

            Assert.Contains("165", location.Mailboxes);
        }

        [Fact]
        public void Parse_RangeInRawCodes_MovesToRanges()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("74 - 82");

            Assert.Contains("74-82", location.Ranges);
            Assert.Empty(location.RawCodes);
        }

        [Fact]
        public void Parse_BusinessCenterText_MovesToNotes()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("210 (БЦ Речной Вокзал)");

            Assert.Contains("210", location.RawCodes);
            Assert.Contains("БЦ Речной Вокзал", location.Notes);
        }

        [Theory]
        [InlineData("ЭТ/ПОМ 1/40", "1", "40")]
        [InlineData("ЭТАЖ/ПОМЕЩ. 11/97", "11", "97")]
        public void Parse_SlashChain_ExtractsFloorAndPremise(string input, string floor, string premise)
        {
            var location = AddressNormalizerTestHost.Parser.Parse(input);

            Assert.Contains(floor, location.Floors);
            Assert.Contains(premise, location.Premises);
        }

        [Fact]
        public void Parse_SlashChainAntresolFloorWithTextAndSecondHeaderChain_ExtractsAllSegments()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("ЭТАЖ/ПОМЕЩ. АНТРЕСОЛЬ 2/I КОМ./ОФИС 17/Е9Е");

            Assert.Contains("АНТРЕСОЛЬ 2", location.Floors);
            Assert.Contains("I", location.Premises);
            Assert.Contains("17", location.Rooms);
            Assert.Contains("Е9Е", location.Offices);
            Assert.Empty(location.RawCodes);
            Assert.Empty(location.Unparsed);
        }

        [Fact]
        public void Parse_SlashChainPremiseRoomCompound_SplitsPremiseAndRoom()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("ЭТАЖ/ПОМЕЩ.-КОМ./ОФИС 5/XII-8/34");

            Assert.Contains("5", location.Floors);
            Assert.Contains("XII", location.Premises);
            Assert.Contains("8", location.Rooms);
            Assert.Contains("34", location.Offices);
            Assert.Empty(location.RawCodes);
            Assert.Empty(location.Unparsed);
        }

        [Fact]
        public void Parse_SlashChainFloorPremisesPlural_SplitsMultiplePremises()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("ЭТАЖ/ПОМЕЩЕНИЯ 2/297, 298");

            Assert.Contains("2", location.Floors);
            Assert.Contains("297", location.Premises);
            Assert.Contains("298", location.Premises);
            Assert.Empty(location.RawCodes);
            Assert.Empty(location.Unparsed);
        }

        [Fact]
        public void Parse_SlashChainThreeTypes_ExtractsFloorPremiseRooms()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("эт/пом/ком 4/I/30,31,32");

            Assert.Contains("4", location.Floors);
            Assert.Contains("I", location.Premises);
            Assert.Contains("30", location.Rooms);
            Assert.Contains("31", location.Rooms);
            Assert.Contains("32", location.Rooms);
        }

        [Fact]
        public void Parse_CabinetWorkplaceSlashChain_ExtractsBoth()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("ЭТАЖ 2 КАБИНЕТ/РАБ.МЕСТО 5/2");

            Assert.Contains("2", location.Floors);
            Assert.Contains("5", location.Cabinets);
            Assert.Contains("2", location.Workplaces);
        }

        [Fact]
        public void Parse_PremiseCabinetSlashChain_ExtractsBoth()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("ПОМЕЩ./КАБ. 20/4");

            Assert.Contains("20", location.Premises);
            Assert.Contains("4", location.Cabinets);
        }

        [Fact]
        public void Parse_RoomWithBackslash_NormalizesToSlashInValue()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("КОМНАТА 16\\313");

            Assert.Contains("16/313", location.Rooms);
            Assert.Empty(location.Unparsed);
        }

        [Fact]
        public void Parse_BareCodeWithBackslash_NormalizesToSlashInRawCode()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("703\\1");

            Assert.Contains("703/1", location.RawCodes);
            Assert.Empty(location.Unparsed);
        }

        [Theory]
        [InlineData("10-Н, КОМНАТА 16\\313", "16/313", "10-Н")]
        [InlineData("14Н офис 300\\3", "300/3", "14Н")]
        public void Parse_BackslashInMixedStrings_ExtractsStructuredParts(
            string input,
            string structuredValue,
            string rawCode)
        {
            var location = AddressNormalizerTestHost.Parser.Parse(input);

            Assert.Contains(structuredValue, location.Rooms.Concat(location.Offices));
            Assert.Contains(rawCode, location.RawCodes);
            Assert.Empty(location.Unparsed);
        }

        [Fact]
        public void Parse_BlockSection_ExtractsBlockAndSection()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("БЛОК-СЕКЦИЯ 1");

            Assert.Contains("1", location.Blocks);
            Assert.Contains("1", location.Sections);
        }

        [Theory]
        [InlineData("ОФИС 301 ЭТАЖ 3", "301", "3")]
        [InlineData("47, этаж 2", "2", "47")]
        [InlineData("I, комн. 32 этаж 3", "3", "32")]
        public void Parse_InvertedOrder_ExtractsSegments(string input, string floorOrOffice, string other)
        {
            var location = AddressNormalizerTestHost.Parser.Parse(input);

            if (input.StartsWith("ОФИС"))
            {
                Assert.Contains(floorOrOffice, location.Offices);
                Assert.Contains(other, location.Floors);
            }
            else if (input.Contains("комн"))
            {
                Assert.Contains(other, location.Rooms);
                Assert.Contains(floorOrOffice, location.Floors);
            }
            else
            {
                Assert.Contains(floorOrOffice, location.Floors);
                Assert.Contains(other, location.RawCodes);
            }
        }

        [Theory]
        [InlineData("пом. 35-Н, Ч.П. 1 (№ 410).")]
        [InlineData("пом. 35-Н, Ч.П. 1 (№ 410)")]
        public void Parse_PremisePartWithParenthesizedCode_IgnoresTrailingPunctuation(string input)
        {
            var location = AddressNormalizerTestHost.Parser.Parse(input);
            var canonical = AddressNormalizerTestHost.Canonicalizer.ToCanonical(location);

            Assert.Equal("35-Н", location.Premises.Single());
            Assert.Equal("1", location.Parts.Single());
            Assert.Equal("410", location.RawCodes.Single());
            Assert.Empty(location.Unparsed);
            Assert.Equal("пом:35-н|ч.п:1|code:410", canonical);
        }

        [Fact]
        public void Parse_PremiseList_SplitsCommaSeparatedNumbers()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("пом. 35,38");
            var canonical = AddressNormalizerTestHost.Canonicalizer.ToCanonical(location);

            Assert.Equal(new[] { "35", "38" }, location.Premises.OrderBy(v => v).ToArray());
            Assert.Empty(location.RawCodes);
            Assert.Equal("пом:35|пом:38", canonical);
        }

        [Fact]
        public void Parse_PremiseNumericRange_ExpandsToIndividualPremises()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("пом. 35-38");
            var canonical = AddressNormalizerTestHost.Canonicalizer.ToCanonical(location);

            Assert.Equal(new[] { "35", "36", "37", "38" }, location.Premises.OrderBy(v => v).ToArray());
            Assert.Equal("пом:35|пом:36|пом:37|пом:38", canonical);
        }

        [Fact]
        public void Parse_PremiseWithLetterSuffix_DoesNotExpandAsRange()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("пом. 35-Н");

            Assert.Equal("35-Н", location.Premises.Single());
        }

        [Fact]
        public void Parse_ShortRoomWithHyphen_DoesNotExpandAsRange()
        {
            var location = AddressNormalizerTestHost.Parser.Parse("К. 5-20");
            var canonical = AddressNormalizerTestHost.Canonicalizer.ToCanonical(location);

            Assert.Equal("5-20", location.Rooms.Single());
            Assert.Equal("ком:5-20", canonical);
        }
    }
}
