using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models;
using Orbito.Application.Services;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Orbito.Infrastructure.PaymentGateways.Stripe.Models;
using Orbito.Infrastructure.Persistance;
using System.Text.Json;

namespace Orbito.Infrastructure.PaymentGateways.Stripe.EventHandlers
{
    /// <summary>
    /// Handles different types of Stripe webhook events
    /// </summary>
    public class StripeEventHandler
    {
        private readonly ILogger<StripeEventHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentProcessingService _paymentProcessingService;
        private readonly IEmailService _emailService;

        public StripeEventHandler(
            ILogger<StripeEventHandler> logger,
            IUnitOfWork unitOfWork,
            IPaymentProcessingService paymentProcessingService,
            IEmailService emailService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _paymentProcessingService = paymentProcessingService ?? throw new ArgumentNullException(nameof(paymentProcessingService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        /// <summary>
        /// Handles Stripe webhook events based on event type
        /// </summary>
        public async Task<Result> HandleEventAsync(string eventType, StripeWebhookData webhookData, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Handling Stripe event: {EventType} with ID: {EventId}, Created: {Created}",
                    eventType,
                    webhookData.Id,
                    webhookData.CreatedDateTime);

                return eventType switch
                {
                    "payment_intent.succeeded" => await HandlePaymentIntentSucceededAsync(webhookData, cancellationToken),
                    "payment_intent.payment_failed" => await HandlePaymentIntentFailedAsync(webhookData, cancellationToken),
                    "payment_intent.canceled" => await HandlePaymentIntentCanceledAsync(webhookData, cancellationToken),
                    "charge.refunded" => await HandleChargeRefundedAsync(webhookData, cancellationToken),
                    "charge.succeeded" => await HandleChargeSucceededAsync(webhookData, cancellationToken),
                    "charge.failed" => await HandleChargeFailedAsync(webhookData, cancellationToken),
                    "customer.subscription.created" => await HandleCustomerSubscriptionCreatedAsync(webhookData, cancellationToken),
                    "customer.subscription.updated" => await HandleCustomerSubscriptionUpdatedAsync(webhookData, cancellationToken),
                    "customer.subscription.deleted" => await HandleCustomerSubscriptionDeletedAsync(webhookData, cancellationToken),
                    "invoice.payment_succeeded" => await HandleInvoicePaymentSucceededAsync(webhookData, cancellationToken),
                    "invoice.payment_failed" => await HandleInvoicePaymentFailedAsync(webhookData, cancellationToken),
                    _ => await HandleUnknownEventAsync(eventType, webhookData, cancellationToken)
                };
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON parsing error for Stripe event {EventType}: {EventId}", eventType, webhookData.Id);
                return Result.Failure($"Invalid event data format: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error handling Stripe event {EventType}: {EventId}", eventType, webhookData.Id);
                return Result.Failure($"Error handling event: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles successful payment intent
        /// </summary>
        private async Task<Result> HandlePaymentIntentSucceededAsync(StripeWebhookData webhookData, CancellationToken cancellationToken)
        {
            try
            {
                var paymentIntent = DeserializeEventData<StripePaymentIntent>(webhookData);
                if (paymentIntent == null)
                {
                    return Result.Failure("Failed to parse payment intent data");
                }

                _logger.LogDebug("Processing payment intent succeeded: {PaymentIntentId}, Amount: {Amount} {Currency}",
                    paymentIntent.Id, paymentIntent.Amount, paymentIntent.Currency);

                // Find payment by external ID
                var payment = await _unitOfWork.Payments.GetByExternalPaymentIdUnsafeAsync(paymentIntent.Id, cancellationToken);
                if (payment == null)
                {
                    _logger.LogWarning("Payment not found for payment intent: {PaymentIntentId}", paymentIntent.Id);
                    return Result.Failure($"Payment not found for external ID: {paymentIntent.Id}");
                }

                // Verify payment is in correct state
                if (payment.IsCompleted)
                {
                    _logger.LogInformation("Payment {PaymentId} already completed, skipping", payment.Id);
                    return Result.Success();
                }

                // Update payment status
                payment.MarkAsCompleted();
                await _unitOfWork.Payments.UpdateAsync(payment, cancellationToken);

                // Activate subscription if it was pending payment or suspended
                var subscription = payment.Subscription;
                if (subscription != null)
                {
                    if (subscription.Status == SubscriptionStatus.Pending ||
                        subscription.Status == SubscriptionStatus.Suspended)
                    {
                        subscription.Activate();
                        await _unitOfWork.Subscriptions.UpdateAsync(subscription, cancellationToken);
                        _logger.LogInformation("Subscription {SubscriptionId} activated after successful payment", subscription.Id);
                    }
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Handle payment success (e.g., fulfill order, send confirmation)
                await _paymentProcessingService.HandlePaymentSuccessAsync(payment.Id, cancellationToken);

                // Send payment confirmation email to client
                if (payment.Client != null && !string.IsNullOrEmpty(payment.Client.Email))
                {
                    var subscriptionName = subscription?.Plan?.Name ?? "Subskrypcja";
                    var clientName = payment.Client.FullName ?? payment.Client.Email;

                    await _emailService.SendPaymentConfirmationAsync(
                        toEmail: payment.Client.Email,
                        clientName: clientName,
                        subscriptionName: subscriptionName,
                        amount: payment.Amount?.Amount ?? 0,
                        currency: payment.Amount?.CurrencyCode ?? "PLN",
                        paymentId: payment.ExternalPaymentId ?? payment.Id.ToString(),
                        paymentDate: DateTime.UtcNow,
                        cancellationToken: cancellationToken);

                    _logger.LogInformation("Payment confirmation email sent to {Email} for payment {PaymentId}",
                        payment.Client.Email, payment.Id);
                }

                _logger.LogInformation("Successfully processed payment intent succeeded for payment {PaymentId}", payment.Id);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling payment intent succeeded");
                return Result.Failure($"Error handling payment intent succeeded: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles failed payment intent
        /// </summary>
        private async Task<Result> HandlePaymentIntentFailedAsync(StripeWebhookData webhookData, CancellationToken cancellationToken)
        {
            try
            {
                var paymentIntent = DeserializeEventData<StripePaymentIntent>(webhookData);
                if (paymentIntent == null)
                {
                    return Result.Failure("Failed to parse payment intent data");
                }

                _logger.LogDebug("Processing payment intent failed: {PaymentIntentId}, Status: {Status}",
                    paymentIntent.Id, paymentIntent.Status);

                // Find payment by external ID
                var payment = await _unitOfWork.Payments.GetByExternalPaymentIdUnsafeAsync(paymentIntent.Id, cancellationToken);
                if (payment == null)
                {
                    _logger.LogWarning("Payment not found for payment intent: {PaymentIntentId}", paymentIntent.Id);
                    return Result.Failure($"Payment not found for external ID: {paymentIntent.Id}");
                }

                // Build detailed failure reason
                var failureReason = BuildPaymentFailureReason(paymentIntent);

                // Update payment status
                payment.MarkAsFailed(failureReason);
                await _unitOfWork.Payments.UpdateAsync(payment, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Handle payment failure (e.g., notify user, retry logic)
                await _paymentProcessingService.HandlePaymentFailureAsync(payment.Id, failureReason, cancellationToken);

                // Send payment failed email to client
                if (payment.Client != null && !string.IsNullOrEmpty(payment.Client.Email))
                {
                    var subscriptionName = payment.Subscription?.Plan?.Name ?? "Subskrypcja";
                    var clientName = payment.Client.FullName ?? payment.Client.Email;

                    await _emailService.SendPaymentFailedAsync(
                        toEmail: payment.Client.Email,
                        clientName: clientName,
                        subscriptionName: subscriptionName,
                        amount: payment.Amount?.Amount ?? 0,
                        currency: payment.Amount?.CurrencyCode ?? "PLN",
                        failureReason: failureReason,
                        cancellationToken: cancellationToken);

                    _logger.LogInformation("Payment failed email sent to {Email} for payment {PaymentId}",
                        payment.Client.Email, payment.Id);
                }

                _logger.LogInformation("Successfully processed payment intent failed for payment {PaymentId}: {Reason}",
                    payment.Id, failureReason);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling payment intent failed");
                return Result.Failure($"Error handling payment intent failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles canceled payment intent
        /// </summary>
        private async Task<Result> HandlePaymentIntentCanceledAsync(StripeWebhookData webhookData, CancellationToken cancellationToken)
        {
            try
            {
                var paymentIntent = DeserializeEventData<StripePaymentIntent>(webhookData);
                if (paymentIntent == null)
                {
                    return Result.Failure("Failed to parse payment intent data");
                }

                var payment = await _unitOfWork.Payments.GetByExternalPaymentIdUnsafeAsync(paymentIntent.Id, cancellationToken);
                if (payment == null)
                {
                    _logger.LogWarning("Payment not found for payment intent: {PaymentIntentId}", paymentIntent.Id);
                    return Result.Failure($"Payment not found for external ID: {paymentIntent.Id}");
                }

                payment.MarkAsCanceled();
                await _unitOfWork.Payments.UpdateAsync(payment, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Successfully processed payment intent canceled for payment {PaymentId}", payment.Id);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling payment intent canceled");
                return Result.Failure($"Error handling payment intent canceled: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles charge refunded event
        /// </summary>
        private async Task<Result> HandleChargeRefundedAsync(StripeWebhookData webhookData, CancellationToken cancellationToken)
        {
            try
            {
                var charge = DeserializeEventData<StripeCharge>(webhookData);
                if (charge == null)
                {
                    return Result.Failure("Failed to parse charge data");
                }

                // Validate that we have a payment intent ID
                if (string.IsNullOrEmpty(charge.PaymentIntent))
                {
                    _logger.LogWarning("Charge {ChargeId} has no payment intent associated", charge.Id);
                    return Result.Failure("No payment intent associated with charge");
                }

                _logger.LogDebug("Processing charge refunded: {ChargeId}, Amount Refunded: {AmountRefunded}/{TotalAmount}",
                    charge.Id, charge.AmountRefunded, charge.Amount);

                // Find payment by external ID
                var payment = await _unitOfWork.Payments.GetByExternalPaymentIdUnsafeAsync(charge.PaymentIntent, cancellationToken);
                if (payment == null)
                {
                    _logger.LogWarning("Payment not found for payment intent: {PaymentIntentId}", charge.PaymentIntent);
                    return Result.Failure($"Payment not found for external ID: {charge.PaymentIntent}");
                }

                // Handle full vs partial refund
                if (charge.IsFullRefund)
                {
                    var refundReason = $"Full refund processed via Stripe. Amount: {charge.AmountRefunded / 100m} {charge.Currency.ToUpperInvariant()}";
                    payment.MarkAsRefunded(refundReason);
                    _logger.LogInformation("Full refund processed for payment {PaymentId}", payment.Id);
                }
                else if (charge.IsPartialRefund)
                {
                    var refundedAmount = Money.Create(charge.AmountRefunded / 100m, charge.Currency.ToUpperInvariant());
                    var refundReason = $"Partial refund processed via Stripe. Amount: {refundedAmount.Amount} {refundedAmount.CurrencyCode} of {charge.Amount / 100m}";
                    payment.MarkAsPartiallyRefunded(refundReason, refundedAmount);
                    _logger.LogInformation("Partial refund processed for payment {PaymentId}: {AmountRefunded}/{TotalAmount}",
                        payment.Id, charge.AmountRefunded, charge.Amount);
                }
                else
                {
                    _logger.LogWarning("Unexpected refund state for charge {ChargeId}: Refunded={Refunded}, AmountRefunded={AmountRefunded}",
                        charge.Id, charge.Refunded, charge.AmountRefunded);
                }

                await _unitOfWork.Payments.UpdateAsync(payment, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Successfully processed charge refunded for payment {PaymentId}", payment.Id);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling charge refunded");
                return Result.Failure($"Error handling charge refunded: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles charge succeeded event
        /// </summary>
        private async Task<Result> HandleChargeSucceededAsync(StripeWebhookData webhookData, CancellationToken cancellationToken)
        {
            try
            {
                var charge = DeserializeEventData<StripeCharge>(webhookData);
                if (charge == null)
                {
                    return Result.Failure("Failed to parse charge data");
                }

                _logger.LogDebug("Processing charge succeeded: {ChargeId}, Amount: {Amount} {Currency}",
                    charge.Id, charge.Amount, charge.Currency);

                // This event is typically handled by payment_intent.succeeded
                // But we can use it for additional validation or logging
                _logger.LogInformation("Charge succeeded: {ChargeId} for payment intent: {PaymentIntent}",
                    charge.Id, charge.PaymentIntent);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling charge succeeded");
                return Result.Failure($"Error handling charge succeeded: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles charge failed event
        /// </summary>
        private async Task<Result> HandleChargeFailedAsync(StripeWebhookData webhookData, CancellationToken cancellationToken)
        {
            try
            {
                var charge = DeserializeEventData<StripeCharge>(webhookData);
                if (charge == null)
                {
                    return Result.Failure("Failed to parse charge data");
                }

                _logger.LogWarning("Charge failed: {ChargeId}, Status: {Status}", charge.Id, charge.Status);

                // This is typically handled by payment_intent.payment_failed
                // Log for monitoring purposes
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling charge failed");
                return Result.Failure($"Error handling charge failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles customer subscription created event
        /// </summary>
        private async Task<Result> HandleCustomerSubscriptionCreatedAsync(StripeWebhookData webhookData, CancellationToken cancellationToken)
        {
            try
            {
                var subscription = DeserializeEventData<StripeSubscription>(webhookData);
                if (subscription == null)
                {
                    return Result.Failure("Failed to parse subscription data");
                }

                _logger.LogDebug("Processing subscription created: {SubscriptionId}, Customer: {Customer}, Status: {Status}",
                    subscription.Id, subscription.Customer, subscription.Status);

                // Find subscription by external ID
                var internalSubscription = await _unitOfWork.Subscriptions.GetByExternalIdAsync(subscription.Id, cancellationToken);
                if (internalSubscription == null)
                {
                    _logger.LogWarning("Subscription not found for external ID: {ExternalId}", subscription.Id);
                    return Result.Failure($"Subscription not found for external ID: {subscription.Id}");
                }

                // Update subscription based on initial status
                if (subscription.Status == "active")
                {
                    internalSubscription.Activate();
                }

                await _unitOfWork.Subscriptions.UpdateAsync(internalSubscription, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Successfully processed subscription created for subscription {SubscriptionId}", internalSubscription.Id);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling subscription created");
                return Result.Failure($"Error handling subscription created: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles customer subscription updated event
        /// </summary>
        private async Task<Result> HandleCustomerSubscriptionUpdatedAsync(StripeWebhookData webhookData, CancellationToken cancellationToken)
        {
            try
            {
                var subscription = DeserializeEventData<StripeSubscription>(webhookData);
                if (subscription == null)
                {
                    return Result.Failure("Failed to parse subscription data");
                }

                _logger.LogDebug("Processing subscription updated: {SubscriptionId}, Status: {Status}, CancelAtPeriodEnd: {CancelAtPeriodEnd}",
                    subscription.Id, subscription.Status, subscription.CancelAtPeriodEnd);

                // Find subscription by external ID
                var internalSubscription = await _unitOfWork.Subscriptions.GetByExternalIdAsync(subscription.Id, cancellationToken);
                if (internalSubscription == null)
                {
                    _logger.LogWarning("Subscription not found for external ID: {ExternalId}", subscription.Id);
                    return Result.Failure($"Subscription not found for external ID: {subscription.Id}");
                }

                // Update subscription status based on Stripe status
                switch (subscription.Status)
                {
                    case "active":
                        internalSubscription.Activate();
                        _logger.LogDebug("Subscription {SubscriptionId} activated", internalSubscription.Id);
                        break;
                    case "canceled":
                        internalSubscription.Cancel();
                        _logger.LogDebug("Subscription {SubscriptionId} canceled", internalSubscription.Id);
                        break;
                    case "past_due":
                        internalSubscription.MarkAsPastDue();
                        _logger.LogDebug("Subscription {SubscriptionId} marked as past due", internalSubscription.Id);
                        break;
                    case "unpaid":
                        internalSubscription.MarkAsUnpaid();
                        _logger.LogDebug("Subscription {SubscriptionId} marked as unpaid", internalSubscription.Id);
                        break;
                    case "trialing":
                        internalSubscription.StartTrial(subscription.CurrentPeriodEndDateTime);
                        _logger.LogDebug("Subscription {SubscriptionId} in trial", internalSubscription.Id);
                        break;
                    case "incomplete":
                    case "incomplete_expired":
                        _logger.LogWarning("Subscription {SubscriptionId} in incomplete state: {Status}",
                            internalSubscription.Id, subscription.Status);
                        break;
                    default:
                        _logger.LogWarning("Unknown subscription status: {Status} for subscription {SubscriptionId}",
                            subscription.Status, subscription.Id);
                        break;
                }

                // Handle cancel_at_period_end flag
                if (subscription.CancelAtPeriodEnd && !internalSubscription.IsCanceled)
                {
                    internalSubscription.ScheduleCancellation(subscription.CurrentPeriodEndDateTime);
                    _logger.LogInformation("Subscription {SubscriptionId} scheduled for cancellation at {CancelDate}",
                        internalSubscription.Id, subscription.CurrentPeriodEndDateTime);
                }

                await _unitOfWork.Subscriptions.UpdateAsync(internalSubscription, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Successfully processed subscription updated for subscription {SubscriptionId}", internalSubscription.Id);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling subscription updated");
                return Result.Failure($"Error handling subscription updated: {ex.Message}");
            }
}

/// <summary>
/// Handles customer subscription deleted event
/// </summary>
private async Task<Result> HandleCustomerSubscriptionDeletedAsync(StripeWebhookData webhookData, CancellationToken cancellationToken)
{
    try
    {
        var subscription = DeserializeEventData<StripeSubscription>(webhookData);
        if (subscription == null)
        {
            return Result.Failure("Failed to parse subscription data");
        }

        _logger.LogDebug("Processing subscription deleted: {SubscriptionId}, Customer: {Customer}",
            subscription.Id, subscription.Customer);

        // Find subscription by external ID
        var internalSubscription = await _unitOfWork.Subscriptions.GetByExternalIdAsync(subscription.Id, cancellationToken);
        if (internalSubscription == null)
        {
            _logger.LogWarning("Subscription not found for external ID: {ExternalId}", subscription.Id);
            return Result.Failure($"Subscription not found for external ID: {subscription.Id}");
        }

        // Cancel the subscription
        internalSubscription.Cancel();
        internalSubscription.MarkAsDeleted(); // If you have this method

        await _unitOfWork.Subscriptions.UpdateAsync(internalSubscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully processed subscription deleted for subscription {SubscriptionId}", internalSubscription.Id);
        return Result.Success();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error handling subscription deleted");
        return Result.Failure($"Error handling subscription deleted: {ex.Message}");
    }
}

/// <summary>
/// Handles invoice payment succeeded event
/// </summary>
private async Task<Result> HandleInvoicePaymentSucceededAsync(StripeWebhookData webhookData, CancellationToken cancellationToken)
{
    try
    {
        var invoice = DeserializeEventData<StripeInvoice>(webhookData);
        if (invoice == null)
        {
            return Result.Failure("Failed to parse invoice data");
        }

        _logger.LogDebug("Processing invoice payment succeeded: {InvoiceId}, Amount Paid: {AmountPaid} {Currency}, Subscription: {Subscription}",
            invoice.Id, invoice.AmountPaid, invoice.Currency, invoice.Subscription);

        // Validate that we have a payment intent ID
        if (string.IsNullOrEmpty(invoice.PaymentIntent))
        {
            _logger.LogWarning("Invoice {InvoiceId} has no payment intent. Billing reason: {BillingReason}",
                invoice.Id, invoice.BillingReason);

            // Some invoices may not have payment intents (e.g., $0 invoices, manual payments)
            // Handle subscription renewal without payment intent
            if (!string.IsNullOrEmpty(invoice.Subscription))
            {
                return await HandleSubscriptionRenewalAsync(invoice, cancellationToken);
            }

            return Result.Success(); // Log but don't fail
        }

        // Find payment by external ID
        var payment = await _unitOfWork.Payments.GetByExternalPaymentIdUnsafeAsync(invoice.PaymentIntent, cancellationToken);
        if (payment == null)
        {
            _logger.LogWarning("Payment not found for payment intent: {PaymentIntentId}", invoice.PaymentIntent);
            return Result.Failure($"Payment not found for external ID: {invoice.PaymentIntent}");
        }

        // Verify payment is not already completed
        if (payment.IsCompleted)
        {
            _logger.LogInformation("Payment {PaymentId} already completed, skipping", payment.Id);
            return Result.Success();
        }

        // Update payment status
        payment.MarkAsCompleted();
        await _unitOfWork.Payments.UpdateAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully processed invoice payment succeeded for payment {PaymentId}", payment.Id);
        return Result.Success();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error handling invoice payment succeeded");
        return Result.Failure($"Error handling invoice payment succeeded: {ex.Message}");
    }
}

/// <summary>
/// Handles invoice payment failed event
/// </summary>
private async Task<Result> HandleInvoicePaymentFailedAsync(StripeWebhookData webhookData, CancellationToken cancellationToken)
{
    try
    {
        var invoice = DeserializeEventData<StripeInvoice>(webhookData);
        if (invoice == null)
        {
            return Result.Failure("Failed to parse invoice data");
        }

        _logger.LogDebug("Processing invoice payment failed: {InvoiceId}, Amount Due: {AmountDue} {Currency}, Attempt: {AttemptCount}",
            invoice.Id, invoice.AmountDue, invoice.Currency, invoice.AttemptCount);

        // Validate that we have a payment intent ID
        if (string.IsNullOrEmpty(invoice.PaymentIntent))
        {
            _logger.LogWarning("Invoice {InvoiceId} has no payment intent. Billing reason: {BillingReason}",
                invoice.Id, invoice.BillingReason);

            // Handle subscription payment failure without payment intent
            if (!string.IsNullOrEmpty(invoice.Subscription))
            {
                return await HandleSubscriptionPaymentFailureAsync(invoice, cancellationToken);
            }

            return Result.Success(); // Log but don't fail
        }

        // Find payment by external ID
        var payment = await _unitOfWork.Payments.GetByExternalPaymentIdUnsafeAsync(invoice.PaymentIntent, cancellationToken);
        if (payment == null)
        {
            _logger.LogWarning("Payment not found for payment intent: {PaymentIntentId}", invoice.PaymentIntent);
            return Result.Failure($"Payment not found for external ID: {invoice.PaymentIntent}");
        }

        // Build failure reason with attempt count
        var failureReason = $"Invoice payment failed (Attempt {invoice.AttemptCount ?? 1})";

        payment.MarkAsFailed(failureReason);
        await _unitOfWork.Payments.UpdateAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Handle payment failure
        await _paymentProcessingService.HandlePaymentFailureAsync(payment.Id, failureReason, cancellationToken);

        _logger.LogInformation("Successfully processed invoice payment failed for payment {PaymentId}", payment.Id);
        return Result.Success();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error handling invoice payment failed");
        return Result.Failure($"Error handling invoice payment failed: {ex.Message}");
    }
}

/// <summary>
/// Handles subscription renewal without payment intent
/// </summary>
private async Task<Result> HandleSubscriptionRenewalAsync(StripeInvoice invoice, CancellationToken cancellationToken)
{
    try
    {
        if (string.IsNullOrEmpty(invoice.Subscription))
        {
            return Result.Success();
        }

        var subscription = await _unitOfWork.Subscriptions.GetByExternalIdAsync(invoice.Subscription, cancellationToken);
        if (subscription != null)
        {
            // Calculate next billing date based on current billing period
            var nextBillingDate = subscription.BillingPeriod.GetNextBillingDate(DateTime.UtcNow);
            subscription.Renew(nextBillingDate);
            await _unitOfWork.Subscriptions.UpdateAsync(subscription, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Subscription {SubscriptionId} renewed via invoice {InvoiceId}",
                subscription.Id, invoice.Id);
        }

        return Result.Success();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error handling subscription renewal");
        return Result.Failure($"Error handling subscription renewal: {ex.Message}");
    }
}

/// <summary>
/// Handles subscription payment failure
/// </summary>
private async Task<Result> HandleSubscriptionPaymentFailureAsync(StripeInvoice invoice, CancellationToken cancellationToken)
{
    try
    {
        if (string.IsNullOrEmpty(invoice.Subscription))
        {
            return Result.Success();
        }

        var subscription = await _unitOfWork.Subscriptions.GetByExternalIdAsync(invoice.Subscription, cancellationToken);
        if (subscription != null)
        {
            subscription.MarkAsPastDue();
            await _unitOfWork.Subscriptions.UpdateAsync(subscription, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogWarning("Subscription {SubscriptionId} marked as past due due to invoice {InvoiceId} failure",
                subscription.Id, invoice.Id);
        }

        return Result.Success();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error handling subscription payment failure");
        return Result.Failure($"Error handling subscription payment failure: {ex.Message}");
    }
}

/// <summary>
/// Handles unknown event types
/// </summary>
private Task<Result> HandleUnknownEventAsync(string eventType, StripeWebhookData webhookData, CancellationToken cancellationToken)
{
    _logger.LogInformation("Received unknown Stripe event type: {EventType} with ID: {EventId}. This event will be logged but not processed.",
        eventType, webhookData.Id);

    // Don't fail on unknown events - Stripe may add new event types
    return Task.FromResult(Result.Success());
}

/// <summary>
/// Deserializes event data to specified type
/// </summary>
private T? DeserializeEventData<T>(StripeWebhookData webhookData) where T : class
{
    try
    {
        var json = webhookData.Data.Object.GetRawText();
        return JsonSerializer.Deserialize<T>(json);
    }
    catch (JsonException ex)
    {
        _logger.LogError(ex, "Failed to deserialize event data to type {Type}", typeof(T).Name);
        return null;
    }
}

/// <summary>
/// Builds detailed payment failure reason
/// </summary>
private string BuildPaymentFailureReason(StripePaymentIntent paymentIntent)
{
    if (paymentIntent.LastPaymentError == null)
    {
        return "Payment failed without specific error details";
    }

    var error = paymentIntent.LastPaymentError;
    var parts = new List<string>();

    if (!string.IsNullOrEmpty(error.Message))
    {
        parts.Add(error.Message);
    }

    if (!string.IsNullOrEmpty(error.Code))
    {
        parts.Add($"Code: {error.Code}");
    }

    if (!string.IsNullOrEmpty(error.DeclineCode))
    {
        parts.Add($"Decline Code: {error.DeclineCode}");
    }

    if (!string.IsNullOrEmpty(error.Type))
    {
        parts.Add($"Type: {error.Type}");
    }

    return parts.Any() ? string.Join(" | ", parts) : "Payment failed";
}
    }
}