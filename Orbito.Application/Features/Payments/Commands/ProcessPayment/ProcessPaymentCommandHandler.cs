using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;
using Orbito.Domain.Constants;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.Errors;
using Orbito.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Diagnostics;

namespace Orbito.Application.Features.Payments.Commands.ProcessPayment;

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, Result<PaymentDto>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IClientRepository _clientRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProcessPaymentCommandHandler> _logger;

    private static readonly HashSet<string> ValidCurrencies = new()
    {
        "PLN", "EUR", "USD", "GBP", "CHF", "CZK", "SEK", "NOK", "DKK", "JPY", "CAD", "AUD"
    };

    public ProcessPaymentCommandHandler(
        IPaymentRepository paymentRepository,
        ISubscriptionRepository subscriptionRepository,
        IClientRepository clientRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork,
        ILogger<ProcessPaymentCommandHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _subscriptionRepository = subscriptionRepository;
        _clientRepository = clientRepository;
        _tenantContext = tenantContext;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Processes a payment for a subscription.
    /// Validates client, subscription, and amount before creating the payment.
    /// Handles idempotency via ExternalTransactionId.
    /// </summary>
    /// <remarks>
    /// This operation is idempotent - multiple requests with the same ExternalTransactionId
    /// will return the same payment without creating duplicates.
    /// </remarks>
    public async Task<Result<PaymentDto>> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Processing payment for subscription {SubscriptionId} and client {ClientId}",
            request.SubscriptionId, request.ClientId);

        // Validate tenant context
        if (!_tenantContext.HasTenant)
        {
            _logger.LogWarning("Payment processing failed: Tenant context is required");
            return Result.Failure<PaymentDto>(DomainErrors.Tenant.NoTenantContext);
        }

        var tenantId = _tenantContext.CurrentTenantId!;

        // Validate currency
        if (!IsValidCurrencyCode(request.Currency))
        {
            _logger.LogWarning("Payment processing failed: Invalid currency code {Currency}", request.Currency);
            return Result.Failure<PaymentDto>(DomainErrors.Payment.InvalidCurrency);
        }

        // Validate Money.Create before transaction to catch validation errors early
        Money amount;
        try
        {
            amount = Money.Create(request.Amount, request.Currency);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Payment processing failed: Invalid money value - {Message}", ex.Message);
            return Result.Failure<PaymentDto>(DomainErrors.Payment.InvalidAmount);
        }

        // Validate payment method
        if (!Domain.Constants.PaymentMethods.IsValid(request.PaymentMethod))
        {
            _logger.LogWarning("Payment processing failed: Invalid payment method {PaymentMethod}", request.PaymentMethod);
            return Result.Failure<PaymentDto>(DomainErrors.Payment.InvalidPaymentMethod);
        }

        // Start transaction with ReadCommitted isolation - unique constraint handles race conditions
        await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        try
        {
            // Validate client exists and belongs to tenant
            var client = await _clientRepository.GetByIdAsync(request.ClientId, cancellationToken);
            if (client == null)
            {
                _logger.LogWarning("Payment processing failed: Client {ClientId} not found", request.ClientId);
                return await FailWithRollback(DomainErrors.Client.NotFound, cancellationToken);
            }

            if (client.TenantId != tenantId)
            {
                _logger.LogWarning("Payment processing failed: Client {ClientId} does not belong to tenant {TenantId}",
                    request.ClientId, tenantId);
                return await FailWithRollback(DomainErrors.Tenant.CrossTenantAccess, cancellationToken);
            }

            // Validate subscription exists and belongs to client
            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(request.SubscriptionId, cancellationToken);
            if (subscription == null)
            {
                _logger.LogWarning("Payment processing failed: Subscription {SubscriptionId} not found", request.SubscriptionId);
                return await FailWithRollback(DomainErrors.Subscription.NotFound, cancellationToken);
            }

            if (subscription.ClientId != request.ClientId)
            {
                _logger.LogWarning("Payment processing failed: Subscription {SubscriptionId} does not belong to client {ClientId}",
                    request.SubscriptionId, request.ClientId);
                return await FailWithRollback(DomainErrors.Tenant.CrossTenantAccess, cancellationToken);
            }

            if (subscription.TenantId != tenantId)
            {
                _logger.LogWarning("Payment processing failed: Subscription {SubscriptionId} does not belong to tenant {TenantId}",
                    request.SubscriptionId, tenantId);
                return await FailWithRollback(DomainErrors.Tenant.CrossTenantAccess, cancellationToken);
            }

            // Check if subscription is active and can accept payments
            if (subscription.Status != SubscriptionStatus.Active)
            {
                _logger.LogWarning("Payment processing failed: Subscription {SubscriptionId} is not active (status: {Status})",
                    request.SubscriptionId, subscription.Status);
                return await FailWithRollback(DomainErrors.Payment.SubscriptionNotActive, cancellationToken);
            }

            // Validate payment amount and currency against subscription
            if (subscription.Plan.Price.Currency != request.Currency)
            {
                _logger.LogWarning("Currency mismatch: payment {PaymentCurrency} vs subscription {SubscriptionCurrency}",
                    request.Currency, subscription.Plan.Price.Currency);
                return await FailWithRollback(DomainErrors.Payment.CurrencyMismatch, cancellationToken);
            }

            if (subscription.Plan.Price.Amount != request.Amount)
            {
                _logger.LogWarning("Amount mismatch: payment {PaymentAmount} vs subscription {SubscriptionAmount}",
                    request.Amount, subscription.Plan.Price.Amount);
                return await FailWithRollback(DomainErrors.Payment.AmountMismatch, cancellationToken);
            }

            // Validate payment method requirements
            if (request.PaymentMethod == Domain.Constants.PaymentMethods.Card && string.IsNullOrEmpty(request.ExternalPaymentId))
            {
                return await FailWithRollback(DomainErrors.Payment.ExternalPaymentIdRequired, cancellationToken);
            }

            // Check for duplicate external transaction ID within transaction
            if (!string.IsNullOrEmpty(request.ExternalTransactionId))
            {
                // NOTE: Using deprecated method because this command is only accessible by Providers and PlatformAdmins
                // who have proper authorization to view all payments in their tenant
#pragma warning disable CS0618 // Type or member is obsolete
                var existingPayment = await _paymentRepository.GetByExternalTransactionIdAsync(request.ExternalTransactionId, cancellationToken);
#pragma warning restore CS0618 // Type or member is obsolete
                if (existingPayment != null)
                {
                    // Option: Return existing payment for idempotency
                    _logger.LogInformation("Payment already exists for external transaction, returning existing payment {PaymentId}",
                        existingPayment.Id);
                    await _unitOfWork.RollbackAsync(cancellationToken);
                    return Result.Success(MapToDto(existingPayment));
                }
            }

            // Create payment
            var payment = Payment.Create(
                tenantId,
                request.SubscriptionId,
                request.ClientId,
                amount,
                request.ExternalTransactionId,
                request.PaymentMethod,
                request.ExternalPaymentId);

            _logger.LogInformation("Created payment {PaymentId} for subscription {SubscriptionId}",
                payment.Id, request.SubscriptionId);

            // Save payment
            var createdPayment = await _paymentRepository.AddAsync(payment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            stopwatch.Stop();
            _logger.LogInformation("Successfully processed payment {PaymentId} for subscription {SubscriptionId} in {ElapsedMs}ms",
                createdPayment.Id, request.SubscriptionId, stopwatch.ElapsedMilliseconds);

            // Map to DTO and return (no need for additional query since we have the created payment)
            var paymentDto = MapToDto(createdPayment);
            return Result.Success(paymentDto);
        }
        catch (DbUpdateException ex) when (IsDuplicateKeyException(ex))
        {
            _logger.LogWarning("Duplicate external transaction ID detected during save");
            await _unitOfWork.RollbackAsync(cancellationToken);
            return Result.Failure<PaymentDto>(DomainErrors.Payment.DuplicateExternalTransactionId);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Payment processing failed due to validation error: {Message}", ex.Message);
            await _unitOfWork.RollbackAsync(cancellationToken);
            return Result.Failure<PaymentDto>(DomainErrors.Payment.InvalidAmount);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Payment processing failed due to business rule violation: {Message}", ex.Message);
            await _unitOfWork.RollbackAsync(cancellationToken);
            return Result.Failure<PaymentDto>(DomainErrors.Payment.ProcessingFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Payment processing failed due to unexpected error: {Message}", ex.Message);
            await _unitOfWork.RollbackAsync(cancellationToken);
            return Result.Failure<PaymentDto>(DomainErrors.General.UnexpectedError);
        }
        finally
        {
            if (stopwatch.IsRunning)
            {
                stopwatch.Stop();
            }
            _logger.LogDebug("Payment processing attempt completed in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
        }
    }

    private async Task<Result<PaymentDto>> FailWithRollback(Error error, CancellationToken cancellationToken)
    {
        await _unitOfWork.RollbackAsync(cancellationToken);
        return Result.Failure<PaymentDto>(error);
    }

    private static bool IsValidCurrencyCode(string currency)
    {
        return !string.IsNullOrWhiteSpace(currency) &&
               ValidCurrencies.Contains(currency.ToUpperInvariant());
    }

    private static bool IsDuplicateKeyException(DbUpdateException ex)
    {
        // Check specific database error messages and types
        var innerEx = ex.InnerException;
        if (innerEx == null) return false;

        // Check for SQL Server unique constraint violations
        if (innerEx.GetType().Name == "SqlException")
        {
            // Use reflection to check Number property for SQL Server
            var numberProperty = innerEx.GetType().GetProperty("Number");
            if (numberProperty?.GetValue(innerEx) is int number)
            {
                return number == 2601 || number == 2627; // Unique constraint violations
            }
        }

        // Check for PostgreSQL unique violations
        if (innerEx.GetType().Name == "NpgsqlException")
        {
            var sqlStateProperty = innerEx.GetType().GetProperty("SqlState");
            if (sqlStateProperty?.GetValue(innerEx) is string sqlState)
            {
                return sqlState == "23505"; // Unique violation
            }
        }

        // Fallback to message-based detection
        return innerEx.Message?.Contains("IX_Payments_ExternalTransactionId") == true ||
               innerEx.Message?.Contains("duplicate", StringComparison.OrdinalIgnoreCase) == true ||
               innerEx.Message?.Contains("unique", StringComparison.OrdinalIgnoreCase) == true;
    }

    private static PaymentDto MapToDto(Payment payment)
    {
        return new PaymentDto
        {
            Id = payment.Id,
            TenantId = payment.TenantId.Value,
            SubscriptionId = payment.SubscriptionId,
            ClientId = payment.ClientId,
            Amount = payment.Amount.Amount,
            Currency = payment.Amount.Currency,
            Status = payment.Status.ToString(),
            ExternalTransactionId = payment.ExternalTransactionId,
            PaymentMethod = payment.PaymentMethod,
            ExternalPaymentId = payment.ExternalPaymentId,
            PaymentMethodId = payment.PaymentMethodId,
            CreatedAt = payment.CreatedAt,
            ProcessedAt = payment.ProcessedAt,
            FailedAt = payment.FailedAt,
            RefundedAt = payment.RefundedAt,
            FailureReason = payment.FailureReason
        };
    }
}
