using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging.Abstractions;
using VTBL.AddressNormalizer.WebApi.Mapping;
using VTBL.AddressNormalizer.WebApi.Models;
using VTBL.AddressNormalizer.WebApi.Services;
using Xunit;

namespace VTBL.AddressNormalizer.UnitTests.WebApi
{
    /// <summary>
    /// Unit-тесты AddressNormalizationService (реальная оркестрация).
    /// </summary>
    public class AddressNormalizationServiceTests
    {
        private const string SampleWithIndoor = "г Москва, ул Сухонская, д 11, кв 89";
        private const string SampleOutdoorOnly = "г Москва, ул Сухонская, д 11";

        private readonly AddressNormalizationService _sut = CreateSut();

        private static AddressNormalizationService CreateSut() =>
            new AddressNormalizationService(
                NullLogger<AddressNormalizationService>.Instance,
                AddressNormalizerTestHost.BuildingLocationExtractor,
                AddressNormalizerTestHost.BuildingAddressCanonicalizer,
                AddressNormalizerTestHost.Parser,
                AddressNormalizerTestHost.Canonicalizer,
                AddressNormalizerTestHost.Hash);

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void NormalizeFull_InvalidSource_ThrowsArgumentException(string source)
        {
            var ex = Assert.Throws<ArgumentException>(() => _sut.NormalizeFull(source));
            Assert.Equal("source", ex.ParamName);
        }

        [Fact]
        public void NormalizeFull_WithIndoor_ReturnsExtractedCanonicalHashAndApartments()
        {
            var value = _sut.NormalizeFull(SampleWithIndoor);

            var split = AddressNormalizerTestHost.BuildingLocationExtractor.ExtractSplit(SampleWithIndoor);
            var outdoorCanonical = AddressNormalizerTestHost.BuildingAddressCanonicalizer.ToCanonical(split.Outdoor);
            var expectedHash = AddressNormalizerTestHost.Hash.ComputeSha256(outdoorCanonical);

            Assert.Equal("suggest-house-fias-id", value.BuildingValue.FiasId);
            Assert.NotNull(value.BuildingValue.Suggest);
            Assert.NotNull(value.BuildingValue.Clean);
            Assert.Equal(split.Outdoor, value.BuildingValue.Extracted);
            Assert.Equal(outdoorCanonical, value.BuildingValue.NormalizedAddress);
            Assert.Equal(expectedHash, value.BuildingValue.Hash);
            var unitLocation = AddressNormalizerTestHost.Parser.Parse(split.Indoor);
            var unitCanonical = AddressNormalizerTestHost.Canonicalizer.ToCanonical(unitLocation);
            var unitHash = AddressNormalizerTestHost.Hash.ComputeSha256(unitCanonical);
            Assert.Equal(split.Indoor, value.IndoorValue.Extracted);
            Assert.Equal(unitHash, value.IndoorValue.Hash);
            Assert.Contains("89", IndoorValueTestHelper.GetMarkValues(value.IndoorValue, IndoorValueMapper.MarkIds.Apartment));
            Assert.Equal("квартира", IndoorValueTestHelper.GetMark(value.IndoorValue, IndoorValueMapper.MarkIds.Apartment).Name);
        }

        [Fact]
        public void NormalizeFull_WithoutIndoor_ReturnsEmptyUnits()
        {
            var value = _sut.NormalizeFull(SampleOutdoorOnly);
            var split = AddressNormalizerTestHost.BuildingLocationExtractor.ExtractSplit(SampleOutdoorOnly);

            Assert.Equal(split.Indoor, value.IndoorValue.Extracted);
            Assert.NotNull(value.IndoorValue.Units);
            Assert.Empty(value.IndoorValue.Units);
        }

        [Fact]
        public void NormalizeUnit_ShortIndoor_MatchesFactory()
        {
            const string source = "кв 10";

            var result = _sut.NormalizeUnit(source);
            var location = AddressNormalizerTestHost.Parser.Parse(source);
            var expectedCanonical = AddressNormalizerTestHost.Canonicalizer.ToCanonical(location);
            var expectedHash = AddressNormalizerTestHost.Hash.ComputeSha256(expectedCanonical);

            Assert.Equal(expectedCanonical, result.Canonical);
            Assert.Equal(expectedHash, result.Hash);
            Assert.Equal(source, result.IndoorValue.Extracted);
            Assert.Equal(expectedHash, result.IndoorValue.Hash);
            Assert.Contains("10", IndoorValueTestHelper.GetMarkValues(result.IndoorValue, IndoorValueMapper.MarkIds.Apartment));
        }

