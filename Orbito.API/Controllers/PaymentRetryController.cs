using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Orbito.Application.Features.Payments.Commands;
using Orbito.Application.Features.Payments.Queries;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models;

namespace Orbito.API.Controllers
{
    /// <summary>
    /// Controller for managing payment retry operations
    /// </summary>
    [Authorize(Roles = "Provider,Client")]
    [Route("api/payments/retry")]
    public class PaymentRetryController : BaseController
    {
        private readonly IUserContextService _userContextService;
        private readonly ISecurityLimitService _securityLimitService;
        private readonly IMemoryCache _cache;

        public PaymentRetryController(
            IMediator mediator,
            ILogger<PaymentRetryController> logger,
            IUserContextService userContextService,
            ISecurityLimitService securityLimitService,
            IMemoryCache cache)
            : base(mediator, logger)
        {
            _userContextService = userContextService;
            _securityLimitService = securityLimitService;
            _cache = cache;
        }

        /// <summary>
        /// Retry a failed payment
        /// </summary>
        /// <param name="paymentId">ID of the payment to retry</param>
        /// <param name="request">Retry request details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result of the retry operation</returns>
        [HttpPost("{paymentId}")]
        [ProducesResponseType(typeof(RetryFailedPaymentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RetryFailedPaymentResult>> RetryPayment(
            Guid paymentId,
            [FromBody] RetryPaymentRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                Logger.LogInformation("Retry payment request for payment {PaymentId}", paymentId);

                // SECURITY: Get ClientId from user context with caching
                var currentClientId = await GetCachedClientIdAsync(cancellationToken);
                if (currentClientId == null)
                {
                    Logger.LogWarning("User context does not contain valid ClientId");
                    return Unauthorized(new { error = "Unable to determine client context" });
                }

                // Validate payment ID format
                if (paymentId == Guid.Empty)
                {
                    return BadRequest(new { error = "Invalid payment ID" });
                }

                var command = new RetryFailedPaymentCommand
                {
                    PaymentId = paymentId,
                    ClientId = currentClientId.Value, // SECURITY: Use authenticated client ID
                    Reason = request.Reason
                };

                var result = await Mediator.Send(command, cancellationToken);

                if (!result.Success)
                {
                    return BadRequest(new { error = result.ErrorMessage });
                }

                Logger.LogInformation("Successfully scheduled retry for payment {PaymentId}", paymentId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error processing retry request for payment {PaymentId}", paymentId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred while processing the retry request" });
            }
        }

        /// <summary>
        /// Retry multiple failed payments in bulk
        /// </summary>
        /// <param name="request">Bulk retry request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result of the bulk retry operation</returns>
        [HttpPost("bulk")]
        [ProducesResponseType(typeof(BulkRetryPaymentsResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BulkRetryPaymentsResult>> BulkRetry(
            [FromBody] BulkRetryPaymentsRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                Logger.LogInformation("Bulk retry request for {Count} payments", request.PaymentIds.Count);

                // SECURITY: Get ClientId from user context with caching
                var currentClientId = await GetCachedClientIdAsync(cancellationToken);
                if (currentClientId == null)
                {
                    Logger.LogWarning("User context does not contain valid ClientId");
                    return Unauthorized(new { error = "Unable to determine client context" });
                }

                // Validate payment IDs
                if (request.PaymentIds == null || !request.PaymentIds.Any())
                {
                    return BadRequest(new { error = "Payment IDs list cannot be empty" });
                }

                if (request.PaymentIds.Any(id => id == Guid.Empty))
                {
                    return BadRequest(new { error = "All payment IDs must be valid GUIDs" });
                }

                // Check bulk operation limits
                if (request.PaymentIds.Count > _securityLimitService.MaxBulkRetryLimit)
                {
                    return BadRequest(new { error = $"Maximum {_securityLimitService.MaxBulkRetryLimit} payments allowed per bulk retry operation" });
                }

                var command = new BulkRetryPaymentsCommand
                {
                    PaymentIds = request.PaymentIds,
                    ClientId = currentClientId.Value, // SECURITY: Use authenticated client ID
                    Reason = request.Reason
                };

                var result = await Mediator.Send(command, cancellationToken);

                Logger.LogInformation("Bulk retry completed: {Successful} successful, {Failed} failed",
                    result.SuccessfulRetries, result.FailedRetries);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error processing bulk retry request");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred while processing the bulk retry request" });
            }
        }

        /// <summary>
        /// Get scheduled retries with filtering and pagination
        /// </summary>
        /// <param name="clientId">Client ID to filter by (optional)</param>
        /// <param name="status">Status to filter by (optional)</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10, max: 100)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated list of scheduled retries</returns>
        [HttpGet("scheduled")]
        [ResponseCache(Duration = 30, VaryByQueryKeys = new[] { "clientId", "status", "pageNumber", "pageSize" })]
        [ProducesResponseType(typeof(PaginatedList<RetryScheduleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedList<RetryScheduleDto>>> GetScheduledRetries(
            [FromQuery] Guid? clientId = null,
            [FromQuery] string? status = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // SECURITY: For Client role, use authenticated client ID
                var currentClientId = await _userContextService.GetCurrentClientIdAsync(cancellationToken);
                var currentUserRole = _userContextService.GetCurrentUserRole();
                
                // If user is Client, they can only see their own retries
                if (currentUserRole == "Client" && currentClientId.HasValue)
                {
                    clientId = currentClientId.Value;
                }

                // Validate and normalize pagination parameters
                var pagination = ValidatePagination(pageNumber, pageSize);

                var query = new GetScheduledRetriesQuery
                {
                    ClientId = clientId,
                    Status = status,
                    Pagination = pagination
                };

                var result = await Mediator.Send(query, cancellationToken);

                Logger.LogInformation("Retrieved {Count} scheduled retries out of {Total}",
                    result.Items.Count, result.TotalCount);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting scheduled retries");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred while retrieving scheduled retries" });
            }
        }

        /// <summary>
        /// Get failed payments that can be retried
        /// </summary>
        /// <param name="clientId">Client ID to filter by (optional)</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10, max: 100)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated list of failed payments</returns>
        [HttpGet("failed")]
        [ResponseCache(Duration = 60, VaryByQueryKeys = new[] { "clientId", "pageNumber", "pageSize" })]
        [ProducesResponseType(typeof(PaginatedList<FailedPaymentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedList<FailedPaymentDto>>> GetFailedPayments(
            [FromQuery] Guid? clientId = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // SECURITY: For Client role, use authenticated client ID
                var currentClientId = await _userContextService.GetCurrentClientIdAsync(cancellationToken);
                var currentUserRole = _userContextService.GetCurrentUserRole();
                
                // If user is Client, they can only see their own failed payments
                if (currentUserRole == "Client" && currentClientId.HasValue)
                {
                    clientId = currentClientId.Value;
                }

                // Validate and normalize pagination parameters
                var pagination = ValidatePagination(pageNumber, pageSize);

                var query = new GetFailedPaymentsForRetryQuery
                {
                    ClientId = clientId,
                    Pagination = pagination
                };

                var result = await Mediator.Send(query, cancellationToken);

                Logger.LogInformation("Retrieved {Count} failed payments out of {Total}",
                    result.Items.Count, result.TotalCount);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting failed payments");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred while retrieving failed payments" });
            }
        }

        /// <summary>
        /// Cancel a scheduled retry
        /// </summary>
        /// <param name="scheduleId">ID of the retry schedule to cancel</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result of the cancellation operation</returns>
        [HttpDelete("{scheduleId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CancelRetry(
            Guid scheduleId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                Logger.LogInformation("Cancel retry request for schedule {ScheduleId}", scheduleId);

                // Validate schedule ID
                if (scheduleId == Guid.Empty)
                {
                    return BadRequest(new { error = "Invalid schedule ID" });
                }

                // SECURITY: Get ClientId from user context with caching
                var currentClientId = await GetCachedClientIdAsync(cancellationToken);
                if (currentClientId == null)
                {
                    Logger.LogWarning("User context does not contain valid ClientId");
                    return Unauthorized(new { error = "Unable to determine client context" });
                }

                // Create cancel retry command
                var command = new CancelRetryCommand
                {
                    ScheduleId = scheduleId,
                    ClientId = currentClientId.Value
                };

                var result = await Mediator.Send(command, cancellationToken);

                if (!result.Success)
                {
                    return BadRequest(new { error = result.ErrorMessage });
                }

                Logger.LogInformation("Successfully cancelled retry schedule {ScheduleId}", scheduleId);
                return NoContent();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error cancelling retry {ScheduleId}", scheduleId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred while cancelling the retry" });
            }
        }

