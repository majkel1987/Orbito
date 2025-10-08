using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Common.Interfaces
{
    /// <summary>
    /// Service interface for security limits and rate limiting
    /// </summary>
    public interface ISecurityLimitService
    {
        /// <summary>
        /// Maximum number of payment methods per client
        /// </summary>
        int MaxPaymentMethodsPerClient { get; }

        /// <summary>
        /// Maximum page size for queries
        /// </summary>
        int MaxPageSize { get; }

        /// <summary>
        /// Maximum number of payments allowed in bulk retry operation
        /// </summary>
        int MaxBulkRetryLimit { get; }

        /// <summary>
        /// Rate limit window for payment attempts
        /// </summary>
        TimeSpan PaymentAttemptWindow { get; }

        /// <summary>
        /// Maximum payment attempts within window
        /// </summary>
        int MaxPaymentAttemptsPerWindow { get; }

        /// <summary>
        /// Tolerance window for marking retry schedules as overdue (in minutes)
        /// </summary>
        int RetryOverdueToleranceMinutes { get; }

        /// <summary>
        /// Validates if page size is within limits
        /// </summary>
        /// <param name="pageSize">Requested page size</param>
        /// <returns>Validated page size (clamped to max)</returns>
        int ValidatePageSize(int pageSize);

        /// <summary>
        /// Checks if client can add more payment methods
        /// </summary>
        /// <param name="clientId">Client ID</param>
        /// <param name="currentCount">Current payment methods count</param>
        /// <returns>True if can add, false otherwise</returns>
        bool CanAddPaymentMethod(Guid clientId, int currentCount);

        /// <summary>
        /// Checks if client has exceeded payment attempt rate limit
        /// </summary>
        /// <param name="clientId">Client ID</param>
        /// <param name="attemptCount">Recent attempt count</param>
        /// <param name="lastAttempt">Last attempt timestamp</param>
        /// <returns>Delay before next attempt or null if allowed</returns>
        TimeSpan? CheckPaymentRateLimit(Guid clientId, int attemptCount, DateTime lastAttempt);

        /// <summary>
        /// Validates refund amount against original payment
        /// </summary>
        /// <param name="refundAmount">Requested refund amount</param>
        /// <param name="originalAmount">Original payment amount</param>
        /// <param name="totalRefunded">Total already refunded</param>
        /// <returns>True if refund is valid, false otherwise</returns>
        bool ValidateRefundAmount(Money refundAmount, Money originalAmount, Money totalRefunded);

        /// <summary>
        /// Sanitizes sensitive data from webhook logs
        /// </summary>
        /// <param name="webhookData">Raw webhook data</param>
        /// <returns>Sanitized webhook data for logging</returns>
        string SanitizeWebhookDataForLogging(string webhookData);
    }
}