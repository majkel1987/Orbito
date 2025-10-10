using FluentValidation;
using Orbito.Application.Common.Constants;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Features.Payments.Queries;
using Orbito.Domain.Enums;

namespace Orbito.Application.Validators
{
    /// <summary>
    /// Validator for GetScheduledRetriesQuery
    /// </summary>
    public class GetScheduledRetriesQueryValidator : AbstractValidator<GetScheduledRetriesQuery>
    {
        private readonly ITenantContext _tenantContext;
        private readonly IClientRepository _clientRepository;
        private readonly ISecurityLimitService _securityLimitService;

        public GetScheduledRetriesQueryValidator(
            ITenantContext tenantContext,
            IClientRepository clientRepository,
            ISecurityLimitService securityLimitService)
        {
            _tenantContext = tenantContext;
            _clientRepository = clientRepository;
            _securityLimitService = securityLimitService;

            // Pagination validation
            RuleFor(x => x.Pagination)
                .NotNull()
                .WithMessage("Pagination parameters are required");

            RuleFor(x => x.Pagination.PageNumber)
                .GreaterThan(0)
                .WithMessage("Page number must be greater than 0");

            RuleFor(x => x.Pagination.PageSize)
                .GreaterThan(0)
                .WithMessage("Page size must be greater than 0")
                .LessThanOrEqualTo(ValidationConstants.MaxPageSize)
                .WithMessage($"Page size cannot exceed {ValidationConstants.MaxPageSize} records");

            // ClientId validation (optional but if provided, must be valid)
            RuleFor(x => x.ClientId)
                .MustAsync(async (clientId, ct) =>
                {
                    if (!clientId.HasValue)
                        return true; // Optional field

                    if (!_tenantContext.HasTenant)
                        return false;

                    var client = await _clientRepository.GetByIdAsync(clientId.Value, ct);
                    return client != null && client.TenantId == _tenantContext.CurrentTenantId;
                })
                .WithMessage("Client does not exist or does not belong to current tenant")
                .When(x => x.ClientId.HasValue);

            // Status validation - must be valid RetryStatus enum value
            RuleFor(x => x.Status)
                .Must(status => string.IsNullOrEmpty(status) || 
                               Enum.TryParse<RetryStatus>(status, true, out _))
                .WithMessage("Status must be a valid retry status (Scheduled, InProgress, Completed, Failed, Cancelled)")
                .When(x => !string.IsNullOrEmpty(x.Status));

            // Security: Prevent excessive page size to avoid DoS attacks
            RuleFor(x => x.Pagination.PageSize)
                .Must(pageSize => pageSize <= _securityLimitService.MaxPageSize)
                .WithMessage($"Page size cannot exceed {_securityLimitService.MaxPageSize} records")
                .When(x => x.Pagination != null);
        }
    }
}
