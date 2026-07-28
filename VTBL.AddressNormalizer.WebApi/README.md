# VTBL.AddressNormalizer.WebApi

HTTP API для нормализации адресов и внутренних адресных частей поверх ядра (`AddAddressNormalizer` + `IAddressNormalizationService`).

Общее описание решения — в [корневом README](../README.md).

## Запуск

```powershell
dotnet run --project VTBL.AddressNormalizer.WebApi
```

| | |
|--|--|
| URL | `http://localhost:5000` (`Properties/launchSettings.json`) |
| Swagger | `http://localhost:5000/swagger` (только `Development`) |
| Auth | нет |
| TFM | `net5.0` |

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project VTBL.AddressNormalizer.WebApi
```

## Endpoints

| Method | Path | Тело | Успех |
|--------|------|------|--------|
| POST | `/api/v1/normalize` | `{ "source": "..." }` | `200` + `buildingValue` + `indoorValue` |
| POST | `/api/v1/normalize/batch` | `{ "items": [ { "source": "..." }, ... ] }` | `200` + per-item `ok`/`error` |
| POST | `/api/v1/unit/normalize` | `{ "source": "..." }` | `200` + `indoorValue` + top-level `canonical`/`hash` |
| POST | `/api/v1/address/extract` | `{ "source": "..." }` | `200` + `extracted` |
| POST | `/api/v1/address/canonicalize` | `{ "source": "..." }` | `200` + `canonical` |
| GET | `/health` | — | `200` `{ "status": "Healthy", "service": { "assemblyVersion": "...", "startedAtUtc": "...", "uptimeMs": 1234, "uptime": "00:20:34" }, "checks": { ... } }` |
| GET | `/health/live` | — | `200` liveness-only (`self`) |

Пустой / null / whitespace `source` → **400** `{ "error": "..." }` (русский текст).  
Unhandled → **500** `{ "error": "..." }` (`ApiExceptionFilter`).

### Batch

- Лимит: `Batch:MaxItems` в `appsettings.json` (default **100**).
- Ошибка одного элемента не останавливает остальные (`status: "error"` в item).
- Если **все** элементы упали: validation → **400**, исключения/mixed → **500**; тело — одна ошибка, без `items`.

### Request Id

Можно передать заголовок `X-VTBL-Request-ID`. Если он не указан, сервис сгенерирует его автоматически.  
Это значение возвращается в response header `X-VTBL-Request-ID` и пишется в NLog (`CorrelationId` в layout).

## Контракт ответа normalize

```json
{
  "source": "г Москва, ул Сухонская, д 11, кв 89",
  "value": {
    "buildingValue": {
      "extracted": "г Москва, ул Сухонская, д 11",
      "normalizedAddress": "г Москва, ул Сухонская, д 11",
      "hash": "<sha256 от normalizedAddress>",
      "fiasId": null,
      "dadata": {
        "suggest": {
          "suggestions": [
            {
              "value": "г Москва, ул Сухонская, д 11",
              "unrestricted_value": "г Москва, ул Сухонская, д 11",
              "data": { "source": "г Москва, ул Сухонская, д 11", "result": "г Москва, ул Сухонская, д 11", "country": "Россия", "country_iso_code": "RU" }
            }
          ]
        },
        "clean": {
          "source": "г Москва, ул Сухонская, д 11",
          "result": "г Москва, ул Сухонская, д 11",
          "country": "Россия",
          "country_iso_code": "RU"
        }
      }
    },
    "indoorValue": {
      "extracted": "кв 89",
      "hash": "<sha256 от unit canonical>",
      "units": [
        { "id": "apartment", "name": "квартира", "values": ["89"] }
      ]
    }
  }
}
```

| Поле | Смысл |
|------|--------|
| `buildingValue.extracted` | Outdoor после extract |
| `buildingValue.normalizedAddress` | Канон outdoor |
| `buildingValue.hash` | SHA256(normalizedAddress) |
| `buildingValue.fiasId` | `dadata.suggest.suggestions[0].data.house_fias_id` → fallback `dadata.clean.house_fias_id` |
| `buildingValue.dadata.suggest` | Заглушка структуры ответа DaData `suggest/address` |
| `buildingValue.dadata.clean` | Заглушка структуры ответа DaData `clean/address` |
| `indoorValue.extracted` | Indoor после extract (внутренний хвост адреса) |
| `indoorValue.hash` | SHA256 unit-канона (`ToCanonical`) |
| `indoorValue.units` | Sparse-массив `{ id, name, values }`; только категории с данными |

**Unit** (`/api/v1/unit/normalize`): тот же `indoorValue.units` + top-level `canonical` и `hash` (дублирует `indoorValue.hash`).

## Пример запроса

```powershell
curl -X POST http://localhost:5000/api/v1/normalize `
  -H "Content-Type: application/json" `
  -H "X-VTBL-Request-ID: demo-1" `
  -d "{\"source\":\"г Москва, ул Сухонская, д 11, кв 89\"}"
```

## Конфигурация

| Файл / секция | Назначение |
|---------------|------------|
| `appsettings.json` → `Batch:MaxItems` | Максимум элементов batch |
| `appsettings.json` → `NLog` | Console + `C:\inetpub\logs\VTBL.AddressNormalizer.WebApi\webapi-*.log`; правило `VTBL.AddressNormalizer*` (Debug+); layout с `CorrelationId` |
| `appsettings.Development.json` → `Logging:LogLevel:VTBL.AddressNormalizer` | Debug логов ядра в Development |
| `web.config` → `ASPNETCORE_ENVIRONMENT` | IIS: `Development` (Swagger + DeveloperExceptionPage); на Prod сменить на `Production` |
| `Properties/launchSettings.json` | URL, `ASPNETCORE_ENVIRONMENT` (локальный `dotnet run`) |

## Слои

```
Controllers/   → IAddressNormalizationService
Services/      → оркестрация (ExtractSplit, Parse, ToCanonical, hash, mapper)
Services/Dadata/ → IDadataService (suggest/clean DaData)
Mapping/       → BuildingUnitLocation → IndoorValueDto
Middleware/    → Correlation Id, RequestLogging
Filters/       → ApiExceptionFilter (500)
Health/        → readiness/liveness, AddWebApiHealthChecks, MapWebApiHealthChecks
Logging/       → AddAddressNormalizerLogging → MEL/NLog
Models/        → DTO запросов/ответов
Swagger/       → OpenAPI, примеры, AddWebApiSwagger()
```

DI (`Startup`): `AddAddressNormalizerLogging()` → `AddAddressNormalizer()` → `IDadataService` → `AddressNormalizationService`.

**Логирование (тексты на русском):**
- `RequestLoggingMiddleware` — HTTP method/path/status/duration (skip `/health`, `/swagger`)
- `AddressNormalizationService` — старт операций, Warning на валидацию
- `ApiExceptionFilter` — Error на unhandled
- Ядро — Debug через `Abstractions.Logging.ILogger` на `ExtractSplit` (категория `VTBL.AddressNormalizer`)

## Тесты

```powershell
dotnet test VTBL.AddressNormalizer.sln --filter "FullyQualifiedName~WebApi"
```

Каталог: `VTBL.AddressNormalizer.UnitTests/WebApi/` (`WebApplicationFactory`, Environment=`Production`).
