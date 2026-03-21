using Orbito.Domain.Common;

namespace Orbito.Domain.Errors;

/// <summary>
/// Contains all domain-level error definitions.
/// Organized by domain entity/aggregate.
/// </summary>
public static class DomainErrors
{
    /// <summary>
    /// General errors applicable across domains
    /// </summary>
    public static class General
    {
        public static Error UnexpectedError => Error.Create(
            "General.UnexpectedError",
            "An unexpected error occurred");

        public static Error ValidationFailed => Error.Create(
            "General.ValidationFailed",
            "Validation failed");

        public static Error NotFound => Error.Create(
            "General.NotFound",
            "The requested resource was not found");

        public static Error Conflict => Error.Create(
            "General.Conflict",
            "A conflict occurred");

        public static Error Unauthorized => Error.Create(
            "General.Unauthorized",
            "Unauthorized access");
    }

    /// <summary>
    /// Tenant-related errors
    /// </summary>
    public static class Tenant
    {
        public static Error NotFound => Error.Create(
            "Tenant.NotFound",
            "Tenant was not found");

        public static Error SubdomainAlreadyExists => Error.Create(
            "Tenant.SubdomainAlreadyExists",
            "Subdomain is already taken");

        public static Error NoTenantContext => Error.Create(
            "Tenant.NoTenantContext",
            "Tenant context is not available");

        public static Error InvalidTenantId => Error.Create(
            "Tenant.InvalidTenantId",
            "Invalid tenant identifier");

        public static Error CrossTenantAccess => Error.Create(
            "Tenant.CrossTenantAccess",
            "Cross-tenant access is not allowed");
    }

    /// <summary>
    /// Provider-related errors
    /// </summary>
    public static class Provider
    {
        public static Error NotFound => Error.Create(
            "Provider.NotFound",
            "Provider was not found");

        public static Error SubdomainAlreadyExists => Error.Create(
            "Provider.SubdomainAlreadyExists",
            "Subdomain is already taken");

        public static Error InvalidSubdomain => Error.Create(
            "Provider.InvalidSubdomain",
            "Invalid subdomain format");

        public static Error UserAlreadyHasProvider => Error.Create(
            "Provider.UserAlreadyHasProvider",
            "User already has a provider assigned");

        public static Error CannotDeleteWithActiveClients => Error.Create(
            "Provider.CannotDeleteWithActiveClients",
            "Cannot delete provider with active clients");

        public static Error Inactive => Error.Create(
            "Provider.Inactive",
            "Provider is not active");
    }

    /// <summary>
    /// Client-related errors
    /// </summary>
    public static class Client
    {
        public static Error NotFound => Error.Create(
            "Client.NotFound",
            "Client was not found");

        public static Error EmailAlreadyExists => Error.Create(
            "Client.EmailAlreadyExists",
            "A client with this email already exists");

        public static Error UserAlreadyExists => Error.Create(
            "Client.UserAlreadyExists",
            "A client with this user already exists");

        public static Error InvalidEmail => Error.Create(
            "Client.InvalidEmail",
            "Invalid email address format");

        public static Error Inactive => Error.Create(
            "Client.Inactive",
            "Client is not active");

        public static Error AlreadyActive => Error.Create(
            "Client.AlreadyActive",
            "Client is already active");

        public static Error AlreadyInactive => Error.Create(
            "Client.AlreadyInactive",
            "Client is already inactive");

        public static Error CannotDeleteWithActiveSubscriptions => Error.Create(
            "Client.CannotDeleteWithActiveSubscriptions",
            "Cannot delete client with active subscriptions");

        public static Error AlreadyConfirmed => Error.Create(
            "Client.AlreadyConfirmed",
            "Client email has already been confirmed");

        public static Error InvitationExpired => Error.Create(
            "Client.InvitationExpired",
            "Client invitation has expired");

        public static Error InvalidToken => Error.Create(
            "Client.InvalidToken",
            "Invalid invitation token");
    }

    /// <summary>
    /// Subscription-related errors
    /// </summary>
    public static class Subscription
    {
        public static Error NotFound => Error.Create(
            "Subscription.NotFound",
            "Subscription was not found");

        public static Error CannotBeCancelled => Error.Create(
            "Subscription.CannotBeCancelled",
            "Subscription cannot be cancelled in current state");

