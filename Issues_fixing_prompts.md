# Orbito Platform - Issues Fixing & Feature Enhancements Prompts

> **🚀 NOWA FAZA - Issue Fixing & Feature Enhancements**
>
> Ten plik zawiera prompty implementacyjne dla krytycznych poprawek i nowych funkcjonalności biznesowych.
> Zaczynamy od **ISSUE 1.1** (Zarządzanie kontem Admina).

> **INSTRUKCJA DLA AGENTÓW AI**
>
> Ten plik zawiera szczegółowe prompty dla każdego bloku implementacji.
> Każdy blok jest oznaczony markerami `<!-- BLOCK_START: ISSUE_X.X -->` i `<!-- BLOCK_END: ISSUE_X.X -->`.
>
> **Workflow:**
>
> 1. 🚨 **PRZECZYTAJ `API_RULES.md` PRZED ROZPOCZĘCIEM!** (obowiązkowe)
> 2. Znajdź swój blok używając markera
> 3. Przeczytaj CAŁY prompt przed rozpoczęciem pracy
> 4. Wykonuj DOKŁADNIE kroki opisane w promptcie
> 5. Na końcu przejdź przez CHECKLIST WERYFIKACJI
> 6. Zaktualizuj `claude-progress.txt`

---

## 🚨 KRYTYCZNE ZASADY – PRZECZYTAJ PRZED KAŻDYM BLOKIEM!

> **UWAGA**: Te zasady są BEZWZGLĘDNE. Ich złamanie = bug w produkcji.
> Pochodzą z `api-rules.md` i doświadczeń z poprzednich faz.

### ❌ ABSOLUTNIE ZABRONIONE:

1. **Hardcoded Data**: `const data = 0`, `const items = []`, `const user = { name: "Test" }`
2. **Mock Functions**: `console.log('TODO: call API')`, placeholder funkcje
3. **TODO Comments**: Zostawianie TODO bez implementacji
4. **Pomijanie Auth**: Każdy request MUSI mieć `Authorization: Bearer ...` header
5. **Niebezpieczne metody repozytorium**: Używaj TYLKO metod `ForTenantAsync`, `ForClientAsync`, `UnsafeAsync` (po weryfikacji sygnatury webhook)

### ✅ ZAWSZE WYMAGANE:

1. **Backend Result\<T\> Pattern**: Każdy handler MUSI zwracać `Result<T>` lub `Result`. Używaj `DomainErrors` z katalogu `Orbito.Domain.Errors.DomainErrors`.
2. **Frontend: Import hooków z Orval**: Po zmianach w backendzie ZAWSZE uruchom `npm run api:generate` i importuj hooki z `@/core/api/generated/`.
3. **Obsługa 3 stanów UI**: loading (Skeleton), error (ErrorMessage), data (actual content).
4. **Weryfikacja Network Tab**: Przed zamknięciem bloku sprawdź DevTools → 200 OK + Authorization header.
5. **Multi-tenancy**: Każde zapytanie do bazy MUSI przechodzić przez `ITenantContext`. Nigdy nie twórz zapytań bez filtra tenanta.
6. **FluentValidation**: Każdy nowy Command MUSI mieć odpowiedni Validator.

---

## 📊 PODSUMOWANIE BLOKÓW

| Blok | Faza | Opis | Status | Zależności |
|---|---|---|---|---|
| ISSUE 1.1 | Admin | Singleton PlatformAdmin & Izolacja | ✅ | Brak |
| ISSUE 2.1 | Rejestracja | Auto-tworzenie Provider przy rejestracji | ⬜ | Brak |
| ISSUE 3.1 | Zapraszanie | Backend: Token, Email Service, Endpoint | ⬜ | ISSUE 2.1 |
| ISSUE 3.2 | Zapraszanie | Backend: Confirmation Endpoint & Status | ⬜ | ISSUE 3.1 |
| ISSUE 3.3 | Zapraszanie | Frontend: Formularz Invite Client | ⬜ | ISSUE 3.2 |
| ISSUE 3.4 | Zapraszanie | Frontend: Strona potwierdzenia /portal/confirm | ⬜ | ISSUE 3.3 |
| ISSUE 4.1 | Płatności | Stripe Elements w Client Portal | ⬜ | ISSUE 3.4 |
| ISSUE 4.2 | Płatności | Webhook Sync → Provider Dashboard | ⬜ | ISSUE 4.1 |
| ISSUE 5.1 | Restrykcje | Hardening Client Portal (PortalGuard + API) | ⬜ | ISSUE 4.2 |
| ISSUE 6.1 | Trial | Backend: Domain Model Trial + Auto-create przy rejestracji + Provider jako klient Admina | ⬜ | ISSUE 1.1, ISSUE 2.1 |
| ISSUE 6.2 | Trial | Backend: Background Job – powiadomienia 5d/3d/24h przed końcem triala | ⬜ | ISSUE 6.1, ISSUE 3.1 |
| ISSUE 6.3 | Trial | Backend: Wygaśnięcie triala, email z instrukcjami płatności | ⬜ | ISSUE 6.2 |
| ISSUE 6.4 | Trial | Frontend: Wybór planu przy rejestracji + UI płatności za subskrypcję Providera | ⬜ | ISSUE 6.3, ISSUE 4.1 |

---

<!-- BLOCK_START: ISSUE_1.1 -->
#### 🛠️ ISSUE 1.1: Singleton PlatformAdmin & Izolacja Danych

| # | Zadanie | Priorytet | Status | Opis |
|---|---|---|---|---|
| 1 | 🔴 Singleton PlatformAdmin Guard | Krytyczne | ⬜ | Backend: Walidacja że tylko 1 Admin może istnieć w systemie |
| 2 | 🔴 Admin Tenant Isolation | Krytyczne | ⬜ | Backend: Admin widzi TYLKO swoich bezpośrednich klientów, NIE klientów innych Providerów |
| 3 | 🟡 Admin Dashboard Scope | Ważne | ⬜ | Frontend: Dashboard Admina wyświetla jedynie dane z jego własnego kontekstu tenanta |
| 4 | 🟡 Seed PlatformAdmin | Ważne | ⬜ | Backend: Seed data / migracja gwarantująca istnienie dokładnie jednego admina |

**Blok 1.1 - Wymagania wejściowe**: Brak (niezależny blok)
**Blok 1.1 - Rezultat**: W systemie istnieje dokładnie jedno konto PlatformAdmin. Admin ma pełen dostęp do funkcji programu, ale widzi wyłącznie swoich bezpośrednich klientów – nie ma dostępu do klientów innych Providerów.

---

##### 🤖 PROMPT

**Działaj jako Senior Backend (.NET 9) & Frontend (Next.js 15) Developer pracujący nad Orbito Platform.**

**CEL BIZNESOWY:**
Zapewnić, że w systemie może istnieć dokładnie jedno konto PlatformAdmin. Admin ma pełen dostęp do funkcji platformy, lecz obowiązuje go ścisła izolacja – NIE widzi klientów stworzonych przez innych Providerów (nie widzi "klientów swoich klientów").

**KROKI DO WYKONANIA (BACKEND - CQRS, MediatR):**

* **Domain/Entities:**
    * W encji `Provider` (lub `User`) dodaj flagę/rolę `IsPlatformAdmin` lub zweryfikuj, że rola `PlatformAdmin` z enuma `UserRole` jest odpowiednio obsługiwana.
    * Rozważ Value Object `PlatformAdminGuard` w Domain Services, który waliduje regułę "max 1 admin".

* **Application:**
    * **`RegisterCommandHandler`** (lub odpowiedni handler rejestracji): Dodaj walidację na początku handlera – jeśli przypisywana rola to `PlatformAdmin`, sprawdź w bazie czy nie istnieje już inny admin. Jeśli istnieje → zwróć `Result.Failure(DomainErrors.Admin.AlreadyExists)`.
    * Dodaj nowy błąd domenowy: `DomainErrors.Admin.AlreadyExists` = `new Error("Admin.AlreadyExists", "Platform Admin account already exists.")`.
    * **`GetClientsQueryHandler`** (i inne handlery pobierające dane klientów): Upewnij się, że zapytania ZAWSZE filtrują przez `ITenantContext.CurrentTenantId`. Admin jako Provider ma swój własny `TenantId` – widzi jedynie klientów przypisanych do TEGO tenanta.

* **Security:**
    * `ITenantContext` MUSI być respektowany nawet dla PlatformAdmin. Admin NIE jest "super-userem" omijającym tenant isolation.
    * W `ProviderTeamHandler` (authorization handler) upewnij się, że `PlatformAdmin` ma dostęp do zasobów, ale wyłącznie w obrębie swojego tenanta.
    * Dodaj integration test: Admin próbuje pobrać klienta z innego tenanta → oczekiwany 404 Not Found (lub 403 Forbidden).

* **API/Controllers:**
    * Żadne zmiany w kontrolerach nie powinny być konieczne – izolacja działa na poziomie repozytoriów i `ITenantContext`.
    * Opcjonalnie: Endpoint `GET /api/Admin/status` zwracający informację czy Admin jest już utworzony (dla UI seed panelu).

* **Infrastructure/Seed:**
    * W `ApplicationDbContext.OnModelCreating()` lub w `SeedData` dodaj logikę tworzenia jednego konta PlatformAdmin z domyślnymi kredencjalami (do zmiany przy pierwszym logowaniu).
    * W migracji dodaj unique constraint na rolę PlatformAdmin (np. filtered unique index: `WHERE Role = 'PlatformAdmin'`).

**KROKI DO WYKONANIA (FRONTEND - Next.js 15):**

* **API & Hooks:** Uruchom `npm run api:generate` po zmianach w backendzie. Jeśli dodano nowy endpoint `/api/Admin/status`, wygeneruj hook.
* **Components:** Brak istotnych zmian UI – Admin korzysta z tego samego dashboardu co Provider. Kluczowe to, że widziane dane filtrowane są po stronie backendu.
* **Routing/State:** Brak zmian. Rola PlatformAdmin już powinna być obsługiwana w `TenantGuard` i `ProviderTeamHandler`. Zweryfikuj, że Admin logujący się widzi `/dashboard` (nie `/portal`).

**✅ CHECKLIST WERYFIKACJI (Dla Agenta):**

* [x] W bazie danych istnieje dokładnie 1 rekord z rolą PlatformAdmin
* [x] Próba rejestracji drugiego PlatformAdmin zwraca błąd `Admin.AlreadyExists` (sprawdź w Network Tab → 400/409)
* [x] Admin zalogowany w UI widzi TYLKO swoich klientów (Network Tab: `GET /api/Clients` zwraca klientów z tenanta Admina)
* [x] Admin NIE widzi klientów przypisanych do innych Providerów (sprawdź bezpośrednio w bazie, porównaj TenantId)
* [ ] Integration test: Admin wywołuje `GET /api/Clients/{idKlientaInnegoProvdera}` → 404 Not Found
* [x] `dotnet build` → ZERO błędów (uwaga: Orbito.Tests ma pre-existing CS0111, niezwiązany z ISSUE_1.1)
* [x] Git commit: `fix(admin): enforce singleton PlatformAdmin with strict tenant isolation`

<!-- BLOCK_END: ISSUE_1.1 -->

---

<!-- BLOCK_START: ISSUE_2.1 -->
#### 🛠️ ISSUE 2.1: Automatyzacja Rejestracji – Auto-tworzenie Provider

| # | Zadanie | Priorytet | Status | Opis |
|---|---|---|---|---|
| 1 | 🔴 Auto-create Provider entity | Krytyczne | ⬜ | Backend: `RegisterCommandHandler` tworzy encję `Provider` i wiąże ją z nowym userem |
| 2 | 🔴 Auto-assign TenantId | Krytyczne | ⬜ | Backend: Nowy Provider dostaje unikalny `TenantId`, zapisywany w JWT claims |
| 3 | 🔴 Auto-create TeamMember (Owner) | Krytyczne | ⬜ | Backend: Rejestrujący się user staje się automatycznie TeamMember z rolą Owner |
| 4 | 🟡 Transakcyjność | Ważne | ⬜ | Backend: Cały flow (User + Provider + TeamMember) w jednej transakcji DB |
| 5 | 🟡 Walidacja duplikatów | Ważne | ⬜ | Backend: Sprawdzanie czy email nie jest już zarejestrowany |

**Blok 2.1 - Wymagania wejściowe**: Brak (niezależny blok, ale logicznie powiązany z ISSUE 1.1)
**Blok 2.1 - Rezultat**: Rejestracja nowego użytkownika przez `POST /api/Account/register` automatycznie tworzy encję Provider, przypisuje TenantId i dodaje usera jako TeamMember z rolą Owner. Cały proces jest atomowy (jedna transakcja).

---

##### 🤖 PROMPT

**Działaj jako Senior Backend (.NET 9) Developer pracujący nad Orbito Platform.**

**CEL BIZNESOWY:**
Proces rejestracji nowego użytkownika (przez `POST /api/Account/register`) musi automatycznie tworzyć w systemie encję Provider, podpinać pod niego unikalny TenantId i dodawać rejestrującego się usera jako TeamMember z rolą Owner. Cały proces musi być atomowy.

**KROKI DO WYKONANIA (BACKEND - CQRS, MediatR):**

* **Domain/Entities:**
    * Zweryfikuj, że encja `Provider` ma factory method `Create(string name, string email)` zwracające `Result<Provider>`.
    * Zweryfikuj, że encja `TeamMember` ma factory method `CreateOwner(Guid userId, TenantId tenantId)`.
    * Upewnij się, że `TenantId` jest generowany jako `TenantId.Create(Guid.NewGuid())`.

