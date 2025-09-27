using Orbito.Domain.Enums;
using Orbito.Domain.Interfaces;
using Orbito.Domain.ValueObjects;

namespace Orbito.Domain.Entities
{
    public class PaymentHistory : IMustHaveTenant
    {
        public Guid Id { get; set; }
        public TenantId TenantId { get; set; }
        public Guid PaymentId { get; set; }

        // History Details
        public string Action { get; set; } = string.Empty; // Created, Processed, Failed, Refunded, etc.
        public PaymentStatus Status { get; set; }
        public DateTime OccurredAt { get; set; }
        public string? Details { get; set; }
        public string? ErrorMessage { get; set; }

        // Navigation Properties
        public Payment Payment { get; set; } = default!;

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