        public static Error AlreadyActive => Error.Create(
            "Subscription.AlreadyActive",
            "Subscription is already active");

        public static Error AlreadyCancelled => Error.Create(
            "Subscription.AlreadyCancelled",
            "Subscription is already cancelled");

        public static Error AlreadySuspended => Error.Create(
            "Subscription.AlreadySuspended",
            "Subscription is already suspended");

        public static Error AlreadyExpired => Error.Create(
            "Subscription.AlreadyExpired",
            "Subscription has already expired");

        public static Error NotActive => Error.Create(
            "Subscription.NotActive",
            "Subscription is not active");

        public static Error InvalidStatus => Error.Create(
            "Subscription.InvalidStatus",
            "Invalid subscription status");

        public static Error CannotDowngrade => Error.Create(
            "Subscription.CannotDowngrade",
            "Cannot downgrade to this plan");

        public static Error CannotUpgrade => Error.Create(
            "Subscription.CannotUpgrade",
            "Cannot upgrade to this plan");

        public static Error CannotSuspend => Error.Create(
            "Subscription.CannotSuspend",
            "Subscription cannot be suspended in current state");

        public static Error CannotResume => Error.Create(
            "Subscription.CannotResume",
            "Subscription cannot be resumed in current state");

        public static Error CannotRenew => Error.Create(
            "Subscription.CannotRenew",
            "Subscription cannot be renewed in current state");

        public static Error PlanNotFound => Error.Create(
            "Subscription.PlanNotFound",
            "Subscription plan was not found");

        public static Error InvalidDateRange => Error.Create(
            "Subscription.InvalidDateRange",
            "Invalid subscription date range");
    }

    /// <summary>
    /// SubscriptionPlan-related errors
    /// </summary>
    public static class SubscriptionPlan
    {
        public static Error NotFound => Error.Create(
            "SubscriptionPlan.NotFound",
            "Subscription plan was not found");

        public static Error NameAlreadyExists => Error.Create(
            "SubscriptionPlan.NameAlreadyExists",
            "A plan with this name already exists");

        public static Error InvalidPrice => Error.Create(
            "SubscriptionPlan.InvalidPrice",
            "Invalid plan price");

        public static Error InvalidBillingPeriod => Error.Create(
            "SubscriptionPlan.InvalidBillingPeriod",
            "Invalid billing period");

        public static Error CannotDeleteWithActiveSubscriptions => Error.Create(
            "SubscriptionPlan.CannotDeleteWithActiveSubscriptions",
            "Cannot delete plan with active subscriptions");

        public static Error Inactive => Error.Create(
            "SubscriptionPlan.Inactive",
            "Subscription plan is not active");
    }

    /// <summary>
    /// Payment-related errors
    /// </summary>
    public static class Payment
    {
        public static Error NotFound => Error.Create(
            "Payment.NotFound",
            "Payment was not found");

        public static Error ProcessingFailed => Error.Create(
            "Payment.ProcessingFailed",
            "Payment processing failed");

        public static Error InvalidAmount => Error.Create(
            "Payment.InvalidAmount",
            "Invalid payment amount");

        public static Error InvalidCurrency => Error.Create(
            "Payment.InvalidCurrency",
            "Invalid currency code");

        public static Error DuplicateIdempotencyKey => Error.Create(
            "Payment.DuplicateIdempotencyKey",
            "Payment with this idempotency key already exists");

        public static Error DuplicateExternalTransactionId => Error.Create(
            "Payment.DuplicateExternalTransactionId",
            "Payment with this external transaction ID already exists");

        public static Error InvalidPaymentMethod => Error.Create(
            "Payment.InvalidPaymentMethod",
            "Invalid payment method");

        public static Error AlreadyProcessed => Error.Create(
            "Payment.AlreadyProcessed",
            "Payment has already been processed");

        public static Error AlreadyRefunded => Error.Create(
            "Payment.AlreadyRefunded",
            "Payment has already been refunded");

        public static Error CannotRefund => Error.Create(
            "Payment.CannotRefund",
            "Payment cannot be refunded");

        public static Error AmountMismatch => Error.Create(
            "Payment.AmountMismatch",
            "Payment amount does not match subscription amount");

        public static Error CurrencyMismatch => Error.Create(
            "Payment.CurrencyMismatch",
            "Payment currency does not match subscription currency");

        public static Error SubscriptionNotActive => Error.Create(
            "Payment.SubscriptionNotActive",
            "Cannot process payment for inactive subscription");

