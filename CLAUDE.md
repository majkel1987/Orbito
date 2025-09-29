# CLAUDE.md - Zasady Pracy dla Orbito Platform

## 🏗️ Architektura i Wzorce

### Clean Architecture

- **Warstwy**: API → Application → Domain ← Infrastructure
- **Zależności**: Tylko w kierunku Domain (centrum)
- **Izolacja**: Domain nie zna Infrastructure ani API

### Domain-Driven Design (DDD)

- **Encje**: Rich domain models z business logic
- **Value Objects**: Immutable types (`TenantId`, `Money`, `Email`)
- **Domain Services**: Złożona logika biznesowa
- **Aggregates**: Consistency boundaries

### CQRS + MediatR

- **Commands**: Modyfikacja stanu (Create, Update, Delete)
- **Queries**: Odczyt danych (Get, Search, List)
- **Handlers**: Jedna odpowiedzialność per handler
- **Pipeline Behaviors**: Logging, Validation, Performance

## 🛠️ Standardy Kodowania

### C# 13 / .NET 9

- **Nullable Reference Types**: Zawsze włączone
- **File Scoped Namespaces**: `namespace Orbito.Application.Features;`
- **Primary Constructors**: Dla prostych klas
- **Records**: Dla immutable data transfer objects

### Konwencje Nazewnictwa

- **Klasy**: PascalCase (`UserService`, `CreateProviderCommand`)
- **Metody**: PascalCase (`CreateAsync`, `GetByIdAsync`)
- **Właściwości**: PascalCase (`TenantId`, `Email`)
- **Pola**: camelCase z underscore (`_repository`, `_logger`)
- **Zmienne lokalne**: camelCase (`clientId`, `isActive`)

### Struktura Plików

```
Orbito.Application/
├── Features/
│   ├── Providers/
│   │   ├── Commands/
│   │   │   ├── CreateProviderCommand.cs
│   │   │   └── CreateProviderCommandHandler.cs
│   │   ├── Queries/
│   │   │   ├── GetProviderByIdQuery.cs
│   │   │   └── GetProviderByIdQueryHandler.cs
│   │   └── Validators/
│   │       └── CreateProviderCommandValidator.cs
│   └── Clients/
└── Services/
```

## 🔒 Multi-Tenancy

### Tenant Context

- **TenantId**: Wymagany w każdej operacji
- **Automatyczne filtrowanie**: Query filters w EF Core
- **Izolacja**: Każdy tenant widzi tylko swoje dane
- **JWT Claims**: `tenant_id` w tokenach

### Implementacja

```csharp
// Zawsze sprawdzaj TenantId w handlerach
var provider = await _repository.GetByIdAsync(request.Id, cancellationToken);
if (provider.TenantId != _tenantContext.TenantId)
    throw new UnauthorizedAccessException();

// Query filters automatyczne w DbContext
builder.Entity<Provider>()
    .HasQueryFilter(p => p.TenantId.Value == currentTenantId);
```

## 🧪 Testowanie

### Pokrycie Testami

- **Testy jednostkowe**: Minimum 95% coverage
- **Testy integracyjne**: Kluczowe scenariusze end-to-end
- **xUnit + FluentAssertions + Moq**: Standard stack

### Konwencje Testów

```csharp
[Trait("Category", "Unit")]
public class CreateProviderCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateProvider()
    {
        // Arrange
        var command = new CreateProviderCommand { /* ... */ };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }
}
```

### Uruchomienie Testów

```bash
# Wszystkie testy
dotnet test

# Tylko testy jednostkowe
dotnet test --filter "Category=Unit"

# Tylko testy integracyjne
dotnet test --filter "Category=Integration"

# Z pokryciem kodu
dotnet test --collect:"XPlat Code Coverage"
```

## 🔐 Bezpieczeństwo

### Uwierzytelnianie

- **JWT Bearer**: Tokeny z claims
- **ASP.NET Core Identity**: Zarządzanie użytkownikami
- **Role-based**: PlatformAdmin, Provider, Client

### Autoryzacja

- **Controller level**: `[Authorize(Roles = "Provider")]`
- **Business logic**: Sprawdzanie TenantId
- **Data access**: Query filters

### Walidacja

- **FluentValidation**: Wszystkie commands/queries
- **Domain validation**: Rich domain models
- **Input sanitization**: Zawsze waliduj input

## 📊 Logowanie i Monitorowanie

### Serilog

- **Structured logging**: JSON format
- **File sinks**: `logs/app-.log`
- **Performance logging**: Pipeline behaviors

### Health Checks

- **Database**: EF Core health check
- **External services**: Custom health checks
- **UI**: `/healthchecks-ui`

### Performance

