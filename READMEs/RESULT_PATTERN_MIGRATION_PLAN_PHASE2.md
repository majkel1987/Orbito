# Result Pattern - Plan Migracji Faza 2

**Progress:** Ukończone 38/38 handlerów w Fazie 2 🎉🎉🎉

## 📊 Status Ogólny

### ✅ Ukończone (Faza 1 - Payments)

- Wszystkie Payment handlers (ProcessPayment, Retry, Refund, GetPaymentById, etc.)
- PaymentController endpoints
- PaymentRetryController
- PaymentMetricsController
- WebhookController
- Podstawowe Provider handlers (Create, Register)

### 🚧 W Trakcie (Faza 2A - Subscription Handlers)

- ✅ CreateSubscriptionCommandHandler - UKOŃCZONE (2025-01-29)
  - Handler zmigrowany do Result Pattern
  - 5 testów zaktualizowanych
  - Kontroler zaktualizowany
  - CreateSubscriptionResult usunięty
  - SubscriptionDto utworzony
- ✅ CancelSubscriptionCommandHandler - UKOŃCZONE (2025-01-29)
  - Handler zmigrowany do Result Pattern (z SuccessResult/FailureResult → Result.Success/Failure)
  - 7 testów zaktualizowanych
  - Kontroler zaktualizowany (dodano ClientId do request DTO)
  - CancelSubscriptionResult usunięty
- ✅ SuspendSubscriptionCommandHandler - UKOŃCZONE (2025-01-29)
  - Handler zmigrowany do Result Pattern (z SuccessResult/FailureResult → Result.Success/Failure)
  - Brak testów (do stworzenia w przyszłości)
  - Kontroler zaktualizowany (dodano ClientId do request DTO)
  - SuspendSubscriptionResult usunięty
- ✅ ResumeSubscriptionCommandHandler - UKOŃCZONE (2025-01-30)
  - Handler zmigrowany do Result Pattern (już był zmigrowany, tylko kontroler wymagał aktualizacji)
  - Brak testów (do stworzenia w przyszłości)
  - Kontroler zaktualizowany (dodano ClientId do request DTO, używa HandleResult)
  - ResumeSubscriptionResult usunięty
- ✅ UpgradeSubscriptionCommandHandler - UKOŃCZONE (2025-01-30)
  - Handler zmigrowany do Result Pattern
  - 6 testów zaktualizowanych
  - Kontroler zaktualizowany (dodano ClientId, używa HandleResult)
  - UpgradeSubscriptionResult usunięty
- ✅ DowngradeSubscriptionCommandHandler - UKOŃCZONE (2025-01-30)
  - Handler zmigrowany do Result Pattern
  - Brak testów (do stworzenia w przyszłości)
  - Kontroler zaktualizowany (dodano ClientId, używa HandleResult)
  - DowngradeSubscriptionResult usunięty
- ✅ RenewSubscriptionCommandHandler - UKOŃCZONE (2025-01-30)
  - Handler zmigrowany do Result Pattern
  - Brak testów (do stworzenia w przyszłości)
  - Kontroler zaktualizowany (dodano ClientId, używa HandleResult)
  - RenewSubscriptionResult usunięty
- ✅ ActivateSubscriptionCommandHandler - UKOŃCZONE (2025-01-30)
  - Handler zmigrowany do Result Pattern
  - 7 testów zaktualizowanych
  - Kontroler zaktualizowany (dodano ClientId request DTO, używa HandleResult)
  - ActivateSubscriptionResult usunięty
- ✅ GetActiveSubscriptionsQueryHandler - UKOŃCZONE (2025-01-30)
  - Handler zmigrowany do Result Pattern
  - Używa Result<PaginatedList<SubscriptionDto>> zamiast GetActiveSubscriptionsResult
  - Zabezpieczenie: Weryfikacja ITenantContext.HasTenant przed query
  - Używa ISubscriptionRepository.GetActiveSubscriptionsForTenantAsync() z explicit TenantId
  - Brak testów (handler nie miał testów wcześniej)
  - Kontroler zaktualizowany (używa HandleResult)
  - GetActiveSubscriptionsResult usunięty (wraz z ActiveSubscriptionDto)
- ✅ GetExpiringSubscriptionsQueryHandler - UKOŃCZONE (2025-01-30)
  - Handler zmigrowany do Result Pattern
  - Używa Result<PaginatedList<SubscriptionDto>> zamiast GetExpiringSubscriptionsResult
  - Zabezpieczenie: Weryfikacja ITenantContext.HasTenant przed query
  - Używa ISubscriptionRepository.GetExpiringSubscriptionsForTenantAsync() zamiast deprecated service method
  - Dodane IDateTime dependency dla consistent date handling
  - Brak testów (handler nie miał testów wcześniej)
  - Kontroler zaktualizowany (używa HandleResult)
  - GetExpiringSubscriptionsResult usunięty (wraz z ExpiringSubscriptionDto)
- ✅ CreateClientCommandHandler - UKOŃCZONE (2025-01-30)
  - Handler zmigrowany do Result Pattern
  - Utworzono globalny ClientDto w Orbito.Application/DTOs/
  - Usunięto try-catch, używa Result.Success()/Result.Failure<T>()
  - Błędy zmapowane do DomainErrors (dodano nowy Client.UserAlreadyExists)
  - 12 unit testów zaktualizowanych
  - 6 integration testów zaktualizowanych
  - Kontroler zaktualizowany (używa HandleResult z CreatedAtAction)
  - CreateClientResult usunięty (wraz z lokalnym ClientDto)
- ✅ DeactivateClientCommandHandler - UKOŃCZONE (2025-01-30)
  - Handler zmigrowany do Result Pattern
  - Używa globalnego ClientDto zamiast lokalnego DTO
  - Usunięto try-catch, używa Result.Success() / Result.Failure<T>()
  - Błędy zmapowane do DomainErrors (NoTenantContext, NotFound, CrossTenantAccess, AlreadyInactive)
  - 6 unit testów zaktualizowanych
  - Kontroler zaktualizowany (używa HandleResult)
  - DeactivateClientResult usunięty
- ✅ GetClientByIdQueryHandler - UKOŃCZONE (2025-01-30)
  - Handler zmigrowany do Result Pattern
  - Używa globalnego ClientDto zamiast lokalnego DTO
  - Usunięto try-catch, używa Result.Success() / Result.Failure<T>()
  - Błędy zmapowane do DomainErrors (NoTenantContext, NotFound, CrossTenantAccess)
  - 6 unit testów zaktualizowanych
  - Kontroler zaktualizowany (używa HandleResult)
  - GetClientByIdResult usunięty (z pliku Query)
- ✅ SearchClientsQueryHandler - UKOŃCZONE (2025-01-30)
  - Handler zmigrowany do Result Pattern
  - Używa `PaginatedList<ClientDto>` zamiast custom result
  - Usunięto try-catch, używa Result.Success() / Result.Failure<T>()
  - Błędy zmapowane do DomainErrors (NoTenantContext, Validation.Required)
  - 9 unit testów zaktualizowanych
  - Kontroler zaktualizowany (używa HandleResult)
  - SearchClientsResult usunięty
- ✅ UpdateProviderCommandHandler - UKOŃCZONE (2025-01-30)
  - Handler zmigrowany do Result Pattern
  - Używa globalnego `ProviderDto` z `Orbito.Application.DTOs` zamiast lokalnego
  - Usunięto try-catch, używa Result.Success() / Result.Failure<T>()
  - Błędy zmapowane do DomainErrors (NotFound, SubdomainAlreadyExists, SaveFailed)
  - 6 unit testów zaktualizowanych
  - Kontroler zaktualizowany (używa HandleResult)
  - UpdateProviderResult i lokalny ProviderDto usunięte
- ✅ DeleteProviderCommandHandler - UKOŃCZONE (2025-01-30)
  - Handler zmigrowany do Result Pattern
  - Używa `Result<Unit>` zamiast custom result
  - Usunięto try-catch, używa Result.Success(Unit.Value) / Result.Failure<Unit>()
  - Błędy zmapowane do DomainErrors (NotFound, CannotDeleteWithActiveClients, DeleteFailed)
  - 6 unit testów zaktualizowanych
  - Kontroler zaktualizowany (używa HandleResult)
  - DeleteProviderResult usunięty
- ✅ GetSubscriptionPlanByIdQueryHandler - UKOŃCZONE (2025-01-30)
  - Handler zmigrowany do Result Pattern
  - Query zwraca `Result<SubscriptionPlanDto>` zamiast `SubscriptionPlanDto?`
  - Zastąpiono `throw new InvalidOperationException` przez `Result.Failure<SubscriptionPlanDto>(DomainErrors.Tenant.NoTenantContext)`
  - Zastąpiono `return null` przez `Result.Failure<SubscriptionPlanDto>(DomainErrors.SubscriptionPlan.NotFound)`
  - Dodano `ILogger<GetSubscriptionPlanByIdQueryHandler>`
  - Usunięto try-catch block (exceptions propagują do GlobalExceptionHandler)
  - 8 testów zaktualizowanych (usunięto 1 test propagacji exception)
  - Kontroler zaktualizowany (używa HandleResult)
- ✅ GetSubscriptionPlansByProviderQueryHandler - UKOŃCZONE (2025-01-30)
  - Handler zmigrowany do Result Pattern
  - Query zwraca `Result<PaginatedList<SubscriptionPlanListItemDto>>` zamiast `SubscriptionPlansListDto`
  - Używa globalnego `PaginatedList<T>` z `Orbito.Application.Common.Models`
  - Zastąpiono `throw new InvalidOperationException` przez `Result.Failure<PaginatedList<SubscriptionPlanListItemDto>>(DomainErrors.Tenant.NoTenantContext)`
  - Dodano `ILogger<GetSubscriptionPlansByProviderQueryHandler>`
  - Usunięto try-catch block (exceptions propagują do GlobalExceptionHandler)
  - 8 testów zaktualizowanych (7 pozytywnych + 1 negatywny)
  - Kontroler zaktualizowany (używa HandleResult)
- ✅ GetActiveSubscriptionPlansQueryHandler - UKOŃCZONE (2025-01-30)
  - Handler zmigrowany do Result Pattern
  - Query zwraca `Result<ActiveSubscriptionPlansDto>` zamiast `ActiveSubscriptionPlansDto`
  - Zastąpiono `throw new InvalidOperationException` przez `Result.Failure<ActiveSubscriptionPlansDto>(DomainErrors.Tenant.NoTenantContext)`
  - Dodano `ILogger<GetActiveSubscriptionPlansQueryHandler>`
  - Usunięto try-catch block (exceptions propagują do GlobalExceptionHandler)
  - 9 testów zaktualizowanych (8 pozytywnych + 1 negatywny)
  - Kontroler zaktualizowany (używa HandleResult)
  - Endpoint jest publiczny (`[AllowAnonymous]`) ale nadal wymaga tenant context dla bezpieczeństwa

