using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using VTBL.AddressNormalizer.WebApi.Models;
using VTBL.AddressNormalizer.WebApi.Services;

namespace VTBL.AddressNormalizer.WebApi.Controllers
{
    /// <summary>
    /// Операции над building-частью адреса: извлечение и канонизация.
    /// </summary>
    [ApiController]
    [Route("api/v1/address")]
    public class AddressController : ControllerBase
    {
        private readonly IAddressNormalizationService _service;

        /// <summary>
        /// Создаёт контроллер операций над building-частью адреса.
        /// </summary>
        public AddressController(IAddressNormalizationService service)
        {
            _service = service;
        }

        /// <summary>
        /// Возвращает из исходной строки только building-часть адреса.
        /// </summary>
        /// <remarks>
        /// Метод отделяет building-часть от indoor-хвоста, но не канонизирует адрес.
        /// Если indoor-маркеров нет, поле <c>extracted</c> обычно совпадает с исходной строкой.
        /// Если строка содержит только indoor-часть, <c>extracted</c> может оказаться пустым.
        /// </remarks>
        /// <param name="request">Запрос с полем <c>source</c>.</param>
        /// <response code="200">Building-часть адреса успешно извлечена.</response>
        /// <response code="400">Тело запроса отсутствует или поле <c>source</c> не заполнено.</response>
        [HttpPost("extract")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ExtractResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public ActionResult Extract(
            [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] SourceRequest request)
        {
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
        /// Приводит building-часть адреса к нормализованному виду.
        /// </summary>
        /// <remarks>
        /// Метод принимает строку как building-часть адреса и возвращает только нормализованное представление.
        /// Скрытое извлечение indoor-части не выполняется, hash в ответ не добавляется.
        /// </remarks>
        /// <param name="request">Запрос с полем <c>source</c>.</param>
        /// <response code="200">Нормализованный building-адрес.</response>
        /// <response code="400">Тело запроса отсутствует или поле <c>source</c> не заполнено.</response>
        [HttpPost("canonicalize")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(CanonicalizeResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public ActionResult Canonicalize(
            [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] SourceRequest request)
        {
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
