using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orbito.Application.Features.Payments.Commands.ProcessPayment;
using Orbito.Application.Features.Payments.Commands.UpdatePaymentStatus;
using Orbito.Application.Features.Payments.Commands;
using Orbito.Application.Features.Payments.Queries.GetPaymentById;
using Orbito.Application.Features.Payments.Queries.GetPaymentsBySubscription;

namespace Orbito.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PaymentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Przetwarza nową płatność
        /// </summary>
        /// <param name="command">Dane płatności do przetworzenia</param>
        /// <returns>Wynik przetwarzania płatności</returns>
        [HttpPost("process")]
        [Authorize(Roles = "Provider,PlatformAdmin")]
        public async Task<ActionResult<ProcessPaymentResult>> ProcessPayment([FromBody] ProcessPaymentCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetPaymentById), new { id = result.Payment!.Id }, result);
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
            var result = await _mediator.Send(query);

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
            var result = await _mediator.Send(query);

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
            var result = await _mediator.Send(commandWithId);

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
            var result = await _mediator.Send(commandWithId);

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
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
