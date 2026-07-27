using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using VTBL.AddressNormalizer.Infrastructure.Composition;
using VTBL.AddressNormalizer.WebApi.Logging;
using VTBL.AddressNormalizer.WebApi.Filters;
using VTBL.AddressNormalizer.WebApi.Health;
using VTBL.AddressNormalizer.WebApi.Middleware;
using VTBL.AddressNormalizer.WebApi.Options;
using VTBL.AddressNormalizer.WebApi.Services;
using VTBL.AddressNormalizer.WebApi.Swagger;

namespace VTBL.AddressNormalizer.WebApi
{
    /// <summary>
    /// Конфигурация DI и HTTP pipeline WebApi.
    /// </summary>
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        /// <summary>
        /// Регистрация сервисов, options, фильтров и Swagger.
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<BatchOptions>(Configuration.GetSection("Batch"));

            services.AddControllers(options =>
            {
                options.Filters.Add<ApiExceptionFilter>();
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "VTBL.AddressNormalizer.WebApi",
                    Version = "v1",
                    Description =
                        "HTTP API нормализации адресов поверх ядра AddressNormalizer.\n\n" +
                        "**Auth:** отсутствует.\n\n" +
                        "**Request ID:** передайте `X-VTBL-Request-ID`; иначе сервер сгенерирует GUID. " +
                        "Значение пишется в response header и в NLog.\n\n" +
                        "**Валидация:** пустая / null / whitespace строка `source` → HTTP 400.\n\n" +
                        "**Batch:** per-item ошибки не останавливают обработку; если упали все элементы — одна ошибка (400/500)."
                });

                var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
                if (File.Exists(xmlPath))
                    c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

                c.OperationFilter<SwaggerExamplesOperationFilter>();
            });

            services.AddAddressNormalizerLogging();
            services.AddAddressNormalizer();
            services.AddSingleton<IAddressNormalizationService, AddressNormalizationService>();
            services
                .AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy("HTTP host запущен."), tags: new[] { "live" })
                .AddCheck<AddressNormalizerReadinessHealthCheck>(
                    "address_normalizer_readiness",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { "ready" });
        }

        /// <summary>
        /// HTTP pipeline: Correlation middleware, routing, endpoints.
        /// </summary>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseMiddleware<RequestLoggingMiddleware>();

            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "VTBL.AddressNormalizer.WebApi v1"));
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = _ => true,
                    ResponseWriter = HealthCheckResponseWriter.WriteAsync
                });
                endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains("live"),
                    ResponseWriter = HealthCheckResponseWriter.WriteAsync
                });
            });
        }
    }
}
