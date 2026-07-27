using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using VTBL.AddressNormalizer.WebApi.Options;
using VTBL.AddressNormalizer.WebApi.Services;

namespace VTBL.AddressNormalizer.WebApi.Health
{
    /// <summary>
    /// Readiness-проверка: базовая валидация конфигурации и синтетический вызов сервиса.
    /// </summary>
    public sealed class AddressNormalizerReadinessHealthCheck : IHealthCheck
    {
        private readonly IAddressNormalizationService _normalizationService;
        private readonly BatchOptions _batchOptions;

        public AddressNormalizerReadinessHealthCheck(
            IAddressNormalizationService normalizationService,
            IOptions<BatchOptions> batchOptions)
        {
            _normalizationService = normalizationService ?? throw new ArgumentNullException(nameof(normalizationService));
            _batchOptions = batchOptions?.Value ?? throw new ArgumentNullException(nameof(batchOptions));
        }

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            if (_batchOptions.MaxItems <= 0)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    "Некорректная конфигурация Batch:MaxItems. Значение должно быть > 0."));
            }

            try
            {
                var unit = _normalizationService.NormalizeUnit("КВ 1");
                if (unit?.IndoorValue == null || string.IsNullOrWhiteSpace(unit.Hash))
                {
                    return Task.FromResult(HealthCheckResult.Unhealthy(
                        "Сервис нормализации вернул неполный результат на синтетическом запросе."));
                }

                return Task.FromResult(HealthCheckResult.Healthy(
                    "Нормализация и конфигурация доступны."));
            }
            catch (Exception ex)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    "Сбой readiness-проверки нормализации.",
                    ex));
            }
        }
    }
}
