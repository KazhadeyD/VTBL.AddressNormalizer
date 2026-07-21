using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VTBL.AddressNormalizer.WebApi.Models;
using VTBL.AddressNormalizer.WebApi.Options;
using VTBL.AddressNormalizer.WebApi.Services;

namespace VTBL.AddressNormalizer.WebApi.Controllers
{
    /// <summary>
    /// Полная нормализация адресной строки: outdoor (extract + canonical + hash) и structured indoor.
    /// </summary>
    [ApiController]
    [Route("api/v1/normalize")]
    public class NormalizeController : ControllerBase
    {
        private readonly IAddressNormalizationService _service;
        private readonly BatchOptions _batchOptions;
        private readonly ILogger<NormalizeController> _logger;

        /// <summary>
        /// Создаёт контроллер normalize/batch.
        /// </summary>
        public NormalizeController(
            IAddressNormalizationService service,
            IOptions<BatchOptions> batchOptions,
            ILogger<NormalizeController> logger)
        {
            _service = service;
            _batchOptions = batchOptions.Value;
            _logger = logger;
        }

        /// <summary>
        /// Полная нормализация одного адреса.
        /// </summary>
        /// <remarks>
        /// Разбирает полную адресную строку:
        /// 1. extract outdoor / indoor через ядро (`ExtractSplit`);
        /// 2. канонизация outdoor + SHA256 → `dadataOutdoor`;
        /// 3. нормализация indoor через `IBuildingUnitNormalizer` → `indoorValue` (все категории с name/values);
        /// 4. `fiasId` в v1 всегда `null`.
        ///
        /// Пример запроса:
        ///
        ///     POST /api/v1/normalize
        ///     { "source": "г Москва, ул Сухонская, д 11, кв 89" }
        /// </remarks>
        /// <param name="request">Обёртка с полем <c>source</c> (непустая строка).</param>
        /// <response code="200">Успешная нормализация.</response>
        /// <response code="400">Тело отсутствует или <c>source</c> пустой / whitespace.</response>
        [HttpPost]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(NormalizeResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public ActionResult Normalize(
            [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] SourceRequest request)
        {
            _logger.LogInformation("NormalizeFull started");

            if (request == null)
                return BadRequest(new ErrorResponse { Error = "request body is required" });

            try
            {
                var value = _service.NormalizeFull(request.Source);
                return Ok(new NormalizeResponse
                {
                    Source = request.Source,
                    Value = value
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErrorResponse { Error = ex.Message });
            }
        }

        /// <summary>
        /// Batch полной нормализации массива адресов.
        /// </summary>
        /// <remarks>
        /// Обрабатывает каждый элемент независимо (тот же пайплайн, что у одиночного normalize).
        /// При ошибке одного элемента остальные продолжают обрабатываться; в ответе 200 —
        /// массив <c>items</c> со статусами <c>ok</c>/<c>error</c>.
        /// Если упали **все** элементы: validation → 400, runtime/mixed → 500 (одна ошибка, без items).
        /// Лимит размера задаётся конфигурацией <c>Batch:MaxItems</c> (по умолчанию 100).
        /// </remarks>
        /// <param name="request">Объект с массивом <c>items[].source</c>.</param>
        /// <response code="200">Частичный или полный успех; per-item статусы в <c>items</c>.</response>
        /// <response code="400">Невалидный запрос или все элементы провалили валидацию.</response>
        /// <response code="500">Все элементы упали с исключением (или mixed all-fail).</response>
        [HttpPost("batch")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(BatchNormalizeResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public ActionResult Batch(
            [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] BatchNormalizeRequest request)
        {
            _logger.LogInformation("NormalizeBatch started");

            if (request == null)
                return BadRequest(new ErrorResponse { Error = "request body is required" });

            IReadOnlyList<string> sources = null;
            if (request.Items != null)
            {
                var list = new List<string>(request.Items.Count);
                for (var i = 0; i < request.Items.Count; i++)
                    list.Add(request.Items[i]?.Source);
                sources = list;
            }

            var outcome = _service.NormalizeBatch(sources, _batchOptions.MaxItems);
            return MapBatchOutcome(outcome);
        }

        private static ActionResult MapBatchOutcome(BatchOutcome outcome)
        {
            switch (outcome.Kind)
            {
                case BatchOutcomeKind.PartialOrSuccess:
                    return new OkObjectResult(new BatchNormalizeResponse { Items = outcome.Items });

                case BatchOutcomeKind.RequestInvalid:
                case BatchOutcomeKind.AllFailValidation:
                    return new BadRequestObjectResult(new ErrorResponse
                    {
                        Error = outcome.ErrorMessage ?? "batch request is invalid"
                    });

                case BatchOutcomeKind.AllFailException:
                case BatchOutcomeKind.AllFailMixed:
                    return new ObjectResult(new ErrorResponse
                    {
                        Error = outcome.ErrorMessage ?? "all batch items failed"
                    })
                    {
                        StatusCode = StatusCodes.Status500InternalServerError
                    };

                default:
                    return new ObjectResult(new ErrorResponse
                    {
                        Error = outcome.ErrorMessage ?? "unexpected batch outcome"
                    })
                    {
                        StatusCode = StatusCodes.Status500InternalServerError
                    };
            }
        }
    }
}
