using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;

namespace Orbito.Application.Features.Payments.Queries.GetPaymentById
{
    public class GetPaymentByIdQueryHandler : IRequestHandler<GetPaymentByIdQuery, Result<PaymentDto>>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ITenantContext _tenantContext;
        private readonly ILogger<GetPaymentByIdQueryHandler> _logger;

        public GetPaymentByIdQueryHandler(
            IPaymentRepository paymentRepository,
            ITenantContext tenantContext,
            ILogger<GetPaymentByIdQueryHandler> logger)
        {
            _paymentRepository = paymentRepository;
            _tenantContext = tenantContext;
            _logger = logger;
        }

        public async Task<Result<PaymentDto>> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Sprawdź czy mamy kontekst tenanta
                if (!_tenantContext.HasTenant)
                {
                    _logger.LogWarning("GetPaymentById attempted without tenant context");
                    return Result.Failure<PaymentDto>(DomainErrors.Tenant.NoTenantContext);
                }

                // Pobierz płatność z weryfikacją ClientId (security best practice)
                var payment = await _paymentRepository.GetByIdForClientAsync(request.PaymentId, request.ClientId, cancellationToken);

                // Bezpieczeństwo: ten sam komunikat dla obu przypadków
                if (payment == null || payment.TenantId != _tenantContext.CurrentTenantId)
                {
                    _logger.LogWarning("Payment {PaymentId} not found for client {ClientId} or cross-tenant access attempt",
                        request.PaymentId, request.ClientId);
                    return Result.Failure<PaymentDto>(DomainErrors.Payment.NotFound);
                }

                var paymentDto = MapToDto(payment);
                return Result.Success(paymentDto);
            }
            catch (Exception ex)
            {
                // Zaloguj szczegóły
                _logger.LogError(ex, "Error retrieving payment {PaymentId}", request.PaymentId);
                // Zwróć ogólny komunikat
                return Result.Failure<PaymentDto>(DomainErrors.General.UnexpectedError);
            }
        }

        private static PaymentDto MapToDto(Domain.Entities.Payment payment)
        {
            return new PaymentDto
            {
                Id = payment.Id,
                TenantId = payment.TenantId.Value,
                SubscriptionId = payment.SubscriptionId,
                ClientId = payment.ClientId,
                Amount = payment.Amount.Amount,
                Currency = payment.Amount.Currency,
                Status = payment.Status.ToString(),
                ExternalTransactionId = payment.ExternalTransactionId,
                PaymentMethod = payment.PaymentMethod,
                ExternalPaymentId = payment.ExternalPaymentId,
                PaymentMethodId = payment.PaymentMethodId,
                CreatedAt = payment.CreatedAt,
                ProcessedAt = payment.ProcessedAt,
                FailedAt = payment.FailedAt,
                RefundedAt = payment.RefundedAt,
                FailureReason = payment.FailureReason
            };
        }
    }
}
