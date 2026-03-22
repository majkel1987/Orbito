using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orbito.Application.Common.Authorization;
using Orbito.Application.Features.ProviderSubscriptions.Commands.CreateProviderPaymentIntent;

namespace Orbito.API.Controllers;

/// <summary>
/// Controller for Provider billing operations.
/// Handles payment intents for platform subscriptions.
///
/// ⚠️ CRITICAL: This controller MUST work even with expired trial!
/// Do NOT add ActiveProviderSubscription policy here.
/// </summary>
[Route("api/[controller]")]
[Authorize(Policy = PolicyNames.ProviderTeamAccess)]
public class ProviderBillingController : BaseController
{
    public ProviderBillingController(IMediator mediator, ILogger<ProviderBillingController> logger)
        : base(mediator, logger)
    {
    }

    /// <summary>
    /// Create a Stripe PaymentIntent for Provider platform subscription.
    /// Returns client secret for Stripe Elements (PCI DSS compliant).
    /// </summary>
    /// <param name="command">Optional PlatformPlanId (null = use current plan)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Client secret and payment details</returns>
    [HttpPost("create-payment-intent")]
    [ProducesResponseType(typeof(CreateProviderPaymentIntentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreatePaymentIntent(
        [FromBody] CreateProviderPaymentIntentCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}
