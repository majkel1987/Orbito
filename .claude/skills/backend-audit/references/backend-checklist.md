# Backend Audit Checklist

Heurystyki i red flags do manualnego code review per phase.

---

## 1. Domain Layer

### 1.1 Entities

**Dobre praktyki:**
- Private setters na properties
- Constructor invariants (walidacja w konstruktorze)
- Business logic w entity, nie w handlers
- Navigation properties jako `IReadOnlyCollection<T>`
- Domain events collection (`List<IDomainEvent>`)
- Brak infra concerns (EF/JSON attributes)
- `IMustHaveTenant` interface dla multi-tenancy
- Equality by ID

**Red flags:**
- ❌ Public setters: `public string Name { get; set; }`
- ❌ Business rules w handlers zamiast w entity
- ❌ Entity z 20+ properties (rozbij na aggregate)
- ❌ Entity bez metod = anemic model
- ❌ `[JsonProperty]` / `[Column]` na domain entity
- ❌ `ILogger` w entity
- ❌ Referencje do DbContext

**Thresholds:**
- Max 15 properties per entity
- Max 10 public methods per entity

### 1.2 Value Objects

**Dobre praktyki:**
- Immutability (`init` / `get` only)
- Structural equality (`Equals` / `GetHashCode`)
- Self-validation w constructor
- Brak entity references (only primitives or other VOs)

**Red flags:**
- ❌ `string Email` zamiast `Email` value object
- ❌ Mutable properties
- ❌ Value Object z `Id` property (to entity!)
- ❌ Brak operator overloading dla equality

### 1.3 Enums & Constants

**Dobre praktyki:**
- Explicit int values: `Active = 1, Inactive = 2`
- Brak magic strings/numbers w kodzie
- PascalCase naming
- `[Flags]` gdzie appropriate

**Red flags:**
- ❌ `Status = 1, 2, 3` bez nazw (nieczytelne)
- ❌ Stringi `"active"` / `"pending"` w kodzie zamiast enum
- ❌ Enum z 15+ values (prawdopodobnie potrzebna tabela w DB)
- ❌ Inconsistent naming (mix camelCase/PascalCase)

### 1.4 Domain Events & Errors

**Dobre praktyki:**
- Past tense naming: `PaymentCompleted`, `ClientCreated`
- Tylko IDs + essential data (nie całe entities)
- Immutable (records)
- Error codes + messages
- Pełny error catalog per aggregate

**Red flags:**
- ❌ Events z navigation properties
- ❌ Generic `Exception` zamiast domain-specific errors
- ❌ Brakujące error types dla common failures
- ❌ Future tense naming: `PaymentComplete` (błędne!)

### 1.5 Domain Interfaces

**Dobre praktyki:**
- Tylko repository interfaces w Domain
- ISP compliance (max 5 metod per interface)
- Zwracają entities, nie DTOs
- Zero infra types (IQueryable OK, DbContext NIE)

**Red flags:**
- ❌ Service interfaces w Domain layer
- ❌ God interface z 15+ metod
- ❌ Interface zwraca DTO zamiast entity
- ❌ `DbContext` / `HttpClient` w domain interfaces

---

## 2. Application Layer

### 2.1-2.5 Handlers (Commands & Queries)

**Dobre praktyki:**
- Jeden handler = jedna operacja
- Handler nie woła innych handlers (composition via services)
- FluentValidation PRZED logiką
- `CancellationToken` propagowany wszędzie
- `Result<T>` zamiast exceptions
- Tenant filtering na każdym query
- Idempotency na payment operations

**Red flags:**
- ❌ Handler >100 linii
- ❌ Handler z >5 dependencies (zbyt wiele odpowiedzialności)
- ❌ Brak validation przed logiką
- ❌ Catch-all exception: `catch (Exception ex)`
- ❌ Brak `CancellationToken` w async methods
- ❌ Direct DB access zamiast przez repository

**Thresholds:**
- Max 80 linii per handler
- Max 5 injected dependencies
- 100% CancellationToken propagation

### 2.6 Interfaces

