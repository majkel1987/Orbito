# Orbito - Multi-Tenant SaaS Platform

## 🏗️ Architektura Aplikacji

Orbito to nowoczesna platforma SaaS zbudowana w architekturze Clean Architecture, wykorzystująca wzorce DDD (Domain-Driven Design) i CQRS z MediatR.

### 📁 Struktura Projektu

```
Orbito/
├── Orbito.API/                 # Warstwa prezentacji (Web API)
├── Orbito.Application/         # Warstwa aplikacji (CQRS, MediatR)
├── Orbito.Domain/             # Warstwa domeny (Encje, Value Objects)
└── Orbito.Infrastructure/     # Warstwa infrastruktury (EF Core, Identity)
```

### 🎯 Główne Komponenty

#### Orbito.API

- **ASP.NET Core 9.0** Web API
- **Swagger/OpenAPI** dokumentacja
- **Serilog** logowanie do plików
- **JWT Authentication**
- **CORS** konfiguracja
- **Health Checks**

#### Orbito.Application

- **MediatR** - CQRS pattern
- **FluentValidation** - walidacja
- **Pipeline Behaviors**:
  - `LoggingBehaviour` - logowanie żądań
  - `ValidationBehaviour` - walidacja
  - `PerformanceBehaviour` - monitorowanie wydajności
- **Commands/Queries**:
  - `CreateProviderCommand` - tworzenie nowego providera z automatycznym TenantId
- **Services**:
  - `TenantContext` - zarządzanie kontekstem tenanta
  - `DateTimeService` - abstrakcja dla operacji na czasie

#### Orbito.Domain

- **Encje domenowe**: `Provider`, `Client`, `Subscription`, `Payment`
- **Value Objects**: `TenantId`, `Money`, `Email`, `BillingPeriod`
- **Identity**: `ApplicationUser`, `ApplicationRole`
- **Enums**: `PaymentStatus`, `SubscriptionStatus`, `UserRole`

#### Orbito.Infrastructure

- **Entity Framework Core 9.0** z SQL Server
- **ASP.NET Core Identity**
- **JWT Bearer Authentication**
- **Health Checks** z EF Core
- **Repository Pattern** - UnitOfWork z generycznymi repozytoriami
- **Tenant Middleware** - automatyczne wykrywanie kontekstu tenanta

## 🚀 Uruchomienie Aplikacji

### Wymagania

- .NET 9.0 SDK
- SQL Server (LocalDB/SQL Express)
- Visual Studio 2022 lub VS Code

### Konfiguracja

1. **Klonowanie repozytorium**

```bash
git clone <repository-url>
cd Orbito
```

2. **Konfiguracja bazy danych**

```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=Orbito_test;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=true"
  }
}
```

3. **Uruchomienie migracji**

```bash
cd Orbito.API
dotnet ef database update --project ../Orbito.Infrastructure
```

4. **Uruchomienie aplikacji**

```bash
dotnet run --project Orbito.API
```

### 🌐 Endpointy

- **API**: `https://localhost:5211`
- **Swagger**: `https://localhost:5211/swagger`
- **Health Check**: `https://localhost:5211/health`
- **Health Check UI**: `https://localhost:5211/healthchecks-ui`

### 🔐 Endpointy Uwierzytelniania

#### AccountController

- `GET /api/account/admin-setup-status` - Sprawdza status setup administratora
- `POST /api/account/setup-admin` - Bezpieczna rejestracja początkowego administratora (tylko przy pierwszym uruchomieniu)
- `POST /api/account/register-provider` - Rejestracja nowego providera z automatycznym przypisaniem roli
- `POST /api/account/login` - Logowanie użytkownika

#### ProvidersController

- `POST /api/providers` - Tworzenie nowego providera (wymaga roli PlatformAdmin)

## 📊 Logowanie

Aplikacja wykorzystuje **Serilog** z konfiguracją do oddzielnych plików:

- **Informacje**: `logs/info-{date}.txt`
- **Błędy**: `logs/errors-{date}.txt`
- **Konsola**: Wszystkie poziomy w trybie Development

