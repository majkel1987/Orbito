# Phase 0.3: Code Metrics — Audit Report

**Data:** 2026-03-22
**Score:** B (Acceptable)
**Status:** Completed (partial fix applied)

---

## Summary

| Metryka         | Wartość   |
| --------------- | --------- |
| Pliki .cs       | 587       |
| Linie kodu      | ~95,000   |
| Critical issues | 0         |
| Major issues    | 1 (FIXED) |
| Minor issues    | 6 (TODO)  |
| Suggestions     | 7         |

---

## Fixed Issues

### [MAJOR] Backup file removed

- **Plik:** `Orbito.Infrastructure/Persistance/PaymentRepository.cs.bak`
- **Status:** USUNIĘTY

---

## Pending TODO Comments (6)

Poniższe TODO komentarze wymagają implementacji logiki biznesowej.

### 1. PaymentReconciliationService.cs:261

**Plik:** `Orbito.Infrastructure/Services/PaymentReconciliationService.cs`
**Linia:** 261

```csharp
// TODO: Get provider email from tenant context
```

**Co zrobić:**

- Wstrzyknąć `ITenantContext` do serwisu (jeśli jeszcze nie ma)
- Pobrać email providera z kontekstu tenanta lub z repozytorium `IProviderRepository`
- Użyć tego emaila do wysyłki powiadomień o reconciliation

**Przykładowa implementacja:**

```csharp
var provider = await _providerRepository.GetByTenantIdAsync(_tenantContext.CurrentTenantId, ct);
var providerEmail = provider?.ContactEmail ?? provider?.User?.Email;
```

---

### 2. PaymentReconciliationService.cs:292

**Plik:** `Orbito.Infrastructure/Services/PaymentReconciliationService.cs`
**Linia:** 292

```csharp
// TODO: Send Slack/Teams webhook notification for critical discrepancies
```

**Co zrobić:**

- Stworzyć interfejs `IWebhookNotificationService` w Application layer
- Zaimplementować w Infrastructure (HTTP client do Slack/Teams webhooks)
- Dodać konfigurację webhook URL w `appsettings.json`
- Wywołać webhook przy wykryciu krytycznych rozbieżności (np. kwota > 1000 PLN)

**Przykładowa implementacja:**

```csharp
// W appsettings.json:
"Webhooks": {
  "SlackUrl": "https://hooks.slack.com/services/...",
  "TeamsUrl": "https://outlook.office.com/webhook/..."
}

// Interface:
public interface IWebhookNotificationService
{
    Task SendCriticalDiscrepancyAlert(PaymentDiscrepancy discrepancy, CancellationToken ct);
}
```

---

### 3. PaymentRetryService.cs:334

**Plik:** `Orbito.Application/Services/PaymentRetryService.cs`
**Linia:** 334

```csharp
// TODO: Implement actual payment retry logic with Stripe
```

**Co zrobić:**

- Wywołać `IPaymentGateway.ChargeAsync()` lub podobną metodę
- Obsłużyć odpowiedź Stripe (sukces/failure)
- Zaktualizować `PaymentRetrySchedule` o wynik próby
- Inkrementować `AttemptCount` i ustawić `NextRetryAt` zgodnie z exponential backoff

**Przykładowa implementacja:**

```csharp
var chargeResult = await _paymentGateway.ChargePaymentMethodAsync(
    payment.PaymentMethodId,
    payment.Amount,
    payment.Currency,
    ct);

if (chargeResult.IsSuccess)
{
    retrySchedule.MarkAsSucceeded();
    payment.MarkAsPaid(chargeResult.TransactionId);
}
else
{
    retrySchedule.RecordFailedAttempt(chargeResult.ErrorMessage);
}
```

---

### 4. PaymentMetricsService.cs:281

**Plik:** `Orbito.Application/Services/PaymentMetricsService.cs`
**Linia:** 281

```csharp
GrowthPercentage = 0, // TODO: Implement growth calculation with previous period
```

**Co zrobić:**

- Pobrać dane z poprzedniego okresu (np. poprzedni miesiąc)
- Obliczyć growth: `((current - previous) / previous) * 100`
- Obsłużyć edge case gdy `previous = 0`

**Przykładowa implementacja:**

```csharp
var previousPeriodStart = periodStart.AddMonths(-1);
var previousPeriodEnd = periodEnd.AddMonths(-1);

var previousRevenue = await _paymentRepository.GetTotalRevenueAsync(
    tenantId, previousPeriodStart, previousPeriodEnd, ct);

var growthPercentage = previousRevenue > 0
    ? ((currentRevenue - previousRevenue) / previousRevenue) * 100
    : (currentRevenue > 0 ? 100 : 0);
```

---

### 5. PaymentMetricsService.cs:286

**Plik:** `Orbito.Application/Services/PaymentMetricsService.cs`
**Linia:** 286

```csharp
MonthlyRecurringRevenue = 0, // TODO: Calculate MRR for subscription-based revenue
```

