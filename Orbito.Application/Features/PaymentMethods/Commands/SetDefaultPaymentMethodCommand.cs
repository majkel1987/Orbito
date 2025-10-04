using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models;
using Orbito.Domain.Events;

namespace Orbito.Application.Features.PaymentMethods.Commands;

/// <summary>
/// Command for setting a payment method as default
/// </summary>
public record SetDefaultPaymentMethodCommand : IRequest<Result<SetDefaultPaymentMethodResult>>
{
    /// <summary>
    /// Payment method ID to set as default
    /// </summary>
    public required Guid PaymentMethodId { get; init; }

    /// <summary>
    /// Client ID for security verification
    /// </summary>
    public required Guid ClientId { get; init; }
}

/// <summary>
/// Result for set default payment method command
/// </summary>
public record SetDefaultPaymentMethodResult
{
    public required Guid PaymentMethodId { get; init; }
    public required Guid ClientId { get; init; }
    public required bool IsDefault { get; init; }
    public Guid? PreviousDefaultPaymentMethodId { get; init; }
}

/// <summary>
/// Validator for set default payment method command
/// </summary>
public class SetDefaultPaymentMethodCommandValidator : AbstractValidator<SetDefaultPaymentMethodCommand>
{
    public SetDefaultPaymentMethodCommandValidator()
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
/// Handler for set default payment method command
/// </summary>
public class SetDefaultPaymentMethodCommandHandler : IRequestHandler<SetDefaultPaymentMethodCommand, Result<SetDefaultPaymentMethodResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SetDefaultPaymentMethodCommandHandler> _logger;
    private readonly ITenantContext _tenantContext;

    public SetDefaultPaymentMethodCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<SetDefaultPaymentMethodCommandHandler> logger,
        ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    public async Task<Result<SetDefaultPaymentMethodResult>> Handle(SetDefaultPaymentMethodCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Setting payment method {PaymentMethodId} as default for client {ClientId}",
                request.PaymentMethodId, request.ClientId);

            // SECURITY: Verify tenant context
            if (!_tenantContext.HasTenant)
            {
                _logger.LogWarning("No tenant context for setting default payment method");
                return Result<SetDefaultPaymentMethodResult>.Failure("Access denied");
            }

            // SECURITY: Get payment method with client verification
            var paymentMethod = await _unitOfWork.PaymentMethods.GetByIdAsync(
                request.PaymentMethodId, request.ClientId, cancellationToken);

            if (paymentMethod == null)
            {
                _logger.LogWarning("Payment method {PaymentMethodId} not found for client {ClientId}",
                    request.PaymentMethodId, request.ClientId);
                return Result<SetDefaultPaymentMethodResult>.Failure("Payment method not found");
            }

            // SECURITY: Verify tenant ownership
            if (paymentMethod.TenantId != _tenantContext.CurrentTenantId)
            {
                _logger.LogWarning("Tenant mismatch for payment method {PaymentMethodId}. Expected: {ExpectedTenant}, Actual: {ActualTenant}",
                    request.PaymentMethodId, _tenantContext.CurrentTenantId, paymentMethod.TenantId);
                return Result<SetDefaultPaymentMethodResult>.Failure("Access denied");
            }

            // Check if payment method can be used
            if (!paymentMethod.CanBeUsed())
            {
                _logger.LogWarning("Payment method {PaymentMethodId} cannot be used (expired or invalid)",
                    request.PaymentMethodId);
                return Result<SetDefaultPaymentMethodResult>.Failure("Payment method cannot be used");
            }

            // If already default, nothing to do
            if (paymentMethod.IsDefault)
            {
                _logger.LogInformation("Payment method {PaymentMethodId} is already default", request.PaymentMethodId);

                var existingResult = new SetDefaultPaymentMethodResult
                {
                    PaymentMethodId = paymentMethod.Id,
                    ClientId = paymentMethod.ClientId,
                    IsDefault = true,
                    PreviousDefaultPaymentMethodId = null
                };

                return Result<SetDefaultPaymentMethodResult>.Success(existingResult);
            }

            // Get current default payment method
            var currentDefaults = await _unitOfWork.PaymentMethods.GetDefaultPaymentMethodsByClientAsync(
                request.ClientId, cancellationToken);

            Guid? previousDefaultId = null;

            // Remove default from all other payment methods
            foreach (var currentDefault in currentDefaults)
            {
                if (currentDefault.Id != request.PaymentMethodId)
                {
                    previousDefaultId = currentDefault.Id;
                    currentDefault.RemoveAsDefault();
                    await _unitOfWork.PaymentMethods.UpdateAsync(currentDefault, cancellationToken);
                }
            }

            // Set new payment method as default
            paymentMethod.SetAsDefault();
            await _unitOfWork.PaymentMethods.UpdateAsync(paymentMethod, cancellationToken);
            await _unitOfWork.PaymentMethods.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Payment method {PaymentMethodId} set as default for client {ClientId}",
                request.PaymentMethodId, request.ClientId);

            var result = new SetDefaultPaymentMethodResult
            {
                PaymentMethodId = paymentMethod.Id,
                ClientId = paymentMethod.ClientId,
                IsDefault = paymentMethod.IsDefault,
                PreviousDefaultPaymentMethodId = previousDefaultId
            };

            return Result<SetDefaultPaymentMethodResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting default payment method {PaymentMethodId} for client {ClientId}",
                request.PaymentMethodId, request.ClientId);
            return Result<SetDefaultPaymentMethodResult>.Failure("Error setting default payment method. Please try again.");
        }
    }
}
