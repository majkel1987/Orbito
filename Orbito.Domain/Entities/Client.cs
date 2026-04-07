using Orbito.Domain.Common;
using Orbito.Domain.Enums;
using Orbito.Domain.Errors;
using Orbito.Domain.Identity;
using Orbito.Domain.Interfaces;
using Orbito.Domain.ValueObjects;

namespace Orbito.Domain.Entities
{
    public class Client : IMustHaveTenant
    {
        public Guid Id { get; private set; }
        public TenantId TenantId { get; private set; }

        public ApplicationUser? User { get; private set; }
        public Guid? UserId { get; private set; }

        // Client Details
        public string? CompanyName { get; private set; }
        public string? Phone { get; private set; }

        public string? DirectEmail { get; private set; }       // Dla klientów bez konta Identity
        public string? DirectFirstName { get; private set; }   // Dla klientów bez konta Identity
        public string? DirectLastName { get; private set; }    // Dla klientów bez konta Identity

        // Invitation Flow
        public ClientStatus Status { get; private set; } = ClientStatus.Inactive;
        public string? InvitationToken { get; private set; }
        public DateTime? InvitationTokenExpiresAt { get; private set; }
        public DateTime? ConfirmedAt { get; private set; }

        // Platform Data
        public DateTime CreatedAt { get; private set; }
        public bool IsActive { get; private set; }

        // Payment Gateway Integration
        public string? StripeCustomerId { get; private set; }

        // Navigation Properties
        public Provider Provider { get; private set; } = null!;
        private readonly List<Subscription> _subscriptions = [];
        public IReadOnlyCollection<Subscription> Subscriptions => _subscriptions.AsReadOnly();
        private readonly List<Payment> _payments = [];
        public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();
        private readonly List<PaymentMethod> _paymentMethods = [];
        public IReadOnlyCollection<PaymentMethod> PaymentMethods => _paymentMethods.AsReadOnly();

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

        public static Client CreateInvited(
            TenantId tenantId,
            string email,
            string firstName,
            string lastName,
            string invitationToken,
            DateTime invitationTokenExpiresAt,
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
                Status = ClientStatus.Inactive,
                IsActive = false,
                InvitationToken = invitationToken,
                InvitationTokenExpiresAt = invitationTokenExpiresAt,
                CreatedAt = DateTime.UtcNow
            };
        }

        public Result ConfirmEmail()
        {
            if (Status == ClientStatus.Active)
                return Result.Failure(DomainErrors.Client.AlreadyConfirmed);

            if (InvitationToken == null || InvitationTokenExpiresAt < DateTime.UtcNow)
                return Result.Failure(DomainErrors.Client.InvitationExpired);

            Status = ClientStatus.Active;
            IsActive = true;
            ConfirmedAt = DateTime.UtcNow;
            InvitationToken = null;
            InvitationTokenExpiresAt = null;

            return Result.Success();
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

        public Result UpdateDirectInfo(string? email, string? firstName, string? lastName)
        {
            if (UserId != null)
                return Result.Failure(DomainErrors.Client.CannotUpdateDirectInfoWithIdentity);

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

            return Result.Success();
        }

        public bool CanBeDeleted()
        {
            // Klient może być usunięty tylko jeśli nie ma aktywnych subskrypcji
            return !Subscriptions.Any(s => s.Status == SubscriptionStatus.Active);
        }

        public void SetUserId(Guid userId)
        {
            UserId = userId;
        }

        public Result SetStripeCustomerId(string stripeCustomerId)
        {
            if (string.IsNullOrWhiteSpace(stripeCustomerId))
                return Result.Failure(DomainErrors.Client.StripeCustomerIdCannotBeEmpty);

            StripeCustomerId = stripeCustomerId;
            return Result.Success();
        }

        public void SetPhone(string? phone)
        {
            Phone = phone;
        }

        public Result RegenerateInvitationToken(string token, DateTime expiresAt)
        {
            if (string.IsNullOrWhiteSpace(token))
                return Result.Failure(DomainErrors.Client.TokenCannotBeEmpty);

            InvitationToken = token;
            InvitationTokenExpiresAt = expiresAt;
            return Result.Success();
        }

        public Result SetProvider(Provider provider)
        {
            if (provider == null)
                return Result.Failure(DomainErrors.Client.ProviderCannotBeNull);

            Provider = provider;
            return Result.Success();
        }
    }
}
