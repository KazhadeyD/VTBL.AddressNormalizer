using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VTBL.AddressNormalizer.Infrastructure.Composition;
using VTBL.AddressNormalizer.WebApi.Filters;
using VTBL.AddressNormalizer.WebApi.Health;
using VTBL.AddressNormalizer.WebApi.Logging;
using VTBL.AddressNormalizer.WebApi.Middleware;
using VTBL.AddressNormalizer.WebApi.Options;
using VTBL.AddressNormalizer.WebApi.Services;
using VTBL.AddressNormalizer.WebApi.Services.Dadata;
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
            // Options
            services.Configure<BatchOptions>(Configuration.GetSection("Batch"));

            // MVC
            services.AddControllers(options =>
            {
                options.Filters.Add<ApiExceptionFilter>();
            });

            // OpenAPI
            services.AddWebApiSwagger();

            // Domain
            services.AddAddressNormalizerLogging();
            services.AddAddressNormalizer();
            services.AddSingleton<IDadataService, DadataService>();
            services.AddSingleton<IAddressNormalizationService, AddressNormalizationService>();

            // Health
            services.AddWebApiHealthChecks();
        }

        /// <summary>
        /// HTTP pipeline: middleware, routing, endpoints.
        /// </summary>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "VTBL.AddressNormalizer.WebApi v1"));
            }

            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseMiddleware<RequestLoggingMiddleware>();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapWebApiHealthChecks();
            });
        }
    }
}
