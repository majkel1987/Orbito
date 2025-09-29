using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Orbito.Application.Common.Models.PaymentGateway;

namespace Orbito.Application.Services
{
    /// <summary>
    /// Serwis do przetwarzania płatności z integracją payment gateway
    /// </summary>
    public class PaymentProcessingService : IPaymentProcessingService
    {
        private readonly IPaymentGateway _paymentGateway;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PaymentProcessingService> _logger;
        private readonly ITenantContext _tenantContext;

        // Limity bezpieczeństwa
        private const decimal MinPaymentAmount = 0.50m; // Stripe minimum
        private const decimal MaxPaymentAmount = 999999.99m; // Sensowny limit

        public PaymentProcessingService(
            IPaymentGateway paymentGateway,
            IUnitOfWork unitOfWork,
            ILogger<PaymentProcessingService> logger,
            ITenantContext tenantContext)
        {
            _paymentGateway = paymentGateway;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _tenantContext = tenantContext;
        }

        /// <summary>
        /// Przetwarza płatność subskrypcji
        /// </summary>
        public async Task<PaymentResult> ProcessSubscriptionPaymentAsync(
            Guid subscriptionId,
            Money amount,
            string paymentMethodId,
            string description,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Processing subscription payment for subscription {SubscriptionId}", subscriptionId);
                _logger.LogDebug("Payment amount: {Amount}", amount);

                // SECURITY: Sprawdź kontekst tenanta
                if (!_tenantContext.HasTenant)
                {
                    _logger.LogWarning("No tenant context for subscription payment {SubscriptionId}", subscriptionId);
                    return PaymentResult.Failure("Access denied", "ACCESS_DENIED");
                }

                // SECURITY: Walidacja kwot
                if (amount.Amount < MinPaymentAmount)
                {
                    _logger.LogWarning("Payment amount too small: {Amount}", amount.Amount);
                    return PaymentResult.Failure($"Amount must be at least {MinPaymentAmount}", "AMOUNT_TOO_SMALL");
                }

                if (amount.Amount > MaxPaymentAmount)
                {
                    _logger.LogWarning("Payment amount too large: {Amount}", amount.Amount);
                    return PaymentResult.Failure($"Amount cannot exceed {MaxPaymentAmount}", "AMOUNT_TOO_LARGE");
                }

                // Pobierz subskrypcję
                var subscription = await _unitOfWork.Subscriptions.GetByIdAsync(subscriptionId, cancellationToken);
                if (subscription == null)
                {
                    _logger.LogWarning("Subscription {SubscriptionId} not found", subscriptionId);
                    return PaymentResult.Failure("Subscription not found", "SUBSCRIPTION_NOT_FOUND");
                }

                // SECURITY: Weryfikacja tenant context
                if (subscription.TenantId != _tenantContext.CurrentTenantId)
                {
                    _logger.LogWarning("Tenant mismatch for subscription {SubscriptionId}. Expected: {ExpectedTenant}, Actual: {ActualTenant}",
                        subscriptionId, _tenantContext.CurrentTenantId, subscription.TenantId);
                    return PaymentResult.Failure("Access denied", "ACCESS_DENIED");
                }

                // Sprawdź czy subskrypcja może być opłacona
                if (!subscription.CanBePaid())
                {
                    _logger.LogWarning("Subscription {SubscriptionId} cannot be paid", subscriptionId);
                    return PaymentResult.Failure("Subscription cannot be paid", "SUBSCRIPTION_CANNOT_BE_PAID");
                }

                // SECURITY: Sprawdź czy nie ma płatności w trakcie (race condition)
                var recentPayment = await _unitOfWork.Payments
                    .GetRecentBySubscriptionIdAsync(subscriptionId, TimeSpan.FromMinutes(5), cancellationToken);

                if (recentPayment != null && recentPayment.Status == PaymentStatus.Processing)
                {
                    _logger.LogWarning("Payment already in progress for subscription {SubscriptionId}", subscriptionId);
                    return PaymentResult.Failure("Payment already in progress", "PAYMENT_IN_PROGRESS");
                }

                // Utwórz płatność w systemie
                var payment = Payment.Create(
                    subscription.TenantId,
                    subscriptionId,
                    subscription.ClientId,
                    amount,
                    paymentMethod: "Stripe");

                await _unitOfWork.Payments.AddAsync(payment, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Przetwórz płatność przez payment gateway
                var request = new ProcessPaymentRequest
                {
                    PaymentId = payment.Id,
                    SubscriptionId = subscriptionId,
                    ClientId = subscription.ClientId,
                    Amount = amount,
                    PaymentMethodId = paymentMethodId,
                    Description = description,
                    TenantId = subscription.TenantId.Value,
                    IdempotencyKey = $"payment_{payment.Id}_{DateTime.UtcNow.Ticks}",
                    Metadata = new Dictionary<string, string>
                    {
                        ["subscription_id"] = subscriptionId.ToString(),
                        ["client_id"] = subscription.ClientId.ToString(),
                        ["tenant_id"] = subscription.TenantId.Value.ToString()
                    }
                };

                var result = await _paymentGateway.ProcessPaymentAsync(request);

                // Aktualizuj status płatności w systemie
                if (result.IsSuccess)
                {
                    payment.ExternalPaymentId = result.ExternalPaymentId;
                    payment.ExternalTransactionId = result.TransactionId;
                    payment.PaymentMethodId = paymentMethodId;

                    if (result.Status == PaymentStatus.Completed)
                    {
                        payment.MarkAsCompleted();
                    }
                    else if (result.Status == PaymentStatus.Processing)
                    {
                        payment.MarkAsProcessing();
                    }
                }
                else
                {
                    payment.MarkAsFailed(result.ErrorMessage ?? "Payment processing failed");
                }

                await _unitOfWork.Payments.UpdateAsync(payment, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Payment {PaymentId} processed with status {Status}", 
                    payment.Id, payment.Status);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing subscription payment for subscription {SubscriptionId}", subscriptionId);
                return PaymentResult.Failure("An error occurred while processing payment", "PAYMENT_PROCESSING_ERROR");
            }
        }

