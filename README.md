# VTBL.AddressNormalizer

Нормализация адресных данных:

1. **BuildingUnit** — локация внутри здания → structured canonical + JSON + SHA256  
2. **BuildingAddress** — полный адрес → extract outdoor/indoor + читаемый канон строения  
3. **WebApi** — HTTP API v1 поверх ядра (`AddAddressNormalizer`)

## Быстрый старт

```powershell
dotnet build VTBL.AddressNormalizer.sln
dotnet test VTBL.AddressNormalizer.sln          # 332 теста
dotnet run --project VTBL.AddressNormalizer.Console
dotnet run --project VTBL.AddressNormalizer.Console -- address
dotnet run --project VTBL.AddressNormalizer.Console -- unit "КВАРТИРА 837"
dotnet run --project VTBL.AddressNormalizer.WebApi
```

**Console:** без аргументов — обе демо-секции; `address` / `unit` / `help`; второй аргумент — произвольная строка.

**Требования:** .NET 5.0 runtime, .NET SDK 6+ для сборки, Docker Compose (опционально, MSSQL).

## WebAPI

Подробности: [VTBL.AddressNormalizer.WebApi/README.md](VTBL.AddressNormalizer.WebApi/README.md).

| Method | Path | Назначение |
|--------|------|------------|
| POST | `/api/v1/normalize` | Полная нормализация (outdoor + indoor) |
| POST | `/api/v1/normalize/batch` | Batch той же нормализации (max `Batch:MaxItems`, default 100) |
| POST | `/api/v1/unit/normalize` | Только indoor / unit |
| POST | `/api/v1/address/extract` | Только outdoor |
| POST | `/api/v1/address/canonicalize` | Канон building location (без extract) |
| GET | `/health` | Liveness |

- Auth нет. Порт: `http://localhost:5000`. Swagger UI — только `Development` (`/swagger`).
- Вход: `{ "source": "..." }` (непустая строка; иначе 400, сообщения на русском).
- Correlation: `X-Correlation-Id` → иначе `X-Request-Id` → иначе GUID; echo в response header.
- Batch: ошибка элемента не останавливает остальные; если упали все — одна ошибка 400/500 без `items`.

### Пример ответа `POST /api/v1/normalize`

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

- `dadataOutdoor.fiasId` / `dadata` в v1 всегда `null` (заглушки).
- `indoorValue.hash` — SHA256 unit-канона (`ToCanonical`); пустые категории: `values: []`.
- Unit-endpoint дополнительно отдаёт top-level `canonical` и `hash` (тот же hash, что в `indoorValue`).

## Архитектура

```
VTBL.AddressNormalizer.sln
├── Abstractions/       # контракты, BuildingUnitLocation, Logging.ILogger
├── Infrastructure/    # реализации + AddAddressNormalizer
├── Console/           # CLI-демо (DemoServices)
├── WebApi/            # HTTP host, orchestration, NLog, Swagger
└── UnitTests/         # xUnit + WebApplicationFactory
```

```mermaid
flowchart LR
  HTTP["WebApi controllers"] --> SVC["IAddressNormalizationService"]
  SVC --> EXT["IBuildingLocationExtractor"]
  SVC --> CAN["IBuildingAddressCanonicalizer"]
  SVC --> PAR["IBuildingUnitParser"]
  SVC --> UCAN["IBuildingUnitCanonicalizer"]
  SVC --> HASH["ICanonicalHash"]
  SVC --> MAP["IndoorValueMapper"]
```

| Entry point | Когда |
|-------------|--------|
| HTTP `/api/v1/normalize` | Внешний доступ: outdoor + indoor |
| `IBuildingAddressNormalizer` | In-process: extract + readable canonical |
| `IBuildingLocationExtractor` | `ExtractSplit` / `Extract` |
| `IBuildingUnitParser` + `IBuildingUnitCanonicalizer` + `ICanonicalHash` | Indoor / unit: parse → canonical + SHA256 |

