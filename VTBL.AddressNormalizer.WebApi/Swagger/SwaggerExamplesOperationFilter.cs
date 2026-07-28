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
                    ["normalizedAddress"] = new OpenApiString("г Москва, ул Сухонская, д 11"),
                    ["hash"] = new OpenApiString("a1b2c3d4e5f6789012345678901234567890abcdef1234567890abcdef123456"),
                    ["fiasId"] = new OpenApiNull(),
                    ["dadata"] = new OpenApiObject
                    {
                        ["suggest"] = SuggestStub("г Москва, ул Сухонская, д 11"),
                        ["clean"] = CleanStub("г Москва, ул Сухонская, д 11")
                    }
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
                            ["normalizedAddress"] = new OpenApiString("г Москва, ул Сухонская, д 11"),
                            ["hash"] = new OpenApiString("a1b2c3d4e5f6789012345678901234567890abcdef1234567890abcdef123456"),
                            ["fiasId"] = new OpenApiNull(),
                            ["dadata"] = new OpenApiObject
                            {
                                ["suggest"] = SuggestStub("г Москва, ул Сухонская, д 11"),
                                ["clean"] = CleanStub("г Москва, ул Сухонская, д 11")
                            }
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

        private static OpenApiObject SuggestStub(string address) => new OpenApiObject
        {
            ["suggestions"] = new OpenApiArray
            {
                new OpenApiObject
                {
                    ["value"] = new OpenApiString(address),
                    ["unrestricted_value"] = new OpenApiString(address),
                    ["data"] = AddressDataStub(address, address)
                }
            }
        };

        private static OpenApiObject CleanStub(string address) => AddressDataStub(address, address);

        private static OpenApiObject AddressDataStub(string source, string result) => new OpenApiObject
        {
            ["source"] = new OpenApiString(source),
            ["result"] = new OpenApiString(result),
            ["postal_code"] = new OpenApiNull(),
            ["country"] = new OpenApiString("Россия"),
            ["country_iso_code"] = new OpenApiString("RU"),
            ["federal_district"] = new OpenApiNull(),
            ["region_fias_id"] = new OpenApiNull(),
            ["region_kladr_id"] = new OpenApiNull(),
            ["region_iso_code"] = new OpenApiNull(),
            ["region_with_type"] = new OpenApiNull(),
            ["region_type"] = new OpenApiNull(),
            ["region_type_full"] = new OpenApiNull(),
            ["region"] = new OpenApiNull(),
            ["area_fias_id"] = new OpenApiNull(),
            ["area_kladr_id"] = new OpenApiNull(),
            ["area_with_type"] = new OpenApiNull(),
            ["area_type"] = new OpenApiNull(),
            ["area_type_full"] = new OpenApiNull(),
            ["area"] = new OpenApiNull(),
            ["city_fias_id"] = new OpenApiNull(),
            ["city_kladr_id"] = new OpenApiNull(),
            ["city_with_type"] = new OpenApiNull(),
            ["city_type"] = new OpenApiNull(),
            ["city_type_full"] = new OpenApiNull(),
            ["city"] = new OpenApiNull(),
            ["city_area"] = new OpenApiNull(),
            ["city_district_fias_id"] = new OpenApiNull(),
            ["city_district_kladr_id"] = new OpenApiNull(),
            ["city_district_with_type"] = new OpenApiNull(),
            ["city_district_type"] = new OpenApiNull(),
            ["city_district_type_full"] = new OpenApiNull(),
            ["city_district"] = new OpenApiNull(),
            ["settlement_fias_id"] = new OpenApiNull(),
            ["settlement_kladr_id"] = new OpenApiNull(),
            ["settlement_with_type"] = new OpenApiNull(),
            ["settlement_type"] = new OpenApiNull(),
            ["settlement_type_full"] = new OpenApiNull(),
            ["settlement"] = new OpenApiNull(),
            ["street_fias_id"] = new OpenApiNull(),
            ["street_kladr_id"] = new OpenApiNull(),
            ["street_with_type"] = new OpenApiNull(),
            ["street_type"] = new OpenApiNull(),
            ["street_type_full"] = new OpenApiNull(),
            ["street"] = new OpenApiNull(),
            ["stead_fias_id"] = new OpenApiNull(),
            ["stead_kladr_id"] = new OpenApiNull(),
            ["stead_cadnum"] = new OpenApiNull(),
            ["stead_type"] = new OpenApiNull(),
            ["stead_type_full"] = new OpenApiNull(),
            ["stead"] = new OpenApiNull(),
            ["house_fias_id"] = new OpenApiNull(),
            ["house_kladr_id"] = new OpenApiNull(),
            ["house_cadnum"] = new OpenApiNull(),
            ["house_flat_count"] = new OpenApiNull(),
            ["house_type"] = new OpenApiNull(),
            ["house_type_full"] = new OpenApiNull(),
            ["house"] = new OpenApiNull(),
            ["block_type"] = new OpenApiNull(),
            ["block_type_full"] = new OpenApiNull(),
            ["block"] = new OpenApiNull(),
            ["entrance"] = new OpenApiNull(),
            ["floor"] = new OpenApiNull(),
            ["flat_fias_id"] = new OpenApiNull(),
            ["flat_cadnum"] = new OpenApiNull(),
            ["flat_type"] = new OpenApiNull(),
            ["flat_type_full"] = new OpenApiNull(),
            ["flat"] = new OpenApiNull(),
            ["flat_area"] = new OpenApiNull(),
            ["square_meter_price"] = new OpenApiNull(),
            ["flat_price"] = new OpenApiNull(),
            ["postal_box"] = new OpenApiNull(),
            ["room_fias_id"] = new OpenApiNull(),
            ["room_cadnum"] = new OpenApiNull(),
            ["room_type"] = new OpenApiNull(),
            ["room_type_full"] = new OpenApiNull(),
            ["room"] = new OpenApiNull(),
            ["fias_id"] = new OpenApiNull(),
            ["fias_code"] = new OpenApiNull(),
            ["fias_level"] = new OpenApiNull(),
            ["fias_actuality_state"] = new OpenApiNull(),
            ["kladr_id"] = new OpenApiNull(),
            ["geoname_id"] = new OpenApiNull(),
            ["capital_marker"] = new OpenApiNull(),
            ["okato"] = new OpenApiNull(),
            ["oktmo"] = new OpenApiNull(),
            ["tax_office"] = new OpenApiNull(),
            ["tax_office_legal"] = new OpenApiNull(),
            ["timezone"] = new OpenApiNull(),
            ["geo_lat"] = new OpenApiNull(),
            ["geo_lon"] = new OpenApiNull(),
            ["beltway_hit"] = new OpenApiNull(),
            ["beltway_distance"] = new OpenApiNull(),
            ["metro"] = new OpenApiNull(),
            ["divisions"] = new OpenApiNull(),
            ["qc_geo"] = new OpenApiNull(),
            ["qc_complete"] = new OpenApiNull(),
            ["qc_house"] = new OpenApiNull(),
            ["history_values"] = new OpenApiNull(),
            ["unparsed_parts"] = new OpenApiNull(),
            ["qc"] = new OpenApiNull()
        };

        private static OpenApiObject SampleIndoorWithApartment() => new OpenApiObject
        {
            ["extracted"] = new OpenApiString("кв 89"),
            ["hash"] = new OpenApiString("b2c3d4e5f6789012345678901234567890abcdef1234567890abcdef12345678"),
            ["units"] = new OpenApiArray
            {
                Mark("apartment", "квартира", "89")
            }
        };

        private static OpenApiObject SampleIndoorFloorAndApartment() => new OpenApiObject
        {
            ["extracted"] = new OpenApiString("ЭТАЖ 2, КВАРТИРА 89"),
            ["hash"] = new OpenApiString("b2c3d4e5f6789012345678901234567890abcdef1234567890abcdef12345678"),
            ["units"] = new OpenApiArray
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
