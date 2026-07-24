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
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project VTBL.AddressNormalizer.WebApi
```

## Endpoints

| Method | Path | Тело | Успех |
|--------|------|------|--------|
| POST | `/api/v1/normalize` | `{ "source": "..." }` | `200` + `dadataOutdoor` + `indoorValue` |
| POST | `/api/v1/normalize/batch` | `{ "items": [ { "source": "..." }, ... ] }` | `200` + per-item `ok`/`error` |
| POST | `/api/v1/unit/normalize` | `{ "source": "..." }` | `200` + `indoorValue` + top-level `canonical`/`hash` |
| POST | `/api/v1/address/extract` | `{ "source": "..." }` | `200` + `extracted` |
| POST | `/api/v1/address/canonicalize` | `{ "source": "..." }` | `200` + `canonical` |
| GET | `/health` | — | `200` `{ "status": "Healthy" }` |

Пустой / null / whitespace `source` → **400** `{ "error": "..." }` (русский текст).  
Unhandled → **500** `{ "error": "..." }` (`ApiExceptionFilter`).

### Batch

- Лимит: `Batch:MaxItems` в `appsettings.json` (default **100**).
- Ошибка одного элемента не останавливает остальные (`status: "error"` в item).
- Если **все** элементы упали: validation → **400**, исключения/mixed → **500**; тело — одна ошибка, без `items`.

### Correlation Id

Приоритет: `X-Correlation-Id` → `X-Request-Id` → GUID.  
Значение: response header `X-Correlation-Id` и NLog (`CorrelationId` в layout).

## Контракт ответа normalize

```json
{
  "source": "г Москва, ул Сухонская, д 11, кв 89",
  "value": {
    "dadataOutdoor": {
      "extracted": "г Москва, ул Сухонская, д 11",
      "outdoorCanonical": "г Москва, ул Сухонская, д 11",
      "hash": "<sha256 от outdoorCanonical>",
      "fiasId": null,
      "dadata": null
    },
    "indoorValue": {
      "hash": "<sha256 от unit canonical>",
      "apartments": { "name": "квартира", "values": ["89"] },
      "floors": { "name": "этаж", "values": [] }
    }
  }
}
```

| Поле | Смысл |
|------|--------|
| `dadataOutdoor.extracted` | Outdoor после extract |
| `dadataOutdoor.outdoorCanonical` | Канон outdoor |
| `dadataOutdoor.hash` | SHA256(outdoorCanonical) |
| `dadataOutdoor.fiasId` | Заглушка v1 = `null` |
| `dadataOutdoor.dadata` | Заглушка v1 = `null` |
| `indoorValue.hash` | SHA256 unit-канона (`ToCanonical`) |
| `indoorValue.*` | 17 категорий `{ name, values }` |

**Unit** (`/api/v1/unit/normalize`): те же категории в `indoorValue` + top-level `canonical` и `hash` (дублирует `indoorValue.hash`).

## Пример запроса

```powershell
curl -X POST http://localhost:5000/api/v1/normalize `
  -H "Content-Type: application/json" `
  -H "X-Correlation-Id: demo-1" `
  -d "{\"source\":\"г Москва, ул Сухонская, д 11, кв 89\"}"
```

## Конфигурация

| Файл / секция | Назначение |
|---------------|------------|
| `appsettings.json` → `Batch:MaxItems` | Максимум элементов batch |
| `appsettings.Development.json` → `Logging:LogLevel:VTBL.AddressNormalizer` | Debug логов ядра в Development |
| `nlog.config` | Console + `logs/webapi-*.log`; правило `VTBL.AddressNormalizer*` (Debug+); layout с `CorrelationId` |
| `Properties/launchSettings.json` | URL, `ASPNETCORE_ENVIRONMENT` |

## Слои

```
Controllers/   → IAddressNormalizationService
Services/      → оркестрация (ExtractSplit, Parse, ToCanonical, hash, mapper)
Mapping/       → BuildingUnitLocation → IndoorValueDto
Middleware/    → Correlation Id, RequestLogging
Filters/       → ApiExceptionFilter (500)
Logging/       → AddAddressNormalizerLogging → MEL/NLog
Models/        → DTO запросов/ответов
Swagger/       → примеры OpenAPI
```

DI (`Startup`): `AddAddressNormalizerLogging()` → `AddAddressNormalizer()` → `AddressNormalizationService`.

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
