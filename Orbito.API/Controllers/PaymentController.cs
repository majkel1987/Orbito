using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orbito.Application.Features.Payments.Commands.ProcessPayment;
using Orbito.Application.Features.Payments.Commands.UpdatePaymentStatus;
using Orbito.Application.Features.Payments.Commands;
using Orbito.Application.Features.Payments.Commands.SavePaymentMethod;
using Orbito.Application.Features.Payments.Queries.GetPaymentById;
using Orbito.Application.Features.Payments.Queries.GetPaymentsBySubscription;
using Orbito.Application.Features.Payments.Queries.GetPaymentMethodsByClient;
using Orbito.Domain.Enums;

namespace Orbito.API.Controllers
{
    [Route("api/[controller]")]
    public class PaymentController : BaseController
    {
        public PaymentController(IMediator mediator, ILogger<PaymentController> logger)
            : base(mediator, logger)
        {
        }

        /// <summary>
        /// Przetwarza nową płatność
        /// </summary>
        /// <param name="command">Dane płatności do przetworzenia</param>
        /// <returns>Wynik przetwarzania płatności</returns>
        [HttpPost("process")]
        [Authorize(Roles = "Provider,PlatformAdmin")]
        public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentCommand command)
        {
            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                return HandleResult(result);
            }

            return CreatedAtAction(nameof(GetPaymentById), new { id = result.Value.Id }, result.Value);
        }

        /// <summary>
        /// Pobiera płatność po ID
        /// </summary>
        /// <param name="id">ID płatności</param>
        /// <returns>Szczegóły płatności</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "Provider,PlatformAdmin")]
        public async Task<ActionResult<GetPaymentByIdResult>> GetPaymentById(Guid id)
        {
            var query = new GetPaymentByIdQuery(id);
            var result = await Mediator.Send(query);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Pobiera płatności dla subskrypcji
        /// </summary>
        /// <param name="subscriptionId">ID subskrypcji</param>
        /// <param name="pageNumber">Numer strony (domyślnie 1)</param>
        /// <param name="pageSize">Rozmiar strony (domyślnie 10)</param>
        /// <returns>Lista płatności dla subskrypcji</returns>
        [HttpGet("subscription/{subscriptionId}")]
        [Authorize(Roles = "Provider,PlatformAdmin")]
        public async Task<ActionResult<GetPaymentsBySubscriptionResult>> GetPaymentsBySubscription(
            Guid subscriptionId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new GetPaymentsBySubscriptionQuery(subscriptionId, pageNumber, pageSize);
            var result = await Mediator.Send(query);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Aktualizuje status płatności
        /// </summary>
        /// <param name="id">ID płatności</param>
        /// <param name="command">Dane do aktualizacji statusu</param>
        /// <returns>Zaktualizowana płatność</returns>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Provider,PlatformAdmin")]
        public async Task<ActionResult<UpdatePaymentStatusResult>> UpdatePaymentStatus(
            Guid id,
            [FromBody] UpdatePaymentStatusCommand command)
        {
            // Upewnij się, że ID w command jest takie samo jak w URL
            var commandWithId = command with { PaymentId = id };
            var result = await Mediator.Send(commandWithId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Zwraca płatność
        /// </summary>
        /// <param name="id">ID płatności</param>
        /// <param name="command">Dane zwrotu</param>
        /// <returns>Wynik zwrotu płatności</returns>
        [HttpPost("{id}/refund")]
        [Authorize(Roles = "Provider,PlatformAdmin")]
        public async Task<ActionResult<RefundPaymentResult>> RefundPayment(
            Guid id,
            [FromBody] RefundPaymentCommand command)
        {
            // Upewnij się, że ID w command jest takie samo jak w URL
            var commandWithId = command with { PaymentId = id };
            var result = await Mediator.Send(commandWithId);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Tworzy klienta Stripe
        /// </summary>
        /// <param name="command">Dane klienta</param>
        /// <returns>Wynik tworzenia klienta Stripe</returns>
        [HttpPost("create-customer")]
        [Authorize(Roles = "Provider,PlatformAdmin")]
        public async Task<ActionResult<CreateStripeCustomerResult>> CreateStripeCustomer(
            [FromBody] CreateStripeCustomerCommand command)
        {
            var result = await Mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Zapisuje metodę płatności
        /// </summary>
        /// <param name="command">Dane metody płatności</param>
        /// <returns>Wynik zapisywania metody płatności</returns>
        [HttpPost("payment-methods")]
        [Authorize(Roles = "Provider,PlatformAdmin")]
        public async Task<ActionResult<SavePaymentMethodResult>> SavePaymentMethod(
            [FromBody] SavePaymentMethodCommand command)
        {
            var result = await Mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetPaymentMethodsByClient), new { clientId = command.ClientId }, result.Value);
        }

        /// <summary>
        /// Pobiera metody płatności klienta
        /// </summary>
        /// <param name="clientId">ID klienta</param>
        /// <param name="pageNumber">Numer strony</param>
        /// <param name="pageSize">Rozmiar strony</param>
        /// <param name="type">Typ metody płatności</param>
        /// <param name="activeOnly">Tylko aktywne metody</param>
        /// <returns>Lista metod płatności</returns>
        [HttpGet("payment-methods/client/{clientId}")]
        [Authorize(Roles = "Provider,PlatformAdmin")]
        public async Task<ActionResult<GetPaymentMethodsByClientResult>> GetPaymentMethodsByClient(
            Guid clientId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] PaymentMethodType? type = null,
            [FromQuery] bool activeOnly = true)
        {
            var query = new GetPaymentMethodsByClientQuery
            {
                ClientId = clientId,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Type = type,
                ActiveOnly = activeOnly
            };

            var result = await Mediator.Send(query);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result.Value);
        }
    }
}