### 🎯 Do Zmigrowania (Faza 2 - Pozostałe)

## 📋 Priorytetyzacja Migracji

### **Priorytet 1 - Subscription Handlers** (Największy wpływ biznesowy)

#### Command Handlers (8 handlerów)

1. **CreateSubscriptionCommandHandler** ✅ COMPLETED

   - Plik: `Orbito.Application/Subscriptions/Commands/CreateSubscription/CreateSubscriptionCommandHandler.cs`
   - Command: `CreateSubscriptionCommand` → **ZMIGROWANE** do `Result<SubscriptionDto>`
   - DTO: Utworzono `SubscriptionDto` w `Orbito.Application/DTOs/SubscriptionDto.cs`
   - Błędy obsłużone:
     - Client not found → `DomainErrors.Client.NotFound` ✅
     - Plan not found → `DomainErrors.SubscriptionPlan.NotFound` ✅
     - Plan inactive → `DomainErrors.SubscriptionPlan.Inactive` ✅
   - Test: `CreateSubscriptionCommandHandlerTests` (5 testów) ✅ ZAKTUALIZOWANE
   - Kontroler: `SubscriptionsController.CreateSubscription()` ✅ ZAKTUALIZOWANY
   - Result class: `CreateSubscriptionResult` ✅ USUNIĘTY

2. **CancelSubscriptionCommandHandler** ✅ COMPLETED

   - Plik: `Orbito.Application/Subscriptions/Commands/CancelSubscription/CancelSubscriptionCommandHandler.cs`
   - Command: `CancelSubscriptionCommand` → **ZMIGROWANE** do `Result<SubscriptionDto>`
   - Błędy obsłużone:
     - No tenant context → `DomainErrors.Tenant.NoTenantContext` ✅
     - Subscription not found → `DomainErrors.Subscription.NotFound` ✅
     - Cannot be cancelled → `DomainErrors.Subscription.CannotBeCancelled` ✅
   - Test: `CancelSubscriptionCommandHandlerTests` (7 testów) ✅ ZAKTUALIZOWANE
   - Kontroler: `SubscriptionsController.CancelSubscription()` ✅ ZAKTUALIZOWANY (dodano ClientId do DTO)
   - Result class: `CancelSubscriptionResult` ✅ USUNIĘTY

3. **SuspendSubscriptionCommandHandler** ✅ COMPLETED

   - Plik: `Orbito.Application/Subscriptions/Commands/SuspendSubscription/SuspendSubscriptionCommandHandler.cs`
   - Command: `SuspendSubscriptionCommand` → **ZMIGROWANE** do `Result<SubscriptionDto>`
   - Błędy obsłużone:
     - No tenant context → `DomainErrors.Tenant.NoTenantContext` ✅
     - Subscription not found → `DomainErrors.Subscription.NotFound` ✅
     - Cannot suspend → `DomainErrors.Subscription.CannotSuspend` ✅
   - Test: Brak testów (do stworzenia w przyszłości)
   - Kontroler: `SubscriptionsController.SuspendSubscription()` ✅ ZAKTUALIZOWANY (dodano ClientId do DTO)
   - Result class: `SuspendSubscriptionResult` ✅ USUNIĘTY

4. **ResumeSubscriptionCommandHandler** ✅ COMPLETED

   - Plik: `Orbito.Application/Subscriptions/Commands/ResumeSubscription/ResumeSubscriptionCommandHandler.cs`
   - Command: `ResumeSubscriptionCommand` → **ZMIGROWANE** do `Result<SubscriptionDto>`
   - Błędy obsłużone:
     - No tenant context → `DomainErrors.Tenant.NoTenantContext` ✅
     - Subscription not found → `DomainErrors.Subscription.NotFound` ✅
     - Cannot resume → `DomainErrors.Subscription.CannotResume` ✅
   - Test: Brak testów (do stworzenia w przyszłości)
   - Kontroler: `SubscriptionsController.ResumeSubscription()` ✅ ZAKTUALIZOWANY (dodano ClientId do DTO)
   - Result class: `ResumeSubscriptionResult` ✅ USUNIĘTY

5. **UpgradeSubscriptionCommandHandler** ✅ COMPLETED

   - Plik: `Orbito.Application/Subscriptions/Commands/UpgradeSubscription/UpgradeSubscriptionCommandHandler.cs`
   - Command: `UpgradeSubscriptionCommand` → **ZMIGROWANE** do `Result<SubscriptionDto>`
   - Błędy obsłużone:
     - No tenant context → `DomainErrors.Tenant.NoTenantContext` ✅
     - Subscription not found → `DomainErrors.Subscription.NotFound` ✅
     - Cannot upgrade → `DomainErrors.Subscription.CannotUpgrade` ✅
   - Test: `UpgradeSubscriptionCommandHandlerTests` (6 testów) ✅ ZAKTUALIZOWANE
   - Kontroler: `SubscriptionsController.UpgradeSubscription()` ✅ ZAKTUALIZOWANY (dodano ClientId, używa HandleResult)
   - Result class: `UpgradeSubscriptionResult` ✅ USUNIĘTY

6. **DowngradeSubscriptionCommandHandler** ✅ COMPLETED

   - Plik: `Orbito.Application/Subscriptions/Commands/DowngradeSubscription/DowngradeSubscriptionCommandHandler.cs`
   - Command: `DowngradeSubscriptionCommand` → **ZMIGROWANE** do `Result<SubscriptionDto>`
   - Błędy obsłużone:
     - No tenant context → `DomainErrors.Tenant.NoTenantContext` ✅
     - Subscription not found → `DomainErrors.Subscription.NotFound` ✅
     - Cannot downgrade → `DomainErrors.Subscription.CannotDowngrade` ✅
   - Test: Brak testów (do stworzenia w przyszłości)
   - Kontroler: `SubscriptionsController.DowngradeSubscription()` ✅ ZAKTUALIZOWANY (dodano ClientId, używa HandleResult)
   - Result class: `DowngradeSubscriptionResult` ✅ USUNIĘTY

7. **RenewSubscriptionCommandHandler** ✅ COMPLETED

   - Plik: `Orbito.Application/Subscriptions/Commands/RenewSubscription/RenewSubscriptionCommandHandler.cs`
   - Command: `RenewSubscriptionCommand` → **ZMIGROWANE** do `Result<SubscriptionDto>`
   - Błędy obsłużone:
     - No tenant context → `DomainErrors.Tenant.NoTenantContext` ✅
     - Subscription not found → `DomainErrors.Subscription.NotFound` ✅
     - Cannot renew (payment failed) → `DomainErrors.Subscription.CannotRenew` ✅
   - Test: Brak testów (do stworzenia w przyszłości)
   - Kontroler: `SubscriptionsController.RenewSubscription()` ✅ ZAKTUALIZOWANY (dodano ClientId, używa HandleResult)
   - Result class: `RenewSubscriptionResult` ✅ USUNIĘTY

8. **ActivateSubscriptionCommandHandler** ✅ COMPLETED

   - Plik: `Orbito.Application/Subscriptions/Commands/ActivateSubscription/ActivateSubscriptionCommandHandler.cs`
   - Command: `ActivateSubscriptionCommand` → **ZMIGROWANE** do `Result<SubscriptionDto>`
   - Błędy obsłużone:
     - No tenant context → `DomainErrors.Tenant.NoTenantContext` ✅
     - Subscription not found → `DomainErrors.Subscription.NotFound` ✅
     - Already active (cannot resume) → `DomainErrors.Subscription.AlreadyActive` ✅
   - Test: `ActivateSubscriptionCommandHandlerTests` (7 testów) ✅ ZAKTUALIZOWANE
   - Kontroler: `SubscriptionsController.ActivateSubscription()` ✅ ZAKTUALIZOWANY (dodano ClientId request DTO, używa HandleResult)
   - Result class: `ActivateSubscriptionResult` ✅ USUNIĘTY

#### Query Handlers (4 handlery)

9. **GetSubscriptionByIdQueryHandler** ✅ COMPLETED

   - Plik: `Orbito.Application/Subscriptions/Queries/GetSubscriptionById/GetSubscriptionByIdQueryHandler.cs`
   - Query: `GetSubscriptionByIdQuery` → **ZMIGROWANE** do `Result<SubscriptionDto>`
   - Błędy obsłużone:
     - No tenant context → `DomainErrors.Tenant.NoTenantContext` ✅
     - Subscription not found → `DomainErrors.Subscription.NotFound` ✅
     - Cross-tenant access → `DomainErrors.Tenant.CrossTenantAccess` ✅
   - Test: `GetSubscriptionByIdQueryHandlerTests` (6 testów) ✅ ZAKTUALIZOWANE
   - Kontroler: `SubscriptionsController.GetSubscriptionById()` ✅ ZAKTUALIZOWANY (używa HandleResult)
   - Result class: `GetSubscriptionByIdResult` ✅ USUNIĘTY
   - DTO: Rozszerzono `SubscriptionDto` o pola szczegółowe (Client, Plan, Payment details) ✅

10. **GetSubscriptionsByClientQueryHandler** ✅ COMPLETED

    - Plik: `Orbito.Application/Subscriptions/Queries/GetSubscriptionsByClient/GetSubscriptionsByClientQueryHandler.cs`
    - Query: `GetSubscriptionsByClientQuery` → **ZMIGROWANE** do `Result<PaginatedList<SubscriptionDto>>`
    - Używa globalnego `SubscriptionDto` zamiast lokalnego DTO
    - Zwraca `PaginatedList<SubscriptionDto>` z pełnym wsparciem dla paginacji
    - Test: `GetSubscriptionsByClientQueryHandlerTests` (7 testów) ✅ ZAKTUALIZOWANE
    - Kontroler: `SubscriptionsController.GetSubscriptionsByClient()` ✅ ZAKTUALIZOWANY (używa HandleResult)
    - Result class: `GetSubscriptionsByClientResult` ✅ USUNIĘTY (wraz z lokalnym SubscriptionDto)

11. **GetActiveSubscriptionsQueryHandler** ✅ COMPLETED

    - Plik: `Orbito.Application/Subscriptions/Queries/GetActiveSubscriptions/GetActiveSubscriptionsQueryHandler.cs`
    - Query: `GetActiveSubscriptionsQuery` → **ZMIGROWANE** do `Result<PaginatedList<SubscriptionDto>>`
    - Używa `ISubscriptionRepository.GetActiveSubscriptionsForTenantAsync()` z explicit TenantId
    - Zabezpieczenie: Weryfikacja `ITenantContext.HasTenant` przed wykonaniem query
    - Mapowanie do globalnego `SubscriptionDto` z wszystkimi polami
    - Zwraca `PaginatedList<SubscriptionDto>` z pełnym wsparciem dla paginacji i search
    - Test: **BRAK TESTÓW** (handler wcześniej nie miał testów)
    - Kontroler: `SubscriptionsController.GetSubscriptions()` ✅ ZAKTUALIZOWANY (używa HandleResult)
    - Result class: `GetActiveSubscriptionsResult` ✅ USUNIĘTY (wraz z lokalnym ActiveSubscriptionDto)

