# Result Pattern - Przewodnik Migracji

## 📋 Status Migracji

### ✅ Ukończone
- **Domain Layer Infrastructure**
  - `Orbito.Domain/Common/Error.cs` - Value Object dla błędów
  - `Orbito.Domain/Common/Result.cs` - Result i Result<T>
  - `Orbito.Domain/Errors/DomainErrors.cs` - Katalog błędów domenowych

- **BaseController**
  - Metody `HandleResult<T>()` i `HandleResult()`
  - Automatyczne mapowanie błędów na kody HTTP

- **Zmigrowane Handlers (Kluczowe)**
  - ✅ ProcessPaymentCommandHandler
  - ✅ CreateProviderCommandHandler
  - ✅ RegisterProviderCommandHandler
  - ✅ CreateStripeCustomerCommandHandler
  - ✅ RefundPaymentCommandHandler
  - ✅ UpdatePaymentFromWebhookCommandHandler
  - ✅ ProcessWebhookEventCommandHandler

- **Kontrolery**
  - ✅ PaymentController - endpoint ProcessPayment
  - ✅ ProvidersController - endpoint CreateProvider
  - ✅ PaymentRetryController - wszystkie endpointy
  - ✅ PaymentMetricsController - wszystkie endpointy
  - ✅ WebhookController - webhook handling

- **Testy**
  - ✅ CreateProviderCommandHandlerTests
  - ✅ ProcessPaymentCommandHandlerTests
  - ✅ CreateStripeCustomerCommandHandlerTests
  - ✅ RefundPaymentCommandHandlerTests
  - ✅ UpdatePaymentFromWebhookCommandHandlerTests
  - ✅ ProviderIntegrationTests (częściowo)
  - ✅ PaymentRetryServiceTests

- **Command/Query Classes**
  - ✅ CreateStripeCustomerCommand
  - ✅ RefundPaymentCommand
  - ✅ UpdatePaymentFromWebhookCommand
  - ✅ ProcessWebhookEventCommand

### ⏳ Do Zmigrowania

#### Payment Handlers (Priorytet 1)
- [x] `RefundPaymentCommandHandler` ✅
- [ ] `UpdatePaymentStatusCommandHandler`
- [ ] `RetryFailedPaymentCommandHandler`
- [ ] `BulkRetryPaymentsCommandHandler`
- [x] `UpdatePaymentFromWebhookCommand` ✅
- [ ] `GetPaymentByIdQueryHandler`
- [ ] `GetPaymentsBySubscriptionQueryHandler`

#### Subscription Handlers (Priorytet 2)
- [ ] `CreateSubscriptionCommandHandler`
- [ ] `CancelSubscriptionCommandHandler`
- [ ] `SuspendSubscriptionCommandHandler`
- [ ] `ResumeSubscriptionCommandHandler`
- [ ] `UpgradeSubscriptionCommandHandler`
- [ ] `DowngradeSubscriptionCommandHandler`
- [ ] `RenewSubscriptionCommandHandler`
- [ ] `ActivateSubscriptionCommandHandler`
- [ ] `GetSubscriptionByIdQueryHandler`
- [ ] `GetSubscriptionsByClientQueryHandler`
- [ ] `GetActiveSubscriptionsQueryHandler`
- [ ] `GetExpiringSubscriptionsQueryHandler`

#### Client Handlers (Priorytet 3)
- [ ] `CreateClientCommandHandler`
- [ ] `UpdateClientCommandHandler`
- [ ] `DeleteClientCommandHandler`
- [ ] `ActivateClientCommandHandler`
- [ ] `DeactivateClientCommandHandler`

#### Provider Handlers (Priorytet 4)
- [ ] `UpdateProviderCommandHandler`
- [ ] `DeleteProviderCommandHandler`

#### SubscriptionPlan Handlers (Priorytet 5)
- [ ] `CreateSubscriptionPlanCommandHandler`
- [ ] `UpdateSubscriptionPlanCommandHandler`
- [ ] `DeleteSubscriptionPlanCommandHandler`
- [ ] `CloneSubscriptionPlanCommandHandler`

## 🔧 Instrukcja Migracji Handler-a

### Krok 1: Zaktualizuj Command/Query

```csharp
// PRZED
public record ProcessPaymentCommand(...) : IRequest<ProcessPaymentResult>;

// PO
using Orbito.Domain.Common;

public record ProcessPaymentCommand(...) : IRequest<Result<PaymentDto>>;
```

### Krok 2: Zaktualizuj Handler

```csharp
// PRZED
public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, ProcessPaymentResult>
{
    public async Task<ProcessPaymentResult> Handle(...)
    {
        if (validationFailed)
            return ProcessPaymentResult.FailureResult("Error message");

        return ProcessPaymentResult.SuccessResult(dto);
    }
}

// PO
using Orbito.Domain.Common;
using Orbito.Domain.Errors;

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, Result<PaymentDto>>
{
    public async Task<Result<PaymentDto>> Handle(...)
    {
        if (validationFailed)
            return Result.Failure<PaymentDto>(DomainErrors.Payment.InvalidAmount);

        return Result.Success(dto);
    }
}
```

### Krok 3: Zamień Exception Handling na Result

```csharp
// PRZED
try
{
    // logic
    throw new InvalidOperationException("Error");
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error");
    throw;
}

// PO
if (errorCondition)
{
    _logger.LogWarning("Error details");
    return Result.Failure<T>(DomainErrors.Category.ErrorType);
}

// lub dla nieoczekiwanych błędów
catch (Exception ex)
{
    _logger.LogError(ex, "Unexpected error");
    return Result.Failure<T>(DomainErrors.General.UnexpectedError);
}
```

