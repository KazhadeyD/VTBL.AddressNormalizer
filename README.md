# VTBL.AddressNormalizer

Решение для нормализации адресов (VTBL).

## Структура

| Проект | Назначение |
|--------|------------|
| `VTBL.AddressNormalizer.Abstractions` | Контракты и модели |
| `VTBL.AddressNormalizer.Infrastructure` | Реализация нормализации |
| `VTBL.AddressNormalizer.Console` | Консольное приложение |
| `VTBL.AddressNormalizer.UnitTests` | Модульные тесты (MSTest) |

## Сборка

Открыть `VTBL.AddressNormalizer.sln` в Visual Studio или собрать через MSBuild:

```powershell
msbuild VTBL.AddressNormalizer.sln /p:Configuration=Debug
```

## История изменений

### 2026-07-21

- Решение переведено с формата `.slnx` на классический `.sln` для совместимости с MSBuild и старыми версиями Visual Studio.
