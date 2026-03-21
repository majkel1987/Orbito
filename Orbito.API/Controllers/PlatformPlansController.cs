using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orbito.Application.Features.PlatformPlans.Queries.GetPlatformPlans;

namespace Orbito.API.Controllers;

/// <summary>
/// Controller for Platform Plans (Orbito's subscription plans for Providers).
/// Public endpoint - plans are visible on registration page.
/// </summary>
[Route("api/[controller]")]
public class PlatformPlansController : BaseController
{
    public PlatformPlansController(IMediator mediator, ILogger<PlatformPlansController> logger)
        : base(mediator, logger)
    {
    }

    /// <summary>
    /// Get all active platform plans.
    /// Used on registration page for plan selection.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of active platform plans</returns>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PlatformPlanDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetPlatformPlansQuery(), cancellationToken);
        return HandleResult(result);
    }
}