* **Application:**
    * **`RegisterCommandHandler`** – zmodyfikuj handler, aby PO utworzeniu użytkownika (Identity):
        1. Tworzył nową encję `Provider` z danymi z komendy rejestracji (nazwa, email).
        2. Generował nowy `TenantId` i przypisywał go do Providera.
        3. Tworzył rekord `TeamMember` z rolą `Owner` (TeamMemberRole.Owner), powiązany z userId i TenantId.
        4. Zapisywał wszystko w jednej transakcji (`_unitOfWork.SaveChangesAsync()` na końcu).
    * Jeśli którykolwiek krok zawiedzie, cały process musi się wycofać (rollback).
    * Zwracaj `Result.Success()` lub `Result.Failure(DomainErrors.Account.RegistrationFailed)`.
    * Dodaj `RegisterCommandValidator` z FluentValidation – walidacja email (format + unikalność), hasła (minimalna długość, złożoność), nazwy firmy.

* **Security:**
    * Po rejestracji, przy logowaniu, `JwtTokenService` musi dodawać do tokena claims: `tenant_id`, `team_role: "Owner"`, `team_member_id`.
    * Zweryfikuj, że `LoginCommandHandler` pobiera `TeamMember` i wstawia odpowiednie claims do JWT.

* **API/Controllers:**
    * Endpoint `POST /api/Account/register` powinien przyjmować: `Email`, `Password`, `FirstName`, `LastName`, `CompanyName` (opcjonalne).
    * Zwracaj `201 Created` z tokenem JWT (lub `200 OK` + token).
    * Dodaj `[ProducesResponseType]` atrybuty dla Swagger/OpenAPI.

* **Infrastructure/Database:**
    * Sprawdź, czy migracja poprawnie tworzy tabele `Providers` i `TeamMembers` z odpowiednimi relacjami (FK).
    * Dodaj unique constraint na `Provider.Email`.

**KROKI DO WYKONANIA (FRONTEND - Next.js 15):**

* **API & Hooks:** Uruchom `npm run api:generate` jeśli zmienił się kontrakt `POST /api/Account/register` (nowe pola w request DTO).
* **Components:** Zaktualizuj `RegisterForm` w `src/features/auth/components/` aby zawierał pole `CompanyName` (jeśli dodane).
* **Routing/State:** Po rejestracji user powinien być automatycznie przekierowany na `/dashboard` z prawidłowym `tenantId` w sesji.

**✅ CHECKLIST WERYFIKACJI (Dla Agenta):**

* [ ] `POST /api/Account/register` z nowymi danymi → 201 Created (Network Tab)
* [ ] W bazie danych: nowy rekord w `AspNetUsers`, nowy rekord w `Providers`, nowy rekord w `TeamMembers` (rola Owner)
* [ ] TenantId w `Providers` jest unikalne i niepuste (sprawdź SQL)
* [ ] Logowanie po rejestracji: JWT zawiera claims `tenant_id`, `team_role`, `team_member_id` (dekoduj token na jwt.io)
* [ ] Frontend: Po rejestracji user ląduje na `/dashboard` z poprawnymi danymi
* [ ] Próba rejestracji z istniejącym emailem → 400 Bad Request z komunikatem błędu
* [ ] `npm run typecheck` → ZERO błędów (frontend)
* [ ] `dotnet build` → ZERO błędów (backend)
* [ ] Git commit: `feat(registration): auto-create Provider and TeamMember on registration`

<!-- BLOCK_END: ISSUE_2.1 -->

---

<!-- BLOCK_START: ISSUE_3.1 -->
#### 🛠️ ISSUE 3.1: Zapraszanie Klienta – Backend: Token & Email Service

| # | Zadanie | Priorytet | Status | Opis |
|---|---|---|---|---|
| 1 | 🔴 ClientInvitationToken ValueObject | Krytyczne | ⬜ | Domain: Bezpieczny token zaproszenia z datą wygaśnięcia |
| 2 | 🔴 IEmailService interface | Krytyczne | ⬜ | Application: Kontrakt usługi emailowej |
| 3 | 🔴 EmailService implementation | Krytyczne | ⬜ | Infrastructure: Implementacja wysyłki email (SMTP / SendGrid / Mailgun) |
| 4 | 🔴 InviteClientCommand & Handler | Krytyczne | ⬜ | Application: CQRS command tworzący klienta Inactive + generujący token + wysyłający email |
| 5 | 🟡 Email Template | Ważne | ⬜ | Infrastructure: HTML template zaproszenia z linkiem do portalu |

**Blok 3.1 - Wymagania wejściowe**: ISSUE 2.1 (rejestracja tworzy Provider)
**Blok 3.1 - Rezultat**: Backend potrafi wygenerować bezpieczny token zaproszenia, stworzyć klienta ze statusem Inactive i wysłać email z linkiem aktywacyjnym.

---

##### 🤖 PROMPT

**Działaj jako Senior Backend (.NET 9) Developer pracujący nad Orbito Platform.**

**CEL BIZNESOWY:**
Dodawanie nowego klienta jest wyłącznym prawem Providera. Utworzenie klienta uruchamia automatyczny flow: klient dodawany jest ze statusem `Inactive`, system generuje bezpieczny token i wysyła email z linkiem dostępowym do Client Portal.

**KROKI DO WYKONANIA (BACKEND - CQRS, MediatR):**

* **Domain/ValueObjects:**
    * Utwórz `ClientInvitationToken` jako ValueObject:
        ```csharp
        public class ClientInvitationToken : ValueObject
        {
            public string Token { get; private set; }
            public DateTime ExpiresAt { get; private set; }
            public bool IsExpired => DateTime.UtcNow > ExpiresAt;
            
            public static ClientInvitationToken Create(TimeSpan validFor)
            {
                return new ClientInvitationToken
                {
                    Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                        .Replace("+", "-").Replace("/", "_").TrimEnd('='),
                    ExpiresAt = DateTime.UtcNow.Add(validFor)
                };
            }
            
            protected override IEnumerable<object> GetEqualityComponents()
            {
                yield return Token;
            }
        }
        ```
    * Dodaj do encji `Client` nowe właściwości:
        * `InvitationToken` (string, nullable)
        * `InvitationTokenExpiresAt` (DateTime?, nullable)
        * `Status` (enum: `Inactive`, `Active`, `Suspended`) – jeśli jeszcze nie istnieje.
        * `ConfirmedAt` (DateTime?, nullable)

* **Domain/Enums:**
    * Jeśli nie istnieje, utwórz `ClientStatus` enum: `Inactive = 0, Active = 1, Suspended = 2`.

* **Application/Interfaces:**
    * Utwórz `IEmailService`:
        ```csharp
        public interface IEmailService
        {
            Task<Result> SendClientInvitationAsync(
                string toEmail, 
                string clientName, 
                string providerName, 
                string invitationLink, 
                CancellationToken cancellationToken = default);
        }
        ```

* **Application/Features/Clients/Commands:**
    * Utwórz `InviteClientCommand`:
        ```csharp
        public record InviteClientCommand : IRequest<Result<Guid>>
        {
            public string Email { get; init; }
            public string FirstName { get; init; }
            public string LastName { get; init; }
            public string? CompanyName { get; init; }
        }
        ```
    * Utwórz `InviteClientCommandHandler`:
        1. Pobierz `TenantId` z `ITenantContext`.
        2. Sprawdź czy klient z tym emailem już istnieje w tym tenancie → `DomainErrors.Client.AlreadyExists`.
        3. Utwórz encję `Client` ze statusem `Inactive`.
        4. Wygeneruj `ClientInvitationToken.Create(TimeSpan.FromDays(7))`.
        5. Zapisz token i `ExpiresAt` w encji Client.
        6. Utwórz link zaproszenia: `{baseUrl}/portal/confirm?token={token}`.
        7. Wywołaj `IEmailService.SendClientInvitationAsync(...)`.
        8. Zapisz zmiany przez `_unitOfWork.SaveChangesAsync()`.
        9. Zwróć `Result.Success(client.Id)`.
    * Utwórz `InviteClientCommandValidator` z FluentValidation:
        * `Email`: NotEmpty, valid email format.
        * `FirstName`: NotEmpty, MaxLength(100).
        * `LastName`: NotEmpty, MaxLength(100).

* **Infrastructure/Services:**
    * Utwórz `EmailService : IEmailService` w `Orbito.Infrastructure/Services/`:
        * Na etapie MVP: Użyj SMTP z `MailKit` lub prostego `SmtpClient`.
        * Alternatywa: SendGrid SDK (do konfiguracji w `appsettings.json`).
        * Template HTML z linkiem: `<a href="{invitationLink}">Aktywuj konto w {providerName}</a>`.
        * Loguj każdą próbę wysłania emaila przez Serilog: `_logger.LogInformation("Sending invitation email to {Email} for tenant {TenantId}", ...)`.
    * Zarejestruj `IEmailService` w `DependencyInjection.cs`.

* **Infrastructure/Data:**
    * Migracja EF Core: Dodaj kolumny do tabeli `Clients`:
        * `InvitationToken` (nvarchar(200), nullable)
        * `InvitationTokenExpiresAt` (datetime2, nullable)
        * `Status` (int, default: 0 = Inactive)
        * `ConfirmedAt` (datetime2, nullable)
    * W konfiguracji EF Core (`ClientConfiguration.cs`): Mapuj nowe właściwości.

**✅ CHECKLIST WERYFIKACJI (Dla Agenta):**

* [ ] `ClientInvitationToken` ValueObject istnieje w `Orbito.Domain/ValueObjects/`
* [ ] `IEmailService` zdefiniowany w `Orbito.Application/Common/Interfaces/`
* [ ] `EmailService` zaimplementowany w `Orbito.Infrastructure/Services/`
* [ ] `InviteClientCommand` + Handler + Validator istnieją w `Application/Features/Clients/Commands/`
* [ ] Handler zwraca `Result<Guid>` (nie rzuca wyjątków!)
* [ ] Migracja EF Core: `dotnet ef migrations add AddClientInvitationFields` → sukces
* [ ] `dotnet build` → ZERO błędów
* [ ] Unit test: InviteClientCommandHandler tworzy klienta Inactive, generuje token, wywołuje EmailService
* [ ] Git commit: `feat(clients): add invitation token, email service, and InviteClientCommand`

<!-- BLOCK_END: ISSUE_3.1 -->

---

<!-- BLOCK_START: ISSUE_3.2 -->
#### 🛠️ ISSUE 3.2: Zapraszanie Klienta – Backend: Potwierdzenie Emaila & Aktywacja

| # | Zadanie | Priorytet | Status | Opis |
|---|---|---|---|---|
| 1 | 🔴 ConfirmClientEmailCommand & Handler | Krytyczne | ⬜ | Application: Endpoint weryfikujący token i aktywujący klienta |
| 2 | 🔴 Client status Active | Krytyczne | ⬜ | Domain: Zmiana statusu Client z Inactive → Active po potwierdzeniu |
| 3 | 🔴 Auto-create User account for Client | Krytyczne | ⬜ | Application: Tworzenie konta użytkownika Identity z rolą Client |
| 4 | 🟡 Token expiration guard | Ważne | ⬜ | Application: Obsługa wygasłych tokenów z czytelnym komunikatem |
| 5 | 🟡 Resend invitation | Ważne | ⬜ | Application: Endpoint do ponownego wysłania zaproszenia (nowy token) |

**Blok 3.2 - Wymagania wejściowe**: ISSUE 3.1 (token, email service)
**Blok 3.2 - Rezultat**: Endpoint `POST /api/Clients/confirm-email` weryfikuje token, tworzy konto użytkownika z rolą Client, zmienia status klienta na Active. Wygasłe tokeny są odrzucane z czytelnym komunikatem.

---

##### 🤖 PROMPT

**Działaj jako Senior Backend (.NET 9) Developer pracujący nad Orbito Platform.**

**CEL BIZNESOWY:**
Po kliknięciu w link z emaila, klient trafia na endpoint potwierdzający. System weryfikuje token, tworzy konto użytkownika (Identity) z rolą `Client` i zmienia status klienta na `Active`. Od tego momentu klient może logować się do Client Portal.

**KROKI DO WYKONANIA (BACKEND - CQRS, MediatR):**

* **Domain/Entities:**
    * Dodaj do encji `Client` metodę domenową:
        ```csharp
        public Result ConfirmEmail()
        {
            if (Status == ClientStatus.Active)
                return Result.Failure(DomainErrors.Client.AlreadyConfirmed);
            
            if (InvitationToken == null || InvitationTokenExpiresAt < DateTime.UtcNow)
                return Result.Failure(DomainErrors.Client.InvitationExpired);
            
            Status = ClientStatus.Active;
            ConfirmedAt = DateTime.UtcNow;
            InvitationToken = null; // Zużyj token (jednorazowy)
            InvitationTokenExpiresAt = null;
            
            return Result.Success();
        }
        ```

* **Application/Features/Clients/Commands:**
    * Utwórz `ConfirmClientEmailCommand`:
        ```csharp
        public record ConfirmClientEmailCommand : IRequest<Result>
        {
            public string Token { get; init; }
            public string Password { get; init; } // Klient ustala hasło przy potwierdzeniu
        }
        ```
    * Utwórz `ConfirmClientEmailCommandHandler`:
        1. Pobierz klienta po tokenie: Dodaj do `IClientRepository` metodę `GetByInvitationTokenAsync(string token, CancellationToken ct)`.
        2. Jeśli klient nie znaleziony → `DomainErrors.Client.InvalidToken`.
        3. Wywołaj `client.ConfirmEmail()` → jeśli `Result.IsFailure`, zwróć błąd.
        4. Utwórz konto Identity (ASP.NET Core Identity `UserManager`):
            * `Email` = client.Email
            * `UserName` = client.Email
            * `Password` = z komendy
            * Przypisz rolę `Client`.
        5. Powiąż `UserId` z encją Client.
        6. Zapisz zmiany.
        7. Zwróć `Result.Success()`.
    * Utwórz `ConfirmClientEmailCommandValidator`:
        * `Token`: NotEmpty.
        * `Password`: NotEmpty, MinLength(8), regex (duża litera, cyfra, znak specjalny).

    * (Opcjonalnie) Utwórz `ResendClientInvitationCommand`:
        1. Pobierz klienta po ID (z kontekstu tenanta Providera).
        2. Wygeneruj nowy token.
        3. Wyślij ponownie email.
        4. Zwróć `Result.Success()`.

