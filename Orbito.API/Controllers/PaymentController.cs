using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orbito.Application.Common.Authorization;
using Orbito.Application.Features.Payments.Commands.ProcessPayment;
using Orbito.Application.Features.Payments.Commands.UpdatePaymentStatus;
using Orbito.Application.Features.Payments.Commands;
using Orbito.Application.Features.Payments.Commands.SavePaymentMethod;
using Orbito.Application.Features.Payments.Queries.GetPaymentById;
using Orbito.Application.Features.Payments.Queries.GetPaymentsBySubscription;
using Orbito.Application.Features.Payments.Queries.GetPaymentMethodsByClient;
using Orbito.Application.Features.Payments.Queries.GetAllPayments;
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
        /// Pobiera listę wszystkich płatności dla tenanta z paginacją i filtrami
        /// </summary>
        /// <param name="pageNumber">Numer strony (domyślnie 1)</param>
        /// <param name="pageSize">Rozmiar strony (domyślnie 10)</param>
        /// <param name="searchTerm">Wyszukiwana fraza (opcjonalnie)</param>
        /// <param name="status">Filtruj po statusie (opcjonalnie)</param>
        /// <param name="clientId">Filtruj po kliencie (opcjonalnie)</param>
        /// <returns>Lista płatności z paginacją</returns>
        [HttpGet]
        [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
        [ProducesResponseType(typeof(GetAllPaymentsResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllPayments(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] PaymentStatus? status = null,
            [FromQuery] Guid? clientId = null)
        {
            var query = new GetAllPaymentsQuery(pageNumber, pageSize, searchTerm, status, clientId);
            var result = await Mediator.Send(query);

            return HandleResult(result);
        }

        /// <summary>
        /// Przetwarza nową płatność
        /// </summary>
        /// <param name="command">Dane płatności do przetworzenia</param>
        /// <returns>Wynik przetwarzania płatności</returns>
        [HttpPost("process")]
        [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
        public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentCommand command)
        {
            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                return HandleResult(result);
            }

            return CreatedAtAction(nameof(GetPaymentById), new { id = result.Value.Id, clientId = result.Value.ClientId }, result.Value);
        }

        /// <summary>
        /// Pobiera płatność po ID
        /// </summary>
        /// <param name="id">ID płatności</param>
        /// <param name="clientId">ID klienta (security: client verification)</param>
        /// <returns>Szczegóły płatności</returns>
        [HttpGet("{id}")]
        [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
        public async Task<IActionResult> GetPaymentById(Guid id, [FromQuery] Guid clientId)
        {
            var query = new GetPaymentByIdQuery(id, clientId);
            var result = await Mediator.Send(query);

            return HandleResult(result);
        }

        /// <summary>
        /// Pobiera płatności dla subskrypcji
        /// </summary>
        /// <param name="subscriptionId">ID subskrypcji</param>
        /// <param name="clientId">ID klienta (security: client verification)</param>
        /// <param name="pageNumber">Numer strony (domyślnie 1)</param>
        /// <param name="pageSize">Rozmiar strony (domyślnie 10)</param>
        /// <returns>Lista płatności dla subskrypcji</returns>
        [HttpGet("subscription/{subscriptionId}")]
        [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
        public async Task<IActionResult> GetPaymentsBySubscription(
            Guid subscriptionId,
            [FromQuery] Guid clientId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new GetPaymentsBySubscriptionQuery(subscriptionId, clientId, pageNumber, pageSize);
            var result = await Mediator.Send(query);

            return HandleResult(result);
        }

        /// <summary>
        /// Aktualizuje status płatności
        /// </summary>
        /// <param name="id">ID płatności</param>
        /// <param name="clientId">ID klienta (security: client verification)</param>
        /// <param name="command">Dane do aktualizacji statusu</param>
        /// <returns>Zaktualizowana płatność</returns>
        [HttpPut("{id}/status")]
        [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
        public async Task<IActionResult> UpdatePaymentStatus(
            Guid id,
            [FromQuery] Guid clientId,
            [FromBody] UpdatePaymentStatusCommand command)
        {
            // Upewnij się, że ID w command jest takie samo jak w URL
            var commandWithId = command with { PaymentId = id, ClientId = clientId };
            var result = await Mediator.Send(commandWithId);

            return HandleResult(result);
        }

        /// <summary>
        /// Zwraca płatność
        /// </summary>
        /// <param name="id">ID płatności</param>
        /// <param name="clientId">ID klienta (security: client verification)</param>
        /// <param name="command">Dane zwrotu</param>
        /// <returns>Wynik zwrotu płatności</returns>
        [HttpPost("{id}/refund")]
        [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
        public async Task<ActionResult<RefundPaymentResult>> RefundPayment(
            Guid id,
            [FromQuery] Guid clientId,
            [FromBody] RefundPaymentCommand command)
        {
            // Upewnij się, że ID w command jest takie samo jak w URL
            var commandWithId = command with { PaymentId = id, ClientId = clientId };
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
        [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
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
        [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
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
        [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
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
