using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Features.Payments.Commands.SavePaymentMethod
{
    /// <summary>
    /// Command for saving payment method
    /// </summary>
    public record SavePaymentMethodCommand : IRequest<Result<SavePaymentMethodResult>>
    {
        /// <summary>
        /// Client ID
        /// </summary>
        public required Guid ClientId { get; init; }

        /// <summary>
        /// Payment method type
        /// </summary>
        public required PaymentMethodType Type { get; init; }

        /// <summary>
        /// Encrypted payment method token
        /// </summary>
        public required string Token { get; init; }

        /// <summary>
        /// Last four digits of the card
        /// </summary>
        public string? LastFourDigits { get; init; }

        /// <summary>
        /// Expiry date of the payment method
        /// </summary>
        public DateTime? ExpiryDate { get; init; }

        /// <summary>
        /// Whether this is the default payment method
        /// </summary>
        public bool IsDefault { get; init; }

        /// <summary>
        /// Additional metadata
        /// </summary>
        public Dictionary<string, string> Metadata { get; init; } = new();
    }

    /// <summary>
    /// Result for saving payment method
    /// </summary>
    public record SavePaymentMethodResult
    {
        /// <summary>
        /// Payment method ID
        /// </summary>
        public required Guid PaymentMethodId { get; init; }

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
    /// Handler for saving payment method
    /// </summary>
    public class SavePaymentMethodCommandHandler : IRequestHandler<SavePaymentMethodCommand, Result<SavePaymentMethodResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SavePaymentMethodCommandHandler> _logger;
        private readonly ITenantContext _tenantContext;

        public SavePaymentMethodCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<SavePaymentMethodCommandHandler> logger,
            ITenantContext tenantContext)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        }

        public async Task<Result<SavePaymentMethodResult>> Handle(SavePaymentMethodCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Saving payment method for client {ClientId}", request.ClientId);

                // Security: Check tenant context
                if (!_tenantContext.HasTenant)
                {
                    _logger.LogWarning("No tenant context for saving payment method for client {ClientId}", request.ClientId);
                    return Result<SavePaymentMethodResult>.Failure("Access denied");
                }

                // Get the client
                var client = await _unitOfWork.Clients.GetByIdAsync(request.ClientId, cancellationToken);
                if (client == null)
                {
                    _logger.LogWarning("Client {ClientId} not found", request.ClientId);
                    return Result<SavePaymentMethodResult>.Failure("Client not found");
                }

                // Security: Verify tenant context
                if (client.TenantId != _tenantContext.CurrentTenantId)
                {
                    _logger.LogWarning("Tenant mismatch for client {ClientId}. Expected: {ExpectedTenant}, Actual: {ActualTenant}",
                        request.ClientId, _tenantContext.CurrentTenantId, client.TenantId);
                    return Result<SavePaymentMethodResult>.Failure("Access denied");
                }

                // Begin transaction to prevent race condition when setting default
                var transactionResult = await _unitOfWork.BeginTransactionAsync(cancellationToken);
                if (!transactionResult.IsSuccess)
                {
                    _logger.LogError("Failed to begin transaction for saving payment method: {Error}", transactionResult.ErrorMessage);
                    return Result<SavePaymentMethodResult>.Failure("Failed to begin transaction");
                }

                try
                {
                    // If this is set as default, remove default from other payment methods
                    if (request.IsDefault)
                    {
                        var existingDefaultMethods = await _unitOfWork.PaymentMethods.GetDefaultPaymentMethodsByClientAsync(request.ClientId, cancellationToken);
                        foreach (var method in existingDefaultMethods)
                        {
                            method.RemoveAsDefault();
                            await _unitOfWork.PaymentMethods.UpdateAsync(method, cancellationToken);
                        }
                    }

                    // Create new payment method
                    var paymentMethod = new PaymentMethod
                    {
                        Id = Guid.NewGuid(),
                        TenantId = client.TenantId,
                        ClientId = request.ClientId,
                        Type = request.Type,
                        Token = request.Token,
                        LastFourDigits = request.LastFourDigits,
                        ExpiryDate = request.ExpiryDate,
                        IsDefault = request.IsDefault
                    };

                    // Add metadata if provided
                    if (request.Metadata.Any())
                    {
                        // You might need to extend PaymentMethod entity to support metadata
                        _logger.LogDebug("Adding metadata to payment method for client {ClientId}", request.ClientId);
                    }

                    // Save the payment method
                    await _unitOfWork.PaymentMethods.AddAsync(paymentMethod, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    // Commit transaction
                    var commitResult = await _unitOfWork.CommitAsync(cancellationToken);
                    if (!commitResult.IsSuccess)
                    {
                        _logger.LogError("Failed to commit transaction: {Error}", commitResult.ErrorMessage);
                        return Result<SavePaymentMethodResult>.Failure("Failed to save payment method");
                    }

                    _logger.LogInformation("Successfully saved payment method {PaymentMethodId} for client {ClientId}",
                        paymentMethod.Id, request.ClientId);

                    var result = new SavePaymentMethodResult
                    {
                        PaymentMethodId = paymentMethod.Id,
                        Success = true
                    };

                    return Result<SavePaymentMethodResult>.Success(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during transaction for saving payment method for client {ClientId}", request.ClientId);
                    var rollbackResult = await _unitOfWork.RollbackAsync(cancellationToken);
                    if (!rollbackResult.IsSuccess)
                    {
                        _logger.LogError("Failed to rollback transaction: {Error}", rollbackResult.ErrorMessage);
                    }
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving payment method for client {ClientId}", request.ClientId);
                return Result<SavePaymentMethodResult>.Failure($"Error saving payment method: {ex.Message}");
            }
        }
    }
}
