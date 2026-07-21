using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VTBL.AddressNormalizer.WebApi.Middleware;
using Xunit;

namespace VTBL.AddressNormalizer.UnitTests.WebApi
{
    /// <summary>
    /// Unit-тесты <see cref="RequestLoggingMiddleware"/>.
    /// </summary>
    public class RequestLoggingMiddlewareTests
    {
        [Fact]
        public async Task Invoke_OkResponse_LogsInformationWithStatus200()
        {
            var logger = new CapturingLogger();
            var middleware = new RequestLoggingMiddleware(
                _ =>
                {
                    _.Response.StatusCode = StatusCodes.Status200OK;
                    return Task.CompletedTask;
                },
                logger);

            var context = new DefaultHttpContext();
            context.Request.Method = "POST";
            context.Request.Path = "/api/v1/normalize";

            await middleware.InvokeAsync(context);

            Assert.Single(logger.Entries);
            Assert.Equal(LogLevel.Information, logger.Entries[0].Level);
            Assert.Contains("200", logger.Entries[0].Message);
            Assert.Contains("/api/v1/normalize", logger.Entries[0].Message);
        }

        [Fact]
        public async Task Invoke_BadRequest_LogsWarningWithStatus400()
        {
            var logger = new CapturingLogger();
            var middleware = new RequestLoggingMiddleware(
                _ =>
                {
                    _.Response.StatusCode = StatusCodes.Status400BadRequest;
                    return Task.CompletedTask;
                },
                logger);

            var context = new DefaultHttpContext();
            context.Request.Method = "POST";
            context.Request.Path = "/api/v1/normalize";

            await middleware.InvokeAsync(context);

            Assert.Single(logger.Entries);
            Assert.Equal(LogLevel.Warning, logger.Entries[0].Level);
            Assert.Contains("400", logger.Entries[0].Message);
        }

        [Fact]
        public async Task Invoke_Health_DoesNotLog()
        {
            var logger = new CapturingLogger();
            var middleware = new RequestLoggingMiddleware(
                _ =>
                {
                    _.Response.StatusCode = StatusCodes.Status200OK;
                    return Task.CompletedTask;
                },
                logger);

            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = "/health";

            await middleware.InvokeAsync(context);

            Assert.Empty(logger.Entries);
        }

        private sealed class CapturingLogger : ILogger<RequestLoggingMiddleware>
        {
            public List<LogEntry> Entries { get; } = new List<LogEntry>();

            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception exception,
                Func<TState, Exception, string> formatter)
            {
                Entries.Add(new LogEntry
                {
                    Level = logLevel,
                    Message = formatter(state, exception)
                });
            }
        }

        private sealed class LogEntry
        {
            public LogLevel Level { get; set; }
            public string Message { get; set; }
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new NullScope();
            public void Dispose()
            {
            }
        }
    }
}
