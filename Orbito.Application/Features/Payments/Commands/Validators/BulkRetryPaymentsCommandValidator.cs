using FluentValidation;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Enums;

namespace Orbito.Application.Features.Payments.Commands.Validators;

/// <summary>
/// Validator for bulk retry payments command.
/// Combines basic input validation with advanced business rules including
/// tenant verification and payment status checks.
/// </summary>
public class BulkRetryPaymentsCommandValidator : AbstractValidator<BulkRetryPaymentsCommand>
{
    /// <summary>
    /// Maximum number of payments allowed in a single bulk retry operation
    /// </summary>
    private const int MaxBulkRetryLimit = 50;

    private readonly ITenantContext _tenantContext;
    private readonly IClientRepository _clientRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentRetryRepository _retryRepository;

    public BulkRetryPaymentsCommandValidator(
        ITenantContext tenantContext,
        IClientRepository clientRepository,
        IPaymentRepository paymentRepository,
        IPaymentRetryRepository retryRepository)
    {
        _tenantContext = tenantContext;
        _clientRepository = clientRepository;
        _paymentRepository = paymentRepository;
        _retryRepository = retryRepository;

        // Basic validations
        RuleFor(x => x.PaymentIds)
            .NotEmpty()
            .WithMessage("At least one payment ID is required")
            .Must(ids => ids.Count <= MaxBulkRetryLimit)
            .WithMessage($"Cannot retry more than {MaxBulkRetryLimit} payments at once");

        RuleForEach(x => x.PaymentIds)
            .NotEmpty()
            .WithMessage("Payment ID cannot be empty");

        // Validate all payment IDs are valid GUIDs
        RuleFor(x => x.PaymentIds)
            .Must(ids => ids.All(id => id != Guid.Empty))
            .WithMessage("All payment IDs must be valid GUIDs")
            .When(x => x.PaymentIds.Any());

        // Validate uniqueness of payment IDs
        RuleFor(x => x.PaymentIds)
            .Must(ids => ids.Count == ids.Distinct().Count())
            .WithMessage("Payment IDs must be unique - duplicates are not allowed")
            .When(x => x.PaymentIds.Any());

        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("Client ID is required");

        // Reason validation with whitespace check
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

        // Business logic: Verify all payments exist and belong to client
        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                var payments = await _paymentRepository.GetByIdsForClientAsync(cmd.PaymentIds, cmd.ClientId, ct);
                return payments.Count == cmd.PaymentIds.Count;
            })
            .WithMessage("Some payments not found or do not belong to specified client")
            .When(x => x.PaymentIds.Any() && x.ClientId != Guid.Empty);

        // Business logic: Verify all payments have Failed status
        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                var payments = await _paymentRepository.GetByIdsForClientAsync(cmd.PaymentIds, cmd.ClientId, ct);
                return payments.Values.All(p => p.Status == PaymentStatus.Failed);
            })
            .WithMessage("All payments must have Failed status to be retried")
            .When(x => x.PaymentIds.Any() && x.ClientId != Guid.Empty);

        // Business logic: Verify no active retry schedules exist for these payments
        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                var existingRetries = await _retryRepository.GetRetrySchedulesByPaymentIdsAsync(cmd.PaymentIds, cmd.ClientId, ct);
                var activeRetries = existingRetries.Where(r =>
                    r.Status == RetryStatus.Scheduled ||
                    r.Status == RetryStatus.InProgress).ToList();

                return !activeRetries.Any();
            })
            .WithMessage("Some payments already have active retry schedules. Cancel existing retries first.")
            .When(x => x.PaymentIds.Any() && x.ClientId != Guid.Empty);
    }
}
