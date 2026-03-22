using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orbito.Application.Common.Authorization;
using Orbito.Application.DTOs;
using Orbito.Application.Features.Payments.Commands.CreatePaymentIntent;
using Orbito.Application.Portal.Queries.GetMyInvoices;
using Orbito.Application.Portal.Queries.GetMySubscriptions;

namespace Orbito.API.Controllers
{
    /// <summary>
    /// Self-service portal for authenticated clients.
    /// All endpoints require ClientAccess policy (role = Client).
    /// </summary>
    [Route("api/[controller]")]
    [Authorize(Policy = PolicyNames.ClientAccess)]
    public class PortalController : BaseController
    {
        public PortalController(IMediator mediator, ILogger<PortalController> logger)
            : base(mediator, logger)
        {
        }

        /// <summary>
        /// Gets all subscriptions for the currently authenticated client.
        /// </summary>
        [HttpGet("subscriptions")]
        [ProducesResponseType(typeof(List<SubscriptionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMySubscriptions(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Portal: GetMySubscriptions request");
            var result = await Mediator.Send(new GetMySubscriptionsQuery(), cancellationToken);
            return HandleResult(result);
        }

        /// <summary>
        /// Gets the payment history (invoices) for the currently authenticated client.
        /// </summary>
        [HttpGet("invoices")]
        [ProducesResponseType(typeof(List<PaymentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMyInvoices(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            Logger.LogInformation("Portal: GetMyInvoices request (page {Page}, size {Size})", pageNumber, pageSize);
            var result = await Mediator.Send(
                new GetMyInvoicesQuery { PageNumber = pageNumber, PageSize = pageSize },
                cancellationToken);
            return HandleResult(result);
        }

        /// <summary>
        /// Creates a Stripe PaymentIntent for paying a subscription.
        /// Returns client secret for use with Stripe Elements on the frontend.
        /// PCI DSS compliant - card data never touches Orbito servers.
        /// </summary>
        [HttpPost("payments/create-intent")]
        [ProducesResponseType(typeof(CreatePaymentIntentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreatePaymentIntent(
            [FromBody] CreatePaymentIntentCommand command,
            CancellationToken cancellationToken)
        {
            Logger.LogInformation(
                "Portal: CreatePaymentIntent request for subscription {SubscriptionId}",
                command.SubscriptionId);

            var result = await Mediator.Send(command, cancellationToken);
            return HandleResult(result);
        }
    }
}
