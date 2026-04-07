using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Features.Payments.Commands;

/// <summary>
/// Handler for refund payment command
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

                // Sprawdź czy płatność istnieje z weryfikacją ClientId (security best practice)
                var payment = await _unitOfWork.Payments.GetByIdForClientAsync(request.PaymentId, request.ClientId, cancellationToken);
                if (payment == null)
                {
                    _logger.LogWarning("Payment {PaymentId} not found for client {ClientId}", request.PaymentId, request.ClientId);
                    return Result.Failure<RefundPaymentResult>(DomainErrors.Payment.NotFound);
                }

                // Sprawdź czy płatność należy do tego samego tenanta
                if (payment.TenantId != _tenantContext.CurrentTenantId)
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

                    return Result.Success(new RefundPaymentResult
                    {
                        ExternalRefundId = result.ExternalRefundId ?? string.Empty,
                        Status = result.Status.ToString()
                    });
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
