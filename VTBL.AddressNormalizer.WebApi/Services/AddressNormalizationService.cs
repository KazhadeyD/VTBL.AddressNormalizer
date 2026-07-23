using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VTBL.AddressNormalizer.Abstractions.BuildingAddress;
using VTBL.AddressNormalizer.Abstractions.BuildingUnit;
using VTBL.AddressNormalizer.Abstractions.Shared;
using VTBL.AddressNormalizer.WebApi.Mapping;
using VTBL.AddressNormalizer.WebApi.Models;

namespace VTBL.AddressNormalizer.WebApi.Services
{
    /// <summary>
    /// Оркестрация нормализации адреса поверх сервисов ядра (DI).
    /// </summary>
    public class AddressNormalizationService : IAddressNormalizationService
    {
        private const string InvalidSourceMessage = "source должен быть непустой строкой";
        private const string InvalidBatchMessage = "список items должен быть непустым и не превышать MaxItems";
        private const string ItemValidationError = "source должен быть непустой строкой";
        private const string AllFailValidationMessage = "все элементы batch не прошли валидацию";
        private const string AllFailExceptionMessage = "все элементы batch завершились с ошибкой";
        private const string AllFailMixedMessage = "все элементы batch завершились неуспешно";

        private readonly ILogger<AddressNormalizationService> _logger;
        private readonly IBuildingLocationExtractor _locationExtractor;
        private readonly IBuildingAddressCanonicalizer _addressCanonicalizer;
        private readonly IBuildingUnitNormalizer _unitNormalizer;
        private readonly ICanonicalHash _canonicalHash;

        /// <summary>
        /// Создаёт сервис оркестрации с внедрёнными зависимостями ядра.
        /// </summary>
        public AddressNormalizationService(
            ILogger<AddressNormalizationService> logger,
            IBuildingLocationExtractor locationExtractor,
            IBuildingAddressCanonicalizer addressCanonicalizer,
            IBuildingUnitNormalizer unitNormalizer,
            ICanonicalHash canonicalHash)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _locationExtractor = locationExtractor ?? throw new ArgumentNullException(nameof(locationExtractor));
            _addressCanonicalizer = addressCanonicalizer ?? throw new ArgumentNullException(nameof(addressCanonicalizer));
            _unitNormalizer = unitNormalizer ?? throw new ArgumentNullException(nameof(unitNormalizer));
            _canonicalHash = canonicalHash ?? throw new ArgumentNullException(nameof(canonicalHash));
        }

        /// <inheritdoc />
        public NormalizeValueDto NormalizeFull(string source)
        {
            _logger.LogInformation("Запущена полная нормализация");
            EnsureValidSource(source);
            return NormalizeFullCore(source);
        }

        /// <inheritdoc />
        public UnitNormalizeResult NormalizeUnit(string source)
        {
            _logger.LogInformation("Запущена нормализация unit");
            EnsureValidSource(source);

            var unit = _unitNormalizer.Normalize(source);
            var indoor = IndoorValueMapper.ToIndoorValueDto(unit.Location);
            indoor.Hash = unit.Hash;
            return new UnitNormalizeResult
            {
                Source = source,
                IndoorValue = indoor,
                Canonical = unit.Canonical,
                Hash = unit.Hash
            };
        }

        /// <inheritdoc />
        public string ExtractOutdoor(string source)
        {
            _logger.LogInformation("Запущено извлечение outdoor");
            EnsureValidSource(source);
            return _locationExtractor.ExtractSplit(source).Outdoor;
        }

        /// <inheritdoc />
        public string Canonicalize(string source)
        {
            _logger.LogInformation("Запущена канонизация");
            EnsureValidSource(source);
            return _addressCanonicalizer.ToCanonical(source);
        }

        /// <inheritdoc />
        public BatchOutcome NormalizeBatch(IReadOnlyList<string> sources, int maxItems)
        {
            _logger.LogInformation("Запущена пакетная нормализация");

            if (sources == null || sources.Count == 0 || sources.Count > maxItems)
            {
                _logger.LogWarning("Пакетная нормализация: ошибка валидации — пустой список или превышен MaxItems");
                return new BatchOutcome
                {
                    Kind = BatchOutcomeKind.RequestInvalid,
                    ErrorMessage = InvalidBatchMessage,
                    Items = null
                };
            }

            var items = new List<BatchItemResultDto>(sources.Count);
            var validationFailCount = 0;
            var exceptionFailCount = 0;
            var okCount = 0;

            for (var i = 0; i < sources.Count; i++)
            {
                var raw = sources[i];
                var displaySource = raw ?? string.Empty;

                if (string.IsNullOrWhiteSpace(raw))
                {
                    validationFailCount++;
                    _logger.LogWarning("Пакетная нормализация: элемент {ItemIndex} не прошёл валидацию", i);
                    items.Add(new BatchItemResultDto
                    {
                        Status = "error",
                        Source = displaySource,
                        Error = ItemValidationError
                    });
                    continue;
                }

                try
                {
                    var value = NormalizeFullCore(raw);
                    okCount++;
                    items.Add(new BatchItemResultDto
                    {
                        Status = "ok",
                        Source = displaySource,
                        Value = value
                    });
                }
                catch (Exception ex)
                {
                    exceptionFailCount++;
                    _logger.LogWarning(ex, "Пакетная нормализация: элемент {ItemIndex} завершился с исключением", i);
                    items.Add(new BatchItemResultDto
                    {
                        Status = "error",
                        Source = displaySource,
                        Error = ex.Message
                    });
                }
            }

            if (okCount > 0)
            {
                return new BatchOutcome
                {
                    Kind = BatchOutcomeKind.PartialOrSuccess,
                    Items = items
                };
            }

            if (exceptionFailCount == 0)
            {
                return new BatchOutcome
                {
                    Kind = BatchOutcomeKind.AllFailValidation,
                    Items = items,
                    ErrorMessage = AllFailValidationMessage
                };
            }

            if (validationFailCount == 0)
            {
                return new BatchOutcome
                {
                    Kind = BatchOutcomeKind.AllFailException,
                    Items = items,
                    ErrorMessage = AllFailExceptionMessage
                };
            }

            return new BatchOutcome
            {
                Kind = BatchOutcomeKind.AllFailMixed,
                Items = items,
                ErrorMessage = AllFailMixedMessage
            };
        }

        /// <summary>
        /// Полная нормализация без top-level validation (для single после EnsureValidSource и для batch).
        /// </summary>
        /// <remarks>
        /// Virtual — seam для unit/HTTP-тестов all-fail exception без мока ядра.
        /// </remarks>
        protected virtual NormalizeValueDto NormalizeFullCore(string source)
        {
            var split = _locationExtractor.ExtractSplit(source);
            var outdoorCanonical = _addressCanonicalizer.ToCanonical(split.Outdoor);
            var hash = _canonicalHash.ComputeSha256(outdoorCanonical);
            var unit = _unitNormalizer.Normalize(split.Indoor);
            var indoor = IndoorValueMapper.ToIndoorValueDto(unit.Location);
            indoor.Hash = unit.Hash;

            return new NormalizeValueDto
            {
                DadataOutdoor = new DadataOutdoorDto
                {
                    Extracted = split.Outdoor,
                    OutdoorCanonical = outdoorCanonical,
                    Hash = hash,
                    FiasId = null,
                    Dadata = null
                },
                IndoorValue = indoor
            };
        }

        private void EnsureValidSource(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                _logger.LogWarning("Валидация source не пройдена: пустая строка или пробелы");
                throw new ArgumentException(InvalidSourceMessage, nameof(source));
            }
        }
    }
}
