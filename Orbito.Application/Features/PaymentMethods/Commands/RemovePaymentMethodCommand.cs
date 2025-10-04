using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models;
using Orbito.Domain.Events;

namespace Orbito.Application.Features.PaymentMethods.Commands;

/// <summary>
/// Command for removing a payment method
/// </summary>
public record RemovePaymentMethodCommand : IRequest<Result<RemovePaymentMethodResult>>
{
    /// <summary>
    /// Payment method ID to remove
    /// </summary>
    public required Guid PaymentMethodId { get; init; }

    /// <summary>
    /// Client ID for security verification
    /// </summary>
    public required Guid ClientId { get; init; }
}

/// <summary>
/// Result for remove payment method command
/// </summary>
public record RemovePaymentMethodResult
{
    public required Guid PaymentMethodId { get; init; }
    public required Guid ClientId { get; init; }
    public required bool WasDefault { get; init; }
    public Guid? NewDefaultPaymentMethodId { get; init; }
}

/// <summary>
/// Validator for remove payment method command
/// </summary>
public class RemovePaymentMethodCommandValidator : AbstractValidator<RemovePaymentMethodCommand>
{
    public RemovePaymentMethodCommandValidator()
    {
        RuleFor(x => x.PaymentMethodId)
            .NotEmpty()
            .WithMessage("Payment method ID is required");

        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("Client ID is required");
    }
}

/// <summary>
/// Handler for remove payment method command
/// </summary>
public class RemovePaymentMethodCommandHandler : IRequestHandler<RemovePaymentMethodCommand, Result<RemovePaymentMethodResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RemovePaymentMethodCommandHandler> _logger;
    private readonly ITenantContext _tenantContext;
    private readonly IPaymentNotificationService _notificationService;

    public RemovePaymentMethodCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<RemovePaymentMethodCommandHandler> logger,
        ITenantContext tenantContext,
        IPaymentNotificationService notificationService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    }

    public async Task<Result<RemovePaymentMethodResult>> Handle(RemovePaymentMethodCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Removing payment method {PaymentMethodId} for client {ClientId}",
                request.PaymentMethodId, request.ClientId);

            // SECURITY: Verify tenant context
            if (!_tenantContext.HasTenant)
            {
                _logger.LogWarning("No tenant context for removing payment method");
                return Result<RemovePaymentMethodResult>.Failure("Access denied");
            }

            // SECURITY: Get payment method with client verification
            var paymentMethod = await _unitOfWork.PaymentMethods.GetByIdAsync(
                request.PaymentMethodId, request.ClientId, cancellationToken);

            if (paymentMethod == null)
            {
                _logger.LogWarning("Payment method {PaymentMethodId} not found for client {ClientId}",
                    request.PaymentMethodId, request.ClientId);
                return Result<RemovePaymentMethodResult>.Failure("Payment method not found");
            }

            // SECURITY: Verify tenant ownership
            if (paymentMethod.TenantId != _tenantContext.CurrentTenantId)
            {
                _logger.LogWarning("Tenant mismatch for payment method {PaymentMethodId}. Expected: {ExpectedTenant}, Actual: {ActualTenant}",
                    request.PaymentMethodId, _tenantContext.CurrentTenantId, paymentMethod.TenantId);
                return Result<RemovePaymentMethodResult>.Failure("Access denied");
            }

            // Check if there are active subscriptions
            var activeSubscriptions = await _unitOfWork.Subscriptions.GetActiveSubscriptionsByClientAsync(
                request.ClientId, cancellationToken);

            var wasDefault = paymentMethod.IsDefault;
            var lastFourDigits = paymentMethod.LastFourDigits ?? "****";

            // Get all payment methods BEFORE deletion to check count
            var allPaymentMethodsBeforeDeletion = await _unitOfWork.PaymentMethods.GetByClientIdAsync(
                request.ClientId, pageNumber: 1, pageSize: 100, activeOnly: true, cancellationToken: cancellationToken);

            // Check if this is the only payment method and there are active subscriptions
            if (allPaymentMethodsBeforeDeletion.Count() == 1 && activeSubscriptions.Any())
            {
                _logger.LogWarning("Cannot remove last payment method for client {ClientId} with active subscriptions",
                    request.ClientId);
                return Result<RemovePaymentMethodResult>.Failure(
                    "Cannot remove the last payment method while you have active subscriptions");
            }

            // Remove the payment method
            await _unitOfWork.PaymentMethods.DeleteAsync(paymentMethod, cancellationToken);

            Guid? newDefaultId = null;

            // If this was the default payment method, set another one as default
            if (wasDefault && allPaymentMethodsBeforeDeletion.Count() > 1)
            {
                // Filter out the removed payment method
                var nextPaymentMethod = allPaymentMethodsBeforeDeletion
                    .Where(pm => pm.Id != request.PaymentMethodId && pm.CanBeUsed())
                    .OrderByDescending(pm => pm.CreatedAt)
                    .FirstOrDefault();

                if (nextPaymentMethod != null)
                {
                    nextPaymentMethod.SetAsDefault();
                    await _unitOfWork.PaymentMethods.UpdateAsync(nextPaymentMethod, cancellationToken);
                    newDefaultId = nextPaymentMethod.Id;
                }
            }

            await _unitOfWork.PaymentMethods.SaveChangesAsync(cancellationToken);

            // Send notification (fire and forget - don't wait)
            _ = Task.Run(async () =>
            {
                if (cancellationToken.IsCancellationRequested) return;

                try
                {
                    await _notificationService.SendPaymentMethodRemovedNotificationAsync(
                        request.ClientId, lastFourDigits, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending payment method removed notification");
                }
            }, CancellationToken.None);

            _logger.LogInformation("Payment method {PaymentMethodId} removed successfully for client {ClientId}",
                request.PaymentMethodId, request.ClientId);

            var result = new RemovePaymentMethodResult
            {
                PaymentMethodId = request.PaymentMethodId,
                ClientId = request.ClientId,
                WasDefault = wasDefault,
                NewDefaultPaymentMethodId = newDefaultId
            };

            return Result<RemovePaymentMethodResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing payment method {PaymentMethodId} for client {ClientId}",
                request.PaymentMethodId, request.ClientId);
            return Result<RemovePaymentMethodResult>.Failure("Error removing payment method. Please try again.");
        }
    }
}
