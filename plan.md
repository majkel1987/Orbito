# Plan Implementacji Orbito Platform

## <� Plan Implementacji Payment System - Faza 2

### **Etap 1: Retry Logic & Scheduling (Dni 1-2)** - ZAIMPLEMENTOWANE

#### 1.1 Domain Layer

- **PaymentRetrySchedule.cs** (Domain/Entities/)
  - Properties: Id, PaymentId, NextAttemptAt, AttemptNumber, MaxAttempts, Status, LastError
  - Value Objects: RetryStatus enum (Scheduled, InProgress, Completed, Cancelled)
  - Business rules: ValidateAttemptNumber, CanRetry, CalculateBackoff

#### 1.2 Application Layer - Services

- **IPaymentRetryService.cs + PaymentRetryService.cs** (Application/Services/)
  - `ScheduleRetryAsync(Guid paymentId, int attemptNumber, string errorReason)`
  - `ProcessScheduledRetriesAsync(CancellationToken)` - background job
  - `CalculateNextRetryTime(int attemptNumber)` - exponential backoff (5m, 15m, 1h, 6h, 24h)
  - `CancelScheduledRetriesAsync(Guid paymentId)`
  - Thread-safe z pessimistic locking

#### 1.3 Application Layer - Commands & Queries

- **Commands/RetryFailedPaymentCommand.cs** + Handler

  - Walidacja: czy payment istnieje, czy failed, czy clientId si zgadza
  - Security: TenantId + ClientId verification
  - Idempotency: sprawdzenie czy retry ju| w toku

- **Commands/BulkRetryPaymentsCommand.cs** + Handler

  - Input: List<Guid> paymentIds, Guid clientId
  - Batch processing z transaction scope
  - Rate limiting: max 50 payments naraz

- **Queries/GetScheduledRetriesQuery.cs** + Handler

  - Filtering: po ClientId, TenantId, Status
  - Pagination: max 100 records
  - DTOs: RetryScheduleDto z payment details

- **Queries/GetFailedPaymentsForRetryQuery.cs** + Handler
  - Filter: tylko Failed payments z < MaxAttempts
  - Sort: po FailedAt desc

#### 1.4 Infrastructure Layer - Repository

- **IPaymentRetryRepository.cs** + Implementation
  - `GetDueRetriesAsync(DateTime now)` - z query filter po TenantId
  - `GetByPaymentIdAsync(Guid paymentId, Guid clientId)`
  - `MarkAsProcessingAsync(Guid scheduleId)` - optimistic concurrency
  - Indexes: (PaymentId, Status), (NextAttemptAt, Status)

#### 1.5 FluentValidation Validators

- **RetryFailedPaymentCommandValidator.cs**
- **BulkRetryPaymentsCommandValidator.cs**

---

### **Etap 2: Reconciliation System**

#### 2.1 Infrastructure Layer - Models

- **Models/ReconciliationReport.cs**

  - ReportId, RunDate, TenantId, DiscrepanciesCount, AutoResolvedCount, Status
  - Statistics: TotalPayments, MatchedPayments, MismatchedPayments

- **Models/PaymentDiscrepancy.cs**
  - PaymentId, DiscrepancyType enum (StatusMismatch, AmountMismatch, Missing),
  - OrbitoStatus, StripeStatus, OrbitoAmount, StripeAmount, Resolution, ResolvedAt

#### 2.2 Infrastructure Layer - Services

- **IPaymentReconciliationService.cs + PaymentReconciliationService.cs**

  - `ReconcileWithStripeAsync(DateTime fromDate, DateTime toDate, Guid tenantId)`

    - Batch fetch z Stripe API (100 per request)
    - Parallel comparison z local DB
    - Generate discrepancy list

  - `GenerateDiscrepancyReportAsync(List<PaymentDiscrepancy> discrepancies)`

    - Group by DiscrepancyType
    - Calculate statistics
    - Save to ReconciliationReports table

  - `AutoResolveDiscrepanciesAsync(ReconciliationReport report)`
    - Auto-fix rules:
      - Status mismatch: update local if Stripe is source of truth
      - Amount mismatch: flag for manual review
      - Missing in Stripe: mark as potentially fraudulent
    - Transaction safety: SaveChangesAsync per batch

#### 2.3 Infrastructure Layer - Repository

- **IReconciliationRepository.cs** + Implementation
  - `SaveReportAsync(ReconciliationReport)`
  - `GetRecentReportsAsync(int count, Guid tenantId)`
  - `GetDiscrepanciesByReportIdAsync(Guid reportId)`
  - Indexes: (RunDate, TenantId), (ReportId, DiscrepancyType)

