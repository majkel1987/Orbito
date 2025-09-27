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
        public ICollection<PaymentMethod> PaymentMethods { get; set; } = [];

        public string Email => User?.Email ?? DirectEmail ?? "";
        public string FirstName => User?.FirstName ?? DirectFirstName ?? "";
        public string LastName => User?.LastName ?? DirectLastName ?? "";
        public string FullName => $"{FirstName} {LastName}".Trim();

        // Computed Properties
        public Subscription? ActiveSubscription
        {
            get
            {
                var activeSubscriptions = Subscriptions.Where(s => s.Status == SubscriptionStatus.Active).ToList();
                
                if (activeSubscriptions.Count == 0)
                    return null;
                
                if (activeSubscriptions.Count > 1)
                {
                    // Logika biznesowa: jeśli klient ma wiele aktywnych subskrypcji,
                    // zwróć najnowszą (najprawdopodobniej najważniejszą)
                    return activeSubscriptions.OrderByDescending(s => s.StartDate).First();
                }
                
                return activeSubscriptions.First();
            }
        }

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

        // Business Operations
        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void UpdateContactInfo(string? companyName, string? phone)
        {
            if (!string.IsNullOrWhiteSpace(companyName))
            {
                CompanyName = companyName;
            }

            if (!string.IsNullOrWhiteSpace(phone))
            {
                Phone = phone;
            }
        }

        public void UpdateDirectInfo(string? email, string? firstName, string? lastName)
        {
            if (UserId != null)
                throw new InvalidOperationException("Cannot update direct info for clients with Identity account. Use Identity user management instead.");

            if (!string.IsNullOrWhiteSpace(email))
            {
                DirectEmail = email;
            }

            if (!string.IsNullOrWhiteSpace(firstName))
            {
                DirectFirstName = firstName;
            }

            if (!string.IsNullOrWhiteSpace(lastName))
            {
                DirectLastName = lastName;
            }
        }

        public bool CanBeDeleted()
        {
            // Klient może być usunięty tylko jeśli nie ma aktywnych subskrypcji
            return !Subscriptions.Any(s => s.Status == SubscriptionStatus.Active);
        }
    }
}
