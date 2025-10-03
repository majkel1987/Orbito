Przejrzałem kontrolery PaymentController i WebhookController. Oto analiza z uwagami:
🔴 Krytyczne problemy
WebhookController - Brak użycia MediatR i Command Pattern
csharp// ❌ PROBLEM: Bezpośrednie wywołanie \_webhookProcessor
var processResult = await \_webhookProcessor.ProcessWebhookEventAsync(eventType, payload);

// ✅ POWINNO BYĆ: Użycie ProcessWebhookEventCommand przez MediatR
var command = new ProcessWebhookEventCommand
{
EventType = eventType,
EventId = extractedEventId, // z payload
Payload = payload,
Signature = signature,
Provider = "Stripe"
};
var result = await \_mediator.Send(command);
Dlaczego to problem?

Omijasz walidację FluentValidation
Nie działają MediatR behaviors (logging, validation pipeline)
Niespójna architektura - reszta aplikacji używa CQRS/MediatR

WebhookController - Brak EventId i idempotencji
csharp// ❌ Nigdzie nie wyciągasz EventId z payload
// Stripe zawsze wysyła 'id' w JSONie
// Bez tego nie działa idempotencja z UpdatePaymentFromWebhookCommand
WebhookController - Brak obsługi błędów zgodnej z webhook best practices
csharp// ❌ Zwracasz 500 przy błędzie procesowania
// Stripe będzie retry'ować w nieskończoność

// ✅ Powinieneś:
// - 200 OK dla zdarzeń już przetworzonych (idempotencja)
// - 200 OK dla nieznanych event types (żeby nie retry'ować)
// - 500 tylko dla rzeczywistych błędów infrastruktury
⚠️ Ważne problemy
PaymentController
csharp// 1. Niespójność w sprawdzaniu result
if (!result.Success) // linia 37
if (!result.IsSuccess) // linia 141

// Zdecyduj się na jedną konwencję

// 2. Brak rate limiting
// Webhooks mogą być atakowane - dodaj [RateLimit]

// 3. CreatedAtAction bez właściwej implementacji
return CreatedAtAction(nameof(GetPaymentById), new { id = result.Payment!.Id }, result);
// ⚠️ Używasz result.Payment!.Id - null-forgiving operator
// Lepiej: sprawdź najpierw czy Payment != null

// 4. Inconsistency w response types
return Ok(result); // zwraca cały Result object
return Ok(result.Value); // zwraca tylko Value
// Wybierz jedną strategię
WebhookController - Brak autoryzacji
csharp// ❌ Brak [Authorize] - OK dla webhooków
// Ale powinieneś mieć rate limiting i IP whitelisting

// ✅ Dodaj middleware lub attribute:
[ServiceFilter(typeof(WebhookIpWhitelistFilter))]
[RateLimit(PermitLimit = 100, Window = 1, QueueLimit = 0)]
📋 Szczegółowe rekomendacje

