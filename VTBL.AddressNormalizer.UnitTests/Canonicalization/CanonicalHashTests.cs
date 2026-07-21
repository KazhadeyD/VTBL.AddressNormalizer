using VTBL.AddressNormalizer.Abstractions.Shared;
using VTBL.AddressNormalizer.UnitTests;
using Xunit;

namespace VTBL.AddressNormalizer.UnitTests.Canonicalization
{
    public class CanonicalHashTests
    {
        [Fact]
        public void ComputeSha256_KnownValue_MatchesExpected()
        {
            const string canonical = "эт:4|пом:2";
            const string expected = "d16038d40c6cb7c7b0c5e596227cabff014b400c32448cb5423919cf549da1b6";

            Assert.Equal(expected, AddressNormalizerTestHost.Hash.ComputeSha256(canonical));
        }

        [Fact]
        public void ComputeSha256_NullInput_ReturnsEmptyStringHash()
        {
            const string expected = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";

            Assert.Equal(expected, AddressNormalizerTestHost.Hash.ComputeSha256(null));
        }

        [Fact]
        public void ComputeSha256_SameInput_ReturnsSameHash()
        {
            const string canonical = "оф:18с";

            var first = AddressNormalizerTestHost.Hash.ComputeSha256(canonical);
            var second = AddressNormalizerTestHost.Hash.ComputeSha256(canonical);

            Assert.Equal(first, second);
        }
    }
}
