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
        public void Resolve_RequestIdPresent_ReturnsSameValue()
        {
            var id = CorrelationIdResolver.Resolve("req-1");
            Assert.Equal("req-1", id);
        }

        [Fact]
        public void Resolve_EmptyRequestId_GeneratesGuid()
        {
            var id = CorrelationIdResolver.Resolve(string.Empty);
            Assert.True(Guid.TryParseExact(id, "D", out _));
        }

        [Fact]
        public void Resolve_WhitespaceRequestId_GeneratesGuid()
        {
            var id = CorrelationIdResolver.Resolve("  \t  ");
            Assert.True(Guid.TryParseExact(id, "D", out _));
        }

        [Fact]
        public void Resolve_Absent_GeneratesGuid()
        {
            var id = CorrelationIdResolver.Resolve(null);
            Assert.True(Guid.TryParseExact(id, "D", out _));
        }
    }
}
