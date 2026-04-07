using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;

namespace Orbito.Application.Features.Payments.Queries.GetPaymentsBySubscription;

/// <summary>
/// Handler for retrieving paginated payments for a specific subscription with ownership verification.
/// </summary>
public class GetPaymentsBySubscriptionQueryHandler : IRequestHandler<GetPaymentsBySubscriptionQuery, Result<GetPaymentsBySubscriptionResponse>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetPaymentsBySubscriptionQueryHandler> _logger;

    public GetPaymentsBySubscriptionQueryHandler(
        IPaymentRepository paymentRepository,
        ISubscriptionRepository subscriptionRepository,
        ITenantContext tenantContext,
        ILogger<GetPaymentsBySubscriptionQueryHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _subscriptionRepository = subscriptionRepository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<GetPaymentsBySubscriptionResponse>> Handle(GetPaymentsBySubscriptionQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Getting payments for subscription {SubscriptionId}, page {PageNumber}, size {PageSize}",
                request.SubscriptionId, request.PageNumber, request.PageSize);

            if (!_tenantContext.HasTenant)
            {
                _logger.LogWarning("Missing tenant context for payments query");
                return Result.Failure<GetPaymentsBySubscriptionResponse>(DomainErrors.Tenant.NoTenantContext);
            }

            if (request.PageNumber < 1)
            {
                _logger.LogWarning("Invalid page number: {PageNumber}", request.PageNumber);
                return Result.Failure<GetPaymentsBySubscriptionResponse>(DomainErrors.Validation.InvalidPageNumber);
            }

            if (request.PageSize < 1 || request.PageSize > 100)
            {
                _logger.LogWarning("Invalid page size: {PageSize}", request.PageSize);
                return Result.Failure<GetPaymentsBySubscriptionResponse>(DomainErrors.Validation.InvalidPageSize);
            }

            // SECURITY: Use ForClient method to verify ownership
            var subscription = await _subscriptionRepository.GetByIdForClientAsync(request.SubscriptionId, request.ClientId, cancellationToken);

            if (subscription == null || subscription.TenantId != _tenantContext.CurrentTenantId)
            {
                _logger.LogWarning(
                    "Unauthorized access attempt or subscription not found. SubscriptionId: {SubscriptionId}, ClientId: {ClientId}, TenantId: {TenantId}",
                    request.SubscriptionId,
                    request.ClientId,
                    _tenantContext.CurrentTenantId);
                return Result.Failure<GetPaymentsBySubscriptionResponse>(DomainErrors.Subscription.NotFound);
            }

            var payments = await _paymentRepository.GetBySubscriptionIdAsync(
                request.SubscriptionId,
                request.PageNumber,
                request.PageSize,
                cancellationToken);

            var totalCount = await _paymentRepository.GetCountBySubscriptionIdAsync(
                request.SubscriptionId,
                cancellationToken);

            var paymentDtos = PaymentMapper.ToDto(payments).ToList();
            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            _logger.LogDebug("Retrieved {PaymentCount} payments for subscription {SubscriptionId} (total: {TotalCount})",
                paymentDtos.Count, request.SubscriptionId, totalCount);

            var response = new GetPaymentsBySubscriptionResponse
            {
                Payments = paymentDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = totalPages
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving payments. SubscriptionId: {SubscriptionId}, TenantId: {TenantId}",
                request.SubscriptionId,
                _tenantContext.CurrentTenantId);
            return Result.Failure<GetPaymentsBySubscriptionResponse>(DomainErrors.General.UnexpectedError);
        }
    }
}
