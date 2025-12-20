using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;

namespace Orbito.Application.Features.Payments.Queries.GetAllPayments
{
    public class GetAllPaymentsQueryHandler : IRequestHandler<GetAllPaymentsQuery, Result<GetAllPaymentsResponse>>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ITenantContext _tenantContext;
        private readonly ILogger<GetAllPaymentsQueryHandler> _logger;

        public GetAllPaymentsQueryHandler(
            IPaymentRepository paymentRepository,
            ITenantContext tenantContext,
            ILogger<GetAllPaymentsQueryHandler> logger)
        {
            _paymentRepository = paymentRepository;
            _tenantContext = tenantContext;
            _logger = logger;
        }

        public async Task<Result<GetAllPaymentsResponse>> Handle(GetAllPaymentsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Getting all payments for tenant, page {PageNumber}, size {PageSize}, search: {SearchTerm}, status: {Status}, clientId: {ClientId}",
                    request.PageNumber, request.PageSize, request.SearchTerm, request.Status, request.ClientId);

                // Validate tenant context
                if (!_tenantContext.HasTenant)
                {
                    _logger.LogWarning("Missing tenant context for payments query");
                    return Result.Failure<GetAllPaymentsResponse>(DomainErrors.Tenant.NoTenantContext);
                }

                // Validate pagination parameters
                if (request.PageNumber < 1)
                {
                    _logger.LogWarning("Invalid page number: {PageNumber}", request.PageNumber);
                    return Result.Failure<GetAllPaymentsResponse>(DomainErrors.Validation.InvalidPageNumber);
                }

                if (request.PageSize < 1 || request.PageSize > 100)
                {
                    _logger.LogWarning("Invalid page size: {PageSize}", request.PageSize);
                    return Result.Failure<GetAllPaymentsResponse>(DomainErrors.Validation.InvalidPageSize);
                }

                var tenantId = _tenantContext.CurrentTenantId;

                // Get payments for tenant with filters
                var payments = await _paymentRepository.GetAllForTenantAsync(
                    tenantId,
                    request.PageNumber,
                    request.PageSize,
                    request.SearchTerm,
                    request.Status,
                    request.ClientId,
                    cancellationToken);

                // Get total count with same filters
                var totalCount = await _paymentRepository.GetCountForTenantAsync(
                    tenantId,
                    request.SearchTerm,
                    request.Status,
                    request.ClientId,
                    cancellationToken);

                var paymentDtos = payments.Select(MapToDto).ToList();
                var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

                _logger.LogDebug("Retrieved {PaymentCount} payments for tenant {TenantId} (total: {TotalCount})",
                    paymentDtos.Count, tenantId, totalCount);

                var response = new GetAllPaymentsResponse
                {
                    Payments = paymentDtos,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalPages = totalPages
                };

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error retrieving payments for tenant {TenantId}",
                    _tenantContext.CurrentTenantId);
                return Result.Failure<GetAllPaymentsResponse>(DomainErrors.General.UnexpectedError);
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
