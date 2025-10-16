using MediatR;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.DTOs;
using Orbito.Domain.Enums;

namespace Orbito.Application.Features.Payments.Commands.UpdatePaymentStatus
{
    public class UpdatePaymentStatusCommandHandler : IRequestHandler<UpdatePaymentStatusCommand, UpdatePaymentStatusResult>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ITenantContext _tenantContext;
        private readonly IUnitOfWork _unitOfWork;

        public UpdatePaymentStatusCommandHandler(
            IPaymentRepository paymentRepository,
            ITenantContext tenantContext,
            IUnitOfWork unitOfWork)
        {
            _paymentRepository = paymentRepository;
            _tenantContext = tenantContext;
            _unitOfWork = unitOfWork;
        }

        public async Task<UpdatePaymentStatusResult> Handle(UpdatePaymentStatusCommand request, CancellationToken cancellationToken)
        {
            // Sprawdź czy mamy kontekst tenanta
            if (!_tenantContext.HasTenant)
            {
                return UpdatePaymentStatusResult.FailureResult("Tenant context is required");
            }

            // Pobierz płatność
            // NOTE: Using deprecated method because this command is only accessible by Providers and PlatformAdmins
            // who have proper authorization to view all payments in their tenant
#pragma warning disable CS0618 // Type or member is obsolete
            var payment = await _paymentRepository.GetByIdAsync(request.PaymentId, cancellationToken);
#pragma warning restore CS0618 // Type or member is obsolete
            if (payment == null)
            {
                return UpdatePaymentStatusResult.FailureResult($"Payment with ID {request.PaymentId} not found");
            }

            // Sprawdź czy płatność należy do tego samego tenanta
            if (payment.TenantId != _tenantContext.CurrentTenantId)
            {
                return UpdatePaymentStatusResult.FailureResult("Payment does not belong to current tenant");
            }

            // Walidacja przejścia między statusami i wymaganych pól
            var validationError = ValidateStatusTransition(request, payment.Status);
            if (validationError != null)
            {
                return UpdatePaymentStatusResult.FailureResult(validationError);
            }

            // Aktualizuj status płatności
            switch (request.Status)
            {
                case PaymentStatus.Processing:
                    payment.MarkAsProcessing();
                    break;
                case PaymentStatus.Completed:
                    payment.MarkAsCompleted();
                    break;
                case PaymentStatus.Failed:
                    payment.MarkAsFailed(request.FailureReason ?? "No failure reason provided");
                    break;
                case PaymentStatus.Cancelled:
                    payment.MarkAsCancelled();
                    break;
                case PaymentStatus.Refunded:
                    payment.MarkAsRefunded(request.RefundReason ?? "No refund reason provided");
                    break;
                case PaymentStatus.PartiallyRefunded:
                    var refundedAmount = request.RefundedAmount ?? payment.Amount;
                    payment.MarkAsPartiallyRefunded(request.RefundReason ?? "No refund reason provided", refundedAmount);
                    break;
                default:
                    return UpdatePaymentStatusResult.FailureResult($"Unsupported payment status: {request.Status}");
            }

            // Zapisz zmiany
            await _paymentRepository.UpdateAsync(payment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Użyj już zaktualizowanego obiektu zamiast ponownego odczytu z bazy
            var paymentDto = MapToDto(payment);
            return UpdatePaymentStatusResult.SuccessResult(paymentDto);
        }

        private static string? ValidateStatusTransition(UpdatePaymentStatusCommand request, PaymentStatus currentStatus)
        {
            if (!IsValidStatusTransition(currentStatus, request.Status))
            {
                return $"Invalid status transition from {currentStatus} to {request.Status}";
            }

            if (request.Status == PaymentStatus.Failed && string.IsNullOrWhiteSpace(request.FailureReason))
            {
                return "FailureReason is required when marking payment as Failed";
            }

            if ((request.Status == PaymentStatus.Refunded || request.Status == PaymentStatus.PartiallyRefunded)
                && string.IsNullOrWhiteSpace(request.RefundReason))
            {
                return $"RefundReason is required when marking payment as {request.Status}";
            }

            return null;
        }

        private static bool IsValidStatusTransition(PaymentStatus from, PaymentStatus to)
        {
            return (from, to) switch
            {
                (PaymentStatus.Pending, PaymentStatus.Processing) => true,
                (PaymentStatus.Pending, PaymentStatus.Cancelled) => true,
                (PaymentStatus.Processing, PaymentStatus.Completed) => true,
                (PaymentStatus.Processing, PaymentStatus.Failed) => true,
                (PaymentStatus.Completed, PaymentStatus.Refunded) => true,
                (PaymentStatus.Completed, PaymentStatus.PartiallyRefunded) => true,
                _ => false
            };
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
