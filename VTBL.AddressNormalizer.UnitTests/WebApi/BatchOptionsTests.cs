using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using VTBL.AddressNormalizer.WebApi.Options;
using Xunit;

namespace VTBL.AddressNormalizer.UnitTests.WebApi
{
    /// <summary>
    /// Тесты options batch-лимита.
    /// </summary>
    public class BatchOptionsTests
    {
        [Fact]
        public void MaxItems_DefaultConstructor_Is100()
        {
            var options = new BatchOptions();

            Assert.Equal(100, options.MaxItems);
        }

        [Fact]
        public void MaxItems_BindFromEmptySection_Remains100()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>())
                .Build();

            var options = new BatchOptions();
            configuration.GetSection("Batch").Bind(options);

            Assert.Equal(100, options.MaxItems);
        }

        [Fact]
        public void MaxItems_BindFromAppsettingsStyle_Is100()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Batch:MaxItems"] = "100"
                })
                .Build();

            var options = new BatchOptions();
            configuration.GetSection("Batch").Bind(options);

            Assert.Equal(100, options.MaxItems);
        }
    }
}
