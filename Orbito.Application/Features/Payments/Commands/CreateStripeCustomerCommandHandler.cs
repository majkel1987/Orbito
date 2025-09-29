using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Features.Payments.Commands;

namespace Orbito.Application.Features.Payments.Commands
{
    /// <summary>
    /// Handler dla komendy tworzenia klienta Stripe
    /// </summary>
    public class CreateStripeCustomerCommandHandler : IRequestHandler<CreateStripeCustomerCommand, CreateStripeCustomerResult>
    {
        private readonly IPaymentProcessingService _paymentProcessingService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantContext _tenantContext;
        private readonly ILogger<CreateStripeCustomerCommandHandler> _logger;

        public CreateStripeCustomerCommandHandler(
            IPaymentProcessingService paymentProcessingService,
            IUnitOfWork unitOfWork,
            ITenantContext tenantContext,
            ILogger<CreateStripeCustomerCommandHandler> logger)
        {
            _paymentProcessingService = paymentProcessingService;
            _unitOfWork = unitOfWork;
            _tenantContext = tenantContext;
            _logger = logger;
        }

        public async Task<CreateStripeCustomerResult> Handle(CreateStripeCustomerCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Sprawdź czy mamy kontekst tenanta
                if (!_tenantContext.HasTenant)
                {
                    _logger.LogWarning("Attempted to create Stripe customer without tenant context");
                    return CreateStripeCustomerResult.Failure("Tenant context is required", "TENANT_CONTEXT_REQUIRED");
                }

                _logger.LogInformation("Creating Stripe customer for client {ClientId}", request.ClientId);

                // Sprawdź czy klient istnieje
                var client = await _unitOfWork.Clients.GetByIdAsync(request.ClientId, cancellationToken);
                if (client == null)
                {
                    _logger.LogWarning("Client {ClientId} not found", request.ClientId);
                    return CreateStripeCustomerResult.Failure("Client not found", "CLIENT_NOT_FOUND");
                }

                // Sprawdź czy klient należy do tego samego tenanta
                if (client.TenantId != _tenantContext.CurrentTenantId)
                {
                    _logger.LogWarning("Client {ClientId} does not belong to current tenant {TenantId}",
                        request.ClientId, _tenantContext.CurrentTenantId);
                    return CreateStripeCustomerResult.Failure("Access denied", "ACCESS_DENIED");
                }

                // Sprawdź czy klient ma już utworzonego klienta Stripe
                // TODO: Implement proper idempotency check by adding StripeCustomerId field to Client entity
                // Example implementation:
                // if (!string.IsNullOrEmpty(client.StripeCustomerId))
                // {
                //     _logger.LogInformation("Client {ClientId} already has Stripe customer {StripeCustomerId}",
                //         request.ClientId, client.StripeCustomerId);
                //     return CreateStripeCustomerResult.Success(client.StripeCustomerId, request.Email,
                //         request.FirstName, request.LastName);
                // }

                // Utwórz klienta w Stripe
                var result = await _paymentProcessingService.CreateStripeCustomerAsync(
                    request.ClientId,
                    request.Email,
                    request.FirstName,
                    request.LastName,
                    request.CompanyName,
                    request.Phone,
                    cancellationToken);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Stripe customer created for client {ClientId} with ID {StripeCustomerId}", 
                        request.ClientId, result.ExternalCustomerId);

                    return CreateStripeCustomerResult.Success(
                        result.ExternalCustomerId ?? string.Empty,
                        result.Email,
                        result.FirstName,
                        result.LastName);
                }
                else
                {
                    _logger.LogError("Failed to create Stripe customer for client {ClientId}: {ErrorMessage}", 
                        request.ClientId, result.ErrorMessage);

                    return CreateStripeCustomerResult.Failure(
                        result.ErrorMessage ?? "Failed to create Stripe customer",
                        result.ErrorCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Stripe customer for client {ClientId}", request.ClientId);
                return CreateStripeCustomerResult.Failure("An error occurred while creating Stripe customer", "CUSTOMER_CREATION_ERROR");
            }
        }
    }
}