12. **GetExpiringSubscriptionsQueryHandler** ✅ COMPLETED
    - Plik: `Orbito.Application/Subscriptions/Queries/GetExpiringSubscriptions/GetExpiringSubscriptionsQueryHandler.cs`
    - Query: `GetExpiringSubscriptionsQuery` → **ZMIGROWANE** do `Result<PaginatedList<SubscriptionDto>>`
    - Używa `ISubscriptionRepository.GetExpiringSubscriptionsForTenantAsync()` zamiast deprecated `ISubscriptionService.GetExpiringSubscriptionsAsync()`
    - Zabezpieczenie: Weryfikacja `ITenantContext.HasTenant` przed wykonaniem query
    - Dodane `IDateTime` dependency dla consistent date handling
    - Mapowanie do globalnego `SubscriptionDto` z wszystkimi polami
    - Zwraca `PaginatedList<SubscriptionDto>` z pełnym wsparciem dla paginacji
    - Test: **BRAK TESTÓW** (handler wcześniej nie miał testów)
    - Kontroler: `SubscriptionsController.GetExpiringSubscriptions()` ✅ ZAKTUALIZOWANY (używa HandleResult)
    - Result class: `GetExpiringSubscriptionsResult` ✅ USUNIĘTY (wraz z lokalnym ExpiringSubscriptionDto)

---

### **Priorytet 2 - Client Handlers** (Podstawowe operacje CRUD)

#### Command Handlers (5 handlerów)

13. **CreateClientCommandHandler** ✅ COMPLETED

    - Plik: `Orbito.Application/Clients/Commands/CreateClient/CreateClientCommandHandler.cs`
    - Command: `CreateClientCommand` → **ZMIGROWANE** do `Result<ClientDto>`
    - Utworzono globalny `ClientDto` w `Orbito.Application/DTOs/ClientDto.cs`
    - Usunięto try-catch, używa `Result.Success()` / `Result.Failure<T>()`
    - Błędy zmapowane:
      - No tenant context → `DomainErrors.Tenant.NoTenantContext`
      - Provider not found → `DomainErrors.Provider.NotFound`
      - Email already exists → `DomainErrors.Client.EmailAlreadyExists`
      - User already exists → `DomainErrors.Client.UserAlreadyExists` (dodany nowy error)
    - Test: `CreateClientCommandHandlerTests` (12 testów) ✅ ZAKTUALIZOWANE
    - Integration Tests: `ClientIntegrationTests` (6 testów CreateClient) ✅ ZAKTUALIZOWANE
    - Kontroler: `ClientsController.CreateClient()` ✅ ZAKTUALIZOWANY (używa HandleResult)
    - Result class: `CreateClientResult` ✅ USUNIĘTY (wraz z lokalnym ClientDto)

14. **UpdateClientCommandHandler** ✅ COMPLETED

    - Plik: `Orbito.Application/Clients/Commands/UpdateClient/UpdateClientCommandHandler.cs`
    - Command: `UpdateClientCommand` → **ZMIGROWANE** do `Result<ClientDto>`
    - Używa globalnego `ClientDto` zamiast lokalnego DTO
    - Usunięto try-catch, używa `Result.Success()` / `Result.Failure<T>()`
    - Błędy zmapowane:
      - No tenant context → `DomainErrors.Tenant.NoTenantContext` ✅
      - Client not found → `DomainErrors.Client.NotFound` ✅
      - Cross-tenant access → `DomainErrors.Tenant.CrossTenantAccess` ✅
      - Email already exists → `DomainErrors.Client.EmailAlreadyExists` ✅
    - Test: `UpdateClientCommandHandlerTests` (12 testów) ✅ ZAKTUALIZOWANE
    - Kontroler: `ClientsController.UpdateClient()` ✅ ZAKTUALIZOWANY (używa HandleResult)
    - Result class: `UpdateClientResult` ✅ USUNIĘTY

15. **DeleteClientCommandHandler** ✅ COMPLETED

    - Plik: `Orbito.Application/Clients/Commands/DeleteClient/DeleteClientCommandHandler.cs`
    - Command: `DeleteClientCommand` → **ZMIGROWANE** do `Result<Unit>`
    - Używa `Unit.Value` dla void success (brak zwracanej wartości)
    - Usunięto try-catch, używa `Result.Success(Unit.Value)` / `Result.Failure<Unit>()`
    - Błędy zmapowane:
      - No tenant context → `DomainErrors.Tenant.NoTenantContext` ✅
      - Client not found → `DomainErrors.Client.NotFound` ✅
      - Cross-tenant access → `DomainErrors.Tenant.CrossTenantAccess` ✅
      - Cannot delete with active subscriptions → `DomainErrors.Client.CannotDeleteWithActiveSubscriptions` ✅
    - Test: `DeleteClientCommandHandlerTests` (7 testów) ✅ ZAKTUALIZOWANE
    - Testy integracyjne: 2 testy w `ClientIntegrationTests` ✅ ZAKTUALIZOWANE
    - Kontroler: `ClientsController.DeleteClient()` ✅ ZAKTUALIZOWANY (używa HandleResult, zwraca NoContent())
    - Result class: `DeleteClientResult` ✅ USUNIĘTY

16. **ActivateClientCommandHandler** ✅ COMPLETED

    - Plik: `Orbito.Application/Clients/Commands/ActivateClient/ActivateClientCommandHandler.cs`
    - Command: `ActivateClientCommand` → **ZMIGROWANE** do `Result<ClientDto>`
    - Używa globalnego `ClientDto` zamiast lokalnego DTO
    - Usunięto try-catch, używa `Result.Success()` / `Result.Failure<T>()`
    - Błędy zmapowane:
      - No tenant context → `DomainErrors.Tenant.NoTenantContext` ✅
      - Client not found → `DomainErrors.Client.NotFound` ✅
      - Cross-tenant access → `DomainErrors.Tenant.CrossTenantAccess` ✅
      - Client already active → `DomainErrors.Client.AlreadyActive` ✅
    - Test: `ActivateClientCommandHandlerTests` (6 testów) ✅ ZAKTUALIZOWANE
    - Testy integracyjne: 1 test w `ClientIntegrationTests` ✅ ZAKTUALIZOWANY
    - Kontroler: `ClientsController.ActivateClient()` ✅ ZAKTUALIZOWANY (używa HandleResult)
    - Result class: `ActivateClientResult` ✅ USUNIĘTY

17. **DeactivateClientCommandHandler** ✅ COMPLETED
    - Plik: `Orbito.Application/Clients/Commands/DeactivateClient/DeactivateClientCommandHandler.cs`
    - Command: `DeactivateClientCommand` → **ZMIGROWANE** do `Result<ClientDto>`
    - Używa globalnego `ClientDto` zamiast lokalnego DTO
    - Usunięto try-catch, używa `Result.Success()` / `Result.Failure<T>()`
    - Błędy zmapowane:
      - No tenant context → `DomainErrors.Tenant.NoTenantContext` ✅
      - Client not found → `DomainErrors.Client.NotFound` ✅
      - Cross-tenant access → `DomainErrors.Tenant.CrossTenantAccess` ✅
      - Client already inactive → `DomainErrors.Client.AlreadyInactive` ✅
    - Test: `DeactivateClientCommandHandlerTests` (6 testów) ✅ ZAKTUALIZOWANE
    - Kontroler: `ClientsController.DeactivateClient()` ✅ ZAKTUALIZOWANY (używa HandleResult)
    - Result class: `DeactivateClientResult` ✅ USUNIĘTY

#### Query Handlers (3 handlery)

18. **GetClientByIdQueryHandler** ✅ COMPLETED

    - Plik: `Orbito.Application/Clients/Queries/GetClientById/GetClientByIdQueryHandler.cs`
    - Query: `GetClientByIdQuery` → **ZMIGROWANE** do `Result<ClientDto>`
    - Używa globalnego `ClientDto` zamiast lokalnego DTO
    - Usunięto try-catch, używa `Result.Success()` / `Result.Failure<T>()`
    - Błędy zmapowane:
      - No tenant context → `DomainErrors.Tenant.NoTenantContext` ✅
      - Client not found → `DomainErrors.Client.NotFound` ✅
      - Cross-tenant access → `DomainErrors.Tenant.CrossTenantAccess` ✅
    - Test: `GetClientByIdQueryHandlerTests` (6 testów) ✅ ZAKTUALIZOWANE
    - Kontroler: `ClientsController.GetClientById()` ✅ ZAKTUALIZOWANY (używa HandleResult)
    - Result class: `GetClientByIdResult` ✅ USUNIĘTY

19. **GetClientsByProviderQueryHandler** ✅ COMPLETED

    - Plik: `Orbito.Application/Clients/Queries/GetClientsByProvider/GetClientsByProviderQueryHandler.cs`
    - Query: `GetClientsByProviderQuery` → **ZMIGROWANE** do `Result<PaginatedList<ClientDto>>`
    - Używa `PaginatedList<ClientDto>` z `Orbito.Application.Common.Models`
    - Usunięto try-catch, używa `Orbito.Domain.Common.Result.Success()`
    - Handler propaguje exceptions (bez mapowania błędów - tenant isolation przez query filters)
    - Rozwiązano konflikt nazw Result (Application.Common.Models vs Domain.Common)
    - Test: `GetClientsByProviderQueryHandlerTests` (6 testów) ✅ ZAKTUALIZOWANE
    - Integration tests: GetClientById i DeactivateClient ✅ NAPRAWIONE (używały starego wzorca)
    - Kontroler: `ClientsController.GetClients()` ✅ ZAKTUALIZOWANY (custom wrapping dla frontend)
    - Result class: `GetClientsByProviderResult` ✅ USUNIĘTY

20. **SearchClientsQueryHandler** ✅ COMPLETED
    - Plik: `Orbito.Application/Clients/Queries/SearchClients/SearchClientsQueryHandler.cs`
    - Query: `SearchClientsQuery` → **ZMIGROWANE** do `Result<PaginatedList<ClientDto>>`
    - Używa `PaginatedList<ClientDto>` z `Orbito.Application.Common.Models`
    - Usunięto try-catch, używa `Result.Success()` / `Result.Failure<T>()`
    - Błędy zmapowane:
      - No tenant context → `DomainErrors.Tenant.NoTenantContext` ✅
      - Empty/null search term → `DomainErrors.Validation.Required("SearchTerm")` ✅
    - Test: `SearchClientsQueryHandlerTests` (9 testów) ✅ ZAKTUALIZOWANE
    - Kontroler: `ClientsController.SearchClients()` ✅ ZAKTUALIZOWANY (używa HandleResult)
    - Result class: `SearchClientsResult` ✅ USUNIĘTY

