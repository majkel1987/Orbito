using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Common;
using Orbito.Domain.Entities;
using Orbito.Domain.Errors;

namespace Orbito.Application.Features.Payments.Commands.SavePaymentMethod;

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
            // Check for cancellation before starting
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Saving payment method for client {ClientId}", request.ClientId);

            // Security: Check tenant context
            if (!_tenantContext.HasTenant)
            {
                _logger.LogWarning("No tenant context for saving payment method for client {ClientId}", request.ClientId);
                return Result.Failure<SavePaymentMethodResult>(DomainErrors.Tenant.NoTenantContext);
            }

            // Get the client
            var client = await _unitOfWork.Clients.GetByIdAsync(request.ClientId, cancellationToken);
            if (client == null)
            {
                _logger.LogWarning("Client {ClientId} not found", request.ClientId);
                return Result.Failure<SavePaymentMethodResult>(DomainErrors.Client.NotFound);
            }

            // Security: Verify tenant context
            if (client.TenantId != _tenantContext.CurrentTenantId)
            {
                _logger.LogWarning("Tenant mismatch for client {ClientId}. Expected: {ExpectedTenant}, Actual: {ActualTenant}",
                    request.ClientId, _tenantContext.CurrentTenantId, client.TenantId);
                return Result.Failure<SavePaymentMethodResult>(DomainErrors.Tenant.CrossTenantAccess);
            }

            // Begin transaction to prevent race condition when setting default
            var transactionResult = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            if (!transactionResult.IsSuccess)
            {
                _logger.LogError("Failed to begin transaction for saving payment method: {Error}", transactionResult.ErrorMessage);
                return Result.Failure<SavePaymentMethodResult>(DomainErrors.General.TransactionFailed);
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

                // Create new payment method using factory method
                var paymentMethod = PaymentMethod.Create(
                    client.TenantId,
                    request.ClientId,
                    request.Type,
                    request.Token,
                    request.LastFourDigits,
                    request.ExpiryDate,
                    request.IsDefault);

                // Log metadata (PaymentMethod entity could be extended to persist this)
                if (request.Metadata.Any())
                {
                    _logger.LogDebug("Payment method metadata for client {ClientId}: {Count} entries", request.ClientId, request.Metadata.Count);
                }

                // Save the payment method
                await _unitOfWork.PaymentMethods.AddAsync(paymentMethod, cancellationToken);
                var saveResult = await _unitOfWork.SaveChangesAsync(cancellationToken);

                if (!saveResult.IsSuccess)
                {
                    _logger.LogError("Failed to save payment method: {Error}", saveResult.ErrorMessage);
                    await _unitOfWork.RollbackAsync(cancellationToken);
                    var error = Error.Create("PaymentMethod.SaveFailed", saveResult.ErrorMessage ?? "Failed to save changes");
                    return Result.Failure<SavePaymentMethodResult>(error);
                }

                // Commit transaction
                var commitResult = await _unitOfWork.CommitAsync(cancellationToken);
                if (!commitResult.IsSuccess)
                {
                    _logger.LogError("Failed to commit transaction: {Error}", commitResult.ErrorMessage);
                    var error = Error.Create("PaymentMethod.CommitFailed", commitResult.ErrorMessage ?? "Failed to save payment method");
                    return Result.Failure<SavePaymentMethodResult>(error);
                }

                _logger.LogInformation("Successfully saved payment method {PaymentMethodId} for client {ClientId}",
                    paymentMethod.Id, request.ClientId);

                var result = new SavePaymentMethodResult
                {
                    PaymentMethodId = paymentMethod.Id
                };

                return Result.Success(result);
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
        catch (OperationCanceledException)
        {
            // Rethrow cancellation exceptions - they should not be caught
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving payment method for client {ClientId}", request.ClientId);
            return Result.Failure<SavePaymentMethodResult>(DomainErrors.General.UnexpectedError);
        }
    }
}
