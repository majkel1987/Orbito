using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models;
using Orbito.Domain.Enums;

namespace Orbito.Application.Features.PaymentMethods.Queries;

/// <summary>
/// Query for getting default payment method for a client
/// </summary>
public record GetDefaultPaymentMethodQuery : IRequest<Result<GetDefaultPaymentMethodResult>>
{
    /// <summary>
    /// Client ID
    /// </summary>
    public required Guid ClientId { get; init; }
}

/// <summary>
/// Result for get default payment method query
/// </summary>
public record GetDefaultPaymentMethodResult
{
    public Guid? PaymentMethodId { get; init; }
    public Guid ClientId { get; init; }
    public PaymentMethodType? Type { get; init; }
    public string? LastFourDigits { get; init; }
    public DateTime? ExpiryDate { get; init; }
    public bool? IsExpired { get; init; }
    public bool? CanBeUsed { get; init; }
    public DateTime? CreatedAt { get; init; }
    public bool HasDefault { get; init; }
}

/// <summary>
/// Validator for get default payment method query
/// </summary>
public class GetDefaultPaymentMethodQueryValidator : AbstractValidator<GetDefaultPaymentMethodQuery>
{
    public GetDefaultPaymentMethodQueryValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("Client ID is required");
    }
}

/// <summary>
/// Handler for get default payment method query
/// </summary>
public class GetDefaultPaymentMethodQueryHandler : IRequestHandler<GetDefaultPaymentMethodQuery, Result<GetDefaultPaymentMethodResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetDefaultPaymentMethodQueryHandler> _logger;
    private readonly ITenantContext _tenantContext;

    public GetDefaultPaymentMethodQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetDefaultPaymentMethodQueryHandler> logger,
        ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    public async Task<Result<GetDefaultPaymentMethodResult>> Handle(GetDefaultPaymentMethodQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting default payment method for client {ClientId}", request.ClientId);

            // SECURITY: Verify tenant context
            if (!_tenantContext.HasTenant)
            {
                _logger.LogWarning("No tenant context for getting default payment method");
                return Result<GetDefaultPaymentMethodResult>.Failure("Access denied");
            }

            // Verify client exists and belongs to tenant
            var client = await _unitOfWork.Clients.GetByIdAsync(request.ClientId, cancellationToken);
            if (client == null)
            {
                _logger.LogWarning("Client {ClientId} not found", request.ClientId);
                return Result<GetDefaultPaymentMethodResult>.Failure("Client not found");
            }

            // SECURITY: Verify tenant ownership
            if (client.TenantId != _tenantContext.CurrentTenantId)
            {
                _logger.LogWarning("Tenant mismatch for client {ClientId}. Expected: {ExpectedTenant}, Actual: {ActualTenant}",
                    request.ClientId, _tenantContext.CurrentTenantId, client.TenantId);
                return Result<GetDefaultPaymentMethodResult>.Failure("Access denied");
            }

            // Get default payment methods
            var defaultPaymentMethods = await _unitOfWork.PaymentMethods.GetDefaultPaymentMethodsByClientAsync(
                request.ClientId, cancellationToken);

            var defaultPaymentMethod = defaultPaymentMethods.FirstOrDefault();

            // DATA INTEGRITY: Log warning if multiple default payment methods exist
            // This should never happen if business invariants are properly enforced
            if (defaultPaymentMethods.Count() > 1)
            {
                _logger.LogWarning(
                    "Data integrity issue: Client {ClientId} has {Count} default payment methods. Expected 1.",
                    request.ClientId,
                    defaultPaymentMethods.Count());
            }

            if (defaultPaymentMethod == null)
            {
                _logger.LogInformation("No default payment method found for client {ClientId}", request.ClientId);

                var noDefaultResult = new GetDefaultPaymentMethodResult
                {
                    PaymentMethodId = null,
                    ClientId = request.ClientId,
                    HasDefault = false
                };

                return Result<GetDefaultPaymentMethodResult>.Success(noDefaultResult);
            }

            _logger.LogInformation("Default payment method {PaymentMethodId} found for client {ClientId}",
                defaultPaymentMethod.Id, request.ClientId);

            var result = new GetDefaultPaymentMethodResult
            {
                PaymentMethodId = defaultPaymentMethod.Id,
                ClientId = defaultPaymentMethod.ClientId,
                Type = defaultPaymentMethod.Type,
                LastFourDigits = defaultPaymentMethod.LastFourDigits,
                ExpiryDate = defaultPaymentMethod.ExpiryDate,
                IsExpired = defaultPaymentMethod.IsExpired(),
                CanBeUsed = defaultPaymentMethod.CanBeUsed(),
                CreatedAt = defaultPaymentMethod.CreatedAt,
                HasDefault = true
            };

            return Result<GetDefaultPaymentMethodResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting default payment method for client {ClientId}", request.ClientId);
            return Result<GetDefaultPaymentMethodResult>.Failure("Error getting default payment method. Please try again.");
        }
    }
}
