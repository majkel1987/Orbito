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

### JWT Configuration

```json
{
  "Jwt": {
    "Key": "your-secret-key-here",
    "Issuer": "http://localhost:5211",
    "Audience": "http://localhost:5211"
  }
}
```

### Role System

- **PlatformAdmin** - Administrator platformy
- **Provider** - Dostawca usług
- **Client** - Klient

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

```csharp
// Automatyczne ustawianie kontekstu tenanta
public class TenantMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var tenantId = ExtractTenantFromRequest(context);
        _tenantContext.SetTenant(tenantId);
        await next(context);
    }
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

#### 1. Rejestracja Nowego Tenanta

```csharp
// 1. Utworzenie Provider (główny tenant)
var provider = Provider.Create(userId, businessName, subdomainSlug);

// 2. Automatyczne przypisanie TenantId
provider.TenantId = TenantId.Create(provider.Id);

// 3. Utworzenie domyślnych planów subskrypcji
var basicPlan = SubscriptionPlan.Create(provider.TenantId, "Basic", 29.99m, "PLN", BillingPeriodType.Monthly);
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

### 🔄 W Trakcie

- [ ] Implementacja CQRS Commands/Queries
- [ ] Testy jednostkowe
- [ ] Testy integracyjne
- [ ] **Tenant Middleware** - automatyczne wykrywanie tenanta z requestu
- [ ] **Tenant Context Service** - zarządzanie kontekstem tenanta

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

---

**Orbito** - Nowoczesna platforma SaaS dla zarządzania subskrypcjami i płatnościami.
