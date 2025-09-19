using Orbito.Domain.Identity;
using Orbito.Domain.Interfaces;
using Orbito.Domain.ValueObjects;

namespace Orbito.Domain.Entities
{
    public class Provider : IMustHaveTenant
    {
        public Guid Id { get; set; }
        public TenantId TenantId { get; set; }

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
            var id = Guid.NewGuid();
            return new Provider
            {
                Id = id,
                TenantId = TenantId.Create(id),
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

        public void UpdateBusinessProfile(string businessName, string? description = null, string? avatar = null)
        {
            if (string.IsNullOrWhiteSpace(businessName))
                throw new ArgumentException("Business name cannot be empty", nameof(businessName));

            BusinessName = businessName;
            Description = description;
            Avatar = avatar;
        }

        public void UpdatePlatformSettings(string subdomainSlug, string? customDomain = null)
        {
            if (string.IsNullOrWhiteSpace(subdomainSlug))
                throw new ArgumentException("Subdomain slug cannot be empty", nameof(subdomainSlug));

            SubdomainSlug = subdomainSlug;
            CustomDomain = customDomain;
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void UpdateActiveClientsCount(int count)
        {
            if (count < 0)
                throw new ArgumentException("Active clients count cannot be negative", nameof(count));

            ActiveClientsCount = count;
        }

        public bool CanBeDeleted()
        {
            // Provider can be deleted only if there are no active clients or subscriptions
            return ActiveClientsCount == 0 && !Subscriptions.Any(s => s.Status == Domain.Enums.SubscriptionStatus.Active);
        }
    }
}