### Konfiguracja Logowania

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/info-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        restrictedToMinimumLevel: LogEventLevel.Information)
    .WriteTo.File("logs/errors-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        restrictedToMinimumLevel: LogEventLevel.Error)
    .CreateLogger();
```

## 🔐 Uwierzytelnianie i Autoryzacja

### 🔒 Bezpieczny Model Uwierzytelniania

Aplikacja implementuje **bezpieczny model uwierzytelniania** z następującymi zabezpieczeniami:

#### 1. Bezpieczna Rejestracja Administratora

- **Jednorazowa konfiguracja** - administrator może być utworzony tylko przy pierwszym uruchomieniu aplikacji
- **Kontrola środowiska** - setup administratora jest dostępny tylko w środowisku Development lub gdy jest włączony w konfiguracji
- **Automatyczna blokada** - po utworzeniu pierwszego administratora, endpoint setup jest automatycznie blokowany

#### 2. Rejestracja Providerów

- **Publiczny endpoint** - providerzy mogą się rejestrować bez ograniczeń
- **Automatyczne przypisanie roli** - nowi providerzy automatycznie otrzymują rolę `Provider`
- **Walidacja subdomain** - sprawdzanie unikalności subdomain przed utworzeniem konta

### JWT Configuration

```json
{
  "Jwt": {
    "Key": "your-secret-key-here",
    "Issuer": "http://localhost:5211",
    "Audience": "http://localhost:5211"
  },
  "AdminSetup": {
    "Enabled": false
  }
}
```

### Konfiguracja AdminSetup

```json
// appsettings.Development.json
{
  "AdminSetup": {
    "Enabled": true
  }
}

// appsettings.json (Production)
{
  "AdminSetup": {
    "Enabled": false
  }
}
```

### Role System

- **PlatformAdmin** - Administrator platformy
- **Provider** - Dostawca usług
- **Client** - Klient

### JWT Claims

Token JWT zawiera następujące claims:

- `sub` - ID użytkownika
- `email` - adres email
- `name` - pełne imię i nazwisko
- `role` - rola użytkownika
- `tenant_id` - ID tenanta (pusty dla PlatformAdmin)
- `user_role` - rola użytkownika (duplikat dla kompatybilności)
- `jti` - unikalny identyfikator tokenu
- `iat` - czas utworzenia tokenu

## 🏢 Multi-Tenancy

Aplikacja implementuje wzorzec **Multi-Tenancy** pozwalający jednej instancji aplikacji obsługiwać wielu "najemców" (tenantów) - różnych klientów, organizacji lub grup użytkowników.

### 🎯 Architektura Multi-Tenancy

#### Tenant Model

- **Provider** - główny tenant (dostawca usług)
- **TenantId** - Value Object identyfikujący tenant
- **Automatyczne filtrowanie** danych według tenant
- **Izolacja danych** na poziomie bazy danych

#### Hierarchia Tenantów

```
Platform (Global)
├── Provider A (TenantId: Guid-A)
│   ├── Clients
│   ├── SubscriptionPlans
│   ├── Subscriptions
│   └── Payments
├── Provider B (TenantId: Guid-B)
│   ├── Clients
│   ├── SubscriptionPlans
│   ├── Subscriptions
│   └── Payments
└── ...
```

### 🔧 Implementacja Multi-Tenancy

#### 1. TenantId Value Object

```csharp
public sealed class TenantId : IEquatable<TenantId>
{
    public Guid Value { get; }

    public static TenantId Create(Guid value)
    public static TenantId New()
    public static implicit operator Guid(TenantId tenantId)
}
```

#### 2. IMustHaveTenant Interface

```csharp
public interface IMustHaveTenant
{
    TenantId TenantId { get; }
}
```

#### 3. Entity Configurations

Wszystkie encje domenowe implementują `IMustHaveTenant`:

- **Provider** - główny tenant
- **Client** - klienci providera
- **SubscriptionPlan** - plany subskrypcji
- **Subscription** - aktywne subskrypcje
- **Payment** - płatności

#### 4. Query Filters

```csharp
// Automatyczne filtrowanie danych według tenant
builder.Entity<Provider>()
    .HasQueryFilter(p => p.TenantId.Value == currentTenantId);

builder.Entity<Client>()
    .HasQueryFilter(c => c.TenantId.Value == currentTenantId);
```

#### 5. Identity Integration

- **ApplicationUser** - może należeć do konkretnego tenanta
- **ApplicationRole** - role globalne (TenantId = null) lub tenant-specific
- **Automatyczne przypisywanie** TenantId dla nowych użytkowników

### 🗄️ Struktura Bazy Danych

#### Tabele Multi-Tenant

```sql
-- Główne tabele z TenantId
Providers (TenantId PK)
Clients (TenantId FK -> Providers.TenantId)
SubscriptionPlans (TenantId FK -> Providers.TenantId)
Subscriptions (TenantId FK -> Providers.TenantId)
Payments (TenantId FK -> Providers.TenantId)

-- Identity z opcjonalnym TenantId
AspNetUsers (TenantId nullable)
AspNetRoles (TenantId nullable)
```

#### Indeksy Multi-Tenancy

```sql
-- Główne indeksy dla wydajności
IX_Providers_TenantId (UNIQUE)
IX_Clients_TenantId
IX_SubscriptionPlans_TenantId
IX_Subscriptions_TenantId
IX_Payments_TenantId

-- Indeksy złożone
IX_Clients_TenantId_DirectEmail
IX_Subscriptions_TenantId_Status
IX_Payments_TenantId_Status
```

### 🔐 Bezpieczeństwo Multi-Tenancy

#### 1. Izolacja Danych

- **Query Filters** - automatyczne filtrowanie na poziomie EF Core
- **TenantId Validation** - sprawdzanie zgodności tenant w operacjach
- **Foreign Key Constraints** - relacje między encjami tego samego tenanta

#### 2. Autoryzacja

```csharp
// Sprawdzanie dostępu do zasobów tenanta
public async Task<bool> CanAccessResource(Guid resourceId, TenantId userTenantId)
{
    var resource = await _context.Resources
        .FirstOrDefaultAsync(r => r.Id == resourceId && r.TenantId == userTenantId);

    return resource != null;
}
```

#### 3. Middleware Multi-Tenancy

Aplikacja implementuje `TenantMiddleware` który automatycznie wykrywa i ustawia kontekst tenanta na podstawie:

1. **JWT Claims** - `tenant_id` claim z tokenu JWT
2. **HTTP Headers** - `X-Tenant-Id` header
3. **Query Parameters** - `tenantId` query parameter

```csharp
// Automatyczne ustawianie kontekstu tenanta
public class TenantMiddleware
{
    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        // Sprawdź JWT claims
        var tenantIdClaim = context.User.FindFirst("tenant_id")?.Value;

        // Sprawdź HTTP header
        var tenantHeader = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();

        // Sprawdź query parameter
        var tenantQuery = context.Request.Query["tenantId"].FirstOrDefault();

        // Ustaw kontekst tenanta
        tenantContext.SetTenant(tenantId);
        await next(context);
    }
}
```

#### 4. Tenant Context Service

```csharp
public interface ITenantContext
{
    TenantId? CurrentTenantId { get; }
    void SetTenant(TenantId? tenantId);
    void ClearTenant();
    bool HasTenant { get; }
}
```

### 📊 Monitorowanie Multi-Tenancy

#### 1. Metryki Tenant

- **Liczba aktywnych klientów** per tenant
- **Przychody miesięczne** per tenant
- **Aktywne subskrypcje** per tenant
- **Wydajność zapytań** per tenant

#### 2. Logowanie Multi-Tenant

```csharp
// Logi z kontekstem tenanta
_logger.LogInformation("User {UserId} from tenant {TenantId} accessed resource {ResourceId}",
    userId, tenantId, resourceId);
