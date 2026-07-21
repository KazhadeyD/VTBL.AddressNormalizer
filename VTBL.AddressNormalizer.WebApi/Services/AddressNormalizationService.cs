using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VTBL.AddressNormalizer.Infrastructure.Composition;
using VTBL.AddressNormalizer.WebApi.Mapping;
using VTBL.AddressNormalizer.WebApi.Models;

namespace VTBL.AddressNormalizer.WebApi.Services
{
    /// <summary>
    /// Оркестрация нормализации адреса через <see cref="AddressNormalizerFactory"/>.
    /// </summary>
    public class AddressNormalizationService : IAddressNormalizationService
    {
        private const string InvalidSourceMessage = "source must be a non-whitespace string";
        private const string InvalidBatchMessage = "batch items must be a non-empty list within MaxItems limit";
        private const string ItemValidationError = "source must be a non-whitespace string";
        private const string AllFailValidationMessage = "all batch items failed validation";
        private const string AllFailExceptionMessage = "all batch items failed with exceptions";
        private const string AllFailMixedMessage = "all batch items failed";

        private readonly ILogger<AddressNormalizationService> _logger;

        /// <summary>
        /// Создаёт сервис оркестрации.
        /// </summary>
        public AddressNormalizationService(ILogger<AddressNormalizationService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public NormalizeValueDto NormalizeFull(string source)
        {
            _logger.LogInformation("NormalizeFull started");
            EnsureValidSource(source);
            return NormalizeFullCore(source);
        }

        /// <inheritdoc />
        public UnitNormalizeResult NormalizeUnit(string source)
        {
            _logger.LogInformation("NormalizeUnit started");
            EnsureValidSource(source);

            var unit = AddressNormalizerFactory.BuildingUnitNormalizer.Normalize(source);
            return new UnitNormalizeResult
            {
                Source = source,
                IndoorValue = IndoorValueMapper.ToIndoorValueDto(unit.Location),
                Canonical = unit.Canonical,
                Hash = unit.Hash
            };
        }

        /// <inheritdoc />
        public string ExtractOutdoor(string source)
        {
            _logger.LogInformation("ExtractOutdoor started");
            EnsureValidSource(source);
            return AddressNormalizerFactory.BuildingLocationExtractor.ExtractSplit(source).Outdoor;
        }

        /// <inheritdoc />
        public string Canonicalize(string source)
        {
            _logger.LogInformation("Canonicalize started");
            EnsureValidSource(source);
            return AddressNormalizerFactory.BuildingAddressCanonicalizer.ToCanonical(source);
        }

        /// <inheritdoc />
        public BatchOutcome NormalizeBatch(IReadOnlyList<string> sources, int maxItems)
        {
            _logger.LogInformation("NormalizeBatch started");

            if (sources == null || sources.Count == 0 || sources.Count > maxItems)
            {
                _logger.LogWarning("NormalizeBatch validation failed: invalid items list or MaxItems exceeded");
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
                    _logger.LogWarning("NormalizeBatch item {ItemIndex} failed validation", i);
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
                    _logger.LogWarning(ex, "NormalizeBatch item {ItemIndex} failed with exception", i);
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
            var split = AddressNormalizerFactory.BuildingLocationExtractor.ExtractSplit(source);
            var outdoorCanonical = AddressNormalizerFactory.BuildingAddressCanonicalizer.ToCanonical(split.Outdoor);
            var hash = AddressNormalizerFactory.CanonicalHash.ComputeSha256(outdoorCanonical);
            var unit = AddressNormalizerFactory.BuildingUnitNormalizer.Normalize(split.Indoor);

            return new NormalizeValueDto
            {
                FiasId = null,
                DadataOutdoor = new DadataOutdoorDto
                {
                    Extracted = split.Outdoor,
                    OutdoorCanonical = outdoorCanonical,
                    Hash = hash
                },
                IndoorValue = IndoorValueMapper.ToIndoorValueDto(unit.Location)
            };
        }

        private void EnsureValidSource(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                _logger.LogWarning("Source validation failed: empty or whitespace");
                throw new ArgumentException(InvalidSourceMessage, nameof(source));
            }
        }
    }
}
