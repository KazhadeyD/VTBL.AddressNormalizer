using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using VTBL.AddressNormalizer.WebApi.Models;
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
            var dto = JsonSerializer.Deserialize<HealthResponse>(body, WebApiTestFixture.JsonOptions);

            Assert.NotNull(dto);
            Assert.Equal("Healthy", dto.Status);
        }
    }
}