        /// <summary>
        /// Obsługuje udaną płatność
        /// </summary>
        public async Task HandlePaymentSuccessAsync(Guid paymentId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Handling payment success for payment {PaymentId}", paymentId);

                var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId, cancellationToken);
                if (payment == null)
                {
                    _logger.LogWarning("Payment {PaymentId} not found", paymentId);
                    return;
                }

                // SECURITY: Weryfikacja tenant context
                if (_tenantContext.HasTenant && payment.TenantId != _tenantContext.CurrentTenantId)
                {
                    _logger.LogWarning("Tenant mismatch for payment success {PaymentId}. Expected: {ExpectedTenant}, Actual: {ActualTenant}",
                        paymentId, _tenantContext.CurrentTenantId, payment.TenantId);
                    return;
                }

                // Sprawdź status płatności w payment gateway
                if (!string.IsNullOrEmpty(payment.ExternalPaymentId))
                {
                    var statusResult = await _paymentGateway.GetPaymentStatusAsync(payment.ExternalPaymentId);
                    if (statusResult.IsSuccess && statusResult.Status == PaymentStatus.Completed)
                    {
                        payment.MarkAsCompleted();
                        await _unitOfWork.Payments.UpdateAsync(payment, cancellationToken);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);

                        _logger.LogInformation("Payment {PaymentId} marked as completed", paymentId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling payment success for payment {PaymentId}", paymentId);
            }
        }

        /// <summary>
        /// Obsługuje nieudaną płatność
        /// </summary>
        public async Task HandlePaymentFailureAsync(
            Guid paymentId,
            string reason,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Handling payment failure for payment {PaymentId}", paymentId);
                _logger.LogDebug("Failure reason: {Reason}", reason);

                var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId, cancellationToken);
                if (payment == null)
                {
                    _logger.LogWarning("Payment {PaymentId} not found", paymentId);
                    return;
                }

                // SECURITY: Weryfikacja tenant context
                if (_tenantContext.HasTenant && payment.TenantId != _tenantContext.CurrentTenantId)
                {
                    _logger.LogWarning("Tenant mismatch for payment failure {PaymentId}. Expected: {ExpectedTenant}, Actual: {ActualTenant}",
                        paymentId, _tenantContext.CurrentTenantId, payment.TenantId);
                    return;
                }

