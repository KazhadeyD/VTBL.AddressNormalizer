using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace VTBL.AddressNormalizer.WebApi.Health
{
    /// <summary>
    /// JSON-ответ health endpoint с итоговым статусом и деталями по checks.
    /// </summary>
    public static class HealthCheckResponseWriter
    {
        private static readonly DateTimeOffset StartedAtUtc = DateTimeOffset.UtcNow;
        private static readonly string AssemblyVersion = ResolveAssemblyVersion();

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static Task WriteAsync(HttpContext context, HealthReport report)
        {
            context.Response.ContentType = "application/json; charset=utf-8";

            var payload = new
            {
                status = report.Status.ToString(),
                service = new
                {
                    assemblyVersion = AssemblyVersion,
                    startedAtUtc = StartedAtUtc,
                    uptimeMs = (DateTimeOffset.UtcNow - StartedAtUtc).TotalMilliseconds
                },
                totalDurationMs = report.TotalDuration.TotalMilliseconds,
                checks = report.Entries.ToDictionary(
                    entry => entry.Key,
                    entry => new
                    {
                        status = entry.Value.Status.ToString(),
                        description = entry.Value.Description,
                        durationMs = entry.Value.Duration.TotalMilliseconds,
                        error = entry.Value.Exception?.Message
                    })
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
        }

        private static string ResolveAssemblyVersion()
        {
            var assembly = typeof(HealthCheckResponseWriter).Assembly;
            return assembly.GetName().Version?.ToString()
                ?? assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version
                ?? "unknown";
        }
    }
}
