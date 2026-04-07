using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models.PaymentGateway;
using Orbito.Domain.Common;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.Errors;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Features.Payments.Commands.CreatePaymentIntent
{
    /// <summary>
    /// Handler for creating Stripe PaymentIntent for client portal payments.
    /// PCI DSS compliant - card data stays on Stripe's servers.
    /// </summary>
    public class CreatePaymentIntentCommandHandler
        : IRequestHandler<CreatePaymentIntentCommand, Result<CreatePaymentIntentResponse>>
    {
        private readonly IUserContextService _userContextService;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPaymentGateway _paymentGateway;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantContext _tenantContext;
        private readonly ILogger<CreatePaymentIntentCommandHandler> _logger;

        public CreatePaymentIntentCommandHandler(
            IUserContextService userContextService,
            ISubscriptionRepository subscriptionRepository,
            IClientRepository clientRepository,
            IPaymentRepository paymentRepository,
            IPaymentGateway paymentGateway,
            IUnitOfWork unitOfWork,
            ITenantContext tenantContext,
            ILogger<CreatePaymentIntentCommandHandler> logger)
        {
            _userContextService = userContextService;
            _subscriptionRepository = subscriptionRepository;
            _clientRepository = clientRepository;
            _paymentRepository = paymentRepository;
            _paymentGateway = paymentGateway;
            _unitOfWork = unitOfWork;
            _tenantContext = tenantContext;
            _logger = logger;
        }

        public async Task<Result<CreatePaymentIntentResponse>> Handle(
            CreatePaymentIntentCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Creating PaymentIntent for subscription {SubscriptionId}",
                request.SubscriptionId);

            // 1. Get current client from context
            var clientId = await _userContextService.GetCurrentClientIdAsync(cancellationToken);
            if (clientId == null)
            {
                _logger.LogWarning("Cannot resolve client ID for current user");
                return Result.Failure<CreatePaymentIntentResponse>(DomainErrors.Client.NotFound);
            }

            // 2. Get subscription with client verification
            var subscription = await _subscriptionRepository.GetByIdForClientAsync(
                request.SubscriptionId,
                clientId.Value,
                cancellationToken);

            if (subscription == null)
            {
                _logger.LogWarning(
                    "Subscription {SubscriptionId} not found or doesn't belong to client {ClientId}",
                    request.SubscriptionId, clientId.Value);
                return Result.Failure<CreatePaymentIntentResponse>(DomainErrors.Subscription.NotFound);
            }

            // 3. Get client with Stripe customer info
            var client = await _clientRepository.GetByIdAsync(clientId.Value, cancellationToken);
            if (client == null)
            {
                _logger.LogWarning("Client {ClientId} not found", clientId.Value);
                return Result.Failure<CreatePaymentIntentResponse>(DomainErrors.Client.NotFound);
            }

            // 4. Ensure client has Stripe customer ID (create if needed)
            if (string.IsNullOrEmpty(client.StripeCustomerId))
            {
                _logger.LogInformation(
                    "Creating Stripe customer for client {ClientId}",
                    clientId.Value);

                var createCustomerResult = await _paymentGateway.CreateCustomerAsync(
                    new CreateCustomerRequest
                    {
                        ClientId = client.Id,
                        TenantId = client.TenantId,
                        Email = client.Email,
                        FirstName = client.FirstName,
                        LastName = client.LastName,
                        Phone = client.Phone,
                        Metadata = new Dictionary<string, string>
                        {
                            ["orbito_client_id"] = client.Id.ToString()
                        }
                    });

                if (!createCustomerResult.IsSuccess)
                {
                    _logger.LogError(
                        "Failed to create Stripe customer for client {ClientId}: {Error}",
                        clientId.Value, createCustomerResult.ErrorMessage);
                    return Result.Failure<CreatePaymentIntentResponse>(
                        DomainErrors.Payment.CustomerCreationFailed(createCustomerResult.ErrorMessage ?? "Unknown error"));
                }

                var setStripeResult = client.SetStripeCustomerId(createCustomerResult.ExternalCustomerId!);
                if (setStripeResult.IsFailure)
                {
                    return Result.Failure<CreatePaymentIntentResponse>(setStripeResult.Error);
                }
                await _clientRepository.UpdateAsync(client, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Created Stripe customer {StripeCustomerId} for client {ClientId}",
                    client.StripeCustomerId, clientId.Value);
            }

            // 5. Create PaymentIntent via Stripe
            var tenantId = _tenantContext.CurrentTenantId;
            var createIntentResult = await _paymentGateway.CreatePaymentIntentAsync(
                new CreatePaymentIntentRequest
                {
                    Amount = subscription.CurrentPrice,
                    CustomerId = client.StripeCustomerId!,
                    SubscriptionId = subscription.Id,
                    ClientId = client.Id,
                    TenantId = tenantId.Value,
                    Description = $"Payment for subscription: {subscription.Plan?.Name ?? "Subscription"}"
                });

            if (!createIntentResult.IsSuccess)
            {
                _logger.LogError(
                    "Failed to create PaymentIntent for subscription {SubscriptionId}: {Error}",
                    request.SubscriptionId, createIntentResult.ErrorMessage);
                return Result.Failure<CreatePaymentIntentResponse>(
                    DomainErrors.Payment.IntentCreationFailed(createIntentResult.ErrorMessage ?? "Unknown error"));
            }

            // 6. Create Payment record with Processing status
            var payment = Payment.Create(
                tenantId,
                subscription.Id,
                client.Id,
                subscription.CurrentPrice,
                externalPaymentId: createIntentResult.PaymentIntentId);

            payment.MarkAsProcessing();

            await _paymentRepository.AddAsync(payment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Created PaymentIntent {PaymentIntentId} and Payment {PaymentId} for subscription {SubscriptionId}",
                createIntentResult.PaymentIntentId, payment.Id, subscription.Id);

            // 7. Return response with client secret
            return Result.Success(new CreatePaymentIntentResponse
            {
                ClientSecret = createIntentResult.ClientSecret!,
                PaymentIntentId = createIntentResult.PaymentIntentId!,
                Amount = createIntentResult.Amount,
                Currency = createIntentResult.Currency!
            });
        }
    }
}