**Dobre praktyki:**
- ISP: max 5 metod per interface
- Async methods z `CancellationToken`
- Result types zamiast void + exceptions
- Clear naming: `IPaymentProcessor`, `IEmailSender`

**Red flags:**
- ❌ `IPaymentService` z 15 metod (god interface)
- ❌ Sync methods w async-heavy interface
- ❌ `void` return z exception throwing
- ❌ Interface z implementation details w nazwie

### 2.7 Validators

**Dobre praktyki:**
- Każdy Command ma Validator
- Input validation w validator (format, required, length)
- Business rule validation w handler, NIE w validator
- Custom validators wyextractowane
- User-friendly error messages

**Red flags:**
- ❌ Command bez walidatora
- ❌ Walidator z business logic (np. "czy user może to zrobić")
- ❌ Generic error messages: "Invalid input"
- ❌ Duplicate validation (validator + handler)

### 2.8 Application Services

**Dobre praktyki:**
- SRP (Single Responsibility)
- Brak direct DB access (przez repos)
- Proper DI (constructor injection)
- Async all the way

**Red flags:**
- ❌ Service z `DbContext` injection
- ❌ Static methods
- ❌ Service doing 5+ unrelated things

### 2.9 Background Jobs

**Dobre praktyki:**
- Retry logic
- Error handling z logging
- Idempotency (safe to re-run)
- Tenant context set before data access
- Graceful cancellation support

**Red flags:**
- ❌ Job bez error handling
- ❌ Job bez retry logic
- ❌ Job accessing data without tenant context
- ❌ Non-idempotent operations

### 2.10 DTOs & Models

**Dobre praktyki:**
- No business logic w DTOs
- Flat structure (avoid deep nesting)
- Proper mapping (AutoMapper lub manual)
- Brak sensitive data w response DTOs

**Red flags:**
- ❌ DTO z methods/validation
- ❌ Password/token w response DTO
- ❌ Deep nesting: `dto.Client.Provider.Plan.Features[0].Name`

### 2.11 Behaviours & Pipeline

**Dobre praktyki:**
- Logging behaviour
- Validation behaviour
- Transaction behaviour
- Correct order: Logging → Validation → Transaction → Handler

**Red flags:**
- ❌ Missing validation in pipeline
- ❌ Wrong behaviour order
- ❌ Exception swallowing in behaviours

---

## 3. Infrastructure Layer

### 3.1 DbContext & EF Config

**Dobre praktyki:**
- Multi-tenant query filters
- SaveChanges auditing (CreatedAt, UpdatedAt)
- Connection resiliency
- Fluent API > attributes
- Proper entity configurations in separate files

**Red flags:**
- ❌ Brak global query filter dla TenantId
- ❌ Data annotations na entities zamiast Fluent API
- ❌ Missing indexes na filtered columns
- ❌ Missing cascade delete configuration

### 3.2 Repositories

**Dobre praktyki:**
- Implementują domain interfaces
- Tenant filtering w KAŻDYM query
- `AsNoTracking()` na reads
- Proper pagination
- Unit of Work pattern

**Red flags:**
- ❌ Query bez TenantId filter — CRITICAL!
- ❌ `ToList()` bez pagination
- ❌ Missing `AsNoTracking()` na reads
- ❌ N+1 queries (Include missing)

### 3.3 Stripe Integration

**Dobre praktyki:**
- Webhook signature verification
- Idempotency keys na operations
- Proper error mapping to domain errors
- Retry logic z exponential backoff
- No secrets in code

**Red flags:**
- ❌ Missing webhook signature check — CRITICAL!
- ❌ Secrets w appsettings.json
- ❌ No idempotency keys
- ❌ Raw Stripe exceptions exposed to API

### 3.4 Infrastructure Services

**Dobre praktyki:**
- Proper abstractions
- No leaked implementation details
- Error handling
- Timeout configuration
- Circuit breaker for external APIs

**Red flags:**
- ❌ `new HttpClient()` — use IHttpClientFactory
- ❌ No timeout configuration
- ❌ Missing circuit breaker

### 3.5 Background Jobs (Infra)

