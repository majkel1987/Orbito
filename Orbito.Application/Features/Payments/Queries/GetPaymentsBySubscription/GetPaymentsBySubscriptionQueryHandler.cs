using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.DTOs;

namespace Orbito.Application.Features.Payments.Queries.GetPaymentsBySubscription
{
    public class GetPaymentsBySubscriptionQueryHandler : IRequestHandler<GetPaymentsBySubscriptionQuery, GetPaymentsBySubscriptionResult>
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

        public async Task<GetPaymentsBySubscriptionResult> Handle(GetPaymentsBySubscriptionQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Getting payments for subscription {SubscriptionId}, page {PageNumber}, size {PageSize}",
                    request.SubscriptionId, request.PageNumber, request.PageSize);

                // Validate tenant context
                if (!_tenantContext.HasTenant)
                {
                    _logger.LogWarning("Missing tenant context for payments query");
                    return GetPaymentsBySubscriptionResult.FailureResult("Access denied");
                }

                // Validate pagination parameters
                if (request.PageNumber < 1)
                {
                    _logger.LogWarning("Invalid page number: {PageNumber}", request.PageNumber);
                    return GetPaymentsBySubscriptionResult.FailureResult("Invalid page number");
                }

                if (request.PageSize < 1 || request.PageSize > 100)
                {
                    _logger.LogWarning("Invalid page size: {PageSize}", request.PageSize);
                    return GetPaymentsBySubscriptionResult.FailureResult("Invalid page size");
                }

                var subscription = await _subscriptionRepository.GetByIdAsync(request.SubscriptionId, cancellationToken);

                if (subscription == null || subscription.TenantId != _tenantContext.CurrentTenantId)
                {
                    _logger.LogWarning(
                        "Unauthorized access attempt or subscription not found. SubscriptionId: {SubscriptionId}, TenantId: {TenantId}",
                        request.SubscriptionId,
                        _tenantContext.CurrentTenantId);
                    return GetPaymentsBySubscriptionResult.FailureResult("Subscription not found");
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

                _logger.LogDebug("Retrieved {PaymentCount} payments for subscription {SubscriptionId} (total: {TotalCount})",
                    paymentDtos.Count, request.SubscriptionId, totalCount);

                return GetPaymentsBySubscriptionResult.SuccessResult(
                    paymentDtos,
                    totalCount,
                    request.PageNumber,
                    request.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error retrieving payments. SubscriptionId: {SubscriptionId}, TenantId: {TenantId}",
                    request.SubscriptionId,
                    _tenantContext.CurrentTenantId);
                return GetPaymentsBySubscriptionResult.FailureResult("An error occurred while retrieving payments");
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