        [Fact]
        public void ResolveBuildingFiasId_WhenSuggestHasHouseFiasId_PrefersSuggest()
        {
            var suggest = new DadataSuggestAddressDto
            {
                Suggestions = new[]
                {
                    new DadataSuggestAddressSuggestionDto
                    {
                        Data = new DadataAddressDataDto
                        {
                            HouseFiasId = "suggest-id"
                        }
                    }
                }
            };

            var clean = new DadataCleanAddressDto
            {
                HouseFiasId = "clean-id"
            };

            Assert.Equal("suggest-id", InvokeResolveBuildingFiasId(suggest, clean));
        }

        [Fact]
        public void ResolveBuildingFiasId_WhenSuggestMissing_FallsBackToClean()
        {
            var suggest = new DadataSuggestAddressDto
            {
                Suggestions = new[]
                {
                    new DadataSuggestAddressSuggestionDto
                    {
                        Data = new DadataAddressDataDto()
                    }
                }
            };

            var clean = new DadataCleanAddressDto
            {
                HouseFiasId = "clean-id"
            };

            Assert.Equal("clean-id", InvokeResolveBuildingFiasId(suggest, clean));
        }

        [Fact]
        public void ExtractOutdoor_MatchesExtractSplitOutdoor()
        {
            var expected = AddressNormalizerTestHost.BuildingLocationExtractor.ExtractSplit(SampleWithIndoor).Outdoor;

            Assert.Equal(expected, _sut.ExtractOutdoor(SampleWithIndoor));
        }

        [Fact]
        public void Canonicalize_WithIndoor_DoesNotExtract_MatchesToCanonicalOfSource()
        {
            var expected = AddressNormalizerTestHost.BuildingAddressCanonicalizer.ToCanonical(SampleWithIndoor);

            Assert.Equal(expected, _sut.Canonicalize(SampleWithIndoor));
        }

        private static string InvokeResolveBuildingFiasId(DadataSuggestAddressDto suggest, DadataCleanAddressDto clean)
        {
            var method = typeof(AddressNormalizationService).GetMethod(
                "ResolveBuildingFiasId",
                BindingFlags.Static | BindingFlags.NonPublic);

            Assert.NotNull(method);
            return (string)method.Invoke(null, new object[] { suggest, clean });
        }

        [Fact]
        public void NormalizeBatch_EmptyList_ReturnsRequestInvalid()
        {
            var outcome = _sut.NormalizeBatch(Array.Empty<string>(), maxItems: 100);

            Assert.Equal(BatchOutcomeKind.RequestInvalid, outcome.Kind);
            Assert.False(string.IsNullOrWhiteSpace(outcome.ErrorMessage));
        }

        [Fact]
        public void NormalizeBatch_NullList_ReturnsRequestInvalid()
        {
            var outcome = _sut.NormalizeBatch(null, maxItems: 100);

            Assert.Equal(BatchOutcomeKind.RequestInvalid, outcome.Kind);
        }

        [Fact]
        public void NormalizeBatch_OverLimit_ReturnsRequestInvalid()
        {
            var sources = new List<string>();
            for (var i = 0; i < 101; i++)
                sources.Add("addr " + i);

            var outcome = _sut.NormalizeBatch(sources, maxItems: 100);

            Assert.Equal(BatchOutcomeKind.RequestInvalid, outcome.Kind);
        }

        [Fact]
        public void NormalizeBatch_ExactlyMaxItems_ReturnsPartialOrSuccess()
        {
            var sources = new List<string>(100);
            for (var i = 0; i < 100; i++)
                sources.Add(SampleOutdoorOnly + ", кв " + (i + 1));

            var outcome = _sut.NormalizeBatch(sources, maxItems: 100);

            Assert.Equal(BatchOutcomeKind.PartialOrSuccess, outcome.Kind);
            Assert.Equal(100, outcome.Items.Count);
            Assert.All(outcome.Items, item => Assert.Equal("ok", item.Status));
        }

        [Fact]
        public void NormalizeBatch_AllInvalid_ReturnsAllFailValidation()
        {
            var outcome = _sut.NormalizeBatch(new[] { "", "  ", null }, maxItems: 100);

            Assert.Equal(BatchOutcomeKind.AllFailValidation, outcome.Kind);
            Assert.Equal(3, outcome.Items.Count);
            Assert.All(outcome.Items, item => Assert.Equal("error", item.Status));
            Assert.Equal(string.Empty, outcome.Items[2].Source);
        }

