using Orbito.Application.Common.Interfaces;
using Orbito.Domain.ValueObjects;
using System.Text.RegularExpressions;

namespace Orbito.Application.Common.Services
{
    /// <summary>
    /// Implementation of security limits and rate limiting service
    /// </summary>
    public class SecurityLimitService : ISecurityLimitService
    {
        public int MaxPaymentMethodsPerClient => 10;
        public int MaxPageSize => 100;
        public int MaxBulkRetryLimit => 50;
        public TimeSpan PaymentAttemptWindow => TimeSpan.FromMinutes(15);
        public int MaxPaymentAttemptsPerWindow => 5;
        public int RetryOverdueToleranceMinutes => 5;

        public int ValidatePageSize(int pageSize)
        {
            if (pageSize <= 0)
                return 10; // Default page size

            if (pageSize > MaxPageSize)
                return MaxPageSize;

            return pageSize;
        }

        public bool CanAddPaymentMethod(Guid clientId, int currentCount)
        {
            return currentCount < MaxPaymentMethodsPerClient;
        }

        public TimeSpan? CheckPaymentRateLimit(Guid clientId, int attemptCount, DateTime lastAttempt)
        {
            if (attemptCount < MaxPaymentAttemptsPerWindow)
                return null;

            var timeSinceLastAttempt = DateTime.UtcNow - lastAttempt;
            var remainingWaitTime = PaymentAttemptWindow - timeSinceLastAttempt;

            if (remainingWaitTime > TimeSpan.Zero)
                return remainingWaitTime;

            return null;
        }

        public bool ValidateRefundAmount(Money refundAmount, Money originalAmount, Money totalRefunded)
        {
            if (refundAmount.Currency != originalAmount.Currency)
                return false;

            if (totalRefunded.Currency != originalAmount.Currency)
                return false;

            var remainingAmount = originalAmount.Subtract(totalRefunded);

            return refundAmount.Amount <= remainingAmount.Amount && refundAmount.Amount > 0;
        }

        public string SanitizeWebhookDataForLogging(string webhookData)
        {
            if (string.IsNullOrEmpty(webhookData))
                return webhookData;

            // Remove sensitive card information
            var sanitized = Regex.Replace(webhookData, @"""card"":\s*\{[^}]*\}", "\"card\":{\"[REDACTED]\"}");

            // Remove CVV/CVC
            sanitized = Regex.Replace(sanitized, @"""cvc"":\s*""[^""]*""", "\"cvc\":\"***\"");

            // Remove card numbers
            sanitized = Regex.Replace(sanitized, @"""number"":\s*""[^""]*""", "\"number\":\"****\"");

            // Remove bank account numbers
            sanitized = Regex.Replace(sanitized, @"""account_number"":\s*""[^""]*""", "\"account_number\":\"****\"");

            // Remove routing numbers
            sanitized = Regex.Replace(sanitized, @"""routing_number"":\s*""[^""]*""", "\"routing_number\":\"****\"");

            return sanitized;
        }
    }
}
