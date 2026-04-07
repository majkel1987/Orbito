# Phase 1.1: Domain Layer Entities — Audit Report

**Data:** 2026-03-22
**Score:** B (Acceptable)
**Status:** Completed

---

## Summary

| Metryka         | Wartość |
| --------------- | ------- |
| Pliki .cs       | 15      |
| Critical issues | 0       |
| Major issues    | 3       |
| Minor issues    | 8       |
| Suggestions     | 5       |

---

## Major Issues

### 1. [MAJOR] Public Setters — Multiple Entities

Większość encji ma public setters zamiast private setters, co narusza enkapsulację DDD.

**Dotknięte pliki:**

| Plik                    | Properties                                                         |
| ----------------------- | ------------------------------------------------------------------ |
| `Client.cs:12-43`       | `Id`, `TenantId`, `UserId`, `CompanyName`, `Phone`, `Status`, etc. |
| `EmailNotification.cs`  | `Id`, `TenantId`, `Type`, `RecipientEmail`, `Subject`, `Body`      |
| `Payment.cs`            | `Id`, `TenantId`, `SubscriptionId`, `ClientId`, `Amount`, `Status` |
| `PaymentDiscrepancy.cs` | `Id`, `TenantId`, `ReconciliationReportId`, wszystkie properties   |
| `PaymentHistory.cs`     | `Id`, `TenantId`, `PaymentId`, `Action`, `Status`                  |
| `PaymentMethod.cs`      | `Id`, `TenantId`, `ClientId`, `Type`, `Token`                      |
| `PaymentWebhookLog.cs`  | `Id`, `TenantId`, `EventId`, `Provider`, `EventType`               |
| `ReconciliationReport.cs` | `Id`, `TenantId`, `RunDate`, `PeriodStart`                       |
| `Subscription.cs`       | `Id`, `TenantId`, `ClientId`, `PlanId`, `Status`                   |
| `SubscriptionPlan.cs`   | `Id`, `TenantId`, `Name`, `Description`, `Price`                   |

**Co zrobić:**

Zmień `{ get; set; }` na `{ get; private set; }` dla wszystkich properties.

```csharp
// Przed
public Guid Id { get; set; }
public string Name { get; set; }

// Po
public Guid Id { get; private set; }
public string Name { get; private set; } = string.Empty;
```

**Wyjątki (dobrze zrobione):**
- `PaymentRetrySchedule.cs` — używa `private set` poprawnie
- `ProviderSubscription.cs` — używa `private set` poprawnie
- `PlatformPlan.cs` — używa `private set` poprawnie
- `TeamMember.cs` — używa `private set` (ale ma inne problemy)

---

### 2. [MAJOR] Navigation Properties Not Readonly

Navigation properties używają mutowalnych kolekcji ICollection zamiast IReadOnlyCollection.

**Dotknięte pliki:**

| Plik               | Linia | Property                                   |
| ------------------ | ----- | ------------------------------------------ |
| `Client.cs`        | 41-43 | `Subscriptions`, `Payments`, `PaymentMethods` |
| `Provider.cs`      | 39-41 | `Plans`, `Clients`, `Subscriptions`        |
| `SubscriptionPlan.cs` | 33 | `Subscriptions`                            |
| `Subscription.cs`  | 39    | `Payments`                                 |
| `PlatformPlan.cs`  | 25    | `ProviderSubscriptions`                    |
| `ReconciliationReport.cs` | 36 | `Discrepancies`                         |

**Co zrobić:**

```csharp
// Przed
public ICollection<Subscription> Subscriptions { get; set; } = [];

// Po (jeśli nie trzeba dodawać z zewnątrz)
private readonly List<Subscription> _subscriptions = [];
public IReadOnlyCollection<Subscription> Subscriptions => _subscriptions.AsReadOnly();

// Lub (jeśli EF Core wymaga ICollection)
public ICollection<Subscription> Subscriptions { get; private set; } = [];
```

---

### 3. [MAJOR] TeamMember Missing IMustHaveTenant

`TeamMember.cs` ma property `TenantId`, ale nie implementuje interfejsu `IMustHaveTenant`.

**Plik:** `Orbito.Domain/Entities/TeamMember.cs`

**Co zrobić:**

```csharp
// Przed
public class TeamMember
{
    public TenantId TenantId { get; private set; }

// Po
public class TeamMember : IMustHaveTenant
{
    public TenantId TenantId { get; private set; }
```

---

## Minor Issues

### 1. [MINOR] Exception Instead of Result — Client.cs:177-179

```csharp
if (UserId != null)
    throw new InvalidOperationException("Cannot update direct info...");
```

**Co zrobić:**

```csharp
public Result UpdateDirectInfo(string? email, string? firstName, string? lastName)
{
    if (UserId != null)
        return Result.Failure(DomainErrors.Client.CannotUpdateDirectInfoWithAccount);
    // ...
    return Result.Success();
}
```

---

### 2. [MINOR] Exception Instead of Result — Provider.cs:66-67