- **Pipeline behaviors**: Automatyczne logowanie > 3s
- **Database queries**: Monitoring N+1 problems
- **Caching**: Redis dla often-used data

## 💾 Baza Danych

### Entity Framework Core 9

- **Code First**: Migrations
- **Query filters**: Multi-tenancy
- **Value converters**: Domain objects → DB

### Migracje

```bash
# Dodanie migracji
dotnet ef migrations add MigrationName --project Orbito.Infrastructure --startup-project Orbito.API

# Aktualizacja bazy
dotnet ef database update --project Orbito.Infrastructure --startup-project Orbito.API
```

### Konwencje

- **Table names**: PascalCase (`Providers`, `Clients`)
- **Column names**: snake_case (`tenant_id`, `created_at`)
- **Foreign keys**: `{Entity}Id` (`ProviderId`, `ClientId`)

## 🚀 Deployment

### Konfiguracja

- **appsettings.json**: Base configuration
- **appsettings.Development.json**: Development overrides
- **Environment variables**: Production secrets

### Connection Strings

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=Orbito_dev;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=true"
  }
}
```

### Build & Run

```bash
# Build solution
dotnet build

# Run API
dotnet run --project Orbito.API

# Watch mode (development)
dotnet watch --project Orbito.API
```

## 📈 Error Handling

### Result Pattern

```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public string Error { get; }

    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string error) => new(false, default, error);
}
```

### Exception Handling

- **Global exception handler**: Middleware
- **Domain exceptions**: Custom exception types
- **Validation errors**: FluentValidation integration

## 🔄 Background Jobs

### Hangfire (Future)

- **Recurring jobs**: Subscription checks
- **Payment processing**: Retry logic
- **Email notifications**: Async processing

## 📝 Dokumentacja

### XML Comments

```csharp
/// <summary>
/// Creates a new provider with the specified details.
/// </summary>
/// <param name="command">The command containing provider details.</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>The created provider result.</returns>
public async Task<Result<ProviderDto>> Handle(CreateProviderCommand command, CancellationToken cancellationToken)
```

### Swagger/OpenAPI

- **Automatic generation**: From controllers
- **Authentication**: JWT bearer setup
- **Examples**: Request/response samples

## 🎯 Najlepsze Praktyki

### SOLID Principles

- **Single Responsibility**: Jedna klasa = jedna odpowiedzialność
- **Open/Closed**: Rozszerzalność bez modyfikacji
- **Liskov Substitution**: Podklasy zastępowalne
- **Interface Segregation**: Małe, skupione interfejsy
- **Dependency Inversion**: Zależności od abstrakcji

### Clean Code

- **Meaningful names**: Opisowe nazwy zmiennych/metod
- **Small functions**: Max 20-30 linii
- **No deep nesting**: Early returns, guard clauses
- **No magic numbers**: Named constants

### Repository Pattern

```csharp
public interface IProviderRepository : IRepository<Provider>
{
    Task<Provider?> GetBySubdomainAsync(string subdomain, CancellationToken cancellationToken);
    Task<bool> IsSubdomainTakenAsync(string subdomain, CancellationToken cancellationToken);
}
```

## 🚨 Do Unikania

### Anti-Patterns

- **God objects**: Klasy robiące za dużo
- **Anemic domain model**: Bez business logic
- **Primitive obsession**: String wszędzie zamiast Value Objects
- **Feature envy**: Klasa używa więcej metod z innej klasy

### Performance Anti-Patterns

- **N+1 queries**: Include related data
- **Large result sets**: Pagination
- **Blocking calls**: Async/await everywhere
- **Memory leaks**: Dispose resources

### Security Anti-Patterns

- **SQL injection**: Always use parameters
- **Hardcoded secrets**: Use configuration
- **Missing authorization**: Check permissions
- **Information disclosure**: Sanitize error messages

## 📋 Code Review Checklist

### Funkcjonalność

- [ ] Kod kompiluje się bez ostrzeżeń
- [ ] Wszystkie testy przechodzą
- [ ] Business logic jest poprawna
- [ ] Error handling jest kompletny

### Jakość

- [ ] Kod jest czytelny i zrozumiały
- [ ] Nazwy są opisowe
- [ ] Brak duplikacji kodu
- [ ] Zastosowano właściwe wzorce

### Bezpieczeństwo

- [ ] Input jest walidowany
- [ ] Authorization jest sprawdzana
- [ ] Secrets nie są hardcoded
- [ ] SQL injection prevented

### Performance

- [ ] Zapytania są optymalne
- [ ] Nie ma N+1 problems
- [ ] Memory usage jest rozsądny
- [ ] Caching gdzie potrzebny

---

**Wersja**: 1.0
**Ostatnia aktualizacja**: 2025-09-28
**Autor**: IT Architect Team
