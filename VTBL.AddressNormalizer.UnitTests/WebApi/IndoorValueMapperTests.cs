using System.Linq;
using VTBL.AddressNormalizer.Abstractions.BuildingUnit;
using VTBL.AddressNormalizer.WebApi.Mapping;
using VTBL.AddressNormalizer.WebApi.Models;
using Xunit;

namespace VTBL.AddressNormalizer.UnitTests.WebApi
{
    /// <summary>
    /// Unit-тесты <see cref="IndoorValueMapper"/>.
    /// </summary>
    public class IndoorValueMapperTests
    {
        [Fact]
        public void ToIndoorValueDto_EmptyLocation_ReturnsEmptyUnits()
        {
            var dto = IndoorValueMapper.ToIndoorValueDto(new BuildingUnitLocation());

            Assert.NotNull(dto.Units);
            Assert.Empty(dto.Units);
        }

        [Fact]
        public void ToIndoorValueDto_WithApartmentsAndFloors_ReturnsOnlyPopulatedUnits()
        {
            var location = new BuildingUnitLocation();
            location.Apartments.Add("89");
            location.Floors.Add("2");

            var dto = IndoorValueMapper.ToIndoorValueDto(location);

            Assert.Equal(2, dto.Units.Count);

            var apartment = IndoorValueTestHelper.GetMark(dto, IndoorValueMapper.MarkIds.Apartment);
            Assert.NotNull(apartment);
            Assert.Equal(IndoorValueMapper.CategoryNames.Apartments, apartment.Name);
            Assert.Equal(new[] { "89" }, apartment.Values);

            var floor = IndoorValueTestHelper.GetMark(dto, IndoorValueMapper.MarkIds.Floor);
            Assert.NotNull(floor);
            Assert.Equal(IndoorValueMapper.CategoryNames.Floors, floor.Name);
            Assert.Equal(new[] { "2" }, floor.Values);

            Assert.Null(IndoorValueTestHelper.GetMark(dto, IndoorValueMapper.MarkIds.Premise));
        }

        [Fact]
        public void ToIndoorValueDto_NullLocation_ReturnsEmptyUnits()
        {
            var dto = IndoorValueMapper.ToIndoorValueDto(null);

            Assert.NotNull(dto.Units);
            Assert.Empty(dto.Units);
        }

        [Fact]
        public void ToIndoorValueDto_PreservesCatalogOrder()
        {
            var location = new BuildingUnitLocation();
            location.Apartments.Add("89");
            location.Floors.Add("2");

            var dto = IndoorValueMapper.ToIndoorValueDto(location);

            Assert.Equal(
                new[] { IndoorValueMapper.MarkIds.Floor, IndoorValueMapper.MarkIds.Apartment },
                dto.Units.Select(mark => mark.Id).ToArray());
        }
    }
}