---

### **Priorytet 3 - Provider Handlers** (Pozostałe)

21. **UpdateProviderCommandHandler** ✅ COMPLETED

    - Plik: `Orbito.Application/Providers/Commands/UpdateProvider/UpdateProviderCommandHandler.cs`
    - Command: `UpdateProviderCommand` → **ZMIGROWANE** do `Result<ProviderDto>`
    - Używa globalnego `ProviderDto` z `Orbito.Application.DTOs`
    - Usunięto try-catch, używa `Result.Success()` / `Result.Failure<T>()`
    - Błędy zmapowane:
      - Provider not found → `DomainErrors.Provider.NotFound` ✅
      - Subdomain already taken → `DomainErrors.Provider.SubdomainAlreadyExists` ✅
      - SaveChanges failed → `Error.Create("Provider.SaveFailed", ...)` ✅
    - Test: `UpdateProviderCommandHandlerTests` (6 testów) ✅ ZAKTUALIZOWANE
    - Kontroler: `ProvidersController.UpdateProvider()` ✅ ZAKTUALIZOWANY (używa HandleResult)
    - Result class: `UpdateProviderResult` ✅ USUNIĘTY
    - Lokalny DTO: `ProviderDto` ✅ USUNIĘTY (używa globalnego z DTOs)

22. **DeleteProviderCommandHandler** ✅ COMPLETED

    - Plik: `Orbito.Application/Providers/Commands/DeleteProvider/DeleteProviderCommandHandler.cs`
    - Command: `DeleteProviderCommand` → **ZMIGROWANE** do `Result<Unit>`
    - Usunięto try-catch, używa `Result.Success(Unit.Value)` / `Result.Failure<Unit>()`
    - Błędy zmapowane:
      - Provider not found → `DomainErrors.Provider.NotFound` ✅
      - Cannot delete with active clients → `DomainErrors.Provider.CannotDeleteWithActiveClients` ✅
      - SaveChanges failed → `Error.Create("Provider.DeleteFailed", ...)` ✅
    - Test: `DeleteProviderCommandHandlerTests` (6 testów) ✅ ZAKTUALIZOWANE
    - Kontroler: `ProvidersController.DeleteProvider()` ✅ ZAKTUALIZOWANY (używa HandleResult)
    - Result class: `DeleteProviderResult` ✅ USUNIĘTY

23. **GetProviderByIdQueryHandler** ✅ COMPLETED

    - Plik: `Orbito.Application/Providers/Queries/GetProviderById/GetProviderByIdQueryHandler.cs`
    - Query: `GetProviderByIdQuery` → **ZMIGROWANE** do `Result<ProviderDto>`
    - Usunięto try-catch block (exceptions propagowane)
    - Używa `Orbito.Domain.Common.Result.Failure<ProviderDto>(DomainErrors.Provider.NotFound)` dla not found
    - Używa `Result.Success(providerDto)` dla sukcesu
    - ProviderDto używany z `Orbito.Application.DTOs` (definicja usunięta z query file)
    - Test: `GetProviderByIdQueryHandlerTests` (3 testy) ✅ ZAKTUALIZOWANE
    - Integration tests: `ProviderIntegrationTests.GetProviderById*` (2 testy) ✅ NAPRAWIONE
    - Kontroler: `ProvidersController.GetProviderById()` ✅ ZAKTUALIZOWANY (używa HandleResult)
    - Result class: `GetProviderByIdResult` i duplikacja `ProviderDto` ✅ USUNIĘTE

24. **GetProviderByUserIdQueryHandler** ✅ COMPLETED

    - Plik: `Orbito.Application/Providers/Queries/GetProviderByUserId/GetProviderByUserIdQueryHandler.cs`
    - Query: `GetProviderByUserIdQuery` → **ZMIGROWANE** do `Result<ProviderDto>`
    - Usunięto try-catch block (exceptions propagowane)
    - Używa `Result.Failure<ProviderDto>(DomainErrors.Provider.NotFound)` dla not found
    - Używa `Result.Success(providerDto)` dla sukcesu
    - Używa globalnego `ProviderDto` z `Orbito.Application.DTOs`
    - Test: `GetProviderByUserIdQueryHandlerTests` (2 testy) ✅ ZAKTUALIZOWANE
    - Kontroler: `ProvidersController.GetProviderByUserId()` ✅ ZAKTUALIZOWANY (używa HandleResult)
    - Result class: `GetProviderByUserIdResult` ✅ USUNIĘTY (wraz z lokalnym ProviderDto)

25. **GetAllProvidersQueryHandler** ✅ COMPLETED

    - Plik: `Orbito.Application/Providers/Queries/GetAllProviders/GetAllProvidersQueryHandler.cs`
    - Query: `GetAllProvidersQuery` → **ZMIGROWANE** do `Result<PaginatedList<ProviderDto>>`
    - Usunięto try-catch block (exceptions propagowane)
    - Używa `PaginatedList<ProviderDto>` z `Orbito.Application.Common.Models`
    - Używa `Result.Success(paginatedList)` dla sukcesu
    - Używa globalnego `ProviderDto` z `Orbito.Application.DTOs` (zamiast ProviderSummaryDto)
    - Test: `GetAllProvidersQueryHandlerTests` (4 testy) ✅ ZAKTUALIZOWANE
    - Kontroler: `ProvidersController.GetAllProviders()` ✅ ZAKTUALIZOWANY (custom wrapping dla frontend, używa HandleResult dla błędów)
    - Result class: `GetAllProvidersResult` ✅ USUNIĘTY (wraz z lokalnym ProviderSummaryDto)

---

### **Priorytet 4 - SubscriptionPlan Handlers**

26. **CreateSubscriptionPlanCommandHandler** ✅ COMPLETED

    - Plik: `Orbito.Application/SubscriptionPlans/Commands/CreateSubscriptionPlan/CreateSubscriptionPlanCommandHandler.cs`
    - Command: `CreateSubscriptionPlanCommand` → ~~`CreateSubscriptionPlanResult`~~ → `Result<SubscriptionPlanDto>`
    - **Zmiany**:
      - ✅ Usunięto `CreateSubscriptionPlanResult` (duplikacja DTO)
      - ✅ Handler zwraca `Result<SubscriptionPlanDto>` (używa globalnego DTO z Query)
      - ✅ Zastąpiono `InvalidOperationException` przez `Result.Failure<SubscriptionPlanDto>(DomainErrors.Tenant.NoTenantContext)`
      - ✅ Dodano `ILogger<CreateSubscriptionPlanCommandHandler>`
      - ✅ Usunięto try-catch block (exceptions propagują do GlobalExceptionHandler)
    - **Błędy**:
      - No tenant context → `DomainErrors.Tenant.NoTenantContext`
    - **Controller**: Zaktualizowano `SubscriptionPlansController.CreateSubscriptionPlan()` - używa `HandleResult()`
    - **Testy**: Zaktualizowano `CreateSubscriptionPlanCommandHandlerTests` - 11 testów (10 pozytywnych + 1 negatywny):
      - ✅ Test #1: `Handle_WithValidCommand_ShouldCreateSubscriptionPlan` - zmieniono na `result.IsSuccess` + `result.Value`
      - ✅ Test #2: `Handle_WithMinimalCommand_ShouldCreateSubscriptionPlan` - zmieniono na `result.Value`
      - ✅ Test #3: `Handle_WithPrivatePlan_ShouldSetIsPublicToFalse` - zmieniono na `result.Value.IsPublic`
      - ✅ Test #4: `Handle_WithTrialPeriod_ShouldSetTrialPeriodDays` - zmieniono na `result.Value.TrialPeriodDays`
      - ✅ Test #5: `Handle_WithFeaturesAndLimitations_ShouldSetJsonProperties` - zmieniono na `result.Value`
      - ✅ Test #6: `Handle_WithCustomSortOrder_ShouldSetSortOrder` - zmieniono na `result.Value.SortOrder`
      - ✅ Test #7: `Handle_WithDifferentBillingPeriods_ShouldCreateCorrectBillingPeriod` - zmieniono na `result.Value.BillingPeriod`
      - ✅ Test #8: `Handle_WithEmptyName_ShouldCreateSubscriptionPlan` - zmieniono na `result.Value.Name`
      - ✅ Test #9: `Handle_WithNegativeAmount_ShouldThrowArgumentException` - pozostawiono (domain validation)
      - ✅ Test #10: `Handle_WithVeryLongName_ShouldCreateSubscriptionPlan` - zmieniono na `result.Value.Name`
      - ✅ Test #11: `Handle_WithoutTenantContext_ShouldReturnFailure` - zmieniono z `ThrowsAsync` na `result.IsFailure` + `DomainErrors.Tenant.NoTenantContext`
      - ❌ Usunięto test: `Handle_WhenRepositoryThrowsException_ShouldPropagateException` (już nie dotyczy)

27. **UpdateSubscriptionPlanCommandHandler** ✅ COMPLETED

    - Plik: `Orbito.Application/SubscriptionPlans/Commands/UpdateSubscriptionPlan/UpdateSubscriptionPlanCommandHandler.cs`
    - Command: `UpdateSubscriptionPlanCommand` → ~~`UpdateSubscriptionPlanResult`~~ → `Result<SubscriptionPlanDto>`
    - **Zmiany**:
      - ✅ Usunięto `UpdateSubscriptionPlanResult` (duplikacja DTO)
      - ✅ Handler zwraca `Result<SubscriptionPlanDto>` (używa globalnego DTO z Query)
      - ✅ Zastąpiono 2 wyjątki `InvalidOperationException` przez Result Pattern:
        - Brak tenant context → `Result.Failure<SubscriptionPlanDto>(DomainErrors.Tenant.NoTenantContext)`
        - Plan nie znaleziony → `Result.Failure<SubscriptionPlanDto>(DomainErrors.SubscriptionPlan.NotFound)`
      - ✅ Dodano `ILogger<UpdateSubscriptionPlanCommandHandler>`
      - ✅ Usunięto try-catch block (exceptions propagują do GlobalExceptionHandler)
    - **Błędy**:
      - No tenant context → `DomainErrors.Tenant.NoTenantContext`
      - Plan not found → `DomainErrors.SubscriptionPlan.NotFound`
    - **Controller**: Zaktualizowano `SubscriptionPlansController.UpdateSubscriptionPlan()` - używa `HandleResult()`
    - **Testy**: Zaktualizowano `UpdateSubscriptionPlanCommandHandlerTests` - 11 testów (9 pozytywnych + 2 negatywne):
      - ✅ Test #1-#9: Updated to use result.Value (pozytywne testy - valid command, minimal updates, deactivation, activation, visibility, features, trial period, sort order)
      - ✅ Test #10: `Handle_WithoutTenantContext_ShouldReturnFailure` - zmieniono z `ThrowsAsync` na `result.IsFailure` + `DomainErrors.Tenant.NoTenantContext`
      - ✅ Test #11: `Handle_WithNonExistentPlan_ShouldReturnFailure` - zmieniono z `ThrowsAsync` na `result.IsFailure` + `DomainErrors.SubscriptionPlan.NotFound`
      - ✅ Test #12 (Theory): `Handle_WithDifferentBillingPeriods_ShouldUpdateCorrectBillingPeriod` - updated to use result.Value
      - ❌ Usunięto test: `Handle_WhenRepositoryThrowsException_ShouldPropagateException` (już nie dotyczy)

