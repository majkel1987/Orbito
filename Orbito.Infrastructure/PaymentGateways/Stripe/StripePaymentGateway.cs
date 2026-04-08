using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models.PaymentGateway;
using Orbito.Infrastructure.PaymentGateways.Stripe.Models;
using Stripe;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Orbito.Infrastructure.PaymentGateways.Stripe
{
    /// <summary>
    /// Implementacja IPaymentGateway dla Stripe
    /// </summary>
    public class StripePaymentGateway : IPaymentGateway, IDisposable
    {
        private readonly StripeConfiguration _configuration;
        private readonly ILogger<StripePaymentGateway> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly SemaphoreSlim _rateLimiter = new(10, 10); // Max 10 concurrent requests

        public StripePaymentGateway(
            IOptions<StripeConfiguration> configuration,
            ILogger<StripePaymentGateway> logger,
            IUnitOfWork unitOfWork)
        {
            _configuration = configuration.Value ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

            // Walidacja konfiguracji
            var (isValid, errorMessage) = _configuration.ValidateConfiguration();
            if (!isValid)
            {
                throw new InvalidOperationException($"Invalid Stripe configuration: {errorMessage}");
            }

            // Sprawdzenie HTTPS dla produkcji
            if (_configuration.IsLiveEnvironment())
            {
                _logger.LogWarning("Live Stripe environment detected. Ensure HTTPS is enabled in production.");
            }

            _logger.LogInformation("Stripe payment gateway initialized for {Environment} environment",
                _configuration.IsTestEnvironment() ? "test" : "live");

            // Dodaj ostrzeżenie o bezpieczeństwie dla środowiska testowego
            if (_configuration.IsTestEnvironment())
            {
                _logger.LogDebug("Running in Stripe TEST mode - transactions will not be charged");
            }
        }

        /// <summary>
        /// Przetwarza płatność przez Stripe
        /// </summary>
        public async Task<PaymentResult> ProcessPaymentAsync(ProcessPaymentRequest request)
        {
            await _rateLimiter.WaitAsync();
            try
            {
                _logger.LogInformation("Processing payment {PaymentId} for subscription {SubscriptionId} via Stripe",
                    request.PaymentId, request.SubscriptionId);

                var paymentIntentService = new PaymentIntentService();

                var options = new PaymentIntentCreateOptions
                {
                    Amount = ConvertToStripeAmount(request.Amount),
                    Currency = request.Amount.Currency.Code.ToLowerInvariant(),
                    Customer = await GetOrCreateStripeCustomerAsync(request),
                    PaymentMethod = request.PaymentMethodId,
                    Description = request.Description,
                    Metadata = new Dictionary<string, string>
                    {
                        ["payment_id"] = request.PaymentId.ToString(),
                        ["subscription_id"] = request.SubscriptionId.ToString(),
                        ["client_id"] = request.ClientId.ToString(),
                        ["tenant_id"] = request.TenantId.ToString()
                    }
                };

                // Dodaj metadane z requestu
                foreach (var metadata in request.Metadata)
                {
                    options.Metadata[metadata.Key] = metadata.Value;
                }

                var requestOptions = new RequestOptions
                {
                    ApiKey = _configuration.SecretKey,
                    IdempotencyKey = $"payment_{request.PaymentId}" // Payment ID is unique, no need for date
                };

                var paymentIntent = await ExecuteWithRetryAsync(async () =>
                    await paymentIntentService.CreateAsync(options, requestOptions), "CreatePaymentIntent");

                // Potwierdź płatność jeśli payment method jest dostępny
                if (!string.IsNullOrEmpty(request.PaymentMethodId))
                {
                    var confirmOptions = new PaymentIntentConfirmOptions
                    {
                        PaymentMethod = request.PaymentMethodId
                    };

                    var confirmRequestOptions = new RequestOptions
                    {
                        ApiKey = _configuration.SecretKey,
                        IdempotencyKey = $"confirm_{request.PaymentId}" // Payment ID is unique
                    };

                    paymentIntent = await ExecuteWithRetryAsync(async () =>
                        await paymentIntentService.ConfirmAsync(paymentIntent.Id, confirmOptions, confirmRequestOptions), "ConfirmPaymentIntent");
                }

                var result = MapStripePaymentIntentToResult(paymentIntent);

                _logger.LogInformation("Payment {PaymentId} processed via Stripe with status {Status}",
                    request.PaymentId, result.Status);

                return result;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error processing payment {PaymentId}: {ErrorMessage}",
                    request.PaymentId, ex.Message);

                return PaymentResult.Failure(
                    ex.Message,
                    ex.StripeError?.Code,
                    new Dictionary<string, string>
                    {
                        ["stripe_error_type"] = ex.StripeError?.Type ?? "unknown",
                        ["stripe_error_code"] = ex.StripeError?.Code ?? "unknown"
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing payment {PaymentId}: {ErrorMessage}",
                    request.PaymentId, ex.Message);

                return PaymentResult.Failure(
                    "An unexpected error occurred while processing payment",
                    "UNEXPECTED_ERROR");
            }
            finally
            {
                _rateLimiter.Release();
            }
        }

        /// <summary>
        /// Zwraca płatność przez Stripe
        /// </summary>
        public async Task<RefundResult> RefundPaymentAsync(RefundRequest request)
        {
            await _rateLimiter.WaitAsync();
            try
            {
                _logger.LogInformation("Processing refund for payment {PaymentId} via Stripe", request.PaymentId);

                var refundService = new RefundService();

                var options = new RefundCreateOptions
                {
                    PaymentIntent = request.ExternalPaymentId,
                    Amount = ConvertToStripeAmount(request.Amount),
                    Reason = MapRefundReason(request.Reason),
                    Metadata = new Dictionary<string, string>
                    {
                        ["payment_id"] = request.PaymentId.ToString(),
                        ["tenant_id"] = request.TenantId.ToString()
                    }
                };

                // Dodaj metadane z requestu
                foreach (var metadata in request.Metadata)
                {
                    options.Metadata[metadata.Key] = metadata.Value;
                }

                var requestOptions = new RequestOptions
                {
                    ApiKey = _configuration.SecretKey,
                    IdempotencyKey = $"refund_{request.PaymentId}" // Payment ID is unique
                };

                var refund = await ExecuteWithRetryAsync(async () =>
                    await refundService.CreateAsync(options, requestOptions), "CreateRefund");

                var result = RefundResult.Success(
                    RefundStatus.Completed,
                    refund.Id,
                    refund.ChargeId,
                    new Dictionary<string, string>
                    {
                        ["stripe_refund_id"] = refund.Id,
                        ["stripe_charge_id"] = refund.ChargeId ?? string.Empty,
                        ["refund_reason"] = request.Reason
                    },
                    refund.Created,
                    null,
                    request.Reason);

                _logger.LogInformation("Refund processed for payment {PaymentId} via Stripe with ID {RefundId}",
                    request.PaymentId, refund.Id);

                return result;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error processing refund for payment {PaymentId}: {ErrorMessage}",
                    request.PaymentId, ex.Message);

                return RefundResult.Failure(
                    ex.Message,
                    ex.StripeError?.Code,
                    new Dictionary<string, string>
                    {
                        ["stripe_error_type"] = ex.StripeError?.Type ?? "unknown",
                        ["stripe_error_code"] = ex.StripeError?.Code ?? "unknown"
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing refund for payment {PaymentId}: {ErrorMessage}",
                    request.PaymentId, ex.Message);

                return RefundResult.Failure(
                    "An unexpected error occurred while processing refund",
                    "UNEXPECTED_ERROR");
            }
            finally
            {
                _rateLimiter.Release();
            }
        }

        /// <summary>
        /// Tworzy klienta w Stripe
        /// </summary>
        public async Task<CustomerResult> CreateCustomerAsync(CreateCustomerRequest request)
        {
            await _rateLimiter.WaitAsync();
            try
            {
                _logger.LogInformation("Creating customer {ClientId} in Stripe", request.ClientId);

                var customerService = new CustomerService();

                var options = new CustomerCreateOptions
                {
                    Email = request.Email,
                    Name = $"{request.FirstName} {request.LastName}".Trim(),
                    Phone = request.Phone,
                    Metadata = new Dictionary<string, string>
                    {
                        ["client_id"] = request.ClientId.ToString(),
                        ["tenant_id"] = request.TenantId.ToString()
                    }
                };

                // Dodaj adres jeśli dostępny
                if (request.Address != null)
                {
                    options.Address = new AddressOptions
                    {
                        Line1 = request.Address.Line1,
                        Line2 = request.Address.Line2,
                        City = request.Address.City,
                        PostalCode = request.Address.PostalCode,
                        State = request.Address.State,
                        Country = request.Address.Country
                    };
                }

                // Dodaj metadane z requestu
                foreach (var metadata in request.Metadata)
                {
                    options.Metadata[metadata.Key] = metadata.Value;
                }

                var requestOptions = new RequestOptions
                {
                    ApiKey = _configuration.SecretKey,
                    IdempotencyKey = $"customer_{request.ClientId}" // Client ID is unique
                };

                var customer = await ExecuteWithRetryAsync(async () =>
                    await customerService.CreateAsync(options, requestOptions), "CreateCustomer");

                var result = CustomerResult.Success(
                    customer.Id,
                    customer.Email,
                    request.FirstName,
                    request.LastName,
                    new Dictionary<string, string>
                    {
                        ["stripe_customer_id"] = customer.Id,
                        ["client_id"] = request.ClientId.ToString(),
                        ["tenant_id"] = request.TenantId.ToString()
                    },
                    customer.Created);

                _logger.LogInformation("Customer {ClientId} created in Stripe with ID {StripeCustomerId}",
                    request.ClientId, customer.Id);

                return result;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating customer {ClientId}: {ErrorMessage}",
                    request.ClientId, ex.Message);

                return CustomerResult.Failure(
                    ex.Message,
                    ex.StripeError?.Code,
                    new Dictionary<string, string>
                    {
                        ["stripe_error_type"] = ex.StripeError?.Type ?? "unknown",
                        ["stripe_error_code"] = ex.StripeError?.Code ?? "unknown"
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating customer {ClientId}: {ErrorMessage}",
                    request.ClientId, ex.Message);

                return CustomerResult.Failure(
                    "An unexpected error occurred while creating customer",
                    "UNEXPECTED_ERROR");
            }
            finally
            {
                _rateLimiter.Release();
            }
        }

        /// <summary>
        /// Creates a PaymentIntent for Stripe Elements (PCI DSS compliant)
        /// Card data stays on Stripe's servers - never touches Orbito
        /// </summary>
        public async Task<CreatePaymentIntentResult> CreatePaymentIntentAsync(CreatePaymentIntentRequest request)
        {
            await _rateLimiter.WaitAsync();
            try
            {
                _logger.LogInformation(
                    "Creating PaymentIntent for subscription {SubscriptionId}, client {ClientId}, amount {Amount} {Currency}",
                    request.SubscriptionId, request.ClientId, request.Amount.Amount, request.Amount.Currency.Code);

                var paymentIntentService = new PaymentIntentService();

                var options = new PaymentIntentCreateOptions
                {
                    Amount = ConvertToStripeAmount(request.Amount),
                    Currency = request.Amount.Currency.Code.ToLowerInvariant(),
                    Customer = request.CustomerId,
                    Description = request.Description,
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                    {
                        Enabled = true
                    },
                    Metadata = new Dictionary<string, string>
                    {
                        ["subscription_id"] = request.SubscriptionId.ToString(),
                        ["client_id"] = request.ClientId.ToString(),
                        ["tenant_id"] = request.TenantId.ToString(),
                        ["subscription_type"] = "client" // Distinguishes from provider platform payments
                    }
                };

                // Add custom metadata
                foreach (var metadata in request.Metadata)
                {
                    options.Metadata[metadata.Key] = metadata.Value;
                }

                var requestOptions = new RequestOptions
                {
                    ApiKey = _configuration.SecretKey,
                    IdempotencyKey = $"pi_{request.SubscriptionId}_{DateTime.UtcNow:yyyyMMddHHmmss}" // Include timestamp for recurring payments
                };

                var paymentIntent = await ExecuteWithRetryAsync(
                    async () => await paymentIntentService.CreateAsync(options, requestOptions),
                    "CreatePaymentIntent");

                _logger.LogInformation(
                    "PaymentIntent {PaymentIntentId} created successfully for subscription {SubscriptionId}",
                    paymentIntent.Id, request.SubscriptionId);

                return CreatePaymentIntentResult.Success(
                    paymentIntent.ClientSecret,
                    paymentIntent.Id,
                    request.Amount.Amount,
                    request.Amount.Currency.Code,
                    new Dictionary<string, string>
                    {
                        ["stripe_payment_intent_id"] = paymentIntent.Id,
                        ["stripe_customer_id"] = paymentIntent.CustomerId ?? string.Empty,
                        ["stripe_status"] = paymentIntent.Status
                    });
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex,
                    "Stripe error creating PaymentIntent for subscription {SubscriptionId}: {ErrorMessage}",
                    request.SubscriptionId, ex.Message);

                return CreatePaymentIntentResult.Failure(
                    ex.Message,
                    ex.StripeError?.Code,
                    new Dictionary<string, string>
                    {
                        ["stripe_error_type"] = ex.StripeError?.Type ?? "unknown",
                        ["stripe_error_code"] = ex.StripeError?.Code ?? "unknown"
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error creating PaymentIntent for subscription {SubscriptionId}: {ErrorMessage}",
                    request.SubscriptionId, ex.Message);

                return CreatePaymentIntentResult.Failure(
                    "An unexpected error occurred while creating payment intent",
                    "UNEXPECTED_ERROR");
            }
            finally
            {
                _rateLimiter.Release();
            }
        }

        /// <summary>
        /// Sprawdza status płatności w Stripe
        /// </summary>
        public async Task<PaymentStatusResult> GetPaymentStatusAsync(string externalPaymentId)
        {
            try
            {
                _logger.LogInformation("Getting payment status for {ExternalPaymentId} from Stripe", externalPaymentId);

                var paymentIntentService = new PaymentIntentService();

                var paymentIntent = await paymentIntentService.GetAsync(externalPaymentId);

                var result = PaymentStatusResult.Success(
                    MapStripeStatusToPaymentStatus(paymentIntent.Status),
                    paymentIntent.Id,
                    new Dictionary<string, string>
                    {
                        ["stripe_payment_intent_id"] = paymentIntent.Id,
                        ["stripe_status"] = paymentIntent.Status,
                        ["stripe_customer_id"] = paymentIntent.CustomerId ?? string.Empty
                    },
                    paymentIntent.Created,
                    paymentIntent.Status == "succeeded" ? paymentIntent.Created : null,
                    paymentIntent.PaymentMethod?.Type,
                    paymentIntent.PaymentMethod?.Card?.Last4,
                    paymentIntent.PaymentMethod?.Card?.Brand);

                _logger.LogInformation("Payment status retrieved for {ExternalPaymentId}: {Status}",
                    externalPaymentId, result.Status);

                return result;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error getting payment status for {ExternalPaymentId}: {ErrorMessage}",
                    externalPaymentId, ex.Message);

                return PaymentStatusResult.Failure(
                    ex.Message,
                    ex.StripeError?.Code,
                    new Dictionary<string, string>
                    {
                        ["stripe_error_type"] = ex.StripeError?.Type ?? "unknown",
                        ["stripe_error_code"] = ex.StripeError?.Code ?? "unknown"
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting payment status for {ExternalPaymentId}: {ErrorMessage}",
                    externalPaymentId, ex.Message);

                return PaymentStatusResult.Failure(
                    "An unexpected error occurred while getting payment status",
                    "UNEXPECTED_ERROR");
            }
        }

        /// <summary>
        /// Waliduje webhook od Stripe
        /// </summary>
        public Task<WebhookValidationResult> ValidateWebhookAsync(string payload, string signature)
        {
            try
            {
                if (string.IsNullOrEmpty(_configuration.WebhookSecret))
                {
                    _logger.LogWarning("Stripe webhook secret not configured, skipping validation");
                    return Task.FromResult(WebhookValidationResult.Failure("Webhook secret not configured"));
                }

                var stripeEvent = EventUtility.ConstructEvent(payload, signature, _configuration.WebhookSecret);

                _logger.LogInformation("Stripe webhook validated successfully for event {EventType}", stripeEvent.Type);

                var metadata = new Dictionary<string, string>
                {
                    ["EventId"] = stripeEvent.Id,
                    ["ApiVersion"] = stripeEvent.ApiVersion ?? "unknown",
                    ["Created"] = stripeEvent.Created.ToString()
                };

                return Task.FromResult(WebhookValidationResult.Success(
                    stripeEvent,
                    stripeEvent.Type,
                    stripeEvent.Created,
                    metadata));
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe webhook validation failed: {ErrorMessage}", ex.Message);
                return Task.FromResult(WebhookValidationResult.Failure($"Stripe validation failed: {ex.Message}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error validating Stripe webhook: {ErrorMessage}", ex.Message);
                return Task.FromResult(WebhookValidationResult.Failure($"Unexpected error: {ex.Message}"));
            }
        }

        #region Private Methods

        /// <summary>
        /// Pobiera lub tworzy klienta Stripe
        /// </summary>
        private async Task<string?> GetOrCreateStripeCustomerAsync(ProcessPaymentRequest request)
        {
            try
            {
                // Sprawdź czy klient już ma Stripe Customer ID
                var client = await _unitOfWork.Clients.GetByIdAsync(request.ClientId, CancellationToken.None);
                if (client == null)
                {
                    _logger.LogWarning("Client {ClientId} not found", request.ClientId);
                    return null;
                }

                // CRITICAL: Verify tenant ownership to prevent cross-tenant access
                if (client.TenantId != request.TenantId)
                {
                    _logger.LogError("SECURITY VIOLATION: Tenant mismatch for client {ClientId}: expected {ExpectedTenantId}, got {ActualTenantId}",
                        request.ClientId, request.TenantId, client.TenantId);
                    throw new UnauthorizedAccessException($"Tenant validation failed for client {request.ClientId}");
                }

                // Jeśli klient ma już Stripe Customer ID, zwróć go
                if (!string.IsNullOrEmpty(client.StripeCustomerId))
                {
                    _logger.LogDebug("Using existing Stripe customer {StripeCustomerId} for client {ClientId}",
                        client.StripeCustomerId, request.ClientId);
                    return client.StripeCustomerId;
                }

                // Utwórz nowego klienta w Stripe
                var createCustomerRequest = new CreateCustomerRequest
                {
                    ClientId = client.Id,
                    TenantId = client.TenantId,
                    Email = client.Email,
                    FirstName = client.FirstName,
                    LastName = client.LastName,
                    Phone = client.Phone,
                    Metadata = new Dictionary<string, string>
                    {
                        ["client_id"] = client.Id.ToString(),
                        ["tenant_id"] = client.TenantId.ToString()
                    }
                };

                var customerResult = await CreateCustomerAsync(createCustomerRequest);
                if (customerResult.IsSuccess)
                {
                    // Zapisz Stripe Customer ID w bazie danych
                    var setResult = client.SetStripeCustomerId(customerResult.ExternalCustomerId!);
                    if (setResult.IsFailure)
                    {
                        _logger.LogError("Failed to set Stripe customer ID for client {ClientId}: {Error}",
                            client.Id, setResult.Error.Message);
                        throw new InvalidOperationException($"Failed to set Stripe customer ID for client {client.Id}: {setResult.Error.Message}");
                    }
                    await _unitOfWork.Clients.UpdateAsync(client, CancellationToken.None);

                    var saveResult = await _unitOfWork.SaveChangesAsync();
                    if (saveResult.IsSuccess)
                    {
                        _logger.LogInformation("Created and linked Stripe customer {StripeCustomerId} for client {ClientId}",
                            customerResult.ExternalCustomerId, client.Id);
                        return customerResult.ExternalCustomerId;
                    }
                    else
                    {
                        _logger.LogError("Failed to save Stripe customer ID for client {ClientId}: {Error}",
                            client.Id, saveResult.ErrorMessage);
                        throw new InvalidOperationException($"Failed to save Stripe customer ID for client {client.Id}: {saveResult.ErrorMessage}");
                    }
                }
                else
                {
                    _logger.LogError("Failed to create Stripe customer for client {ClientId}: {Error}",
                        client.Id, customerResult.ErrorMessage);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting or creating Stripe customer for client {ClientId}", request.ClientId);
                return null;
            }
        }

        /// <summary>
        /// Konwertuje kwotę na format Stripe (centy) z walidacją
        /// </summary>
        private static long ConvertToStripeAmount(Money amount)
        {
            var amountInCents = (long)(amount.Amount * 100);

            // Walidacja minimalnej kwoty (50 centów dla większości walut)
            var minimumAmount = GetMinimumAmountForCurrency(amount.Currency.Code);
            if (amountInCents < minimumAmount)
            {
                throw new ArgumentException(
                    $"Amount {amount.Amount} {amount.Currency.Code} is below Stripe minimum of {minimumAmount / 100.0M} {amount.Currency.Code}");
            }

            // Walidacja maksymalnej kwoty Stripe (99999999 centów)
            const long maxStripeAmount = 99999999;
            if (amountInCents > maxStripeAmount)
            {
                throw new ArgumentException(
                    $"Amount {amount.Amount} {amount.Currency.Code} exceeds Stripe maximum of {maxStripeAmount / 100.0M} {amount.Currency.Code}");
            }

            return amountInCents;
        }

        /// <summary>
        /// Zwraca minimalną kwotę dla danej waluty w centach
        /// </summary>
        private static long GetMinimumAmountForCurrency(string currencyCode)
        {
            return currencyCode.ToUpperInvariant() switch
            {
                "USD" or "EUR" or "GBP" or "CAD" or "AUD" => 50, // 50 centów
                "SEK" or "DKK" or "NOK" => 300, // 3.00 w tych walutach
                "JPY" => 50, // 50 jenów (bez ułamków)
                "MXN" => 1000, // 10.00 pesos
                "PLN" => 200, // 2.00 PLN
                _ => 50 // Domyślnie 50 centów
            };
        }

        /// <summary>
        /// Mapuje status Stripe na PaymentStatus
        /// </summary>
        private static PaymentStatus MapStripeStatusToPaymentStatus(string stripeStatus)
        {
            return stripeStatus switch
            {
                "requires_payment_method" or "requires_confirmation" => PaymentStatus.Pending,
                "processing" => PaymentStatus.Processing,
                "succeeded" => PaymentStatus.Completed,
                "requires_action" => PaymentStatus.Pending,
                "canceled" => PaymentStatus.Cancelled,
                _ => PaymentStatus.Failed
            };
        }

        /// <summary>
        /// Mapuje PaymentIntent na PaymentResult
        /// </summary>
        private static PaymentResult MapStripePaymentIntentToResult(PaymentIntent paymentIntent)
        {
            var status = MapStripeStatusToPaymentStatus(paymentIntent.Status);

            if (status == PaymentStatus.Completed)
            {
                return PaymentResult.Success(
                    status,
                    paymentIntent.Id,
                    paymentIntent.Id,
                    null,
                    new Dictionary<string, string>
                    {
                        ["stripe_payment_intent_id"] = paymentIntent.Id,
                        ["stripe_customer_id"] = paymentIntent.CustomerId ?? string.Empty,
                        ["stripe_status"] = paymentIntent.Status
                    });
            }
            else if (status == PaymentStatus.Failed)
            {
                return PaymentResult.Failure(
                    "Payment failed in Stripe",
                    "STRIPE_PAYMENT_FAILED",
                    new Dictionary<string, string>
                    {
                        ["stripe_payment_intent_id"] = paymentIntent.Id,
                        ["stripe_status"] = paymentIntent.Status
                    });
            }
            else
            {
                return PaymentResult.Success(
                    status,
                    paymentIntent.Id,
                    paymentIntent.Id,
                    null,
                    new Dictionary<string, string>
                    {
                        ["stripe_payment_intent_id"] = paymentIntent.Id,
                        ["stripe_customer_id"] = paymentIntent.CustomerId ?? string.Empty,
                        ["stripe_status"] = paymentIntent.Status
                    });
            }
        }

        /// <summary>
        /// Mapuje powód zwrotu na format Stripe
        /// </summary>
        private static string MapRefundReason(string reason)
        {
            return reason.ToLowerInvariant() switch
            {
                "duplicate" => "duplicate",
                "fraudulent" => "fraudulent",
                "requested_by_customer" => "requested_by_customer",
                _ => "requested_by_customer"
            };
        }

        /// <summary>
        /// Wykonuje operację z retry logic i exponential backoff
        /// </summary>
        private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, string operationName)
        {
            const int maxRetries = 3;
            const int baseDelayMs = 1000;

            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                try
                {
                    return await operation();
                }
                catch (StripeException ex) when (ShouldRetry(ex, attempt, maxRetries))
                {
                    var delay = TimeSpan.FromMilliseconds(baseDelayMs * Math.Pow(2, attempt));
                    _logger.LogWarning("Stripe operation {OperationName} failed (attempt {Attempt}/{MaxRetries}): {Error}. Retrying in {Delay}ms",
                        operationName, attempt + 1, maxRetries + 1, ex.Message, delay.TotalMilliseconds);

                    await Task.Delay(delay);
                }
                catch (StripeException)
                {
                    // Nie próbuj ponownie dla błędów, które nie powinny być retry'owane
                    throw;
                }
            }

            // Ten kod nigdy nie powinien być osiągnięty, ale dodajemy go dla completeness
            throw new InvalidOperationException($"Failed to execute {operationName} after {maxRetries + 1} attempts");
        }

        /// <summary>
        /// Sprawdza czy błąd Stripe powinien być retry'owany
        /// </summary>
        private static bool ShouldRetry(StripeException ex, int currentAttempt, int maxRetries)
        {
            if (currentAttempt >= maxRetries)
                return false;

            // Retry tylko dla tymczasowych błędów
            return (int?)ex.HttpStatusCode switch
            {
                429 => true, // Rate limit
                502 or 503 or 504 => true, // Server errors
                500 when ex.StripeError?.Type == "api_connection_error" => true, // Connection issues
                _ => false
            };
        }

        #endregion

        #region IDisposable

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _rateLimiter?.Dispose();
                }
                _disposed = true;
            }
        }

        #endregion
    }
}