using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using VTBL.AddressNormalizer.WebApi.Mapping;
using VTBL.AddressNormalizer.WebApi.Models;
using Xunit;

namespace VTBL.AddressNormalizer.UnitTests.WebApi
{
    /// <summary>
    /// E2E: POST /api/v1/normalize через реальный host + ядро.
    /// </summary>
    public class NormalizeEndpointTests : IClassFixture<WebApiTestFixture>
    {
        private readonly HttpClient _client;

        public NormalizeEndpointTests(WebApiTestFixture factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Normalize_FullAddressWithApartment_ReturnsCoreOutdoorAndIndoor89()
        {
            const string source = "г Москва, ул Сухонская, д 11, кв 89";

            var response = await WebApiTestFixture.PostJsonAsync(
                _client,
                "/api/v1/normalize",
                "{\"source\":\"" + source + "\"}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var dto = JsonSerializer.Deserialize<NormalizeResponse>(body, WebApiTestFixture.JsonOptions);

            var split = AddressNormalizerTestHost.BuildingLocationExtractor.ExtractSplit(source);
            var outdoorCanonical = AddressNormalizerTestHost.BuildingAddressCanonicalizer.ToCanonical(split.Outdoor);
            var expectedHash = AddressNormalizerTestHost.Hash.ComputeSha256(outdoorCanonical);
            var unitLocation = AddressNormalizerTestHost.Parser.Parse(split.Indoor);
            var unitCanonical = AddressNormalizerTestHost.Canonicalizer.ToCanonical(unitLocation);
            var unitHash = AddressNormalizerTestHost.Hash.ComputeSha256(unitCanonical);

            Assert.NotNull(dto);
            Assert.Equal(source, dto.Source);
            Assert.NotNull(dto.Value);
            Assert.Null(dto.Value.BuildingValue.FiasId);
            Assert.Null(dto.Value.BuildingValue.Dadata);
            Assert.NotNull(dto.Value.BuildingValue);
            Assert.Equal(split.Outdoor, dto.Value.BuildingValue.Extracted);
            Assert.Equal(outdoorCanonical, dto.Value.BuildingValue.NormalizedAddress);
            Assert.Equal(expectedHash, dto.Value.BuildingValue.Hash);

            Assert.Equal(unitHash, dto.Value.IndoorValue.Hash);
            var apartment = IndoorValueTestHelper.GetMark(dto.Value.IndoorValue, IndoorValueMapper.MarkIds.Apartment);
            Assert.NotNull(apartment);
            Assert.Contains("89", apartment.Values);
            Assert.Equal("квартира", apartment.Name);
        }

        [Fact]
        public async Task Normalize_AddressWithoutIndoor_ReturnsEmptyMarks()
        {
            var response = await WebApiTestFixture.PostJsonAsync(
                _client,
                "/api/v1/normalize",
                "{\"source\":\"г Москва, ул Сухонская, д 11\"}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var dto = JsonSerializer.Deserialize<NormalizeResponse>(body, WebApiTestFixture.JsonOptions);

            Assert.NotNull(dto?.Value?.IndoorValue);
            Assert.NotNull(dto.Value.IndoorValue.Marks);
            Assert.Empty(dto.Value.IndoorValue.Marks);
        }

        [Fact]
        public async Task Normalize_WhitespaceSource_Returns400WithError()
        {
            var response = await WebApiTestFixture.PostJsonAsync(
                _client,
                "/api/v1/normalize",
                "{\"source\":\"   \"}");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);
            Assert.True(doc.RootElement.TryGetProperty("error", out var error));
            Assert.False(string.IsNullOrWhiteSpace(error.GetString()));
        }
    }
}
