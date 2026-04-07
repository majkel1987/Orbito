using FluentValidation;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Features.Payments.Commands.ProcessPayment;
using Orbito.Domain.Enums;

namespace Orbito.Application.Features.Payments.Commands.Validators;

/// <summary>
/// Validator for process payment command.
/// Validates subscription ID, client ID, amount, currency, external IDs, tenant ownership, and subscription status.
/// </summary>
public class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
{
    private readonly ITenantContext _tenantContext;
    private readonly IClientRepository _clientRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;

    public ProcessPaymentCommandValidator(
        ITenantContext tenantContext,
        IClientRepository clientRepository,
        ISubscriptionRepository subscriptionRepository)
    {
        _tenantContext = tenantContext;
        _clientRepository = clientRepository;
        _subscriptionRepository = subscriptionRepository;

        RuleFor(x => x.SubscriptionId)
            .NotEmpty()
            .WithMessage("Subscription ID is required");

        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("Client ID is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Payment amount must be greater than zero")
            .LessThan(1000000)
            .WithMessage("Payment amount cannot exceed 1,000,000");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required")
            .Length(3)
            .WithMessage("Currency must be a 3-character code (e.g., USD, EUR, PLN)")
            .Matches(@"^[A-Z]{3}$")
            .WithMessage("Currency must be a valid 3-letter uppercase code");

        RuleFor(x => x.ExternalTransactionId)
            .MaximumLength(255)
            .When(x => !string.IsNullOrWhiteSpace(x.ExternalTransactionId))
            .WithMessage("External transaction ID must not exceed 255 characters");

        RuleFor(x => x.PaymentMethod)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.PaymentMethod))
            .WithMessage("Payment method must not exceed 50 characters");

        RuleFor(x => x.ExternalPaymentId)
            .MaximumLength(255)
            .When(x => !string.IsNullOrWhiteSpace(x.ExternalPaymentId))
            .WithMessage("External payment ID must not exceed 255 characters");

        // Security: Verify ClientId belongs to current tenant
        RuleFor(x => x.ClientId)
            .MustAsync(async (clientId, ct) =>
            {
                if (!_tenantContext.HasTenant)
                    return false;

                var client = await _clientRepository.GetByIdAsync(clientId, ct);
                return client != null && client.TenantId == _tenantContext.CurrentTenantId;
            })
            .WithMessage("Client does not exist or does not belong to current tenant")
            .When(x => x.ClientId != Guid.Empty);

        // Security: Verify SubscriptionId belongs to ClientId and is Active
        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                // Use secure GetByIdForClientAsync to verify subscription belongs to client
                var subscription = await _subscriptionRepository.GetByIdForClientAsync(cmd.SubscriptionId, cmd.ClientId, ct);
                if (subscription == null)
                    return false;

                // Verify subscription is Active
                return subscription.Status == SubscriptionStatus.Active;
            })
            .WithMessage("Subscription not found, does not belong to specified client, or is not Active")
            .When(x => x.SubscriptionId != Guid.Empty && x.ClientId != Guid.Empty);
    }
}
