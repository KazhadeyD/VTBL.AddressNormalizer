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
        private readonly IBuildingUnitParser _unitParser;
        private readonly IBuildingUnitCanonicalizer _unitCanonicalizer;
        private readonly ICanonicalHash _canonicalHash;

        /// <summary>
        /// Создаёт сервис оркестрации с внедрёнными зависимостями ядра.
        /// </summary>
        public AddressNormalizationService(
            ILogger<AddressNormalizationService> logger,
            IBuildingLocationExtractor locationExtractor,
            IBuildingAddressCanonicalizer addressCanonicalizer,
            IBuildingUnitParser unitParser,
            IBuildingUnitCanonicalizer unitCanonicalizer,
            ICanonicalHash canonicalHash)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _locationExtractor = locationExtractor ?? throw new ArgumentNullException(nameof(locationExtractor));
            _addressCanonicalizer = addressCanonicalizer ?? throw new ArgumentNullException(nameof(addressCanonicalizer));
            _unitParser = unitParser ?? throw new ArgumentNullException(nameof(unitParser));
            _unitCanonicalizer = unitCanonicalizer ?? throw new ArgumentNullException(nameof(unitCanonicalizer));
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

            var (location, canonical, hash) = NormalizeUnitCore(source);
            var indoor = IndoorValueMapper.ToIndoorValueDto(location);
            indoor.Extracted = source;
            indoor.Hash = hash;
            return new UnitNormalizeResult
            {
                Source = source,
                IndoorValue = indoor,
                Canonical = canonical,
                Hash = hash
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
            var outdoorHash = _canonicalHash.ComputeSha256(outdoorCanonical);
            var (location, _, indoorHash) = NormalizeUnitCore(split.Indoor);
            var indoor = IndoorValueMapper.ToIndoorValueDto(location);
            indoor.Extracted = split.Indoor;
            indoor.Hash = indoorHash;
            var suggest = CreateSuggestStub(split.Outdoor, outdoorCanonical);
            var clean = CreateCleanStub(split.Outdoor, outdoorCanonical);

            return new NormalizeValueDto
            {
                BuildingValue = new DadataOutdoorDto
                {
                    Extracted = split.Outdoor,
                    NormalizedAddress = outdoorCanonical,
                    Hash = outdoorHash,
                    FiasId = ResolveBuildingFiasId(suggest, clean),
                    Suggest = suggest,
                    Clean = clean
                },
                IndoorValue = indoor
            };
        }

        private static string ResolveBuildingFiasId(DadataSuggestAddressDto suggest, DadataCleanAddressDto clean)
        {
            var suggestHouseFiasId = suggest?.Suggestions?[0]?.Data?.HouseFiasId;
            if (!string.IsNullOrWhiteSpace(suggestHouseFiasId))
                return suggestHouseFiasId;

            return string.IsNullOrWhiteSpace(clean?.HouseFiasId) ? null : clean.HouseFiasId;
        }

        private static DadataSuggestAddressDto CreateSuggestStub(string extracted, string normalizedAddress)
        {
            var data = CreateAddressDataStub(extracted, normalizedAddress, houseFiasId: "suggest-house-fias-id");
            return new DadataSuggestAddressDto
            {
                Suggestions = new[]
                {
                    new DadataSuggestAddressSuggestionDto
                    {
                        Value = normalizedAddress,
                        UnrestrictedValue = normalizedAddress,
                        Data = data
                    }
                }
            };
        }

        private static DadataCleanAddressDto CreateCleanStub(string extracted, string normalizedAddress)
        {
            var data = CreateAddressDataStub(extracted, normalizedAddress, houseFiasId: "clean-house-fias-id");
            return new DadataCleanAddressDto
            {
                Source = data.Source,
                Result = data.Result,
                PostalCode = data.PostalCode,
                Country = data.Country,
                CountryIsoCode = data.CountryIsoCode,
                FederalDistrict = data.FederalDistrict,
                RegionFiasId = data.RegionFiasId,
                RegionKladrId = data.RegionKladrId,
                RegionIsoCode = data.RegionIsoCode,
                RegionWithType = data.RegionWithType,
                RegionType = data.RegionType,
                RegionTypeFull = data.RegionTypeFull,
                Region = data.Region,
                AreaFiasId = data.AreaFiasId,
                AreaKladrId = data.AreaKladrId,
                AreaWithType = data.AreaWithType,
                AreaType = data.AreaType,
                AreaTypeFull = data.AreaTypeFull,
                Area = data.Area,
                CityFiasId = data.CityFiasId,
                CityKladrId = data.CityKladrId,
                CityWithType = data.CityWithType,
                CityType = data.CityType,
                CityTypeFull = data.CityTypeFull,
                City = data.City,
                CityArea = data.CityArea,
                CityDistrictFiasId = data.CityDistrictFiasId,
                CityDistrictKladrId = data.CityDistrictKladrId,
                CityDistrictWithType = data.CityDistrictWithType,
                CityDistrictType = data.CityDistrictType,
                CityDistrictTypeFull = data.CityDistrictTypeFull,
                CityDistrict = data.CityDistrict,
                SettlementFiasId = data.SettlementFiasId,
                SettlementKladrId = data.SettlementKladrId,
                SettlementWithType = data.SettlementWithType,
                SettlementType = data.SettlementType,
                SettlementTypeFull = data.SettlementTypeFull,
                Settlement = data.Settlement,
                StreetFiasId = data.StreetFiasId,
                StreetKladrId = data.StreetKladrId,
                StreetWithType = data.StreetWithType,
                StreetType = data.StreetType,
                StreetTypeFull = data.StreetTypeFull,
                Street = data.Street,
                SteadFiasId = data.SteadFiasId,
                SteadKladrId = data.SteadKladrId,
                SteadCadnum = data.SteadCadnum,
                SteadType = data.SteadType,
                SteadTypeFull = data.SteadTypeFull,
                Stead = data.Stead,
                HouseFiasId = data.HouseFiasId,
                HouseKladrId = data.HouseKladrId,
                HouseCadnum = data.HouseCadnum,
                HouseFlatCount = data.HouseFlatCount,
                HouseType = data.HouseType,
                HouseTypeFull = data.HouseTypeFull,
                House = data.House,
                BlockType = data.BlockType,
                BlockTypeFull = data.BlockTypeFull,
                Block = data.Block,
                Entrance = data.Entrance,
                Floor = data.Floor,
                FlatFiasId = data.FlatFiasId,
                FlatCadnum = data.FlatCadnum,
                FlatType = data.FlatType,
                FlatTypeFull = data.FlatTypeFull,
                Flat = data.Flat,
                FlatArea = data.FlatArea,
                SquareMeterPrice = data.SquareMeterPrice,
                FlatPrice = data.FlatPrice,
                PostalBox = data.PostalBox,
                RoomFiasId = data.RoomFiasId,
                RoomCadnum = data.RoomCadnum,
                RoomType = data.RoomType,
                RoomTypeFull = data.RoomTypeFull,
                Room = data.Room,
                FiasId = data.FiasId,
                FiasCode = data.FiasCode,
                FiasLevel = data.FiasLevel,
                FiasActualityState = data.FiasActualityState,
                KladrId = data.KladrId,
                GeonameId = data.GeonameId,
                CapitalMarker = data.CapitalMarker,
                Okato = data.Okato,
                Oktmo = data.Oktmo,
                TaxOffice = data.TaxOffice,
                TaxOfficeLegal = data.TaxOfficeLegal,
                Timezone = data.Timezone,
                GeoLat = data.GeoLat,
                GeoLon = data.GeoLon,
                BeltwayHit = data.BeltwayHit,
                BeltwayDistance = data.BeltwayDistance,
                Metro = data.Metro,
                Divisions = data.Divisions,
                QcGeo = data.QcGeo,
                QcComplete = data.QcComplete,
                QcHouse = data.QcHouse,
                HistoryValues = data.HistoryValues,
                UnparsedParts = data.UnparsedParts,
                Qc = data.Qc
            };
        }

        private static DadataAddressDataDto CreateAddressDataStub(string extracted, string normalizedAddress, string houseFiasId)
        {
            return new DadataAddressDataDto
            {
                Source = extracted,
                Result = normalizedAddress,
                PostalCode = null,
                Country = "Россия",
                CountryIsoCode = "RU",
                FederalDistrict = null,
                RegionFiasId = null,
                RegionKladrId = null,
                RegionIsoCode = null,
                RegionWithType = null,
                RegionType = null,
                RegionTypeFull = null,
                Region = null,
                AreaFiasId = null,
                AreaKladrId = null,
                AreaWithType = null,
                AreaType = null,
                AreaTypeFull = null,
                Area = null,
                CityFiasId = null,
                CityKladrId = null,
                CityWithType = null,
                CityType = null,
                CityTypeFull = null,
                City = null,
                CityArea = null,
                CityDistrictFiasId = null,
                CityDistrictKladrId = null,
                CityDistrictWithType = null,
                CityDistrictType = null,
                CityDistrictTypeFull = null,
                CityDistrict = null,
                SettlementFiasId = null,
                SettlementKladrId = null,
                SettlementWithType = null,
                SettlementType = null,
                SettlementTypeFull = null,
                Settlement = null,
                StreetFiasId = null,
                StreetKladrId = null,
                StreetWithType = null,
                StreetType = null,
                StreetTypeFull = null,
                Street = null,
                SteadFiasId = null,
                SteadKladrId = null,
                SteadCadnum = null,
                SteadType = null,
                SteadTypeFull = null,
                Stead = null,
                HouseFiasId = houseFiasId,
                HouseKladrId = null,
                HouseCadnum = null,
                HouseFlatCount = null,
                HouseType = null,
                HouseTypeFull = null,
                House = null,
                BlockType = null,
                BlockTypeFull = null,
                Block = null,
                Entrance = null,
                Floor = null,
                FlatFiasId = null,
                FlatCadnum = null,
                FlatType = null,
                FlatTypeFull = null,
                Flat = null,
                FlatArea = null,
                SquareMeterPrice = null,
                FlatPrice = null,
                PostalBox = null,
                RoomFiasId = null,
                RoomCadnum = null,
                RoomType = null,
                RoomTypeFull = null,
                Room = null,
                FiasId = null,
                FiasCode = null,
                FiasLevel = null,
                FiasActualityState = null,
                KladrId = null,
                GeonameId = null,
                CapitalMarker = null,
                Okato = null,
                Oktmo = null,
                TaxOffice = null,
                TaxOfficeLegal = null,
                Timezone = null,
                GeoLat = null,
                GeoLon = null,
                BeltwayHit = null,
                BeltwayDistance = null,
                Metro = null,
                Divisions = null,
                QcGeo = null,
                QcComplete = null,
                QcHouse = null,
                HistoryValues = null,
                UnparsedParts = null,
                Qc = null
            };
        }

        private (BuildingUnitLocation Location, string Canonical, string Hash) NormalizeUnitCore(string source)
        {
            var location = _unitParser.Parse(source);
            var canonical = _unitCanonicalizer.ToCanonical(location);
            var hash = _canonicalHash.ComputeSha256(canonical);
            return (location, canonical, hash);
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
