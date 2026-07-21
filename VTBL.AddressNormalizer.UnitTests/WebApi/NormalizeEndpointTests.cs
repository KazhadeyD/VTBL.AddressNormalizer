using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using VTBL.AddressNormalizer.WebApi.Models;
using Xunit;

namespace VTBL.AddressNormalizer.UnitTests.WebApi
{
    /// <summary>
    /// E2E: POST /api/v1/normalize через реальный host + ядро.
    /// </summary>
    public class NormalizeEndpointTests : IClassFixture<WebApiTestFixture>
    {
        private static readonly string[] IndoorCategoryPropertyNames =
        {
            nameof(IndoorValueDto.Floors),
            nameof(IndoorValueDto.Premises),
            nameof(IndoorValueDto.Rooms),
            nameof(IndoorValueDto.Offices),
            nameof(IndoorValueDto.Workplaces),
            nameof(IndoorValueDto.Parts),
            nameof(IndoorValueDto.Apartments),
            nameof(IndoorValueDto.Cabinets),
            nameof(IndoorValueDto.Entrances),
            nameof(IndoorValueDto.Blocks),
            nameof(IndoorValueDto.Sections),
            nameof(IndoorValueDto.Mailboxes),
            nameof(IndoorValueDto.Literas),
            nameof(IndoorValueDto.Ranges),
            nameof(IndoorValueDto.RawCodes),
            nameof(IndoorValueDto.Notes),
            nameof(IndoorValueDto.Unparsed)
        };

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

            Assert.NotNull(dto);
            Assert.Equal(source, dto.Source);
            Assert.NotNull(dto.Value);
            Assert.Null(dto.Value.FiasId);
            Assert.NotNull(dto.Value.DadataOutdoor);
            Assert.Equal(split.Outdoor, dto.Value.DadataOutdoor.Extracted);
            Assert.Equal(outdoorCanonical, dto.Value.DadataOutdoor.OutdoorCanonical);
            Assert.Equal(expectedHash, dto.Value.DadataOutdoor.Hash);

            AssertAll17IndoorCategoriesPresent(dto.Value.IndoorValue);
            Assert.Contains("89", dto.Value.IndoorValue.Apartments.Values);
            Assert.Equal("квартира", dto.Value.IndoorValue.Apartments.Name);
        }

        [Fact]
        public async Task Normalize_AddressWithoutIndoor_AllIndoorValuesEmpty()
        {
            var response = await WebApiTestFixture.PostJsonAsync(
                _client,
                "/api/v1/normalize",
                "{\"source\":\"г Москва, ул Сухонская, д 11\"}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var dto = JsonSerializer.Deserialize<NormalizeResponse>(body, WebApiTestFixture.JsonOptions);

            Assert.NotNull(dto?.Value?.IndoorValue);
            AssertAll17IndoorCategoriesPresent(dto.Value.IndoorValue);
            AssertAllIndoorValuesEmpty(dto.Value.IndoorValue);
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

        private static void AssertAll17IndoorCategoriesPresent(IndoorValueDto indoor)
        {
            var properties = typeof(IndoorValueDto)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.PropertyType == typeof(IndoorCategoryDto))
                .ToArray();

            Assert.Equal(17, properties.Length);
            Assert.Equal(
                IndoorCategoryPropertyNames.OrderBy(x => x),
                properties.Select(p => p.Name).OrderBy(x => x));

            foreach (var property in properties)
            {
                var category = (IndoorCategoryDto)property.GetValue(indoor);
                Assert.NotNull(category);
                Assert.False(string.IsNullOrWhiteSpace(category.Name), $"{property.Name}.Name");
                Assert.NotNull(category.Values);
            }
        }

        private static void AssertAllIndoorValuesEmpty(IndoorValueDto indoor)
        {
            var properties = typeof(IndoorValueDto)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.PropertyType == typeof(IndoorCategoryDto));

            foreach (var property in properties)
            {
                var category = (IndoorCategoryDto)property.GetValue(indoor);
                Assert.Empty(category.Values);
            }
        }
    }
}
