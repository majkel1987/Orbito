using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Common;
using Orbito.Domain.Enums;
using Orbito.Domain.Errors;

namespace Orbito.Application.Features.Payments.Queries.GetPaymentMethodsByClient;

/// <summary>
/// Query for retrieving paginated payment methods for a specific client.
/// </summary>
public record GetPaymentMethodsByClientQuery : IRequest<Result<GetPaymentMethodsByClientResult>>
{
    /// <summary>
    /// Client ID to retrieve payment methods for.
    /// </summary>
    public required Guid ClientId { get; init; }

    /// <summary>
    /// Page number for pagination (1-indexed).
    /// </summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Number of items per page.
    /// </summary>
    public int PageSize { get; init; } = 10;

    /// <summary>
    /// Optional filter by payment method type.
    /// </summary>
    public PaymentMethodType? Type { get; init; }

    /// <summary>
    /// When true, only returns non-expired, usable payment methods.
    /// </summary>
    public bool ActiveOnly { get; init; } = true;
}

/// <summary>
/// Response containing paginated payment methods.
/// </summary>
public record GetPaymentMethodsByClientResult
{
    /// <summary>
    /// List of payment methods for the current page.
    /// </summary>
    public required IEnumerable<PaymentMethodDto> PaymentMethods { get; init; }

    /// <summary>
    /// Total number of payment methods matching the filter criteria.
    /// </summary>
    public required int TotalCount { get; init; }

    /// <summary>
    /// Current page number (1-indexed).
    /// </summary>
    public required int PageNumber { get; init; }

    /// <summary>
    /// Number of items per page.
    /// </summary>
    public required int PageSize { get; init; }

    /// <summary>
    /// Total number of pages available.
    /// </summary>
    public required int TotalPages { get; init; }
}

/// <summary>
/// DTO representing a payment method.
/// </summary>
public record PaymentMethodDto
{
    /// <summary>
    /// Unique identifier for the payment method.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Client ID that owns this payment method.
    /// </summary>
    public required Guid ClientId { get; init; }

    /// <summary>
    /// Type of payment method (Card, BankTransfer, etc.).
    /// </summary>
    public required PaymentMethodType Type { get; init; }

    /// <summary>
    /// Last four digits of the card/account number (masked).
    /// </summary>
    public string? LastFourDigits { get; init; }

    /// <summary>
    /// Expiration date for cards.
    /// </summary>
    public DateTime? ExpiryDate { get; init; }

    /// <summary>
    /// Whether this is the client's default payment method.
    /// </summary>
    public required bool IsDefault { get; init; }

    /// <summary>
    /// Whether the payment method has expired.
    /// </summary>
    public required bool IsExpired { get; init; }

    /// <summary>
    /// Whether the payment method can be used for transactions.
    /// </summary>
    public required bool CanBeUsed { get; init; }

    /// <summary>
    /// When the payment method was created.
    /// </summary>
    public required DateTime CreatedAt { get; init; }
}

/// <summary>
/// Handler for retrieving payment methods by client with tenant verification.
/// </summary>
public class GetPaymentMethodsByClientQueryHandler : IRequestHandler<GetPaymentMethodsByClientQuery, Result<GetPaymentMethodsByClientResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetPaymentMethodsByClientQueryHandler> _logger;
    private readonly ITenantContext _tenantContext;

    public GetPaymentMethodsByClientQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetPaymentMethodsByClientQueryHandler> logger,
        ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    public async Task<Result<GetPaymentMethodsByClientResult>> Handle(GetPaymentMethodsByClientQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Getting payment methods for client {ClientId}", request.ClientId);

            if (!_tenantContext.HasTenant)
            {
                _logger.LogWarning("No tenant context for getting payment methods for client {ClientId}", request.ClientId);
                return Result.Failure<GetPaymentMethodsByClientResult>(DomainErrors.Tenant.NoTenantContext);
            }

            if (request.PageNumber < 1)
            {
                _logger.LogWarning("Invalid page number: {PageNumber}", request.PageNumber);
                return Result.Failure<GetPaymentMethodsByClientResult>(DomainErrors.Validation.InvalidPageNumber);
            }

            if (request.PageSize < 1 || request.PageSize > 100)
            {
                _logger.LogWarning("Invalid page size: {PageSize}", request.PageSize);
                return Result.Failure<GetPaymentMethodsByClientResult>(DomainErrors.Validation.InvalidPageSize);
            }

            var client = await _unitOfWork.Clients.GetByIdAsync(request.ClientId, cancellationToken);
            if (client == null)
            {
                _logger.LogWarning("Client {ClientId} not found", request.ClientId);
                return Result.Failure<GetPaymentMethodsByClientResult>(DomainErrors.Client.NotFound);
            }

            // Security: Verify tenant ownership - same error message to prevent enumeration
            if (client.TenantId != _tenantContext.CurrentTenantId)
            {
                _logger.LogWarning("Tenant mismatch for client {ClientId}. Expected: {ExpectedTenant}, Actual: {ActualTenant}",
                    request.ClientId, _tenantContext.CurrentTenantId, client.TenantId);
                return Result.Failure<GetPaymentMethodsByClientResult>(DomainErrors.Client.NotFound);
            }

            var (paymentMethods, totalCount) = await _unitOfWork.PaymentMethods.GetByClientIdWithCountAsync(
                request.ClientId,
                request.PageNumber,
                request.PageSize,
                request.Type,
                request.ActiveOnly,
                cancellationToken);

            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            var paymentMethodDtos = paymentMethods.Select(pm => new PaymentMethodDto
            {
                Id = pm.Id,
                ClientId = pm.ClientId,
                Type = pm.Type,
                LastFourDigits = pm.LastFourDigits,
                ExpiryDate = pm.ExpiryDate,
                IsDefault = pm.IsDefault,
                IsExpired = pm.IsExpired(),
                CanBeUsed = pm.CanBeUsed(),
                CreatedAt = pm.CreatedAt
            });

            var result = new GetPaymentMethodsByClientResult
            {
                PaymentMethods = paymentMethodDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = totalPages
            };

            _logger.LogDebug("Successfully retrieved {Count} payment methods for client {ClientId}",
                paymentMethods.Count(), request.ClientId);

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment methods for client {ClientId}", request.ClientId);
            return Result.Failure<GetPaymentMethodsByClientResult>(DomainErrors.General.UnexpectedError);
        }
    }
}
