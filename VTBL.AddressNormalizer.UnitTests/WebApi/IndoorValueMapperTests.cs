using System;
using System.Linq;
using System.Reflection;
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
        private static readonly string[] ExpectedPropertyNames =
        {
            nameof(IndoorValueDto.Floors),
            nameof(IndoorValueDto.Premises),
            nameof(IndoorValueDto.Rooms),
            nameof(IndoorValueDto.Offices),
            nameof(IndoorValueDto.Workplaces),
            nameof(IndoorValueDto.Parts),
            nameof(IndoorValueDto.Apartments),
            nameof(IndoorValueDto.Cabinets),
            nameof(IndoorValueDto.Entrances),
            nameof(IndoorValueDto.Blocks),
            nameof(IndoorValueDto.Sections),
            nameof(IndoorValueDto.Mailboxes),
            nameof(IndoorValueDto.Literas),
            nameof(IndoorValueDto.Ranges),
            nameof(IndoorValueDto.RawCodes),
            nameof(IndoorValueDto.Notes),
            nameof(IndoorValueDto.Unparsed)
        };

        [Fact]
        public void ToIndoorValueDto_EmptyLocation_ReturnsAll17CategoriesWithNamesAndEmptyValues()
        {
            var dto = IndoorValueMapper.ToIndoorValueDto(new BuildingUnitLocation());

            AssertAll17CategoriesPresent(dto);

            Assert.Equal(IndoorValueMapper.CategoryNames.Floors, dto.Floors.Name);
            Assert.Equal(IndoorValueMapper.CategoryNames.Premises, dto.Premises.Name);
            Assert.Equal(IndoorValueMapper.CategoryNames.Rooms, dto.Rooms.Name);
            Assert.Equal(IndoorValueMapper.CategoryNames.Offices, dto.Offices.Name);
            Assert.Equal(IndoorValueMapper.CategoryNames.Workplaces, dto.Workplaces.Name);
            Assert.Equal(IndoorValueMapper.CategoryNames.Parts, dto.Parts.Name);
            Assert.Equal(IndoorValueMapper.CategoryNames.Apartments, dto.Apartments.Name);
            Assert.Equal(IndoorValueMapper.CategoryNames.Cabinets, dto.Cabinets.Name);
            Assert.Equal(IndoorValueMapper.CategoryNames.Entrances, dto.Entrances.Name);
            Assert.Equal(IndoorValueMapper.CategoryNames.Blocks, dto.Blocks.Name);
            Assert.Equal(IndoorValueMapper.CategoryNames.Sections, dto.Sections.Name);
            Assert.Equal(IndoorValueMapper.CategoryNames.Mailboxes, dto.Mailboxes.Name);
            Assert.Equal(IndoorValueMapper.CategoryNames.Literas, dto.Literas.Name);
            Assert.Equal(IndoorValueMapper.CategoryNames.Ranges, dto.Ranges.Name);
            Assert.Equal(IndoorValueMapper.CategoryNames.RawCodes, dto.RawCodes.Name);
            Assert.Equal(IndoorValueMapper.CategoryNames.Notes, dto.Notes.Name);
            Assert.Equal(IndoorValueMapper.CategoryNames.Unparsed, dto.Unparsed.Name);

            Assert.Empty(dto.Apartments.Values);
            Assert.Empty(dto.Floors.Values);
        }

        [Fact]
        public void ToIndoorValueDto_WithApartments_CopiesValuesAndLeavesOtherEmpty()
        {
            var location = new BuildingUnitLocation();
            location.Apartments.Add("89");
            location.Floors.Add("2");

            var dto = IndoorValueMapper.ToIndoorValueDto(location);

            Assert.Equal(new[] { "89" }, dto.Apartments.Values);
            Assert.Equal(IndoorValueMapper.CategoryNames.Apartments, dto.Apartments.Name);
            Assert.Equal(new[] { "2" }, dto.Floors.Values);
            Assert.Empty(dto.Premises.Values);
            Assert.Empty(dto.Rooms.Values);
            Assert.Empty(dto.Offices.Values);
            Assert.Empty(dto.Unparsed.Values);
        }

        [Fact]
        public void ToIndoorValueDto_NullLocation_ReturnsEmptyCategories()
        {
            var dto = IndoorValueMapper.ToIndoorValueDto(null);

            AssertAll17CategoriesPresent(dto);
            Assert.Empty(dto.Apartments.Values);
        }

        private static void AssertAll17CategoriesPresent(IndoorValueDto dto)
        {
            var properties = typeof(IndoorValueDto)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.PropertyType == typeof(IndoorCategoryDto))
                .ToArray();

            Assert.Equal(17, properties.Length);
            Assert.Equal(ExpectedPropertyNames.OrderBy(x => x), properties.Select(p => p.Name).OrderBy(x => x));

            foreach (var property in properties)
            {
                var category = (IndoorCategoryDto)property.GetValue(dto);
                Assert.NotNull(category);
                Assert.False(string.IsNullOrWhiteSpace(category.Name), $"{property.Name}.Name");
                Assert.NotNull(category.Values);
            }
        }
    }
}
