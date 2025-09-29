using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Features.Payments.Commands;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Features.Payments.Commands
{
    /// <summary>
    /// Handler dla komendy zwrotu płatności
    /// </summary>
    public class RefundPaymentCommandHandler : IRequestHandler<RefundPaymentCommand, RefundPaymentResult>
    {
        private readonly IPaymentProcessingService _paymentProcessingService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantContext _tenantContext;
        private readonly ILogger<RefundPaymentCommandHandler> _logger;

        public RefundPaymentCommandHandler(
            IPaymentProcessingService paymentProcessingService,
            IUnitOfWork unitOfWork,
            ITenantContext tenantContext,
            ILogger<RefundPaymentCommandHandler> logger)
        {
            _paymentProcessingService = paymentProcessingService;
            _unitOfWork = unitOfWork;
            _tenantContext = tenantContext;
            _logger = logger;
        }

        public async Task<RefundPaymentResult> Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Sprawdź czy mamy kontekst tenanta
                if (!_tenantContext.HasTenant)
                {
                    _logger.LogWarning("Attempted to refund payment without tenant context");
                    return RefundPaymentResult.Failure("Tenant context is required", "TENANT_CONTEXT_REQUIRED");
                }

                _logger.LogInformation("Processing refund for payment {PaymentId} with amount {Amount} {Currency}",
                    request.PaymentId, request.Amount, request.Currency);

                // Sprawdź czy płatność istnieje
                var payment = await _unitOfWork.Payments.GetByIdAsync(request.PaymentId, cancellationToken);
                if (payment == null)
                {
                    _logger.LogWarning("Payment {PaymentId} not found", request.PaymentId);
                    return RefundPaymentResult.Failure("Payment not found", "PAYMENT_NOT_FOUND");
                }

                // Sprawdź czy płatność należy do klienta w ramach tego samego tenanta
                // Assuming payment has a subscription and subscription has a client
                if (payment.Subscription?.Client?.TenantId != _tenantContext.CurrentTenantId)
                {
                    _logger.LogWarning("Payment {PaymentId} does not belong to current tenant {TenantId}",
                        request.PaymentId, _tenantContext.CurrentTenantId);
                    return RefundPaymentResult.Failure("Access denied", "ACCESS_DENIED");
                }

                // Sprawdź czy płatność może być zwrócona
                if (!payment.CanBeRefunded())
                {
                    _logger.LogWarning("Payment {PaymentId} cannot be refunded", request.PaymentId);
                    return RefundPaymentResult.Failure("Payment cannot be refunded", "PAYMENT_CANNOT_BE_REFUNDED");
                }

                // Sprawdź czy kwota zwrotu nie przekracza kwoty płatności
                var refundAmount = Money.Create(request.Amount, request.Currency);
                if (refundAmount.Amount > payment.Amount.Amount)
                {
                    _logger.LogWarning("Refund amount {RefundAmount} exceeds payment amount {PaymentAmount}", 
                        refundAmount.Amount, payment.Amount.Amount);
                    return RefundPaymentResult.Failure("Refund amount exceeds payment amount", "REFUND_AMOUNT_EXCEEDS_PAYMENT");
                }

                // Przetwórz zwrot przez payment gateway
                var result = await _paymentProcessingService.RefundPaymentAsync(
                    request.PaymentId,
                    refundAmount,
                    request.Reason,
                    cancellationToken);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Payment {PaymentId} refunded successfully with external ID {ExternalRefundId}", 
                        request.PaymentId, result.ExternalRefundId);

                    return RefundPaymentResult.Success(
                        result.ExternalRefundId ?? string.Empty,
                        result.Status.ToString());
                }
                else
                {
                    _logger.LogError("Failed to refund payment {PaymentId}: {ErrorMessage}", 
                        request.PaymentId, result.ErrorMessage);

                    return RefundPaymentResult.Failure(
                        result.ErrorMessage ?? "Refund failed",
                        result.ErrorCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund for payment {PaymentId}", request.PaymentId);
                return RefundPaymentResult.Failure("An error occurred while processing refund", "REFUND_PROCESSING_ERROR");
            }
        }
    }
}
