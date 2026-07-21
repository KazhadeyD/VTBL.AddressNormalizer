using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VTBL.AddressNormalizer.WebApi;
using VTBL.AddressNormalizer.WebApi.Models;
using VTBL.AddressNormalizer.WebApi.Services;
using Xunit;

namespace VTBL.AddressNormalizer.UnitTests.WebApi
{
    /// <summary>
    /// E2E: ApiExceptionFilter → HTTP 500 + <c>{ "error" }</c>.
    /// </summary>
    public class ApiExceptionFilterTests : IClassFixture<ApiExceptionFilterTests.ThrowingServiceFixture>
    {
        private readonly HttpClient _client;

        public ApiExceptionFilterTests(ThrowingServiceFixture factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task UnhandledException_Returns500WithErrorBody()
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/normalize")
            {
                Content = new StringContent(
                    "{\"source\":\"trigger-unhandled\"}",
                    System.Text.Encoding.UTF8,
                    "application/json")
            };
            request.Headers.TryAddWithoutValidation("X-Correlation-Id", "err-corr-1");

            var response = await _client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.True(response.Headers.TryGetValues("X-Correlation-Id", out var corrValues));
            Assert.Equal("err-corr-1", corrValues.Single());

            var body = JsonSerializer.Deserialize<ErrorResponse>(json, WebApiTestFixture.JsonOptions);
            Assert.NotNull(body);
            Assert.Equal(ThrowingAddressNormalizationService.ErrorMessage, body.Error);
        }

        /// <summary>
        /// Host с сервисом, бросающим unhandled exception на NormalizeFull.
        /// </summary>
        public sealed class ThrowingServiceFixture : WebApplicationFactory<Program>
        {
            protected override void ConfigureWebHost(IWebHostBuilder builder)
            {
                builder.UseEnvironment("Production");
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<IAddressNormalizationService>();
                    services.AddSingleton<IAddressNormalizationService, ThrowingAddressNormalizationService>();
                });
            }
        }

        private sealed class ThrowingAddressNormalizationService : IAddressNormalizationService
        {
            public const string ErrorMessage = "intentional unhandled failure";

            public NormalizeValueDto NormalizeFull(string source) =>
                throw new InvalidOperationException(ErrorMessage);

            public UnitNormalizeResult NormalizeUnit(string source) =>
                throw new NotSupportedException();

            public string ExtractOutdoor(string source) =>
                throw new NotSupportedException();

            public string Canonicalize(string source) =>
                throw new NotSupportedException();

            public BatchOutcome NormalizeBatch(IReadOnlyList<string> sources, int maxItems) =>
                throw new NotSupportedException();
        }
    }
}
