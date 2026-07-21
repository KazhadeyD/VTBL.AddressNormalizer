using System;
using VTBL.AddressNormalizer.WebApi.Middleware;
using Xunit;

namespace VTBL.AddressNormalizer.UnitTests.WebApi
{
    /// <summary>
    /// Модульные тесты алгоритма выбора Correlation Id.
    /// </summary>
    public class CorrelationIdResolverTests
    {
        [Fact]
        public void Resolve_CorrelationPresent_TakesCorrelationOverRequest()
        {
            var id = CorrelationIdResolver.Resolve("corr-1", "req-1");
            Assert.Equal("corr-1", id);
        }

        [Fact]
        public void Resolve_OnlyRequest_TakesRequest()
        {
            var id = CorrelationIdResolver.Resolve(null, "req-1");
            Assert.Equal("req-1", id);
        }

        [Fact]
        public void Resolve_EmptyCorrelation_FallsBackToRequest()
        {
            var id = CorrelationIdResolver.Resolve(string.Empty, "req-1");
            Assert.Equal("req-1", id);
        }

        [Fact]
        public void Resolve_WhitespaceCorrelation_FallsBackToRequest()
        {
            var id = CorrelationIdResolver.Resolve("   ", "req-1");
            Assert.Equal("req-1", id);
        }

        [Fact]
        public void Resolve_WhitespaceRequest_GeneratesGuid()
        {
            var id = CorrelationIdResolver.Resolve(null, "  \t  ");
            Assert.True(Guid.TryParseExact(id, "D", out _));
        }

        [Fact]
        public void Resolve_BothAbsent_GeneratesGuid()
        {
            var id = CorrelationIdResolver.Resolve(null, null);
            Assert.True(Guid.TryParseExact(id, "D", out _));
        }

        [Fact]
        public void Resolve_BothEmpty_GeneratesGuid()
        {
            var id = CorrelationIdResolver.Resolve(string.Empty, string.Empty);
            Assert.True(Guid.TryParseExact(id, "D", out _));
        }
    }
}
