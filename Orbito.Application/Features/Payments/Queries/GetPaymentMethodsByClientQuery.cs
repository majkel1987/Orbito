using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;

namespace Orbito.Application.Features.Payments.Queries.GetPaymentMethodsByClient
{
    /// <summary>
    /// Query for getting payment methods by client
    /// </summary>
    public record GetPaymentMethodsByClientQuery : IRequest<Result<GetPaymentMethodsByClientResult>>
    {
        /// <summary>
        /// Client ID
        /// </summary>
        public required Guid ClientId { get; init; }

        /// <summary>
        /// Page number
        /// </summary>
        public int PageNumber { get; init; } = 1;

        /// <summary>
        /// Page size
        /// </summary>
        public int PageSize { get; init; } = 10;

        /// <summary>
        /// Filter by payment method type
        /// </summary>
        public PaymentMethodType? Type { get; init; }

        /// <summary>
        /// Include only active payment methods
        /// </summary>
        public bool ActiveOnly { get; init; } = true;
    }

    /// <summary>
    /// Result for getting payment methods by client
    /// </summary>
    public record GetPaymentMethodsByClientResult
    {
        /// <summary>
        /// List of payment methods
        /// </summary>
        public required IEnumerable<PaymentMethodDto> PaymentMethods { get; init; }

        /// <summary>
        /// Total count of payment methods
        /// </summary>
        public required int TotalCount { get; init; }

        /// <summary>
        /// Page number
        /// </summary>
        public required int PageNumber { get; init; }

        /// <summary>
        /// Page size
        /// </summary>
        public required int PageSize { get; init; }

        /// <summary>
        /// Total pages
        /// </summary>
        public required int TotalPages { get; init; }

        /// <summary>
        /// Whether the operation was successful
        /// </summary>
        public required bool Success { get; init; }

        /// <summary>
        /// Error message if operation failed
        /// </summary>
        public string? ErrorMessage { get; init; }
    }

    /// <summary>
    /// Payment method DTO
    /// </summary>
    public record PaymentMethodDto
    {
        /// <summary>
        /// Payment method ID
        /// </summary>
        public required Guid Id { get; init; }

        /// <summary>
        /// Client ID
        /// </summary>
        public required Guid ClientId { get; init; }

        /// <summary>
        /// Payment method type
        /// </summary>
        public required PaymentMethodType Type { get; init; }

        /// <summary>
        /// Last four digits
        /// </summary>
        public string? LastFourDigits { get; init; }

        /// <summary>
        /// Expiry date
        /// </summary>
        public DateTime? ExpiryDate { get; init; }

        /// <summary>
        /// Whether this is the default payment method
        /// </summary>
        public required bool IsDefault { get; init; }

        /// <summary>
        /// Whether the payment method is expired
        /// </summary>
        public required bool IsExpired { get; init; }

        /// <summary>
        /// Whether the payment method can be used
        /// </summary>
        public required bool CanBeUsed { get; init; }

        /// <summary>
        /// Created date
        /// </summary>
        public required DateTime CreatedAt { get; init; }
    }

    /// <summary>
    /// Handler for getting payment methods by client
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
                _logger.LogInformation("Getting payment methods for client {ClientId}", request.ClientId);

                // Security: Check tenant context
                if (!_tenantContext.HasTenant)
                {
                    _logger.LogWarning("No tenant context for getting payment methods for client {ClientId}", request.ClientId);
                    return Result<GetPaymentMethodsByClientResult>.Failure("Access denied");
                }

                // Get the client
                var client = await _unitOfWork.Clients.GetByIdAsync(request.ClientId, cancellationToken);
                if (client == null)
                {
                    _logger.LogWarning("Client {ClientId} not found", request.ClientId);
                    return Result<GetPaymentMethodsByClientResult>.Failure("Client not found");
                }

                // Security: Verify tenant context
                if (client.TenantId != _tenantContext.CurrentTenantId)
                {
                    _logger.LogWarning("Tenant mismatch for client {ClientId}. Expected: {ExpectedTenant}, Actual: {ActualTenant}",
                        request.ClientId, _tenantContext.CurrentTenantId, client.TenantId);
                    return Result<GetPaymentMethodsByClientResult>.Failure("Access denied");
                }

                // Get payment methods with count in a single optimized query
                var (paymentMethods, totalCount) = await _unitOfWork.PaymentMethods.GetByClientIdWithCountAsync(
                    request.ClientId,
                    request.PageNumber,
                    request.PageSize,
                    request.Type,
                    request.ActiveOnly,
                    cancellationToken);

                var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

                // Map to DTOs
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
                    TotalPages = totalPages,
                    Success = true
                };

                _logger.LogInformation("Successfully retrieved {Count} payment methods for client {ClientId}", 
                    paymentMethods.Count(), request.ClientId);

                return Result<GetPaymentMethodsByClientResult>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment methods for client {ClientId}", request.ClientId);
                return Result<GetPaymentMethodsByClientResult>.Failure($"Error getting payment methods: {ex.Message}");
            }
        }
    }
}