**Composition:** `AddAddressNormalizer()` — единый DI-граф для WebApi, Console и тестов.  
**Логирование ядра:** `Abstractions.Logging.ILogger`. Хост регистрирует реализацию через `AddAddressNormalizerLogging()` (WebApi → MEL/NLog, Console → stdout). Иначе — `NullLogger`. Debug на границе `BuildingLocationExtractor.ExtractSplit` (без полного адреса). Сообщения логов — на русском.

### In-process

```csharp
using Microsoft.Extensions.DependencyInjection;
using VTBL.AddressNormalizer.Abstractions.BuildingAddress;
using VTBL.AddressNormalizer.Infrastructure.Composition;

var services = new ServiceCollection();
services.AddAddressNormalizer();
var sp = services.BuildServiceProvider();

var result = sp.GetRequiredService<IBuildingAddressNormalizer>()
    .Normalize("г Москва, ул Сухонская, д 11, кв 89");

var split = sp.GetRequiredService<IBuildingLocationExtractor>()
    .ExtractSplit("г Москва, ул Сухонская, д 11, кв 89");
// Outdoor → "г Москва, ул Сухонская, д 11"
// Indoor  → "кв 89"
```

## Канонические префиксы (BuildingUnit)

Контракт matching — `Canonical` + `Hash`. Префиксы **не менять** без миграции данных.

| Префикс | Поле | Пример |
|---------|------|--------|
| `эт:` | floors | `эт:4` |
| `пом:` | premises | `пом:410` |
| `ком:` | rooms | `ком:35` |
| `оф:` | offices | `оф:18с` |
| `раб.м:` | workplaces | `раб.м:1` |
| `ч.п:` | parts | `ч.п:666` |
| `кв:` | apartments | `кв:837` |
| `каб:` | cabinets | `каб:69` |
| `под:` | entrances | `под:5` |
| `проезд:` | passages | `проезд:1` |
| `влад:` | holdings | `влад:1` |
| `склад:` | storages | `склад:1` |
| `блок:` | blocks | `блок:1` |
| `секц:` | sections | `секц:2` |
| `а/я:` | mailboxes | `а/я:165` |
| `лит:` | literas | `лит:б` |
| `диап:` | ranges | `диап:74-82` |
| `code:` | rawCodes | `code:659318` |
| `note:` | notes | `note:вход с торца` / `note:вход с фасада` |
| `unparsed:` | unparsed | `unparsed:…` |

## Тесты

```powershell
dotnet test VTBL.AddressNormalizer.sln
```

**332** теста (24.07.2026): BuildingUnit (полное unit-покрытие матрицы §2.M — статусы C/G; parser, slash UC-04 А1, corpus, category/negative/gaps, notes, проезд/владение/склад), BuildingAddress, composition DI, WebApi HTTP E2E.

## MSSQL (Docker, опционально)

```powershell
copy .env.example .env
docker compose up -d
```

`localhost:1435`, БД `AddressNormalizer`, user `sa`. Init: `docker/mssql/init/`.

## История изменений

### 24.07.2026 — Полное unit-покрытие BuildingUnit (матрица C/G)

- Приёмка §2.M / §2.M.0: все строки матрицы — только **C** или **G**; expand F11/R08/O05/W04/T03/A03/C03 закрыты
- UC-02 N-01…N-11, UC-04 А1 early≠slash-header, UC-06 G01–G06 — зелёные; прод Parser/Canonicalizer не менялся
- Решение: **332** теста (`dotnet test VTBL.AddressNormalizer.sln`); BuildingUnit-фильтр — 197

### 24.07.2026 — Slash UC-04 А1 early≠slash-header

- SlashChainTests: Theory `Parse_EarlyMarkers_AreNotSlashChainHeaders` — `проезд 1` / `влад 1` / `склад 1` (канон + field-assert; не multi-header)
- Fact `Slash_Checklist_Uc04_CoveredByExistingCases`; SampleCases и прод не менялись

### 24.07.2026 — Негативы UC-02 (N-01…N-11) + bare H02/ST02/B04/M02

