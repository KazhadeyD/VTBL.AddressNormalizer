using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace VTBL.AddressNormalizer.WebApi.Swagger
{
    /// <summary>
    /// Регистрация Swagger/OpenAPI для WebApi.
    /// </summary>
    public static class SwaggerServiceCollectionExtensions
    {
        private const string ApiTitle = "VTBL.AddressNormalizer.WebApi";
        private const string ApiVersion = "v1";

        private const string ApiDescription =
            "HTTP API для нормализации адресов и внутренних адресных частей.\n\n" +
            "**Авторизация:** не требуется.\n\n" +
            "**Идентификатор запроса:** можно передать заголовок `X-VTBL-Request-ID`. " +
            "Если заголовок не указан, сервис сгенерирует его автоматически. " +
            "Это значение возвращается в ответе и пишется в лог.\n\n" +
            "**Валидация:** если поле `source` отсутствует, пустое или состоит только из пробелов, " +
            "API вернёт `400 Bad Request`.\n\n" +
            "**Пакетная обработка:** ошибки отдельных элементов не прерывают обработку остальных. " +
            "Если не удалось обработать весь пакет целиком, API вернёт одну общую ошибку.";

        /// <summary>
        /// Подключает Swagger-документацию, XML-комментарии и примеры операций.
        /// </summary>
        /// <param name="services">Коллекция DI.</param>
        /// <returns>Та же коллекция для цепочки вызовов.</returns>
        public static IServiceCollection AddWebApiSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(ApiVersion, new OpenApiInfo
                {
                    Title = ApiTitle,
                    Version = ApiVersion,
                    Description = ApiDescription
                });

                var xmlPath = Path.Combine(
                    AppContext.BaseDirectory,
                    $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");

                if (File.Exists(xmlPath))
                    options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

                options.OperationFilter<SwaggerExamplesOperationFilter>();
            });

            return services;
        }
    }
}
