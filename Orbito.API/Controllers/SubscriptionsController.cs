using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orbito.Application.Common.Authorization;
using Orbito.Application.Subscriptions.Commands.ActivateSubscription;
using Orbito.Application.Subscriptions.Commands.CancelSubscription;
using Orbito.Application.Subscriptions.Commands.CreateSubscription;
using Orbito.Application.Subscriptions.Commands.DowngradeSubscription;
using Orbito.Application.Subscriptions.Commands.RenewSubscription;
using Orbito.Application.Subscriptions.Commands.ResumeSubscription;
using Orbito.Application.Subscriptions.Commands.SuspendSubscription;
using Orbito.Application.Subscriptions.Commands.UpgradeSubscription;
using Orbito.Application.Subscriptions.Queries.GetActiveSubscriptions;
using Orbito.Application.Subscriptions.Queries.GetExpiringSubscriptions;
using Orbito.Application.Subscriptions.Queries.GetSubscriptionById;
using Orbito.Application.Subscriptions.Queries.GetSubscriptionsByClient;

namespace Orbito.API.Controllers
{
    [Route("api/[controller]")]
    public class SubscriptionsController : BaseController
    {
        public SubscriptionsController(IMediator mediator, ILogger<SubscriptionsController> logger)
            : base(mediator, logger)
        {
        }

        /// <summary>
        /// Creates a new subscription for a client
        /// </summary>
        /// <param name="command">Subscription creation details</param>
        /// <returns>Created subscription details</returns>
        [HttpPost]
        [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
        public async Task<IActionResult> CreateSubscription(CreateSubscriptionCommand command)
        {
            Logger.LogInformation("Creating subscription for client {ClientId} with plan {PlanId}",
                command.ClientId, command.PlanId);

            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                return HandleResult(result);
            }

            return CreatedAtAction(nameof(GetSubscriptionById), new { id = result.Value.Id }, result.Value);
        }

