using System.Text.RegularExpressions;
using VTBL.AddressNormalizer.Abstractions.Shared;
using VTBL.AddressNormalizer.UnitTests;
using VTBL.AddressNormalizer.Abstractions.BuildingUnit;
using VTBL.AddressNormalizer.Abstractions.FieldAdapters.Crm;
using Xunit;

namespace VTBL.AddressNormalizer.UnitTests.FieldAdapters.Crm
{
    public class CrmNewFlatNormalizerTests
    {
        [Theory]
        [InlineData("КВАРТИРА 837", BuildingUnitCategory.Apartment, "кв:837")]
        [InlineData("КАБИНЕТ 69", BuildingUnitCategory.Cabinet, "каб:69")]
        [InlineData("ПОДЪЕЗД 5, ОФИС 1", BuildingUnitCategory.Office, "оф:1|под:5")]
        [InlineData("ПОМЕЩ. 6 СЕКЦИЯ НОМЕР 2", BuildingUnitCategory.Premise, "пом:6|секц:2")]
        [InlineData("А/Я 165", BuildingUnitCategory.ServiceMarker, "а/я:165")]
        [InlineData("74 - 82", BuildingUnitCategory.Unknown, "диап:74-82")]
        [InlineData("210 (БЦ Речной Вокзал)", BuildingUnitCategory.Unknown, "code:210|note:бц речной вокзал")]
        [InlineData("ЭТ/ПОМ 1/40", BuildingUnitCategory.Premise, "эт:1|пом:40")]
        [InlineData("БЛОК-СЕКЦИЯ 1", BuildingUnitCategory.Unknown, "блок:1|секц:1")]
        [InlineData("ОФИС 301 ЭТАЖ 3", BuildingUnitCategory.Office, "эт:3|оф:301")]
        [InlineData("10-Н, КОМНАТА 16\\313", BuildingUnitCategory.Room, "ком:16/313|code:10-н")]
        [InlineData("703\\1", BuildingUnitCategory.Unknown, "code:703/1")]
        [InlineData("ЭТАЖ/ПОМЕЩЕНИЯ 2/297, 298", BuildingUnitCategory.Premise, "эт:2|пом:297|пом:298")]
        [InlineData("ЭТАЖ/ПОМЕЩ.-КОМ./ОФИС 5/XII-8/34", BuildingUnitCategory.Mixed, "эт:5|пом:xii|ком:8|оф:34")]
        public void Normalize_ReturnsExpectedKindAndCanonical(string input, BuildingUnitCategory expectedKind, string expectedCanonical)
        {
            var result = AddressNormalizerTestHost.CrmNewFlat.Normalize(input);

            Assert.Equal(input, result.Original);
            Assert.Equal(expectedKind, result.Category);
            Assert.Equal(expectedCanonical, result.Canonical);
        }

        [Fact]
        public void Normalize_GarbageValue_HashesUnparsedCanonical()
        {
            var result = AddressNormalizerTestHost.CrmNewFlat.Normalize("#ИМЯ?");

            Assert.Equal(BuildingUnitCategory.Garbage, result.Category);
            Assert.Equal("unparsed:#имя?", result.Canonical);
            Assert.Equal(AddressNormalizerTestHost.Hash.ComputeSha256(result.Canonical), result.Hash);
        }

        [Fact]
        public void Normalize_Hash_IsSha256OfCanonical()
        {
            var result = AddressNormalizerTestHost.CrmNewFlat.Normalize("КВАРТИРА 837");

            Assert.Equal(AddressNormalizerTestHost.Hash.ComputeSha256(result.Canonical), result.Hash);
            Assert.Matches(new Regex("^[0-9a-f]{64}$"), result.Hash);
        }

        [Fact]
        public void Normalize_Json_ContainsExtendedStructure()
        {
            var result = AddressNormalizerTestHost.CrmNewFlat.Normalize("КВАРТИРА 837");

            Assert.Contains("\"apartments\":[\"837\"]", result.Json);
            Assert.Single(result.Location.Apartments);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Normalize_EmptyInput_ReturnsEmptyCanonicalAndUnknownKind(string input)
        {
            var result = AddressNormalizerTestHost.CrmNewFlat.Normalize(input);

            Assert.Equal(BuildingUnitCategory.Unknown, result.Category);
            Assert.Equal(string.Empty, result.Canonical);
            Assert.Empty(result.Location.Unparsed);
        }
    }
}