**Co zrobić:**

- Pobrać wszystkie aktywne subskrypcje
- Zsumować ich miesięczną wartość (uwzględnić billing cycle — roczne / 12)
- Zwrócić jako MRR

**Przykładowa implementacja:**

```csharp
var activeSubscriptions = await _subscriptionRepository
    .GetActiveSubscriptionsAsync(tenantId, ct);

var mrr = activeSubscriptions.Sum(s =>
    s.BillingCycle == BillingCycle.Monthly
        ? s.Price
        : s.Price / 12m);
```

---

### 6. CreateStripeCustomerCommandHandler.cs:65

**Plik:** `Orbito.Application/Features/Payments/Commands/CreateStripeCustomerCommandHandler.cs`
**Linia:** 65

```csharp
// TODO: Implement proper idempotency check by adding StripeCustomerId field to Client entity
```

**Co zrobić:**

1. Dodać pole `StripeCustomerId` do encji `Client` (Domain layer)
2. Dodać migrację EF Core
3. Przed utworzeniem customera w Stripe — sprawdzić czy `Client.StripeCustomerId` już istnieje
4. Jeśli istnieje — zwrócić istniejący ID zamiast tworzyć nowego

**Kroki implementacji:**

**Krok 1 — Domain (Client.cs):**

```csharp
public class Client : BaseEntity
{
    // ... existing properties
    public string? StripeCustomerId { get; private set; }

    public void SetStripeCustomerId(string customerId)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            throw new DomainException("StripeCustomerId cannot be empty");
        StripeCustomerId = customerId;
    }
}
```

**Krok 2 — Migration:**

```bash
dotnet ef migrations add AddStripeCustomerIdToClient -p Orbito.Infrastructure -s Orbito.API
```

**Krok 3 — Handler:**

```csharp
// Check if already exists
if (!string.IsNullOrEmpty(client.StripeCustomerId))
{
    return Result.Success(new CreateStripeCustomerResult(client.StripeCustomerId));
}

// Create in Stripe
var stripeCustomerId = await _stripeService.CreateCustomerAsync(client.Email, ct);

// Save to database
client.SetStripeCustomerId(stripeCustomerId);
await _clientRepository.UpdateAsync(client, ct);
```

---

## Complexity Hotspots (Refactoring Candidates)

Te pliki przekraczają 500 linii i są kandydatami do refaktoryzacji:

| Plik                                                                               | Linie | Problem       | Sugestia                                                                             |
| ---------------------------------------------------------------------------------- | ----- | ------------- | ------------------------------------------------------------------------------------ |
| `Orbito.Application/Services/PaymentProcessingService.cs`                          | 1,169 | God Class     | Rozbić na: PaymentCreationService, PaymentExecutionService, PaymentValidationService |
| `Orbito.Infrastructure/Persistance/PaymentRepository.cs`                           | 975   | Large repo    | Wydzielić PaymentQueryRepository (read) vs PaymentCommandRepository (write)          |
| `Orbito.Infrastructure/PaymentGateways/Stripe/EventHandlers/StripeEventHandler.cs` | 918   | Large handler | Użyć Strategy pattern — osobny handler per event type                                |
| `Orbito.Infrastructure/PaymentGateways/Stripe/StripePaymentGateway.cs`             | 810   | Large gateway | Wydzielić StripeCustomerService, StripeChargeService, StripeRefundService            |
| `Orbito.Application/Services/PaymentNotificationService.cs`                        | 682   | Large service | Wydzielić EmailNotificationService, SmsNotificationService                           |
| `Orbito.Infrastructure/Services/PaymentReconciliationService.cs`                   | 647   | Large service | OK dla teraz, ale monitorować                                                        |

---

## Other Suggestions

### 1. Folder typo: Persistance vs Persistence

**Problem:** Istnieją dwa foldery:

- `Orbito.Infrastructure/Persistance/` (typo)
- `Orbito.Infrastructure/Persistence/` (correct)

**Fix:**

```bash
# Przenieść pliki z Persistance do Persistence
git mv Orbito.Infrastructure/Persistance/* Orbito.Infrastructure/Persistence/
rmdir Orbito.Infrastructure/Persistance
```

### 2. Potential duplicate: StripePaymentGateway

**Problem:** Znaleziono dwa pliki:

- `Orbito.API/Services/StripePaymentGateway.cs` (592 lines)
- `Orbito.Infrastructure/PaymentGateways/Stripe/StripePaymentGateway.cs` (810 lines)

**Do sprawdzenia:**

- Czy oba są używane?
- Który jest aktualny?
- Czy API layer powinien mieć własną implementację?

---

## Next Steps

1. [ ] Zaimplementować 6 TODO (opisane powyżej)
2. [ ] Skonsolidować folder Persistance/Persistence
3. [ ] Sprawdzić duplicate StripePaymentGateway
4. [ ] Rozważyć refaktoryzację PaymentProcessingService (1,169 lines)

---

_Wygenerowano przez backend-audit skill_
