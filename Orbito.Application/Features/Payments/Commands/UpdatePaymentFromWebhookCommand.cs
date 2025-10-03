using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models;
using Orbito.Domain.Enums;
using System.Text.Json;

namespace Orbito.Application.Features.Payments.Commands.UpdatePaymentFromWebhook
{
    /// <summary>
    /// Command for updating payment from webhook data
    /// </summary>
    public record UpdatePaymentFromWebhookCommand : IRequest<Result>
    {
        /// <summary>
        /// Payment ID to update
        /// </summary>
        public required Guid PaymentId { get; init; }

        /// <summary>
        /// Webhook event ID for idempotency
        /// </summary>
        public required string EventId { get; init; }

        /// <summary>
        /// Webhook event type
        /// </summary>
        public required string EventType { get; init; }

        /// <summary>
        /// Webhook payload data
        /// </summary>
        public required string Payload { get; init; }

        /// <summary>
        /// External payment ID from webhook
        /// </summary>
        public required string ExternalPaymentId { get; init; }

        /// <summary>
        /// New payment status from webhook
        /// </summary>
        public required PaymentStatus NewStatus { get; init; }

        /// <summary>
        /// Error message if payment failed
        /// </summary>
        public string? ErrorMessage { get; init; }

        /// <summary>
        /// Additional metadata from webhook
        /// </summary>
        public Dictionary<string, string> Metadata { get; init; } = new();
    }

    /// <summary>
    /// Handler for updating payment from webhook
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

                // Get the payment
                var payment = await _unitOfWork.Payments.GetByIdAsync(request.PaymentId, cancellationToken);
                if (payment == null)
                {
                    _logger.LogWarning("Payment {PaymentId} not found", request.PaymentId);
                    return Result.Failure("Payment not found");
                }

                // Security: Verify tenant context
                if (_tenantContext.HasTenant && payment.TenantId != _tenantContext.CurrentTenantId)
                {
                    _logger.LogWarning("Tenant mismatch for payment {PaymentId}. Expected: {ExpectedTenant}, Actual: {ActualTenant}",
                        request.PaymentId, _tenantContext.CurrentTenantId, payment.TenantId);
                    return Result.Failure("Access denied");
                }

                // Validate status transition
                if (!payment.CanTransitionTo(request.NewStatus))
                {
                    _logger.LogWarning("Invalid status transition for payment {PaymentId} from {CurrentStatus} to {NewStatus}",
                        request.PaymentId, payment.Status, request.NewStatus);
                    return Result.Failure($"Invalid status transition from {payment.Status} to {request.NewStatus}");
                }

                // Verify ExternalPaymentId matches
                if (!string.IsNullOrEmpty(payment.ExternalPaymentId) &&
                    !string.IsNullOrEmpty(request.ExternalPaymentId) &&
                    payment.ExternalPaymentId != request.ExternalPaymentId)
                {
                    _logger.LogWarning("ExternalPaymentId mismatch for payment {PaymentId}. Expected: {Expected}, Received: {Received}",
                        request.PaymentId, payment.ExternalPaymentId, request.ExternalPaymentId);
                    return Result.Failure("ExternalPaymentId mismatch");
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
                        return Result.Failure($"Unknown event type: {request.EventType}");
                }

                // Update external payment ID if provided
                if (!string.IsNullOrEmpty(request.ExternalPaymentId))
                {
                    payment.ExternalPaymentId = request.ExternalPaymentId;
                }

                // Update metadata
                foreach (var metadata in request.Metadata)
                {
                    // Add metadata to payment (you might need to extend the Payment entity for this)
                    _logger.LogDebug("Adding metadata {Key}: {Value} to payment {PaymentId}", metadata.Key, metadata.Value, request.PaymentId);
                }

                // Save changes
                await _unitOfWork.Payments.UpdateAsync(payment, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Successfully updated payment {PaymentId} from webhook", request.PaymentId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment {PaymentId} from webhook", request.PaymentId);
                return Result.Failure($"Error updating payment: {ex.Message}");
            }
        }
    }
}
