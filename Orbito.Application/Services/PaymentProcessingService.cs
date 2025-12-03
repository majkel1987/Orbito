using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Configuration;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Orbito.Application.Common.Models.PaymentGateway;

namespace Orbito.Application.Services
{
    /// <summary>
    /// Service for processing payments with payment gateway integration
    /// </summary>
    public class PaymentProcessingService : IPaymentProcessingService
    {
        private readonly IPaymentGateway _paymentGateway;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PaymentProcessingService> _logger;
        private readonly ITenantContext _tenantContext;

        // Security limits
        private const decimal MinPaymentAmount = 0.50m; // Stripe minimum
        private const decimal MaxPaymentAmount = 999999.99m; // Reasonable limit

        /// <summary>
        /// Checks if the exception is a unique constraint violation
        /// </summary>
        private static bool IsUniqueConstraintViolation(DbUpdateException ex)
        {
            return ex.InnerException?.Message?.Contains("IX_Payments_SubscriptionId_Status_Unique") == true ||
                   ex.InnerException?.Message?.Contains("duplicate key") == true ||
                   ex.InnerException?.Message?.Contains("UNIQUE constraint") == true;
        }

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
        /// Validates subscription payment request
        /// </summary>
        private async Task<(bool IsValid, PaymentResult? Error, Subscription? Subscription)> ValidateSubscriptionPaymentAsync(
            Guid subscriptionId,
            Money amount,
            CancellationToken cancellationToken)
        {
            // SECURITY: Check tenant context
            if (!_tenantContext.HasTenant)
            {
                _logger.LogWarning("No tenant context for subscription payment {SubscriptionId}", subscriptionId);
                return (false, PaymentResult.Failure("Access denied", "ACCESS_DENIED"), null);
            }

            // SECURITY: Validate amounts
            if (amount.Amount < MinPaymentAmount)
            {
                _logger.LogWarning("Payment amount too small: {Amount}", amount.Amount);
                return (false, PaymentResult.Failure($"Amount must be at least {MinPaymentAmount}", "AMOUNT_TOO_SMALL"), null);
            }

            if (amount.Amount > MaxPaymentAmount)
            {
                _logger.LogWarning("Payment amount too large: {Amount}", amount.Amount);
                return (false, PaymentResult.Failure($"Amount cannot exceed {MaxPaymentAmount}", "AMOUNT_TOO_LARGE"), null);
            }

            // SECURITY: Get subscription and verify tenant context
            var subscription = await _unitOfWork.Subscriptions.GetByIdAsync(subscriptionId, cancellationToken);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found", subscriptionId);
                return (false, PaymentResult.Failure("Subscription not found", "SUBSCRIPTION_NOT_FOUND"), null);
            }

            // SECURITY: Verify tenant ownership (tenant context already verified earlier)
            if (subscription.TenantId != _tenantContext.CurrentTenantId)
            {
                _logger.LogWarning("Cross-tenant access attempt: Subscription {SubscriptionId} does not belong to tenant {TenantId}",
                    subscriptionId, _tenantContext.CurrentTenantId);
                return (false, PaymentResult.Failure("Access denied", "ACCESS_DENIED"), null);
            }

            // Check if subscription can be paid
            if (!subscription.CanBePaid())
            {
                _logger.LogWarning("Subscription {SubscriptionId} cannot be paid", subscriptionId);
                return (false, PaymentResult.Failure("Subscription cannot be paid", "SUBSCRIPTION_CANNOT_BE_PAID"), null);
            }

            // SECURITY: Check for duplicate payment attempts (race condition prevention)
            // Database unique constraint IX_Payments_SubscriptionId_Status_Unique ensures
            // only one Pending/Processing payment per subscription can exist
            // This check provides early detection before attempting DB insert
            var recentPayment = await _unitOfWork.Payments
                .GetRecentBySubscriptionIdAsync(subscriptionId, PaymentProcessingConfiguration.DuplicatePaymentCheckWindow, cancellationToken);

            if (recentPayment != null && (recentPayment.Status == PaymentStatus.Processing || recentPayment.Status == PaymentStatus.Pending))
            {
                _logger.LogWarning("Payment already in progress for subscription {SubscriptionId}", subscriptionId);
                return (false, PaymentResult.Failure("Payment already in progress", "PAYMENT_IN_PROGRESS"), null);
            }

