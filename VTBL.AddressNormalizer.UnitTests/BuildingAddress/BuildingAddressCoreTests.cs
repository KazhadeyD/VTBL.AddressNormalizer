using VTBL.AddressNormalizer.Abstractions.BuildingAddress;
using VTBL.AddressNormalizer.Abstractions.Shared;
using VTBL.AddressNormalizer.Infrastructure.BuildingAddress;
using VTBL.AddressNormalizer.Infrastructure.Composition;
using Xunit;

namespace VTBL.AddressNormalizer.UnitTests.BuildingAddress
{
    public class BuildingLocationExtractorTests
    {
        private readonly IBuildingLocationExtractor _extractor =
            AddressNormalizerFactory.BuildingLocationExtractor;

        [Theory]
        [InlineData("г Москва, ул Сухонская, д 11, кв 89", "г Москва, ул Сухонская, д 11")]
        [InlineData("г Москва, ул Сухонская, д 11, офис 15", "г Москва, ул Сухонская, д 11")]
        [InlineData("г Москва, ул Сухонская, д 11, блок 2, кв 5", "г Москва, ул Сухонская, д 11")]
        [InlineData("г Москва, ул Сухонская, д 1, лит А, кв 5", "г Москва, ул Сухонская, д 1, лит А")]
        [InlineData("г Москва, ул Сухонская, кв 5, лит б", "г Москва, ул Сухонская")]
        [InlineData("п Московский, ул Ленина, д 3, кв 10", "п Московский, ул Ленина, д 3")]
        [InlineData("г Москва, ул Сухонская, д 11", "г Москва, ул Сухонская, д 11")]
        [InlineData("г Москва ул Сухонская д 11 кв 89", "г Москва ул Сухонская д 11")]
        [InlineData("г Москва, ул Сухонская, д 11, кв 89, оф 12", "г Москва, ул Сухонская, д 11")]
        public void Extract_ReturnsExpected(string input, string expected)
        {
            Assert.Equal(expected, _extractor.Extract(input));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("кв 10")]
        [InlineData("г Москва, кв 5, ул Сухонская, д 11")]
        public void Extract_ReturnsEmpty(string input)
        {
            Assert.Equal(string.Empty, _extractor.Extract(input));
        }

        [Fact]
        public void Extract_DoesNotCutWhenMarkerInsideWord()
        {
            // «КВАРТИРНЫЙ» содержит «КВ» без границы слова как отдельный маркер кв — паттерн КВ(?!\p{L})
            // не матчит внутри КВАРТИРНЫЙ; «квартирный» не должен триггерить Apartment как cut mid-word wrongly
            var input = "г Москва, ул Квартирная, д 11";
            Assert.Equal("г Москва, ул Квартирная, д 11", _extractor.Extract(input));
        }
    }

    public class BuildingAddressCanonicalizerTests
    {
        private readonly IBuildingAddressCanonicalizer _canonicalizer =
            AddressNormalizerFactory.BuildingAddressCanonicalizer;

        [Theory]
        [InlineData("Г. МОСКВА, УЛ. Сухонская, Д. 11", "г Москва, ул Сухонская, д 11")]
        [InlineData("город Москва, улица Сухонская, дом 11", "г Москва, ул Сухонская, д 11")]
        [InlineData("г Москва, ул. Сухонская, д. 11, корп. 2, стр. 3", "г Москва, ул Сухонская, д 11, корп 2, стр 3")]
        [InlineData("пр-кт Невский, д 1", "пр-кт Невский, д 1")]
        [InlineData("Москва Сухонская 11", "Москва Сухонская 11")]
        [InlineData("", "")]
        [InlineData(null, "")]
        public void ToCanonical_ReturnsExpected(string input, string expected)
        {
            Assert.Equal(expected, _canonicalizer.ToCanonical(input));
        }
    }

    public class BuildingAddressNormalizerTests
    {
        private readonly IBuildingAddressNormalizer _normalizer =
            AddressNormalizerFactory.BuildingAddressNormalizer;

        [Fact]
        public void Normalize_MainScenario_ExtractAndCanonical()
        {
            var result = _normalizer.Normalize("г Москва, ул Сухонская, д 11, кв 89");

            Assert.Equal("г Москва, ул Сухонская, д 11, кв 89", result.Original);
            Assert.Equal("г Москва, ул Сухонская, д 11", result.Extracted);
            Assert.Equal("г Москва, ул Сухонская, д 11", result.Canonical);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("кв 10")]
        public void Normalize_EmptyCases(string input)
        {
            var result = _normalizer.Normalize(input);
            Assert.Equal(string.Empty, result.Extracted);
            Assert.Equal(string.Empty, result.Canonical);
        }

        [Fact]
        public void Normalize_UpperCaseWithIndoor_Canonicalizes()
        {
            var result = _normalizer.Normalize("Г. МОСКВА, УЛ. Сухонская, Д. 11, КВ. 89");
            Assert.Equal("г Москва, ул Сухонская, д 11", result.Canonical);
        }

        [Fact]
        public void Normalize_IsDeterministic()
        {
            const string input = "г Санкт-Петербург, Невский пр-кт, д 1, корп 2, стр 3, оф 15";
            var first = _normalizer.Normalize(input);
            var second = _normalizer.Normalize(input);
            Assert.Equal(first.Canonical, second.Canonical);
            Assert.Equal("г Санкт-Петербург, Невский пр-кт, д 1, корп 2, стр 3", first.Canonical);
        }

        [Fact]
        public void NormalizeToCanonical_Extension_Works()
        {
            var canonical = _normalizer.NormalizeToCanonical(
                "г Санкт-Петербург, Невский пр-кт, д 1, корп 2, стр 3, оф 15");
            Assert.Equal("г Санкт-Петербург, Невский пр-кт, д 1, корп 2, стр 3", canonical);
        }
    }

    public class IndoorMarkerRegistryTests
    {
        [Fact]
        public void FindFirstMatch_ReturnsLeftmost()
        {
            var match = IndoorMarkerRegistry.FindFirstMatch("кв 1 оф 2");
            Assert.NotNull(match);
            Assert.Equal(0, match.Index);
            Assert.Equal(IndoorMarkerKind.Apartment, match.Kind);
        }

        [Fact]
        public void FindFirstMatch_DoesNotMatchPoselokAsPremise()
        {
            var match = IndoorMarkerRegistry.FindFirstMatch("п Московский");
            Assert.Null(match);
        }
    }
}
