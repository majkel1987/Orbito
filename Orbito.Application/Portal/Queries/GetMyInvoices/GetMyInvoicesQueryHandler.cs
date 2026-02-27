using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;

namespace Orbito.Application.Portal.Queries.GetMyInvoices
{
    public class GetMyInvoicesQueryHandler : IRequestHandler<GetMyInvoicesQuery, Result<List<PaymentDto>>>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IUserContextService _userContextService;
        private readonly ILogger<GetMyInvoicesQueryHandler> _logger;

        public GetMyInvoicesQueryHandler(
            IPaymentRepository paymentRepository,
            IUserContextService userContextService,
            ILogger<GetMyInvoicesQueryHandler> logger)
        {
            _paymentRepository = paymentRepository;
            _userContextService = userContextService;
            _logger = logger;
        }

        public async Task<Result<List<PaymentDto>>> Handle(
            GetMyInvoicesQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting invoices for current client via Portal");

            var clientId = await _userContextService.GetCurrentClientIdAsync(cancellationToken);
            if (clientId == null)
            {
                _logger.LogWarning("Portal: cannot resolve client ID for authenticated user");
                return Result.Failure<List<PaymentDto>>(DomainErrors.Client.NotFound);
            }

            var payments = await _paymentRepository.GetByClientIdAsync(
                clientId.Value,
                request.PageNumber,
                request.PageSize,
                cancellationToken);

            var dtos = payments.Select(MapToDto).ToList();

            _logger.LogInformation("Portal: returned {Count} invoices for client {ClientId}", dtos.Count, clientId);

            return Result.Success(dtos);
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