28. **DeleteSubscriptionPlanCommandHandler** ✅ COMPLETED

    - Plik: `Orbito.Application/SubscriptionPlans/Commands/DeleteSubscriptionPlan/DeleteSubscriptionPlanCommandHandler.cs`
    - Command: `DeleteSubscriptionPlanCommand` → `Result<DeleteSubscriptionPlanResult>`
    - **Zmiany**:
      - ✅ **NIE usunięto** `DeleteSubscriptionPlanResult` - zawiera specyficzne informacje dla operacji delete (IsDeleted, IsHardDelete, Message)
      - ✅ Handler zwraca `Result<DeleteSubscriptionPlanResult>`
      - ✅ Zastąpiono 2 wyjątki `InvalidOperationException` przez Result Pattern:
        - Brak tenant context → `Result.Failure<DeleteSubscriptionPlanResult>(DomainErrors.Tenant.NoTenantContext)`
        - Plan nie znaleziony → `Result.Failure<DeleteSubscriptionPlanResult>(DomainErrors.SubscriptionPlan.NotFound)`
      - ✅ Logika biznesowa: Plan z aktywnymi subskrypcjami może być usunięty tylko przez HardDelete (zwraca `Result.Success` z `IsDeleted=false`)
      - ✅ Dodano `ILogger<DeleteSubscriptionPlanCommandHandler>`
      - ✅ Usunięto try-catch block (exceptions propagują do GlobalExceptionHandler)
    - **Błędy**:
      - No tenant context → `DomainErrors.Tenant.NoTenantContext`
      - Plan not found → `DomainErrors.SubscriptionPlan.NotFound`
    - **Controller**: Zaktualizowano `SubscriptionPlansController.DeleteSubscriptionPlan()` - używa `HandleResult()`
    - **Testy**: Zaktualizowano `DeleteSubscriptionPlanCommandHandlerTests` - 7 testów (5 pozytywnych + 2 negatywne):
      - ✅ Test #1: `Handle_WithSoftDeleteAndDeletablePlan_ShouldDeactivatePlan` - updated to use result.Value
      - ✅ Test #2: `Handle_WithHardDelete_ShouldPermanentlyDeletePlan` - updated to use result.Value
      - ✅ Test #3: `Handle_WithSoftDeleteAndNonDeletablePlan_ShouldReturnFailure` - updated to use result.Value (pozytywny test - zwraca success z IsDeleted=false)
      - ✅ Test #4: `Handle_WithHardDeleteAndNonDeletablePlan_ShouldForceDelete` - updated to use result.Value
      - ✅ Test #5: `Handle_WithSoftDeleteAndPlanWithInactiveSubscriptions_ShouldDeactivatePlan` - updated to use result.Value
      - ✅ Test #6: `Handle_WithoutTenantContext_ShouldReturnFailure` - zmieniono z `ThrowsAsync` na `result.IsFailure` + `DomainErrors.Tenant.NoTenantContext`
      - ✅ Test #7: `Handle_WithNonExistentPlan_ShouldReturnFailure` - zmieniono z `ThrowsAsync` na `result.IsFailure` + `DomainErrors.SubscriptionPlan.NotFound`
      - ✅ Test #8: `Handle_WithDefaultHardDeleteValue_ShouldPerformSoftDelete` - updated to use result.Value
      - ❌ Usunięto test: `Handle_WhenRepositoryThrowsException_ShouldPropagateException` (już nie dotyczy)

29. **CloneSubscriptionPlanCommandHandler** ✅ COMPLETED

    - Plik: `Orbito.Application/SubscriptionPlans/Commands/CloneSubscriptionPlan/CloneSubscriptionPlanCommandHandler.cs`
    - Command: `CloneSubscriptionPlanCommand` → `Result<CloneSubscriptionPlanResult>`
    - **Zmiany**:
      - ✅ **NIE usunięto** `CloneSubscriptionPlanResult` - zawiera unikalne pole `OriginalPlanId` specyficzne dla operacji klonowania
      - ✅ Handler zwraca `Result<CloneSubscriptionPlanResult>`
      - ✅ Zastąpiono 2 wyjątki `InvalidOperationException` przez Result Pattern:
        - Brak tenant context → `Result.Failure<CloneSubscriptionPlanResult>(DomainErrors.Tenant.NoTenantContext)`
        - Oryginalny plan nie znaleziony → `Result.Failure<CloneSubscriptionPlanResult>(DomainErrors.SubscriptionPlan.NotFound)`
      - ✅ Dodano `ILogger<CloneSubscriptionPlanCommandHandler>`
      - ✅ Logika klonowania: Kopiuje wszystkie właściwości z oryginalnego planu (opcjonalne nadpisanie amount, currency, description, sortOrder)
      - ✅ Usunięto try-catch block (exceptions propagują do GlobalExceptionHandler)
    - **Błędy**:
      - No tenant context → `DomainErrors.Tenant.NoTenantContext`
      - Original plan not found → `DomainErrors.SubscriptionPlan.NotFound`
    - **Controller**: Zaktualizowano `SubscriptionPlansController.CloneSubscriptionPlan()` - używa `HandleResult()`, zwraca `CreatedAtAction` z `result.Value`
    - **Testy**: Zaktualizowano `CloneSubscriptionPlanCommandHandlerTests` - 8 testów (6 pozytywnych + 2 negatywne):
      - ✅ Test #1: `Handle_WithValidCommand_ShouldCloneSubscriptionPlan` - updated to use result.Value
      - ✅ Test #2: `Handle_WithMinimalCommand_ShouldCloneWithOriginalValues` - updated to use result.Value
      - ✅ Test #3: `Handle_WithInactiveClonedPlan_ShouldCreateInactivePlan` - updated to use result.Value
      - ✅ Test #4: `Handle_WithPrivateClonedPlan_ShouldCreatePrivatePlan` - updated to use result.Value
      - ✅ Test #5: `Handle_WithPartialOverrides_ShouldCloneWithMixedValues` - updated to use result.Value
      - ✅ Test #6: `Handle_WithFeaturesAndLimitations_ShouldCloneJsonProperties` - updated to use result.Value
      - ✅ Test #7: `Handle_WithoutTenantContext_ShouldReturnFailure` - zmieniono z `ThrowsAsync` na `result.IsFailure` + `DomainErrors.Tenant.NoTenantContext`
      - ✅ Test #8: `Handle_WithNonExistentPlan_ShouldReturnFailure` - zmieniono z `ThrowsAsync` na `result.IsFailure` + `DomainErrors.SubscriptionPlan.NotFound`
      - ✅ Test #9 (Theory): `Handle_WithDifferentBillingPeriods_ShouldCloneCorrectBillingPeriod` - updated to use result.Value
      - ❌ Usunięto test: `Handle_WhenRepositoryThrowsException_ShouldPropagateException` (już nie dotyczy)

30. **GetSubscriptionPlanByIdQueryHandler** ✅ COMPLETED

    - Plik: `Orbito.Application/SubscriptionPlans/Queries/GetSubscriptionPlanById/GetSubscriptionPlanByIdQueryHandler.cs`
    - Query: `GetSubscriptionPlanByIdQuery` → **ZMIGROWANE** do `Result<SubscriptionPlanDto>`
    - **Zmiany**:
      - ✅ Handler zwraca `Result<SubscriptionPlanDto>` zamiast `SubscriptionPlanDto?`
      - ✅ Zastąpiono `throw new InvalidOperationException` przez `Result.Failure<SubscriptionPlanDto>(DomainErrors.Tenant.NoTenantContext)`
      - ✅ Zastąpiono `return null` przez `Result.Failure<SubscriptionPlanDto>(DomainErrors.SubscriptionPlan.NotFound)`
      - ✅ Dodano `ILogger<GetSubscriptionPlanByIdQueryHandler>`
      - ✅ Usunięto try-catch block (exceptions propagują do GlobalExceptionHandler)
    - **Błędy**:
      - No tenant context → `DomainErrors.Tenant.NoTenantContext` ✅
      - Plan not found → `DomainErrors.SubscriptionPlan.NotFound` ✅
    - **Controller**: Zaktualizowano `SubscriptionPlansController.GetSubscriptionPlan()` - używa `HandleResult()`
    - **Testy**: Zaktualizowano `GetSubscriptionPlanByIdQueryHandlerTests` - 9 testów (8 pozytywnych + 1 negatywny):
      - ✅ Test #1: `Handle_WithValidId_ShouldReturnSubscriptionPlanDto` - zmieniono na `result.IsSuccess` + `result.Value`
      - ✅ Test #2: `Handle_WithPlanWithActiveSubscriptions_ShouldReturnCorrectCounts` - zmieniono na `result.Value`
      - ✅ Test #3: `Handle_WithPlanWithMultipleActiveSubscriptions_ShouldReturnCorrectActiveCount` - zmieniono na `result.Value`
      - ✅ Test #4: `Handle_WithNonExistentPlan_ShouldReturnNull` - zmieniono z `result.Should().BeNull()` na `result.IsFailure` + `DomainErrors.SubscriptionPlan.NotFound`
      - ✅ Test #5: `Handle_WithoutTenantContext_ShouldThrowException` - zmieniono z `ThrowsAsync` na `result.IsFailure` + `DomainErrors.Tenant.NoTenantContext`
      - ✅ Test #6: `Handle_WithMinimalPlan_ShouldReturnDtoWithDefaultValues` - zmieniono na `result.Value`
      - ✅ Test #7 (Theory): `Handle_WithDifferentBillingPeriods_ShouldReturnCorrectBillingPeriod` - updated to use result.Value
      - ✅ Test #8: `Handle_WithUpdatedPlan_ShouldReturnUpdatedAt` - updated to use result.Value
      - ❌ Usunięto test: `Handle_WhenRepositoryThrowsException_ShouldPropagateException` (już nie dotyczy)