- NegativeTests: `NeighborCases` — соседство маркеров ОФ/ОФИС, КОМ/КОМНАТА/К., КВ/К., ПОМ/ПОМЕЩЕНИЕ, ВЛАД/ВЛАДЕНИЕ, СКЛ/СКЛАД, ПР-Д/ПРОЕЗД; bare `владение`/`склад`/`БЛОК`/`А/Я`; characterization `КВАРТИРНЫЙ`
- N-09 не дублирован: ссылка на SlashChain `ЭТАЖ/ОФИС 3/314/5/WP`; прод не менялся

### 24.07.2026 — Known gaps G01–G04 Characterization

- KnownGapTests: `GapCharacterizationCases` — actual Host-канон F08/`ЦОКОЛ`, P09/`НЕЖ.ПОМ`, N03/`СЕКЦ`, R05/`КОМ. 3,4`
- Theory `Gap_G01_`…`Gap_G04_`; Desired из ТЗ §6 только в комментариях; прод не менялся

### 24.07.2026 — Sections/Blocks/Passages/Notes/Ranges/Preprocess (N02, B02, S03–S04, NT02, X01, X07, D02)

- CategoryTests: `BlockSectionCases`, `PassageHoldingStorageCases`, `RangeRawNoteUnparsedCases` (+ NT02/NT02b/D02), `PreprocessMixedCases` (кавычки X01, kitchen-sink X07)
- D02 letter-suffix range: Host actual = `диап:1а-2б`; bare H02/ST02 и gaps N03/P09/F08 не включались; прод не менялся

### 24.07.2026 — Floors / Rooms / Offices: ordinal, special, expand (F04–F12, R08, O05)

- CategoryTests: `FloorCases` (`ЭТАЖ 4-Я`, `ПОДВАЛ`, bare `ЭТАЖ`+офис), `ExpandRangeCases` (`ЭТ 1-3`, `КОМ 1-3`, `ОФИС 1-3`) + field-assert состава
- F08/`ЦОКОЛ` не добавлялся (G01); SampleCases и прод не менялись

### 24.07.2026 — Known gaps G05/G06 DocumentationOnly

- KnownGapTests: `Gap_G05_Doc_LiteraAbsentFromOutdoorPatterns` — outdoor `IndoorMarkerPatterns.All` Count=15, нет property `Litera`, Ids∋G05 (без `IndoorMarkerKind.Litera`)
- KnownGapTests: `Gap_G06_Doc_DotSlashExcludesCabRab` — reflection `SlashTypeHeaderRegex` без КАБ/РАБ, Ids∋G06
- Прод и outdoor Patterns не менялись; статус матрицы G05/G06 → **G**

### 24.07.2026 — Workplaces / Parts: space-формы и expand (W02/W04, T02/T03)

- CategoryTests: `WorkplaceCases` (`РАБ М 2`), `PartCases` (`Ч П 12`), `ExpandRangeCases` (`РАБ.М.1-3`, `Ч.П.1-2`) + field-assert состава
- SampleCases с `РАБ.М.1` / `ч.п.` не трогались; прод не менялся

### 24.07.2026 — Cabinets / Entrances / Apartments expand (C02–C03, E02, A03)

- CategoryTests: `CabinetCases` (`КАБ.`/`КАБ`), `EntranceCases` (slash `ПОДЪЕЗД/ЭТ`), `ExpandRangeCases` (`КАБ 1-3`, `КВ 1-3`) + field-assert состава
- Pipeline-якоря A04/C05/E03 в SampleCases не дублировались

### 24.07.2026 — LiteraCases (L01–L03, L02b)

- В `BuildingUnitParserCategoryTests` добавлены C-кейсы литеры: `ЛИТЕРА`/`ЛИТРА`/`ЛИТР`, characterization голого `ЛИТ`, mixed с офисом
- Expected для L02 зафиксирован по фактическому Parse (`code:б|code:лит`); прод не менялся

### 24.07.2026 — Pipeline SampleCases: кв/каб/под/а/я/диап

