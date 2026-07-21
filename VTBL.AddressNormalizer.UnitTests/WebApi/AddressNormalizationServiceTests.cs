using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using VTBL.AddressNormalizer.Infrastructure.Composition;
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

        private readonly AddressNormalizationService _sut =
            new AddressNormalizationService(NullLogger<AddressNormalizationService>.Instance);

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

            var split = AddressNormalizerFactory.BuildingLocationExtractor.ExtractSplit(SampleWithIndoor);
            var outdoorCanonical = AddressNormalizerFactory.BuildingAddressCanonicalizer.ToCanonical(split.Outdoor);
            var expectedHash = AddressNormalizerFactory.CanonicalHash.ComputeSha256(outdoorCanonical);

            Assert.Null(value.FiasId);
            Assert.Equal(split.Outdoor, value.DadataOutdoor.Extracted);
            Assert.Equal(outdoorCanonical, value.DadataOutdoor.OutdoorCanonical);
            Assert.Equal(expectedHash, value.DadataOutdoor.Hash);
            Assert.Contains("89", value.IndoorValue.Apartments.Values);
            Assert.Equal("квартира", value.IndoorValue.Apartments.Name);
        }

        [Fact]
        public void NormalizeFull_WithoutIndoor_AllIndoorValuesEmpty()
        {
            var value = _sut.NormalizeFull(SampleOutdoorOnly);

            Assert.All(
                new[]
                {
                    value.IndoorValue.Floors.Values,
                    value.IndoorValue.Premises.Values,
                    value.IndoorValue.Rooms.Values,
                    value.IndoorValue.Offices.Values,
                    value.IndoorValue.Workplaces.Values,
                    value.IndoorValue.Parts.Values,
                    value.IndoorValue.Apartments.Values,
                    value.IndoorValue.Cabinets.Values,
                    value.IndoorValue.Entrances.Values,
                    value.IndoorValue.Blocks.Values,
                    value.IndoorValue.Sections.Values,
                    value.IndoorValue.Mailboxes.Values,
                    value.IndoorValue.Literas.Values,
                    value.IndoorValue.Ranges.Values,
                    value.IndoorValue.RawCodes.Values,
                    value.IndoorValue.Notes.Values,
                    value.IndoorValue.Unparsed.Values
                },
                values => Assert.Empty(values));
        }

        [Fact]
        public void NormalizeUnit_ShortIndoor_MatchesFactory()
        {
            const string source = "кв 10";

            var result = _sut.NormalizeUnit(source);
            var expected = AddressNormalizerFactory.BuildingUnitNormalizer.Normalize(source);

            Assert.Equal(expected.Canonical, result.Canonical);
            Assert.Equal(expected.Hash, result.Hash);
            Assert.Contains("10", result.IndoorValue.Apartments.Values);
        }

        [Fact]
        public void ExtractOutdoor_MatchesExtractSplitOutdoor()
        {
            var expected = AddressNormalizerFactory.BuildingLocationExtractor.ExtractSplit(SampleWithIndoor).Outdoor;

            Assert.Equal(expected, _sut.ExtractOutdoor(SampleWithIndoor));
        }

        [Fact]
        public void Canonicalize_WithIndoor_DoesNotExtract_MatchesToCanonicalOfSource()
        {
            var expected = AddressNormalizerFactory.BuildingAddressCanonicalizer.ToCanonical(SampleWithIndoor);

            Assert.Equal(expected, _sut.Canonicalize(SampleWithIndoor));
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
            Assert.Equal(expected.DadataOutdoor.Hash, outcome.Items[0].Value.DadataOutdoor.Hash);
            Assert.Equal(expected.DadataOutdoor.Extracted, outcome.Items[0].Value.DadataOutdoor.Extracted);
            Assert.Contains("89", outcome.Items[0].Value.IndoorValue.Apartments.Values);
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
                : base(NullLogger<AddressNormalizationService>.Instance)
            {
            }

            protected override NormalizeValueDto NormalizeFullCore(string source) =>
                throw new InvalidOperationException(ErrorMessage);
        }
    }
}