* **Application/Errors:**
    * Dodaj do `DomainErrors`:
        ```csharp
        public static class Client
        {
            public static readonly Error AlreadyConfirmed = new("Client.AlreadyConfirmed", "This client account is already confirmed.");
            public static readonly Error InvitationExpired = new("Client.InvitationExpired", "The invitation link has expired. Please ask your provider to send a new one.");
            public static readonly Error InvalidToken = new("Client.InvalidToken", "Invalid or expired invitation token.");
            public static readonly Error AlreadyExists = new("Client.AlreadyExists", "A client with this email already exists.");
        }
        ```

* **Infrastructure/Repositories:**
    * Dodaj do `ClientRepository`:
        ```csharp
        public async Task<Client?> GetByInvitationTokenAsync(string token, CancellationToken cancellationToken)
        {
            return await _context.Clients
                .FirstOrDefaultAsync(c => c.InvitationToken == token, cancellationToken);
        }
        ```
    * ⚠️ Ta metoda NIE filtruje po tenancie – token jest globalnie unikalny. To jest świadoma decyzja (endpoint jest publiczny, token służy jako "klucz").

* **API/Controllers:**
    * Dodaj do `ClientsController` (lub utwórz `ClientConfirmationController`):
        ```csharp
        [AllowAnonymous] // Endpoint publiczny – klient nie jest jeszcze zalogowany
        [HttpPost("confirm-email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmClientEmailCommand command)
        {
            var result = await Mediator.Send(command);
            return result.IsSuccess ? Ok() : HandleFailure(result);
        }
        ```
    * (Opcjonalnie) Endpoint `POST /api/Clients/{id}/resend-invitation`:
        ```csharp
        [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
        [HttpPost("{id}/resend-invitation")]
        public async Task<IActionResult> ResendInvitation(Guid id) { ... }
        ```

**✅ CHECKLIST WERYFIKACJI (Dla Agenta):**

* [ ] `POST /api/Clients/confirm-email` z prawidłowym tokenem → 200 OK
* [ ] Po potwierdzeniu: `Client.Status` = `Active`, `Client.ConfirmedAt` != null, `Client.InvitationToken` = null (sprawdź SQL)
* [ ] W `AspNetUsers` istnieje nowy user z rolą `Client` powiązany z Client entity
* [ ] Token wygasły → 400 Bad Request z `Client.InvitationExpired`
* [ ] Token nieprawidłowy → 400 Bad Request z `Client.InvalidToken`
* [ ] Ponowne użycie zużytego tokena → 400 Bad Request z `Client.AlreadyConfirmed`
* [ ] Klient może się zalogować po potwierdzeniu (sprawdź `POST /api/Account/login`)
* [ ] `dotnet build` → ZERO błędów
* [ ] Git commit: `feat(clients): add email confirmation endpoint with Identity account creation`

<!-- BLOCK_END: ISSUE_3.2 -->

---

<!-- BLOCK_START: ISSUE_3.3 -->
#### 🛠️ ISSUE 3.3: Zapraszanie Klienta – Frontend: Formularz Invite Client

| # | Zadanie | Priorytet | Status | Opis |
|---|---|---|---|---|
| 1 | 🔴 Orval regeneration | Krytyczne | ⬜ | Frontend: `npm run api:generate` po zmianach backendowych |
| 2 | 🔴 InviteClientForm component | Krytyczne | ⬜ | Frontend: Formularz z React Hook Form + Zod |
| 3 | 🔴 useInviteClient hook | Krytyczne | ⬜ | Frontend: Custom hook opakowujący mutation Orval |
| 4 | 🟡 Client status badge | Ważne | ⬜ | Frontend: Badge Inactive/Active w liście klientów |
| 5 | 🟡 Resend invitation button | Ważne | ⬜ | Frontend: Przycisk "Wyślij ponownie" dla klientów Inactive |

**Blok 3.3 - Wymagania wejściowe**: ISSUE 3.1, ISSUE 3.2 (backend endpoints gotowe)
**Blok 3.3 - Rezultat**: Provider może zaprosić klienta z poziomu dashboardu. Formularz wysyła dane do `POST /api/Clients/invite`, wyświetla toast z potwierdzeniem wysłania zaproszenia. Lista klientów pokazuje status Inactive/Active.

---

##### 🤖 PROMPT

**Działaj jako Senior Frontend (Next.js 15) Developer pracujący nad Orbito Platform.**

**CEL BIZNESOWY:**
Provider w swoim dashboardzie (`/dashboard/clients`) ma mieć możliwość zaproszenia nowego klienta przez formularz. Formularz wysyła zaproszenie, system tworzy klienta jako `Inactive` i wysyła email. Lista klientów musi pokazywać status (Inactive/Active).

**KROKI DO WYKONANIA (FRONTEND - Next.js 15):**

* **API & Hooks:**
    1. Uruchom `npm run api:generate` – nowe endpointy (`POST /api/Clients/invite`, `POST /api/Clients/{id}/resend-invitation`) powinny wygenerować hooki.
    2. Zweryfikuj, że wygenerowane hooki istnieją: `usePostApiClientsInvite`, `usePostApiClientsIdResendInvitation`.

* **Components:**
    1. Utwórz `src/features/clients/schemas/invite-client.schema.ts`:
        ```typescript
        import { z } from "zod";
        
        export const InviteClientSchema = z.object({
          email: z.string().email("Podaj prawidłowy email"),
          firstName: z.string().min(1, "Imię jest wymagane").max(100),
          lastName: z.string().min(1, "Nazwisko jest wymagane").max(100),
          companyName: z.string().max(200).optional(),
        });
        
        export type InviteClientFormData = z.infer<typeof InviteClientSchema>;
        ```

    2. Utwórz `src/features/clients/components/InviteClientForm.tsx`:
        * Formularz z polami: Email, FirstName, LastName, CompanyName (optional).
        * React Hook Form + Zod resolver.
        * Submit wywołuje `usePostApiClientsInvite` mutation.
        * Toast sukcesu: "Zaproszenie wysłane do {email}".
        * Toast błędu: error.message z backendu.
        * Button disabled podczas wysyłania (isLoading).

    3. Utwórz `src/features/clients/components/InviteClientDialog.tsx`:
        * Dialog (shadcn/ui `<Dialog>`) z `InviteClientForm` wewnątrz.
        * Trigger: Button "Zaproś klienta" na stronie `/dashboard/clients`.

    4. Zmodyfikuj `src/features/clients/components/ClientsTable.tsx` (istniejący):
        * Dodaj kolumnę "Status" z Badge:
            * `Inactive` → Badge variant `secondary` (szary)
            * `Active` → Badge variant `default` (zielony)
        * Dodaj akcję "Wyślij ponownie zaproszenie" w dropdown menu (tylko dla Inactive).

    5. Utwórz `src/features/clients/hooks/useInviteClient.ts`:
        ```typescript
        import { usePostApiClientsInvite } from "@/core/api/generated/clients/clients";
        import { useQueryClient } from "@tanstack/react-query";
        import { toast } from "sonner";
        
        export function useInviteClient() {
          const queryClient = useQueryClient();
          
          return usePostApiClientsInvite({
            mutation: {
              onSuccess: () => {
                toast.success("Zaproszenie zostało wysłane!");
                queryClient.invalidateQueries({ queryKey: ["/api/Clients"] });
              },
              onError: (error) => {
                toast.error(error.message || "Nie udało się wysłać zaproszenia");
              },
            },
          });
        }
        ```

* **Routing/State:**
    * Strona `/dashboard/clients` – dodaj `InviteClientDialog` obok istniejącego przycisku "Create Client" (lub zamień go, bo teraz klientów tworzymy przez zaproszenie).

**✅ CHECKLIST WERYFIKACJI (Dla Agenta):**

* [ ] `npm run api:generate` → wygenerowane hooki dla invite i resend-invitation
* [ ] Formularz InviteClientForm renderuje się w dialogu
* [ ] Submit → Network Tab: `POST /api/Clients/invite` z Authorization header → 200/201
* [ ] Po wysłaniu: Toast "Zaproszenie wysłane", lista klientów odświeża się
* [ ] Nowy klient w liście ma Badge "Inactive" (szary)
* [ ] Przycisk "Wyślij ponownie" dla Inactive → `POST /api/Clients/{id}/resend-invitation` → 200
* [ ] Walidacja Zod: pusty email → komunikat błędu
* [ ] `npm run typecheck` → ZERO błędów
* [ ] Git commit: `feat(clients): add invite client form with status badges`

<!-- BLOCK_END: ISSUE_3.3 -->

---

<!-- BLOCK_START: ISSUE_3.4 -->
#### 🛠️ ISSUE 3.4: Zapraszanie Klienta – Frontend: Strona Potwierdzenia Tokena

| # | Zadanie | Priorytet | Status | Opis |
|---|---|---|---|---|
| 1 | 🔴 Strona /portal/confirm | Krytyczne | ⬜ | Frontend: Publiczna strona z formularzem ustawiania hasła |
| 2 | 🔴 Token validation UI | Krytyczne | ⬜ | Frontend: Walidacja tokena z URL i komunikaty błędów |
| 3 | 🔴 Password form + submit | Krytyczne | ⬜ | Frontend: Formularz hasła → POST /api/Clients/confirm-email |
| 4 | 🟡 Success state | Ważne | ⬜ | Frontend: Ekran sukcesu z przyciskiem "Zaloguj się" |
| 5 | 🟡 Error states | Ważne | ⬜ | Frontend: Obsługa wygasłego/nieprawidłowego tokena |

**Blok 3.4 - Wymagania wejściowe**: ISSUE 3.2 (backend confirm endpoint), ISSUE 3.3 (frontend invite flow)
**Blok 3.4 - Rezultat**: Klient klikając link z emaila trafia na stronę `/portal/confirm?token=...`, ustawia swoje hasło, a po potwierdzeniu jego konto staje się Active i może się zalogować.

---

##### 🤖 PROMPT

**Działaj jako Senior Frontend (Next.js 15) Developer pracujący nad Orbito Platform.**

**CEL BIZNESOWY:**
Klient, klikając w link z emaila zaproszenia (`/portal/confirm?token=abc123`), trafia na publiczną stronę gdzie ustawia swoje hasło. Po poprawnej weryfikacji tokena, konto klienta staje się Active i może się zalogować do Client Portal.

**KROKI DO WYKONANIA (FRONTEND - Next.js 15):**

* **API & Hooks:**
    1. Upewnij się, że `npm run api:generate` wygenerowało hook do `POST /api/Clients/confirm-email`.
    2. Zweryfikuj hook: `usePostApiClientsConfirmEmail` (lub podobna nazwa).

* **Components:**
    1. Utwórz `src/features/clients/schemas/confirm-email.schema.ts`:
        ```typescript
        import { z } from "zod";
        
        export const ConfirmEmailSchema = z.object({
          token: z.string().min(1),
          password: z.string()
            .min(8, "Hasło musi mieć minimum 8 znaków")
            .regex(/[A-Z]/, "Hasło musi zawierać wielką literę")
            .regex(/[0-9]/, "Hasło musi zawierać cyfrę")
            .regex(/[^a-zA-Z0-9]/, "Hasło musi zawierać znak specjalny"),
          confirmPassword: z.string(),
        }).refine((data) => data.password === data.confirmPassword, {
          message: "Hasła muszą być identyczne",
          path: ["confirmPassword"],
        });
        ```

    2. Utwórz `src/features/clients/components/ConfirmEmailForm.tsx`:
        * Props: `token: string`.
        * Pola: Password, Confirm Password.
        * React Hook Form + Zod resolver.
        * Submit wywołuje `usePostApiClientsConfirmEmail({ data: { token, password } })`.
        * Stany:
            * `idle` – formularz gotowy do wypełnienia.
            * `submitting` – loading spinner na przycisku.
            * `success` – "Konto aktywowane! Możesz się teraz zalogować." + Button → `/login`.
            * `error` – komunikat z backendu (wygasły token, nieprawidłowy token itd.).

    3. Utwórz stronę `src/app/(auth)/portal/confirm/page.tsx`:
        * ⚠️ **WAŻNE**: Ta strona jest w route group `(auth)` bo jest publiczna (klient nie jest jeszcze zalogowany).
        * Odczytaj `token` z `searchParams` (Next.js 15 – async params):
            ```typescript
            export default async function ConfirmEmailPage({
              searchParams,
            }: {
              searchParams: Promise<{ token?: string }>;
            }) {
              const { token } = await searchParams;
              
              if (!token) {
                return <ErrorMessage message="Brak tokena zaproszenia w linku." />;
              }
              
              return (
                <div className="mx-auto max-w-md space-y-6">
                  <h1 className="text-2xl font-bold">Aktywacja konta</h1>
                  <p className="text-muted-foreground">
                    Ustaw hasło, aby aktywować swoje konto w portalu klienta.
                  </p>
                  <ConfirmEmailForm token={token} />
                </div>
              );
            }
            ```

* **Routing/State:**
    * Strona `/portal/confirm` MUSI być publiczna (nie chroniona przez `PortalGuard` ani `TenantGuard`).
    * Po sukcesie: Przycisk "Zaloguj się" przekierowuje na `/login`.
    * Po zalogowaniu z rolą Client: Middleware przekierowuje na `/portal`.

