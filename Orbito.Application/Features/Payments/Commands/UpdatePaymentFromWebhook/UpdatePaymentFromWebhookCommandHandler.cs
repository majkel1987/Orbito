using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Common;
using Orbito.Domain.Enums;
using Orbito.Domain.Errors;

namespace Orbito.Application.Features.Payments.Commands.UpdatePaymentFromWebhook;

/// <summary>
/// Handler for updating payment from webhook.
/// NOTE: Uses GetByIdUnsafeAsync because webhooks are external system callbacks
/// that need to update payments regardless of tenant context.
/// Tenant verification is still performed when context is available.
/// </summary>
public class UpdatePaymentFromWebhookCommandHandler : IRequestHandler<UpdatePaymentFromWebhookCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdatePaymentFromWebhookCommandHandler> _logger;
    private readonly ITenantContext _tenantContext;

    public UpdatePaymentFromWebhookCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<UpdatePaymentFromWebhookCommandHandler> logger,
        ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    public async Task<Result> Handle(UpdatePaymentFromWebhookCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check for cancellation before starting
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Updating payment {PaymentId} from webhook event {EventType} with EventId {EventId}",
                request.PaymentId, request.EventType, request.EventId);

            // Check idempotency - verify if this webhook event has already been processed
            var existingWebhookLog = await _unitOfWork.WebhookLogs.GetByEventIdAsync(request.EventId, cancellationToken);
            if (existingWebhookLog != null && existingWebhookLog.Status == WebhookStatus.Processed)
            {
                _logger.LogInformation("Webhook event {EventId} for payment {PaymentId} already processed. Skipping update.",
                    request.EventId, request.PaymentId);
                return Result.Success();
            }

            // Get the payment using unsafe method - webhooks are external callbacks without tenant context
            var payment = await _unitOfWork.Payments.GetByIdUnsafeAsync(request.PaymentId, cancellationToken);
            if (payment == null)
            {
                _logger.LogWarning("Payment {PaymentId} not found", request.PaymentId);
                return Result.Failure(DomainErrors.Payment.NotFound);
            }

            // Security: Verify tenant context when available (defense in depth)
            if (_tenantContext.HasTenant && payment.TenantId != _tenantContext.CurrentTenantId)
            {
                _logger.LogWarning("Tenant mismatch for payment {PaymentId}. Expected: {ExpectedTenant}, Actual: {ActualTenant}",
                    request.PaymentId, _tenantContext.CurrentTenantId, payment.TenantId);
                return Result.Failure(DomainErrors.Tenant.CrossTenantAccess);
            }

            // Validate status transition
            if (!payment.CanTransitionTo(request.NewStatus))
            {
                _logger.LogWarning("Invalid status transition for payment {PaymentId} from {CurrentStatus} to {NewStatus}",
                    request.PaymentId, payment.Status, request.NewStatus);
                return Result.Failure(DomainErrors.Payment.InvalidStatus);
            }

            // Verify ExternalPaymentId matches
            if (!string.IsNullOrEmpty(payment.ExternalPaymentId) &&
                !string.IsNullOrEmpty(request.ExternalPaymentId) &&
                payment.ExternalPaymentId != request.ExternalPaymentId)
            {
                _logger.LogWarning("ExternalPaymentId mismatch for payment {PaymentId}. Expected: {Expected}, Received: {Received}",
                    request.PaymentId, payment.ExternalPaymentId, request.ExternalPaymentId);
                return Result.Failure(DomainErrors.Payment.ExternalPaymentIdRequired);
            }

            // Update payment status based on webhook event
            switch (request.EventType)
            {
                case "payment_intent.succeeded":
                    payment.MarkAsCompleted();
                    _logger.LogInformation("Payment {PaymentId} marked as completed from webhook", request.PaymentId);
                    break;

                case "payment_intent.payment_failed":
                    var failureReason = request.ErrorMessage ?? "Payment failed";
                    payment.MarkAsFailed(failureReason);
                    _logger.LogInformation("Payment {PaymentId} marked as failed from webhook: {Reason}", request.PaymentId, failureReason);
                    break;

                case "charge.refunded":
                    var refundReason = request.ErrorMessage ?? "Payment refunded via webhook";
                    payment.MarkAsRefunded(refundReason);
                    _logger.LogInformation("Payment {PaymentId} marked as refunded from webhook: {Reason}", request.PaymentId, refundReason);
                    break;

                case "invoice.payment_succeeded":
                    payment.MarkAsCompleted();
                    _logger.LogInformation("Payment {PaymentId} marked as completed from invoice webhook", request.PaymentId);
                    break;

                case "invoice.payment_failed":
                    var invoiceFailureReason = request.ErrorMessage ?? "Invoice payment failed";
                    payment.MarkAsFailed(invoiceFailureReason);
                    _logger.LogInformation("Payment {PaymentId} marked as failed from invoice webhook: {Reason}", request.PaymentId, invoiceFailureReason);
                    break;

                default:
                    _logger.LogWarning("Unknown webhook event type {EventType} for payment {PaymentId}", request.EventType, request.PaymentId);
                    return Result.Failure(DomainErrors.General.UnexpectedError);
            }

            // Update external payment ID if provided
            if (!string.IsNullOrEmpty(request.ExternalPaymentId))
            {
                payment.SetExternalPaymentId(request.ExternalPaymentId);
            }

            // Log metadata (Payment entity could be extended to persist this)
            foreach (var metadata in request.Metadata)
            {
                _logger.LogDebug("Webhook metadata {Key}: {Value} for payment {PaymentId}", metadata.Key, metadata.Value, request.PaymentId);
            }

            // Save changes
            await _unitOfWork.Payments.UpdateAsync(payment, cancellationToken);
            var saveResult = await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (!saveResult.IsSuccess)
            {
                _logger.LogError("Failed to save payment update from webhook: {Error}", saveResult.ErrorMessage);
                var error = Error.Create("Payment.SaveFailed", saveResult.ErrorMessage ?? "Failed to save payment update");
                return Result.Failure(error);
            }

            _logger.LogInformation("Successfully updated payment {PaymentId} from webhook", request.PaymentId);
            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            // Rethrow cancellation exceptions - they should not be caught
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment {PaymentId} from webhook", request.PaymentId);
            return Result.Failure(DomainErrors.General.UnexpectedError);
        }
    }
}
