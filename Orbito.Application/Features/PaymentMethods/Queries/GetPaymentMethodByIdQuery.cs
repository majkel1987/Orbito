using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models;

namespace Orbito.Application.Features.PaymentMethods.Queries;

/// <summary>
/// Query for getting a payment method by ID
/// </summary>
public record GetPaymentMethodByIdQuery : IRequest<Result<GetPaymentMethodByIdResult>>
{
    /// <summary>
    /// Payment method ID
    /// </summary>
    public required Guid PaymentMethodId { get; init; }

    /// <summary>
    /// Client ID for security verification
    /// </summary>
    public required Guid ClientId { get; init; }
}

/// <summary>
/// Result for get payment method by ID query
/// </summary>
public record GetPaymentMethodByIdResult
{
    public required Guid Id { get; init; }
    public required Guid ClientId { get; init; }
    public required string Type { get; init; }
    public required string LastFourDigits { get; init; }
    public required string Brand { get; init; }
    public required DateTime ExpiryDate { get; init; }
    public required bool IsDefault { get; init; }
    public required bool IsActive { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

/// <summary>
/// Handler for get payment method by ID query
/// </summary>
public class GetPaymentMethodByIdQueryHandler : IRequestHandler<GetPaymentMethodByIdQuery, Result<GetPaymentMethodByIdResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetPaymentMethodByIdQueryHandler> _logger;
    private readonly ITenantContext _tenantContext;

    public GetPaymentMethodByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetPaymentMethodByIdQueryHandler> logger,
        ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    public async Task<Result<GetPaymentMethodByIdResult>> Handle(GetPaymentMethodByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting payment method {PaymentMethodId} for client {ClientId}",
                request.PaymentMethodId, request.ClientId);

            // SECURITY: Verify tenant context
            if (!_tenantContext.HasTenant)
            {
                _logger.LogWarning("No tenant context for getting payment method");
                return Result<GetPaymentMethodByIdResult>.Failure("Access denied");
            }

            // Get payment method with client verification
            var paymentMethod = await _unitOfWork.PaymentMethods.GetByIdAsync(
                request.PaymentMethodId, request.ClientId, cancellationToken);

            if (paymentMethod == null)
            {
                _logger.LogWarning("Payment method {PaymentMethodId} not found for client {ClientId}",
                    request.PaymentMethodId, request.ClientId);
                return Result<GetPaymentMethodByIdResult>.Failure("Payment method not found");
            }

            // SECURITY: Verify tenant ownership
            if (paymentMethod.TenantId != _tenantContext.CurrentTenantId)
            {
                _logger.LogWarning("Tenant mismatch for payment method {PaymentMethodId}. Expected: {ExpectedTenant}, Actual: {ActualTenant}",
                    request.PaymentMethodId, _tenantContext.CurrentTenantId, paymentMethod.TenantId);
                return Result<GetPaymentMethodByIdResult>.Failure("Access denied");
            }

            var result = new GetPaymentMethodByIdResult
            {
                Id = paymentMethod.Id,
                ClientId = paymentMethod.ClientId,
                Type = paymentMethod.Type.ToString(),
                LastFourDigits = paymentMethod.LastFourDigits ?? "",
                Brand = "", // PaymentMethod doesn't have Brand property
                ExpiryDate = paymentMethod.ExpiryDate ?? DateTime.MinValue,
                IsDefault = paymentMethod.IsDefault,
                IsActive = paymentMethod.CanBeUsed(), // Use CanBeUsed() instead of IsActive
                CreatedAt = paymentMethod.CreatedAt,
                UpdatedAt = paymentMethod.UpdatedAt
            };

            return Result<GetPaymentMethodByIdResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment method {PaymentMethodId} for client {ClientId}",
                request.PaymentMethodId, request.ClientId);
            return Result<GetPaymentMethodByIdResult>.Failure("Error retrieving payment method. Please try again.");
        }
    }
}