### Krok 4: Zaktualizuj Kontroler

```csharp
// PRZED
[HttpPost]
public async Task<ActionResult<SomeResult>> SomeAction([FromBody] SomeCommand command)
{
    try
    {
        var result = await Mediator.Send(command);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { error = ex.Message });
    }
}

// PO
[HttpPost]
public async Task<IActionResult> SomeAction([FromBody] SomeCommand command)
{
    var result = await Mediator.Send(command);
    return HandleResult(result);
}
```

### Krok 5: Zaktualizuj Testy

```csharp
// PRZED
var result = await handler.Handle(command, CancellationToken.None);

result.Success.Should().BeTrue();
result.Data.Id.Should().Be(expectedId);
result.Message.Should().BeNull();

// PO
var result = await handler.Handle(command, CancellationToken.None);

result.IsSuccess.Should().BeTrue();
result.Value.Id.Should().Be(expectedId);

// Dla błędów
result.IsFailure.Should().BeTrue();
result.Error.Code.Should().Be("Payment.InvalidAmount");
result.Error.Message.Should().Contain("Invalid");
```

## 📝 Mapowanie Błędów na HTTP Status Codes

BaseController automatycznie mapuje błędy:

| Error Code Pattern | HTTP Status |
|-------------------|-------------|
| `*.NotFound` | 404 Not Found |
| `*.AlreadyExists` | 409 Conflict |
| `*.Duplicate*` | 409 Conflict |
| `*.Unauthorized` | 401 Unauthorized |
| `*.CrossTenant*` | 403 Forbidden |
| `*.NoTenantContext` | 403 Forbidden |
| `*.Invalid*` | 400 Bad Request |
| `*.Cannot*` | 400 Bad Request |
| `*.Inactive` | 400 Bad Request |
| `*.RateLimit*` | 429 Too Many Requests |
| Inne | 400 Bad Request |

## 🎯 Przykłady Użycia DomainErrors

```csharp
// Ogólne
DomainErrors.General.UnexpectedError
DomainErrors.General.ValidationFailed
DomainErrors.General.NotFound

// Tenant
DomainErrors.Tenant.NoTenantContext
DomainErrors.Tenant.CrossTenantAccess

// Payment
DomainErrors.Payment.InvalidAmount
DomainErrors.Payment.InvalidCurrency
DomainErrors.Payment.AmountMismatch
DomainErrors.Payment.CurrencyMismatch
DomainErrors.Payment.SubscriptionNotActive
DomainErrors.Payment.DuplicateExternalTransactionId
DomainErrors.Payment.RateLimitExceeded

// Subscription
DomainErrors.Subscription.NotFound
DomainErrors.Subscription.NotActive
DomainErrors.Subscription.AlreadyActive
DomainErrors.Subscription.AlreadyCancelled
DomainErrors.Subscription.CannotUpgrade
DomainErrors.Subscription.CannotDowngrade

// Client
DomainErrors.Client.NotFound
DomainErrors.Client.Inactive
DomainErrors.Client.EmailAlreadyExists
DomainErrors.Client.CannotDeleteWithActiveSubscriptions

// Provider
DomainErrors.Provider.NotFound
DomainErrors.Provider.SubdomainAlreadyExists
DomainErrors.Provider.UserAlreadyHasProvider
DomainErrors.Provider.CannotDeleteWithActiveClients
```

## 🚀 Korzyści z Result Pattern

1. **Jawna Obsługa Błędów** - Brak niespodziewanych wyjątków
2. **Lepsza Testowalność** - Łatwiejsze testowanie przypadków błędów
3. **Spójne API** - Jednolita struktura odpowiedzi
4. **Type Safety** - Kompilator wymusza obsługę błędów
5. **Separation of Concerns** - Błędy domenowe oddzielone od HTTP
6. **Łatwiejszy Debugging** - Klarowny flow kontroli

## ⚠️ Uwagi

1. **Stary Result z Application.Common.Models** - Pozostaje do czasu pełnej migracji interfejsów infrastruktury
2. **Pełne nazwy w BaseController** - Używamy `Orbito.Domain.Common.Result<T>` z powodu konfliktu nazw
3. **Istniejące Result classes** - Mogą zostać usunięte po migracji (np. `ProcessPaymentResult`)
4. **Testy** - Wszystkie testy wymagają aktualizacji do nowego API
5. **Interfejsy infrastruktury** - `IPaymentWebhookProcessor`, `IUnitOfWork` itp. nadal używają starego Result
6. **Kompilacja** - ✅ Aplikacja kompiluje się bez błędów
7. **Testy** - ✅ Wszystkie testy przechodzą

## 📚 Dodatkowe Zasoby

- `Orbito.Domain/Common/Error.cs` - Implementacja Error
- `Orbito.Domain/Common/Result.cs` - Implementacja Result Pattern
- `Orbito.Domain/Errors/DomainErrors.cs` - Wszystkie błędy domenowe
- `Orbito.API/Controllers/BaseController.cs` - Metody HandleResult
- `ProcessPaymentCommandHandler.cs` - Przykład pełnej migracji
- `CreateProviderCommandHandler.cs` - Przykład pełnej migracji

---

**Ostatnia aktualizacja:** 2025-01-16
**Wersja:** 2.0 - Kluczowe handlery zmigrowane