```

### 🚀 Korzyści Multi-Tenancy

#### 1. Skalowalność

- **Jedna instancja** aplikacji dla wielu klientów
- **Współdzielone zasoby** (serwer, baza danych)
- **Automatyczne skalowanie** bez duplikacji infrastruktury

#### 2. Koszty

- **Niższe koszty** operacyjne
- **Współdzielona infrastruktura**
- **Efektywne wykorzystanie** zasobów

#### 3. Zarządzanie

- **Centralne zarządzanie** wszystkimi tenantami
- **Jednolite aktualizacje** dla wszystkich klientów
- **Uproszczone wdrożenia**

### 🔄 Workflow Multi-Tenancy

#### 1. Bezpieczna Rejestracja Administratora (Jednorazowa)

```csharp
// 1. Sprawdzenie statusu setup administratora
GET /api/account/admin-setup-status
// Response: { "isSetupRequired": true, "isSetupEnabled": true }

// 2. Utworzenie początkowego administratora (tylko przy pierwszym uruchomieniu)
POST /api/account/setup-admin
{
    "email": "admin@example.com",
    "password": "SecurePassword123!",
    "firstName": "Jan",
    "lastName": "Kowalski"
}

// 3. Logowanie PlatformAdmin
POST /api/account/login
{
    "email": "admin@example.com",
    "password": "SecurePassword123!"
}
```

#### 2. Rejestracja Nowego Providera

```csharp
// 1. Rejestracja Provider (publiczny endpoint)
POST /api/account/register-provider
{
    "email": "provider@example.com",
    "password": "SecurePassword123!",
    "firstName": "Anna",
    "lastName": "Nowak",
    "businessName": "Moja Firma",
    "subdomainSlug": "moja-firma",
    "description": "Opis firmy"
}

