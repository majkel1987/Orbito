using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models.PaymentGateway;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;

namespace Orbito.Application.Features.ProviderSubscriptions.Commands.CreateProviderPaymentIntent
{
    /// <summary>
    /// Handler for creating Stripe PaymentIntent for Provider platform subscription.
    /// PCI DSS compliant - card data stays on Stripe's servers.
    /// </summary>
    public class CreateProviderPaymentIntentCommandHandler
        : IRequestHandler<CreateProviderPaymentIntentCommand, Result<CreateProviderPaymentIntentResponse>>
    {
        private readonly ITenantContext _tenantContext;
        private readonly IProviderRepository _providerRepository;
        private readonly IProviderSubscriptionRepository _providerSubscriptionRepository;
        private readonly IPlatformPlanRepository _platformPlanRepository;
        private readonly IPaymentGateway _paymentGateway;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateProviderPaymentIntentCommandHandler> _logger;

        public CreateProviderPaymentIntentCommandHandler(
            ITenantContext tenantContext,
            IProviderRepository providerRepository,
            IProviderSubscriptionRepository providerSubscriptionRepository,
            IPlatformPlanRepository platformPlanRepository,
            IPaymentGateway paymentGateway,
            IUnitOfWork unitOfWork,
            ILogger<CreateProviderPaymentIntentCommandHandler> logger)
        {
            _tenantContext = tenantContext;
            _providerRepository = providerRepository;
            _providerSubscriptionRepository = providerSubscriptionRepository;
            _platformPlanRepository = platformPlanRepository;
            _paymentGateway = paymentGateway;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<CreateProviderPaymentIntentResponse>> Handle(
            CreateProviderPaymentIntentCommand request,
            CancellationToken cancellationToken)
        {
            // 1. Get current tenant (Provider's TenantId == Provider.Id)
            var tenantId = _tenantContext.CurrentTenantId;
            if (tenantId == null)
            {
                _logger.LogWarning("No tenant context available");
                return Result.Failure<CreateProviderPaymentIntentResponse>(DomainErrors.Provider.NotFound);
            }

            var providerId = tenantId.Value;
            _logger.LogInformation(
                "Creating Provider PaymentIntent for provider {ProviderId}, plan {PlatformPlanId}",
                providerId, request.PlatformPlanId);

            // 2. Get Provider
            var provider = await _providerRepository.GetByIdAsync(providerId, cancellationToken);
            if (provider == null)
            {
                _logger.LogWarning("Provider {ProviderId} not found", providerId);
                return Result.Failure<CreateProviderPaymentIntentResponse>(DomainErrors.Provider.NotFound);
            }

            // 3. Get ProviderSubscription
            var subscription = await _providerSubscriptionRepository.GetByProviderIdAsync(providerId, cancellationToken);
            if (subscription == null)
            {
                _logger.LogWarning("ProviderSubscription not found for provider {ProviderId}", providerId);
                return Result.Failure<CreateProviderPaymentIntentResponse>(DomainErrors.ProviderSubscription.NotFound);
            }

            // 4. Determine which plan to use
            var planId = request.PlatformPlanId ?? subscription.PlatformPlanId;
            var plan = await _platformPlanRepository.GetByIdAsync(planId, cancellationToken);
            if (plan == null)
            {
                _logger.LogWarning("PlatformPlan {PlanId} not found", planId);
                return Result.Failure<CreateProviderPaymentIntentResponse>(DomainErrors.ProviderSubscription.PlanNotFound);
            }

            if (!plan.IsActive)
            {
                _logger.LogWarning("PlatformPlan {PlanId} is not active", planId);
                return Result.Failure<CreateProviderPaymentIntentResponse>(DomainErrors.PlatformPlan.Inactive);
            }

            // 5. Ensure provider has Stripe customer ID (create if needed)
            if (string.IsNullOrEmpty(provider.StripeCustomerId))
            {
                _logger.LogInformation(
                    "Creating Stripe customer for provider {ProviderId}",
                    providerId);

                var createCustomerResult = await _paymentGateway.CreateCustomerAsync(
                    new CreateCustomerRequest
                    {
                        ClientId = providerId, // Using ProviderId as ClientId for Stripe
                        TenantId = tenantId,
                        Email = provider.User?.Email ?? $"provider-{providerId}@orbito.pl",
                        FirstName = provider.BusinessName,
                        LastName = "",
                        Phone = null,
                        Metadata = new Dictionary<string, string>
                        {
                            ["orbito_provider_id"] = providerId.ToString(),
                            ["customer_type"] = "provider"
                        }
                    });

                if (!createCustomerResult.IsSuccess)
                {
                    _logger.LogError(
                        "Failed to create Stripe customer for provider {ProviderId}: {Error}",
                        providerId, createCustomerResult.ErrorMessage);
                    return Result.Failure<CreateProviderPaymentIntentResponse>(
                        DomainErrors.Payment.CustomerCreationFailed(createCustomerResult.ErrorMessage ?? "Unknown error"));
                }

                provider.StripeCustomerId = createCustomerResult.ExternalCustomerId;
                await _providerRepository.UpdateAsync(provider, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Created Stripe customer {StripeCustomerId} for provider {ProviderId}",
                    provider.StripeCustomerId, providerId);
            }

            // 6. Create PaymentIntent via Stripe with platform-specific metadata
            var createIntentResult = await _paymentGateway.CreatePaymentIntentAsync(
                new CreatePaymentIntentRequest
                {
                    Amount = plan.Price,
                    CustomerId = provider.StripeCustomerId!,
                    SubscriptionId = subscription.Id,
                    ClientId = providerId, // Provider as client
                    TenantId = tenantId.Value,
                    Description = $"Platform subscription: {plan.Name}",
                    Metadata = new Dictionary<string, string>
                    {
                        ["provider_id"] = providerId.ToString(),
                        ["platform_plan_id"] = plan.Id.ToString(),
                        ["subscription_type"] = "platform" // Key differentiator!
                    }
                });

            if (!createIntentResult.IsSuccess)
            {
                _logger.LogError(
                    "Failed to create PaymentIntent for provider {ProviderId}: {Error}",
                    providerId, createIntentResult.ErrorMessage);
                return Result.Failure<CreateProviderPaymentIntentResponse>(
                    DomainErrors.Payment.IntentCreationFailed(createIntentResult.ErrorMessage ?? "Unknown error"));
            }

            _logger.LogInformation(
                "Created PaymentIntent {PaymentIntentId} for provider {ProviderId}, plan {PlanName}",
                createIntentResult.PaymentIntentId, providerId, plan.Name);

            // 7. Return response with client secret
            return Result.Success(new CreateProviderPaymentIntentResponse
            {
                ClientSecret = createIntentResult.ClientSecret!,
                PaymentIntentId = createIntentResult.PaymentIntentId!,
                Amount = createIntentResult.Amount,
                Currency = createIntentResult.Currency!,
                PlanName = plan.Name
            });
        }
    }
}
