# Backend Audit Checklist — Detailed Heuristics & Thresholds

Reference document for the backend-audit skill. Read ONLY the section relevant to your current phase.

## Table of Contents

1. [Domain Layer (Phase 1)](#1-domain-layer)
2. [Application Layer (Phase 2)](#2-application-layer)
3. [Infrastructure Layer (Phase 3)](#3-infrastructure-layer)
4. [API Layer (Phase 4)](#4-api-layer)
5. [Multi-Tenancy Security (Phase 5.1)](#51-multi-tenancy-security)
6. [Error Handling & Result Pattern (Phase 5.2)](#52-error-handling--result-pattern)
7. [Logging & Observability (Phase 5.3)](#53-logging--observability)
8. [Security Hardening (Phase 5.4)](#54-security-hardening)
9. [Performance Patterns (Phase 5.5)](#55-performance-patterns)
10. [Test Quality (Phase 6)](#6-test-quality)

---

## 1. Domain Layer

### 1.1 Entities

**What to check:**
- Entities have private setters — no public `set` on domain properties
- Constructors enforce invariants (no entity can be created in invalid state)
- Business logic lives IN the entity, not in handlers/services
- Navigation properties are `IReadOnlyCollection<T>`, not `List<T>` or `ICollection<T>`
- Entity has domain events collection if it raises events
- No infrastructure concerns (EF, JSON attributes) leak into domain entities
- `IMustHaveTenant` or equivalent is implemented where needed
- Proper equality — entities compare by ID, not by value

**Red flags:**
- `public string Name { get; set; }` — should be `{ get; private set; }` or `init`
- Business rules in handlers like `if (entity.Status == Active)` instead of `entity.CanActivate()`
- Entity with 20+ properties — may need splitting or value objects
- Entity without any methods — anemic domain model
- `[JsonProperty]` or `[Column]` attributes on domain entities

**Thresholds:**
- Max entity properties: 15 (flag if > 15, suggest value objects)
- Max entity methods: 10 (flag if > 10, suggest decomposition)
- Zero tolerance: public setters on invariant-protected properties

### 1.2 Value Objects

**What to check:**
- Immutability — all properties are `init` or `{ get; }` with constructor-only setting
- Structural equality — override `Equals`, `GetHashCode`, or inherit from `ValueObject` base
- Self-validation in constructor — `Money(-5, "USD")` should throw
- No entity references inside value objects
- Used where appropriate: Email, Money, TenantId, IdempotencyKey

**Red flags:**
- `string email` used directly instead of `Email` value object
- Mutable properties on value objects
- Value object with an `Id` property (that's an entity)

### 1.3 Enums & Constants

**What to check:**
- Enums have explicit integer values (for DB stability)
- No magic strings/numbers — all constants are in `Constants/` or enum
- Enum names are PascalCase, values are meaningful
- Consider: should this enum be a smart enum or a value object?

**Red flags:**
- `Status = 1, 2, 3` without named values
- Strings like `"active"`, `"pending"` scattered in code instead of enum
- Enum with 15+ values — may need decomposition

### 1.4 Domain Events & Errors

**What to check:**
- Events are past tense: `PaymentCompleted`, not `CompletePayment`
- Events carry only IDs and essential data, not full entities
- Events are immutable (record types preferred)
- Domain errors have unique codes and descriptive messages
- Error catalog covers all business rule violations

**Red flags:**
- Events carrying navigation properties or full entity graphs
- Generic `throw new Exception("Something went wrong")`
- Missing error types for known failure scenarios

### 1.5 Domain Interfaces

**What to check:**
- Only repository interfaces live in Domain (not service interfaces)
- Interfaces follow ISP — no god interfaces
- Repository interfaces return domain entities, not DTOs
- No infrastructure types (DbContext, HttpClient) referenced

---

## 2. Application Layer

### 2.1-2.5 Command/Query Handlers

**What to check:**
- One handler per command/query (SRP)
- Handler only orchestrates — delegates business logic to entities/domain services
- Validation is in separate `Validator` class, not in handler
- Handler uses `Result<T>` for expected failures, exceptions only for unexpected
- CancellationToken is accepted and passed through all async calls
- Proper unit of work — `SaveChangesAsync` called once at the end
- No direct infrastructure calls (no `DbContext`, no `HttpClient`)
- Idempotency considered for mutation commands

**Red flags:**
- Handler > 80 lines — too much logic, needs decomposition
- `try/catch` wrapping the entire handler — should use Result pattern
- Multiple `SaveChangesAsync` calls in one handler (transaction boundary issue)
- Handler calling another handler directly (use MediatR pipeline instead)
- Business validation inside handler instead of FluentValidation validator
- Missing `CancellationToken` parameter
- `_ = cancellationToken;` (accepting but ignoring it)

**Thresholds:**
- Handler line count: < 60 ideal, flag > 80
- Dependencies injected: < 5, flag > 7 (SRP violation)

### 2.6 Interfaces

**What to check:**
- No interface has > 7 methods (ISP violation)
- No `I<EntityName>Service` interfaces with 15+ methods — split by use case
- All interfaces have at least one implementation
- Interfaces defined in Application, implemented in Infrastructure
- No circular references between interfaces

**Red flags:**
- `IPaymentService` with methods for create, process, refund, retry, reconcile, report, export (god interface)
- Interface with only 1 method used by only 1 class (over-abstraction)
- `IRepository<T>` with 20+ methods

### 2.7 Validators

**What to check:**
- Every command has a corresponding validator
- Validators use `.NotEmpty()`, `.MaximumLength()`, `.Must()` appropriately
- Error messages are user-friendly (not developer-speak)
- Nested object validation with `.SetValidator()`
- No business rule validation (that's handler/entity responsibility)

### 2.8 Application Services

**What to check:**
- Services don't duplicate entity logic
- Services are stateless
- No DbContext direct access (use repositories)
- Clear responsibility — not a dumping ground

### 2.9 Background Jobs

**What to check:**
- Jobs are idempotent — safe to retry
- Jobs have proper error handling and logging
- Jobs use `CancellationToken`
- Jobs don't hold long transactions
- Tenant context is set correctly in job scope
- Dead letter / failure handling exists

**Red flags:**
- Job that processes all tenants in one transaction
- No try/catch at job level
- Job without structured logging
- Job accessing data without tenant scoping

### 2.10 DTOs & Models

**What to check:**
- DTOs are records (immutable)
- No domain logic in DTOs
- Result<T> used consistently
- PaginatedList/PaginationParams standardized
- No sensitive data exposed (passwords, keys, internal IDs)

### 2.11 Behaviours & Pipeline

**What to check:**
- ValidationBehaviour runs before handler
- LoggingBehaviour captures timing and errors
- PerformanceBehaviour flags slow operations
- Pipeline order is correct in DI registration

---

## 3. Infrastructure Layer

### 3.1 DbContext & EF Core Configuration

**What to check:**
- Global query filter for TenantId: `.HasQueryFilter(e => e.TenantId == _tenantId)`
- Entity configurations in separate `IEntityTypeConfiguration<T>` classes
- Indexes on frequently queried columns (TenantId, Status, CreatedAt, Email)
- Composite indexes where appropriate
- Proper cascade delete configuration
- `HasMaxLength()` on all string properties
- Decimal precision configured: `.HasPrecision(18, 2)` for money
- Soft delete filter if used
- Concurrency tokens on critical entities (`[ConcurrencyCheck]` or `RowVersion`)
- No lazy loading (explicit `.Include()` only)

**Red flags:**
- Missing index on TenantId (every query will be slow)
- `nvarchar(max)` on fields that should be bounded
- No `ON DELETE` configuration (relying on EF defaults)
- `decimal` without precision (defaults to 18,2 in SQL Server but should be explicit)

### 3.2 Repositories

**What to check:**
- Every repository filters by TenantId (no data leaks)
- `ForClientAsync` methods exist for client-owned data
- `AsNoTracking()` used for read-only queries
- Proper `Include()` — not eager-loading entire graph
- Pagination implemented at DB level, not in memory
- No raw SQL without parameterization

**Red flags:**
- `GetAll()` without pagination — will crash at scale
- Missing `.Where(x => x.TenantId == tenantId)` on ANY query
- `.ToListAsync()` followed by `.Where()` (loading all then filtering in memory)
- Duplicate repository files (e.g., `PaymentRepository.cs` + `PaymentRepository.cs.bak`)

### 3.3 Stripe Integration

**What to check:**
- Webhook signature verification (StripeSignatureVerificationMiddleware)
- Idempotency keys on all mutation calls
- PaymentIntent metadata includes tenantId, clientId, subscriptionId
- Amounts converted correctly (Stripe uses cents/minor units)
- Error handling for Stripe API failures (retry with backoff)
- No API keys hardcoded (must be from configuration)
- Webhook events logged with status
- Duplicate event handling (idempotent webhook processing)

**Red flags:**
- `new StripeClient("sk_live_...")` — hardcoded key
- Missing webhook signature check
- `amount = price` instead of `amount = price * 100` (Stripe uses cents)
- No retry logic on Stripe API calls
- Webhook handler that doesn't check for duplicate events

### 3.4 Services

**What to check:**
- EmailSender has proper error handling (email is fire-and-forget)
- CacheService has TTL configuration
- TransactionService wraps multiple operations correctly
- UserContextService correctly extracts claims from JWT

### 3.5 Background Jobs (Infra)

**What to check:**
- Jobs registered with proper schedule
- Cron expressions are correct
- Job concurrency is handled (no duplicate runs)

### 3.6 DI Registration

**What to check:**
- Scoped vs Singleton vs Transient lifetimes are correct
- DbContext is Scoped (never Singleton)
- HttpClient uses IHttpClientFactory (not `new HttpClient()`)
- All interfaces have registered implementations
- No missing registrations (would throw at runtime)

---

## 4. API Layer

### 4.1-4.2 Controllers

**What to check:**
- Controllers are thin — only MediatR.Send() + return status code
- Proper HTTP status codes (201 Created, 204 NoContent, 400, 404, 409)
- `[Authorize]` attribute on all protected endpoints
- `[ProducesResponseType]` for Swagger documentation
- Route naming follows REST conventions
- No business logic in controllers
- Model binding uses `[FromBody]`, `[FromRoute]`, `[FromQuery]` explicitly
- File size limits on upload endpoints
- Rate limiting on public endpoints

**Red flags:**
- Controller > 50 lines per action (too much logic)
- `try/catch` in controller (should be in middleware)
- Missing `[Authorize]` on mutation endpoints
- Returning 200 for everything (incorrect status codes)
- Raw string concatenation in routes

### 4.3 Middleware

**What to check:**
- GlobalExceptionHandler catches and logs all unhandled exceptions
- Error response format is consistent (ErrorResponse DTO)
- No stack traces in production error responses
- Idempotency middleware checks header and cache
- Tenant middleware sets context before handlers run
- Middleware order is correct in pipeline

### 4.4 Program.cs

**What to check:**
- CORS configuration is restrictive (not `AllowAnyOrigin` in production)
- Authentication configured before Authorization
- HTTPS enforcement
- Swagger disabled in production
- Rate limiting configured
- Response compression enabled
- Health checks registered

---

## 5.1 Multi-Tenancy Security

**CRITICAL audit — run this early.**

**What to check:**
- EVERY repository query includes TenantId filter
- Global query filter on DbContext for all tenant entities
- TenantId comes from authenticated user claims, never from request body
- No endpoint allows specifying someone else's TenantId
- Background jobs correctly set tenant context
- Cache keys include TenantId (no cross-tenant cache pollution)
- Webhook processing validates tenant ownership

**Automated scan pattern:**
```bash
# Find all repository methods that DON'T filter by tenant
grep -rn "async Task.*Repository" --include="*.cs" | grep -v "TenantId\|tenantId\|_tenantContext"
```

**Zero tolerance:** Any query that can return cross-tenant data is CRITICAL.

---

## 5.2 Error Handling & Result Pattern

**What to check:**
- `Result<T>` used for all expected failures (not found, validation, business rules)
- Exceptions used ONLY for unexpected failures (DB down, network error)
- No swallowed exceptions (`catch { }` or `catch (Exception) { }`)
- Error messages don't expose internals (connection strings, stack traces)
- Controller maps Result → HTTP status code consistently

**Red flags:**
- `throw new NotFoundException()` for missing entities (should be Result.Failure)
- Empty catch blocks
- `catch (Exception ex) { return Ok(); }` — hiding errors
- Inconsistent mix of exceptions and Result pattern

---

## 5.3 Logging & Observability

**What to check:**
- Serilog configured with structured logging
- Correlation ID on all requests
- Log levels used correctly: Info for business events, Warn for recoverable issues, Error for failures
- Sensitive data NOT logged (passwords, tokens, card numbers)
- Performance-critical operations have timing logs
- Background jobs log start/end/duration/outcome

**Red flags:**
- `_logger.LogInformation($"User {email} logged in with password {password}")`
- Missing logging in catch blocks
- Only `Console.WriteLine` (no structured logging)
- No correlation ID for request tracing

---

## 5.4 Security Hardening

**What to check:**
- JWT secret key is strong (>= 32 characters)
- Token expiration is reasonable (15-60 min access, 7-30 day refresh)
- CORS allows only specific origins (not `*`)
- Anti-forgery tokens for forms
- Input validation on all endpoints
- Rate limiting on auth endpoints
- No secrets in `appsettings.json` (use user secrets or env vars)
- HTTPS enforced
- Security headers (X-Content-Type-Options, X-Frame-Options, etc.)

---

## 5.5 Performance Patterns

**What to check:**
- No N+1 queries — use `.Include()` or projection
- `AsNoTracking()` on read-only queries
- Pagination on all list endpoints
- Caching on frequently-read, rarely-changed data
- `CancellationToken` propagated through all async chains
- No `Task.Wait()` or `.Result` (sync-over-async)
- No `ToList()` before `Where()` (in-memory filtering)
- DbContext not held open during external calls (Stripe)
- Indexes on query filter columns

**Automated scan patterns:**
```bash
# Sync-over-async
grep -rn "\.Result\b\|\.Wait()\|\.GetAwaiter().GetResult()" --include="*.cs"

# Missing CancellationToken
grep -rn "async Task" --include="*.cs" | grep -v "CancellationToken\|cancellationToken"

# In-memory filtering after ToList
grep -rn "ToListAsync\|ToList()" --include="*.cs" -A2 | grep "\.Where\|\.Select\|\.OrderBy"
```

---

## 6. Test Quality

### 6.1 Coverage Gaps

**What to check:**
- Every CommandHandler has a test file
- Every QueryHandler has a test file
- Every Validator has a test file
- Background jobs have test files
- Domain entities have behavior tests
- Integration tests cover critical paths (auth, payments, subscriptions)

### 6.2-6.3 Test Quality

**What to check:**
- Tests follow AAA pattern (Arrange, Act, Assert)
- Test names describe behavior: `Should_ReturnError_When_ClientNotFound`
- No logic in tests (no if/else, loops)
- Mocks are focused — don't mock everything
- Integration tests use `WebApplicationFactory`
- Tests are isolated — no shared state between tests
- Assert on behavior, not implementation details

**Red flags:**
- `Assert.True(result != null)` — use `Assert.NotNull`
- Tests with 0 assertions
- Tests that test the mock, not the SUT
- `Thread.Sleep()` in tests
- Tests depending on execution order
