using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using VTBL.AddressNormalizer.WebApi;

namespace VTBL.AddressNormalizer.UnitTests.WebApi
{
    /// <summary>
    /// HTTP host для E2E-тестов WebApi через <see cref="WebApplicationFactory{TEntryPoint}"/>.
    /// Environment=Production: без DeveloperExceptionPage, чтобы ApiExceptionFilter отдавал <c>{ error }</c>.
    /// </summary>
    public class WebApiTestFixture : WebApplicationFactory<Program>
    {
        /// <summary>
        /// Опции десериализации JSON ответов API (camelCase).
        /// </summary>
        public static JsonSerializerOptions JsonOptions { get; } = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        /// <inheritdoc />
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Production");
        }

        /// <summary>
        /// Отправляет JSON POST на указанный путь.
        /// </summary>
        public static Task<HttpResponseMessage> PostJsonAsync(HttpClient client, string path, string json)
        {
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return client.PostAsync(path, content);
        }
    }
}
