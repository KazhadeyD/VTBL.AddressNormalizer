# VTBL.AddressNormalizer.WebApi

HTTP API нормализации адресов поверх ядра (`AddAddressNormalizer` + `IAddressNormalizationService`).

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
# Development (Swagger включён)
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project VTBL.AddressNormalizer.WebApi
```

## Endpoints

| Method | Path | Тело | Успех |
|--------|------|------|--------|
| POST | `/api/v1/normalize` | `{ "source": "..." }` | `200` + outdoor/indoor |
| POST | `/api/v1/normalize/batch` | `{ "items": [ { "source": "..." }, ... ] }` | `200` + per-item `ok`/`error` |
| POST | `/api/v1/unit/normalize` | `{ "source": "..." }` | `200` + indoor + canonical + hash |
| POST | `/api/v1/address/extract` | `{ "source": "..." }` | `200` + `extracted` |
| POST | `/api/v1/address/canonicalize` | `{ "source": "..." }` | `200` + `canonical` |
| GET | `/health` | — | `200` `{ "status": "Healthy" }` |

Пустой / null / whitespace `source` → **400** `{ "error": "..." }`.
Unhandled → **500** `{ "error": "..." }` (`ApiExceptionFilter`).

### Batch

- Лимит: `Batch:MaxItems` в `appsettings.json` (default **100**).
- Ошибка одного элемента не останавливает остальные (`status: "error"` в item).
- Если **все** элементы упали: validation → **400**, исключения/mixed → **500**; тело — одна ошибка, без `items`.

### Correlation Id

Приоритет: `X-Correlation-Id` → `X-Request-Id` → GUID. Значение пишется в response header `X-Correlation-Id` и в NLog (`CorrelationId`).

## Пример

```powershell
curl -X POST http://localhost:5000/api/v1/normalize `
  -H "Content-Type: application/json" `
  -H "X-Correlation-Id: demo-1" `
  -d "{\"source\":\"г Москва, ул Сухонская, д 11, кв 89\"}"
```

```json
{
  "source": "г Москва, ул Сухонская, д 11, кв 89",
  "value": {
    "fiasId": null,
    "dadataOutdoor": {
      "extracted": "г Москва, ул Сухонская, д 11",
      "outdoorCanonical": "г Москва, ул Сухонская, д 11",
      "hash": "<sha256 от outdoorCanonical>"
    },
    "indoorValue": {
      "apartments": { "name": "квартира", "values": ["89"] }
    }
  }
}
```

`indoorValue` — все категории локации с русским `name` и `values` (пустые категории: `values: []`).

## Конфигурация

| Файл / секция | Назначение |
|---------------|------------|
| `appsettings.json` → `Batch:MaxItems` | Максимум элементов batch |
| `nlog.config` | Console + `logs/webapi-*.log`; layout с `CorrelationId` |
| `Properties/launchSettings.json` | URL, `ASPNETCORE_ENVIRONMENT` |

## Слои проекта

```
Controllers/   → только IAddressNormalizationService
Services/      → оркестрация (ExtractSplit, canonical, hash, unit, mapper)
Mapping/       → BuildingUnitLocation → IndoorValueDto
Middleware/    → Correlation Id
Filters/       → ApiExceptionFilter (500)
Models/        → DTO запросов/ответов
Swagger/       → примеры OpenAPI
```

DI (`Startup`): `AddAddressNormalizerLogging()` + `AddAddressNormalizer()` + `AddressNormalizationService` (singleton).

**Логирование:**
- **HTTP:** `RequestLoggingMiddleware` — method, path, status, duration (2xx Info / 4xx Warning / 5xx Error); skip `/health`, `/swagger`
- **Orchestration:** `AddressNormalizationService` — Information на старт, Warning на валидацию
- **Unhandled:** `ApiExceptionFilter` — Error
- **Ядро:** `Abstractions.Logging.ILogger` → `MicrosoftExtensionsAddressNormalizerLogger` (категория `VTBL.AddressNormalizer`); Infrastructure пишет Debug на `ExtractSplit` / `BuildingUnit.Normalize` (вкл. через `Logging:LogLevel` / `nlog.config`)

## Тесты

HTTP E2E и unit orchestration: `VTBL.AddressNormalizer.UnitTests/WebApi/` (`WebApplicationFactory`, Environment=`Production`).

```powershell
dotnet test VTBL.AddressNormalizer.sln --filter "FullyQualifiedName~WebApi"
```