        /// <summary>
        /// Helper method to get cached client context with fallback
        /// </summary>
        private async Task<Guid?> GetCachedClientIdAsync(CancellationToken cancellationToken)
        {
            var cacheKey = $"client_id_{User.Identity?.Name}";
            
            if (_cache.TryGetValue(cacheKey, out Guid cachedClientId))
            {
                return cachedClientId;
            }

            var clientId = await _userContextService.GetCurrentClientIdAsync(cancellationToken);
            if (clientId.HasValue)
            {
                // Cache for 5 minutes
                _cache.Set(cacheKey, clientId.Value, TimeSpan.FromMinutes(5));
            }

            return clientId;
        }

        /// <summary>
        /// Helper method to validate and normalize pagination parameters
        /// </summary>
        private PaginationParams ValidatePagination(int pageNumber, int pageSize)
        {
            var pagination = new PaginationParams(pageNumber, pageSize);
            pagination.Validate(_securityLimitService);
            return pagination;
        }
    }

    /// <summary>
    /// Request model for retry payment
    /// </summary>
    public class RetryPaymentRequest
    {
        /// <summary>
        /// Reason for the retry (optional)
        /// </summary>
        public string? Reason { get; set; }
    }

    /// <summary>
    /// Request model for bulk retry payments
    /// </summary>
    public class BulkRetryPaymentsRequest
    {
        /// <summary>
        /// List of payment IDs to retry
        /// </summary>
        public List<Guid> PaymentIds { get; set; } = new();

        /// <summary>
        /// Reason for the bulk retry (optional)
        /// </summary>
        public string? Reason { get; set; }
    }
}