        /// <summary>
        /// Gets all subscriptions with pagination and filtering
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="searchTerm">Search term for filtering</param>
        /// <returns>List of active subscriptions</returns>
        [HttpGet]
        [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
        public async Task<IActionResult> GetSubscriptions(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            Logger.LogInformation("Getting subscriptions with pagination {PageNumber}/{PageSize}",
                pageNumber, pageSize);

            var query = new GetActiveSubscriptionsQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm
            };

            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Gets a subscription by ID
        /// </summary>
        /// <param name="id">Subscription ID</param>
        /// <param name="includeDetails">Include detailed information (default: false)</param>
        /// <returns>Subscription details</returns>
        [HttpGet("{id}")]
        [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
        public async Task<IActionResult> GetSubscriptionById(
            Guid id,
            [FromQuery] bool includeDetails = false)
        {
            Logger.LogInformation("Getting subscription {SubscriptionId} with details: {IncludeDetails}",
                id, includeDetails);

            var query = new GetSubscriptionByIdQuery
            {
                SubscriptionId = id,
                IncludeDetails = includeDetails
            };

            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Gets subscriptions for a specific client
        /// </summary>
        /// <param name="clientId">Client ID</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="activeOnly">Show only active subscriptions (default: false)</param>
        /// <returns>List of client subscriptions</returns>
        [HttpGet("client/{clientId}")]
        [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
        public async Task<IActionResult> GetSubscriptionsByClient(
            Guid clientId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool activeOnly = false)
        {
            Logger.LogInformation("Getting subscriptions for client {ClientId}", clientId);

            var query = new GetSubscriptionsByClientQuery
            {
                ClientId = clientId,
                PageNumber = pageNumber,
                PageSize = pageSize,
                ActiveOnly = activeOnly
            };

            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Gets expiring subscriptions
        /// </summary>
        /// <param name="daysBeforeExpiration">Days before expiration to check (default: 7)</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <returns>List of expiring subscriptions</returns>
        [HttpGet("expiring")]
        [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
        public async Task<IActionResult> GetExpiringSubscriptions(
            [FromQuery] int daysBeforeExpiration = 7,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            Logger.LogInformation("Getting expiring subscriptions within {Days} days", daysBeforeExpiration);

            var query = new GetExpiringSubscriptionsQuery
            {
                DaysBeforeExpiration = daysBeforeExpiration,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Activates a subscription
        /// </summary>
        /// <param name="id">Subscription ID</param>
        /// <param name="request">Activation request with client ID</param>
        /// <returns>Activation result</returns>
        [HttpPost("{id}/activate")]
        [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
        public async Task<IActionResult> ActivateSubscription(
            Guid id,
            [FromBody] ActivateSubscriptionRequest request)
        {
            Logger.LogInformation("Activating subscription {SubscriptionId} for client {ClientId}",
                id, request.ClientId);

            var command = new ActivateSubscriptionCommand
            {
                SubscriptionId = id,
                ClientId = request.ClientId
            };

            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Cancels a subscription
        /// </summary>
        /// <param name="id">Subscription ID</param>
        /// <param name="request">Cancellation request with reason and client ID</param>
        /// <returns>Cancellation result</returns>
        [HttpPost("{id}/cancel")]
        [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
        public async Task<IActionResult> CancelSubscription(
            Guid id,
            [FromBody] CancelSubscriptionRequestDto request)
        {
            Logger.LogInformation("Cancelling subscription {SubscriptionId}", id);

            var command = new CancelSubscriptionCommand
            {
                SubscriptionId = id,
                ClientId = request.ClientId,
                Reason = request.Reason
            };

            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Suspends a subscription
        /// </summary>
        /// <param name="id">Subscription ID</param>
        /// <param name="request">Suspension request with client ID and reason</param>
        /// <returns>Suspension result</returns>
        [HttpPost("{id}/suspend")]
        [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
        public async Task<IActionResult> SuspendSubscription(
            Guid id,
            [FromBody] SuspendSubscriptionRequestDto request)
        {
            Logger.LogInformation("Suspending subscription {SubscriptionId}", id);

            var command = new SuspendSubscriptionCommand
            {
                SubscriptionId = id,
                ClientId = request.ClientId,
                Reason = request.Reason
            };

            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Resumes a suspended subscription
        /// </summary>
        /// <param name="id">Subscription ID</param>
        /// <param name="request">Resume request with client ID</param>
        /// <returns>Resume result</returns>
        [HttpPost("{id}/resume")]
        [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
        public async Task<IActionResult> ResumeSubscription(
            Guid id,
            [FromBody] ResumeSubscriptionRequestDto request)
        {
            Logger.LogInformation("Resuming subscription {SubscriptionId}", id);

            var command = new ResumeSubscriptionCommand
            {
                SubscriptionId = id,
                ClientId = request.ClientId
            };

            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Upgrades a subscription to a higher plan
        /// </summary>
        /// <param name="id">Subscription ID</param>
        /// <param name="request">Upgrade details</param>
        /// <returns>Upgrade result</returns>
        [HttpPost("{id}/upgrade")]
        [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
        public async Task<IActionResult> UpgradeSubscription(
            Guid id,
            [FromBody] UpgradeSubscriptionRequest request)
        {
            Logger.LogInformation("Upgrading subscription {SubscriptionId} to plan {NewPlanId}",
                id, request.NewPlanId);

            var command = new UpgradeSubscriptionCommand
            {
                SubscriptionId = id,
                ClientId = request.ClientId,
                NewPlanId = request.NewPlanId,
                NewAmount = request.NewAmount,
                Currency = request.Currency
            };

            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Downgrades a subscription to a lower plan
        /// </summary>
        /// <param name="id">Subscription ID</param>
        /// <param name="request">Downgrade details</param>
        /// <returns>Downgrade result</returns>
        [HttpPost("{id}/downgrade")]
        [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
        public async Task<IActionResult> DowngradeSubscription(
            Guid id,
            [FromBody] DowngradeSubscriptionRequest request)
        {
            Logger.LogInformation("Downgrading subscription {SubscriptionId} to plan {NewPlanId}",
                id, request.NewPlanId);

            var command = new DowngradeSubscriptionCommand
            {
                SubscriptionId = id,
                ClientId = request.ClientId,
                NewPlanId = request.NewPlanId,
                NewAmount = request.NewAmount,
                Currency = request.Currency
            };

            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Renews a subscription with payment
        /// </summary>
        /// <param name="id">Subscription ID</param>
        /// <param name="request">Renewal details</param>
        /// <returns>Renewal result</returns>
        [HttpPost("{id}/renew")]
        [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
        public async Task<IActionResult> RenewSubscription(
            Guid id,
            [FromBody] RenewSubscriptionRequest request)
        {
            Logger.LogInformation("Renewing subscription {SubscriptionId} for client {ClientId}",
                id, request.ClientId);

            var command = new RenewSubscriptionCommand
            {
                SubscriptionId = id,
                ClientId = request.ClientId,
                Amount = request.Amount,
                Currency = request.Currency,
                ExternalPaymentId = request.ExternalPaymentId
            };

            var result = await Mediator.Send(command);
            return HandleResult(result);
        }
    }

    // Request DTOs
    public record ActivateSubscriptionRequest(Guid ClientId);
    public record CancelSubscriptionRequest(string? Reason);
    public record CancelSubscriptionRequestDto(Guid ClientId, string? Reason);
    public record SuspendSubscriptionRequest(string? Reason);
    public record SuspendSubscriptionRequestDto(Guid ClientId, string? Reason);
    public record ResumeSubscriptionRequestDto(Guid ClientId);
    public record UpgradeSubscriptionRequest(Guid ClientId, Guid NewPlanId, decimal NewAmount, string Currency);
    public record DowngradeSubscriptionRequest(Guid ClientId, Guid NewPlanId, decimal NewAmount, string Currency);
    public record RenewSubscriptionRequest(Guid ClientId, decimal Amount, string Currency, string? ExternalPaymentId = null);
}
