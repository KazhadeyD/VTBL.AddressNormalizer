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
    /// E2E: POST /api/v1/unit/normalize (UC-02) через реальный host + ядро.
    /// </summary>
    public class UnitEndpointTests : IClassFixture<WebApiTestFixture>
    {
        private readonly HttpClient _client;

        public UnitEndpointTests(WebApiTestFixture factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task UnitNormalize_Apartment_MatchesFactoryCanonicalAndHash()
        {
            const string source = "кв 89";

            var response = await WebApiTestFixture.PostJsonAsync(
                _client,
                "/api/v1/unit/normalize",
                "{\"source\":\"" + source + "\"}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var dto = JsonSerializer.Deserialize<UnitNormalizeResponse>(body, WebApiTestFixture.JsonOptions);
            var expected = AddressNormalizerFactory.BuildingUnitNormalizer.Normalize(source);

            Assert.NotNull(dto);
            Assert.Equal(source, dto.Source);
            Assert.Equal(expected.Canonical, dto.Canonical);
            Assert.Equal(expected.Hash, dto.Hash);
            Assert.Contains("89", dto.IndoorValue.Apartments.Values);

            using var doc = JsonDocument.Parse(body);
            Assert.False(doc.RootElement.TryGetProperty("category", out _));
        }
    }
}