31. **GetSubscriptionPlansByProviderQueryHandler** ✅ COMPLETED

    - Plik: `Orbito.Application/SubscriptionPlans/Queries/GetSubscriptionPlansByProvider/GetSubscriptionPlansByProviderQueryHandler.cs`
    - Query: `GetSubscriptionPlansByProviderQuery` → **ZMIGROWANE** do `Result<PaginatedList<SubscriptionPlanListItemDto>>`
    - **Zmiany**:
      - ✅ Handler zwraca `Result<PaginatedList<SubscriptionPlanListItemDto>>` zamiast `SubscriptionPlansListDto`
      - ✅ Używa globalnego `PaginatedList<T>` z `Orbito.Application.Common.Models` zamiast lokalnego `SubscriptionPlansListDto`
      - ✅ Zastąpiono `throw new InvalidOperationException` przez `Result.Failure<PaginatedList<SubscriptionPlanListItemDto>>(DomainErrors.Tenant.NoTenantContext)`
      - ✅ Dodano `ILogger<GetSubscriptionPlansByProviderQueryHandler>`
      - ✅ Usunięto try-catch block (exceptions propagują do GlobalExceptionHandler)
    - **Błędy**:
      - No tenant context → `DomainErrors.Tenant.NoTenantContext` ✅
    - **Controller**: Zaktualizowano `SubscriptionPlansController.GetSubscriptionPlans()` - używa `HandleResult()`
    - **Testy**: Zaktualizowano `GetSubscriptionPlansByProviderQueryHandlerTests` - 8 testów (7 pozytywnych + 1 negatywny):
      - ✅ Test #1: `Handle_WithValidQuery_ShouldReturnSubscriptionPlansList` - zmieniono na `result.IsSuccess` + `result.Value`
      - ✅ Test #2: `Handle_WithActiveOnlyFilter_ShouldReturnOnlyActivePlans` - zmieniono na `result.Value`
      - ✅ Test #3: `Handle_WithPublicOnlyFilter_ShouldReturnOnlyPublicPlans` - zmieniono na `result.Value`
      - ✅ Test #4: `Handle_WithSearchTerm_ShouldFilterPlansByName` - zmieniono na `result.Value`
      - ✅ Test #5: `Handle_WithPagination_ShouldReturnCorrectPaginationInfo` - zmieniono na `result.Value`
      - ✅ Test #6: `Handle_WithEmptyResults_ShouldReturnEmptyList` - zmieniono na `result.Value`
      - ✅ Test #7: `Handle_WithPlansWithSubscriptions_ShouldCalculateSubscriptionCounts` - zmieniono na `result.Value`
      - ✅ Test #8: `Handle_WithoutTenantContext_ShouldThrowException` - zmieniono z `ThrowsAsync` na `result.IsFailure` + `DomainErrors.Tenant.NoTenantContext`
    - **DTO**: `SubscriptionPlansListDto` pozostaje w folderze Query (używany przez kontroler dla kompatybilności z frontendem), ale handler używa `PaginatedList<SubscriptionPlanListItemDto>`

32. **GetActiveSubscriptionPlansQueryHandler** ✅ COMPLETED

    - Plik: `Orbito.Application/SubscriptionPlans/Queries/GetActiveSubscriptionPlans/GetActiveSubscriptionPlansQueryHandler.cs`
    - Query: `GetActiveSubscriptionPlansQuery` → **ZMIGROWANE** do `Result<ActiveSubscriptionPlansDto>`
    - **Zmiany**:
      - ✅ Handler zwraca `Result<ActiveSubscriptionPlansDto>` zamiast `ActiveSubscriptionPlansDto`
      - ✅ Zastąpiono `throw new InvalidOperationException` przez `Result.Failure<ActiveSubscriptionPlansDto>(DomainErrors.Tenant.NoTenantContext)`
      - ✅ Dodano `ILogger<GetActiveSubscriptionPlansQueryHandler>`
      - ✅ Usunięto try-catch block (exceptions propagują do GlobalExceptionHandler)
    - **Błędy**:
      - No tenant context → `DomainErrors.Tenant.NoTenantContext` ✅
    - **Controller**: Zaktualizowano `SubscriptionPlansController.GetActiveSubscriptionPlans()` - używa `HandleResult()`
    - **Testy**: Zaktualizowano `GetActiveSubscriptionPlansQueryHandlerTests` - 9 testów (8 pozytywnych + 1 negatywny):
      - ✅ Test #1: `Handle_WithValidQuery_ShouldReturnActiveSubscriptionPlans` - zmieniono na `result.IsSuccess` + `result.Value`
      - ✅ Test #2: `Handle_WithPublicOnlyTrue_ShouldReturnOnlyPublicPlans` - zmieniono na `result.Value`
      - ✅ Test #3: `Handle_WithPublicOnlyFalse_ShouldReturnAllActivePlans` - zmieniono na `result.Value`
      - ✅ Test #4: `Handle_WithLimit_ShouldReturnLimitedPlans` - zmieniono na `result.Value`
      - ✅ Test #5: `Handle_WithPlansWithFeaturesAndLimitations_ShouldIncludeJsonProperties` - zmieniono na `result.Value`
      - ✅ Test #6: `Handle_WithPlansWithSubscriptions_ShouldCalculateActiveSubscriptionCount` - zmieniono na `result.Value`
      - ✅ Test #7: `Handle_WithEmptyResults_ShouldReturnEmptyList` - zmieniono na `result.Value`
      - ✅ Test #8: `Handle_WithDifferentBillingPeriods_ShouldReturnCorrectBillingPeriodStrings` - zmieniono na `result.Value`
      - ✅ Test #9: `Handle_WithoutTenantContext_ShouldThrowException` - zmieniono z `ThrowsAsync` na `result.IsFailure` + `DomainErrors.Tenant.NoTenantContext`

---

### **Priorytet 5 - TeamMembers Handlers** (Nowa funkcjonalność)

33. **InviteTeamMemberCommandHandler** ✅ COMPLETED

    - Plik: `Orbito.Application/Features/TeamMembers/Commands/InviteTeamMember/InviteTeamMemberCommandHandler.cs`
    - Command: `InviteTeamMemberCommand` → **ZMIGROWANE** do `Result<TeamMemberDto>`
    - **Zmiany**:
      - ✅ Usunięto try-catch block (exceptions propagują do GlobalExceptionHandler)
      - ✅ Handler już używał Result Pattern, tylko usunięto exception handling
      - ✅ Używa `Result.Success()` / `Result.Failure<TeamMemberDto>()`
    - **Błędy**:
      - No tenant context → `DomainErrors.Tenant.NoTenantContext` ✅
      - Email already exists → `DomainErrors.TeamMember.EmailAlreadyExists` ✅
      - Cannot assign owner role → `DomainErrors.TeamMember.CannotAssignOwnerRole` ✅
    - **Controller**: `TeamMembersController.InviteTeamMember()` ✅ JUŻ używa `HandleResult()`
    - **Testy**: `InviteTeamMemberCommandHandlerTests` - 7 testów ✅ JUŻ ZAKTUALIZOWANE
      - ✅ Test #1: `Handle_ValidInvitation_ShouldCreateTeamMember` - używa `result.IsSuccess` + `result.Value`
      - ✅ Test #2: `Handle_InvalidEmail_ShouldReturnFailure_WhenEmailAlreadyExists` - używa `result.IsFailure` + `result.Error`
      - ✅ Test #3: `Handle_DuplicateEmail_ShouldReturnFailure` - używa `result.IsFailure`
      - ✅ Test #4: `Handle_InvalidRole_ShouldReturnFailure_WhenTryingToAssignOwnerRole` - używa `result.IsFailure`
      - ✅ Test #5: `Handle_UserNotProvider_ShouldReturnFailure_WhenNoTenantContext` - używa `result.IsFailure`
      - ✅ Test #6 (Theory): `Handle_VariousRoles_ShouldAssignCorrectly` - używa `result.Value`
      - ✅ Test #7: `Handle_ValidInvitation_ShouldCreateTeamMemberWithInvitationToken` - używa `result.Value`
      - ❌ Usunięto test: `Handle_Exception_ShouldReturnFailure` (już nie dotyczy - exceptions propagowane)

34. **AcceptInvitationCommandHandler** ✅ COMPLETED

    - Plik: `Orbito.Application/Features/TeamMembers/Commands/AcceptInvitation/AcceptInvitationCommandHandler.cs`
    - Command: `AcceptInvitationCommand` → **ZMIGROWANE** do `Result<TeamMemberDto>`
    - **Zmiany**:
      - ✅ Usunięto zewnętrzny try-catch block (linie 36-113) - exceptions propagują do GlobalExceptionHandler
      - ✅ Usunięto wewnętrzny try-catch block wokół `AcceptInvitation()` (linie 88-96) - InvalidOperationException będzie propagowany
      - ✅ Handler już używał Result Pattern, tylko usunięto exception handling
      - ✅ Używa `Result.Success()` / `Result.Failure<TeamMemberDto>()`
    - **Błędy**:
      - Empty token → `DomainErrors.Validation.Required("Invitation token")` ✅
      - No authenticated user → `DomainErrors.General.Unauthorized` ✅
      - Token not found → `DomainErrors.TeamMember.NotFound` ✅
      - Invitation expired → `DomainErrors.TeamMember.InvitationExpired` ✅
      - Already accepted → `DomainErrors.TeamMember.AlreadyAccepted` ✅
      - Email mismatch → `DomainErrors.General.Unauthorized` ✅
    - **Controller**: `TeamMembersController.AcceptInvitation()` ✅ JUŻ używa `HandleResult()`
    - **Testy**: `AcceptInvitationCommandHandlerTests` - 7 testów ✅ JUŻ ZAKTUALIZOWANE
      - ✅ Test #1: `Handle_ValidToken_ShouldActivateTeamMember` - używa `result.IsSuccess` + `result.Value`
      - ✅ Test #2: `Handle_ExpiredToken_ShouldReturnFailure` - używa `result.IsFailure` + `result.Error`
      - ✅ Test #3: `Handle_InvalidToken_ShouldReturnFailure` - używa `result.IsFailure`
      - ✅ Test #4: `Handle_AlreadyActivated_ShouldReturnFailure` - używa `result.IsFailure`
      - ✅ Test #5: `Handle_EmptyToken_ShouldReturnFailure` - używa `result.IsFailure`
      - ✅ Test #6: `Handle_NoAuthenticatedUser_ShouldReturnFailure` - używa `result.IsFailure`
      - ✅ Test #7: `Handle_EmailMismatch_ShouldReturnFailure` - używa `result.IsFailure`