#### 2.4 Infrastructure Layer - Background Job

- **BackgroundJobs/DailyReconciliationJob.cs**
  - Hangfire/Quartz scheduled job: daily at 2:00 AM UTC
  - `RunReconciliationAsync()`:
    - Iterate przez wszystkie tenants
    - Run reconciliation dla last 24h
    - Generate report
  - `SendReconciliationReportAsync(ReconciliationReport)`:
    - Email notification z summary
    - Critical discrepancies � Slack/Teams webhook

---

### **Etap 3: Monitoring & Health Checks**

#### 3.1 API Layer - Health Checks

- **HealthChecks/StripeHealthCheck.cs**

  - Check: Stripe API connectivity (GET /v1/balance)
  - Timeout: 5 seconds
  - Return: Healthy/Degraded/Unhealthy

- **HealthChecks/PaymentSystemHealthCheck.cs**
  - Checks:
    - DB connection (EF Core check)
    - Failed payments ratio last 1h (> 20% � Degraded)
    - Pending retries count (> 1000 � Degraded)
    - Stripe webhook response time (> 3s � Degraded)
  - Composite health check

#### 3.2 Program.cs Registration

```csharp
builder.Services.AddHealthChecks()
    .AddCheck<StripeHealthCheck>("stripe", tags: new[] { "external" })
    .AddCheck<PaymentSystemHealthCheck>("payment_system", tags: new[] { "critical" })
    .AddDbContextCheck<ApplicationDbContext>();

app.MapHealthChecks("/health", new HealthCheckOptions {
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

---

### **Etap 4: Metrics & Statistics**

#### 4.1 Application Layer - Services

- **IPaymentMetricsService.cs + PaymentMetricsService.cs**

  - `GetPaymentSuccessRateAsync(DateRange range, Guid? providerId)`

    - SQL: `COUNT(*) FILTER(WHERE Status = 'Succeeded') / COUNT(*) * 100`
    - Group by hour/day/week

  - `GetAverageProcessingTimeAsync(DateRange range)`

    - SQL: `AVG(DATEDIFF(second, CreatedAt, CompletedAt))`
    - Filter: tylko Succeeded

  - `GetFailureReasonsBreakdownAsync(DateRange range, Guid? providerId)`

    - Group by FailureReason
    - Count + percentage

  - `GetRevenueMetricsAsync(DateRange range, Guid providerId)`
    - SUM(Amount) WHERE Status = 'Succeeded'
    - Group by Currency, SubscriptionPlan

#### 4.2 Application Layer - Queries

- **Queries/GetPaymentStatisticsQuery.cs** + Handler

  - Input: DateRange, ProviderId (optional), ClientId (optional)
  - Output: StatisticsDto (SuccessRate, TotalRevenue, AverageAmount, TopFailureReasons)
  - Security: TenantId filtering

- **Queries/GetRevenueReportQuery.cs** + Handler

  - Input: DateRange, GroupBy (Day/Week/Month), ProviderId
  - Output: List<RevenueDataPoint> (Date, Amount, Currency, TransactionCount)

- **Queries/GetPaymentTrendsQuery.cs** + Handler
  - Input: DateRange, Granularity (Hourly/Daily/Weekly)
  - Output: TrendDto (SuccessRate over time, Volume over time, Average amount trend)

---

### **Etap 5: Security & Idempotency**

#### 5.1 Domain Layer

- **ValueObjects/IdempotencyKey.cs**
  - Immutable record
  - Validation: GUID format or custom string (max 100 chars)
  - Factory method: `Create(string key)` z walidacj

#### 5.2 Domain Entity Update

- **Payment.cs** - dodaj property:
  - `IdempotencyKey? IdempotencyKey { get; init; }`

#### 5.3 API Layer - Middleware

- **Middleware/IdempotencyMiddleware.cs**
  - Intercept POST requests do `/api/payments/*`
  - Extract header: `X-Idempotency-Key`
  - Check cache (Redis): czy request ju| przetworzony
  - If exists � return cached response (200 OK)
  - If new � process + cache response (TTL: 24h)
  - Thread-safe z distributed lock (Redis)

#### 5.4 Infrastructure Layer - Cache

- **Services/IdempotencyCacheService.cs**
  - Redis implementation
  - `TryGetCachedResponseAsync(string key)`
  - `CacheResponseAsync(string key, object response, TimeSpan ttl)`

---

### **Etap 6: Database Migrations**

#### 6.1 Migration: AddPaymentRetrySchedules

```sql
CREATE TABLE PaymentRetrySchedules (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    PaymentId UNIQUEIDENTIFIER FOREIGN KEY REFERENCES Payments(Id),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    NextAttemptAt DATETIME2 NOT NULL,
    AttemptNumber INT NOT NULL,
    MaxAttempts INT NOT NULL DEFAULT 5,
    Status NVARCHAR(50) NOT NULL,
    LastError NVARCHAR(MAX),
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2
);
CREATE INDEX IX_PaymentRetrySchedules_NextAttemptAt_Status
    ON PaymentRetrySchedules(NextAttemptAt, Status) WHERE Status = 'Scheduled';
CREATE INDEX IX_PaymentRetrySchedules_PaymentId
    ON PaymentRetrySchedules(PaymentId);
```

#### 6.2 Migration: AddReconciliationTables

```sql
CREATE TABLE ReconciliationReports (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    RunDate DATETIME2 NOT NULL,
    FromDate DATETIME2 NOT NULL,
    ToDate DATETIME2 NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    DiscrepanciesCount INT NOT NULL,
    AutoResolvedCount INT NOT NULL,
    TotalPayments INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL
);

CREATE TABLE PaymentDiscrepancies (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ReportId UNIQUEIDENTIFIER FOREIGN KEY REFERENCES ReconciliationReports(Id),
    PaymentId UNIQUEIDENTIFIER,
    DiscrepancyType NVARCHAR(50) NOT NULL,
    OrbitoStatus NVARCHAR(50),
    StripeStatus NVARCHAR(50),
    OrbitoAmount DECIMAL(18,2),
    StripeAmount DECIMAL(18,2),
    Resolution NVARCHAR(MAX),
    ResolvedAt DATETIME2
);
```

#### 6.3 Migration: AddIdempotencyKeyToPayments

```sql
ALTER TABLE Payments
ADD IdempotencyKey NVARCHAR(100) NULL;

CREATE UNIQUE INDEX IX_Payments_IdempotencyKey
    ON Payments(IdempotencyKey) WHERE IdempotencyKey IS NOT NULL;
```

#### 6.4 Migration: AddPerformanceIndexes

```sql
CREATE INDEX IX_Payments_CreatedAt_Status
    ON Payments(CreatedAt, Status) INCLUDE (Amount, Currency);
CREATE INDEX IX_Payments_TenantId_ClientId_Status
    ON Payments(TenantId, ClientId, Status);
```

---

### **Etap 7: Configuration (DzieD 9)**

#### 7.1 appsettings.json

```json
{
  "PaymentSettings": {
    "Retry": {
      "MaxAttempts": 5,
      "BackoffMultiplier": 2.0,
      "InitialDelayMinutes": 5,
      "MaxDelayHours": 24
    },
    "Reconciliation": {
      "RunTime": "02:00:00",
      "LookbackDays": 7,
      "EmailRecipients": ["finance@orbito.com", "admin@orbito.com"],
      "SlackWebhookUrl": "https://hooks.slack.com/...",
      "CriticalDiscrepancyThreshold": 10
    },
    "Monitoring": {
      "FailureRateThresholdPercent": 20,
      "MaxPendingRetries": 1000,
      "StripeHealthCheckTimeoutSeconds": 5
    },
    "Idempotency": {
      "CacheTtlHours": 24,
      "RedisConnectionString": "localhost:6379"
    }
  }
}
```

#### 7.2 Configuration Classes

- **PaymentRetrySettings.cs**
- **ReconciliationSettings.cs**
- **MonitoringSettings.cs**
- **IdempotencySettings.cs**

---

### **Etap 8: Integration & Testing (Dni 10-11)**

#### 8.1 Unit Tests (95% coverage)

- **PaymentRetryServiceTests.cs**

  - Test exponential backoff calculation
  - Test thread-safety (parallel retry scheduling)
  - Test max attempts enforcement

- **ReconciliationServiceTests.cs**

  - Test discrepancy detection
  - Test auto-resolution rules
  - Mock Stripe API responses

- **IdempotencyMiddlewareTests.cs**
  - Test duplicate request handling
  - Test cache expiration
  - Test concurrent requests

#### 8.2 Integration Tests

- **PaymentRetryIntegrationTests.cs**

  - End-to-end: failed payment � schedule retry � process retry � success
  - Test with real DB (in-memory)

- **ReconciliationIntegrationTests.cs**
  - Test full reconciliation flow
  - Test report generation + email sending

#### 8.3 Load Tests

- **PaymentRetryLoadTests.cs** (k6 or NBomber)
  - Simulate 1000 failed payments
  - Test retry processing performance
  - Measure DB connection pool usage

---

### **Etap 9: API Endpoints (DzieD 12)**

#### 9.1 PaymentRetryController.cs

```csharp
[Authorize(Roles = "Provider,Client")]
[Route("api/payments/retry")]
public class PaymentRetryController : ApiControllerBase
{
    [HttpPost("{paymentId}")]
    public async Task<IActionResult> RetryPayment(Guid paymentId, CancellationToken ct);

    [HttpPost("bulk")]
    public async Task<IActionResult> BulkRetry([FromBody] BulkRetryRequest request, CancellationToken ct);

    [HttpGet("scheduled")]
    public async Task<IActionResult> GetScheduledRetries([FromQuery] PaginationParams pagination, CancellationToken ct);

    [HttpDelete("{scheduleId}")]
    public async Task<IActionResult> CancelRetry(Guid scheduleId, CancellationToken ct);
}
```

#### 9.2 ReconciliationController.cs

```csharp
[Authorize(Roles = "PlatformAdmin,Provider")]
[Route("api/reconciliation")]
public class ReconciliationController : ApiControllerBase
{
    [HttpPost("run")]
    public async Task<IActionResult> RunReconciliation([FromBody] ReconciliationRequest request, CancellationToken ct);

    [HttpGet("reports")]
    public async Task<IActionResult> GetReports([FromQuery] PaginationParams pagination, CancellationToken ct);

    [HttpGet("reports/{reportId}/discrepancies")]
    public async Task<IActionResult> GetDiscrepancies(Guid reportId, CancellationToken ct);
}
```

#### 9.3 PaymentMetricsController.cs

```csharp
[Authorize(Roles = "Provider")]
[Route("api/payments/metrics")]
public class PaymentMetricsController : ApiControllerBase
{
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics([FromQuery] DateRangeParams dateRange, CancellationToken ct);

    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenue([FromQuery] RevenueReportParams params, CancellationToken ct);

    [HttpGet("trends")]
    public async Task<IActionResult> GetTrends([FromQuery] TrendParams params, CancellationToken ct);

    [HttpGet("failure-reasons")]
    public async Task<IActionResult> GetFailureReasons([FromQuery] DateRangeParams dateRange, CancellationToken ct);
}
```

---

### **Etap 10: Documentation & Deployment (DzieD 13)**

#### 10.1 XML Documentation

- Dodaj XML comments do wszystkich publicznych API
- Swagger examples dla complex DTOs

#### 10.2 Migration Scripts

- Production-ready migration scripts
- Rollback scripts for emergency

#### 10.3 Deployment Checklist

- [ ] Wszystkie testy green (Unit + Integration)
- [ ] Migracje przetestowane na staging
- [ ] Redis cache configured
- [ ] Hangfire/Quartz job registered
- [ ] Health checks verified
- [ ] Monitoring dashboards created (Grafana)
- [ ] Alert rules configured (critical failures)
- [ ] Performance baseline established

---

## = Security Checklist

- [ ] **Wszystkie repozytoria** u|ywaj `ITenantContext` + `ClientId` verification
- [ ] **Query filters** po TenantId w ka|dej tabeli
- [ ] **Rate limiting** dla retry endpoints (max 5 requests/15min)
- [ ] **Idempotency keys** wymagane dla payment creation
- [ ] **Webhook signature verification** dla Stripe callbacks
- [ ] **Input validation** FluentValidation dla wszystkich commands
- [ ] **Authorization** checks w ka|dym endpoincie (Roles + TenantId)
- [ ] **SQL injection prevention** - tylko parametryzowane queries
- [ ] **Secrets management** - Azure Key Vault/AWS Secrets Manager
- [ ] **Audit logging** dla sensitive operations (retry, reconciliation)

---

## =� Success Metrics

### Performance KPIs

- Payment retry success rate > 80%
- Reconciliation runtime < 5 minutes (dla 10k payments)
- Health check response time < 500ms
- API response time (p95) < 200ms

### Quality KPIs

- Test coverage > 95%
- Zero critical security vulnerabilities (SonarQube)
- Code duplication < 3%
- Technical debt ratio < 5%

---

## =� Deployment Strategy

### Phase 1: Dark Launch (Week 1)

- Deploy retry logic (disabled via feature flag)
- Monitor logs, no user impact

### Phase 2: Canary (Week 2)

- Enable retry dla 10% tenants
- Monitor metrics closely

### Phase 3: Full Rollout (Week 3)

- Enable dla wszystkich tenants
- 24/7 on-call monitoring

### Phase 4: Reconciliation (Week 4)

- Enable daily reconciliation job
- Email reports to finance team

---

**Szacowany czas realizacji**: 13 dni roboczych (2.5 tygodnia)
**Team size**: 2 senior developers + 1 QA engineer
**Risk level**: Medium (external API dependency - Stripe)
