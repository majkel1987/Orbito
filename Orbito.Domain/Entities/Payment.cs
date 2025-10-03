using Orbito.Domain.Enums;
using Orbito.Domain.Interfaces;
using Orbito.Domain.ValueObjects;
using Orbito.Domain.Events;
using Orbito.Domain.Common;

namespace Orbito.Domain.Entities
{
    public class Payment : IMustHaveTenant
    {
        public Guid Id { get; set; }
        public TenantId TenantId { get; set; }

        // Payment Details
        public Guid SubscriptionId { get; set; }
        public Guid ClientId { get; set; }
        public Money Amount { get; set; }
        public PaymentStatus Status { get; set; }

        // External Payment Data
        public string? ExternalTransactionId { get; set; }
        public string? PaymentMethod { get; set; }
        public string? ExternalPaymentId { get; set; }
        public string? PaymentMethodId { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime? FailedAt { get; set; }
        public DateTime? RefundedAt { get; set; }

        // Failure and Refund Information
        public string? FailureReason { get; set; }
        public string? RefundReason { get; set; }

        // Navigation Properties
        public Subscription Subscription { get; set; }
        public Client Client { get; set; }

        // Domain Events
        private readonly List<IDomainEvent> _domainEvents = [];
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        public void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }

        private Payment() { }

        public static Payment Create(
            TenantId tenantId,
            Guid subscriptionId,
            Guid clientId,
            Money amount,
            string? externalTransactionId = null,
            string? paymentMethod = null,
            string? externalPaymentId = null)
        {
            if (tenantId == null)
                throw new ArgumentNullException(nameof(tenantId));

            if (subscriptionId == Guid.Empty)
                throw new ArgumentException("Subscription ID cannot be empty", nameof(subscriptionId));

            if (clientId == Guid.Empty)
                throw new ArgumentException("Client ID cannot be empty", nameof(clientId));

            if (amount == null)
                throw new ArgumentNullException(nameof(amount));

            if (amount.Amount <= 0)
                throw new ArgumentException("Payment amount must be greater than zero", nameof(amount));

            return new Payment
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                SubscriptionId = subscriptionId,
                ClientId = clientId,
                Amount = amount,
                Status = PaymentStatus.Pending,
                ExternalTransactionId = externalTransactionId,
                PaymentMethod = paymentMethod,
                ExternalPaymentId = externalPaymentId,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void MarkAsProcessing()
        {
            Status = PaymentStatus.Processing;
        }

        public void MarkAsCompleted()
        {
            Status = PaymentStatus.Completed;
            ProcessedAt = DateTime.UtcNow;

            AddDomainEvent(new PaymentCompletedEvent(
                Id,
                SubscriptionId,
                ClientId,
                Amount,
                ExternalTransactionId,
                ProcessedAt.Value));
        }

        public void MarkAsFailed(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Failure reason cannot be empty", nameof(reason));

            Status = PaymentStatus.Failed;
            FailedAt = DateTime.UtcNow;
            FailureReason = reason;

            AddDomainEvent(new PaymentFailedEvent(
                Id,
                SubscriptionId,
                ClientId,
                Amount,
                reason,
                ExternalTransactionId,
                FailedAt.Value));
        }

        public void MarkAsCancelled()
        {
            Status = PaymentStatus.Cancelled;
            FailedAt = DateTime.UtcNow;
            FailureReason = "Payment cancelled";
        }

        public void MarkAsCanceled()
        {
            MarkAsCancelled();
        }

        public void MarkAsRefunded(string refundReason)
        {
            if (string.IsNullOrWhiteSpace(refundReason))
                throw new ArgumentException("Refund reason cannot be empty", nameof(refundReason));

            if (Status != PaymentStatus.Completed)
                throw new InvalidOperationException("Only completed payments can be refunded");

            Status = PaymentStatus.Refunded;
            RefundedAt = DateTime.UtcNow;
            RefundReason = refundReason;

            AddDomainEvent(new PaymentRefundedEvent(
                Id,
                SubscriptionId,
                ClientId,
                Amount,
                refundReason,
                ExternalTransactionId,
                RefundedAt.Value));
        }

        public void MarkAsPartiallyRefunded(string refundReason, Money refundedAmount)
        {
            if (string.IsNullOrWhiteSpace(refundReason))
                throw new ArgumentException("Refund reason cannot be empty", nameof(refundReason));

            Status = PaymentStatus.PartiallyRefunded;
            RefundedAt = DateTime.UtcNow;
            RefundReason = refundReason;
        }

        public bool IsCompleted => Status == PaymentStatus.Completed;

        public void RetryPayment()
        {
            if (Status == PaymentStatus.Failed)
            {
                Status = PaymentStatus.Pending;
                FailedAt = null;
                FailureReason = null;
            }
        }

        public bool CanBeRetried(int retryDaysLimit = 30)
        {
            return Status == PaymentStatus.Failed &&
                   FailedAt.HasValue &&
                   FailedAt.Value.AddDays(retryDaysLimit) > DateTime.UtcNow;
        }

        public bool CanBeRefunded()
        {
            return Status == PaymentStatus.Completed;
        }

        /// <summary>
        /// Validates if the payment can transition to the new status
        /// </summary>
        /// <param name="newStatus">Target status</param>
        /// <returns>True if transition is valid</returns>
        public bool CanTransitionTo(PaymentStatus newStatus)
        {
            // Same status is always allowed (idempotent)
            if (Status == newStatus)
                return true;

            return Status switch
            {
                PaymentStatus.Pending => newStatus is PaymentStatus.Processing
                    or PaymentStatus.Completed
                    or PaymentStatus.Failed
                    or PaymentStatus.Cancelled,

                PaymentStatus.Processing => newStatus is PaymentStatus.Completed
                    or PaymentStatus.Failed,

                PaymentStatus.Completed => newStatus is PaymentStatus.Refunded
                    or PaymentStatus.PartiallyRefunded,

                PaymentStatus.Failed => newStatus is PaymentStatus.Pending, // Retry

                PaymentStatus.Cancelled => false, // Cannot transition from cancelled

                PaymentStatus.Refunded => false, // Cannot transition from refunded

                PaymentStatus.PartiallyRefunded => newStatus is PaymentStatus.Refunded, // Can complete the refund

                _ => false
            };
        }
    }
}