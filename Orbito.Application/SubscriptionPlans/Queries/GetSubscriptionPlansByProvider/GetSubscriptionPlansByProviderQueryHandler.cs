using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;

namespace Orbito.Application.SubscriptionPlans.Queries.GetSubscriptionPlansByProvider;

public class GetSubscriptionPlansByProviderQueryHandler : IRequestHandler<GetSubscriptionPlansByProviderQuery, Orbito.Domain.Common.Result<PaginatedList<SubscriptionPlanListItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetSubscriptionPlansByProviderQueryHandler> _logger;

    public GetSubscriptionPlansByProviderQueryHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<GetSubscriptionPlansByProviderQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Orbito.Domain.Common.Result<PaginatedList<SubscriptionPlanListItemDto>>> Handle(GetSubscriptionPlansByProviderQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.HasTenant || _tenantContext.CurrentTenantId == null)
        {
            _logger.LogWarning("Attempted to get subscription plans without valid tenant context");
            return Orbito.Domain.Common.Result.Failure<PaginatedList<SubscriptionPlanListItemDto>>(DomainErrors.Tenant.NoTenantContext);
        }

        var subscriptionPlans = await _unitOfWork.SubscriptionPlans.GetAllAsync(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm,
            request.ActiveOnly,
            request.PublicOnly,
            cancellationToken);

        var totalCount = await _unitOfWork.SubscriptionPlans.GetCountAsync(
            request.SearchTerm,
            request.ActiveOnly,
            request.PublicOnly,
            cancellationToken);

        var items = SubscriptionPlanMapper.ToListItemDtos(subscriptionPlans);

        var paginatedList = new PaginatedList<SubscriptionPlanListItemDto>(
            items,
            totalCount,
            request.PageNumber,
            request.PageSize);

        _logger.LogDebug("Successfully retrieved {Count} subscription plans for page {PageNumber}", items.Count, request.PageNumber);

        return Orbito.Domain.Common.Result.Success(paginatedList);
    }
}
