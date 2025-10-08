using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models;
using Orbito.Domain.Enums;

namespace Orbito.Application.Features.Payments.Queries
{
    /// <summary>
    /// Handler for get failed payments for retry query
    /// </summary>
    public class GetFailedPaymentsForRetryQueryHandler : IRequestHandler<GetFailedPaymentsForRetryQuery, PaginatedList<FailedPaymentDto>>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPaymentRetryRepository _retryRepository;
        private readonly IUserContextService _userContextService;
        private readonly ISecurityLimitService _securityLimitService;
        private readonly ILogger<GetFailedPaymentsForRetryQueryHandler> _logger;

        public GetFailedPaymentsForRetryQueryHandler(
            IPaymentRepository paymentRepository,
            IPaymentRetryRepository retryRepository,
            IUserContextService userContextService,
            ISecurityLimitService securityLimitService,
            ILogger<GetFailedPaymentsForRetryQueryHandler> logger)
        {
            _paymentRepository = paymentRepository;
            _retryRepository = retryRepository;
            _userContextService = userContextService;
            _securityLimitService = securityLimitService;
            _logger = logger;
        }

        public async Task<PaginatedList<FailedPaymentDto>> Handle(GetFailedPaymentsForRetryQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // SECURITY: Validate pagination parameters to prevent DoS attacks
                request.Pagination.Validate(_securityLimitService);

                _logger.LogInformation("Getting failed payments for retry with filters: ClientId={ClientId}, Page={Page}, PageSize={PageSize}",
                    request.ClientId, request.Pagination.PageNumber, request.Pagination.PageSize);

                // Get current user context for security
                var currentClientId = await _userContextService.GetCurrentClientIdAsync();
                var currentUserRole = _userContextService.GetCurrentUserRole();

                // SECURITY: Prevent cross-client data access
                // Providers can query any client in their tenant
                // Clients can only query their own data
                Guid? clientId;
                if (currentUserRole == UserRole.Provider.ToString() || currentUserRole == UserRole.PlatformAdmin.ToString())
                {
                    // Providers/Admins can specify clientId or see all
                    clientId = request.ClientId ?? currentClientId;
                }
                else
                {
                    // Clients must use their own clientId
                    if (request.ClientId.HasValue && request.ClientId != currentClientId)
                    {
                        _logger.LogWarning("Client {CurrentClientId} attempted to access data for client {RequestedClientId}",
                            currentClientId, request.ClientId);
                        throw new UnauthorizedAccessException("You can only access your own payment data");
                    }
                    clientId = currentClientId;
                }

                if (!clientId.HasValue)
                {
                    _logger.LogWarning("No client context available for failed payments query");
                    return PaginatedList<FailedPaymentDto>.Empty(request.Pagination.PageNumber, request.Pagination.PageSize);
                }

                // Build query for failed payments
                var query = await _paymentRepository.GetFailedPaymentsQueryAsync(
                    clientId: clientId.Value,
                    cancellationToken: cancellationToken);

                // Apply pagination with read-only optimization
                var totalCount = await query.CountAsync(cancellationToken);
                var payments = await query
                    .AsNoTracking()
                    .OrderByDescending(p => p.FailedAt)
                    .Skip((request.Pagination.PageNumber - 1) * request.Pagination.PageSize)
                    .Take(request.Pagination.PageSize)
                    .ToListAsync(cancellationToken);

                // Get retry information for each payment
                var paymentIds = payments.Select(p => p.Id).ToList();

                // SECURITY: Pass clientId to prevent cross-client data leak
                var retrySchedules = await _retryRepository.GetRetrySchedulesByPaymentIdsAsync(
                    paymentIds,
                    clientId.Value,
                    cancellationToken);

                var retryLookup = retrySchedules
                    .GroupBy(rs => rs.PaymentId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Map to DTOs
                var items = payments.Select(payment =>
                {
                    var paymentRetries = retryLookup.GetValueOrDefault(payment.Id) ?? new List<Domain.Entities.PaymentRetrySchedule>();
                    var activeRetry = paymentRetries.FirstOrDefault(r => r.Status == RetryStatus.Scheduled || r.Status == RetryStatus.InProgress);
                    var completedRetries = paymentRetries.Count(r => r.Status == RetryStatus.Completed);

                    // SECURITY: Use the highest MaxAttempts from all retry schedules to avoid incorrect CanRetry
                    // If there are no retries, default to 5 attempts
                    var maxAttempts = paymentRetries.Any()
                        ? paymentRetries.Max(r => r.MaxAttempts)
                        : 5;

                    return new FailedPaymentDto
                    {
                        Id = payment.Id,
                        ClientId = payment.ClientId,
                        SubscriptionId = payment.SubscriptionId,
                        Amount = payment.Amount.Amount,
                        Currency = payment.Amount.Currency.Code,
                        CreatedAt = payment.CreatedAt,
                        FailedAt = payment.FailedAt,
                        FailureReason = payment.FailureReason,
                        ExternalTransactionId = payment.ExternalTransactionId,
                        PaymentMethod = payment.PaymentMethod ?? "Unknown",
                        HasActiveRetry = activeRetry != null,
                        RetryAttempts = completedRetries,
                        CanRetry = payment.Status == PaymentStatus.Failed &&
                                  activeRetry == null &&
                                  completedRetries < maxAttempts
                    };
                }).ToList();

                _logger.LogInformation("Retrieved {Count} failed payments out of {Total}", items.Count, totalCount);

                return new PaginatedList<FailedPaymentDto>(items, totalCount, request.Pagination.PageNumber, request.Pagination.PageSize);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Request cancelled while getting failed payments for retry");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting failed payments for retry for client {ClientId}", request.ClientId);
                throw; // Let global exception handler deal with it
            }
        }
    }
}
