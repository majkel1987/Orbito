using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.Events;

namespace Orbito.Application.Features.PaymentMethods.Commands;

/// <summary>
/// Command for adding a new payment method
/// </summary>
public record AddPaymentMethodCommand : IRequest<Result<AddPaymentMethodResult>>
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
    /// Payment method token from payment gateway
    /// </summary>
    public required string Token { get; init; }

    /// <summary>
    /// Last four digits of card
    /// </summary>
    public string? LastFourDigits { get; init; }

    /// <summary>
    /// Card expiry month (1-12)
    /// </summary>
    public int? ExpiryMonth { get; init; }

    /// <summary>
    /// Card expiry year (4 digits)
    /// </summary>
    public int? ExpiryYear { get; init; }

    /// <summary>
    /// Set as default payment method
    /// </summary>
    public bool SetAsDefault { get; init; }
}

/// <summary>
/// Result for add payment method command
/// </summary>
public record AddPaymentMethodResult
{
    public required Guid PaymentMethodId { get; init; }
    public required Guid ClientId { get; init; }
    public required PaymentMethodType Type { get; init; }
    public required bool IsDefault { get; init; }
    public required DateTime CreatedAt { get; init; }
}

/// <summary>
/// Validator for add payment method command
/// </summary>
public class AddPaymentMethodCommandValidator : AbstractValidator<AddPaymentMethodCommand>
{
    public AddPaymentMethodCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("Client ID is required");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Invalid payment method type");

        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage("Payment method token is required")
            .MaximumLength(500)
            .WithMessage("Token cannot exceed 500 characters");

        RuleFor(x => x.LastFourDigits)
            .Length(4)
            .When(x => !string.IsNullOrEmpty(x.LastFourDigits))
            .WithMessage("Last four digits must be exactly 4 characters")
            .Matches(@"^\d{4}$")
            .When(x => !string.IsNullOrEmpty(x.LastFourDigits))
            .WithMessage("Last four digits must contain only numbers");

        RuleFor(x => x.ExpiryMonth)
            .InclusiveBetween(1, 12)
            .When(x => x.ExpiryMonth.HasValue)
            .WithMessage("Expiry month must be between 1 and 12");

        RuleFor(x => x.ExpiryYear)
            .Must(year => year >= DateTime.UtcNow.Year)
            .When(x => x.ExpiryYear.HasValue)
            .WithMessage(_ => $"Expiry year must be {DateTime.UtcNow.Year} or later")
            .Must(year => year <= DateTime.UtcNow.Year + 20)
            .When(x => x.ExpiryYear.HasValue)
            .WithMessage(_ => $"Expiry year cannot be more than 20 years in the future");

        // Both expiry month and year must be provided together
        RuleFor(x => x.ExpiryMonth)
            .NotNull()
            .When(x => x.ExpiryYear.HasValue)
            .WithMessage("Expiry month is required when expiry year is provided");

        RuleFor(x => x.ExpiryYear)
            .NotNull()
            .When(x => x.ExpiryMonth.HasValue)
            .WithMessage("Expiry year is required when expiry month is provided");
    }
}