1.  Przepisz WebhookController na MediatR
    csharp[HttpPost("stripe")]
    public async Task<IActionResult> HandleStripeWebhook()
    {
    try
    {
    using var reader = new StreamReader(Request.Body, Encoding.UTF8);
    var payload = await reader.ReadToEndAsync();

            var signature = Request.Headers["Stripe-Signature"].FirstOrDefault();
            if (string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(payload))
            {
                return BadRequest("Invalid request");
            }

            // Parse Stripe event to get ID and type
            var stripeEvent = ParseStripeEvent(payload);
            if (stripeEvent == null)
            {
                return BadRequest("Invalid payload format");
            }

            var command = new ProcessWebhookEventCommand
            {
                EventId = stripeEvent.Id,
                EventType = stripeEvent.Type,
                Payload = payload,
                Signature = signature,
                Provider = "Stripe"
            };

            var result = await _mediator.Send(command);

            // Webhook best practice: zawsze zwracaj 200 jeśli request był OK
            if (!result.IsSuccess)
            {
                // Log error ale zwróć 200 dla known errors
                if (IsKnownError(result.ErrorMessage))
                {
                    _logger.LogWarning("Known error processing webhook: {Error}", result.ErrorMessage);
                    return Ok(new { received = true, processed = false, reason = "already_processed_or_known_error" });
                }

                // 500 tylko dla unexpected errors
                _logger.LogError("Unexpected error processing webhook: {Error}", result.ErrorMessage);
                return StatusCode(500);
            }

            return Ok(new { received = true, processed = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Stripe webhook");
            return StatusCode(500);
        }

    }

private StripeEventInfo? ParseStripeEvent(string payload)
{
try
{
var json = JsonDocument.Parse(payload);
return new StripeEventInfo
{
Id = json.RootElement.GetProperty("id").GetString() ?? "",
Type = json.RootElement.GetProperty("type").GetString() ?? ""
};
}
catch
{
return null;
}
}

private bool IsKnownError(string? errorMessage)
{
if (string.IsNullOrEmpty(errorMessage)) return false;

    var knownErrors = new[]
    {
        "already processed",
        "Invalid webhook signature",
        "Unknown event type"
    };

    return knownErrors.Any(e => errorMessage.Contains(e, StringComparison.OrdinalIgnoreCase));

} 2. Popraw PaymentController consistency
csharp// Wybierz jedną konwencję dla wszystkich endpoints:

// Opcja A - Zawsze zwracaj Value
[HttpPost("payment-methods")]
public async Task<ActionResult<SavePaymentMethodResult>> SavePaymentMethod(
[FromBody] SavePaymentMethodCommand command)
{
var result = await \_mediator.Send(command);

    if (!result.IsSuccess)
    {
        return BadRequest(new { error = result.ErrorMessage });
    }

    return CreatedAtAction(
        nameof(GetPaymentMethodsByClient),
        new { clientId = command.ClientId },
        result.Value);

}

// Opcja B - Zawsze zwracaj cały Result (moja rekomendacja)
[HttpPost("payment-methods")]
public async Task<ActionResult<Result<SavePaymentMethodResult>>> SavePaymentMethod(
[FromBody] SavePaymentMethodCommand command)
{
var result = await \_mediator.Send(command);

    if (!result.IsSuccess)
    {
        return BadRequest(result);
    }

    return Created($"api/payment/payment-methods/client/{command.ClientId}", result);

} 3. Dodaj Global Exception Handler
csharp// Zamiast try-catch w każdym kontrolerze
public class GlobalExceptionHandler : IExceptionHandler
{
private readonly ILogger<GlobalExceptionHandler> \_logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception occurred");

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An error occurred",
            Detail = exception.Message
        };

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

}

// W Program.cs
builder.Services.AddExceptionHandler<GlobalExceptionHandler>(); 4. Dodaj API Response wrapper
csharppublic class ApiResponse<T>
{
public bool Success { get; set; }
public T? Data { get; set; }
public string? Error { get; set; }
public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

// Użycie w kontrolerze
return Ok(new ApiResponse<SavePaymentMethodResult>
{
Success = true,
Data = result.Value
});
🔒 Bezpieczeństwo
Krytyczne dla WebhookController:
csharp// 1. IP Whitelist dla Stripe
// https://stripe.com/docs/ips
private static readonly string[] StripeIPs =
{
"3.18.12.0/24",
"3.130.192.0/24",
// ... reszta IP Stripe
};

// 2. Rate limiting
[EnableRateLimiting("webhook")]

// 3. Request size limit
[RequestSizeLimit(1048576)] // 1MB max

// 4. Timeout
[RequestTimeout(30000)] // 30 sekund max
📊 Priorytety
Krytyczne (przed production):

Przepisz WebhookController na MediatR
Dodaj wyciąganie EventId z payload
Popraw response codes w webhook (200 dla known errors)
Zunifikuj Result.IsSuccess vs Result.Success

Wysokie: 5. Dodaj rate limiting dla webhooków 6. Global exception handler 7. IP whitelisting dla Stripe
Średnie: 8. API response wrapper dla spójności 9. Null-safe CreatedAtAction
Niskie: 10. Swagger documentation 11. Health checks
