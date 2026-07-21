using VTBL.AddressNormalizer.Abstractions.BuildingUnit;
using VTBL.AddressNormalizer.UnitTests;
using VTBL.AddressNormalizer.Abstractions.FieldAdapters.Crm;
using Xunit;

namespace VTBL.AddressNormalizer.UnitTests.FieldAdapters.Crm
{
    public class CrmNewFlatClassifierTests
    {
        [Theory]
        [InlineData("ЭТАЖ 4 ПОМЕЩЕНИЕ 2", BuildingUnitCategory.Premise)]
        [InlineData("ОФИС №18С", BuildingUnitCategory.Office)]
        [InlineData("К. 5-20", BuildingUnitCategory.Room)]
        [InlineData("ЭТАЖ 23", BuildingUnitCategory.Floor)]
        [InlineData("КАБИНЕТ 69", BuildingUnitCategory.Cabinet)]
        [InlineData("КВАРТИРА 837", BuildingUnitCategory.Apartment)]
        [InlineData("ПОДЪЕЗД 5", BuildingUnitCategory.ServiceMarker)]
        [InlineData("#ИМЯ?", BuildingUnitCategory.Garbage)]
        [InlineData("06.10.2007", BuildingUnitCategory.Garbage)]
        [InlineData("210", BuildingUnitCategory.Unknown)]
        public void Classify_ReturnsExpectedKind(string input, BuildingUnitCategory expectedKind)
        {
            var actual = AddressNormalizerTestHost.Classifier.Classify(input);

            Assert.Equal(expectedKind, actual);
        }

        [Theory]
        [InlineData("ЭТАЖ 9 ПОМЕЩ. XVI КОМ. 2 ОФИС 64")]
        [InlineData("ЭТ/ПОМ/КОМ 13/XXI/13, РАБ.МЕСТО 1")]
        public void Classify_MultiMarkerString_ReturnsMixed(string input)
        {
            var actual = AddressNormalizerTestHost.Classifier.Classify(input);

            Assert.Equal(BuildingUnitCategory.Mixed, actual);
        }

        [Theory]
        [InlineData("КВ. 10, КОМ. 3,4", BuildingUnitCategory.Room)]
        [InlineData("ПОДЪЕЗД 5, ОФИС 1", BuildingUnitCategory.Office)]
        public void Classify_TwoMarkerString_ReturnsDominantKind(string input, BuildingUnitCategory expectedKind)
        {
            var actual = AddressNormalizerTestHost.Classifier.Classify(input);

            Assert.Equal(expectedKind, actual);
        }

        [Fact]
        public void Classify_QuotedOfficeValue_ReturnsOffice()
        {
            var actual = AddressNormalizerTestHost.Classifier.Classify("\"ОФ. 302 БЦ \"\"К2\"\"\"");

            Assert.Equal(BuildingUnitCategory.Office, actual);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Classify_EmptyInput_ReturnsUnknown(string input)
        {
            var actual = AddressNormalizerTestHost.Classifier.Classify(input);

            Assert.Equal(BuildingUnitCategory.Unknown, actual);
        }
    }
}