**✅ CHECKLIST WERYFIKACJI (Dla Agenta):**

* [ ] Strona `/portal/confirm?token=validToken` wyświetla formularz hasła
* [ ] Strona `/portal/confirm` (bez tokena) → komunikat błędu "Brak tokena"
* [ ] Submit z prawidłowym tokenem → Network Tab: `POST /api/Clients/confirm-email` → 200 OK
* [ ] Po sukcesie: ekran "Konto aktywowane" z przyciskiem "Zaloguj się"
* [ ] Submit z wygasłym tokenem → komunikat "Link wygasł"
* [ ] Submit z nieprawidłowym tokenem → komunikat "Nieprawidłowy token"
* [ ] Walidacja hasła: <8 znaków, brak dużej litery, brak cyfry → komunikaty błędów
* [ ] Klient może się zalogować po potwierdzeniu → ląduje na `/portal`
* [ ] `npm run typecheck` → ZERO błędów
* [ ] Git commit: `feat(clients): add email confirmation page with password setup`

<!-- BLOCK_END: ISSUE_3.4 -->

---

<!-- BLOCK_START: ISSUE_4.1 -->
#### 🛠️ ISSUE 4.1: Płatności – Stripe Elements w Client Portal

| # | Zadanie | Priorytet | Status | Opis |
|---|---|---|---|---|
| 1 | 🔴 Stripe Elements integration | Krytyczne | ⬜ | Frontend: Osadzenie Stripe Payment Element w portalu klienta (PCI DSS) |
| 2 | 🔴 CreatePaymentIntent endpoint | Krytyczne | ⬜ | Backend: Endpoint tworzący Stripe PaymentIntent z client_secret |
| 3 | 🔴 PaymentForm component | Krytyczne | ⬜ | Frontend: Formularz płatności z Stripe Elements |
| 4 | 🔴 Payment success/failure UI | Krytyczne | ⬜ | Frontend: Obsługa stanów po płatności |
| 5 | 🟡 Subscription-Payment linking | Ważne | ⬜ | Backend: Powiązanie PaymentIntent z konkretną subskrypcją klienta |

**Blok 4.1 - Wymagania wejściowe**: ISSUE 3.4 (klient może się logować do portalu)
**Blok 4.1 - Rezultat**: Klient w Client Portal widzi swoje subskrypcje i może opłacić je bezpośrednio przez bezpieczną bramkę Stripe (Stripe Payment Element). Dane karty NIGDY nie przechodzą przez serwer Orbito (PCI DSS compliance).

---

##### 🤖 PROMPT

**Działaj jako Senior Backend (.NET 9) & Frontend (Next.js 15) Developer pracujący nad Orbito Platform.**

**CEL BIZNESOWY:**
W Client Portal (`/portal`), klient widzi przypisane mu subskrypcje i musi mieć możliwość opłacenia ich bezpośrednio przez bramkę Stripe. Zgodność z PCI DSS jest krytyczna – dane kart nie mogą NIGDY przechodzić przez serwery Orbito.

⚠️ **KRYTYCZNE: PCI DSS Compliance**
* Numer karty, CVV, data ważności – te dane NIGDY nie mogą trafić do backendu Orbito.
* Używamy `Stripe Payment Element` (Stripe.js + React Stripe.js) – formularz karty renderowany przez Stripe.
* Backend jedynie tworzy `PaymentIntent` i zwraca `clientSecret` do frontendu.

**KROKI DO WYKONANIA (BACKEND - CQRS, MediatR):**

* **Application/Features/Payments/Commands:**
    * Utwórz `CreatePaymentIntentCommand`:
        ```csharp
        public record CreatePaymentIntentCommand : IRequest<Result<CreatePaymentIntentResponse>>
        {
            public Guid SubscriptionId { get; init; }
        }
        
        public record CreatePaymentIntentResponse
        {
            public string ClientSecret { get; init; }
            public string PaymentIntentId { get; init; }
            public decimal Amount { get; init; }
            public string Currency { get; init; }
        }
        ```
    * Utwórz `CreatePaymentIntentCommandHandler`:
        1. Pobierz subskrypcję po ID (z kontekstu klienta – `GetByIdForClientAsync`).
        2. Zweryfikuj, że subskrypcja należy do zalogowanego klienta.
        3. Pobierz Stripe CustomerId klienta (lub utwórz przez `CreateStripeCustomerCommand` jeśli nie istnieje).
        4. Wywołaj Stripe API:
            ```csharp
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(subscription.CurrentPrice.Amount * 100), // Stripe operuje w groszach
                Currency = subscription.CurrentPrice.Currency.ToLower(),
                Customer = client.StripeCustomerId,
                Metadata = new Dictionary<string, string>
                {
                    { "subscription_id", subscription.Id.ToString() },
                    { "tenant_id", tenantId.ToString() },
                    { "client_id", client.Id.ToString() }
                }
            };
            var paymentIntent = await _stripeClient.PaymentIntents.CreateAsync(options);
            ```
        5. Utwórz rekord `Payment` w bazie ze statusem `Processing` i `ExternalPaymentId` = `paymentIntent.Id`.
        6. Zwróć `Result.Success(new CreatePaymentIntentResponse { ClientSecret = paymentIntent.ClientSecret, ... })`.

* **API/Controllers:**
    * Dodaj do `PortalController` (lub nowego `PortalPaymentController`):
        ```csharp
        [Authorize(Policy = PolicyNames.ClientAccess)]
        [HttpPost("payments/create-intent")]
        [ProducesResponseType(typeof(CreatePaymentIntentResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] CreatePaymentIntentCommand command)
        {
            var result = await Mediator.Send(command);
            return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
        }
        ```

**KROKI DO WYKONANIA (FRONTEND - Next.js 15):**

* **API & Hooks:**
    1. Zainstaluj Stripe: `npm install @stripe/react-stripe-js @stripe/stripe-js`.
    2. Uruchom `npm run api:generate` – hook do `POST /api/Portal/payments/create-intent`.

* **Components:**
    1. Utwórz `src/core/providers/StripeProvider.tsx`:
        ```typescript
        "use client";
        import { Elements } from "@stripe/react-stripe-js";
        import { loadStripe } from "@stripe/stripe-js";
        
        const stripePromise = loadStripe(process.env.NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY!);
        
        export function StripeProvider({ clientSecret, children }: { clientSecret: string; children: React.ReactNode }) {
          return (
            <Elements stripe={stripePromise} options={{ clientSecret }}>
              {children}
            </Elements>
          );
        }
        ```

    2. Utwórz `src/features/client-portal/components/PaymentForm.tsx`:
        ```typescript
        "use client";
        import { PaymentElement, useStripe, useElements } from "@stripe/react-stripe-js";
        // Formularz z Stripe Payment Element
        // Submit → stripe.confirmPayment({ elements, confirmParams: { return_url } })
        // PO POTWIERDZENIU: Stripe przekierowuje na return_url z payment_intent query param
        ```
        * ⚠️ **NIE TWÓRZ WŁASNYCH INPUTÓW NA NUMER KARTY!** Używaj wyłącznie `<PaymentElement />` ze Stripe.
        * Obsługa stanów: loading (Skeleton), submitting (disable button), error (Stripe error message), success (redirect).

    3. Utwórz `src/features/client-portal/components/PaySubscriptionDialog.tsx`:
        * Dialog otwierany przyciskiem "Opłać" przy subskrypcji.
        * Flow:
            1. Otwórz dialog → wywołaj `POST /api/Portal/payments/create-intent` z subscriptionId.
            2. Po otrzymaniu `clientSecret` → renderuj `<StripeProvider>` z `<PaymentForm>`.
            3. Klient wypełnia dane karty (Stripe Element) → Submit.
            4. Stripe przetwarza płatność → redirect na `/portal?payment=success`.

    4. Zmodyfikuj `src/features/client-portal/components/MySubscriptionsCard.tsx`:
        * Dodaj przycisk "Opłać" przy każdej subskrypcji z `PaymentStatus != "Paid"`.
        * Przycisk otwiera `PaySubscriptionDialog`.

    5. Utwórz `src/app/(portal)/portal/payment-success/page.tsx`:
        * Strona z komunikatem "Płatność przetwarzana. Otrzymasz potwierdzenie emailem."
        * Link "Wróć do portalu" → `/portal`.

* **Environment Variables:**
    * Dodaj `NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY` do `.env.local` (klucz publiczny – **NIE** secret key!).

**✅ CHECKLIST WERYFIKACJI (Dla Agenta):**

* [ ] `POST /api/Portal/payments/create-intent` → 200 OK z `clientSecret` (Network Tab)
* [ ] Stripe Payment Element renderuje się poprawnie w dialogu (widoczne pola karty)
* [ ] ⚠️ **PCI DSS**: W Network Tab NIE MA żadnych requestów z numerem karty do backendu Orbito – dane karty idą TYLKO do `api.stripe.com`
* [ ] Po opłaceniu: Stripe przekierowuje na `/portal?payment=success` (lub `/portal/payment-success`)
* [ ] Rekord `Payment` w bazie ma status `Processing` i `ExternalPaymentId` = `pi_...`
* [ ] `npm run typecheck` → ZERO błędów
* [ ] `dotnet build` → ZERO błędów
* [ ] Git commit: `feat(portal): add Stripe Payment Element for subscription payments`

<!-- BLOCK_END: ISSUE_4.1 -->

---

<!-- BLOCK_START: ISSUE_4.2 -->
#### 🛠️ ISSUE 4.2: Płatności – Webhook Sync do Provider Dashboard

| # | Zadanie | Priorytet | Status | Opis |
|---|---|---|---|---|
| 1 | 🔴 Stripe Webhook Handler update | Krytyczne | ⬜ | Backend: Obsługa `payment_intent.succeeded` i `payment_intent.payment_failed` |
| 2 | 🔴 Payment status sync | Krytyczne | ⬜ | Backend: Automatyczna aktualizacja statusu Payment w bazie |
| 3 | 🔴 MRR recalculation trigger | Krytyczne | ⬜ | Backend: Po potwierdzeniu płatności → przelicz MRR w Analytics |
| 4 | 🟡 Real-time notification (optional) | Ważne | ⬜ | Frontend: Provider widzi natychmiast nową płatność w /dashboard/payments |
| 5 | 🟡 Payment confirmation email | Ważne | ⬜ | Backend: Email do klienta po udanej płatności |

**Blok 4.2 - Wymagania wejściowe**: ISSUE 4.1 (Stripe Payment Intent flow)
**Blok 4.2 - Rezultat**: Opłacenie subskrypcji przez klienta (potwierdzone przez Stripe Webhook) natychmiast i automatycznie odzwierciedla się w panelu Providera – w widoku `/dashboard/payments` oraz statystykach MRR w `/dashboard/analytics`.

---

##### 🤖 PROMPT

**Działaj jako Senior Backend (.NET 9) & Frontend (Next.js 15) Developer pracujący nad Orbito Platform.**

**CEL BIZNESOWY:**
Kiedy klient opłaci subskrypcję przez Stripe, potwierdzenie płatności (Stripe Webhook `payment_intent.succeeded`) musi natychmiast i automatycznie zaktualizować status płatności w bazie danych. Ta zmiana musi być widoczna w panelu Providera: w widoku `/dashboard/payments` (nowa opłacona płatność) oraz w statystykach MRR na `/dashboard/analytics`.

**KROKI DO WYKONANIA (BACKEND - CQRS, MediatR):**

* **Application/Features/Payments/Commands:**
    * Zmodyfikuj istniejący `UpdatePaymentFromWebhookCommandHandler` (lub `ProcessWebhookEventCommandHandler`):
        1. Obsłuż event `payment_intent.succeeded`:
            * Pobierz Payment po `ExternalPaymentId` (= PaymentIntent ID) używając `GetByExternalPaymentIdUnsafeAsync` (metoda Unsafe – webhook jest zweryfikowany sygnaturą).
            * Zmień status Payment na `Completed`.
            * Zaktualizuj `PaidAt = DateTime.UtcNow`.
            * Wyciągnij `subscription_id` z `metadata` PaymentIntent.
            * Zaktualizuj status Subscription jeśli potrzebne (np. `Active` jeśli był `PendingPayment`).
        2. Obsłuż event `payment_intent.payment_failed`:
            * Zmień status Payment na `Failed`.
            * Zapisz `FailureReason` z Stripe.
        3. **Po udanej płatności**: Wyślij email potwierdzający do klienta (jeśli `IEmailService` jest gotowy).

    * Zweryfikuj, że `UpdatePaymentFromWebhookCommandHandler` poprawnie:
        * Waliduje sygnaturę Stripe Webhook (`Stripe-Signature` header).
        * Zwraca `Result<T>` (nie rzuca wyjątków).
        * Loguje event przez Serilog.

* **Application/Features/Analytics:**
    * Zweryfikuj, że `GetDashboardStatsQueryHandler` liczy MRR na podstawie `Payment.Status == Completed`. Jeśli liczy na podstawie aktywnych subskrypcji → zweryfikuj, że po opłaceniu subskrypcji jej status poprawnie się zmienia.
    * MRR powinien odświeżać się automatycznie przy następnym zapytaniu z frontendu (React Query refetch).

* **API/Controllers:**
    * Zweryfikuj `WebhookController`:
        ```csharp
        [AllowAnonymous] // Webhook Stripe nie wysyła JWT
        [HttpPost("stripe")]
        public async Task<IActionResult> HandleStripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var stripeSignature = Request.Headers["Stripe-Signature"];
            
            // Weryfikacja sygnatury!
            var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, _webhookSecret);
            
            // Dispatch do MediatR
            var result = await Mediator.Send(new ProcessWebhookEventCommand { Event = stripeEvent });
            return result.IsSuccess ? Ok() : HandleFailure(result);
        }
        ```
    * ⚠️ **KRYTYCZNE**: Endpoint webhook MUSI:
        * Być `[AllowAnonymous]`.
        * Weryfikować `Stripe-Signature` header.
        * Być idempotentny (obsługa duplikatów eventów).

