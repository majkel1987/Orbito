using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;

namespace Orbito.Application.Features.Payments.Queries.GetPaymentsBySubscription
{
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

                // Validate tenant context
                if (!_tenantContext.HasTenant)
                {
                    _logger.LogWarning("Missing tenant context for payments query");
                    return Result.Failure<GetPaymentsBySubscriptionResponse>(DomainErrors.Tenant.NoTenantContext);
                }

                // Validate pagination parameters
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

                // Get payments for subscription
                var payments = await _paymentRepository.GetBySubscriptionIdAsync(
                    request.SubscriptionId,
                    request.PageNumber,
                    request.PageSize,
                    cancellationToken);

                // Get total count of payments for this subscription
                var totalCount = await _paymentRepository.GetCountBySubscriptionIdAsync(
                    request.SubscriptionId,
                    cancellationToken);

                var paymentDtos = payments.Select(MapToDto).ToList();
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

        private static PaymentDto MapToDto(Domain.Entities.Payment payment)
        {
            return new PaymentDto
            {
                Id = payment.Id,
                TenantId = payment.TenantId.Value,
                SubscriptionId = payment.SubscriptionId,
                ClientId = payment.ClientId,
                Amount = payment.Amount.Amount,
                Currency = payment.Amount.Currency,
                Status = payment.Status.ToString(),
                ExternalTransactionId = payment.ExternalTransactionId,
                PaymentMethod = payment.PaymentMethod,
                ExternalPaymentId = payment.ExternalPaymentId,
                PaymentMethodId = payment.PaymentMethodId,
                CreatedAt = payment.CreatedAt,
                ProcessedAt = payment.ProcessedAt,
                FailedAt = payment.FailedAt,
                RefundedAt = payment.RefundedAt,
                FailureReason = payment.FailureReason
            };
        }
    }
}