        public static Error ExternalPaymentIdRequired => Error.Create(
            "Payment.ExternalPaymentIdRequired",
            "External payment ID is required for this payment method");

        public static Error RateLimitExceeded => Error.Create(
            "Payment.RateLimitExceeded",
            "Payment rate limit exceeded");

        public static Error InvalidStatus => Error.Create(
            "Payment.InvalidStatus",
            "Invalid payment status");

        public static Error InvalidStatusTransition => Error.Create(
            "Payment.InvalidStatusTransition",
            "Invalid status transition for payment");

        public static Error FailureReasonRequired => Error.Create(
            "Payment.FailureReasonRequired",
            "Failure reason is required when marking payment as failed");

        public static Error RefundReasonRequired => Error.Create(
            "Payment.RefundReasonRequired",
            "Refund reason is required when refunding payment");

        public static Error UnsupportedStatus => Error.Create(
            "Payment.UnsupportedStatus",
            "Unsupported payment status");

        public static Error CustomerCreationFailed(string reason) => Error.Create(
            "Payment.CustomerCreationFailed",
            $"Failed to create payment customer: {reason}");

        public static Error IntentCreationFailed(string reason) => Error.Create(
            "Payment.IntentCreationFailed",
            $"Failed to create payment intent: {reason}");
    }

    /// <summary>
    /// PaymentMethod-related errors
    /// </summary>
    public static class PaymentMethod
    {
        public static Error NotFound => Error.Create(
            "PaymentMethod.NotFound",
            "Payment method was not found");

        public static Error AlreadyExists => Error.Create(
            "PaymentMethod.AlreadyExists",
            "Payment method already exists");

        public static Error MaxLimitReached => Error.Create(
            "PaymentMethod.MaxLimitReached",
            "Maximum number of payment methods reached");

        public static Error CannotSetAsDefault => Error.Create(
            "PaymentMethod.CannotSetAsDefault",
            "Cannot set inactive payment method as default");

        public static Error Inactive => Error.Create(
            "PaymentMethod.Inactive",
            "Payment method is not active");

        public static Error InvalidCardDetails => Error.Create(
            "PaymentMethod.InvalidCardDetails",
            "Invalid card details");

        public static Error CannotDeleteDefaultMethod => Error.Create(
            "PaymentMethod.CannotDeleteDefaultMethod",
            "Cannot delete default payment method");
    }

    /// <summary>
    /// PaymentRetry-related errors
    /// </summary>
    public static class PaymentRetry
    {
        public static Error NotFound => Error.Create(
            "PaymentRetry.NotFound",
            "Payment retry schedule was not found");

        public static Error MaxRetriesReached => Error.Create(
            "PaymentRetry.MaxRetriesReached",
            "Maximum number of retries reached");

        public static Error AlreadyCompleted => Error.Create(
            "PaymentRetry.AlreadyCompleted",
            "Payment retry already completed");

        public static Error CannotRetry => Error.Create(
            "PaymentRetry.CannotRetry",
            "Payment cannot be retried");

        public static Error InvalidRetryDelay => Error.Create(
            "PaymentRetry.InvalidRetryDelay",
            "Invalid retry delay");

        public static Error AlreadyActive => Error.Create(
            "PaymentRetry.AlreadyActive",
            "Payment already has an active retry schedule");

        public static Error NotFailedStatus => Error.Create(
            "PaymentRetry.NotFailedStatus",
            "Only failed payments can be retried");
    }

    /// <summary>
    /// PlatformAdmin-related errors
    /// </summary>
    public static class Admin
    {
        public static Error AlreadyExists => Error.Create(
            "Admin.AlreadyExists",
            "Platform Admin account already exists");
    }

    /// <summary>
    /// ProviderSubscription-related errors (Provider's platform subscription to Orbito)
    /// </summary>
    public static class ProviderSubscription
    {
        public static Error NotFound => Error.Create(
            "ProviderSubscription.NotFound",
            "Provider subscription was not found");

        public static Error StillActive => Error.Create(
            "ProviderSubscription.StillActive",
            "Subscription is still active and cannot be expired");

        public static Error TrialExpired => Error.Create(
            "ProviderSubscription.TrialExpired",
            "Your trial period has expired. Please subscribe to continue");