// 2. Automatyczne przypisanie roli Provider i utworzenie TenantId
// 3. Logowanie Provider
POST /api/account/login
{
    "email": "provider@example.com",
    "password": "SecurePassword123!"
}
```

#### 3. Tworzenie Provider przez PlatformAdmin (Alternatywny sposób)

```csharp
// 1. Logowanie PlatformAdmin
POST /api/account/login
{
    "email": "admin@example.com",
    "password": "SecurePassword123!"
}

// 2. Tworzenie Provider (wymaga roli PlatformAdmin)
POST /api/providers
{
    "userId": "user-guid",
    "businessName": "Moja Firma",
    "subdomainSlug": "moja-firma",
    "description": "Opis firmy"
}
```

#### 2. Dodawanie Klienta do Tenanta

```csharp
// Klient automatycznie otrzymuje TenantId providera
var client = Client.CreateWithUser(provider.TenantId, userId, companyName);
```

#### 3. Zarządzanie Subskrypcjami

```csharp
// Subskrypcja automatycznie dziedziczy TenantId
var subscription = Subscription.Create(provider.TenantId, clientId, planId, price, billingPeriod);
```

## 📈 Monitorowanie Wydajności

### Performance Settings

```json
{
  "PerformanceSettings": {
    "MonitorThresholdMs": 200,
    "WarningThresholdMs": 500,
    "CriticalThresholdMs": 1000
  }
}
```

### Pipeline Behaviors

- **LoggingBehaviour**: Loguje wszystkie żądania z czasem wykonania
- **ValidationBehaviour**: Waliduje żądania przed przetworzeniem
- **PerformanceBehaviour**: Monitoruje wydajność i loguje ostrzeżenia

## 🛠️ Narzędzia Deweloperskie

### Packages

- **MediatR** 13.0.0 - CQRS pattern
- **FluentValidation** 12.0.0 - Walidacja
- **Serilog.AspNetCore** 8.0.0 - Logowanie
- **Entity Framework Core** 9.0.0 - ORM
- **ASP.NET Core Identity** 9.0.9 - Uwierzytelnianie

### Development Tools

- **Swagger/OpenAPI** - Dokumentacja API
- **Health Checks** - Monitorowanie stanu aplikacji
- **EF Core Tools** - Migracje bazy danych

## 📋 Plan Rozwoju

### ✅ Zakończone

- [x] Podstawowa architektura Clean Architecture
- [x] Konfiguracja logowania z Serilog
- [x] JWT Authentication
- [x] Pipeline Behaviors (Logging, Validation, Performance)
- [x] Health Checks
- [x] Swagger dokumentacja
- [x] **Multi-Tenancy Architecture** - kompletna implementacja
- [x] **Entity Configurations** - wszystkie encje skonfigurowane
- [x] **Value Objects** - TenantId, Money, Email, BillingPeriod
- [x] **Database Schema** - struktura multi-tenant
- [x] **Identity Integration** - ApplicationUser/ApplicationRole z TenantId
- [x] **AccountController** - rejestracja PlatformAdmin i logowanie
- [x] **JWT Claims** - TenantId i Role w tokenach JWT
- [x] **CreateProviderCommand** - CQRS command do tworzenia providerów
- [x] **Tenant Middleware** - automatyczne wykrywanie tenanta z requestu
- [x] **Tenant Context Service** - zarządzanie kontekstem tenanta
- [x] **Repository Pattern** - implementacja UnitOfWork z repozytoriami
- [x] **🔒 Bezpieczny Model Uwierzytelniania** - refaktoryzacja systemu bezpieczeństwa
- [x] **AdminSetupService** - bezpieczna rejestracja administratora z kontrolą środowiska
- [x] **RegisterProviderCommand** - CQRS command do rejestracji providerów
- [x] **Bezpieczne Endpointy** - usunięcie publicznego dostępu do tworzenia administratorów

### 🔄 W Trakcie

- [ ] Testy jednostkowe
- [ ] Testy integracyjne
- [ ] **Dodatkowe Commands/Queries** - rozszerzenie CQRS pattern
- [ ] **Provider Management** - pełne zarządzanie providerami

### 📅 Planowane

- [ ] Frontend aplikacja (React/Next.js)
- [ ] Docker containerization
- [ ] CI/CD pipeline
- [ ] Monitoring i alerting
- [ ] Caching (Redis)
- [ ] Message Queue (RabbitMQ/Azure Service Bus)

## 🤝 Współpraca

### Konwencje Kodowania

- **C# Coding Conventions** - Microsoft standards
- **Clean Code** principles
- **SOLID** principles
- **DRY** principle

### Git Workflow

- **Feature branches** dla nowych funkcjonalności
- **Pull Requests** z code review
- **Conventional Commits** dla historii

## 📞 Wsparcie

W przypadku problemów lub pytań:

1. Sprawdź logi w folderze `logs/`
2. Sprawdź Health Check endpoint
3. Sprawdź dokumentację Swagger
4. Utwórz issue w repozytorium

## 🎉 Podsumowanie Implementacji

### ✅ Zrealizowane Funkcjonalności

1. **🔒 Bezpieczna Rejestracja PlatformAdmin** - jednorazowa konfiguracja administratora z kontrolą środowiska
2. **JWT Claims** - rozszerzone tokeny JWT o TenantId i Role dla multi-tenancy
3. **CreateProviderCommand** - komenda CQRS do tworzenia providerów z automatycznym przypisaniem TenantId
4. **RegisterProviderCommand** - komenda CQRS do rejestracji providerów z automatycznym przypisaniem roli
5. **Tenant Middleware** - middleware automatycznie wykrywający kontekst tenanta z JWT, headers i query parameters
6. **Repository Pattern** - implementacja UnitOfWork z generycznymi repozytoriami
7. **Tenant Context Service** - serwis do zarządzania kontekstem tenanta w aplikacji
8. **AdminSetupService** - serwis do bezpiecznego zarządzania początkową konfiguracją administratora

### 🔧 Architektura

- **Clean Architecture** z podziałem na warstwy
- **CQRS** z MediatR dla separacji komend i zapytań
- **Multi-Tenancy** z automatyczną izolacją danych
- **JWT Authentication** z rozszerzonymi claims
- **Repository Pattern** dla abstrakcji dostępu do danych
- **🔒 Bezpieczny Model Uwierzytelniania** z kontrolą dostępu i jednorazową konfiguracją

### 🚀 Następne Kroki

1. **Testy jednostkowe** dla nowych komponentów
2. **Testy integracyjne** dla endpointów API
3. **Rozszerzenie CQRS** o dodatkowe komendy i zapytania
4. **Frontend aplikacja** w React/Next.js
5. **Docker containerization** i CI/CD pipeline

## 🔒 Bezpieczeństwo Aplikacji

### 🛡️ Implementowane Zabezpieczenia

#### 1. Bezpieczna Rejestracja Administratora

- **Jednorazowa konfiguracja** - administrator może być utworzony tylko przy pierwszym uruchomieniu
- **Kontrola środowiska** - setup dostępny tylko w Development lub gdy włączony w konfiguracji
- **Automatyczna blokada** - endpoint setup jest blokowany po utworzeniu pierwszego administratora

#### 2. Kontrola Dostępu

- **Role-based Authorization** - system ról z kontrolą dostępu do zasobów
- **JWT Authentication** - bezpieczne tokeny z claims dla multi-tenancy
- **Tenant Isolation** - automatyczna izolacja danych między tenantami

#### 3. Walidacja i Sanityzacja

- **FluentValidation** - walidacja wszystkich żądań
- **Input Sanitization** - sanityzacja danych wejściowych
- **SQL Injection Protection** - Entity Framework Core z parametrami

#### 4. Logowanie i Monitorowanie

- **Structured Logging** - szczegółowe logi z kontekstem tenanta
- **Performance Monitoring** - monitorowanie wydajności operacji
- **Error Tracking** - śledzenie błędów z kontekstem

### 🚨 Zalecenia Bezpieczeństwa

1. **Zmiana klucza JWT** - użyj silnego, unikalnego klucza w produkcji
2. **HTTPS** - wymuś HTTPS we wszystkich środowiskach
3. **Rate Limiting** - implementuj ograniczenia częstotliwości żądań
4. **Audit Logging** - dodaj logi audytu dla operacji administracyjnych
5. **Backup Strategy** - regularne kopie zapasowe bazy danych

---

**Orbito** - Nowoczesna platforma SaaS dla zarządzania subskrypcjami i płatnościami z zaawansowanymi zabezpieczeniami.
