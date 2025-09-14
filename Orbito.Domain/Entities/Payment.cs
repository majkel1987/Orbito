using Orbito.Domain.Enums;
using Orbito.Domain.Interfaces;
using Orbito.Domain.ValueObjects;

namespace Orbito.Domain.Entities
{
    public class Payment : IMustHaveTenant
    {
        public Guid Id { get; set; }
        public TenantId TenantId { get; set; }  // Which provider this belongs to

        // Payment Details
        public Guid SubscriptionId { get; set; }
        public Guid ClientId { get; set; }
        public Money Amount { get; set; }
        public PaymentStatus Status { get; set; }

        // External Payment Data
        public string? ExternalPaymentId { get; set; }  // Stripe Payment Intent ID
        public string? PaymentMethodId { get; set; }    // Stripe Payment Method

        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime? FailedAt { get; set; }

        // Failure Information
        public string? FailureReason { get; set; }

        // Navigation Properties
        public Subscription Subscription { get; set; }
        public Client Client { get; set; }

        private Payment() { } // EF Core

        public static Payment Create(
            TenantId tenantId,
            Guid subscriptionId,
            Guid clientId,
            Money amount,
            string? externalPaymentId = null)
        {
            return new Payment
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                SubscriptionId = subscriptionId,
                ClientId = clientId,
                Amount = amount,
                Status = PaymentStatus.Pending,
                ExternalPaymentId = externalPaymentId,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void MarkAsSucceeded()
        {
            Status = PaymentStatus.Succeeded;
            ProcessedAt = DateTime.UtcNow;
        }

        public void MarkAsFailed(string reason)
        {
            Status = PaymentStatus.Failed;
            FailedAt = DateTime.UtcNow;
            FailureReason = reason;
        }
    }
}

