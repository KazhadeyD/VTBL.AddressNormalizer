using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using VTBL.AddressNormalizer.WebApi.Middleware;

namespace VTBL.AddressNormalizer.WebApi.Swagger
{
    /// <summary>
    /// Примеры request/response и описание Correlation-заголовков для Swagger UI.
    /// </summary>
    public sealed class SwaggerExamplesOperationFilter : IOperationFilter
    {
        private static readonly OpenApiSchema StringSchema = new OpenApiSchema { Type = "string" };

        /// <inheritdoc />
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation == null)
                return;

            AddCorrelationHeaders(operation);

            var path = context?.ApiDescription?.RelativePath ?? string.Empty;
            var method = context?.ApiDescription?.HttpMethod ?? string.Empty;

            if (Is(path, method, "api/v1/normalize", "POST") &&
                !path.Contains("batch", StringComparison.OrdinalIgnoreCase))
            {
                SetJsonRequest(operation, NormalizeRequestExample);
                SetJsonResponse(operation, "200", NormalizeResponseExample);
                SetJsonResponse(operation, "400", ErrorExample("source должен быть непустой строкой"));
                return;
            }

            if (Is(path, method, "api/v1/normalize/batch", "POST"))
            {
                SetJsonRequest(operation, BatchRequestExample);
                SetJsonResponse(operation, "200", BatchResponseExample);
                SetJsonResponse(operation, "400", ErrorExample("список items должен быть непустым и не превышать MaxItems"));
                SetJsonResponse(operation, "500", ErrorExample("все элементы batch завершились неуспешно"));
                return;
            }

            if (Is(path, method, "api/v1/unit/normalize", "POST"))
            {
                SetJsonRequest(operation, UnitRequestExample);
                SetJsonResponse(operation, "200", UnitResponseExample);
                SetJsonResponse(operation, "400", ErrorExample("source должен быть непустой строкой"));
                return;
            }

            if (Is(path, method, "api/v1/address/extract", "POST"))
            {
                SetJsonRequest(operation, NormalizeRequestExample);
                SetJsonResponse(operation, "200", ExtractResponseExample);
                SetJsonResponse(operation, "400", ErrorExample("source должен быть непустой строкой"));
                return;
            }

            if (Is(path, method, "api/v1/address/canonicalize", "POST"))
            {
                SetJsonRequest(operation, CanonicalizeRequestExample);
                SetJsonResponse(operation, "200", CanonicalizeResponseExample);
                SetJsonResponse(operation, "400", ErrorExample("source должен быть непустой строкой"));
                return;
            }