**KROKI DO WYKONANIA (FRONTEND - Next.js 15):**

* **API & Hooks:** Żadne zmiany w hookach nie są konieczne. Istniejące hooki (`useGetApiPayment`, `useGetApiAnalyticsDashboard`) pobierają aktualne dane przy każdym renderze / refetch.

* **Components:**
    * **`/dashboard/payments`**: Zweryfikuj, że `PaymentsTable` poprawnie wyświetla nowo opłacone płatności ze statusem `Completed`. React Query `staleTime` powinien być odpowiednio krótki (np. 30s), aby nowe płatności pojawiały się szybko.
    * **`/dashboard/analytics`**: Zweryfikuj, że `StatCards` (MRR) odświeżają się. Jeśli MRR się nie zmienia po płatności, sprawdź logikę `GetDashboardStatsQueryHandler` w backendzie.

* **(Opcjonalnie) Real-time updates:**
    * Jeśli chcesz natychmiastowe odświeżanie (bez czekania na refetch): Rozważ `queryClient.invalidateQueries` wywoływany przez polling lub WebSocket. Na etapie MVP wystarczy React Query refetch z `staleTime: 30000` (30s).

**✅ CHECKLIST WERYFIKACJI (Dla Agenta):**

* [ ] Stripe Webhook `payment_intent.succeeded` → `Payment.Status` = `Completed` w bazie (sprawdź SQL)
* [ ] Stripe Webhook `payment_intent.payment_failed` → `Payment.Status` = `Failed` w bazie
* [ ] `/dashboard/payments`: Nowa opłacona płatność widoczna po odświeżeniu strony (max 30s delay)
* [ ] `/dashboard/analytics`: MRR zaktualizowany po opłaceniu subskrypcji
* [ ] Webhook endpoint waliduje `Stripe-Signature` header (nieprawidłowa sygnatura → 400)
* [ ] Webhook jest idempotentny: wysłanie tego samego eventu 2x nie tworzy duplikatów
* [ ] `dotnet build` → ZERO błędów
* [ ] Git commit: `feat(payments): webhook sync with provider dashboard and MRR update`

<!-- BLOCK_END: ISSUE_4.2 -->

---

<!-- BLOCK_START: ISSUE_5.1 -->
#### 🛠️ ISSUE 5.1: Restrykcje Client Portal – Hardening PortalGuard & API