35. **RemoveTeamMemberCommandHandler** ✅ COMPLETED

    - Plik: `Orbito.Application/Features/TeamMembers/Commands/RemoveTeamMember/RemoveTeamMemberCommandHandler.cs`
    - Command: `RemoveTeamMemberCommand` → **ZMIGROWANE** do `Result` (nie zwraca wartości)
    - **Zmiany**:
      - ✅ Usunięto try-catch block (linie 32-90) - exceptions propagują do GlobalExceptionHandler
      - ✅ Handler już używał Result Pattern, tylko usunięto exception handling
      - ✅ Używa `Result.Success()` / `Result.Failure()`
      - ✅ Deactivates team member instead of deleting (soft delete pattern)
    - **Błędy**:
      - No tenant context → `DomainErrors.Tenant.NoTenantContext` ✅
      - Team member not found → `DomainErrors.TeamMember.NotFound` ✅
      - Cannot remove owner → `DomainErrors.TeamMember.CannotRemoveOwner` ✅
      - Already inactive → `DomainErrors.TeamMember.AlreadyInactive` ✅
    - **Controller**: `TeamMembersController.RemoveTeamMember()` ✅ JUŻ używa `HandleResult()` + NoContent()
    - **Testy**: `RemoveTeamMemberCommandHandlerTests` - 8 testów → 7 testów ✅
      - ✅ Test #1: `Handle_ValidRemoval_ShouldRemoveTeamMember` - używa `result.IsSuccess`
      - ✅ Test #2: `Handle_RemoveOwner_ShouldReturnFailure` - używa `result.IsFailure` + `result.Error`
      - ✅ Test #3: `Handle_NonExistentMember_ShouldReturnFailure` - używa `result.IsFailure`
      - ✅ Test #4: `Handle_UnauthorizedUser_ShouldReturnFailure_WhenNoTenantContext` - używa `result.IsFailure`
      - ✅ Test #5: `Handle_AlreadyInactiveMember_ShouldReturnFailure` - używa `result.IsFailure`
      - ✅ Test #6: `Handle_CrossTenantAccess_ShouldReturnFailure` - używa `result.IsFailure`
      - ✅ Test #7 (Theory): `Handle_RemoveNonOwner_ShouldSucceed` - używa `result.IsSuccess`
      - ✅ Test #8: `Handle_RemovalWithoutReason_ShouldSucceed` - używa `result.IsSuccess`
      - ❌ Usunięto test: `Handle_Exception_ShouldReturnFailure` (już nie dotyczy - exceptions propagowane)

36. **UpdateTeamMemberRoleCommandHandler** ✅ COMPLETED

    - Plik: `Orbito.Application/Features/TeamMembers/Commands/UpdateTeamMemberRole/UpdateTeamMemberRoleCommandHandler.cs`
    - Command: `UpdateTeamMemberRoleCommand` → **ZMIGROWANE** do `Result<TeamMemberDto>`
    - **Zmiany**:
      - ✅ Usunięto try-catch block (linie 35-95) - exceptions propagują do GlobalExceptionHandler
      - ✅ Handler już używał Result Pattern, tylko usunięto exception handling
      - ✅ Używa `Result.Success()` / `Result.Failure<TeamMemberDto>()`
      - ✅ Zawiera private method `ValidateRoleChange()` do walidacji zmian roli
    - **Błędy**:
      - No tenant context → `DomainErrors.Tenant.NoTenantContext` ✅
      - Team member not found → `DomainErrors.TeamMember.NotFound` ✅
      - Team member inactive → `DomainErrors.TeamMember.Inactive` ✅
      - Same role → `DomainErrors.TeamMember.SameRole` ✅
      - Cannot assign owner role → `DomainErrors.TeamMember.CannotAssignOwnerRole` ✅
      - Cannot demote owner → `DomainErrors.TeamMember.CannotDemoteOwner` ✅
    - **Controller**: `TeamMembersController.UpdateTeamMemberRole()` ✅ JUŻ używa `HandleResult()`
    - **Testy**: `UpdateTeamMemberRoleCommandHandlerTests` - 10 testów → 9 testów ✅
      - ✅ Test #1: `Handle_ValidRoleUpdate_ShouldUpdateRole` - używa `result.IsSuccess` + `result.Value`
      - ✅ Test #2: `Handle_OwnerRoleChange_ShouldReturnFailure` - używa `result.IsFailure` + `result.Error`
      - ✅ Test #3: `Handle_NonExistentMember_ShouldReturnFailure` - używa `result.IsFailure`
      - ✅ Test #4: `Handle_UnauthorizedUser_ShouldReturnFailure_WhenNoTenantContext` - używa `result.IsFailure`
      - ✅ Test #5: `Handle_SameRole_ShouldReturnFailure` - używa `result.IsFailure`
      - ✅ Test #6: `Handle_OwnerDemotion_ShouldReturnFailure` - używa `result.IsFailure`
      - ✅ Test #7: `Handle_InactiveMember_ShouldReturnFailure` - używa `result.IsFailure`
      - ✅ Test #8 (Theory): `Handle_RoleChange_ShouldUpdateCorrectly` - używa `result.IsSuccess` + `result.Value`
      - ✅ Test #9: `Handle_CrossTenantAccess_ShouldReturnFailure` - używa `result.IsFailure`
      - ❌ Usunięto test: `Handle_Exception_ShouldReturnFailure` (już nie dotyczy - exceptions propagowane)

37. **GetTeamMemberByIdQueryHandler** ✅ COMPLETED

    - Plik: `Orbito.Application/Features/TeamMembers/Queries/GetTeamMemberById/GetTeamMemberByIdQueryHandler.cs`
    - Query: `GetTeamMemberByIdQuery` → **ZMIGROWANE** do `Result<TeamMemberDto>`
    - **Zmiany**:
      - ✅ Usunięto try-catch block (linie 34-71) - exceptions propagują do GlobalExceptionHandler
      - ✅ Handler już używał Result Pattern, tylko usunięto exception handling
      - ✅ Używa `Result.Success()` / `Result.Failure<TeamMemberDto>()`
      - ✅ Read-only query - zwraca TeamMemberDto dla istniejącego team member
    - **Błędy**:
      - No tenant context → `DomainErrors.Tenant.NoTenantContext` ✅
      - Team member not found → `DomainErrors.TeamMember.NotFound` ✅
    - **Controller**: `TeamMembersController.GetTeamMemberById()` ✅ JUŻ używa `HandleResult()`
    - **Testy**: `GetTeamMemberByIdQueryHandlerTests` - 8 testów → 7 testów ✅
      - ✅ Test #1: `Handle_ValidId_ShouldReturnTeamMember` - używa `result.IsSuccess` + `result.Value`
      - ✅ Test #2: `Handle_NonExistentId_ShouldReturnFailure` - używa `result.IsFailure` + `result.Error`
      - ✅ Test #3: `Handle_CrossTenantAccess_ShouldReturnFailure` - używa `result.IsFailure`
      - ✅ Test #4: `Handle_NoTenantContext_ShouldReturnFailure` - używa `result.IsFailure`
      - ✅ Test #5 (Theory): `Handle_ValidId_ShouldReturnTeamMemberWithCorrectRole` - używa `result.IsSuccess` + `result.Value`
      - ✅ Test #6: `Handle_InactiveMember_ShouldStillReturn` - używa `result.IsSuccess` (query zwraca też nieaktywnych)
      - ✅ Test #7: `Handle_ValidId_ShouldIncludeAllMemberDetails` - używa `result.Value` (weryfikacja wszystkich pól DTO)
      - ❌ Usunięto test: `Handle_Exception_ShouldReturnFailure` (już nie dotyczy - exceptions propagowane)

38. **GetTeamMembersQueryHandler** ✅ COMPLETED

    - Plik: `Orbito.Application/Features/TeamMembers/Queries/GetTeamMembers/GetTeamMembersQueryHandler.cs`
    - Query: `GetTeamMembersQuery` → **ZMIGROWANE** do `Result<IEnumerable<TeamMemberDto>>`
    - **Zmiany**:
      - ✅ Usunięto try-catch block (linie 36-88) - exceptions propagują do GlobalExceptionHandler
      - ✅ Handler już używał Result Pattern, tylko usunięto exception handling
      - ✅ Używa `Result.Success()` / `Result.Failure<IEnumerable<TeamMemberDto>>()`
      - ✅ Query z filtrowaniem (role filter, include inactive) i paginacją
    - **Błędy**:
      - No tenant context → `DomainErrors.Tenant.NoTenantContext` ✅
    - **Funkcjonalności**:
      - Filtrowanie po roli (RoleFilter: Owner/Admin/Member)
      - Opcja włączenia nieaktywnych członków (IncludeInactive)
      - Paginacja (PageNumber, PageSize)
      - In-memory filtrowanie i paginacja (po pobraniu z repozytorium)
    - **Controller**: `TeamMembersController.GetTeamMembers()` ✅ JUŻ używa `HandleResult()`
    - **Testy**: `GetTeamMembersQueryHandlerTests` - 10 testów → 9 testów ✅
      - ✅ Test #1: `Handle_ValidRequest_ShouldReturnAllActiveMembers` - używa `result.IsSuccess` + `result.Value`
      - ✅ Test #2: `Handle_EmptyTeam_ShouldReturnEmptyList` - używa `result.IsSuccess` + pusty result
      - ✅ Test #3: `Handle_WithPagination_ShouldReturnPagedResults` - używa `result.Value` (weryfikacja paginacji)
      - ✅ Test #4: `Handle_FilterByRole_ShouldReturnFilteredResults` - używa `result.Value` (filtrowanie po Admin)
      - ✅ Test #5: `Handle_IncludeInactive_ShouldReturnAllMembers` - używa `result.Value` (aktywni + nieaktywni)
      - ✅ Test #6: `Handle_NoTenantContext_ShouldReturnFailure` - używa `result.IsFailure` + `result.Error`
      - ✅ Test #7: `Handle_InvalidRoleFilter_ShouldReturnAllMembers` - używa `result.Value` (ignoruje nieprawidłowy filtr)
      - ✅ Test #8 (Theory): `Handle_FilterBySpecificRole_ShouldReturnOnlyThatRole` - używa `result.Value` (weryfikacja każdej roli)
      - ❌ Usunięto test: `Handle_Exception_ShouldReturnFailure` (już nie dotyczy - exceptions propagowane)

---

## 🧪 Wpływ na Testy

### Kategorie Testów do Aktualizacji

#### 1. Command Handler Tests (24 pliki)

**Obecny wzorzec:**

```csharp
var result = await handler.Handle(command, CancellationToken.None);

result.Should().NotBeNull();
result.Success.Should().BeTrue();
result.Message.Should().Be("Success message");
result.Data.Id.Should().Be(expectedId);
```

**Nowy wzorzec:**

```csharp
var result = await handler.Handle(command, CancellationToken.None);

result.IsSuccess.Should().BeTrue();
result.Value.Id.Should().Be(expectedId);

// Dla błędów:
result.IsFailure.Should().BeTrue();
result.Error.Code.Should().Be("Subscription.NotFound");
result.Error.Message.Should().Contain("not found");
```

**Testowane pliki:**

