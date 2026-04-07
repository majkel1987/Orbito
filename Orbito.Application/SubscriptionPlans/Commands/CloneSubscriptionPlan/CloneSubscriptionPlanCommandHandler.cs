using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Common;
using Orbito.Domain.Entities;
using Orbito.Domain.Errors;

namespace Orbito.Application.SubscriptionPlans.Commands.CloneSubscriptionPlan;

public class CloneSubscriptionPlanCommandHandler : IRequestHandler<CloneSubscriptionPlanCommand, Result<CloneSubscriptionPlanResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<CloneSubscriptionPlanCommandHandler> _logger;

    public CloneSubscriptionPlanCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<CloneSubscriptionPlanCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<CloneSubscriptionPlanResult>> Handle(CloneSubscriptionPlanCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.HasTenant || _tenantContext.CurrentTenantId == null)
        {
            _logger.LogWarning("Attempted to clone subscription plan without valid tenant context");
            return Result.Failure<CloneSubscriptionPlanResult>(DomainErrors.Tenant.NoTenantContext);
        }

        var originalPlan = await _unitOfWork.SubscriptionPlans.GetByIdAsync(request.Id, cancellationToken);
        if (originalPlan == null)
        {
            _logger.LogWarning("Subscription plan {PlanId} not found for cloning", request.Id);
            return Result.Failure<CloneSubscriptionPlanResult>(DomainErrors.SubscriptionPlan.NotFound);
        }

        // Create new plan based on original
        var clonedPlan = SubscriptionPlan.Create(
            _tenantContext.CurrentTenantId!,
            request.NewName,
            request.NewAmount ?? originalPlan.Price.Amount,
            request.NewCurrency ?? originalPlan.Price.Currency,
            originalPlan.BillingPeriod.Type,
            request.NewDescription ?? originalPlan.Description,
            originalPlan.TrialDays,
            originalPlan.TrialPeriodDays,
            originalPlan.FeaturesJson,
            originalPlan.LimitationsJson,
            request.NewSortOrder ?? originalPlan.SortOrder);

        // Set status and visibility
        if (request.IsActive)
            clonedPlan.Activate();
        else
            clonedPlan.Deactivate();

        clonedPlan.UpdateVisibility(request.IsPublic);

        await _unitOfWork.SubscriptionPlans.AddAsync(clonedPlan, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var result = new CloneSubscriptionPlanResult
        {
            Id = clonedPlan.Id,
            Name = clonedPlan.Name,
            Description = clonedPlan.Description,
            Amount = clonedPlan.Price.Amount,
            Currency = clonedPlan.Price.Currency,
            BillingPeriod = clonedPlan.BillingPeriod.ToString(),
            TrialPeriodDays = clonedPlan.TrialPeriodDays,
            IsActive = clonedPlan.IsActive,
            IsPublic = clonedPlan.IsPublic,
            SortOrder = clonedPlan.SortOrder,
            CreatedAt = clonedPlan.CreatedAt,
            OriginalPlanId = originalPlan.Id
        };

        _logger.LogInformation(
            "Cloned subscription plan {OriginalPlanId} to {NewPlanId} with name {NewPlanName}",
            originalPlan.Id,
            clonedPlan.Id,
            clonedPlan.Name);

        return Result.Success(result);
    }
}
