using VTBL.AddressNormalizer.Abstractions.BuildingAddress;
using VTBL.AddressNormalizer.Infrastructure.Composition;
using Xunit;

namespace VTBL.AddressNormalizer.UnitTests.BuildingAddress
{
    /// <summary>
    /// Unit-тесты <see cref="IBuildingLocationExtractor.ExtractSplit"/>.
    /// </summary>
    public class BuildingLocationExtractorSplitTests
    {
        private readonly IBuildingLocationExtractor _extractor =
            AddressNormalizerFactory.BuildingLocationExtractor;

        [Theory]
        [InlineData("г Москва, ул Сухонская, д 11, кв 89", "г Москва, ул Сухонская, д 11", "кв 89")]
        [InlineData("г Москва, ул Сухонская, д 11", "г Москва, ул Сухонская, д 11", "")]
        [InlineData("кв 10", "", "кв 10")]
        public void ExtractSplit_TzSection13_ReturnsExpectedOutdoorAndIndoor(
            string input,
            string expectedOutdoor,
            string expectedIndoor)
        {
            var split = _extractor.ExtractSplit(input);

            Assert.Equal(expectedOutdoor, split.Outdoor);
            Assert.Equal(expectedIndoor, split.Indoor);
        }

        [Fact]
        public void ExtractSplit_IndoorBeforeHouse_OutdoorEmptyIndoorFromMarker()
        {
            const string input = "г Москва, кв 5, ул Сухонская, д 11";

            var split = _extractor.ExtractSplit(input);

            Assert.Equal(string.Empty, split.Outdoor);
            Assert.StartsWith("кв", split.Indoor);
            Assert.Equal("кв 5, ул Сухонская, д 11", split.Indoor);
        }

        [Fact]
        public void ExtractSplit_CommaBeforeMarker_IndoorStartsAtMarkerNotCutIndex()
        {
            const string input = "г Москва, ул Сухонская, д 11, кв 89";

            var split = _extractor.ExtractSplit(input);

            Assert.Equal("г Москва, ул Сухонская, д 11", split.Outdoor);
            Assert.Equal("кв 89", split.Indoor);
            Assert.False(split.Indoor.StartsWith(","));
            Assert.False(split.Indoor.StartsWith(" "));
        }

        [Fact]
        public void ExtractSplit_IndoorNeverNull()
        {
            Assert.NotNull(_extractor.ExtractSplit(null).Indoor);
            Assert.NotNull(_extractor.ExtractSplit("").Indoor);
            Assert.NotNull(_extractor.ExtractSplit("г Москва, ул Сухонская, д 11").Indoor);
            Assert.NotNull(_extractor.ExtractSplit("г Москва, ул Сухонская, д 11, кв 89").Indoor);
            Assert.NotNull(_extractor.ExtractSplit("кв 10").Indoor);
        }

        [Theory]
        [InlineData("г Москва, ул Сухонская, д 11, кв 89")]
        [InlineData("г Москва, ул Сухонская, д 11, офис 15")]
        [InlineData("г Москва, ул Сухонская, д 11, блок 2, кв 5")]
        [InlineData("г Москва, ул Сухонская, д 1, лит А, кв 5")]
        [InlineData("г Москва, ул Сухонская, кв 5, лит б")]
        [InlineData("п Московский, ул Ленина, д 3, кв 10")]
        [InlineData("г Москва, ул Сухонская, д 11")]
        [InlineData("г Москва ул Сухонская д 11 кв 89")]
        [InlineData("г Москва, ул Сухонская, д 11, кв 89, оф 12")]
        [InlineData("600001, Владимирская область, г. Владимир, ул. Студеная Гора, д.44 А, офис 318")]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("кв 10")]
        [InlineData("г Москва, кв 5, ул Сухонская, д 11")]
        public void Extract_EqualsExtractSplitOutdoor(string input)
        {
            Assert.Equal(_extractor.ExtractSplit(input).Outdoor, _extractor.Extract(input));
        }
    }
}
