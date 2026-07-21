using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VTBL.AddressNormalizer.WebApi.Models;

namespace VTBL.AddressNormalizer.WebApi.Controllers
{
    /// <summary>
    /// Liveness/health без внешних зависимостей.
    /// </summary>
    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase
    {
        /// <summary>
        /// Проверка живости сервиса.
        /// </summary>
        /// <remarks>
        /// Всегда возвращает <c>{ "status": "Healthy" }</c> при поднятом host.
        /// Auth и зависимости ядра не проверяются.
        /// </remarks>
        /// <response code="200">Сервис доступен.</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
        public ActionResult<HealthResponse> Get()
        {
            return Ok(new HealthResponse { Status = "Healthy" });
        }
    }
}