        [Fact]
        public void NormalizeBatch_NullSourceItem_UsesEmptyString()
        {
            var outcome = _sut.NormalizeBatch(new[] { SampleOutdoorOnly, null }, maxItems: 100);

            Assert.Equal(BatchOutcomeKind.PartialOrSuccess, outcome.Kind);
            Assert.Equal(string.Empty, outcome.Items[1].Source);
            Assert.Equal("error", outcome.Items[1].Status);
        }

        [Fact]
        public void NormalizeBatch_PreservesInputOrder()
        {
            var sources = new[] { SampleWithIndoor, "  ", SampleOutdoorOnly };

            var outcome = _sut.NormalizeBatch(sources, maxItems: 100);

            Assert.Equal(BatchOutcomeKind.PartialOrSuccess, outcome.Kind);
            Assert.Equal(3, outcome.Items.Count);
            Assert.Equal(SampleWithIndoor, outcome.Items[0].Source);
            Assert.Equal("  ", outcome.Items[1].Source);
            Assert.Equal(SampleOutdoorOnly, outcome.Items[2].Source);
            Assert.Equal(new[] { "ok", "error", "ok" }, outcome.Items.Select(i => i.Status).ToArray());
        }

        [Fact]
        public void NormalizeBatch_Mixed_ReturnsPartialOrSuccessWithRealValue()
        {
            var outcome = _sut.NormalizeBatch(new[] { SampleWithIndoor, "  " }, maxItems: 100);

            Assert.Equal(BatchOutcomeKind.PartialOrSuccess, outcome.Kind);
            Assert.Equal(2, outcome.Items.Count);
            Assert.Equal("ok", outcome.Items[0].Status);
            Assert.Equal("error", outcome.Items[1].Status);

            var expected = _sut.NormalizeFull(SampleWithIndoor);
            Assert.Equal(expected.BuildingValue.Hash, outcome.Items[0].Value.BuildingValue.Hash);
            Assert.Equal(expected.BuildingValue.Extracted, outcome.Items[0].Value.BuildingValue.Extracted);
            Assert.Contains("89", IndoorValueTestHelper.GetMarkValues(outcome.Items[0].Value.IndoorValue, IndoorValueMapper.MarkIds.Apartment));
        }

        [Fact]
        public void NormalizeBatch_AllCoreExceptions_ReturnsAllFailException()
        {
            var sut = new ThrowingCoreService();

            var outcome = sut.NormalizeBatch(new[] { "a", "b" }, maxItems: 100);

            Assert.Equal(BatchOutcomeKind.AllFailException, outcome.Kind);
            Assert.Equal(2, outcome.Items.Count);
            Assert.All(outcome.Items, item =>
            {
                Assert.Equal("error", item.Status);
                Assert.Equal(ThrowingCoreService.ErrorMessage, item.Error);
            });
            Assert.False(string.IsNullOrWhiteSpace(outcome.ErrorMessage));
        }

        [Fact]
        public void NormalizeBatch_MixedValidationAndException_ReturnsAllFailMixed()
        {
            var sut = new ThrowingCoreService();

            var outcome = sut.NormalizeBatch(new[] { "a", "  " }, maxItems: 100);

            Assert.Equal(BatchOutcomeKind.AllFailMixed, outcome.Kind);
            Assert.Equal(2, outcome.Items.Count);
            Assert.Equal("error", outcome.Items[0].Status);
            Assert.Equal(ThrowingCoreService.ErrorMessage, outcome.Items[0].Error);
            Assert.Equal("error", outcome.Items[1].Status);
            Assert.False(string.IsNullOrWhiteSpace(outcome.ErrorMessage));
        }

        private sealed class ThrowingCoreService : AddressNormalizationService
        {
            public const string ErrorMessage = "intentional core failure";

            public ThrowingCoreService()
                : base(
                    NullLogger<AddressNormalizationService>.Instance,
                    AddressNormalizerTestHost.BuildingLocationExtractor,
                    AddressNormalizerTestHost.BuildingAddressCanonicalizer,
                    AddressNormalizerTestHost.Parser,
                    AddressNormalizerTestHost.Canonicalizer,
                    AddressNormalizerTestHost.Hash)
            {
            }

            protected override NormalizeValueDto NormalizeFullCore(string source) =>
                throw new InvalidOperationException(ErrorMessage);
        }
    }
}
