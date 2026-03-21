using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orbito.Application.Common.Authorization;
using Orbito.Application.Features.ProviderSubscriptions.Queries.GetMyProviderSubscription;

namespace Orbito.API.Controllers;

/// <summary>
/// Controller for Provider's platform subscription management.
/// Handles trial status, subscription info for the dashboard banner.
/// </summary>
[Route("api/[controller]")]
[Authorize(Policy = PolicyNames.ProviderTeamAccess)]
public class ProviderSubscriptionController : BaseController
{
    public ProviderSubscriptionController(IMediator mediator, ILogger<ProviderSubscriptionController> logger)
        : base(mediator, logger)
    {
    }

    /// <summary>
    /// Get the current provider's platform subscription status.
    /// Used for displaying trial banner in the dashboard.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Provider subscription details including trial status</returns>
    [HttpGet("my")]
    [ProducesResponseType(typeof(ProviderSubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMy(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetMyProviderSubscriptionQuery(), cancellationToken);
        return HandleResult(result);
    }
}
