using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orbito.Application.Common.Authorization;
using Orbito.Application.SubscriptionPlans.Commands.CloneSubscriptionPlan;
using Orbito.Application.SubscriptionPlans.Commands.CreateSubscriptionPlan;
using Orbito.Application.SubscriptionPlans.Commands.DeleteSubscriptionPlan;
using Orbito.Application.SubscriptionPlans.Commands.UpdateSubscriptionPlan;
using Orbito.Application.SubscriptionPlans.Queries.GetActiveSubscriptionPlans;
using Orbito.Application.SubscriptionPlans.Queries.GetSubscriptionPlanById;
using Orbito.Application.SubscriptionPlans.Queries.GetSubscriptionPlansByProvider;

namespace Orbito.API.Controllers
{
    [Route("api/[controller]")]
    public class SubscriptionPlansController : BaseController
    {
        public SubscriptionPlansController(IMediator mediator, ILogger<SubscriptionPlansController> logger)
            : base(mediator, logger)
        {
        }

        /// <summary>
        /// Creates a new subscription plan
        /// </summary>
        /// <param name="command">Subscription plan creation details</param>
        /// <returns>Created subscription plan</returns>
        [HttpPost]
        [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
        public async Task<IActionResult> CreateSubscriptionPlan(
            [FromBody] CreateSubscriptionPlanCommand command)
        {
            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                return HandleResult(result);
            }

            return CreatedAtAction(nameof(GetSubscriptionPlan), new { id = result.Value.Id }, result.Value);
        }

        /// <summary>
        /// Gets all subscription plans for the current provider
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="activeOnly">Filter only active plans</param>
        /// <param name="publicOnly">Filter only public plans</param>
        /// <param name="searchTerm">Search term for name or description</param>
        /// <returns>List of subscription plans</returns>
        [HttpGet]
        [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
        public async Task<ActionResult<SubscriptionPlansListDto>> GetSubscriptionPlans(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool activeOnly = false,
            [FromQuery] bool publicOnly = false,
            [FromQuery] string? searchTerm = null)
        {
            var query = new GetSubscriptionPlansByProviderQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                ActiveOnly = activeOnly,
                PublicOnly = publicOnly,
                SearchTerm = searchTerm
            };

            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Gets a specific subscription plan by ID
        /// </summary>
        /// <param name="id">Subscription plan ID</param>
        /// <returns>Subscription plan details</returns>
        [HttpGet("{id}")]
        [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
        public async Task<ActionResult<SubscriptionPlanDto>> GetSubscriptionPlan(Guid id)
        {
            var query = new GetSubscriptionPlanByIdQuery { Id = id };
            var result = await Mediator.Send(query);

            return HandleResult(result);
        }

        /// <summary>
        /// Updates an existing subscription plan
        /// </summary>
        /// <param name="id">Subscription plan ID</param>
        /// <param name="command">Updated subscription plan details</param>
        /// <returns>Updated subscription plan</returns>
        [HttpPut("{id}")]
        [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
        public async Task<IActionResult> UpdateSubscriptionPlan(
            Guid id,
            [FromBody] UpdateSubscriptionPlanCommand command)
        {
            command = command with { Id = id };
            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                return HandleResult(result);
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Deletes a subscription plan
        /// </summary>
        /// <param name="id">Subscription plan ID</param>
        /// <param name="hardDelete">Whether to perform hard delete (default: false)</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("{id}")]
        [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
        public async Task<IActionResult> DeleteSubscriptionPlan(
            Guid id,
            [FromQuery] bool hardDelete = false)
        {
            var command = new DeleteSubscriptionPlanCommand
            {
                Id = id,
                HardDelete = hardDelete
            };

            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                return HandleResult(result);
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Clones an existing subscription plan
        /// </summary>
        /// <param name="id">Original subscription plan ID</param>
        /// <param name="command">Clone details</param>
        /// <returns>Cloned subscription plan</returns>
        [HttpPost("{id}/clone")]
        [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
        public async Task<IActionResult> CloneSubscriptionPlan(
            Guid id,
            [FromBody] CloneSubscriptionPlanCommand command)
        {
            command = command with { Id = id };
            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                return HandleResult(result);
            }

            return CreatedAtAction(nameof(GetSubscriptionPlan), new { id = result.Value.Id }, result.Value);
        }

        /// <summary>
        /// Gets all active subscription plans (public endpoint for clients)
        /// </summary>
        /// <param name="publicOnly">Filter only public plans (default: true)</param>
        /// <param name="limit">Maximum number of plans to return</param>
        /// <returns>List of active subscription plans</returns>
        [HttpGet("active")]
        [AllowAnonymous]
        public async Task<ActionResult<ActiveSubscriptionPlansDto>> GetActiveSubscriptionPlans(
            [FromQuery] bool publicOnly = true,
            [FromQuery] int? limit = null)
        {
            var query = new GetActiveSubscriptionPlansQuery
            {
                PublicOnly = publicOnly,
                Limit = limit
            };

            var result = await Mediator.Send(query);
            return HandleResult(result);
        }
    }
}
