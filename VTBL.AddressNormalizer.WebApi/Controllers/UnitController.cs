using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using VTBL.AddressNormalizer.WebApi.Models;
using VTBL.AddressNormalizer.WebApi.Services;

namespace VTBL.AddressNormalizer.WebApi.Controllers
{
    /// <summary>
    /// Нормализует indoor-часть адреса без building-части.
    /// </summary>
    [ApiController]
    [Route("api/v1/unit")]
    public class UnitController : ControllerBase
    {
        private readonly IAddressNormalizationService _service;

        /// <summary>
        /// Создаёт контроллер нормализации indoor-строк.
        /// </summary>
        public UnitController(IAddressNormalizationService service)
        {
            _service = service;
        }

        /// <summary>
        /// Нормализует indoor-строку и возвращает структурированный результат.
        /// </summary>
        /// <remarks>
        /// Возвращает извлечённый indoor-фрагмент, категории внутренней адресации,
        /// каноническую строку и её hash.
        /// Подходит для случаев, когда building-часть адреса уже известна или не нужна.
        ///
        /// Пример:
        ///
        ///     POST /api/v1/unit/normalize
        ///     { "source": "ЭТАЖ 2, КВАРТИРА 89" }
        /// </remarks>
        /// <param name="request">Запрос с полем <c>source</c>.</param>
        /// <response code="200">Indoor-строка успешно нормализована.</response>
        /// <response code="400">Тело запроса отсутствует или поле <c>source</c> не заполнено.</response>
        [HttpPost("normalize")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(UnitNormalizeResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public ActionResult Normalize(
            [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] SourceRequest request)
        {
            if (request == null)
                return BadRequest(new ErrorResponse { Error = "тело запроса обязательно" });

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
