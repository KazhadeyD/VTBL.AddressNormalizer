using System.Linq;
using VTBL.AddressNormalizer.Abstractions.Shared;
using VTBL.AddressNormalizer.Infrastructure.Shared;
using Xunit;

namespace VTBL.AddressNormalizer.UnitTests.BuildingAddress
{
    /// <summary>
    /// Контракт синхронизации <see cref="IndoorMarkerPatterns"/> с набором indoor-маркеров ядра.
    /// </summary>
    public class IndoorMarkerPatternsContractTests
    {
        [Fact]
        public void All_ContainsFourteenMarkers()
        {
            Assert.Equal(14, IndoorMarkerPatterns.All.Count);
        }

        [Fact]
        public void All_ContainsExpectedKinds()
        {
            var kinds = IndoorMarkerPatterns.All.Select(x => x.Kind).Distinct().ToList();

            Assert.Contains(IndoorMarkerKind.Apartment, kinds);
            Assert.Contains(IndoorMarkerKind.Office, kinds);
            Assert.Contains(IndoorMarkerKind.Premise, kinds);
            Assert.Contains(IndoorMarkerKind.Room, kinds);
            Assert.Contains(IndoorMarkerKind.Cabinet, kinds);
            Assert.Contains(IndoorMarkerKind.Floor, kinds);
            Assert.Contains(IndoorMarkerKind.Entrance, kinds);
            Assert.Contains(IndoorMarkerKind.Passage, kinds);
            Assert.Contains(IndoorMarkerKind.Holding, kinds);
            Assert.Contains(IndoorMarkerKind.Block, kinds);
            Assert.Contains(IndoorMarkerKind.Section, kinds);
            Assert.Contains(IndoorMarkerKind.Workplace, kinds);
            Assert.Contains(IndoorMarkerKind.Part, kinds);
            Assert.Contains(IndoorMarkerKind.Mailbox, kinds);
        }

        [Theory]
        [InlineData("КВ. 10")]
        [InlineData("КВАРТИРА 5")]
        public void ApartmentPattern_MatchesKvForms(string input)
        {
            Assert.Matches(IndoorMarkerPatterns.Apartment, input);
        }

        [Fact]
        public void PremisePattern_DoesNotMatchPoselok()
        {
            Assert.DoesNotMatch(IndoorMarkerPatterns.Premise, "п Московский");
        }
    }
}
