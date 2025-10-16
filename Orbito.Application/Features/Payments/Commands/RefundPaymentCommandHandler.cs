using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Features.Payments.Commands;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Features.Payments.Commands
{
    /// <summary>
    /// Handler dla komendy zwrotu płatności
    /// </summary>
    public class RefundPaymentCommandHandler : IRequestHandler<RefundPaymentCommand, Result<RefundPaymentResult>>
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

        public async Task<Result<RefundPaymentResult>> Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Sprawdź czy mamy kontekst tenanta
                if (!_tenantContext.HasTenant)
                {
                    _logger.LogWarning("Attempted to refund payment without tenant context");
                    return Result.Failure<RefundPaymentResult>(DomainErrors.Tenant.NoTenantContext);
                }

                _logger.LogInformation("Processing refund for payment {PaymentId} with amount {Amount} {Currency}",
                    request.PaymentId, request.Amount, request.Currency);

                // Sprawdź czy płatność istnieje
                // NOTE: Using deprecated method because this command is only accessible by Providers and PlatformAdmins
                // who have proper authorization to view all payments in their tenant
#pragma warning disable CS0618 // Type or member is obsolete
                var payment = await _unitOfWork.Payments.GetByIdAsync(request.PaymentId, cancellationToken);
#pragma warning restore CS0618 // Type or member is obsolete
                if (payment == null)
                {
                    _logger.LogWarning("Payment {PaymentId} not found", request.PaymentId);
                    return Result.Failure<RefundPaymentResult>(DomainErrors.Payment.NotFound);
                }

                // Sprawdź czy płatność należy do klienta w ramach tego samego tenanta
                // Assuming payment has a subscription and subscription has a client
                if (payment.Subscription?.Client?.TenantId != _tenantContext.CurrentTenantId)
                {
                    _logger.LogWarning("Payment {PaymentId} does not belong to current tenant {TenantId}",
                        request.PaymentId, _tenantContext.CurrentTenantId);
                    return Result.Failure<RefundPaymentResult>(DomainErrors.Tenant.CrossTenantAccess);
                }

                // Sprawdź czy płatność może być zwrócona
                if (!payment.CanBeRefunded())
                {
                    _logger.LogWarning("Payment {PaymentId} cannot be refunded", request.PaymentId);
                    return Result.Failure<RefundPaymentResult>(DomainErrors.Payment.CannotRefund);
                }

                // Sprawdź czy kwota zwrotu nie przekracza kwoty płatności
                var refundAmount = Money.Create(request.Amount, request.Currency);
                if (refundAmount.Amount > payment.Amount.Amount)
                {
                    _logger.LogWarning("Refund amount {RefundAmount} exceeds payment amount {PaymentAmount}", 
                        refundAmount.Amount, payment.Amount.Amount);
                    return Result.Failure<RefundPaymentResult>(DomainErrors.Payment.AmountMismatch);
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

                    return Result.Success(RefundPaymentResult.Success(
                        result.ExternalRefundId ?? string.Empty,
                        result.Status.ToString()));
                }
                else
                {
                    _logger.LogError("Failed to refund payment {PaymentId}: {ErrorMessage}", 
                        request.PaymentId, result.ErrorMessage);

                    return Result.Failure<RefundPaymentResult>(DomainErrors.Payment.ProcessingFailed);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund for payment {PaymentId}", request.PaymentId);
                return Result.Failure<RefundPaymentResult>(DomainErrors.General.UnexpectedError);
            }
        }
    }
}
