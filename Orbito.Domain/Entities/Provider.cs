using Orbito.Domain.Common;
using Orbito.Domain.Errors;
using Orbito.Domain.Identity;
using Orbito.Domain.Interfaces;
using Orbito.Domain.ValueObjects;

namespace Orbito.Domain.Entities
{
    /// <summary>
    /// Provider entity - represents a tenant in the system
    /// Provider does NOT implement IMustHaveTenant because it IS a tenant itself
    /// Provider.TenantId is self-referencing (TenantId == Provider.Id)
    /// </summary>
    public class Provider
    {
        public Guid Id { get; private set; }
        public TenantId TenantId { get; private set; }

        public ApplicationUser? User { get; private set; }
        public Guid? UserId { get; private set; }

        // Business Profile
        public string BusinessName { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public string? Avatar { get; private set; }

        // Platform Settings
        public string SubdomainSlug { get; private set; } = string.Empty;
        public string? CustomDomain { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }

        // Stripe Integration (for platform billing)
        public string? StripeCustomerId { get; private set; }

        // Business Metrics (cached values)
        public Money MonthlyRevenue { get; private set; }
        public int ActiveClientsCount { get; private set; }

        // Navigation Properties
        private readonly List<SubscriptionPlan> _plans = [];
        public IReadOnlyCollection<SubscriptionPlan> Plans => _plans.AsReadOnly();
        private readonly List<Client> _clients = [];
        public IReadOnlyCollection<Client> Clients => _clients.AsReadOnly();
        private readonly List<Subscription> _subscriptions = [];
        public IReadOnlyCollection<Subscription> Subscriptions => _subscriptions.AsReadOnly();

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

        public Result UpdateMonthlyRevenue(Money revenue)
        {
            if (revenue.Currency != MonthlyRevenue.Currency)
                return Result.Failure(DomainErrors.Payment.CurrencyMismatch);

            MonthlyRevenue = revenue;
            return Result.Success();
        }

        public Result UpdateBusinessProfile(string businessName, string? description = null, string? avatar = null)
        {
            if (string.IsNullOrWhiteSpace(businessName))
                return Result.Failure(DomainErrors.Provider.BusinessNameCannotBeEmpty);

            BusinessName = businessName;
            Description = description;
            Avatar = avatar;
            return Result.Success();
        }

        public Result UpdatePlatformSettings(string subdomainSlug, string? customDomain = null)
        {
            if (string.IsNullOrWhiteSpace(subdomainSlug))
                return Result.Failure(DomainErrors.Provider.InvalidSubdomain);

            SubdomainSlug = subdomainSlug;
            CustomDomain = customDomain;
            return Result.Success();
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public Result UpdateActiveClientsCount(int count)
        {
            if (count < 0)
                return Result.Failure(DomainErrors.Provider.ActiveClientsCountCannotBeNegative);

            ActiveClientsCount = count;
            return Result.Success();
        }

        public bool CanBeDeleted()
        {
            // Provider can be deleted only if there are no active clients or subscriptions
            return ActiveClientsCount == 0 && !Subscriptions.Any(s => s.Status == Domain.Enums.SubscriptionStatus.Active);
        }

        public void SetDescription(string? description)
        {
            Description = description;
        }

        public void SetAvatar(string? avatar)
        {
            Avatar = avatar;
        }

        public void SetCustomDomain(string? customDomain)
        {
            CustomDomain = customDomain;
        }

        public Result SetStripeCustomerId(string stripeCustomerId)
        {
            if (string.IsNullOrWhiteSpace(stripeCustomerId))
                return Result.Failure(DomainErrors.Provider.StripeCustomerIdCannotBeEmpty);

            StripeCustomerId = stripeCustomerId;
            return Result.Success();
        }
    }
}
