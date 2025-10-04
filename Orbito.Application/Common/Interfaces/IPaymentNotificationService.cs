using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Common.Interfaces;

/// <summary>
/// Service interface for payment-related notifications
/// </summary>
public interface IPaymentNotificationService
{
    /// <summary>
    /// Sends payment confirmation notification
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendPaymentConfirmationAsync(Guid paymentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends payment failure notification
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <param name="reason">Failure reason</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendPaymentFailureNotificationAsync(Guid paymentId, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends refund confirmation notification (full or partial)
    /// </summary>
    /// <param name="paymentId">Original payment ID</param>
    /// <param name="refundAmount">Refund amount</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendRefundConfirmationAsync(Guid paymentId, Money refundAmount, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends partial refund confirmation notification
    /// </summary>
    /// <param name="paymentId">Original payment ID</param>
    /// <param name="refundAmount">Partial refund amount</param>
    /// <param name="originalAmount">Original payment amount</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendPartialRefundConfirmationAsync(Guid paymentId, Money refundAmount, Money originalAmount, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends upcoming payment reminder notification
    /// </summary>
    /// <param name="subscriptionId">Subscription ID</param>
    /// <param name="daysUntilPayment">Days until payment is due</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendUpcomingPaymentReminderAsync(Guid subscriptionId, int daysUntilPayment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends expired card notification
    /// </summary>
    /// <param name="paymentMethodId">Payment method ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendExpiredCardNotificationAsync(Guid paymentMethodId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends payment method added notification
    /// </summary>
    /// <param name="paymentMethodId">Payment method ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendPaymentMethodAddedNotificationAsync(Guid paymentMethodId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends payment method removed notification
    /// </summary>
    /// <param name="clientId">Client ID</param>
    /// <param name="lastFourDigits">Last four digits of removed card</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendPaymentMethodRemovedNotificationAsync(Guid clientId, string lastFourDigits, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends card expiring soon notification (30 days before expiry)
    /// </summary>
    /// <param name="paymentMethodId">Payment method ID</param>
    /// <param name="daysUntilExpiry">Days until card expires</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendCardExpiringSoonNotificationAsync(Guid paymentMethodId, int daysUntilExpiry, CancellationToken cancellationToken = default);
}