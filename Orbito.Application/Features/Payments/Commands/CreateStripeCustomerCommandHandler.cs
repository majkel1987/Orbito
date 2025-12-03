using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Features.Payments.Commands;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;

namespace Orbito.Application.Features.Payments.Commands
{
    /// <summary>
    /// Handler dla komendy tworzenia klienta Stripe
    /// </summary>
    public class CreateStripeCustomerCommandHandler : IRequestHandler<CreateStripeCustomerCommand, Result<CreateStripeCustomerResult>>
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
            _paymentProcessingService = paymentProcessingService ?? throw new ArgumentNullException(nameof(paymentProcessingService));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<CreateStripeCustomerResult>> Handle(CreateStripeCustomerCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Check for cancellation before starting
                cancellationToken.ThrowIfCancellationRequested();

                // Sprawdź czy mamy kontekst tenanta
                if (!_tenantContext.HasTenant)
                {
                    _logger.LogWarning("Attempted to create Stripe customer without tenant context");
                    return Result.Failure<CreateStripeCustomerResult>(DomainErrors.Tenant.NoTenantContext);
                }

                _logger.LogInformation("Creating Stripe customer for client {ClientId}", request.ClientId);

                // Sprawdź czy klient istnieje
                var client = await _unitOfWork.Clients.GetByIdAsync(request.ClientId, cancellationToken);
                if (client == null)
                {
                    _logger.LogWarning("Client {ClientId} not found", request.ClientId);
                    return Result.Failure<CreateStripeCustomerResult>(DomainErrors.Client.NotFound);
                }

                // Sprawdź czy klient należy do tego samego tenanta
                if (client.TenantId != _tenantContext.CurrentTenantId)
                {
                    _logger.LogWarning("Client {ClientId} does not belong to current tenant {TenantId}",
                        request.ClientId, _tenantContext.CurrentTenantId);
                    return Result.Failure<CreateStripeCustomerResult>(DomainErrors.Tenant.CrossTenantAccess);
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

                // Utwórz klienta w payment gateway
                var result = await _paymentProcessingService.CreateCustomerAsync(
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

                    return Result.Success(CreateStripeCustomerResult.Success(
                        result.ExternalCustomerId ?? string.Empty,
                        result.Email,
                        result.FirstName,
                        result.LastName));
                }
                else
                {
                    _logger.LogError("Failed to create Stripe customer for client {ClientId}: {ErrorMessage}", 
                        request.ClientId, result.ErrorMessage);

                    return Result.Failure<CreateStripeCustomerResult>(DomainErrors.General.UnexpectedError);
                }
            }
            catch (OperationCanceledException)
            {
                // Rethrow cancellation exceptions - they should not be caught
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Stripe customer for client {ClientId}", request.ClientId);
                return Result.Failure<CreateStripeCustomerResult>(DomainErrors.General.UnexpectedError);
            }
        }
    }
}