            if (Is(path, method, "health", "GET"))
            {
                SetJsonResponse(operation, "200", HealthResponseExample);
            }
        }

        private static bool Is(string path, string method, string expectedPath, string expectedMethod) =>
            string.Equals(method, expectedMethod, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(path.Trim('/'), expectedPath.Trim('/'), StringComparison.OrdinalIgnoreCase);

        private static void AddCorrelationHeaders(OpenApiOperation operation)
        {
            operation.Parameters ??= new List<OpenApiParameter>();

            if (!operation.Parameters.Any(p =>
                    string.Equals(p.Name, CorrelationIdResolver.RequestIdHeaderName, StringComparison.OrdinalIgnoreCase)))
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = CorrelationIdResolver.RequestIdHeaderName,
                    In = ParameterLocation.Header,
                    Required = false,
                    Schema = StringSchema,
                    Description =
                        "Идентификатор запроса VTBL. Если не передан — сервер генерирует GUID. " +
                        "Значение дублируется в response header и в NLog layout."
                });
            }
        }

        private static void SetJsonRequest(OpenApiOperation operation, IOpenApiAny example)
        {
            if (operation.RequestBody?.Content == null)
                return;

            if (operation.RequestBody.Content.TryGetValue("application/json", out var media))
                media.Example = example;
            else if (operation.RequestBody.Content.Count > 0)
                operation.RequestBody.Content.Values.First().Example = example;
        }

        private static void SetJsonResponse(OpenApiOperation operation, string statusCode, IOpenApiAny example)
        {
            if (operation.Responses == null ||
                !operation.Responses.TryGetValue(statusCode, out var response) ||
                response.Content == null)
                return;

            if (response.Content.TryGetValue("application/json", out var media))
                media.Example = example;
            else if (response.Content.Count > 0)
                response.Content.Values.First().Example = example;
            else
                response.Content["application/json"] = new OpenApiMediaType { Example = example };
        }

        private static IOpenApiAny ErrorExample(string message) => new OpenApiObject
        {
            ["error"] = new OpenApiString(message)
        };

        private static readonly IOpenApiAny NormalizeRequestExample = new OpenApiObject
        {
            ["source"] = new OpenApiString("г Москва, ул Сухонская, д 11, кв 89")
        };

        private static readonly IOpenApiAny NormalizeResponseExample = new OpenApiObject
        {
            ["source"] = new OpenApiString("г Москва, ул Сухонская, д 11, кв 89"),
            ["value"] = new OpenApiObject
            {
                ["buildingValue"] = new OpenApiObject
                {
                    ["extracted"] = new OpenApiString("г Москва, ул Сухонская, д 11"),
                    ["outdoorCanonical"] = new OpenApiString("г Москва, ул Сухонская, д 11"),
                    ["hash"] = new OpenApiString("a1b2c3d4e5f6789012345678901234567890abcdef1234567890abcdef123456"),
                    ["fiasId"] = new OpenApiNull(),
                    ["dadata"] = new OpenApiNull()
                },
                ["indoorValue"] = SampleIndoorWithApartment()
            }
        };

        private static readonly IOpenApiAny BatchRequestExample = new OpenApiObject
        {
            ["items"] = new OpenApiArray
            {
                new OpenApiObject
                {
                    ["source"] = new OpenApiString("г Москва, ул Сухонская, д 11, кв 89")
                },
                new OpenApiObject
                {
                    ["source"] = new OpenApiString("   ")
                }
            }
        };

        private static readonly IOpenApiAny BatchResponseExample = new OpenApiObject
        {
            ["items"] = new OpenApiArray
            {
                new OpenApiObject
                {
                    ["status"] = new OpenApiString("ok"),
                    ["source"] = new OpenApiString("г Москва, ул Сухонская, д 11, кв 89"),
                    ["value"] = new OpenApiObject
                    {
                        ["buildingValue"] = new OpenApiObject
                        {
                            ["extracted"] = new OpenApiString("г Москва, ул Сухонская, д 11"),
                            ["outdoorCanonical"] = new OpenApiString("г Москва, ул Сухонская, д 11"),
                            ["hash"] = new OpenApiString("a1b2c3d4e5f6789012345678901234567890abcdef1234567890abcdef123456"),
                            ["fiasId"] = new OpenApiNull(),
                            ["dadata"] = new OpenApiNull()
                        },
                        ["indoorValue"] = SampleIndoorWithApartment()
                    },
                    ["error"] = new OpenApiNull()
                },
                new OpenApiObject
                {
                    ["status"] = new OpenApiString("error"),
                    ["source"] = new OpenApiString("   "),
                    ["value"] = new OpenApiNull(),
                    ["error"] = new OpenApiString("source должен быть непустой строкой")
                }
            }
        };

        private static readonly IOpenApiAny UnitRequestExample = new OpenApiObject
        {
            ["source"] = new OpenApiString("ЭТАЖ 2, КВАРТИРА 89")
        };

        private static readonly IOpenApiAny UnitResponseExample = new OpenApiObject
        {
            ["source"] = new OpenApiString("ЭТАЖ 2, КВАРТИРА 89"),
            ["indoorValue"] = SampleIndoorFloorAndApartment(),
            ["canonical"] = new OpenApiString("эт:2|кв:89"),
            ["hash"] = new OpenApiString("b2c3d4e5f6789012345678901234567890abcdef1234567890abcdef12345678")
        };

        private static readonly IOpenApiAny ExtractResponseExample = new OpenApiObject
        {
            ["source"] = new OpenApiString("г Москва, ул Сухонская, д 11, кв 89"),
            ["extracted"] = new OpenApiString("г Москва, ул Сухонская, д 11")
        };

        private static readonly IOpenApiAny CanonicalizeRequestExample = new OpenApiObject
        {
            ["source"] = new OpenApiString("г Москва, ул Сухонская, д 11")
        };

        private static readonly IOpenApiAny CanonicalizeResponseExample = new OpenApiObject
        {
            ["source"] = new OpenApiString("г Москва, ул Сухонская, д 11"),
            ["canonical"] = new OpenApiString("г Москва, ул Сухонская, д 11")
        };

        private static readonly IOpenApiAny HealthResponseExample = new OpenApiObject
        {
            ["status"] = new OpenApiString("Healthy"),
            ["totalDurationMs"] = new OpenApiDouble(0.74),
            ["checks"] = new OpenApiObject
            {
                ["self"] = new OpenApiObject
                {
                    ["status"] = new OpenApiString("Healthy"),
                    ["description"] = new OpenApiString("HTTP host запущен."),
                    ["durationMs"] = new OpenApiDouble(0.02),
                    ["error"] = new OpenApiNull()
                },
                ["address_normalizer_readiness"] = new OpenApiObject
                {
                    ["status"] = new OpenApiString("Healthy"),
                    ["description"] = new OpenApiString("Нормализация и конфигурация доступны."),
                    ["durationMs"] = new OpenApiDouble(0.72),
                    ["error"] = new OpenApiNull()
                }
            }
        };

        private static OpenApiObject SampleIndoorWithApartment() => new OpenApiObject
        {
            ["hash"] = new OpenApiString("b2c3d4e5f6789012345678901234567890abcdef1234567890abcdef12345678"),
            ["marks"] = new OpenApiArray
            {
                Mark("apartment", "квартира", "89")
            }
        };

        private static OpenApiObject SampleIndoorFloorAndApartment() => new OpenApiObject
        {
            ["hash"] = new OpenApiString("b2c3d4e5f6789012345678901234567890abcdef1234567890abcdef12345678"),
            ["marks"] = new OpenApiArray
            {
                Mark("floor", "этаж", "2"),
                Mark("apartment", "квартира", "89")
            }
        };

        private static OpenApiObject Mark(string id, string name, params string[] values)
        {
            var arr = new OpenApiArray();
            foreach (var value in values)
                arr.Add(new OpenApiString(value));

            return new OpenApiObject
            {
                ["id"] = new OpenApiString(id),
                ["name"] = new OpenApiString(name),
                ["values"] = arr
            };
        }
    }
}
