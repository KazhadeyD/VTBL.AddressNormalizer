using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using VTBL.AddressNormalizer.WebApi.Middleware;
using Xunit;

namespace VTBL.AddressNormalizer.UnitTests.WebApi
{
    /// <summary>
    /// E2E: алгоритм Correlation Id.
    /// </summary>
    public class CorrelationIdTests : IClassFixture<WebApiTestFixture>
    {
        private readonly HttpClient _client;

        public CorrelationIdTests(WebApiTestFixture factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Post_WithRequestId_EchoesSameValue()
        {
            using var request = CreateNormalizeRequest();
            request.Headers.TryAddWithoutValidation(CorrelationIdResolver.RequestIdHeaderName, "abc");

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("abc", GetRequestId(response));
        }

        [Fact]
        public async Task Post_WhitespaceRequestId_ReturnsNonEmptyGuid()
        {
            using var request = CreateNormalizeRequest();
            request.Headers.TryAddWithoutValidation(CorrelationIdResolver.RequestIdHeaderName, "   ");

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var id = GetRequestId(response);
            Assert.True(Guid.TryParseExact(id, "D", out _));
        }

        [Fact]
        public async Task Post_WithoutHeaders_ReturnsNonEmptyGuid()
        {
            using var request = CreateNormalizeRequest();

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var id = GetRequestId(response);
            Assert.False(string.IsNullOrWhiteSpace(id));
            Assert.True(Guid.TryParseExact(id, "D", out _));
        }

        [Fact]
        public async Task AnyPost_ReturnsNonEmptyRequestIdHeader()
        {
            var response = await WebApiTestFixture.PostJsonAsync(
                _client,
                "/api/v1/normalize",
                "{\"source\":\"г Москва, ул Сухонская, д 11, кв 89\"}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(response.Headers.TryGetValues(CorrelationIdResolver.RequestIdHeaderName, out var values));
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

        private static string GetRequestId(HttpResponseMessage response)
        {
            Assert.True(response.Headers.TryGetValues(CorrelationIdResolver.RequestIdHeaderName, out var values));
            return values.Single();
        }
    }
}