            return (true, null, subscription);
        }

        /// <summary>
        /// Validates and retrieves payment method for payment processing
        /// </summary>
        private async Task<(bool IsValid, PaymentResult? Error, PaymentMethod? PaymentMethod)> ValidatePaymentMethodForProcessingAsync(
            Guid paymentMethodId,
            Guid clientId,
            CancellationToken cancellationToken)
        {
            var paymentMethod = await _unitOfWork.PaymentMethods.GetByIdAsync(paymentMethodId, clientId, cancellationToken);
            if (paymentMethod == null)
            {
                _logger.LogWarning("Payment method {PaymentMethodId} not found for client {ClientId}", paymentMethodId, clientId);
                return (false, PaymentResult.Failure("Payment method not found", "PAYMENT_METHOD_NOT_FOUND"), null);
            }

            if (!paymentMethod.CanBeUsed())
            {
                _logger.LogWarning("Payment method {PaymentMethodId} cannot be used (expired or invalid)", paymentMethodId);
                return (false, PaymentResult.Failure("Payment method cannot be used", "PAYMENT_METHOD_INVALID"), null);
            }

            return (true, null, paymentMethod);
        }

        /// <summary>
        /// Creates payment record in database with race condition protection
        /// </summary>
        private async Task<Payment> CreatePaymentRecordAsync(
            Subscription subscription,
            Money amount,
            CancellationToken cancellationToken)
        {
            var payment = Payment.Create(
                subscription.TenantId,
                subscription.Id,
                subscription.ClientId,
                amount,
                paymentMethod: PaymentProcessingConfiguration.DefaultPaymentMethod);

            try
            {
                await _unitOfWork.Payments.AddAsync(payment, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                _logger.LogWarning(ex, "Duplicate payment attempt detected for subscription {SubscriptionId}", subscription.Id);
                throw new InvalidOperationException("Payment already in progress", ex);
            }

            return payment;
        }

        /// <summary>
        /// Processes payment through gateway and updates payment status
        /// </summary>
        private async Task<PaymentResult> ProcessPaymentThroughGatewayAsync(
            Payment payment,
            Subscription subscription,
            PaymentMethod paymentMethod,
            Money amount,
            Guid paymentMethodId,
            string description,
            CancellationToken cancellationToken)
        {
            // Build payment request
            var request = new ProcessPaymentRequest
            {
                PaymentId = payment.Id,
                SubscriptionId = subscription.Id,
                ClientId = subscription.ClientId,
                Amount = amount,
                PaymentMethodId = paymentMethod.Token, // Use the Stripe token
                Description = description,
                TenantId = subscription.TenantId.Value,
                IdempotencyKey = $"payment_{payment.Id}_{DateTime.UtcNow.Ticks}",
                Metadata = new Dictionary<string, string>
                {
                    ["subscription_id"] = subscription.Id.ToString(),
                    ["client_id"] = subscription.ClientId.ToString(),
                    ["tenant_id"] = subscription.TenantId.Value.ToString(),
                    ["payment_method_id"] = paymentMethodId.ToString()
                }
            };

            // Process through payment gateway
            var result = await _paymentGateway.ProcessPaymentAsync(request);

            // Update payment status in system
            if (result.IsSuccess)
            {
                payment.ExternalPaymentId = result.ExternalPaymentId;
                payment.ExternalTransactionId = result.TransactionId;
                payment.PaymentMethodId = paymentMethodId.ToString();

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

        /// <summary>
        /// Processes subscription payment
        /// </summary>
        public async Task<PaymentResult> ProcessSubscriptionPaymentAsync(
            Guid subscriptionId,
            Money amount,
            Guid paymentMethodId,
            string description,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Processing subscription payment for subscription {SubscriptionId}", subscriptionId);
                _logger.LogDebug("Payment amount: {Amount}", amount);

                // Step 1: Validate subscription and amount
                var (isValidSubscription, subscriptionError, subscription) =
                    await ValidateSubscriptionPaymentAsync(subscriptionId, amount, cancellationToken);

                if (!isValidSubscription || subscription == null)
                    return subscriptionError!;

                // Step 2: Validate payment method
                var (isValidPaymentMethod, paymentMethodError, paymentMethod) =
                    await ValidatePaymentMethodForProcessingAsync(paymentMethodId, subscription.ClientId, cancellationToken);

                if (!isValidPaymentMethod || paymentMethod == null)
                    return paymentMethodError!;

                // Step 3: Create payment record with transaction
                Payment? payment = null;
                try
                {
                    payment = await CreatePaymentRecordAsync(subscription, amount, cancellationToken);

                    // Step 4: Process through payment gateway and update status
                    var result = await ProcessPaymentThroughGatewayAsync(
                        payment,
                        subscription,
                        paymentMethod,
                        amount,
                        paymentMethodId,
                        description,
                        cancellationToken);

                    return result;
                }
                catch (Exception ex)
                {
                    // COMPENSATION: Mark payment as failed if it was created
                    if (payment != null)
                    {
                        try
                        {
                            _logger.LogWarning("Compensating payment {PaymentId} due to error: {Error}", payment.Id, ex.Message);
                            payment.MarkAsFailed($"Gateway error: {ex.Message}");
                            await _unitOfWork.Payments.UpdateAsync(payment, cancellationToken);
                            await _unitOfWork.SaveChangesAsync(cancellationToken);
                        }
                        catch (Exception compensationEx)
                        {
                            _logger.LogError(compensationEx, "Failed to compensate payment {PaymentId}", payment.Id);
                        }
                    }
                    
                    throw;
                }
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Payment already in progress"))
            {
                _logger.LogWarning("Payment already in progress for subscription {SubscriptionId}", subscriptionId);
                return PaymentResult.Failure("Payment already in progress", "PAYMENT_IN_PROGRESS");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing subscription payment for subscription {SubscriptionId}", subscriptionId);
                return PaymentResult.Failure("An error occurred while processing payment", "PAYMENT_PROCESSING_ERROR");
            }
        }

        /// <summary>
        /// Handles successful payment
        /// </summary>
        public async Task HandlePaymentSuccessAsync(Guid paymentId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Handling payment success for payment {PaymentId}", paymentId);

                var payment = await _unitOfWork.Payments.GetByIdUnsafeAsync(paymentId, cancellationToken);
                if (payment == null)
                {
                    _logger.LogWarning("Payment {PaymentId} not found", paymentId);
                    return;
                }

                // SECURITY: Verify tenant context
                if (_tenantContext.HasTenant && payment.TenantId != _tenantContext.CurrentTenantId)
                {
                    _logger.LogWarning("Tenant mismatch for payment success {PaymentId}. Expected: {ExpectedTenant}, Actual: {ActualTenant}",
                        paymentId, _tenantContext.CurrentTenantId, payment.TenantId);
                    return;
                }

                // Check payment status in payment gateway
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
        /// Handles failed payment
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

                var payment = await _unitOfWork.Payments.GetByIdUnsafeAsync(paymentId, cancellationToken);
                if (payment == null)
                {
                    _logger.LogWarning("Payment {PaymentId} not found", paymentId);
                    return;
                }

                // SECURITY: Verify tenant context
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
        /// Refunds payment (full or partial)
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

                // SECURITY: Check tenant context
                if (!_tenantContext.HasTenant)
                {
                    _logger.LogWarning("No tenant context for payment refund {PaymentId}", paymentId);
                    return RefundResult.Failure("Access denied", "ACCESS_DENIED");
                }

                var payment = await _unitOfWork.Payments.GetByIdUnsafeAsync(paymentId, cancellationToken);
                if (payment == null)
                {
                    _logger.LogWarning("Payment {PaymentId} not found", paymentId);
                    return RefundResult.Failure("Payment not found", "PAYMENT_NOT_FOUND");
                }

                // SECURITY: Verify tenant context
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

                // SECURITY: Validate currency match
                if (amount.Currency != payment.Amount.Currency)
                {
                    _logger.LogWarning("Refund currency mismatch for payment {PaymentId}. Refund: {RefundCurrency}, Payment: {PaymentCurrency}",
                        paymentId, amount.Currency, payment.Amount.Currency);
                    return RefundResult.Failure($"Currency mismatch. Payment is in {payment.Amount.Currency}", "CURRENCY_MISMATCH");
                }

                // SECURITY: Check that total refunds don't exceed payment amount
                var totalRefunded = await _unitOfWork.Payments.GetTotalRefundedAmountAsync(paymentId, cancellationToken);

                if (totalRefunded + amount.Amount > payment.Amount.Amount)
                {
                    _logger.LogWarning("Refund amount {RefundAmount} exceeds remaining balance. Total refunded: {TotalRefunded}, Payment amount: {PaymentAmount}",
                        amount.Amount, totalRefunded, payment.Amount.Amount);
                    return RefundResult.Failure("Refund amount exceeds remaining balance", "REFUND_EXCEEDS_BALANCE");
                }

                // Refund payment through payment gateway
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

                // Update payment status in system
                if (result.IsSuccess)
                {
                    if (amount.Amount == payment.Amount.Amount)
                    {
                        payment.MarkAsRefunded(reason);
                    }
                    else
                    {
                        payment.MarkAsPartiallyRefunded(reason, amount);
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
        /// Creates customer in payment gateway
        /// </summary>
        public async Task<CustomerResult> CreateCustomerAsync(
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
                _logger.LogInformation("Creating customer for client {ClientId}", clientId);

                // SECURITY: Check tenant context
                if (!_tenantContext.HasTenant)
                {
                    _logger.LogWarning("No tenant context for creating customer {ClientId}", clientId);
                    return CustomerResult.Failure("Access denied", "ACCESS_DENIED");
                }

                var client = await _unitOfWork.Clients.GetByIdAsync(clientId, cancellationToken);
                if (client == null)
                {
                    _logger.LogWarning("Client {ClientId} not found", clientId);
                    return CustomerResult.Failure("Client not found", "CLIENT_NOT_FOUND");
                }

                // SECURITY: Verify tenant context
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
                    _logger.LogInformation("Customer created for client {ClientId} with ID {CustomerId}", 
                        clientId, result.ExternalCustomerId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer for client {ClientId}", clientId);
                return CustomerResult.Failure("An error occurred while creating customer", "CUSTOMER_CREATION_ERROR");
            }
        }

        /// <summary>
        /// Processes pending payments with batch processing to avoid N+1 queries
        /// Uses TenantId from current tenant context
        /// </summary>
        public async Task ProcessPendingPaymentsAsync(DateTime billingDate, CancellationToken cancellationToken = default)
        {
            // SECURITY: Verify tenant context
            if (!_tenantContext.HasTenant)
            {
                _logger.LogError("SECURITY: ProcessPendingPaymentsAsync called without tenant context");
                throw new InvalidOperationException("Tenant context is required for processing pending payments");
            }

            var tenantId = _tenantContext.CurrentTenantId!;
            await ProcessPendingPaymentsForTenantAsync(tenantId, billingDate, cancellationToken);
        }

        /// <summary>
        /// Processes pending payments for a specific tenant (for background jobs)
        /// SECURITY: Requires explicit TenantId to prevent cross-tenant access
        /// </summary>
        public async Task ProcessPendingPaymentsForTenantAsync(TenantId tenantId, DateTime billingDate, CancellationToken cancellationToken = default)
        {
            const int BatchSize = 10; // Process 10 payments concurrently

            try
            {
                _logger.LogInformation("Processing pending payments for tenant {TenantId} on date {BillingDate}", tenantId.Value, billingDate);

                // Get pending payments for specific tenant
                var pendingPayments = await _unitOfWork.Payments.GetPendingPaymentsForTenantAsync(tenantId, cancellationToken);

                // Filter payments that should be processed
                var paymentsToProcess = pendingPayments
                    .Where(p => p.CreatedAt.Add(PaymentProcessingConfiguration.PendingPaymentMinAge) <= billingDate)
                    .Where(p => !string.IsNullOrEmpty(p.ExternalPaymentId))
                    .ToList();

                _logger.LogInformation("Found {Count} pending payments to process", paymentsToProcess.Count);

                // Process in batches to avoid overwhelming the payment gateway
                var batches = paymentsToProcess.Chunk(BatchSize);
                var processedCount = 0;
                var updatedCount = 0;

                foreach (var batch in batches)
                {
                    try
                    {
                        // Process batch in parallel with throttling
                        var tasks = batch.Select(async payment =>
                        {
                            try
                            {
                                var statusResult = await _paymentGateway.GetPaymentStatusAsync(payment.ExternalPaymentId!);
                                return (payment, statusResult);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error checking status for payment {PaymentId}", payment.Id);
                                return (payment, (PaymentStatusResult?)null);
                            }
                        });

                        var results = await Task.WhenAll(tasks);

                        // Update all payments in batch
                        foreach (var (payment, statusResult) in results)
                        {
                            if (statusResult?.IsSuccess == true)
                            {
                                switch (statusResult.Status)
                                {
                                    case PaymentStatus.Completed:
                                        payment.MarkAsCompleted();
                                        await _unitOfWork.Payments.UpdateAsync(payment, cancellationToken);
                                        updatedCount++;
                                        _logger.LogInformation("Payment {PaymentId} marked as completed", payment.Id);
                                        break;
                                    case PaymentStatus.Failed:
                                        payment.MarkAsFailed("Payment failed in gateway");
                                        await _unitOfWork.Payments.UpdateAsync(payment, cancellationToken);
                                        updatedCount++;
                                        _logger.LogInformation("Payment {PaymentId} marked as failed", payment.Id);
                                        break;
                                }
                            }

                            processedCount++;
                        }

                        // Save changes after each batch
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                        _logger.LogDebug("Processed batch of {BatchSize} payments", batch.Count());
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing payment batch");
                    }
                }

                _logger.LogInformation("Successfully processed {ProcessedCount} pending payments for date {BillingDate}. Updated: {UpdatedCount}",
                    processedCount, billingDate, updatedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing pending payments for date {BillingDate}", billingDate);
            }
        }

        /// <summary>
        /// Updates payment from webhook data with signature verification
        /// </summary>
        public async Task UpdatePaymentFromWebhookAsync(string webhookData, string stripeSignature, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Updating payment from webhook data");

                // SECURITY: Verify webhook signature before processing
                var validationResult = await _paymentGateway.ValidateWebhookAsync(webhookData, stripeSignature);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Invalid webhook signature: {ErrorReason}", validationResult.ErrorReason);
                    throw new UnauthorizedAccessException($"Invalid webhook signature: {validationResult.ErrorReason}");
                }

                _logger.LogInformation("Webhook signature verified successfully for event {EventType}", validationResult.EventType);

                // Parse webhook JSON to extract event data
                using var jsonDocument = System.Text.Json.JsonDocument.Parse(webhookData);
                var root = jsonDocument.RootElement;

                // Extract event type and data
                if (!root.TryGetProperty("type", out var typeElement))
                {
                    _logger.LogWarning("Webhook data missing 'type' property");
                    return;
                }

                var eventType = typeElement.GetString();
                if (string.IsNullOrEmpty(eventType))
                {
                    _logger.LogWarning("Webhook event type is null or empty");
                    return;
                }

                // Extract payment intent data
                if (!root.TryGetProperty("data", out var dataElement) ||
                    !dataElement.TryGetProperty("object", out var objectElement))
                {
                    _logger.LogWarning("Webhook data missing 'data.object' property");
                    return;
                }

                // Extract payment intent ID
                if (!objectElement.TryGetProperty("id", out var idElement))
                {
                    _logger.LogWarning("Webhook object missing 'id' property");
                    return;
                }

                var externalPaymentId = idElement.GetString();
                if (string.IsNullOrEmpty(externalPaymentId))
                {
                    _logger.LogWarning("External payment ID is null or empty");
                    return;
                }

                _logger.LogInformation("Processing webhook event {EventType} for payment {ExternalPaymentId}",
                    eventType, externalPaymentId);

                // Find payment by external ID
                var payment = await _unitOfWork.Payments.GetByExternalPaymentIdUnsafeAsync(externalPaymentId, cancellationToken);
                if (payment == null)
                {
                    _logger.LogWarning("Payment not found for external ID {ExternalPaymentId}", externalPaymentId);
                    return;
                }

                // SECURITY: Verify tenant context if available (webhook may not have tenant context)
                if (_tenantContext.HasTenant && payment.TenantId != _tenantContext.CurrentTenantId)
                {
                    _logger.LogWarning("Tenant mismatch for payment {PaymentId}. Expected: {ExpectedTenant}, Actual: {ActualTenant}",
                        payment.Id, _tenantContext.CurrentTenantId, payment.TenantId);
                    return;
                }

                // Update payment based on event type
                switch (eventType)
                {
                    case "payment_intent.succeeded":
                        if (payment.Status != PaymentStatus.Completed)
                        {
                            payment.MarkAsCompleted();
                            await _unitOfWork.Payments.UpdateAsync(payment, cancellationToken);
                            _logger.LogInformation("Payment {PaymentId} marked as completed from webhook", payment.Id);
                        }
                        break;

                    case "payment_intent.payment_failed":
                        if (payment.Status != PaymentStatus.Failed)
                        {
                            // Extract failure reason if available
                            var failureReason = "Payment failed via webhook";
                            if (objectElement.TryGetProperty("last_payment_error", out var errorElement) &&
                                errorElement.TryGetProperty("message", out var messageElement))
                            {
                                failureReason = messageElement.GetString() ?? failureReason;
                            }

                            payment.MarkAsFailed(failureReason);
                            await _unitOfWork.Payments.UpdateAsync(payment, cancellationToken);
                            _logger.LogInformation("Payment {PaymentId} marked as failed from webhook: {Reason}",
                                payment.Id, failureReason);
                        }
                        break;

                    case "payment_intent.processing":
                        if (payment.Status == PaymentStatus.Pending)
                        {
                            payment.MarkAsProcessing();
                            await _unitOfWork.Payments.UpdateAsync(payment, cancellationToken);
                            _logger.LogInformation("Payment {PaymentId} marked as processing from webhook", payment.Id);
                        }
                        break;

                    case "payment_intent.canceled":
                        if (payment.Status != PaymentStatus.Failed)
                        {
                            payment.MarkAsFailed("Payment canceled");
                            await _unitOfWork.Payments.UpdateAsync(payment, cancellationToken);
                            _logger.LogInformation("Payment {PaymentId} marked as canceled from webhook", payment.Id);
                        }
                        break;

                    default:
                        _logger.LogInformation("Unhandled webhook event type: {EventType}", eventType);
                        return;
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Successfully updated payment {PaymentId} from webhook event {EventType}",
                    payment.Id, eventType);
            }
            catch (System.Text.Json.JsonException ex)
            {
                _logger.LogError(ex, "Error parsing webhook JSON data");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment from webhook");
            }
        }

        /// <summary>
        /// Validates payment status
        /// </summary>
        public async Task ValidatePaymentStatusAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Validating payment statuses");

                // SECURITY: Verify tenant context before querying
                if (!_tenantContext.HasTenant)
                {
                    _logger.LogWarning("ValidatePaymentStatusAsync called without tenant context");
                    return;
                }

                var tenantId = _tenantContext.CurrentTenantId;

                // Get payments in processing state for the current tenant
                var processingPayments = await _unitOfWork.Payments.GetProcessingPaymentsForTenantAsync(tenantId, cancellationToken);

                foreach (var payment in processingPayments)
                {
                    try
                    {
                        // Check if payment is too old (timeout)
                        if (payment.CreatedAt.Add(PaymentProcessingConfiguration.PaymentTimeout) < DateTime.UtcNow)
                        {
                            payment.MarkAsFailed("Payment timeout");
                            await _unitOfWork.Payments.UpdateAsync(payment, cancellationToken);
                            _logger.LogWarning("Payment {PaymentId} timed out", payment.Id);
                            continue;
                        }

                        // Check status in payment gateway
                        if (!string.IsNullOrEmpty(payment.ExternalPaymentId))
                        {
                            var statusResult = await _paymentGateway.GetPaymentStatusAsync(payment.ExternalPaymentId);
                            if (statusResult.IsSuccess)
                            {
                                switch (statusResult.Status)
                                {
                                    case PaymentStatus.Completed:
                                        payment.MarkAsCompleted();
                                        await _unitOfWork.Payments.UpdateAsync(payment, cancellationToken);
                                        _logger.LogInformation("Payment {PaymentId} marked as completed", payment.Id);
                                        break;
                                    case PaymentStatus.Failed:
                                        payment.MarkAsFailed("Payment failed in gateway");
                                        await _unitOfWork.Payments.UpdateAsync(payment, cancellationToken);
                                        _logger.LogInformation("Payment {PaymentId} marked as failed", payment.Id);
                                        break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error validating payment status for payment {PaymentId}", payment.Id);
                    }
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Successfully validated payment statuses");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating payment statuses");
            }
        }

        /// <summary>
        /// Synchronizes payment statuses with Stripe
        /// </summary>
        public async Task SyncPaymentStatusesWithStripeAsync(DateTime syncDate, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Syncing payment statuses with Stripe for date {SyncDate}", syncDate);

                // SECURITY: Verify tenant context before querying
                if (!_tenantContext.HasTenant)
                {
                    _logger.LogWarning("SyncPaymentStatusesWithStripeAsync called without tenant context");
                    return;
                }

                var tenantId = _tenantContext.CurrentTenantId;

                // Get payments with external IDs for the current tenant
                var paymentsWithExternalId = await _unitOfWork.Payments.GetPaymentsWithExternalIdForTenantAsync(tenantId, cancellationToken);

                foreach (var payment in paymentsWithExternalId)
                {
                    try
                    {
                        // SECURITY: Validate ExternalPaymentId is not null before calling gateway
                        if (string.IsNullOrEmpty(payment.ExternalPaymentId))
                        {
                            _logger.LogWarning("Payment {PaymentId} has null ExternalPaymentId, skipping sync", payment.Id);
                            continue;
                        }

                        // Check status in Stripe
                        var statusResult = await _paymentGateway.GetPaymentStatusAsync(payment.ExternalPaymentId);
                        if (statusResult.IsSuccess)
                        {
                            var currentStatus = payment.Status;
                            var gatewayStatus = statusResult.Status;

                            // Update status only if different
                            if (currentStatus != gatewayStatus)
                            {
                                switch (gatewayStatus)
                                {
                                    case PaymentStatus.Completed:
                                        payment.MarkAsCompleted();
                                        break;
                                    case PaymentStatus.Failed:
                                        payment.MarkAsFailed("Status updated from gateway");
                                        break;
                                    case PaymentStatus.Processing:
                                        payment.MarkAsProcessing();
                                        break;
                                }

                                await _unitOfWork.Payments.UpdateAsync(payment, cancellationToken);
                                _logger.LogInformation("Payment {PaymentId} status updated from {OldStatus} to {NewStatus}",
                                    payment.Id, currentStatus, gatewayStatus);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error syncing payment {PaymentId} with Stripe", payment.Id);
                    }
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Successfully synced payment statuses with Stripe for date {SyncDate}", syncDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing payment statuses with Stripe for date {SyncDate}", syncDate);
            }
        }

        /// <summary>
        /// Checks if payment can be refunded
        /// </summary>
        public async Task<bool> CanRefundAsync(Guid paymentId, Money amount, CancellationToken cancellationToken = default)
        {
            try
            {
                // SECURITY: Check tenant context
                if (!_tenantContext.HasTenant)
                {
                    _logger.LogWarning("No tenant context for refund check {PaymentId}", paymentId);
                    return false;
                }

                var payment = await _unitOfWork.Payments.GetByIdUnsafeAsync(paymentId, cancellationToken);
                if (payment == null)
                {
                    _logger.LogWarning("Payment {PaymentId} not found for refund check", paymentId);
                    return false;
                }

                // SECURITY: Verify tenant context
                if (payment.TenantId != _tenantContext.CurrentTenantId)
                {
                    _logger.LogWarning("Tenant mismatch for payment refund check {PaymentId}. Expected: {ExpectedTenant}, Actual: {ActualTenant}",
                        paymentId, _tenantContext.CurrentTenantId, payment.TenantId);
                    return false;
                }

                // Check if payment can be refunded
                if (!payment.CanBeRefunded())
                {
                    return false;
                }

                // SECURITY: Validate currency match
                if (amount.Currency != payment.Amount.Currency)
                {
                    _logger.LogWarning("Refund currency mismatch for payment {PaymentId}", paymentId);
                    return false;
                }

                // SECURITY: Check that total refunds don't exceed payment amount
                var totalRefunded = await _unitOfWork.Payments.GetTotalRefundedAmountAsync(paymentId, cancellationToken);
                if (totalRefunded + amount.Amount > payment.Amount.Amount)
                {
                    _logger.LogWarning("Refund amount {RefundAmount} would exceed remaining balance. Total refunded: {TotalRefunded}, Payment amount: {PaymentAmount}",
                        amount.Amount, totalRefunded, payment.Amount.Amount);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if payment {PaymentId} can be refunded", paymentId);
                return false;
            }
        }

        /// <summary>
        /// Checks rate limit for client
        /// </summary>
        public async Task<TimeSpan?> GetRateLimitDelayAsync(Guid clientId, CancellationToken cancellationToken = default)
        {
            try
            {
                // SECURITY: Check tenant context
                if (!_tenantContext.HasTenant)
                {
                    _logger.LogWarning("No tenant context for rate limit check {ClientId}", clientId);
                    return TimeSpan.FromMinutes(5); // Conservative default
                }

                // Rate limiting implementation - check recent payment attempts
                return await _unitOfWork.Payments.GetRateLimitDelayAsync(clientId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking rate limit for client {ClientId}", clientId);
                return TimeSpan.FromMinutes(5); // Conservative default on error
            }
        }

        /// <summary>
        /// Gets default payment method ID for client
        /// </summary>
        public async Task<Guid?> GetDefaultPaymentMethodAsync(Guid clientId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting default payment method for client {ClientId}", clientId);

                // SECURITY: Verify tenant context
                if (!_tenantContext.HasTenant)
                {
                    _logger.LogWarning("No tenant context for getting default payment method");
                    return null;
                }

                // Get default payment methods
                var defaultPaymentMethods = await _unitOfWork.PaymentMethods.GetDefaultPaymentMethodsByClientAsync(
                    clientId, cancellationToken);

                var defaultPaymentMethod = defaultPaymentMethods.FirstOrDefault();

                if (defaultPaymentMethod == null)
                {
                    _logger.LogWarning("No default payment method found for client {ClientId}", clientId);
                    return null;
                }

                // Verify payment method can be used
                if (!defaultPaymentMethod.CanBeUsed())
                {
                    _logger.LogWarning("Default payment method {PaymentMethodId} for client {ClientId} cannot be used (expired or invalid)",
                        defaultPaymentMethod.Id, clientId);
                    return null;
                }

                _logger.LogInformation("Default payment method {PaymentMethodId} found for client {ClientId}",
                    defaultPaymentMethod.Id, clientId);

                return defaultPaymentMethod.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting default payment method for client {ClientId}", clientId);
                return null;
            }
        }

        /// <summary>
        /// Validates if payment method exists and can be used
        /// </summary>
        public async Task<bool> ValidatePaymentMethodAsync(Guid paymentMethodId, Guid clientId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Validating payment method {PaymentMethodId} for client {ClientId}",
                    paymentMethodId, clientId);

                // SECURITY: Verify tenant context
                if (!_tenantContext.HasTenant)
                {
                    _logger.LogWarning("No tenant context for validating payment method");
                    return false;
                }

                // SECURITY: Get payment method with client verification
                var paymentMethod = await _unitOfWork.PaymentMethods.GetByIdAsync(
                    paymentMethodId, clientId, cancellationToken);

                if (paymentMethod == null)
                {
                    _logger.LogWarning("Payment method {PaymentMethodId} not found for client {ClientId}",
                        paymentMethodId, clientId);
                    return false;
                }

                // SECURITY: Verify tenant ownership
                if (paymentMethod.TenantId != _tenantContext.CurrentTenantId)
                {
                    _logger.LogWarning("Tenant mismatch for payment method {PaymentMethodId}. Expected: {ExpectedTenant}, Actual: {ActualTenant}",
                        paymentMethodId, _tenantContext.CurrentTenantId, paymentMethod.TenantId);
                    return false;
                }

                // Verify payment method can be used
                if (!paymentMethod.CanBeUsed())
                {
                    _logger.LogWarning("Payment method {PaymentMethodId} cannot be used (expired or invalid)",
                        paymentMethodId);
                    return false;
                }

                _logger.LogInformation("Payment method {PaymentMethodId} is valid and can be used", paymentMethodId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating payment method {PaymentMethodId} for client {ClientId}",
                    paymentMethodId, clientId);
                return false;
            }
        }
    }
}
