using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;

namespace Orbito.Application.SubscriptionPlans.Commands.DeleteSubscriptionPlan;

public class DeleteSubscriptionPlanCommandHandler : IRequestHandler<DeleteSubscriptionPlanCommand, Result<DeleteSubscriptionPlanResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<DeleteSubscriptionPlanCommandHandler> _logger;

    public DeleteSubscriptionPlanCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<DeleteSubscriptionPlanCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<DeleteSubscriptionPlanResult>> Handle(DeleteSubscriptionPlanCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.HasTenant || _tenantContext.CurrentTenantId == null)
        {
            _logger.LogWarning("Attempted to delete subscription plan without valid tenant context");
            return Result.Failure<DeleteSubscriptionPlanResult>(DomainErrors.Tenant.NoTenantContext);
        }

        var subscriptionPlan = await _unitOfWork.SubscriptionPlans.GetByIdAsync(request.Id, cancellationToken);
        if (subscriptionPlan == null)
        {
            _logger.LogWarning("Subscription plan {PlanId} not found", request.Id);
            return Result.Failure<DeleteSubscriptionPlanResult>(DomainErrors.SubscriptionPlan.NotFound);
        }

        // Check if plan can be deleted
        if (!subscriptionPlan.CanBeDeleted() && !request.HardDelete)
        {
            var cannotDeleteResult = new DeleteSubscriptionPlanResult
            {
                Id = request.Id,
                IsDeleted = false,
                IsHardDelete = false,
                Message = "Cannot delete subscription plan with active subscriptions. Use hard delete to force deletion."
            };
            return Result.Success(cannotDeleteResult);
        }

        if (request.HardDelete)
        {
            await _unitOfWork.SubscriptionPlans.DeleteAsync(subscriptionPlan, cancellationToken);
        }
        else
        {
            subscriptionPlan.Deactivate();
            await _unitOfWork.SubscriptionPlans.UpdateAsync(subscriptionPlan, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var result = new DeleteSubscriptionPlanResult
        {
            Id = request.Id,
            IsDeleted = true,
            IsHardDelete = request.HardDelete,
            Message = request.HardDelete ? "Subscription plan permanently deleted" : "Subscription plan deactivated"
        };

        _logger.LogInformation(
            "Subscription plan {PlanId} {DeleteType}",
            request.Id,
            request.HardDelete ? "permanently deleted" : "deactivated");

        return Result.Success(result);
    }
}