- В `BuildingUnitSampleCases.NormalizeCases` добавлены якоря A04, C05, E03, M03, D03 (`кв:`, `каб:`, `под:`, `а/я:`, `диап:`)

### 24.07.2026 — каркас unit-слоя BuildingUnit (B/D/E)

- Helpers: `BuildingUnitTestAsserts`, реестр `BuildingUnitKnownGaps` (G01…G06)
- Скелеты: Category / Negative / KnownGap Tests + `.AGENTS.md` зоны BuildingUnit

### 24.07.2026 — матрица Unparsed (U01–U02)

- CategoryTests: `RangeRawNoteUnparsedCases` — `foo+bar` → `unparsed:foo+bar`; `ОФИС 5 foo+bar` → `оф:5|unparsed:foo+bar`
- Field-assert U02: `Offices` = `5`, `Unparsed` не пуст; прод не менялся

### 24.07.2026 — рефакторинг BuildingUnitParser (вариант C)

- Общие лексемы маркеров: `IndoorMarkerLexemes` + фабрика `IndoorMarkerRegexFactory`
- `IndoorMarkerPatterns` и early-regex парсера собраны из одних лексем
- Early-маркеры table-driven (`EarlyMarkersBefore/AfterBlockSection`); `CollapseWorking`
- Единый `ApplySlashTypeValue` для slash-chain и dot-slash

### 24.07.2026 — indoor «склад»

- Категория `Storages` / канон `склад:`; маркеры `СКЛАД`, `СКЛ.`; форма `склад 1`
- Extract: `IndoorMarkerKind.Storage`

### 24.07.2026 — indoor «владение»

- Категория `Holdings` / канон `влад:`; маркеры `ВЛАДЕНИЕ`, `ВЛАД`, `ВЛ.`; форма `владение 1`
- Extract: `IndoorMarkerKind.Holding`

### 24.07.2026 — indoor «проезд»

- Категория `Passages` / канон `проезд:`; маркеры `ПРОЕЗД`, `ПР-Д`; формы `проезд 1` и `1-й проезд`
- Extract: `IndoorMarkerKind.Passage`

### 24.07.2026 — удаление IBuildingUnitClassifier

- Удалены `IBuildingUnitClassifier`, `BuildingUnitClassifier`, `BuildingUnitCategory` и тесты
- Console-демо без CATEGORY; `IndoorMarkerPatterns` оставлены для parser/extract

### 24.07.2026 — удаление IBuildingUnitNormalizer

- Удалены `IBuildingUnitNormalizer`, `BuildingUnitNormalizer`, `BuildingUnitNormalizationResult`
- Indoor: `IBuildingUnitParser` → `IBuildingUnitCanonicalizer` → `ICanonicalHash` (оркестрация в WebApi/Console)
- Убран `Newtonsoft.Json` из Infrastructure; Console-демо без JSON

### 23.07.2026 — README актуализированы

- Корневой и WebApi README приведены к текущему контракту API (dadataOutdoor / indoorValue.hash)
- История сжата; убраны устаревшие счётчики тестов из промежуточных записей

### 23.07.2026 — DTO normalize

- `fiasId` и `dadata` внутри `dadataOutdoor` (v1 = `null`)
- `indoorValue.hash` = SHA256 unit-канона; unit endpoint сохраняет top-level `canonical`/`hash`

### 23.07.2026 — XML summary

- Однострочные `/// <summary>` → многострочный вид (Abstractions / Infrastructure / WebApi)

### 22.07.2026 — ядро и логирование

- Примечание `вход с фасада` в `NoteRegex`
- `Abstractions.Logging.ILogger` + хост-адаптеры; Debug на границах Infrastructure; тексты логов на русском
- Удалены CRM FieldAdapters; DI вместо Factory (`AddAddressNormalizer`)

### 21.07.2026 — WebApi v1 и ядро

- Endpoints normalize / batch / unit / extract / canonicalize / health
- NLog + Correlation Id; BuildingAddress / BuildingUnit; TFM `net5.0`

### 15–20.07.2026 — старт решения

- Solution, Docker/MSSQL, seed адресов
