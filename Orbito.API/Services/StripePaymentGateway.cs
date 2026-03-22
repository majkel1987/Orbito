using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models.PaymentGateway;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Stripe;
using System.Text.Json;

namespace Orbito.API.Services
{
    /// <summary>
    /// Konfiguracja Stripe payment gateway
    /// </summary>
    public class StripeConfiguration
    {
        /// <summary>
        /// Klucz API Stripe (Secret Key)
        /// </summary>
        public string SecretKey { get; set; } = string.Empty;

        /// <summary>
        /// Klucz publiczny Stripe (Publishable Key)
        /// </summary>
        public string PublishableKey { get; set; } = string.Empty;

        /// <summary>
        /// Webhook secret dla walidacji webhooków
        /// </summary>
        public string WebhookSecret { get; set; } = string.Empty;

        /// <summary>
        /// Środowisko Stripe (test/live)
        /// </summary>
        public string Environment { get; set; } = "test";
    }

    /// <summary>
    /// Implementacja IPaymentGateway dla Stripe
    /// </summary>
    public class StripePaymentGateway : IPaymentGateway
    {
        private readonly StripeConfiguration _configuration;
        private readonly ILogger<StripePaymentGateway> _logger;

        public StripePaymentGateway(
            IOptions<StripeConfiguration> configuration,
            ILogger<StripePaymentGateway> logger)
        {
            _configuration = configuration.Value;
            _logger = logger;

            // Konfiguracja Stripe SDK
            if (!string.IsNullOrEmpty(_configuration.SecretKey))
            {
                // W nowszej wersji Stripe SDK konfiguracja jest przekazywana do każdego requestu
                // StripeConfiguration.ApiKey = _configuration.SecretKey;
            }
        }

        /// <summary>
        /// Przetwarza płatność przez Stripe
        /// </summary>
        public async Task<PaymentResult> ProcessPaymentAsync(ProcessPaymentRequest request)
        {
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

                var paymentIntent = await paymentIntentService.CreateAsync(options);

                // Potwierdź płatność jeśli payment method jest dostępny
                if (!string.IsNullOrEmpty(request.PaymentMethodId))
                {
                    var confirmOptions = new PaymentIntentConfirmOptions
                    {
                        PaymentMethod = request.PaymentMethodId
                    };

                    paymentIntent = await paymentIntentService.ConfirmAsync(paymentIntent.Id, confirmOptions);
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
        }

        /// <summary>
        /// Zwraca płatność przez Stripe
        /// </summary>
        public async Task<RefundResult> RefundPaymentAsync(RefundRequest request)
        {
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

                var refund = await refundService.CreateAsync(options);

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
        }

        /// <summary>
        /// Tworzy klienta w Stripe
        /// </summary>
        public async Task<CustomerResult> CreateCustomerAsync(CreateCustomerRequest request)
        {
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

                var customer = await customerService.CreateAsync(options);

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
        }

        /// <summary>
        /// Creates a PaymentIntent for Stripe Elements (PCI DSS compliant)
        /// </summary>
        public async Task<CreatePaymentIntentResult> CreatePaymentIntentAsync(CreatePaymentIntentRequest request)
        {
            try
            {
                _logger.LogInformation(
                    "Creating PaymentIntent for subscription {SubscriptionId}, amount {Amount} {Currency}",
                    request.SubscriptionId, request.Amount.Amount, request.Amount.Currency.Code);

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
                        ["subscription_type"] = "client"
                    }
                };

                foreach (var metadata in request.Metadata)
                {
                    options.Metadata[metadata.Key] = metadata.Value;
                }

                var paymentIntent = await paymentIntentService.CreateAsync(options);

                _logger.LogInformation(
                    "PaymentIntent {PaymentIntentId} created for subscription {SubscriptionId}",
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
                    "Unexpected error creating PaymentIntent for subscription {SubscriptionId}",
                    request.SubscriptionId);

                return CreatePaymentIntentResult.Failure(
                    "An unexpected error occurred while creating payment intent",
                    "UNEXPECTED_ERROR");
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
        private Task<string?> GetOrCreateStripeCustomerAsync(ProcessPaymentRequest request)
        {
            try
            {
                // Tutaj można dodać logikę wyszukiwania istniejącego klienta
                // Na razie zwracamy null - klient będzie utworzony przez Stripe
                return Task.FromResult<string?>(null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting or creating Stripe customer for client {ClientId}", request.ClientId);
                return Task.FromResult<string?>(null);
            }
        }

        /// <summary>
        /// Konwertuje kwotę na format Stripe (centy)
        /// </summary>
        private static long ConvertToStripeAmount(Money amount)
        {
            return (long)(amount.Amount * 100); // Stripe używa centów
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

        #endregion
    }
}