**Dobre praktyki:**
- Hangfire/job configuration
- Retry policies
- Concurrent execution prevention
- Dead letter handling

**Red flags:**
- ❌ Concurrent execution allowed for non-idempotent jobs
- ❌ No retry policy
- ❌ Missing dead letter handling

### 3.6 DI Registration

**Dobre praktyki:**
- All services registered
- Correct lifetimes:
  - `Scoped` for tenant-context services
  - `Singleton` for stateless services
  - `Transient` for lightweight factories
- No missing registrations

**Red flags:**
- ❌ Missing service registration (runtime error)
- ❌ Singleton service injecting Scoped (captive dependency)
- ❌ DbContext as Singleton

---

## 4. API Layer

### 4.1-4.2 Controllers

**Dobre praktyki:**
- Thin controllers (delegate to MediatR)
- Proper HTTP status codes
- `[Authorize]` on all mutations
- Model binding validation
- Proper route naming
- Consistent response types

**Red flags:**
- ❌ Business logic w controller
- ❌ Missing `[Authorize]` — CRITICAL!
- ❌ Inconsistent response types
- ❌ Controller >100 linii

### 4.3 Middleware

**Dobre praktyki:**
- Error handling middleware (global)
- Tenant resolution middleware
- Idempotency middleware
- Request logging middleware
- **ORDER MATTERS**: Exception → Logging → Tenant → Auth → ...

**Red flags:**
- ❌ Wrong middleware order
- ❌ Exception swallowing
- ❌ Missing global error handler

### 4.4 Program.cs & Config

**Dobre praktyki:**
- Proper middleware order
- CORS config
- Rate limiting
- HTTPS enforcement
- Health checks registered

**Red flags:**
- ❌ Secrets in appsettings.json — use User Secrets
- ❌ CORS `AllowAnyOrigin()` w production
- ❌ No rate limiting
- ❌ HTTP allowed (no HTTPS redirect)

### 4.5 Health Checks

**Dobre praktyki:**
- DB connectivity check
- Stripe API check
- External dependencies check
- Background job health

**Red flags:**
- ❌ Missing health checks
- ❌ Health check exposes sensitive info

---

## 5. Cross-Cutting Concerns

### 5.1 Multi-Tenancy — CRITICAL!

**Dobre praktyki:**
- KAŻDY query filtruje po TenantId
- Global query filters w DbContext
- Tenant z auth claims, NIGDY z request body
- Cache keys zawierają tenant
- Background jobs ustawiają tenant context

**Red flags:**
- ❌ Query bez TenantId filter — CRITICAL!
- ❌ TenantId z `[FromBody]` / `[FromQuery]`
- ❌ Cache key bez TenantId
- ❌ Background job bez tenant context

### 5.2 Error Handling

**Dobre praktyki:**
- `Result<T>` pattern wszędzie
- No naked exceptions
- Proper error codes
- GlobalExceptionHandler jako last resort

**Red flags:**
- ❌ `throw new Exception("...")`
- ❌ Empty catch blocks
- ❌ Stack trace exposed to client

### 5.3 Logging & Observability

**Dobre praktyki:**
- Structured logging (Serilog)
- Correlation IDs
- No sensitive data (PII, tokens, passwords)
- Proper log levels

**Red flags:**
- ❌ `Console.WriteLine()` zamiast ILogger
- ❌ Password/token w logach
- ❌ Missing correlation ID
- ❌ Everything logged as Error

### 5.4 Security Hardening

**Dobre praktyki:**
- JWT validation configured
- CORS properly configured
- Rate limiting on auth endpoints
- Secrets w User Secrets / env vars
- HTTPS enforced
- Security headers (CSP, X-Frame-Options, etc.)

**Red flags:**
- ❌ Weak JWT settings (no expiry, weak key)
- ❌ CORS `*` w production
- ❌ Secrets w source code
- ❌ Missing security headers

### 5.5 Performance Patterns

**See: `scripts/performance_scan.py` for automated checks**

**Additional manual checks:**
- Proper caching strategy
- DB indexes on filtered/sorted columns
- No N+1 queries
- Pagination everywhere
- Async all the way

---