- CreateSubscriptionCommandHandlerTests (5 testów) → Assert.ThrowsAsync → Result.IsFailure
- ActivateSubscriptionCommandHandlerTests
- CancelSubscriptionCommandHandlerTests
- UpgradeSubscriptionCommandHandlerTests
- CreateClientCommandHandlerTests (12 testów) → result.Success → result.IsSuccess
- ActivateClientCommandHandlerTests
- DeactivateClientCommandHandlerTests
- DeleteClientCommandHandlerTests
- UpdateClientCommandHandlerTests
- CreateSubscriptionPlanCommandHandlerTests
- UpdateSubscriptionPlanCommandHandlerTests
- DeleteSubscriptionPlanCommandHandlerTests
- CloneSubscriptionPlanCommandHandlerTests
- InviteTeamMemberCommandHandlerTests
- AcceptInvitationCommandHandlerTests
- RemoveTeamMemberCommandHandlerTests
- UpdateTeamMemberRoleCommandHandlerTests
- UpdateProviderCommandHandlerTests
- DeleteProviderCommandHandlerTests

#### 2. Query Handler Tests (14 plików)

**Obecny wzorzec:**

```csharp
var result = await handler.Handle(query, CancellationToken.None);

result.Should().NotBeNull();
// lub result.Should().BeNull() dla not found
result.SubscriptionId.Should().Be(expectedId);
```

**Nowy wzorzec:**

```csharp
var result = await handler.Handle(query, CancellationToken.None);

result.IsSuccess.Should().BeTrue();
result.Value.Should().NotBeNull();
result.Value.SubscriptionId.Should().Be(expectedId);

// Dla not found:
result.IsFailure.Should().BeTrue();
result.Error.Code.Should().Be("Subscription.NotFound");
```

**Testowane pliki:**

- GetSubscriptionByIdQueryHandlerTests → null checks → Result.IsFailure
- GetSubscriptionsByClientQueryHandlerTests
- GetClientByIdQueryHandlerTests
- GetClientsByProviderQueryHandlerTests
- SearchClientsQueryHandlerTests
- GetSubscriptionPlanByIdQueryHandlerTests
- GetSubscriptionPlansByProviderQueryHandlerTests
- GetActiveSubscriptionPlansQueryHandlerTests
- GetTeamMemberByIdQueryHandlerTests
- GetTeamMembersQueryHandlerTests
- GetProviderByIdQueryHandlerTests
- GetProviderByUserIdQueryHandlerTests
- GetAllProvidersQueryHandlerTests

#### 3. Integration Tests (2 pliki)

- ClientIntegrationTests → pełne flow z bazą danych
- ProviderIntegrationTests → już częściowo zmigrowane

### Szacowana Liczba Testów do Aktualizacji

- **Command Handler Tests**: ~60-80 testów
- **Query Handler Tests**: ~30-40 testów
- **Integration Tests**: ~10-15 testów
- **RAZEM**: ~100-135 testów

---

## 🎯 Nowe Domain Errors do Dodania

### Subscription Errors

```csharp
public static class Subscription
{
    public static Error NotFound => Error.NotFound(
        "Subscription.NotFound",
        "Subscription with the specified ID was not found.");

    public static Error NotActive => Error.Validation(
        "Subscription.NotActive",
        "Subscription is not active.");

    public static Error AlreadyActive => Error.Conflict(
        "Subscription.AlreadyActive",
        "Subscription is already active.");

    public static Error AlreadyCancelled => Error.Conflict(
        "Subscription.AlreadyCancelled",
        "Subscription is already cancelled.");

    public static Error CannotUpgrade => Error.Validation(
        "Subscription.CannotUpgrade",
        "Subscription cannot be upgraded.");

    public static Error CannotDowngrade => Error.Validation(
        "Subscription.CannotDowngrade",
        "Subscription cannot be downgrade.");

    public static Error CannotSuspend => Error.Validation(
        "Subscription.CannotSuspend",
        "Subscription cannot be suspended.");

    public static Error CannotResume => Error.Validation(
        "Subscription.CannotResume",
        "Subscription cannot be resumed.");

    public static Error CannotRenew => Error.Validation(
        "Subscription.CannotRenew",
        "Subscription cannot be renewed.");
}
```

### SubscriptionPlan Errors

```csharp
public static class SubscriptionPlan
{
    public static Error NotFound => Error.NotFound(
        "SubscriptionPlan.NotFound",
        "Subscription plan with the specified ID was not found.");

    public static Error Inactive => Error.Validation(
        "SubscriptionPlan.Inactive",
        "Subscription plan is not active.");

    public static Error NameAlreadyExists => Error.Conflict(
        "SubscriptionPlan.NameAlreadyExists",
        "A subscription plan with this name already exists.");
}
```

### Rozszerzenie Client Errors

```csharp
public static class Client
{
    // Existing...
    public static Error UserAlreadyExists => Error.Conflict(
        "Client.UserAlreadyExists",
        "A client with this user already exists.");
}
```

---

## 📝 Kontrolery do Aktualizacji

### 1. SubscriptionsController (CAŁOŚĆ)

**Obecny wzorzec:**

```csharp
var result = await Mediator.Send(command);
if (!result.Success)
{
    return BadRequest(result);
}
return Ok(result);
```

**Nowy wzorzec:**

```csharp
var result = await Mediator.Send(command);
return HandleResult(result);
```

**Endpointy do zmiany (10):**

- CreateSubscription (+ CreatedAtAction handling)
- GetSubscriptions
- GetSubscriptionById (+ NotFound handling)
- GetSubscriptionsByClient
- GetExpiringSubscriptions
- ActivateSubscription
- CancelSubscription
- SuspendSubscription
- ResumeSubscription
- UpgradeSubscription
- DowngradeSubscription
- RenewSubscription

### 2. ClientsController

**Endpointy do zmiany (8):**

- CreateClient (+ CreatedAtAction handling)
- GetClients ⚠️ SPECJALNY PRZYPADEK - custom wrapping dla frontendu
- GetClientById
- UpdateClient
- DeleteClient
- ActivateClient
- DeactivateClient
- SearchClients

### 3. SubscriptionPlansController (NOWY)

Prawdopodobnie wymaga stworzenia kontrolera lub aktualizacji istniejącego.

### 4. TeamMembersController (NOWY)

Prawdopodobnie już istnieje, wymaga aktualizacji.

---

## 🚀 Plan Wykonania

### Faza 2A - Subscription Handlers (2-3 dni)

1. Dodaj nowe Domain Errors dla Subscription i SubscriptionPlan
2. Migruj command handlers (8 handlerów)
3. Migruj query handlers (4 handlery)
4. Zaktualizuj testy (15-20 testów)
5. Zaktualizuj SubscriptionsController (12 endpointów)
6. Uruchom pełne testy aplikacji
7. Usuń stare Result classes

### Faza 2B - Client Handlers (2 dni)

1. Rozszerz Domain Errors dla Client
2. Migruj command handlers (5 handlerów) - zaczynając od CreateClient
3. Migruj query handlers (3 handlery)
4. Zaktualizuj testy (20-25 testów)
5. Zaktualizuj ClientsController (8 endpointów)
6. Uruchom pełne testy
7. Usuń stare Result classes

### Faza 2C - Provider i SubscriptionPlan Handlers (1-2 dni)

1. Dodaj brakujące Domain Errors
2. Migruj pozostałe Provider handlers (2 handlery + 3 query)
3. Migruj SubscriptionPlan handlers (4 command + 3 query)
4. Zaktualizuj testy (15-20 testów)
5. Uruchom pełne testy
6. Usuń stare Result classes

### Faza 2D - TeamMembers Handlers (1 dzień)

1. Dodaj Domain Errors dla TeamMember
2. Migruj command handlers (4 handlery)
3. Migruj query handlers (2 handlery)
4. Zaktualizuj testy (10-12 testów)
5. Zaktualizuj TeamMembersController
6. Uruchom pełne testy

---

## ⚠️ Ryzyka i Uwagi

### 1. Kontroler ClientsController.GetClients()

**Problem:** Specjalna obsługa dla kompatybilności z frontendem

```csharp
return Ok(new
{
    isSuccess = true,
    value = new { ... },
    error = default(string)
});
```

**Rozwiązanie:** Zachować custom wrapping, ale wewnętrznie używać Result Pattern w handlerze.

### 2. Null returns w Query Handlers

**Przed:**

```csharp
var result = await Mediator.Send(query);
if (result == null)
{
    return NotFound($"Subscription with ID {id} not found");
}
```

**Po:**

```csharp
var result = await Mediator.Send(query);
return HandleResult(result); // BaseController automatycznie zwróci 404 dla NotFound errors
```

### 3. Exceptions vs Result Pattern

Niektóre handlery (np. CreateSubscriptionCommandHandler) używają `throw new InvalidOperationException()`.
Należy zamienić na:

```csharp
return Result.Failure<T>(DomainErrors.Subscription.NotFound);
```

### 4. Try-Catch Blocks

Handlery takie jak CreateClientCommandHandler używają try-catch.
Należy usunąć i obsługiwać błędy przez Result Pattern:

```csharp
// PRZED:
try
{
    // logic
}
catch (Exception ex)
{
    return CreateClientResult.FailureResult($"An error occurred: {ex.Message}");
}

// PO:
if (errorCondition)
{
    return Result.Failure<ClientDto>(DomainErrors.Client.NotFound);
}
// Dla nieoczekiwanych błędów w Infrastructure - niech propagują się do GlobalExceptionHandler
```

---

## 📊 Metryki Sukcesu

### Cel Końcowy Fazy 2

- ✅ 38 dodatkowych handlerów zmigrowanych (100% coverage poza Payment)
- ✅ ~100-135 testów zaktualizowanych
- ✅ 4 kontrolery zaktualizowane (Subscriptions, Clients, SubscriptionPlans, TeamMembers)
- ✅ Wszystkie testy przechodzą (zielone)
- ✅ Aplikacja kompiluje się bez ostrzeżeń
- ✅ Usunięte wszystkie stare Result classes (oprócz Infrastructure interfaces)

### Weryfikacja

```bash
# Uruchom wszystkie testy
dotnet test

# Sprawdź coverage
dotnet test --collect:"XPlat Code Coverage"

# Zweryfikuj kompilację
dotnet build --no-incremental
```

---

## 🔗 Powiązane Dokumenty

- [RESULT_PATTERN_MIGRATION_GUIDE.md](RESULT_PATTERN_MIGRATION_GUIDE.md) - Przewodnik po wzorcu Result
- [CLAUDE.md](../CLAUDE.md) - Zasady pracy dla projektu
- [DomainErrors.cs](../Orbito.Domain/Errors/DomainErrors.cs) - Katalog błędów domenowych
- [BaseController.cs](../Orbito.API/Controllers/BaseController.cs) - Metody HandleResult

---

**Wersja:** 1.0
**Data utworzenia:** 2025-01-29
**Autor:** Claude Code Analysis
**Status:** DRAFT - Do Akceptacji
