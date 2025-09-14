using Orbito.Domain.Enums;
using Orbito.Domain.Interfaces;
using Orbito.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orbito.Domain.Entities
{
    public class Client : IMustHaveTenant
    {
        public Guid Id { get; set; }
        public TenantId TenantId => TenantId.Create(Id);

        // Client Details
        public Email Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? CompanyName { get; set; }
        public string? Phone { get; set; }

        // Platform Data
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; }

        // Navigation Properties
        public Provider Provider { get; set; }
        public ICollection<Subscription> Subscriptions { get; set; } = [];
        public ICollection<Payment> Payments { get; set; } = [];

        // Computed Properties
        public Subscription? ActiveSubscription =>
            Subscriptions.FirstOrDefault(s => s.Status == SubscriptionStatus.Active);

        private Client() { } // EF Core

        public static Client Create(
            TenantId tenantId,
            Email email,
            string firstName,
            string lastName,
            string? companyName = null)
        {
            return new Client
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                CompanyName = companyName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void UpdateLastLogin()
        {
            LastLoginAt = DateTime.UtcNow;
        }
    }
}
