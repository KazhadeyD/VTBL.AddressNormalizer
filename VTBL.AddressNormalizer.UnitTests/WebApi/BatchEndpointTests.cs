using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Abstractions;
using VTBL.AddressNormalizer.Infrastructure.Composition;
using VTBL.AddressNormalizer.WebApi;
using VTBL.AddressNormalizer.WebApi.Models;
using VTBL.AddressNormalizer.WebApi.Services;
using Xunit;

namespace VTBL.AddressNormalizer.UnitTests.WebApi
{
    /// <summary>
    /// E2E: POST /api/v1/normalize/batch.
    /// </summary>
    public class BatchEndpointTests : IClassFixture<WebApiTestFixture>
    {
        private const string SourceA = "г Москва, ул Сухонская, д 11, кв 89";
        private const string SourceB = "г Москва, ул Сухонская, д 11";

        private readonly HttpClient _client;

        public BatchEndpointTests(WebApiTestFixture factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Batch_TwoValidItems_Returns200WithOkValuesInOrder()
        {
            var response = await WebApiTestFixture.PostJsonAsync(
                _client,
                "/api/v1/normalize/batch",
                "{\"items\":[{\"source\":\"" + SourceA + "\"},{\"source\":\"" + SourceB + "\"}]}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var dto = JsonSerializer.Deserialize<BatchNormalizeResponse>(body, WebApiTestFixture.JsonOptions);

            Assert.NotNull(dto);
            Assert.NotNull(dto.Items);
            Assert.Equal(2, dto.Items.Count);
            Assert.Equal("ok", dto.Items[0].Status);
            Assert.Equal("ok", dto.Items[1].Status);
            Assert.Equal(SourceA, dto.Items[0].Source);
            Assert.Equal(SourceB, dto.Items[1].Source);

            AssertOkValueMatchesCore(SourceA, dto.Items[0].Value);
            AssertOkValueMatchesCore(SourceB, dto.Items[1].Value);
            Assert.Contains("89", dto.Items[0].Value.IndoorValue.Apartments.Values);
            Assert.Empty(dto.Items[1].Value.IndoorValue.Apartments.Values);
        }

        [Fact]
        public async Task Batch_ValidAndWhitespace_Returns200WithOkAndError()
        {
            var response = await WebApiTestFixture.PostJsonAsync(
                _client,
                "/api/v1/normalize/batch",
                "{\"items\":[{\"source\":\"" + SourceA + "\"},{\"source\":\"   \"}]}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var dto = JsonSerializer.Deserialize<BatchNormalizeResponse>(body, WebApiTestFixture.JsonOptions);

            Assert.NotNull(dto?.Items);
            Assert.Equal(2, dto.Items.Count);
            Assert.Equal("ok", dto.Items[0].Status);
            Assert.Equal("error", dto.Items[1].Status);
            Assert.Equal("   ", dto.Items[1].Source);
            Assert.False(string.IsNullOrWhiteSpace(dto.Items[1].Error));
            AssertOkValueMatchesCore(SourceA, dto.Items[0].Value);
        }

        [Fact]
        public async Task Batch_AllWhitespace_Returns400WithoutItems()
        {
            var response = await WebApiTestFixture.PostJsonAsync(
                _client,
                "/api/v1/normalize/batch",
                "{\"items\":[{\"source\":\"\"},{\"source\":\"  \"}]}");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            await AssertErrorBodyWithoutItemsAsync(response);
        }

        [Theory]
        [InlineData("{\"items\":[]}")]
        [InlineData("null")]
        [InlineData("{}")]
        public async Task Batch_EmptyOrNullItems_Returns400WithoutItems(string json)
        {
            var response = await WebApiTestFixture.PostJsonAsync(
                _client,
                "/api/v1/normalize/batch",
                json);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            await AssertErrorBodyWithoutItemsAsync(response);
        }

        [Fact]
        public async Task Batch_OverMaxItems_Returns400WithoutItems()
        {
            var sb = new StringBuilder("{\"items\":[");
            for (var i = 0; i < 101; i++)
            {
                if (i > 0)
                    sb.Append(',');
                sb.Append("{\"source\":\"addr ").Append(i).Append("\"}");
            }
            sb.Append("]}");

            var response = await WebApiTestFixture.PostJsonAsync(
                _client,
                "/api/v1/normalize/batch",
                sb.ToString());

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            await AssertErrorBodyWithoutItemsAsync(response);
        }

        private static void AssertOkValueMatchesCore(string source, NormalizeValueDto value)
        {
            var split = AddressNormalizerFactory.BuildingLocationExtractor.ExtractSplit(source);
            var outdoorCanonical = AddressNormalizerFactory.BuildingAddressCanonicalizer.ToCanonical(split.Outdoor);
            var expectedHash = AddressNormalizerFactory.CanonicalHash.ComputeSha256(outdoorCanonical);

            Assert.NotNull(value);
            Assert.Null(value.FiasId);
            Assert.Equal(split.Outdoor, value.DadataOutdoor.Extracted);
            Assert.Equal(outdoorCanonical, value.DadataOutdoor.OutdoorCanonical);
            Assert.Equal(expectedHash, value.DadataOutdoor.Hash);
        }

        private static async Task AssertErrorBodyWithoutItemsAsync(HttpResponseMessage response)
        {
            var body = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);
            Assert.True(doc.RootElement.TryGetProperty("error", out var error));
            Assert.False(string.IsNullOrWhiteSpace(error.GetString()));
            Assert.False(doc.RootElement.TryGetProperty("items", out _));
        }
    }

    /// <summary>
    /// E2E all-fail exception/mixed через seam NormalizeFullCore (без мока ядра).
    /// </summary>
    public class BatchAllFailEndpointTests : IClassFixture<BatchAllFailEndpointTests.ThrowingCoreFixture>
    {
        private readonly HttpClient _client;

        public BatchAllFailEndpointTests(ThrowingCoreFixture factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Batch_AllExceptions_Returns500WithoutItems()
        {
            var response = await WebApiTestFixture.PostJsonAsync(
                _client,
                "/api/v1/normalize/batch",
                "{\"items\":[{\"source\":\"addr-a\"},{\"source\":\"addr-b\"}]}");

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            await AssertErrorWithoutItemsAsync(response);
        }

        [Fact]
        public async Task Batch_MixedAllFail_Returns500WithoutItems()
        {
            var response = await WebApiTestFixture.PostJsonAsync(
                _client,
                "/api/v1/normalize/batch",
                "{\"items\":[{\"source\":\"addr-a\"},{\"source\":\"   \"}]}");

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            await AssertErrorWithoutItemsAsync(response);
        }

        private static async Task AssertErrorWithoutItemsAsync(HttpResponseMessage response)
        {
            var body = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);
            Assert.True(doc.RootElement.TryGetProperty("error", out var error));
            Assert.False(string.IsNullOrWhiteSpace(error.GetString()));
            Assert.False(doc.RootElement.TryGetProperty("items", out _));
        }

        /// <summary>
        /// Host с сервисом, у которого NormalizeFullCore бросает исключение.
        /// </summary>
        public sealed class ThrowingCoreFixture : WebApplicationFactory<Program>
        {
            protected override void ConfigureWebHost(IWebHostBuilder builder)
            {
                builder.UseEnvironment("Production");
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<IAddressNormalizationService>();
                    services.AddSingleton<IAddressNormalizationService>(
                        new ThrowingCoreAddressNormalizationService(
                            NullLogger<AddressNormalizationService>.Instance,
                            AddressNormalizerFactory.BuildingLocationExtractor,
                            AddressNormalizerFactory.BuildingAddressCanonicalizer,
                            AddressNormalizerFactory.BuildingUnitNormalizer,
                            AddressNormalizerFactory.CanonicalHash));
                });
            }
        }

        private sealed class ThrowingCoreAddressNormalizationService : AddressNormalizationService
        {
            public ThrowingCoreAddressNormalizationService(
                Microsoft.Extensions.Logging.ILogger<AddressNormalizationService> logger,
                VTBL.AddressNormalizer.Abstractions.BuildingAddress.IBuildingLocationExtractor locationExtractor,
                VTBL.AddressNormalizer.Abstractions.BuildingAddress.IBuildingAddressCanonicalizer addressCanonicalizer,
                VTBL.AddressNormalizer.Abstractions.BuildingUnit.IBuildingUnitNormalizer unitNormalizer,
                VTBL.AddressNormalizer.Abstractions.Shared.ICanonicalHash canonicalHash)
                : base(logger, locationExtractor, addressCanonicalizer, unitNormalizer, canonicalHash)
            {
            }

            protected override NormalizeValueDto NormalizeFullCore(string source) =>
                throw new InvalidOperationException("intentional batch core failure");
        }
    }
}
