using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;
using Orbito.Domain.Enums;
using Orbito.Domain.Errors;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Subscriptions.Commands.CreateSubscription;

using Orbito.Application.Subscriptions;

/// <summary>
/// Handler for creating a new subscription for a client.
/// </summary>
public class CreateSubscriptionCommandHandler : IRequestHandler<CreateSubscriptionCommand, Result<SubscriptionDto>>
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly IClientRepository _clientRepository;
    private readonly ISubscriptionPlanRepository _subscriptionPlanRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateSubscriptionCommandHandler> _logger;

    public CreateSubscriptionCommandHandler(
        ISubscriptionService subscriptionService,
        IClientRepository clientRepository,
        ISubscriptionPlanRepository subscriptionPlanRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork,
        ILogger<CreateSubscriptionCommandHandler> logger)
    {
        _subscriptionService = subscriptionService;
        _clientRepository = clientRepository;
        _subscriptionPlanRepository = subscriptionPlanRepository;
        _tenantContext = tenantContext;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<SubscriptionDto>> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating subscription for client {ClientId} with plan {PlanId}",
            request.ClientId, request.PlanId);

        // SECURITY: Verify tenant context
        if (!_tenantContext.HasTenant)
        {
            _logger.LogWarning("Attempted to create subscription without tenant context");
            return Result.Failure<SubscriptionDto>(DomainErrors.Tenant.NoTenantContext);
        }

        // Validate client exists
        var client = await _clientRepository.GetByIdAsync(request.ClientId, cancellationToken);
        if (client == null)
        {
            _logger.LogWarning("Client with ID {ClientId} not found", request.ClientId);
            return Result.Failure<SubscriptionDto>(DomainErrors.Client.NotFound);
        }

        // SECURITY: Cross-tenant protection
        if (client.TenantId != _tenantContext.CurrentTenantId)
        {
            _logger.LogWarning("Cross-tenant access attempt: Client {ClientId} belongs to tenant {ClientTenantId}, current tenant is {CurrentTenantId}",
                request.ClientId, client.TenantId.Value, _tenantContext.CurrentTenantId?.Value);
            return Result.Failure<SubscriptionDto>(DomainErrors.Client.NotFound);
        }

        // Validate plan exists and is active
        var plan = await _subscriptionPlanRepository.GetByIdAsync(request.PlanId, cancellationToken);
        if (plan == null)
        {
            _logger.LogWarning("Subscription plan with ID {PlanId} not found", request.PlanId);
            return Result.Failure<SubscriptionDto>(DomainErrors.SubscriptionPlan.NotFound);
        }

        if (!plan.IsActive)
        {
            _logger.LogWarning("Subscription plan with ID {PlanId} is not active", request.PlanId);
            return Result.Failure<SubscriptionDto>(DomainErrors.SubscriptionPlan.Inactive);
        }

        // Create value objects
        var price = Money.Create(request.Amount, request.Currency);
        var billingPeriodType = Enum.Parse<BillingPeriodType>(request.BillingPeriodType, true);
        var billingPeriod = BillingPeriod.Create(request.BillingPeriodValue, billingPeriodType);

        // Create subscription
        var subscription = await _subscriptionService.CreateSubscriptionAsync(
            request.ClientId,
            request.PlanId,
            price,
            billingPeriod,
            request.TrialDays,
            cancellationToken);

        // CRITICAL: Save changes to database
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully created subscription {SubscriptionId} for client {ClientId}",
            subscription.Id, request.ClientId);

        var subscriptionDto = SubscriptionMapper.ToDto(subscription);

        return Result.Success(subscriptionDto);
    }
}
