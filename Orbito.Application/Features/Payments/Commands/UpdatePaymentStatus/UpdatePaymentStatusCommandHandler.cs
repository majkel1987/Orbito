using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;
using Orbito.Domain.Enums;

namespace Orbito.Application.Features.Payments.Commands.UpdatePaymentStatus;

public class UpdatePaymentStatusCommandHandler : IRequestHandler<UpdatePaymentStatusCommand, Result<PaymentDto>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdatePaymentStatusCommandHandler> _logger;

    public UpdatePaymentStatusCommandHandler(
        IPaymentRepository paymentRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork,
        ILogger<UpdatePaymentStatusCommandHandler> logger)
    {
        _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<PaymentDto>> Handle(UpdatePaymentStatusCommand request, CancellationToken cancellationToken)
    {
        // Check for cancellation before starting
        cancellationToken.ThrowIfCancellationRequested();

        // Validate tenant context
        if (!_tenantContext.HasTenant)
        {
            _logger.LogWarning("UpdatePaymentStatus attempted without tenant context");
            return Result.Failure<PaymentDto>(DomainErrors.Tenant.NoTenantContext);
        }

        // Fetch payment with ClientId verification (security best practice)
        var payment = await _paymentRepository.GetByIdForClientAsync(request.PaymentId, request.ClientId, cancellationToken);
        if (payment == null)
        {
            _logger.LogWarning("Payment {PaymentId} not found for client {ClientId}", request.PaymentId, request.ClientId);
            return Result.Failure<PaymentDto>(DomainErrors.Payment.NotFound);
        }

        // Verify payment belongs to current tenant
        if (payment.TenantId != _tenantContext.CurrentTenantId)
        {
            _logger.LogWarning("Cross-tenant access attempt: Payment {PaymentId} belongs to different tenant", request.PaymentId);
            return Result.Failure<PaymentDto>(DomainErrors.Tenant.CrossTenantAccess);
        }

        // Validate status transition and required fields
        var validationError = ValidateStatusTransition(request, payment.Status);
        if (validationError != null)
        {
            return Result.Failure<PaymentDto>(validationError);
        }

        // Update payment status
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
                _logger.LogWarning("Unsupported payment status: {Status}", request.Status);
                return Result.Failure<PaymentDto>(DomainErrors.Payment.UnsupportedStatus);
        }

        // Save changes
        await _paymentRepository.UpdateAsync(payment, cancellationToken);
        var saveResult = await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (!saveResult.IsSuccess)
        {
            _logger.LogError("Failed to save payment status update: {Error}", saveResult.ErrorMessage);
            var error = Error.Create("Payment.SaveFailed", saveResult.ErrorMessage ?? "Failed to save payment changes");
            return Result.Failure<PaymentDto>(error);
        }

        // Use already updated object instead of re-querying from database
        return Result.Success(PaymentMapper.ToDto(payment));
    }

    private static Error? ValidateStatusTransition(UpdatePaymentStatusCommand request, PaymentStatus currentStatus)
    {
        if (!IsValidStatusTransition(currentStatus, request.Status))
        {
            return DomainErrors.Payment.InvalidStatusTransition;
        }

        if (request.Status == PaymentStatus.Failed && string.IsNullOrWhiteSpace(request.FailureReason))
        {
            return DomainErrors.Payment.FailureReasonRequired;
        }

        if ((request.Status == PaymentStatus.Refunded || request.Status == PaymentStatus.PartiallyRefunded)
            && string.IsNullOrWhiteSpace(request.RefundReason))
        {
            return DomainErrors.Payment.RefundReasonRequired;
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
}