        public static Error PlanNotFound => Error.Create(
            "ProviderSubscription.PlanNotFound",
            "Selected platform plan does not exist");

        public static Error AlreadyExists => Error.Create(
            "ProviderSubscription.AlreadyExists",
            "Provider already has an active subscription");

        public static Error AlreadyCancelled => Error.Create(
            "ProviderSubscription.AlreadyCancelled",
            "Subscription is already cancelled");

        public static Error NotificationAlreadySent => Error.Create(
            "ProviderSubscription.NotificationAlreadySent",
            "This notification tier has already been sent");
    }

    /// <summary>
    /// PlatformPlan-related errors
    /// </summary>
    public static class PlatformPlan
    {
        public static Error NotFound => Error.Create(
            "PlatformPlan.NotFound",
            "Platform plan was not found");

        public static Error NameAlreadyExists => Error.Create(
            "PlatformPlan.NameAlreadyExists",
            "A platform plan with this name already exists");

        public static Error Inactive => Error.Create(
            "PlatformPlan.Inactive",
            "Platform plan is not active");
    }

    /// <summary>
    /// User/Authentication-related errors
    /// </summary>
    public static class User
    {
        public static Error NotFound => Error.Create(
            "User.NotFound",
            "User was not found");

        public static Error InvalidCredentials => Error.Create(
            "User.InvalidCredentials",
            "Invalid credentials");

        public static Error EmailNotConfirmed => Error.Create(
            "User.EmailNotConfirmed",
            "Email address not confirmed");

        public static Error AccountLocked => Error.Create(
            "User.AccountLocked",
            "Account is locked");

        public static Error AlreadyExists => Error.Create(
            "User.AlreadyExists",
            "User already exists");
    }

    /// <summary>
    /// TeamMember-related errors
    /// </summary>
    public static class TeamMember
    {
        public static Error NotFound => Error.Create(
            "TeamMember.NotFound",
            "Team member was not found");

        public static Error EmailAlreadyExists => Error.Create(
            "TeamMember.EmailAlreadyExists",
            "A team member with this email already exists");

        public static Error Inactive => Error.Create(
            "TeamMember.Inactive",
            "Team member is not active");

        public static Error AlreadyInactive => Error.Create(
            "TeamMember.AlreadyInactive",
            "Team member is already inactive");

        public static Error CannotRemoveOwner => Error.Create(
            "TeamMember.CannotRemoveOwner",
            "Cannot remove the owner from the team");

        public static Error CannotAssignOwnerRole => Error.Create(
            "TeamMember.CannotAssignOwnerRole",
            "Cannot assign owner role without proper authorization");

        public static Error CannotDemoteOwner => Error.Create(
            "TeamMember.CannotDemoteOwner",
            "Cannot demote the owner to a lower role");

        public static Error SameRole => Error.Create(
            "TeamMember.SameRole",
            "Team member already has this role");

        public static Error InvalidRole => Error.Create(
            "TeamMember.InvalidRole",
            "Invalid team member role");

        public static Error InvitationExpired => Error.Create(
            "TeamMember.InvitationExpired",
            "Team member invitation has expired");

        public static Error AlreadyAccepted => Error.Create(
            "TeamMember.AlreadyAccepted",
            "Team member invitation has already been accepted");
    }

    /// <summary>
    /// Validation-related errors
    /// </summary>
    public static class Validation
    {
        public static Error Required(string fieldName) => Error.Create(
            "Validation.Required",
            $"{fieldName} is required");

        public static Error InvalidFormat(string fieldName) => Error.Create(
            "Validation.InvalidFormat",
            $"{fieldName} has invalid format");

        public static Error TooLong(string fieldName, int maxLength) => Error.Create(
            "Validation.TooLong",
            $"{fieldName} cannot be longer than {maxLength} characters");

        public static Error TooShort(string fieldName, int minLength) => Error.Create(
            "Validation.TooShort",
            $"{fieldName} must be at least {minLength} characters");

        public static Error OutOfRange(string fieldName, object min, object max) => Error.Create(
            "Validation.OutOfRange",
            $"{fieldName} must be between {min} and {max}");

        public static Error InvalidPageNumber => Error.Create(
            "Validation.InvalidPageNumber",
            "Page number must be greater than zero");

        public static Error InvalidPageSize => Error.Create(
            "Validation.InvalidPageSize",
            "Page size must be between 1 and 100");
    }
}
