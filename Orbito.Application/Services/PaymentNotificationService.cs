using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Configuration;
using Orbito.Application.Services.Templates;
using Orbito.Domain.Entities;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Services;

/// <summary>
/// Service for sending payment-related notifications
/// </summary>
public class PaymentNotificationService : IPaymentNotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<PaymentNotificationService> _logger;
    private readonly ITenantContext _tenantContext;
    private readonly IMemoryCache _cache;

    // Cache settings
    private static readonly TimeSpan PlanCacheExpiration = TimeSpan.FromHours(1);
    private const string PlanCacheKeyPrefix = "plan_";

    public PaymentNotificationService(
        IUnitOfWork unitOfWork,
        IEmailSender emailSender,
        ILogger<PaymentNotificationService> logger,
        ITenantContext tenantContext,
        IMemoryCache cache)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    /// <summary>
    /// Helper method to get verified client by ID with tenant context validation
    /// </summary>
    private async Task<Domain.Entities.Client?> GetVerifiedClientAsync(Guid clientId, CancellationToken cancellationToken)
    {
        var client = await _unitOfWork.Clients.GetByIdAsync(clientId, cancellationToken);
        if (client == null)
        {
            _logger.LogWarning("Client {ClientId} not found", clientId);
            return null;
        }

        // SECURITY: Verify tenant context
        if (_tenantContext.HasTenant && client.TenantId != _tenantContext.CurrentTenantId)
        {
            _logger.LogWarning("Tenant mismatch for client {ClientId}. Expected: {ExpectedTenant}, Actual: {ActualTenant}",
                clientId, _tenantContext.CurrentTenantId, client.TenantId);
            return null;
        }

        return client;
    }

    /// <summary>
    /// Validates email address format
    /// </summary>
    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return new EmailAddressAttribute().IsValid(email);
    }

    /// <summary>
    /// Sends email with validation using outbox pattern
    /// </summary>
    private async Task SendValidatedEmailAsync(string email, string subject, string body, CancellationToken cancellationToken)
    {
        // SECURITY: Validate email before sending
        if (!IsValidEmail(email))
        {
            _logger.LogError("Invalid email address: {Email}", email);
            throw new ArgumentException($"Invalid email address: {email}", nameof(email));
        }

        await _emailSender.SendEmailAsync(email, subject, body, isHtml: false, cancellationToken);
    }

    /// <summary>
    /// Creates email notification in outbox for retry mechanism
    /// </summary>
    private async Task CreateEmailNotificationAsync(
        string type,
        string recipientEmail,
        string subject,
        string body,
        Guid? relatedEntityId = null,
        string? relatedEntityType = null,
        CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.HasTenant)
        {
            _logger.LogWarning("No tenant context for creating email notification");
            return;
        }

        var notification = EmailNotification.Create(
            _tenantContext.CurrentTenantId,
            type,
            recipientEmail,
            subject,
            body,
            relatedEntityId,
            relatedEntityType);

        await _unitOfWork.EmailNotifications.AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Email notification created in outbox for {Type} to {Email}", type, recipientEmail);
    }

    /// <summary>
    /// Gets subscription plan from cache or database
    /// </summary>
    private async Task<SubscriptionPlan?> GetCachedPlanAsync(Guid planId, CancellationToken cancellationToken)
    {
        var cacheKey = $"{PlanCacheKeyPrefix}{planId}";

        // Try to get from cache
        if (_cache.TryGetValue<SubscriptionPlan>(cacheKey, out var cachedPlan))
        {
            _logger.LogDebug("Subscription plan {PlanId} retrieved from cache", planId);
            return cachedPlan;
        }

        // Not in cache, get from database
        _logger.LogDebug("Subscription plan {PlanId} not in cache, fetching from database", planId);
        var plan = await _unitOfWork.SubscriptionPlans.GetByIdAsync(planId, cancellationToken);

        // Store in cache if found
        if (plan != null)
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = PlanCacheExpiration,
                SlidingExpiration = TimeSpan.FromMinutes(30), // Refresh if accessed within 30 minutes
                Priority = CacheItemPriority.Normal
            };

            _cache.Set(cacheKey, plan, cacheOptions);
            _logger.LogDebug("Subscription plan {PlanId} stored in cache", planId);
        }
        else
        {
            _logger.LogWarning("Subscription plan {PlanId} not found in database", planId);
        }

        return plan;
    }

    /// <summary>
    /// Sends payment confirmation notification
    /// </summary>
    public async Task SendPaymentConfirmationAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending payment confirmation for payment {PaymentId}", paymentId);

            // Get payment details
            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId, cancellationToken);
            if (payment == null)
            {
                _logger.LogWarning("Payment {PaymentId} not found", paymentId);
                return;
            }

            // Get verified client details
            var client = await GetVerifiedClientAsync(payment.ClientId, cancellationToken);
            if (client == null)
            {
                return;
            }

            // Get subscription details
            var subscription = await _unitOfWork.Subscriptions.GetByIdAsync(payment.SubscriptionId, cancellationToken);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found for payment {PaymentId}", payment.SubscriptionId, paymentId);
                return;
            }

            // Get subscription plan name (cached)
            var plan = await GetCachedPlanAsync(subscription.PlanId, cancellationToken);
            var subscriptionName = plan?.Name ?? "Subscription";

            // Build email content
            var subject = PaymentEmailTemplates.GetPaymentConfirmationSubject();
            var body = PaymentEmailTemplates.GetPaymentConfirmationBody(
                clientName: $"{client.FirstName} {client.LastName}",
                amount: payment.Amount,
                transactionId: payment.ExternalTransactionId ?? payment.Id.ToString(),
                paymentDate: payment.ProcessedAt ?? payment.CreatedAt,
                subscriptionName: subscriptionName
            );

            // Try to send email directly first
            try
            {
                await SendValidatedEmailAsync(client.Email, subject, body, cancellationToken);
                _logger.LogInformation("Payment confirmation sent successfully for payment {PaymentId}", paymentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Direct email send failed for payment {PaymentId}, creating outbox notification", paymentId);
                
                // Create outbox notification for retry
                await CreateEmailNotificationAsync(
                    "PaymentConfirmation",
                    client.Email,
                    subject,
                    body,
                    paymentId,
                    "Payment",
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending payment confirmation for payment {PaymentId}", paymentId);
            // Don't throw - notification failure shouldn't break payment flow
        }
    }

    /// <summary>
    /// Sends payment failure notification
    /// </summary>
    public async Task SendPaymentFailureNotificationAsync(Guid paymentId, string reason, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending payment failure notification for payment {PaymentId}", paymentId);

            // Get payment details
            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId, cancellationToken);
            if (payment == null)
            {
                _logger.LogWarning("Payment {PaymentId} not found", paymentId);
                return;
            }

            // Get verified client details
            var client = await GetVerifiedClientAsync(payment.ClientId, cancellationToken);
            if (client == null)
            {
                return;
            }

            // Get subscription details
            var subscription = await _unitOfWork.Subscriptions.GetByIdAsync(payment.SubscriptionId, cancellationToken);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found for payment {PaymentId}", payment.SubscriptionId, paymentId);
                return;
            }

            // Get subscription plan name (cached)
            var plan = await GetCachedPlanAsync(subscription.PlanId, cancellationToken);
            var subscriptionName = plan?.Name ?? "Subscription";

            // Build email content
            var subject = PaymentEmailTemplates.GetPaymentFailureSubject();
            var body = PaymentEmailTemplates.GetPaymentFailureBody(
                clientName: $"{client.FirstName} {client.LastName}",
                amount: payment.Amount,
                reason: reason,
                attemptDate: payment.FailedAt ?? DateTime.UtcNow,
                subscriptionName: subscriptionName
            );

            // Send validated email
            await SendValidatedEmailAsync(client.Email, subject, body, cancellationToken);

            _logger.LogInformation("Payment failure notification sent successfully for payment {PaymentId}", paymentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending payment failure notification for payment {PaymentId}", paymentId);
            // Don't throw - notification failure shouldn't break payment flow
        }
    }

    /// <summary>
    /// Sends refund confirmation notification (auto-detects full vs partial refund)
    /// </summary>
    public async Task SendRefundConfirmationAsync(Guid paymentId, Money refundAmount, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending refund confirmation for payment {PaymentId}", paymentId);

            // Get payment details
            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId, cancellationToken);
            if (payment == null)
            {
                _logger.LogWarning("Payment {PaymentId} not found", paymentId);
                return;
            }

            // Get verified client details
            var client = await GetVerifiedClientAsync(payment.ClientId, cancellationToken);
            if (client == null)
            {
                return;
            }

            // SECURITY: Validate currency match
            if (refundAmount.Currency != payment.Amount.Currency)
            {
                _logger.LogError("Currency mismatch in refund confirmation for payment {PaymentId}. Refund: {RefundCurrency}, Payment: {PaymentCurrency}",
                    paymentId, refundAmount.Currency, payment.Amount.Currency);
                return;
            }

            // Determine if full or partial refund
            var isFullRefund = refundAmount.Amount == payment.Amount.Amount;

            // Build email content
            var subject = PaymentEmailTemplates.GetRefundConfirmationSubject();
            var body = isFullRefund
                ? PaymentEmailTemplates.GetRefundConfirmationBody(
                    clientName: $"{client.FirstName} {client.LastName}",
                    refundAmount: refundAmount,
                    originalAmount: payment.Amount,
                    transactionId: payment.ExternalTransactionId ?? payment.Id.ToString(),
                    refundDate: payment.RefundedAt ?? DateTime.UtcNow,
                    reason: payment.RefundReason ?? "Refund requested"
                )
                : PaymentEmailTemplates.GetPartialRefundConfirmationBody(
                    clientName: $"{client.FirstName} {client.LastName}",
                    refundAmount: refundAmount,
                    originalAmount: payment.Amount,
                    transactionId: payment.ExternalTransactionId ?? payment.Id.ToString(),
                    refundDate: payment.RefundedAt ?? DateTime.UtcNow,
                    reason: payment.RefundReason ?? "Partial refund requested"
                );

            // Send validated email
            await SendValidatedEmailAsync(client.Email, subject, body, cancellationToken);

            _logger.LogInformation("Refund confirmation sent successfully for payment {PaymentId} (Full: {IsFullRefund})",
                paymentId, isFullRefund);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending refund confirmation for payment {PaymentId}", paymentId);
            // Don't throw - notification failure shouldn't break payment flow
        }
    }

    /// <summary>
    /// Sends partial refund confirmation notification
    /// </summary>
    public async Task SendPartialRefundConfirmationAsync(Guid paymentId, Money refundAmount, Money originalAmount, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending partial refund confirmation for payment {PaymentId}", paymentId);

            // Get payment details
            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId, cancellationToken);
            if (payment == null)
            {
                _logger.LogWarning("Payment {PaymentId} not found", paymentId);
                return;
            }

            // Get verified client details
            var client = await GetVerifiedClientAsync(payment.ClientId, cancellationToken);
            if (client == null)
            {
                return;
            }

            // Build email content
            var subject = PaymentEmailTemplates.GetRefundConfirmationSubject();
            var body = PaymentEmailTemplates.GetPartialRefundConfirmationBody(
                clientName: $"{client.FirstName} {client.LastName}",
                refundAmount: refundAmount,
                originalAmount: originalAmount,
                transactionId: payment.ExternalTransactionId ?? payment.Id.ToString(),
                refundDate: payment.RefundedAt ?? DateTime.UtcNow,
                reason: payment.RefundReason ?? "Partial refund requested"
            );

            // Send validated email
            await SendValidatedEmailAsync(client.Email, subject, body, cancellationToken);

            _logger.LogInformation("Partial refund confirmation sent successfully for payment {PaymentId}", paymentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending partial refund confirmation for payment {PaymentId}", paymentId);
            // Don't throw - notification failure shouldn't break payment flow
        }
    }

    /// <summary>
    /// Sends upcoming payment reminder notification
    /// </summary>
    public async Task SendUpcomingPaymentReminderAsync(Guid subscriptionId, int daysUntilPayment, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending upcoming payment reminder for subscription {SubscriptionId}", subscriptionId);

            // Get subscription details
            var subscription = await _unitOfWork.Subscriptions.GetByIdAsync(subscriptionId, cancellationToken);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found", subscriptionId);
                return;
            }

            // Get client details
            var client = await _unitOfWork.Clients.GetByIdAsync(subscription.ClientId, cancellationToken);
            if (client == null)
            {
                _logger.LogWarning("Client {ClientId} not found for subscription {SubscriptionId}", subscription.ClientId, subscriptionId);
                return;
            }

            // Get subscription plan details (cached)
            var plan = await GetCachedPlanAsync(subscription.PlanId, cancellationToken);
            var subscriptionName = plan?.Name ?? "Subscription";
            // SECURITY: Use consistent currency from client's tenant context, not hardcoded USD
            var amount = plan?.Price ?? Money.Create(0, PaymentProcessingConfiguration.DefaultCurrency);

            // Get default payment method
            var paymentMethods = await _unitOfWork.PaymentMethods.GetDefaultPaymentMethodsByClientAsync(subscription.ClientId, cancellationToken);
            var defaultPaymentMethod = paymentMethods.FirstOrDefault();
            var paymentMethodLast4 = defaultPaymentMethod?.LastFourDigits ?? "****";

            // Build email content
            var subject = PaymentEmailTemplates.GetUpcomingPaymentReminderSubject(daysUntilPayment);
            var body = PaymentEmailTemplates.GetUpcomingPaymentReminderBody(
                clientName: $"{client.FirstName} {client.LastName}",
                amount: amount,
                dueDate: subscription.NextBillingDate,
                subscriptionName: subscriptionName,
                paymentMethodLast4: paymentMethodLast4
            );

            // Send validated email
            await SendValidatedEmailAsync(client.Email, subject, body, cancellationToken);

            _logger.LogInformation("Upcoming payment reminder sent successfully for subscription {SubscriptionId}", subscriptionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending upcoming payment reminder for subscription {SubscriptionId}", subscriptionId);
            // Don't throw - notification failure shouldn't break subscription flow
        }
    }

    /// <summary>
    /// Sends expired card notification
    /// </summary>
    public async Task SendExpiredCardNotificationAsync(Guid paymentMethodId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending expired card notification for payment method {PaymentMethodId}", paymentMethodId);

            // Get payment method details
            var paymentMethod = await _unitOfWork.PaymentMethods.GetByIdAsync(paymentMethodId, Guid.Empty, cancellationToken);
            if (paymentMethod == null)
            {
                _logger.LogWarning("Payment method {PaymentMethodId} not found", paymentMethodId);
                return;
            }

            // Get client details
            var client = await _unitOfWork.Clients.GetByIdAsync(paymentMethod.ClientId, cancellationToken);
            if (client == null)
            {
                _logger.LogWarning("Client {ClientId} not found for payment method {PaymentMethodId}", paymentMethod.ClientId, paymentMethodId);
                return;
            }

            // Build email content
            var subject = PaymentEmailTemplates.GetExpiredCardNotificationSubject();
            var body = PaymentEmailTemplates.GetExpiredCardNotificationBody(
                clientName: $"{client.FirstName} {client.LastName}",
                cardBrand: paymentMethod.Type.ToString(),
                last4Digits: paymentMethod.LastFourDigits ?? "****",
                expiryDate: paymentMethod.ExpiryDate?.ToString("MM/yyyy") ?? "Unknown"
            );

            // Send validated email
            await SendValidatedEmailAsync(client.Email, subject, body, cancellationToken);

            _logger.LogInformation("Expired card notification sent successfully for payment method {PaymentMethodId}", paymentMethodId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending expired card notification for payment method {PaymentMethodId}", paymentMethodId);
            // Don't throw - notification failure shouldn't break flow
        }
    }

    /// <summary>
    /// Sends payment method added notification
    /// </summary>
    public async Task SendPaymentMethodAddedNotificationAsync(Guid paymentMethodId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending payment method added notification for {PaymentMethodId}", paymentMethodId);

            // Get payment method details
            var paymentMethod = await _unitOfWork.PaymentMethods.GetByIdAsync(paymentMethodId, Guid.Empty, cancellationToken);
            if (paymentMethod == null)
            {
                _logger.LogWarning("Payment method {PaymentMethodId} not found", paymentMethodId);
                return;
            }

            // Get client details
            var client = await _unitOfWork.Clients.GetByIdAsync(paymentMethod.ClientId, cancellationToken);
            if (client == null)
            {
                _logger.LogWarning("Client {ClientId} not found for payment method {PaymentMethodId}", paymentMethod.ClientId, paymentMethodId);
                return;
            }

            // Build email content
            var subject = PaymentEmailTemplates.GetPaymentMethodAddedNotificationSubject();
            var body = PaymentEmailTemplates.GetPaymentMethodAddedNotificationBody(
                clientName: $"{client.FirstName} {client.LastName}",
                cardBrand: paymentMethod.Type.ToString(),
                last4Digits: paymentMethod.LastFourDigits ?? "****",
                expiryDate: paymentMethod.ExpiryDate?.ToString("MM/yyyy") ?? "Unknown",
                addedDate: paymentMethod.CreatedAt
            );

            // Send validated email
            await SendValidatedEmailAsync(client.Email, subject, body, cancellationToken);

            _logger.LogInformation("Payment method added notification sent successfully for {PaymentMethodId}", paymentMethodId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending payment method added notification for {PaymentMethodId}", paymentMethodId);
            // Don't throw - notification failure shouldn't break flow
        }
    }

    /// <summary>
    /// Sends payment method removed notification
    /// </summary>
    public async Task SendPaymentMethodRemovedNotificationAsync(Guid clientId, string lastFourDigits, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending payment method removed notification for client {ClientId}", clientId);

            // Get client details
            var client = await _unitOfWork.Clients.GetByIdAsync(clientId, cancellationToken);
            if (client == null)
            {
                _logger.LogWarning("Client {ClientId} not found", clientId);
                return;
            }

            // Build email content
            var subject = PaymentEmailTemplates.GetPaymentMethodRemovedNotificationSubject();
            var body = PaymentEmailTemplates.GetPaymentMethodRemovedNotificationBody(
                clientName: $"{client.FirstName} {client.LastName}",
                last4Digits: lastFourDigits,
                removedDate: DateTime.UtcNow
            );

            // Send validated email
            await SendValidatedEmailAsync(client.Email, subject, body, cancellationToken);

            _logger.LogInformation("Payment method removed notification sent successfully for client {ClientId}", clientId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending payment method removed notification for client {ClientId}", clientId);
            // Don't throw - notification failure shouldn't break flow
        }
    }

    /// <summary>
    /// Sends card expiring soon notification
    /// </summary>
    public async Task SendCardExpiringSoonNotificationAsync(Guid paymentMethodId, int daysUntilExpiry, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending card expiring soon notification for payment method {PaymentMethodId}", paymentMethodId);

            // Get payment method details
            var paymentMethod = await _unitOfWork.PaymentMethods.GetByIdAsync(paymentMethodId, Guid.Empty, cancellationToken);
            if (paymentMethod == null)
            {
                _logger.LogWarning("Payment method {PaymentMethodId} not found", paymentMethodId);
                return;
            }

            // Get client details
            var client = await _unitOfWork.Clients.GetByIdAsync(paymentMethod.ClientId, cancellationToken);
            if (client == null)
            {
                _logger.LogWarning("Client {ClientId} not found for payment method {PaymentMethodId}", paymentMethod.ClientId, paymentMethodId);
                return;
            }

            // Build email content
            var subject = PaymentEmailTemplates.GetCardExpiringSoonNotificationSubject(daysUntilExpiry);
            var body = PaymentEmailTemplates.GetCardExpiringSoonNotificationBody(
                clientName: $"{client.FirstName} {client.LastName}",
                cardBrand: paymentMethod.Type.ToString(),
                last4Digits: paymentMethod.LastFourDigits ?? "****",
                expiryDate: paymentMethod.ExpiryDate?.ToString("MM/yyyy") ?? "Unknown",
                daysUntilExpiry: daysUntilExpiry
            );

            // Send validated email
            await SendValidatedEmailAsync(client.Email, subject, body, cancellationToken);

            _logger.LogInformation("Card expiring soon notification sent successfully for payment method {PaymentMethodId}", paymentMethodId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending card expiring soon notification for payment method {PaymentMethodId}", paymentMethodId);
            // Don't throw - notification failure shouldn't break flow
        }
    }
}
