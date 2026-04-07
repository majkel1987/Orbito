using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;

namespace Orbito.Application.Features.Payments.Queries.GetPaymentById;

/// <summary>
/// Handler for retrieving a single payment by ID with client ownership verification.
/// </summary>
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
            if (!_tenantContext.HasTenant)
            {
                _logger.LogWarning("GetPaymentById attempted without tenant context");
                return Result.Failure<PaymentDto>(DomainErrors.Tenant.NoTenantContext);
            }

            var payment = await _paymentRepository.GetByIdForClientAsync(request.PaymentId, request.ClientId, cancellationToken);

            // Security: same message for both cases to prevent information leakage
            if (payment == null || payment.TenantId != _tenantContext.CurrentTenantId)
            {
                _logger.LogWarning("Payment {PaymentId} not found for client {ClientId} or cross-tenant access attempt",
                    request.PaymentId, request.ClientId);
                return Result.Failure<PaymentDto>(DomainErrors.Payment.NotFound);
            }

            return Result.Success(PaymentMapper.ToDto(payment));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment {PaymentId}", request.PaymentId);
            return Result.Failure<PaymentDto>(DomainErrors.General.UnexpectedError);
        }
    }
}