                payment.MarkAsFailed(reason);
                await _unitOfWork.Payments.UpdateAsync(payment, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Payment {PaymentId} marked as failed", paymentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling payment failure for payment {PaymentId}", paymentId);
            }
        }

        /// <summary>
        /// Zwraca płatność
        /// </summary>
        public async Task<RefundResult> RefundPaymentAsync(
            Guid paymentId,
            Money amount,
            string reason,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Processing refund for payment {PaymentId}", paymentId);
                _logger.LogDebug("Refund amount: {Amount}", amount);

                // SECURITY: Sprawdź kontekst tenanta
                if (!_tenantContext.HasTenant)
                {
                    _logger.LogWarning("No tenant context for payment refund {PaymentId}", paymentId);
                    return RefundResult.Failure("Access denied", "ACCESS_DENIED");
                }

                var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId, cancellationToken);
                if (payment == null)
                {
                    _logger.LogWarning("Payment {PaymentId} not found", paymentId);
                    return RefundResult.Failure("Payment not found", "PAYMENT_NOT_FOUND");
                }

                // SECURITY: Weryfikacja tenant context
                if (payment.TenantId != _tenantContext.CurrentTenantId)
                {
                    _logger.LogWarning("Tenant mismatch for payment {PaymentId}. Expected: {ExpectedTenant}, Actual: {ActualTenant}",
                        paymentId, _tenantContext.CurrentTenantId, payment.TenantId);
                    return RefundResult.Failure("Access denied", "ACCESS_DENIED");
                }

                if (!payment.CanBeRefunded())
                {
                    _logger.LogWarning("Payment {PaymentId} cannot be refunded", paymentId);
                    return RefundResult.Failure("Payment cannot be refunded", "PAYMENT_CANNOT_BE_REFUNDED");
                }

                if (string.IsNullOrEmpty(payment.ExternalPaymentId))
                {
                    _logger.LogWarning("Payment {PaymentId} has no external payment ID", paymentId);
                    return RefundResult.Failure("Payment has no external payment ID", "NO_EXTERNAL_PAYMENT_ID");
                }

                // SECURITY: Sprawdź czy suma zwrotów nie przekracza kwoty płatności
                var totalRefunded = await _unitOfWork.Payments.GetTotalRefundedAmountAsync(paymentId, cancellationToken);

                if (totalRefunded + amount.Amount > payment.Amount.Amount)
                {
                    _logger.LogWarning("Refund amount {RefundAmount} exceeds remaining balance. Total refunded: {TotalRefunded}, Payment amount: {PaymentAmount}",
                        amount.Amount, totalRefunded, payment.Amount.Amount);
                    return RefundResult.Failure("Refund amount exceeds remaining balance", "REFUND_EXCEEDS_BALANCE");
                }

                // Zwróć płatność przez payment gateway
                var request = new RefundRequest
                {
                    PaymentId = paymentId,
                    ExternalPaymentId = payment.ExternalPaymentId,
                    Amount = amount,
                    Reason = reason,
                    TenantId = payment.TenantId.Value,
                    Metadata = new Dictionary<string, string>
                    {
                        ["payment_id"] = paymentId.ToString(),
                        ["subscription_id"] = payment.SubscriptionId.ToString(),
                        ["client_id"] = payment.ClientId.ToString(),
                        ["tenant_id"] = payment.TenantId.Value.ToString()
                    }
                };

                var result = await _paymentGateway.RefundPaymentAsync(request);

                // Aktualizuj status płatności w systemie
                if (result.IsSuccess)
                {
                    if (amount.Amount == payment.Amount.Amount)
                    {
                        payment.MarkAsRefunded(reason);
                    }
                    else
                    {
                        payment.MarkAsPartiallyRefunded(reason);
                    }

                    await _unitOfWork.Payments.UpdateAsync(payment, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Payment {PaymentId} refunded successfully", paymentId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund for payment {PaymentId}", paymentId);
                return RefundResult.Failure("An error occurred while processing refund", "REFUND_PROCESSING_ERROR");
            }
        }

        /// <summary>
        /// Tworzy klienta w payment gateway
        /// </summary>
        public async Task<CustomerResult> CreateStripeCustomerAsync(
            Guid clientId,
            string email,
            string? firstName = null,
            string? lastName = null,
            string? companyName = null,
            string? phone = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Creating Stripe customer for client {ClientId}", clientId);

                // SECURITY: Sprawdź kontekst tenanta
                if (!_tenantContext.HasTenant)
                {
                    _logger.LogWarning("No tenant context for creating Stripe customer {ClientId}", clientId);
                    return CustomerResult.Failure("Access denied", "ACCESS_DENIED");
                }

                var client = await _unitOfWork.Clients.GetByIdAsync(clientId, cancellationToken);
                if (client == null)
                {
                    _logger.LogWarning("Client {ClientId} not found", clientId);
                    return CustomerResult.Failure("Client not found", "CLIENT_NOT_FOUND");
                }

                // SECURITY: Weryfikacja tenant context
                if (client.TenantId != _tenantContext.CurrentTenantId)
                {
                    _logger.LogWarning("Tenant mismatch for client {ClientId}. Expected: {ExpectedTenant}, Actual: {ActualTenant}",
                        clientId, _tenantContext.CurrentTenantId, client.TenantId);
                    return CustomerResult.Failure("Access denied", "ACCESS_DENIED");
                }

                var request = new CreateCustomerRequest
                {
                    ClientId = clientId,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    CompanyName = companyName,
                    Phone = phone,
                    TenantId = client.TenantId.Value,
                    Metadata = new Dictionary<string, string>
                    {
                        ["client_id"] = clientId.ToString(),
                        ["tenant_id"] = client.TenantId.Value.ToString()
                    }
                };

                var result = await _paymentGateway.CreateCustomerAsync(request);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Stripe customer created for client {ClientId} with ID {StripeCustomerId}", 
                        clientId, result.ExternalCustomerId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Stripe customer for client {ClientId}", clientId);
                return CustomerResult.Failure("An error occurred while creating customer", "CUSTOMER_CREATION_ERROR");
            }
        }
    }
}
