# Plan Implementacji Systemu Notyfikacji i Zarządzania Metodami Płatności

## Analiza obecnego stanu

✅ **Już zaimplementowane:**

- `PaymentMethod` entity z metodami `SetAsDefault()`, `IsExpired()`, `CanBeUsed()`
- `IPaymentMethodRepository` + `PaymentMethodRepository` z pełną funkcjonalnością
- `GetPaymentMethodsByClientQuery` - pobieranie metod płatności
- `ISecurityLimitService` - limity bezpieczeństwa
- Background jobs (`ProcessRecurringPaymentsJob`)
- Domain events: `PaymentCompletedEvent`, `PaymentFailedEvent`, `PaymentRefundedEvent`

## Zadania do wykonania

### 1. VALUE OBJECTS (Domain)

- **CardDetails.cs** - Value Object dla szczegółów karty (Brand, ExpiryMonth, ExpiryYear)
- **Aktualizacja PaymentMethod** - dodanie CardDetails jako właściwości

### 2. NOTIFICATION SERVICE (Application/Services)

- **IPaymentNotificationService.cs** - interfejs w Application/Common/Interfaces
- **PaymentNotificationService.cs** - implementacja w Application/Services
  - `SendPaymentConfirmationAsync(paymentId)`
  - `SendPaymentFailureNotificationAsync(paymentId, reason)`
  - `SendRefundConfirmationAsync(refundId, amount)`
  - `SendUpcomingPaymentReminderAsync(subscriptionId, daysUntilPayment)`
  - `SendExpiredCardNotificationAsync(paymentMethodId)`
- **PaymentEmailTemplates.cs** - szablony email w Application/Services/Templates

### 3. PAYMENT METHOD COMMANDS (Application/Features/PaymentMethods)

- **Commands/AddPaymentMethodCommand.cs** + Handler + Validator
- **Commands/SetDefaultPaymentMethodCommand.cs** + Handler + Validator
- **Commands/RemovePaymentMethodCommand.cs** + Handler + Validator

### 4. PAYMENT METHOD QUERIES (Application/Features/PaymentMethods)

- **Queries/GetDefaultPaymentMethodQuery.cs** + Handler
- Aktualizacja istniejącego `GetPaymentMethodsByClientQuery` (przeniesienie do PaymentMethods)

### 5. PAYMENT METHOD CONTROLLER (API)

- **PaymentMethodController.cs** w Orbito.API/Controllers
  - GET `/api/payment-methods/client/{clientId}` - lista metod płatności
  - GET `/api/payment-methods/{id}` - szczegóły metody
  - GET `/api/payment-methods/client/{clientId}/default` - domyślna metoda
  - POST `/api/payment-methods` - dodanie metody
  - PUT `/api/payment-methods/{id}/set-default` - ustawienie jako domyślna
  - DELETE `/api/payment-methods/{id}` - usunięcie metody

### 6. INTEGRACJA Z PaymentProcessingService

- **UseDefaultPaymentMethodAsync(clientId)** - użycie domyślnej metody
- **ValidatePaymentMethodAsync(paymentMethodId)** - walidacja metody

### 7. BACKGROUND JOBS (Application/BackgroundJobs)

- **UpcomingPaymentReminderJob.cs** - przypomnienia 3 dni przed płatnością
- **ExpiredCardNotificationJob.cs** - powiadomienia o wygasłych kartach

### 8. DOMAIN EVENTS

- **PaymentMethodAddedEvent.cs** - dodanie metody płatności
- **PaymentMethodRemovedEvent.cs** - usunięcie metody płatności
- **PaymentMethodSetAsDefaultEvent.cs** - ustawienie jako domyślna

## Zasady implementacji

✅ Multi-tenancy z `ITenantContext`
✅ Repository security pattern - wszystkie metody z weryfikacją `clientId`
✅ FluentValidation dla wszystkich commands
✅ Rate limiting przez `ISecurityLimitService`
✅ Structured logging z Serilog
✅ Result pattern dla error handling
✅ Clean Architecture + CQRS + DDD

## Kolejność implementacji

1. Value Objects (CardDetails)
2. Domain Events
3. Notification Service (interfejs + implementacja)
4. Payment Method Commands/Queries
5. PaymentMethodController
6. Integracja z PaymentProcessingService
7. Background Jobs
