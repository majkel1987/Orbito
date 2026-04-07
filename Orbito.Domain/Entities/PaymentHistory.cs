using Orbito.Domain.Enums;
using Orbito.Domain.Interfaces;
using Orbito.Domain.ValueObjects;

namespace Orbito.Domain.Entities
{
    public class PaymentHistory : IMustHaveTenant
    {
        public Guid Id { get; private set; }
        public TenantId TenantId { get; private set; }
        public Guid PaymentId { get; private set; }

        // History Details
        public string Action { get; private set; } = string.Empty; // Created, Processed, Failed, Refunded, etc.
        public PaymentStatus Status { get; private set; }
        public DateTime OccurredAt { get; private set; }
        public string? Details { get; private set; }
        public string? ErrorMessage { get; private set; }

        // Navigation Properties
        public Payment Payment { get; private set; } = null!;

        private PaymentHistory() { } // EF Core

        public static PaymentHistory Create(
            TenantId tenantId,
            Guid paymentId,
            string action,
            PaymentStatus status,
            string? details = null,
            string? errorMessage = null)
        {
            return new PaymentHistory
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                PaymentId = paymentId,
                Action = action,
                Status = status,
                OccurredAt = DateTime.UtcNow,
                Details = details,
                ErrorMessage = errorMessage
            };
        }

        // Business Operations
        public void UpdateDetails(string? details)
        {
            Details = details;
        }

        public void UpdateErrorMessage(string? errorMessage)
        {
            ErrorMessage = errorMessage;
        }

        public bool IsError()
        {
            return !string.IsNullOrEmpty(ErrorMessage);
        }

        public bool IsSuccess()
        {
            return Status == PaymentStatus.Completed && string.IsNullOrEmpty(ErrorMessage);
        }
    }
}
