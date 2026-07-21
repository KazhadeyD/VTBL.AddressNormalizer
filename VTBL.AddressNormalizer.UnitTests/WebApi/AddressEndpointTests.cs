using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using VTBL.AddressNormalizer.Infrastructure.Composition;
using VTBL.AddressNormalizer.WebApi.Models;
using Xunit;

namespace VTBL.AddressNormalizer.UnitTests.WebApi
{
    /// <summary>
    /// E2E: POST /api/v1/address/extract и canonicalize (UC-03, UC-04).
    /// </summary>
    public class AddressEndpointTests : IClassFixture<WebApiTestFixture>
    {
        private readonly HttpClient _client;

        public AddressEndpointTests(WebApiTestFixture factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Extract_ValidSource_ReturnsExtractSplitOutdoor()
        {
            const string source = "г Москва, ул Сухонская, д 11, кв 89";
            var expected = AddressNormalizerFactory.BuildingLocationExtractor.ExtractSplit(source).Outdoor;

            var response = await WebApiTestFixture.PostJsonAsync(
                _client,
                "/api/v1/address/extract",
                "{\"source\":\"" + source + "\"}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var dto = JsonSerializer.Deserialize<ExtractResponse>(body, WebApiTestFixture.JsonOptions);

            Assert.NotNull(dto);
            Assert.Equal(expected, dto.Extracted);
        }

        [Fact]
        public async Task Extract_IndoorOnlySource_ReturnsEmptyExtracted()
        {
            const string source = "кв 10";
            var expected = AddressNormalizerFactory.BuildingLocationExtractor.ExtractSplit(source).Outdoor;

            var response = await WebApiTestFixture.PostJsonAsync(
                _client,
                "/api/v1/address/extract",
                "{\"source\":\"" + source + "\"}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var dto = JsonSerializer.Deserialize<ExtractResponse>(body, WebApiTestFixture.JsonOptions);

            Assert.Equal(string.Empty, expected);
            Assert.NotNull(dto);
            Assert.Equal(string.Empty, dto.Extracted);
        }

        [Fact]
        public async Task Canonicalize_SourceWithIndoor_ReturnsToCanonicalWithoutExtractAndWithoutHash()
        {
            const string source = "г Москва, ул Сухонская, д 11, кв 89";
            var expected = AddressNormalizerFactory.BuildingAddressCanonicalizer.ToCanonical(source);

            var response = await WebApiTestFixture.PostJsonAsync(
                _client,
                "/api/v1/address/canonicalize",
                "{\"source\":\"" + source + "\"}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var dto = JsonSerializer.Deserialize<CanonicalizeResponse>(body, WebApiTestFixture.JsonOptions);

            Assert.NotNull(dto);
            Assert.Equal(expected, dto.Canonical);

            using var doc = JsonDocument.Parse(body);
            Assert.False(doc.RootElement.TryGetProperty("hash", out _));
        }
    }
}
