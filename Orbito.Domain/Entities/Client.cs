using Orbito.Domain.Enums;
using Orbito.Domain.Identity;
using Orbito.Domain.Interfaces;
using Orbito.Domain.ValueObjects;

namespace Orbito.Domain.Entities
{
    public class Client : IMustHaveTenant
    {
        public Guid Id { get; set; }
        public TenantId TenantId { get; set; }

        public ApplicationUser? User { get; set; }
        public Guid? UserId { get; set; }

        // Client Details
        public string? CompanyName { get; set; }
        public string? Phone { get; set; }

        public string? DirectEmail { get; set; }       // Dla klientów bez konta Identity
        public string? DirectFirstName { get; set; }   // Dla klientów bez konta Identity
        public string? DirectLastName { get; set; }    // Dla klientów bez konta Identity

        // Platform Data
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }

        // Navigation Properties
        public Provider Provider { get; set; }
        public ICollection<Subscription> Subscriptions { get; set; } = [];
        public ICollection<Payment> Payments { get; set; } = [];

        public string Email => User?.Email ?? DirectEmail ?? "";
        public string FirstName => User?.FirstName ?? DirectFirstName ?? "";
        public string LastName => User?.LastName ?? DirectLastName ?? "";
        public string FullName => $"{FirstName} {LastName}".Trim();

        // Computed Properties
        public Subscription? ActiveSubscription =>
            Subscriptions.FirstOrDefault(s => s.Status == SubscriptionStatus.Active);

        private Client() { } // EF Core

        public static Client CreateWithUser(
        TenantId tenantId,
        Guid userId,
        string? companyName = null)
        {
            return new Client
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UserId = userId,
                CompanyName = companyName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static Client CreateDirect(
        TenantId tenantId,
        string email,
        string firstName,
        string lastName,
        string? companyName = null)
        {
            return new Client
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                DirectEmail = email,
                DirectFirstName = firstName,
                DirectLastName = lastName,
                CompanyName = companyName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
