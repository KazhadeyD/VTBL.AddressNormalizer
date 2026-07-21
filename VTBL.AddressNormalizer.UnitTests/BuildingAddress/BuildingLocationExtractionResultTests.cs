using VTBL.AddressNormalizer.Abstractions.BuildingAddress;
using Xunit;

namespace VTBL.AddressNormalizer.UnitTests.BuildingAddress
{
    public class BuildingLocationExtractionResultTests
    {
        [Fact]
        public void Constructor_NullArguments_NormalizedToEmptyStrings()
        {
            var result = new BuildingLocationExtractionResult(null, null);

            Assert.Equal(string.Empty, result.Outdoor);
            Assert.Equal(string.Empty, result.Indoor);
        }

        [Fact]
        public void Constructor_PreservesNonNullValues()
        {
            var result = new BuildingLocationExtractionResult("outdoor", "indoor");

            Assert.Equal("outdoor", result.Outdoor);
            Assert.Equal("indoor", result.Indoor);
        }
    }
}