/// <summary>
/// Handler for add payment method command
/// </summary>
public class AddPaymentMethodCommandHandler : IRequestHandler<AddPaymentMethodCommand, Result<AddPaymentMethodResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddPaymentMethodCommandHandler> _logger;
    private readonly ITenantContext _tenantContext;
    private readonly ISecurityLimitService _securityLimitService;
    private readonly IPaymentNotificationService _notificationService;

    public AddPaymentMethodCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<AddPaymentMethodCommandHandler> logger,
        ITenantContext tenantContext,
        ISecurityLimitService securityLimitService,
        IPaymentNotificationService notificationService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _securityLimitService = securityLimitService ?? throw new ArgumentNullException(nameof(securityLimitService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    }

    public async Task<Result<AddPaymentMethodResult>> Handle(AddPaymentMethodCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Adding payment method for client {ClientId}", request.ClientId);

            // SECURITY: Verify tenant context
            if (!_tenantContext.HasTenant)
            {
                _logger.LogWarning("No tenant context for adding payment method for client {ClientId}", request.ClientId);
                return Result<AddPaymentMethodResult>.Failure("Access denied");
            }

            // Verify client exists and belongs to tenant
            var client = await _unitOfWork.Clients.GetByIdAsync(request.ClientId, cancellationToken);
            if (client == null)
            {
                _logger.LogWarning("Client {ClientId} not found", request.ClientId);
                return Result<AddPaymentMethodResult>.Failure("Client not found");
            }

            // SECURITY: Verify tenant ownership
            if (client.TenantId != _tenantContext.CurrentTenantId)
            {
                _logger.LogWarning("Tenant mismatch for client {ClientId}. Expected: {ExpectedTenant}, Actual: {ActualTenant}",
                    request.ClientId, _tenantContext.CurrentTenantId, client.TenantId);
                return Result<AddPaymentMethodResult>.Failure("Access denied");
            }

            // SECURITY: Check payment method limit
            // NOTE: Edge case - Between this check and final save, another concurrent request could add a payment method.
            // For 100% guarantee, consider using: (a) pessimistic locking, (b) unique constraint in DB, or (c) serializable transaction isolation.
            // Current approach is acceptable for most use cases given the low probability and non-critical nature of exceeding limit by 1.
            var currentCount = await _unitOfWork.PaymentMethods.GetActiveCountByClientIdAsync(request.ClientId, cancellationToken);
            if (!_securityLimitService.CanAddPaymentMethod(request.ClientId, currentCount))
            {
                _logger.LogWarning("Client {ClientId} has reached maximum payment methods limit ({Limit})",
                    request.ClientId, _securityLimitService.MaxPaymentMethodsPerClient);
                return Result<AddPaymentMethodResult>.Failure(
                    $"Maximum payment methods limit reached ({_securityLimitService.MaxPaymentMethodsPerClient})");
            }

            // Calculate expiry date if month and year are provided
            DateTime? expiryDate = null;
            if (request.ExpiryMonth.HasValue && request.ExpiryYear.HasValue)
            {
                expiryDate = new DateTime(
                    request.ExpiryYear.Value,
                    request.ExpiryMonth.Value,
                    DateTime.DaysInMonth(request.ExpiryYear.Value, request.ExpiryMonth.Value)
                );
            }

            // Create payment method
            var paymentMethod = PaymentMethod.Create(
                tenantId: _tenantContext.CurrentTenantId!,
                clientId: request.ClientId,
                type: request.Type,
                token: request.Token,
                lastFourDigits: request.LastFourDigits,
                expiryDate: expiryDate,
                isDefault: false // We'll set default separately if requested
            );

            // If this should be the default, set it
            if (request.SetAsDefault)
            {
                // Remove default from all other payment methods
                var existingDefaults = await _unitOfWork.PaymentMethods.GetDefaultPaymentMethodsByClientAsync(
                    request.ClientId, cancellationToken);

                foreach (var existingDefault in existingDefaults)
                {
                    existingDefault.RemoveAsDefault();
                    await _unitOfWork.PaymentMethods.UpdateAsync(existingDefault, cancellationToken);
                }

                // Set new one as default
                paymentMethod.SetAsDefault();
            }
            else if (currentCount == 0)
            {
                // If this is the first payment method, make it default (using count from earlier check)
                paymentMethod.SetAsDefault();
            }

            // Add payment method (after setting default flag to avoid race condition)
            await _unitOfWork.PaymentMethods.AddAsync(paymentMethod, cancellationToken);
            await _unitOfWork.PaymentMethods.SaveChangesAsync(cancellationToken);

            // Send notification (fire and forget - don't wait)
            _ = Task.Run(async () =>
            {
                if (cancellationToken.IsCancellationRequested) return;

                try
                {
                    await _notificationService.SendPaymentMethodAddedNotificationAsync(paymentMethod.Id, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending payment method added notification");
                }
            }, CancellationToken.None);

            _logger.LogInformation("Payment method {PaymentMethodId} added successfully for client {ClientId}",
                paymentMethod.Id, request.ClientId);

            var result = new AddPaymentMethodResult
            {
                PaymentMethodId = paymentMethod.Id,
                ClientId = paymentMethod.ClientId,
                Type = paymentMethod.Type,
                IsDefault = paymentMethod.IsDefault,
                CreatedAt = paymentMethod.CreatedAt
            };

            return Result<AddPaymentMethodResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding payment method for client {ClientId}", request.ClientId);
            return Result<AddPaymentMethodResult>.Failure("Error adding payment method. Please try again.");
        }
    }
}