| # | Zadanie | Priorytet | Status | Opis |
|---|---|---|---|---|
| 1 | 🔴 API-level restrictions | Krytyczne | ⬜ | Backend: Klient NIE MOŻE wywoływać endpointów Provider (Clients, Plans, Team, Analytics) |
| 2 | 🔴 PortalGuard hardening | Krytyczne | ⬜ | Frontend: PortalGuard blokuje WSZYSTKIE trasy poza /portal/* |
| 3 | 🔴 Navigation restrictions | Krytyczne | ⬜ | Frontend: Menu portalu zawiera TYLKO: Subskrypcje, Historia płatności |
| 4 | 🟡 Direct URL protection | Ważne | ⬜ | Frontend: Wpisanie /dashboard/* w URL → redirect na /portal |
| 5 | 🟡 API authorization audit | Ważne | ⬜ | Backend: Przegląd WSZYSTKICH kontrolerów pod kątem policy ClientAccess vs ProviderTeamAccess |

**Blok 5.1 - Wymagania wejściowe**: ISSUE 4.1, ISSUE 4.2 (Client Portal z płatnościami działa)
**Blok 5.1 - Rezultat**: Portal klienta jest silnie ograniczony. Klient ma dostęp TYLKO do przeglądu swoich aktywnych subskrypcji i historii płatności. Żadne inne moduły platformy nie są dostępne – ani z poziomu UI, ani z poziomu API.

---

##### 🤖 PROMPT

**Działaj jako Senior Backend (.NET 9) & Frontend (Next.js 15) Developer / Security Specialist pracujący nad Orbito Platform.**

**CEL BIZNESOWY:**
Portal klienta musi być silnie ograniczony. Klient ma dostęp TYLKO do: przeglądu swoich aktywnych subskrypcji i historii płatności. Żadne inne moduły platformy (Clients, Plans, Team, Analytics, Subscriptions CRUD) nie mogą być dla niego widoczne ani dostępne z poziomu API. To jest kluczowy wymóg bezpieczeństwa i izolacji.

**KROKI DO WYKONANIA (BACKEND - CQRS, MediatR):**

* **Security – Authorization Audit:**
    * Przejrzyj WSZYSTKIE kontrolery i upewnij się, że mają odpowiednie atrybuty `[Authorize]`:

    | Controller | Wymagana Policy | Dostęp Klienta |
    |---|---|---|
    | `ClientsController` | `ProviderTeamAccess` | ❌ BRAK |
    | `SubscriptionsController` | `ProviderTeamAccess` | ❌ BRAK |
    | `SubscriptionPlansController` | `ProviderTeamAccess` | ❌ BRAK |
    | `TeamMembersController` | `ProviderTeamAccess` | ❌ BRAK |
    | `PaymentController` | `ProviderTeamAccess` | ❌ BRAK |
    | `PaymentMetricsController` | `ProviderTeamAccess` | ❌ BRAK |
    | `AnalyticsController` | `ProviderTeamAccess` | ❌ BRAK |
    | `ProvidersController` | `ProviderTeamAccess` | ❌ BRAK |
    | `PortalController` | `ClientAccess` | ✅ TAK |
    | `WebhookController` | `AllowAnonymous` | N/A (Stripe) |
    | `AccountController` | `AllowAnonymous` / mixed | N/A (Login/Register) |

    * Jeśli JAKIKOLWIEK kontroler z listy "❌ BRAK" nie ma `[Authorize(Policy = PolicyNames.ProviderTeamAccess)]` na poziomie klasy – **NAPRAW TO NATYCHMIAST**.
    * `ProviderTeamHandler` (authorization handler) MUSI odrzucać requesty od użytkowników z rolą `Client`.

* **Application – Portal Scope:**
    * Zweryfikuj, że `PortalController` WYŁĄCZNIE udostępnia:
        * `GET /api/Portal/subscriptions` – subskrypcje klienta
        * `GET /api/Portal/invoices` – historia płatności klienta
        * `POST /api/Portal/payments/create-intent` – tworzenie PaymentIntent (z ISSUE 4.1)
    * **ŻADEN inny endpoint** nie powinien być dostępny z polityką `ClientAccess`.

* **Domain – Data Scope:**
    * Handlery portalu MUSZĄ używać `IUserContextService.GetCurrentClientIdAsync()` do pobierania danych.
    * NIGDY nie akceptuj `clientId` jako parametru od klienta – zawsze pobieraj z kontekstu sesji!
    * Upewnij się, że repozytorium filtruje wyniki po `ClientId` klienta.

**KROKI DO WYKONANIA (FRONTEND - Next.js 15):**

* **PortalGuard Hardening:**
    * Zmodyfikuj `src/features/client-portal/components/PortalGuard.tsx`:
        ```typescript
        // Klient z rolą "Client" → TYLKO /portal/*
        // Klient próbujący wejść na /dashboard/* → redirect na /portal
        // Provider próbujący wejść na /portal/* → redirect na /dashboard
        ```

* **Middleware (src/middleware.ts):**
    * Zweryfikuj, że middleware poprawnie kieruje ruch:
        * User z rolą `Client` wchodzi na `/dashboard/*` → redirect `/portal`
        * User z rolą `Provider`/`PlatformAdmin` wchodzi na `/portal/*` → redirect `/dashboard`
        * Niezalogowany user wchodzi na `/dashboard/*` lub `/portal/*` → redirect `/login`

* **Navigation:**
    * Portal layout (`src/app/(portal)/layout.tsx`) powinien mieć MINIMALNE menu:
        * "Moje subskrypcje" → `/portal`
        * "Historia płatności" → `/portal/payments` (jeśli osobna strona)
        * "Wyloguj się" → sign out
    * **ŻADNYCH** linków do: Dashboard, Clients, Plans, Team, Analytics, Subscriptions.

* **Components:**
    * Usuń lub ukryj WSZYSTKIE elementy nawigacji, które mogłyby prowadzić do modułów Providera.
    * Zweryfikuj, że `Sidebar` component NIE renderuje się w portalu klienta (jest w `(dashboard)/layout.tsx`, nie w `(portal)/layout.tsx`).

**✅ CHECKLIST WERYFIKACJI (Dla Agenta):**

* [ ] **API Test**: Zaloguj się jako Client → `GET /api/Clients` → 403 Forbidden (Network Tab)
* [ ] **API Test**: Zaloguj się jako Client → `GET /api/Plans` → 403 Forbidden
* [ ] **API Test**: Zaloguj się jako Client → `GET /api/Team` → 403 Forbidden
* [ ] **API Test**: Zaloguj się jako Client → `GET /api/Analytics/dashboard` → 403 Forbidden
* [ ] **API Test**: Zaloguj się jako Client → `GET /api/Portal/subscriptions` → 200 OK ✅
* [ ] **API Test**: Zaloguj się jako Client → `GET /api/Portal/invoices` → 200 OK ✅
* [ ] **UI Test**: Klient na `/portal` widzi TYLKO subskrypcje i płatności
* [ ] **UI Test**: Klient wpisuje `/dashboard` w URL → redirect na `/portal`
* [ ] **UI Test**: Provider wpisuje `/portal` w URL → redirect na `/dashboard`
* [ ] **UI Test**: Portal NIE ma Sidebar, NIE ma linków do Dashboard/Clients/Plans/Team/Analytics
* [ ] `npm run typecheck` → ZERO błędów
* [ ] `dotnet build` → ZERO błędów
* [ ] Git commit: `security(portal): enforce strict client portal restrictions on API and UI`

<!-- BLOCK_END: ISSUE_5.1 -->

---

<!-- BLOCK_START: ISSUE_6.1 -->
#### 🛠️ ISSUE 6.1: Trial Subscription – Domain Model, Auto-create przy rejestracji, Provider jako klient Admina

| # | Zadanie | Priorytet | Status | Opis |
|---|---|---|---|---|
| 1 | 🔴 PlatformPlan seed data | Krytyczne | ⬜ | Backend: Seed planów platformowych (np. Starter, Pro, Enterprise) z cenami, do wyboru przy rejestracji |
| 2 | 🔴 TrialSubscription domain logic | Krytyczne | ⬜ | Domain: Encja/ValueObject z TrialStartDate, TrialEndDate, IsTrialActive, DaysRemaining |
| 3 | 🔴 Register flow + plan selection | Krytyczne | ⬜ | Backend: `RegisterCommand` rozszerzony o `SelectedPlanId`, auto-tworzenie 14-dniowego triala |
| 4 | 🔴 Provider as Admin's Client | Krytyczne | ⬜ | Backend: Po rejestracji Provider automatycznie pojawia się jako Client w tenancie PlatformAdmin |
| 5 | 🟡 ProviderSubscription entity | Ważne | ⬜ | Domain: Oddzielna encja/tabela wiążąca Provider z jego planem platformowym (oddzielona od subskrypcji klientów Providera) |
| 6 | 🟡 Admin clients list verification | Ważne | ⬜ | Backend: Admin widzi zarejestrowanych Providerów jako swoich klientów z ich statusem triala |

**Blok 6.1 - Wymagania wejściowe**: ISSUE 1.1 (Admin istnieje), ISSUE 2.1 (auto-create Provider)
**Blok 6.1 - Rezultat**: Przy rejestracji Provider wybiera plan platformowy i automatycznie otrzymuje 14-dniowy trial. Provider pojawia się jako klient w panelu PlatformAdmin. W bazie istnieje powiązanie Provider → PlatformPlan → ProviderSubscription z datami triala.

---

##### 🤖 PROMPT

**Działaj jako Senior Backend (.NET 9) Developer pracujący nad Orbito Platform.**

**CEL BIZNESOWY:**
Po rejestracji Provider automatycznie otrzymuje 14-dniowe konto trial na subskrypcję, którą wybrał przy rejestracji. Provider pojawia się jako klient PlatformAdmin. Po zakończeniu triala musi zapłacić, aby kontynuować korzystanie z platformy.

⚠️ **WAŻNE ROZRÓŻNIENIE POJĘCIOWE:**
W Orbito istnieją DWA typy subskrypcji:
1. **Subskrypcje klientów Providera** – te, które Provider tworzy i sprzedaje swoim klientom (istniejąca logika w `Subscriptions` / `SubscriptionPlans`).
2. **Subskrypcja platformowa Providera** – to, za co Provider płaci Orbito (NOWA logika, ten blok). To jest relacja: Provider → PlatformAdmin.

Upewnij się, że te dwa konteksty są **oddzielone** na poziomie domeny, aby nie mieszać subskrypcji platformowych z subskrypcjami końcowych klientów.

**KROKI DO WYKONANIA (BACKEND - CQRS, MediatR):**

* **Domain/Entities:**
    * Utwórz encję `PlatformPlan`:
        ```csharp
        public class PlatformPlan : BaseEntity
        {
            public string Name { get; private set; }           // "Starter", "Pro", "Enterprise"
            public string Description { get; private set; }
            public Money Price { get; private set; }           // np. 49 PLN/mies, 149 PLN/mies
            public BillingPeriodType BillingPeriod { get; private set; }
            public int TrialDays { get; private set; }         // domyślnie 14
            public bool IsActive { get; private set; }
            public List<string> Features { get; private set; } // lista cech planu
            
            public static PlatformPlan Create(string name, Money price, int trialDays = 14) { ... }
        }
        ```
    * Utwórz encję `ProviderSubscription`:
        ```csharp
        public class ProviderSubscription : BaseEntity
        {
            public Guid ProviderId { get; private set; }
            public Guid PlatformPlanId { get; private set; }
            public ProviderSubscriptionStatus Status { get; private set; } // Trial, Active, Expired, Cancelled
            public DateTime StartDate { get; private set; }
            public DateTime TrialEndDate { get; private set; }
            public DateTime? PaidUntil { get; private set; }
            public DateTime? LastNotificationSentAt { get; private set; }
            
            // Navigation
            public Provider Provider { get; private set; }
            public PlatformPlan PlatformPlan { get; private set; }
            
            public bool IsTrialActive => Status == ProviderSubscriptionStatus.Trial 
                                         && DateTime.UtcNow <= TrialEndDate;
            public int DaysRemaining => IsTrialActive 
                ? (int)Math.Ceiling((TrialEndDate - DateTime.UtcNow).TotalDays) 
                : 0;
            
            public static ProviderSubscription CreateTrial(Guid providerId, Guid planId, int trialDays)
            {
                return new ProviderSubscription
                {
                    ProviderId = providerId,
                    PlatformPlanId = planId,
                    Status = ProviderSubscriptionStatus.Trial,
                    StartDate = DateTime.UtcNow,
                    TrialEndDate = DateTime.UtcNow.AddDays(trialDays),
                };
            }
            
            public Result Activate(DateTime paidUntil)
            {
                Status = ProviderSubscriptionStatus.Active;
                PaidUntil = paidUntil;
                return Result.Success();
            }
            
            public Result Expire()
            {
                if (Status == ProviderSubscriptionStatus.Active && PaidUntil > DateTime.UtcNow)
                    return Result.Failure(DomainErrors.ProviderSubscription.StillActive);
                Status = ProviderSubscriptionStatus.Expired;
                return Result.Success();
            }
        }
        ```
    * Utwórz enum `ProviderSubscriptionStatus`:
        ```csharp
        public enum ProviderSubscriptionStatus
        {
            Trial = 0,
            Active = 1,
            Expired = 2,
            Cancelled = 3
        }
        ```

* **Domain/Errors:**
    * Dodaj do `DomainErrors`:
        ```csharp
        public static class ProviderSubscription
        {
            public static readonly Error StillActive = new("ProviderSubscription.StillActive", "Subscription is still active and cannot be expired.");
            public static readonly Error TrialExpired = new("ProviderSubscription.TrialExpired", "Your trial period has expired. Please subscribe to continue.");
            public static readonly Error PlanNotFound = new("ProviderSubscription.PlanNotFound", "Selected platform plan does not exist.");
            public static readonly Error AlreadyExists = new("ProviderSubscription.AlreadyExists", "Provider already has an active subscription.");
        }
        ```

* **Application/Features/Account/Commands:**
    * Zmodyfikuj `RegisterCommand` – dodaj pole:
        ```csharp
        public Guid SelectedPlatformPlanId { get; init; }
        ```
    * Zmodyfikuj `RegisterCommandHandler` – PO utworzeniu User + Provider + TeamMember:
        1. Pobierz `PlatformPlan` po `SelectedPlatformPlanId` → jeśli nie istnieje: `DomainErrors.ProviderSubscription.PlanNotFound`.
        2. Utwórz `ProviderSubscription.CreateTrial(provider.Id, plan.Id, plan.TrialDays)`.
        3. **Utwórz encję `Client` w tenancie PlatformAdmin**, powiązaną z nowo zarejestrowanym Providerem:
            ```csharp
            // Pobierz TenantId PlatformAdmin (seed/konfiguracja)
            var adminTenantId = await _providerRepository.GetPlatformAdminTenantIdAsync(ct);
            
            var providerAsClient = Client.Create(
                email: command.Email,
                firstName: command.FirstName,
                lastName: command.LastName,
                companyName: command.CompanyName,
                tenantId: adminTenantId,    // ← klient w tenancie ADMINA
                status: ClientStatus.Active  // ← od razu aktywny (sam się zarejestrował)
            );
            await _clientRepository.AddAsync(providerAsClient, ct);
            ```
        4. Zapisz wszystko w jednej transakcji.
    * Zaktualizuj `RegisterCommandValidator`:
        * `SelectedPlatformPlanId`: NotEmpty, MustExistInDatabase (custom validator lub walidacja w handlerze).

* **Infrastructure/Data:**
    * Migracja EF Core:
        * Nowa tabela `PlatformPlans` (Id, Name, Description, PriceAmount, PriceCurrency, BillingPeriod, TrialDays, IsActive, Features jako JSON).
        * Nowa tabela `ProviderSubscriptions` (Id, ProviderId FK, PlatformPlanId FK, Status, StartDate, TrialEndDate, PaidUntil, LastNotificationSentAt).
    * Seed data w `ApplicationDbContext` lub osobnej klasie:
        ```csharp
        // Przykładowe plany platformowe
        new PlatformPlan { Name = "Starter", Price = Money.Create(49, "PLN"), BillingPeriod = Monthly, TrialDays = 14 },
        new PlatformPlan { Name = "Pro",     Price = Money.Create(149, "PLN"), BillingPeriod = Monthly, TrialDays = 14 },
        new PlatformPlan { Name = "Enterprise", Price = Money.Create(399, "PLN"), BillingPeriod = Monthly, TrialDays = 14 },
        ```

* **Infrastructure/Repositories:**
    * Utwórz `IPlatformPlanRepository` i `PlatformPlanRepository`.
    * Utwórz `IProviderSubscriptionRepository` i `ProviderSubscriptionRepository`.
    * Dodaj do `IProviderRepository` metodę: `GetPlatformAdminTenantIdAsync(CancellationToken ct)` – zwraca TenantId jedynego PlatformAdmin.

* **Application/Features/PlatformPlans/Queries:**
    * Utwórz `GetPlatformPlansQuery` + Handler – publiczny endpoint (potrzebny na stronie rejestracji!):
        ```csharp
        // GET /api/PlatformPlans – [AllowAnonymous] – lista planów do wyboru
        ```

* **API/Controllers:**
    * Utwórz `PlatformPlansController`:
        ```csharp
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await Mediator.Send(new GetPlatformPlansQuery());
            return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
        }
        ```

**✅ CHECKLIST WERYFIKACJI (Dla Agenta):**

* [ ] Tabele `PlatformPlans` i `ProviderSubscriptions` istnieją w bazie po migracji
* [ ] Seed data: min. 3 plany platformowe (Starter, Pro, Enterprise) z cenami w PLN
* [ ] `POST /api/Account/register` z `selectedPlatformPlanId` → 201 Created
* [ ] Po rejestracji w bazie: `ProviderSubscriptions` z `Status = Trial`, `TrialEndDate` = +14 dni
* [ ] Po rejestracji w bazie: nowy rekord `Clients` z `TenantId` = TenantId PlatformAdmin
* [ ] Admin zalogowany → `GET /api/Clients` → widzi nowego Providera na liście swoich klientów
* [ ] `GET /api/PlatformPlans` (bez auth) → zwraca listę planów z cenami
* [ ] `dotnet build` → ZERO błędów
* [ ] Git commit: `feat(trial): add PlatformPlan, ProviderSubscription, and auto-trial on registration`

<!-- BLOCK_END: ISSUE_6.1 -->

---

<!-- BLOCK_START: ISSUE_6.2 -->
#### 🛠️ ISSUE 6.2: Trial Subscription – Powiadomienia o zbliżającym się końcu triala (5d / 3d / 24h)

| # | Zadanie | Priorytet | Status | Opis |
|---|---|---|---|---|
| 1 | 🔴 TrialExpirationNotificationJob | Krytyczne | ⬜ | Backend: Background Job sprawdzający daty wygasania triali |
| 2 | 🔴 Email templates (5d, 3d, 24h) | Krytyczne | ⬜ | Backend: 3 warianty emaila z odpowiednim tonem pilności |
| 3 | 🔴 Notification deduplication | Krytyczne | ⬜ | Backend: Zabezpieczenie przed wysyłaniem duplikatów (LastNotificationSentAt + NotificationTier) |
| 4 | 🟡 In-app banner | Ważne | ⬜ | Frontend: Banner ostrzegawczy w dashboardzie Providera |
| 5 | 🟡 NotificationTier tracking | Ważne | ⬜ | Domain: Śledzenie który poziom powiadomienia został wysłany (5d/3d/24h) |

**Blok 6.2 - Wymagania wejściowe**: ISSUE 6.1 (ProviderSubscription z TrialEndDate), ISSUE 3.1 (IEmailService)
**Blok 6.2 - Rezultat**: System automatycznie wysyła powiadomienia email do Providera na 5 dni, 3 dni i 24 godziny przed końcem triala. Każde powiadomienie jest wysyłane dokładnie raz. W dashboardzie Providera wyświetla się banner z informacją o zbliżającym się końcu triala.

---

##### 🤖 PROMPT

**Działaj jako Senior Backend (.NET 9) & Frontend (Next.js 15) Developer pracujący nad Orbito Platform.**

**CEL BIZNESOWY:**
Provider musi być informowany o zbliżającym się końcu okresu próbnego. System wysyła automatyczne emaile: 5 dni przed końcem, 3 dni przed końcem i 24 godziny przed końcem triala. Dodatkowo w dashboardzie wyświetla się banner ostrzegawczy z liczbą pozostałych dni.

**KROKI DO WYKONANIA (BACKEND - CQRS, MediatR):**

* **Domain/Entities:**
    * Rozszerz encję `ProviderSubscription` o śledzenie powiadomień:
        ```csharp
        public TrialNotificationTier LastNotificationTier { get; private set; } // None, FiveDays, ThreeDays, OneDay
        
        public Result MarkNotificationSent(TrialNotificationTier tier)
        {
            if (tier <= LastNotificationTier)
                return Result.Failure(DomainErrors.ProviderSubscription.NotificationAlreadySent);
            
            LastNotificationTier = tier;
            LastNotificationSentAt = DateTime.UtcNow;
            return Result.Success();
        }
        ```
    * Utwórz enum `TrialNotificationTier`:
        ```csharp
        public enum TrialNotificationTier
        {
            None = 0,
            FiveDays = 1,
            ThreeDays = 2,
            OneDay = 3,
            Expired = 4
        }
        ```

* **Infrastructure/Data:**
    * Migracja: Dodaj kolumny `LastNotificationTier` (int, default 0) i `LastNotificationSentAt` do tabeli `ProviderSubscriptions`.

* **Application/Features/ProviderSubscriptions/Commands:**
    * Utwórz `SendTrialExpirationNotificationsCommand` + Handler:
        ```csharp
        public class SendTrialExpirationNotificationsCommandHandler
            : IRequestHandler<SendTrialExpirationNotificationsCommand, Result<int>>
        {
            // 1. Pobierz wszystkie ProviderSubscriptions z Status = Trial
            // 2. Dla każdej sprawdź DaysRemaining:
            //    - DaysRemaining <= 5 && LastNotificationTier < FiveDays → wyślij "5 dni"
            //    - DaysRemaining <= 3 && LastNotificationTier < ThreeDays → wyślij "3 dni"
            //    - DaysRemaining <= 1 && LastNotificationTier < OneDay   → wyślij "24h"
            // 3. Po wysłaniu: subscription.MarkNotificationSent(tier)
            // 4. Zwróć Result.Success(sentCount)
        }
        ```

* **Infrastructure/BackgroundJobs:**
    * Utwórz `TrialExpirationNotificationJob`:
        ```csharp
        public class TrialExpirationNotificationJob : IHostedService, IDisposable
        {
            private Timer? _timer;
            
            public Task StartAsync(CancellationToken cancellationToken)
            {
                // Uruchamiaj co 1 godzinę (wystarczająca granularność dla powiadomień dziennych)
                _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromHours(1));
                return Task.CompletedTask;
            }
            
            private async void DoWork(object? state)
            {
                using var scope = _serviceProvider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                await mediator.Send(new SendTrialExpirationNotificationsCommand());
            }
        }
        ```
    * Zarejestruj job w `DependencyInjection.cs`: `services.AddHostedService<TrialExpirationNotificationJob>();`.

* **Application/Interfaces (IEmailService rozszerzenie):**
    * Dodaj metody do `IEmailService`:
        ```csharp
        Task<Result> SendTrialExpiringAsync(string toEmail, string providerName, int daysRemaining, string planName, CancellationToken ct);
        Task<Result> SendTrialExpiredAsync(string toEmail, string providerName, string planName, string paymentLink, CancellationToken ct);
        ```

* **Infrastructure/Services/EmailService:**
    * Implementuj 3 warianty emaila z rosnącym tonem pilności:
        * **5 dni**: "Twój okres próbny planu {Plan} kończy się za 5 dni. Opłać subskrypcję, aby nie stracić dostępu."
        * **3 dni**: "Zostały Ci tylko 3 dni triala! Przejdź do ustawień konta, aby opłacić subskrypcję."
        * **24h**: "⚠️ OSTATNI DZIEŃ TRIALA! Twoje konto zostanie ograniczone jutro. Opłać teraz: {link}"

* **Application/Features/ProviderSubscriptions/Queries:**
    * Utwórz `GetMyProviderSubscriptionQuery` + Handler:
        * Pobiera ProviderSubscription aktualnie zalogowanego Providera.
        * Zwraca: Status, DaysRemaining, PlanName, PlanPrice, TrialEndDate.
        * Endpoint: `GET /api/ProviderSubscription/my` z polityką `ProviderTeamAccess`.

**KROKI DO WYKONANIA (FRONTEND - Next.js 15):**

* **API & Hooks:**
    1. `npm run api:generate` po dodaniu `GET /api/ProviderSubscription/my`.
    2. Zweryfikuj hook: `useGetApiProviderSubscriptionMy`.

* **Components:**
    1. Utwórz `src/features/billing/components/TrialBanner.tsx`:
        ```typescript
        // Wyświetla się TYLKO gdy ProviderSubscription.Status === "Trial"
        // Warianty wizualne:
        //   - > 5 dni: informacyjny (niebieski) "Okres próbny: X dni pozostało"
        //   - <= 5 dni: ostrzegawczy (żółty) "Twój trial kończy się za X dni!"
        //   - <= 1 dzień: krytyczny (czerwony) "⚠️ Ostatni dzień triala! Opłać teraz →"
        // CTA Button: "Opłać subskrypcję" → /dashboard/billing
        ```
    2. Utwórz `src/features/billing/hooks/useProviderSubscription.ts`:
        ```typescript
        import { useGetApiProviderSubscriptionMy } from "@/core/api/generated/provider-subscription/provider-subscription";
        
        export function useProviderSubscription() {
          const { data, isLoading, error } = useGetApiProviderSubscriptionMy();
          return {
            subscription: data ?? null,
            isLoading,
            error,
            isTrial: data?.status === "Trial",
            isExpired: data?.status === "Expired",
            daysRemaining: data?.daysRemaining ?? 0,
          };
        }
        ```
    3. Zmodyfikuj `src/app/(dashboard)/layout.tsx`:
        * Dodaj `<TrialBanner />` na górze layoutu (nad `{children}`), widoczny na KAŻDEJ stronie dashboardu.
        * Banner powinien się wyświetlać TYLKO gdy `isTrial === true`.

* **Routing/State:**
    * Nowa trasa: `/dashboard/billing` (na razie placeholder – pełna implementacja w ISSUE 6.4).

**✅ CHECKLIST WERYFIKACJI (Dla Agenta):**

* [ ] Migracja dodaje `LastNotificationTier` do `ProviderSubscriptions`
* [ ] `TrialExpirationNotificationJob` jest zarejestrowany i uruchamia się co godzinę (sprawdź logi Serilog)
* [ ] Provider z trialem wygasającym za ≤5 dni → email "5 dni" wysłany, `LastNotificationTier = FiveDays`
* [ ] Ten sam Provider nie dostaje emaila "5 dni" ponownie (deduplikacja działa)
* [ ] Provider z trialem wygasającym za ≤3 dni → email "3 dni" wysłany, `LastNotificationTier = ThreeDays`
* [ ] Provider z trialem wygasającym za ≤1 dzień → email "24h" wysłany, `LastNotificationTier = OneDay`
* [ ] `GET /api/ProviderSubscription/my` → 200 OK z danymi triala (Network Tab)
* [ ] Dashboard: Banner "Okres próbny: X dni pozostało" widoczny na górze
* [ ] Banner zmienia kolor (niebieski → żółty → czerwony) w zależności od daysRemaining
* [ ] `dotnet build` → ZERO błędów, `npm run typecheck` → ZERO błędów
* [ ] Git commit: `feat(trial): add expiration notification job and in-app trial banner`

<!-- BLOCK_END: ISSUE_6.2 -->

---

<!-- BLOCK_START: ISSUE_6.3 -->
#### 🛠️ ISSUE 6.3: Trial Subscription – Wygaśnięcie triala i email z instrukcjami płatności

| # | Zadanie | Priorytet | Status | Opis |
|---|---|---|---|---|
| 1 | 🔴 Trial expiration logic | Krytyczne | ⬜ | Backend: Background Job zmienia status Trial → Expired po upływie terminu |
| 2 | 🔴 Expired email with payment link | Krytyczne | ⬜ | Backend: Email z instrukcjami płatności (link do /dashboard/billing) |
| 3 | 🔴 Access restriction for expired | Krytyczne | ⬜ | Backend: Middleware/policy ograniczająca dostęp Providera z wygasłym trialem |
| 4 | 🟡 Grace period (opcjonalne) | Ważne | ⬜ | Domain: Opcjonalny 3-dniowy grace period po wygaśnięciu przed pełną blokadą |
| 5 | 🟡 Expired state UI | Ważne | ⬜ | Frontend: Ekran blokady z CTA do płatności |

**Blok 6.3 - Wymagania wejściowe**: ISSUE 6.2 (powiadomienia, ProviderSubscription z tiering)
**Blok 6.3 - Rezultat**: Po upływie 14-dniowego triala status ProviderSubscription zmienia się na Expired. Provider otrzymuje email z instrukcjami opłacenia subskrypcji. Dostęp do platformy jest ograniczony do strony billing, dopóki Provider nie opłaci subskrypcji.

---

##### 🤖 PROMPT

**Działaj jako Senior Backend (.NET 9) & Frontend (Next.js 15) Developer pracujący nad Orbito Platform.**

**CEL BIZNESOWY:**
Po upływie okresu próbnego (14 dni) Provider musi zapłacić za subskrypcję. System automatycznie zmienia status na `Expired`, wysyła email z instrukcjami płatności i ogranicza dostęp do platformy, dopóki Provider nie opłaci planu.

**KROKI DO WYKONANIA (BACKEND - CQRS, MediatR):**

* **Application/Features/ProviderSubscriptions/Commands:**
    * Utwórz `ExpireTrialSubscriptionsCommand` + Handler:
        ```csharp
        public class ExpireTrialSubscriptionsCommandHandler
            : IRequestHandler<ExpireTrialSubscriptionsCommand, Result<int>>
        {
            public async Task<Result<int>> Handle(...)
            {
                // 1. Pobierz wszystkie ProviderSubscriptions gdzie:
                //    Status == Trial AND TrialEndDate < DateTime.UtcNow
                var expiredTrials = await _providerSubscriptionRepository
                    .GetExpiredTrialsAsync(cancellationToken);
                
                int count = 0;
                foreach (var subscription in expiredTrials)
                {
                    // 2. Zmień status na Expired
                    var result = subscription.Expire();
                    if (result.IsFailure) continue;
                    
                    // 3. Wyślij email z instrukcjami płatności
                    var provider = await _providerRepository.GetByIdAsync(subscription.ProviderId, ct);
                    var plan = await _platformPlanRepository.GetByIdAsync(subscription.PlatformPlanId, ct);
                    
                    var paymentLink = $"{_appSettings.BaseUrl}/dashboard/billing";
                    
                    await _emailService.SendTrialExpiredAsync(
                        provider.Email,
                        provider.Name,
                        plan.Name,
                        paymentLink,
                        cancellationToken
                    );
                    
                    subscription.MarkNotificationSent(TrialNotificationTier.Expired);
                    count++;
                }
                
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result.Success(count);
            }
        }
        ```

* **Infrastructure/BackgroundJobs:**
    * Rozszerz `TrialExpirationNotificationJob` (z ISSUE 6.2) lub utwórz oddzielny `ExpireTrialsJob`:
        * Uruchamia się co godzinę (razem z powiadomieniami).
        * Wywołuje `SendTrialExpirationNotificationsCommand` (powiadomienia) ORAZ `ExpireTrialSubscriptionsCommand` (wygaśnięcia).

* **Infrastructure/Repositories:**
    * Dodaj do `IProviderSubscriptionRepository`:
        ```csharp
        Task<List<ProviderSubscription>> GetExpiredTrialsAsync(CancellationToken ct);
        // Gdzie: Status == Trial AND TrialEndDate < DateTime.UtcNow
        ```

* **Security – Access Restriction:**
    * Utwórz `ActiveProviderSubscriptionRequirement` i `ActiveProviderSubscriptionHandler`:
        ```csharp
        public class ActiveProviderSubscriptionHandler 
            : AuthorizationHandler<ActiveProviderSubscriptionRequirement>
        {
            protected override async Task HandleRequirementAsync(...)
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null) return;
                
                // PlatformAdmin nie podlega restrykcji triala
                if (context.User.IsInRole("PlatformAdmin"))
                {
                    context.Succeed(requirement);
                    return;
                }
                
                var subscription = await _providerSubscriptionRepository
                    .GetByProviderUserIdAsync(Guid.Parse(userId), ct);
                
                if (subscription == null)
                {
                    context.Fail();
                    return;
                }
                
                // Trial aktywny LUB opłacona subskrypcja → dostęp OK
                if (subscription.IsTrialActive || subscription.Status == ProviderSubscriptionStatus.Active)
                {
                    context.Succeed(requirement);
                    return;
                }
                
                // Expired → brak dostępu (frontend przekieruje na /billing)
                context.Fail();
            }
        }
        ```
    * Zarejestruj nową policy: `PolicyNames.ActiveProviderSubscription`.
    * **WAŻNE**: Zdecyduj, które kontrolery wymagają aktywnej subskrypcji. Rekomendacja:
        * `ClientsController`, `SubscriptionsController`, `PlansController`, `TeamController` → wymagają `ActiveProviderSubscription`.
        * `ProviderSubscriptionController` (billing), `AccountController` (profil) → **NIE** wymagają (Provider musi mieć dostęp do płatności nawet z wygasłym trialem!).
    * Alternatywnie: Zamiast policy per kontroler, dodaj middleware sprawdzający na poziomie pipeline, który zwraca `402 Payment Required` i pozwala na whitelist endpointów.

* **API/Controllers:**
    * Dodaj endpoint informujący frontend o stanie subskrypcji:
        * `GET /api/ProviderSubscription/my` (z ISSUE 6.2) już zwraca status.
        * Frontend na podstawie `status === "Expired"` wyświetli ekran blokady.

**KROKI DO WYKONANIA (FRONTEND - Next.js 15):**

* **Components:**
    1. Utwórz `src/features/billing/components/SubscriptionExpiredOverlay.tsx`:
        ```typescript
        // Full-screen overlay (lub redirect) dla Providera z wygasłą subskrypcją.
        // Tekst: "Twój okres próbny się zakończył"
        // Opis: "Aby kontynuować korzystanie z Orbito, opłać subskrypcję planu {planName}."
        // CTA: Button "Opłać teraz – {price}/mies." → /dashboard/billing
        // Link: "Zmień plan" → /dashboard/billing?change=true
        // Brak możliwości zamknięcia / nawigacji w inne miejsce
        ```
    2. Zmodyfikuj `src/features/billing/components/TrialBanner.tsx` (z ISSUE 6.2):
        * Dodaj wariant `expired`:
            * Czerwony banner: "⚠️ Twój okres próbny się zakończył. Opłać subskrypcję, aby odzyskać dostęp."
    3. Zmodyfikuj `src/app/(dashboard)/layout.tsx`:
        * Po załadowaniu danych `useProviderSubscription()`:
            * Jeśli `isExpired === true` → renderuj `<SubscriptionExpiredOverlay />` zamiast `{children}`.
            * Jeśli `isTrial === true` → renderuj `<TrialBanner />` + `{children}`.
            * W przeciwnym razie → renderuj normalnie `{children}`.
        * **WYJĄTEK**: Trasa `/dashboard/billing` MUSI być dostępna nawet gdy `isExpired === true` (inaczej Provider nie będzie mógł zapłacić!).

* **Routing/State:**
    * Zmodyfikuj `useProviderSubscription` hook – dodaj:
        ```typescript
        const pathname = usePathname();
        const isBillingPage = pathname?.startsWith("/dashboard/billing");
        const shouldBlock = isExpired && !isBillingPage;
        ```

**✅ CHECKLIST WERYFIKACJI (Dla Agenta):**

* [ ] Background Job zmienia `Status = Expired` gdy `TrialEndDate < now` (sprawdź SQL)
* [ ] Email "Trial wygasł" wysłany z linkiem do `/dashboard/billing`
* [ ] Provider z Expired subscription → `GET /api/Clients` → 403 (lub 402 Payment Required)
* [ ] Provider z Expired subscription → `GET /api/ProviderSubscription/my` → 200 OK (dostęp do billing!)
* [ ] Frontend: Expired Provider widzi overlay "Twój okres próbny się zakończył" na dashboardzie
* [ ] Frontend: Expired Provider może przejść na `/dashboard/billing` (nie jest blokowany)
* [ ] Frontend: Expired Provider NIE może przejść na `/dashboard/clients`, `/dashboard/plans` etc.
* [ ] PlatformAdmin NIE jest blokowany przez trial restrictions
* [ ] `dotnet build` → ZERO błędów, `npm run typecheck` → ZERO błędów
* [ ] Git commit: `feat(trial): add trial expiration, access restriction, and expired state UI`

<!-- BLOCK_END: ISSUE_6.3 -->

---

<!-- BLOCK_START: ISSUE_6.4 -->
#### 🛠️ ISSUE 6.4: Trial Subscription – Frontend: Wybór planu przy rejestracji + UI płatności Providera

| # | Zadanie | Priorytet | Status | Opis |
|---|---|---|---|---|
| 1 | 🔴 Register form – plan selection | Krytyczne | ⬜ | Frontend: Krok wyboru planu platformowego w formularzu rejestracji |
| 2 | 🔴 Billing page /dashboard/billing | Krytyczne | ⬜ | Frontend: Strona z aktualnym planem, statusem triala i Stripe Payment |
| 3 | 🔴 CreateProviderPaymentIntent | Krytyczne | ⬜ | Backend: Endpoint tworzący PaymentIntent dla subskrypcji Providera |
| 4 | 🔴 Stripe Elements for Provider | Krytyczne | ⬜ | Frontend: Formularz płatności z Stripe Payment Element (PCI DSS) |
| 5 | 🟡 Plan comparison cards | Ważne | ⬜ | Frontend: Karty porównawcze planów na stronie rejestracji |
| 6 | 🟡 Webhook: Provider payment confirm | Ważne | ⬜ | Backend: Webhook `payment_intent.succeeded` → ProviderSubscription.Activate() |
| 7 | 🟡 Change plan flow | Ważne | ⬜ | Frontend: Możliwość zmiany planu z poziomu /dashboard/billing |

**Blok 6.4 - Wymagania wejściowe**: ISSUE 6.3 (expiration logic), ISSUE 4.1 (Stripe Elements pattern)
**Blok 6.4 - Rezultat**: Przy rejestracji Provider wybiera plan z kart porównawczych. W `/dashboard/billing` widzi swój aktualny plan, status triala/subskrypcji i może opłacić subskrypcję przez Stripe Payment Element. Po udanej płatności subskrypcja staje się Active.

---

##### 🤖 PROMPT

**Działaj jako Senior Backend (.NET 9) & Frontend (Next.js 15) Developer pracujący nad Orbito Platform.**

**CEL BIZNESOWY:**
Provider musi mieć możliwość opłacenia subskrypcji platformowej z poziomu aplikacji. Przy rejestracji wybiera plan (Starter/Pro/Enterprise), dostaje 14-dniowy trial, a potem płaci przez Stripe. Strona `/dashboard/billing` to centralne miejsce zarządzania subskrypcją Providera.

⚠️ **PCI DSS Compliance**: Identycznie jak w ISSUE 4.1 – dane karty NIGDY nie przechodzą przez serwery Orbito. Używamy `Stripe Payment Element`.

**KROKI DO WYKONANIA (BACKEND - CQRS, MediatR):**

* **Application/Features/ProviderSubscriptions/Commands:**
    * Utwórz `CreateProviderPaymentIntentCommand`:
        ```csharp
        public record CreateProviderPaymentIntentCommand : IRequest<Result<CreateProviderPaymentIntentResponse>>
        {
            public Guid? PlatformPlanId { get; init; } // null = opłać aktualny plan
        }
        
        public record CreateProviderPaymentIntentResponse
        {
            public string ClientSecret { get; init; }
            public string PaymentIntentId { get; init; }
            public decimal Amount { get; init; }
            public string Currency { get; init; }
            public string PlanName { get; init; }
        }
        ```
    * Utwórz `CreateProviderPaymentIntentCommandHandler`:
        1. Pobierz aktualnie zalogowanego Providera z `IUserContextService`.
        2. Pobierz `ProviderSubscription` Providera.
        3. Jeśli `PlatformPlanId` podany → użyj tego planu (zmiana planu). Jeśli null → użyj aktualnego `PlatformPlanId`.
        4. Pobierz `PlatformPlan` po ID.
        5. Pobierz lub utwórz Stripe Customer dla Providera (Stripe CustomerId w Provider entity).
        6. Wywołaj Stripe API:
            ```csharp
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(plan.Price.Amount * 100),
                Currency = plan.Price.Currency.ToLower(),
                Customer = provider.StripeCustomerId,
                Metadata = new Dictionary<string, string>
                {
                    { "provider_id", provider.Id.ToString() },
                    { "platform_plan_id", plan.Id.ToString() },
                    { "subscription_type", "platform" } // ← rozróżnienie od subskrypcji klientów!
                }
            };
            ```
        7. Zwróć `Result.Success(response)`.

* **Application/Features/Payments/Commands (Webhook update):**
    * Zmodyfikuj `ProcessWebhookEventCommandHandler` / `UpdatePaymentFromWebhookCommandHandler`:
        * Przy obsłudze `payment_intent.succeeded` sprawdź `metadata["subscription_type"]`:
            * Jeśli `"platform"` → to płatność za subskrypcję Providera:
                ```csharp
                var providerId = Guid.Parse(metadata["provider_id"]);
                var planId = Guid.Parse(metadata["platform_plan_id"]);
                var subscription = await _providerSubscriptionRepository.GetByProviderIdAsync(providerId, ct);
                
                // Aktywuj subskrypcję na 30 dni (lub zgodnie z BillingPeriod planu)
                var plan = await _platformPlanRepository.GetByIdAsync(planId, ct);
                var paidUntil = DateTime.UtcNow.AddDays(plan.BillingPeriod == BillingPeriodType.Monthly ? 30 : 365);
                subscription.Activate(paidUntil);
                
                await _unitOfWork.SaveChangesAsync(ct);
                ```
            * Jeśli brak `subscription_type` lub inny → obsłuż jako dotychczasową płatność klienta.

* **API/Controllers:**
    * Utwórz `ProviderBillingController`:
        ```csharp
        [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
        [ApiController]
        [Route("api/[controller]")]
        public class ProviderBillingController : BaseController
        {
            // Endpoint tworzący PaymentIntent – MUSI działać nawet z expired trial!
            [HttpPost("create-payment-intent")]
            [ProducesResponseType(typeof(CreateProviderPaymentIntentResponse), StatusCodes.Status200OK)]
            public async Task<IActionResult> CreatePaymentIntent([FromBody] CreateProviderPaymentIntentCommand command)
            {
                var result = await Mediator.Send(command);
                return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
            }
        }
        ```
    * ⚠️ **KRYTYCZNE**: `ProviderBillingController` MUSI być dostępny nawet gdy trial wygasł! NIE dodawaj policy `ActiveProviderSubscription` do tego kontrolera.

**KROKI DO WYKONANIA (FRONTEND - Next.js 15):**

* **API & Hooks:**
    1. `npm run api:generate` po dodaniu endpointów.
    2. Zweryfikuj hooki: `useGetApiPlatformPlans`, `usePostApiProviderBillingCreatePaymentIntent`.

* **Components – Rejestracja (Plan Selection):**
    1. Utwórz `src/features/auth/components/PlanSelectionStep.tsx`:
        ```typescript
        // Wyświetla karty porównawcze planów (Starter / Pro / Enterprise)
        // Każda karta zawiera: Nazwę, Cenę, Lista Features, Button "Wybierz"
        // Po wybraniu: setSelectedPlanId(plan.id)
        // Dane z: useGetApiPlatformPlans() (endpoint publiczny AllowAnonymous)
        ```
    2. Zmodyfikuj `src/features/auth/components/RegisterForm.tsx`:
        * Przebuduj na **2-krokowy formularz**:
            * **Krok 1**: `PlanSelectionStep` – wybór planu.
            * **Krok 2**: Istniejące pola (email, hasło, imię, nazwisko, firma) + hidden `selectedPlatformPlanId`.
        * Submit wysyła cały formularz z `selectedPlatformPlanId`.
    3. Utwórz `src/features/billing/components/PlanComparisonCard.tsx`:
        ```typescript
        // Karta pojedynczego planu:
        // - Nazwa (bold, duży font)
        // - Cena: "49 PLN / mies." (formatCurrency)
        // - Features: lista z checkmarks ✅
        // - Badge "Najpopularniejszy" (dla planu Pro)
        // - Button: "Wybierz" / "Aktualny plan" (disabled)
        // - Wariant: isSelected (obramowanie podświetlone)
        ```

* **Components – Billing Page:**
    1. Utwórz `src/app/(dashboard)/dashboard/billing/page.tsx`:
        ```typescript
        // Główna strona billing. Sekcje:
        // 1. "Twój aktualny plan" – karta z nazwą planu, ceną, statusem (Trial/Active/Expired)
        // 2. "Status subskrypcji":
        //    - Trial: "Okres próbny do {date} ({daysRemaining} dni)"
        //    - Active: "Opłacony do {paidUntil}"
        //    - Expired: "⚠️ Subskrypcja wygasła"
        // 3. "Opłać subskrypcję" – sekcja z Stripe Payment Element
        // 4. (Opcjonalnie) "Zmień plan" – karty porównawcze z możliwością zmiany
        ```
    2. Utwórz `src/features/billing/components/BillingPageContent.tsx`:
        * Pobiera dane: `useProviderSubscription()` + `useGetApiPlatformPlans()`.
        * Wyświetla aktualny plan, status, daysRemaining.
        * Sekcja płatności:
            * Button "Opłać {planName} – {price}/mies." → wywołuje `createPaymentIntent` mutation.
            * Po otrzymaniu `clientSecret` → renderuj `<StripeProvider>` + `<PaymentForm>` (reuse z ISSUE 4.1!).
    3. Utwórz `src/features/billing/components/ProviderPaymentForm.tsx`:
        * ⚠️ **Reuse pattern z ISSUE 4.1!** Możesz reużyć `StripeProvider` i `PaymentForm` jako shared components.
        * Po udanej płatności: Stripe redirect na `/dashboard/billing?payment=success`.
        * Success state: "Subskrypcja aktywowana! Dziękujemy za płatność." + Button "Przejdź do dashboardu".
    4. Utwórz `src/features/billing/components/CurrentPlanCard.tsx`:
        ```typescript
        // Karta z aktualnym planem:
        // - Nazwa planu (bold)
        // - Cena (formatCurrency)
        // - Status Badge:
        //     Trial → żółty "Okres próbny"
        //     Active → zielony "Aktywny"
        //     Expired → czerwony "Wygasły"
        // - Daty: TrialEndDate / PaidUntil
        // - DaysRemaining (jeśli Trial)
        ```

* **Routing/State:**
    * `/dashboard/billing` – dostępna ZAWSZE (nawet z expired trial).
    * `/dashboard/billing?payment=success` – success state po płatności.
    * Sidebar: Dodaj link "Subskrypcja" / "Billing" do menu nawigacji (ikona: CreditCard).

**✅ CHECKLIST WERYFIKACJI (Dla Agenta):**

* [ ] **Rejestracja**: Strona register pokazuje karty planów → po wybraniu → formularz rejestracji
* [ ] **Rejestracja**: `POST /api/Account/register` zawiera `selectedPlatformPlanId` (Network Tab)
* [ ] **Billing**: `/dashboard/billing` wyświetla aktualny plan, status, cenę
* [ ] **Billing**: Button "Opłać" → `POST /api/ProviderBilling/create-payment-intent` → 200 z `clientSecret`
* [ ] **Stripe**: Payment Element renderuje się poprawnie (pola karty widoczne)
* [ ] ⚠️ **PCI DSS**: W Network Tab NIE MA requestów z danymi karty do backendu Orbito
* [ ] **Webhook**: Po udanej płatności → `ProviderSubscription.Status = Active`, `PaidUntil` ustawiony
* [ ] **UI po płatności**: Success state "Subskrypcja aktywowana" + przycisk "Przejdź do dashboardu"
* [ ] **Expired Provider**: Ma dostęp do `/dashboard/billing` (nie jest blokowany!)
* [ ] **Expired Provider po płatności**: SubscriptionExpiredOverlay znika, pełny dostęp odzyskany
* [ ] **Sidebar**: Link "Subskrypcja" widoczny w menu nawigacji
* [ ] `npm run typecheck` → ZERO błędów, `dotnet build` → ZERO błędów
* [ ] Git commit: `feat(billing): add plan selection at registration and provider payment UI`

<!-- BLOCK_END: ISSUE_6.4 -->

---

## 📊 PODSUMOWANIE ZALEŻNOŚCI MIĘDZY BLOKAMI

```
ISSUE 1.1 (Admin Singleton) ──────────────── [niezależny]
          │
          └──────────┐
                     ▼
ISSUE 2.1 (Auto-create Provider) ─────────── [niezależny]
          │
          ├──────────────────────────────────┐
          ▼                                  ▼
ISSUE 3.1 (Token & Email Service) ─── ISSUE 6.1 (Trial Domain + Reg + Admin Client)
          │                                  │
          ▼                                  ▼
ISSUE 3.2 (Confirm Email & Activate)  ISSUE 6.2 (Trial Notifications 5d/3d/24h)
          │                                  │
          ▼                                  ▼
ISSUE 3.3 (Frontend: Invite Form)    ISSUE 6.3 (Trial Expiration + Access Block)
          │                                  │
          ▼                                  ▼
ISSUE 3.4 (Frontend: Confirm Page)   ISSUE 6.4 (Plan Selection + Provider Payment UI)
          │
          ▼
ISSUE 4.1 (Stripe Elements Portal) ──────── [wymaga: 3.4]
          │                                  ▲
          ▼                                  │
ISSUE 4.2 (Webhook Sync Dashboard) ─── ISSUE 6.4 (reuse Stripe pattern)
          │
          ▼
ISSUE 5.1 (Portal Hardening) ────────────── [wymaga: 4.1, 4.2]
```

**Rekomendowana kolejność realizacji:**
1. ISSUE 1.1 + ISSUE 2.1 (równolegle – niezależne)
2. ISSUE 3.1 → 3.2 → 3.3 → 3.4 (sekwencyjnie – flow zapraszania klienta)
3. ISSUE 6.1 → 6.2 → 6.3 → 6.4 (sekwencyjnie – flow triala, **równolegle z blokami 3.x**)
4. ISSUE 4.1 → 4.2 (sekwencyjnie – Stripe, po 3.4; pattern reużywany w 6.4)
5. ISSUE 5.1 (na końcu, po pełnym flow)

**Ścieżka krytyczna:**
- Ścieżka A (klienci): 2.1 → 3.1 → 3.2 → 3.3 → 3.4 → 4.1 → 4.2 → 5.1
- Ścieżka B (trial): 1.1 + 2.1 → 6.1 → 6.2 → 6.3 → 6.4
- Ścieżka B może być realizowana **równolegle** ze ścieżką A (po ukończeniu 2.1).
- ISSUE 6.4 reużywa Stripe Elements z ISSUE 4.1 – jeśli 4.1 jest ukończony wcześniej, 6.4 będzie prostszy.

---

_Ostatnia aktualizacja: 2026-02-28_
_Wersja: 2.0 (dodano ISSUE 6.x – Trial Subscription System)_
