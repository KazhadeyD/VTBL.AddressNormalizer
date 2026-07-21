using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace VTBL.AddressNormalizer.UnitTests.WebApi
{
    /// <summary>
    /// E2E: алгоритм Correlation Id (UC-07 / F-API-07).
    /// </summary>
    public class CorrelationIdTests : IClassFixture<WebApiTestFixture>
    {
        private readonly HttpClient _client;

        public CorrelationIdTests(WebApiTestFixture factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Post_WithCorrelationId_EchoesSameValue()
        {
            using var request = CreateNormalizeRequest();
            request.Headers.TryAddWithoutValidation("X-Correlation-Id", "abc");

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("abc", GetCorrelationId(response));
        }

        [Fact]
        public async Task Post_OnlyRequestId_EchoesRequestAsCorrelation()
        {
            using var request = CreateNormalizeRequest();
            request.Headers.TryAddWithoutValidation("X-Request-Id", "req-1");

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("req-1", GetCorrelationId(response));
        }

        [Fact]
        public async Task Post_BothHeadersDifferent_PrefersCorrelation()
        {
            using var request = CreateNormalizeRequest();
            request.Headers.TryAddWithoutValidation("X-Correlation-Id", "corr-wins");
            request.Headers.TryAddWithoutValidation("X-Request-Id", "req-loses");

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("corr-wins", GetCorrelationId(response));
        }

        [Fact]
        public async Task Post_WhitespaceCorrelation_ValidRequest_UsesRequest()
        {
            using var request = CreateNormalizeRequest();
            request.Headers.TryAddWithoutValidation("X-Correlation-Id", "   ");
            request.Headers.TryAddWithoutValidation("X-Request-Id", "req-from-fallback");

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("req-from-fallback", GetCorrelationId(response));
        }

        [Fact]
        public async Task Post_WithoutHeaders_ReturnsNonEmptyGuid()
        {
            using var request = CreateNormalizeRequest();

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var id = GetCorrelationId(response);
            Assert.False(string.IsNullOrWhiteSpace(id));
            Assert.True(Guid.TryParseExact(id, "D", out _));
        }

        [Fact]
        public async Task AnyPost_ReturnsNonEmptyCorrelationIdHeader()
        {
            var response = await WebApiTestFixture.PostJsonAsync(
                _client,
                "/api/v1/normalize",
                "{\"source\":\"г Москва, ул Сухонская, д 11, кв 89\"}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(response.Headers.TryGetValues("X-Correlation-Id", out var values));
            Assert.Contains(values, v => !string.IsNullOrWhiteSpace(v));
        }

        private static HttpRequestMessage CreateNormalizeRequest()
        {
            return new HttpRequestMessage(HttpMethod.Post, "/api/v1/normalize")
            {
                Content = new StringContent(
                    "{\"source\":\"г Москва, ул Сухонская, д 11, кв 89\"}",
                    System.Text.Encoding.UTF8,
                    "application/json")
            };
        }

        private static string GetCorrelationId(HttpResponseMessage response)
        {
            Assert.True(response.Headers.TryGetValues("X-Correlation-Id", out var values));
            return values.Single();
        }
    }
}
