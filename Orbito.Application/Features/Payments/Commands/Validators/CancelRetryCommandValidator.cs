using FluentValidation;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Enums;

namespace Orbito.Application.Features.Payments.Commands.Validators;

/// <summary>
/// Validator for cancel retry command.
/// Validates schedule ID, client ID, tenant ownership, and cancellable status.
/// </summary>
public class CancelRetryCommandValidator : AbstractValidator<CancelRetryCommand>
{
    private readonly ITenantContext _tenantContext;
    private readonly IClientRepository _clientRepository;
    private readonly IPaymentRetryRepository _retryRepository;

    public CancelRetryCommandValidator(
        ITenantContext tenantContext,
        IClientRepository clientRepository,
        IPaymentRetryRepository retryRepository)
    {
        _tenantContext = tenantContext;
        _clientRepository = clientRepository;
        _retryRepository = retryRepository;

        RuleFor(x => x.ScheduleId)
            .NotEmpty()
            .WithMessage("Schedule ID is required");

        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("Client ID is required");

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

        // Business logic: Verify schedule exists and belongs to client, and can be cancelled
        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                var schedule = await _retryRepository.GetByIdForClientAsync(cmd.ScheduleId, cmd.ClientId, ct);
                if (schedule == null)
                    return false;

                // Verify schedule can be cancelled (not already completed/failed)
                return schedule.Status == RetryStatus.Scheduled ||
                       schedule.Status == RetryStatus.InProgress;
            })
            .WithMessage("Retry schedule not found, does not belong to specified client, or cannot be cancelled (already completed or failed)")
            .When(x => x.ScheduleId != Guid.Empty && x.ClientId != Guid.Empty);
    }
}
