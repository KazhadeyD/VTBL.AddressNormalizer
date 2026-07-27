using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace VTBL.AddressNormalizer.UnitTests.WebApi
{
    /// <summary>
    /// E2E: GET /health — финальный контракт.
    /// </summary>
    public class HealthEndpointTests : IClassFixture<WebApiTestFixture>
    {
        private readonly HttpClient _client;

        public HealthEndpointTests(WebApiTestFixture factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Health_Get_ReturnsHealthy()
        {
            var response = await _client.GetAsync("/health");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            using var json = JsonDocument.Parse(body);
            Assert.Equal("Healthy", json.RootElement.GetProperty("status").GetString());

            var checks = json.RootElement.GetProperty("checks");
            Assert.True(checks.TryGetProperty("self", out _));
            Assert.True(checks.TryGetProperty("address_normalizer_readiness", out _));
        }

        [Fact]
        public async Task HealthLive_Get_ReturnsOnlyLivenessCheck()
        {
            var response = await _client.GetAsync("/health/live");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            using var json = JsonDocument.Parse(body);
            Assert.Equal("Healthy", json.RootElement.GetProperty("status").GetString());

            var checks = json.RootElement.GetProperty("checks");
            Assert.True(checks.TryGetProperty("self", out _));
            Assert.False(checks.TryGetProperty("address_normalizer_readiness", out _));
        }
    }
}
