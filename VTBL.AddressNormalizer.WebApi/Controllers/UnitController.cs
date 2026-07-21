using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using VTBL.AddressNormalizer.WebApi.Models;
using VTBL.AddressNormalizer.WebApi.Services;

namespace VTBL.AddressNormalizer.WebApi.Controllers
{
    /// <summary>
    /// Нормализация indoor / unit-строки без полного адреса (поле вроде new_flat).
    /// </summary>
    [ApiController]
    [Route("api/v1/unit")]
    public class UnitController : ControllerBase
    {
        private readonly IAddressNormalizationService _service;
        private readonly ILogger<UnitController> _logger;

        /// <summary>
        /// Создаёт контроллер unit.
        /// </summary>
        public UnitController(
            IAddressNormalizationService service,
            ILogger<UnitController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Нормализация indoor/unit-строки.
        /// </summary>
        /// <remarks>
        /// Использует <c>IBuildingUnitNormalizer</c>: структурированный <c>indoorValue</c>
        /// (все категории с русским name и values), каноническая строка и SHA256-hash.
        /// CRM-category не возвращается.
        ///
        /// Пример:
        ///
        ///     POST /api/v1/unit/normalize
        ///     { "source": "ЭТАЖ 2, КВАРТИРА 89" }
        /// </remarks>
        /// <param name="request">Обёртка с непустым <c>source</c>.</param>
        /// <response code="200">Успешная нормализация unit.</response>
        /// <response code="400">Тело отсутствует или <c>source</c> пустой / whitespace.</response>
        [HttpPost("normalize")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(UnitNormalizeResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public ActionResult Normalize(
            [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] SourceRequest request)
        {
            _logger.LogInformation("NormalizeUnit started");

            if (request == null)
                return BadRequest(new ErrorResponse { Error = "request body is required" });

            try
            {
                var result = _service.NormalizeUnit(request.Source);
                return Ok(new UnitNormalizeResponse
                {
                    Source = result.Source,
                    IndoorValue = result.IndoorValue,
                    Canonical = result.Canonical,
                    Hash = result.Hash
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErrorResponse { Error = ex.Message });
            }
        }
    }
}
