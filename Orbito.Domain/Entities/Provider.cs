using Orbito.Domain.Identity;
using Orbito.Domain.Interfaces;
using Orbito.Domain.ValueObjects;

namespace Orbito.Domain.Entities
{
    public class Provider : IMustHaveTenant
    {
        public Guid Id { get; set; }
        public TenantId TenantId => TenantId.Create(Id);

        public ApplicationUser? User { get; set; }
        public Guid? UserId { get; set; }

        // Business Profile
        public string BusinessName { get; set; }
        public string? Description { get; set; }
        public string? Avatar { get; set; }

        // Platform Settings
        public string SubdomainSlug { get; set; }
        public string? CustomDomain { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // Business Metrics (cached values)
        public Money MonthlyRevenue { get; set; }
        public int ActiveClientsCount { get; set; }

        // Navigation Properties
        public ICollection<SubscriptionPlan> Plans { get; set; } = [];
        public ICollection<Client> Clients { get; set; } = [];
        public ICollection<Subscription> Subscriptions { get; set; } = [];

        private Provider() { }
        
        public static Provider Create(Guid userId,
            string businessName,
            string subdomainSlug)
        {
            return new Provider
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                BusinessName = businessName,
                SubdomainSlug = subdomainSlug,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                MonthlyRevenue = Money.Zero("PLN"),
                ActiveClientsCount = 0
            };
        }

        public void UpdateMonthlyRevenue(Money revenue)
        {
            if (revenue.Currency != MonthlyRevenue.Currency)
                throw new InvalidOperationException("Currency mismatch");

            MonthlyRevenue = revenue;
        }
    }
}
