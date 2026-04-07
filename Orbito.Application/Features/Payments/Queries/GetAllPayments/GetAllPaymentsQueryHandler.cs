using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;

namespace Orbito.Application.Features.Payments.Queries.GetAllPayments;

/// <summary>
/// Handler for retrieving all payments for a tenant with filtering and pagination.
/// </summary>
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

            if (!_tenantContext.HasTenant)
            {
                _logger.LogWarning("Missing tenant context for payments query");
                return Result.Failure<GetAllPaymentsResponse>(DomainErrors.Tenant.NoTenantContext);
            }

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

            var payments = await _paymentRepository.GetAllForTenantAsync(
                tenantId,
                request.PageNumber,
                request.PageSize,
                request.SearchTerm,
                request.Status,
                request.ClientId,
                cancellationToken);

            var totalCount = await _paymentRepository.GetCountForTenantAsync(
                tenantId,
                request.SearchTerm,
                request.Status,
                request.ClientId,
                cancellationToken);

            var paymentDtos = PaymentMapper.ToDto(payments).ToList();
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
}
