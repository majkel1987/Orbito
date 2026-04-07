using FluentValidation;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Enums;

namespace Orbito.Application.Features.Payments.Commands.Validators;

/// <summary>
/// Validator for retry failed payment command.
/// Validates payment ID, client ID, optional reason, tenant ownership, payment status, and no active retries.
/// </summary>
public class RetryFailedPaymentCommandValidator : AbstractValidator<RetryFailedPaymentCommand>
{
    private readonly ITenantContext _tenantContext;
    private readonly IClientRepository _clientRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentRetryRepository _retryRepository;

    public RetryFailedPaymentCommandValidator(
        ITenantContext tenantContext,
        IClientRepository clientRepository,
        IPaymentRepository paymentRepository,
        IPaymentRetryRepository retryRepository)
    {
        _tenantContext = tenantContext;
        _clientRepository = clientRepository;
        _paymentRepository = paymentRepository;
        _retryRepository = retryRepository;

        RuleFor(x => x.PaymentId)
            .NotEmpty()
            .WithMessage("Payment ID is required");

        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("Client ID is required");

        RuleFor(x => x.Reason)
            .Must(reason => !string.IsNullOrWhiteSpace(reason))
            .WithMessage("Reason cannot be only whitespace")
            .When(x => !string.IsNullOrEmpty(x.Reason));

        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .WithMessage("Reason cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Reason));

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

        // Business logic: Verify payment exists, belongs to client, has Failed status, and no active retry
        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                // Use secure GetByIdForClientAsync to verify payment belongs to client
                var payment = await _paymentRepository.GetByIdForClientAsync(cmd.PaymentId, cmd.ClientId, ct);
                if (payment == null)
                    return false;

                // Verify payment has Failed status
                if (payment.Status != PaymentStatus.Failed)
                    return false;

                // Verify no active retry schedule exists
                var existingRetries = await _retryRepository.GetActiveRetriesByPaymentIdAsync(cmd.PaymentId, ct);
                return !existingRetries.Any();
            })
            .WithMessage("Payment not found, does not belong to client, is not in Failed status, or already has an active retry schedule")
            .When(x => x.PaymentId != Guid.Empty && x.ClientId != Guid.Empty);
    }
}
