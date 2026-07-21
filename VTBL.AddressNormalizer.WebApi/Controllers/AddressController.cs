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
    /// Операции над outdoor-частью адреса: extract и canonicalize.
    /// </summary>
    [ApiController]
    [Route("api/v1/address")]
    public class AddressController : ControllerBase
    {
        private readonly IAddressNormalizationService _service;
        private readonly ILogger<AddressController> _logger;

        /// <summary>
        /// Создаёт контроллер address.
        /// </summary>
        public AddressController(
            IAddressNormalizationService service,
            ILogger<AddressController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Извлечение outdoor-части адреса (отсечение indoor).
        /// </summary>
        /// <remarks>
        /// Возвращает только outdoor-строку без канонизации и без indoor-разбора.
        /// Если indoor-маркеров нет — <c>extracted</c> совпадает с исходной (нормализованной extract-логикой) строкой;
        /// если строка целиком indoor — <c>extracted</c> может быть пустой.
        /// </remarks>
        /// <param name="request">Обёртка с непустым <c>source</c>.</param>
        /// <response code="200">Outdoor-часть извлечена.</response>
        /// <response code="400">Тело отсутствует или <c>source</c> пустой / whitespace.</response>
        [HttpPost("extract")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ExtractResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public ActionResult Extract(
            [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] SourceRequest request)
        {
            _logger.LogInformation("ExtractOutdoor started");

            if (request == null)
                return BadRequest(new ErrorResponse { Error = "тело запроса обязательно" });

            try
            {
                var extracted = _service.ExtractOutdoor(request.Source);
                return Ok(new ExtractResponse
                {
                    Source = request.Source,
                    Extracted = extracted
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErrorResponse { Error = ex.Message });
            }
        }

        /// <summary>
        /// Канонизация building location (без предварительного extract).
        /// </summary>
        /// <remarks>
        /// Принимает уже outdoor (или строку, которую нужно канонизировать как building location).
        /// Не выполняет скрытый extract и не возвращает hash — только читаемый канон.
        /// </remarks>
        /// <param name="request">Обёртка с непустым <c>source</c>.</param>
        /// <response code="200">Каноническая строка.</response>
        /// <response code="400">Тело отсутствует или <c>source</c> пустой / whitespace.</response>
        [HttpPost("canonicalize")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(CanonicalizeResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public ActionResult Canonicalize(
            [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] SourceRequest request)
        {
            _logger.LogInformation("Canonicalize started");

            if (request == null)
                return BadRequest(new ErrorResponse { Error = "тело запроса обязательно" });

            try
            {
                var canonical = _service.Canonicalize(request.Source);
                return Ok(new CanonicalizeResponse
                {
                    Source = request.Source,
                    Canonical = canonical
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErrorResponse { Error = ex.Message });
            }
        }
    }
}