```csharp
if (revenue.Currency != MonthlyRevenue.Currency)
    throw new InvalidOperationException("Currency mismatch");
```

---

### 3. [MINOR] Exception Instead of Result — Subscription.cs:127-131

```csharp
if (newPlanId == Guid.Empty)
    throw new ArgumentException("Plan ID cannot be empty", nameof(newPlanId));
```

---

### 4. [MINOR] Exception Instead of Result — Subscription.cs:187-188

```csharp
if (!CanBePaid())
    throw new InvalidOperationException("Subscription cannot be paid in current status");
```

---

### 5. [MINOR] Magic Strings — EmailNotification.cs

**Plik:** `Orbito.Domain/Entities/EmailNotification.cs`
**Linie:** 19, 48, 62, 72, 89

```csharp
public string Status { get; set; } = "Pending";
Status = "Pending";
Status = "Processed";
Status = "Failed";
```

**Co zrobić:**

1. Dodaj enum w `Orbito.Domain/Enums/EmailNotificationStatus.cs`:

```csharp
public enum EmailNotificationStatus
{
    Pending = 1,
    Processing = 2,
    Processed = 3,
    Failed = 4
}
```

2. Zmień property w EmailNotification.cs:

```csharp
public EmailNotificationStatus Status { get; private set; } = EmailNotificationStatus.Pending;
```

---

### 6. [MINOR] Duplicated Cancel Methods — Payment.cs:138-148

```csharp
public void MarkAsCancelled() { ... }

public void MarkAsCanceled()
{
    MarkAsCancelled(); // Alias dla amerykańskiej pisowni
}
```

**Co zrobić:**

Usuń `MarkAsCanceled()` i użyj tylko jednej wersji (British English: `Cancelled`).

---

### 7. [MINOR] Inconsistent Timestamps — TeamMember.cs:251-256

```csharp
public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
```

Wszystkie inne encje używają `DateTime`, nie `DateTimeOffset`.

**Co zrobić:**

Zmień na `DateTime` dla spójności:

```csharp
public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
public DateTime? UpdatedAt { get; private set; }
```

---

### 8. [MINOR] Public Setter on Id — TeamMember.cs:50

```csharp
public Guid Id { get; set; }
```

**Co zrobić:**

```csharp
public Guid Id { get; private set; } = Guid.NewGuid();
```

I ustaw w konstruktorze.

---

## Suggestions

### 1. Domain Events Missing

Tylko `Payment.cs` ma implementację Domain Events. Inne encje mogłyby też emitować eventy:

- `Client.cs` → `ClientCreatedEvent`, `ClientDeactivatedEvent`
- `Subscription.cs` → `SubscriptionCancelledEvent`, `SubscriptionActivatedEvent`
- `Provider.cs` → `ProviderCreatedEvent`

---

### 2. Missing Validation in Factory Methods

Niektóre factory methods nie walidują wszystkich parametrów:

| Plik                     | Metoda             | Brakująca walidacja         |
| ------------------------ | ------------------ | --------------------------- |
| `Client.CreateWithUser()` | brak walidacji    | `tenantId` null check       |
| `EmailNotification.Create()` | brak walidacji | email format                |
| `SubscriptionPlan.Create()` | brak walidacji  | `name` empty check          |

---

### 3. TrialDays vs TrialPeriodDays — SubscriptionPlan.cs

```csharp
public int TrialDays { get; set; }      // Linia 17
public int TrialPeriodDays { get; set; } // Linia 24
```

Dwa podobne pola — rozważyć usunięcie jednego.

---

### 4. Subscription Methods Could Return Result

Metody `Cancel()`, `Activate()`, `Suspend()`, `Resume()` w `Subscription.cs` mogłyby zwracać `Result`:

```csharp
public Result Cancel()
{
    if (Status == SubscriptionStatus.Cancelled)
        return Result.Failure(DomainErrors.Subscription.AlreadyCancelled);

    Status = SubscriptionStatus.Cancelled;
    CancelledAt = DateTime.UtcNow;
    return Result.Success();
}
```

---

### 5. Good Patterns to Follow

Te encje mają bardzo dobrą implementację i mogą służyć jako wzorzec:

| Encja                    | Co jest dobre                                    |
| ------------------------ | ------------------------------------------------ |
| `Payment.cs`             | Domain events, state machine, validation         |
| `PaymentRetrySchedule.cs` | Private setters, full encapsulation             |
| `ProviderSubscription.cs` | Result pattern, computed properties             |

---

## Next Steps

1. [ ] Zmień public setters na private setters w 10 encjach
2. [ ] Dodaj `IMustHaveTenant` do `TeamMember.cs`
3. [ ] Zamień magic strings na enum w `EmailNotification.cs`
4. [ ] Zamień exceptions na Result pattern w metodach biznesowych
5. [ ] Ujednolić DateTime vs DateTimeOffset

---

_Wygenerowano przez backend-audit skill_
