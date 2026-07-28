using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using VTBL.AddressNormalizer.WebApi.Models;
using VTBL.AddressNormalizer.WebApi.Options;
using VTBL.AddressNormalizer.WebApi.Services;

namespace VTBL.AddressNormalizer.WebApi.Controllers
{
    /// <summary>
    /// Нормализует адрес целиком: отдельно building-часть и отдельно indoor-часть.
    /// </summary>
    [ApiController]
    [Route("api/v1/normalize")]
    public class NormalizeController : ControllerBase
    {
        private readonly IAddressNormalizationService _service;
        private readonly BatchOptions _batchOptions;

        /// <summary>
        /// Создаёт контроллер для одиночной и пакетной нормализации адресов.
        /// </summary>
        public NormalizeController(
            IAddressNormalizationService service,
            IOptions<BatchOptions> batchOptions)
        {
            _service = service;
            _batchOptions = batchOptions.Value;
        }

        /// <summary>
        /// Нормализует один адрес и возвращает результат по building- и indoor-частям.
        /// </summary>
        /// <remarks>
        /// Разбирает полную адресную строку в два независимых результата.
        /// Для building-части возвращает извлечённый фрагмент, нормализованный адрес, hash и данные DaData.
        /// Для indoor-части возвращает извлечённый фрагмент, категории внутренней адресации и hash канонической строки.
        /// Поле <c>buildingValue.dadata</c> пока заполняется заглушками для <c>suggest</c> и <c>clean</c>.
        ///
        /// Пример запроса:
        ///
        ///     POST /api/v1/normalize
        ///     { "source": "г Москва, ул Сухонская, д 11, кв 89" }
        /// </remarks>
        /// <param name="request">Запрос с полем <c>source</c>.</param>
        /// <response code="200">Адрес успешно нормализован.</response>
        /// <response code="400">Тело запроса отсутствует или поле <c>source</c> не заполнено.</response>
        [HttpPost]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(NormalizeResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public ActionResult Normalize(
            [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] SourceRequest request)
        {
            if (request == null)
                return BadRequest(new ErrorResponse { Error = "тело запроса обязательно" });

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
        /// Нормализует массив адресов и возвращает результат по каждому элементу отдельно.
        /// </summary>
        /// <remarks>
        /// Каждый адрес обрабатывается независимо по тем же правилам, что и в одиночной нормализации.
        /// Если часть элементов завершилась с ошибкой, остальные всё равно будут обработаны,
        /// а результат вернётся в массиве <c>items</c> со статусами <c>ok</c> и <c>error</c>.
        /// Если не удалось обработать все элементы, API вернёт одну общую ошибку без массива результатов.
        /// Максимальный размер batch задаётся параметром <c>Batch:MaxItems</c>.
        /// </remarks>
        /// <param name="request">Запрос с массивом <c>items</c>, где каждый элемент содержит поле <c>source</c>.</param>
        /// <response code="200">Полный или частичный успех; результат по каждому элементу возвращается в <c>items</c>.</response>
        /// <response code="400">Запрос невалиден или ни один элемент не прошёл валидацию.</response>
        /// <response code="500">Ни один элемент не удалось обработать из-за исключений во время выполнения.</response>
        [HttpPost("batch")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(BatchNormalizeResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public ActionResult Batch(
            [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] BatchNormalizeRequest request)
        {
            if (request == null)
                return BadRequest(new ErrorResponse { Error = "тело запроса обязательно" });

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
                        Error = outcome.ErrorMessage ?? "некорректный batch-запрос"
                    });

                case BatchOutcomeKind.AllFailException:
                case BatchOutcomeKind.AllFailMixed:
                    return new ObjectResult(new ErrorResponse
                    {
                        Error = outcome.ErrorMessage ?? "все элементы batch завершились неуспешно"
                    })
                    {
                        StatusCode = StatusCodes.Status500InternalServerError
                    };

                default:
                    return new ObjectResult(new ErrorResponse
                    {
                        Error = outcome.ErrorMessage ?? "неожиданный результат batch"
                    })
                    {
                        StatusCode = StatusCodes.Status500InternalServerError
                    };
            }
        }
    }
}
