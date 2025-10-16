using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.DTOs;

namespace Orbito.Application.Features.Payments.Queries.GetPaymentById
{
    public class GetPaymentByIdQueryHandler : IRequestHandler<GetPaymentByIdQuery, GetPaymentByIdResult>
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

        public async Task<GetPaymentByIdResult> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Sprawdź czy mamy kontekst tenanta
                if (!_tenantContext.HasTenant)
                {
                    return GetPaymentByIdResult.FailureResult("Tenant context is required");
                }

                // Pobierz płatność
                // NOTE: Using deprecated method because this query is only accessible by Providers and PlatformAdmins
                // who have proper authorization to view all payments in their tenant
#pragma warning disable CS0618 // Type or member is obsolete
                var payment = await _paymentRepository.GetByIdAsync(request.PaymentId, cancellationToken);
#pragma warning restore CS0618 // Type or member is obsolete

                // Bezpieczeństwo: ten sam komunikat dla obu przypadków
                if (payment == null || payment.TenantId != _tenantContext.CurrentTenantId)
                {
                    return GetPaymentByIdResult.FailureResult("Payment not found");
                }

                var paymentDto = MapToDto(payment);
                return GetPaymentByIdResult.SuccessResult(paymentDto);
            }
            catch (Exception ex)
            {
                // Zaloguj szczegóły
                _logger.LogError(ex, "Error retrieving payment {PaymentId}", request.PaymentId);
                // Zwróć ogólny komunikat
                return GetPaymentByIdResult.FailureResult("An error occurred while retrieving payment");
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
