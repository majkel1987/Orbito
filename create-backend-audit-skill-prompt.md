# Prompt dla Claude Code — Stwórz skill `backend-audit`

Skopiuj całą treść poniżej i wklej jako jedną komendę w Claude Code.

---

```
Stwórz od zera skill "backend-audit" do audytowania backendu .NET 9 (Clean Architecture, CQRS/MediatR, EF Core, Stripe). Skill ma działać z Claude Code — użytkownik wpisuje np. `run backend-audit --phase 1.1` i dostaje raport.

## KROK 1: Struktura katalogów

Utwórz poniższą strukturę w `.claude/skills/backend-audit/`:

```
.claude/skills/backend-audit/
├── SKILL.md
├── audit-progress.json
├── QUICKSTART.md
├── scripts/
│   ├── architecture_check.py
│   ├── build_warnings.py
│   ├── code_metrics.py
│   ├── dependency_scan.py
│   ├── performance_scan.py
│   ├── tenant_security_scan.py
│   └── test_coverage_gaps.py
└── references/
    └── backend-checklist.md
```

## KROK 2: SKILL.md

Plik `.claude/skills/backend-audit/SKILL.md` — to jest najważniejszy plik. Musi mieć YAML frontmatter na samym początku:

```yaml
---
name: backend-audit
description: >
  Comprehensive backend audit for .NET 9 Clean Architecture projects
  (CQRS/MediatR, EF Core, Stripe). Use when the user says "run backend-audit",
  "audit backend", "audit phase", or wants to review/improve backend code quality.
  Supports incremental phases like --phase 1.1.
---
```

Potem treść SKILL.md:

### Cel skilla

Audit backendu Orbito — .NET 9, Clean Architecture (Domain → Application → Infrastructure → API), CQRS z MediatR, EF Core, Stripe. Projekt jest multi-tenant (każdy Provider ma izolowane dane).

### Token Management — KRYTYCZNE ZASADY

1. **JEDEN phase na sesję** — nigdy nie rób wielu phases w jednej konwersacji
2. **Czytaj TYLKO pliki z bieżącego phase** — nie eksploruj całego projektu
3. **Po każdym phase aktualizuj audit-progress.json**
4. **Jeśli phase ma >15 plików .cs** — podziel na sub-phases (np. 2.1a, 2.1b) i zapytaj użytkownika

### Komendy

```bash
# Lista wszystkich phases i ich status
run backend-audit --phase list

# Audyt konkretnego phase
run backend-audit --phase 1.1

# Audyt z auto-fixem krytycznych/major issues
run backend-audit --phase 1.1 --fix

# Audyt jednego pliku
run backend-audit --path Orbito.Domain/Entities/Payment.cs
```

### Phases

**PHASE 0: Health Check (automatyczne skany)**

| Block | Target | Skrypt |
|-------|--------|--------|
| 0.1 | Build & Warnings | `scripts/build_warnings.py .` |
| 0.2 | Dependency Scan | `scripts/dependency_scan.py .` |
| 0.3 | Code Metrics | `scripts/code_metrics.py .` |

**PHASE 1: Domain Layer (`Orbito.Domain/`)**

| Block | Target | Est. Files |
|-------|--------|-----------|
| 1.1 | Entities | ~14 |
| 1.2 | Value Objects | ~5 |
| 1.3 | Enums & Constants | ~13 |
| 1.4 | Domain Events & Errors | ~8 |
| 1.5 | Domain Interfaces | ~3 |

**PHASE 2: Application Layer (`Orbito.Application/`)**

| Block | Target | Est. Files |
|-------|--------|-----------|
| 2.1 | Commands — Clients | ~8 |
| 2.2 | Queries — Clients | ~4 |
| 2.3 | Commands — Payments | ~8 |
| 2.4 | Commands — Subscriptions | ~6 |
| 2.5 | Commands — Other (Team, Provider, Plans) | ~10 |
| 2.6 | Interfaces | ~35 (PODZIEL!) |
| 2.7 | Validators | ~8 |
| 2.8 | Application Services | ~10 |
| 2.9 | Background Jobs | ~4 |
| 2.10 | DTOs & Models | ~15 |
| 2.11 | Behaviours & Pipeline | ~3 |

**PHASE 3: Infrastructure Layer (`Orbito.Infrastructure/`)**

| Block | Target | Est. Files |
|-------|--------|-----------|
| 3.1 | DbContext & EF Config | ~5 |
| 3.2 | Repositories | ~12 |
| 3.3 | Stripe Integration | ~6 |
| 3.4 | Infrastructure Services | ~7 |
| 3.5 | Background Jobs (Infra) | ~4 |
| 3.6 | DI Registration | 1 |

**PHASE 4: API Layer (`Orbito.API/`)**

| Block | Target | Est. Files |
|-------|--------|-----------|
| 4.1 | Controllers (Part 1 — Core) | ~6 |
| 4.2 | Controllers (Part 2 — Billing) | ~9 |
| 4.3 | Middleware | ~4 |
| 4.4 | Program.cs & Config | ~4 |
| 4.5 | Health Checks | ~2 |

**PHASE 5: Cross-Cutting Concerns**

| Block | Target | Skrypt |
|-------|--------|--------|
| 5.1 | Multi-Tenancy Security | `scripts/tenant_security_scan.py .` |
| 5.2 | Error Handling & Result<T> | grep scan |
| 5.3 | Logging & Observability | grep scan |
| 5.4 | Security Hardening | config + auth |
| 5.5 | Performance Patterns | `scripts/performance_scan.py .` |

**PHASE 6: Tests (`Orbito.Tests/`)**

| Block | Target | Skrypt |
|-------|--------|--------|
| 6.1 | Test Coverage Gaps | `scripts/test_coverage_gaps.py .` |
| 6.2 | Test Quality — Unit | sample 20 files |
| 6.3 | Test Quality — Integration | ~10 files |
| 6.4 | Failing Tests Triage | 15 failing tests |

### Workflow wykonania phase

Gdy użytkownik uruchamia phase, wykonaj PO KOLEI:

**Krok 1: Scope Check**
```bash
find <target-path> -name "*.cs" ! -path "*/bin/*" ! -path "*/obj/*" | wc -l
```
Jeśli >15 plików — STOP, zapytaj użytkownika o podział.

**Krok 2: Uruchom skrypty automatyczne**
Odpowiedni skrypt z `scripts/` dla danego phase. Skrypty przyjmują ścieżkę do solution root jako argument.

**Krok 3: Manualny code review**
Przeczytaj każdy plik w scope danego phase. Sprawdź heurystyki z `references/backend-checklist.md` (czytaj TYLKO sekcję dla bieżącego phase).

Zapisuj issues w formacie:
```
- **[SEVERITY]** [CATEGORY] — [FILE:LINE]
  Issue: [opis]
  Fix: [konkretna zmiana kodu]
```

Severity:
- `CRITICAL` — luka bezpieczeństwa, wyciek danych, crash na produkcji
- `MAJOR` — bug, niepoprawne zachowanie, naruszenie architektury
- `MINOR` — code smell, niespójność
- `SUGGESTION` — propozycja ulepszenia

**Krok 4: Raport**
```markdown
## Backend Audit Report — Phase [X.Y]: [Name]

### Summary
- Files audited: N
- Issues: X critical, Y major, Z minor, W suggestions
- Phase health: [A/B/C/D/F]

### Critical Issues
[lista]

### Major Issues
[lista]

### Minor Issues & Suggestions
[lista]

### Recommendations
[priorytetyzowane action items]
```

Scoring:
- **A** (0 critical, 0-2 major) — Clean
- **B** (0 critical, 3-5 major) — Acceptable
- **C** (1-2 critical OR 6+ major) — Needs work
- **D** (3+ critical) — Significant issues
- **F** (5+ critical) — Requires rework

**Krok 5: Aktualizuj progress**
Zaktualizuj `audit-progress.json` — zmień status na "completed", wpisz score, issue counts, datę.

**Krok 6: Fixy (jeśli --fix)**
Jeśli flaga `--fix`:
1. Napraw WSZYSTKIE `CRITICAL`
2. Napraw WSZYSTKIE `MAJOR`
3. Zapytaj przed naprawą `MINOR`
4. Ponownie uruchom skrypty automatyczne
5. Uruchom `dotnet build` żeby potwierdzić brak regresji

### Komenda --phase list

Gdy użytkownik wpisze `--phase list`:
1. Przeczytaj `audit-progress.json`
2. Wyświetl tabelkę ze statusem każdego phase (pending/completed/in-progress)
3. Pokaż overall progress (X/N phases completed)

### Znane problemy projektu

- `Orbito.Infrastructure/Persistance/` — literówka w nazwie folderu (powinno być Persistence)
- Istnieją OBA foldery `Persistance/` i `Persistence/` — do konsolidacji
- Istnieje plik `PaymentRepository.cs.bak` — do usunięcia

---

## KROK 3: Skrypty Python

Utwórz 7 skryptów w `scripts/`. Każdy:
- Ma `#!/usr/bin/env python3` na początku
- Przyjmuje `<solution-root>` jako argument z `sys.argv[1]`
- Ma `chmod +x`
- Jest self-contained (zero zewnętrznych zależności)

### scripts/architecture_check.py

Sprawdza Clean Architecture boundaries:
- Domain ZERO zależności od innych warstw
- Application zależy tylko od Domain
- Infrastructure od Application + Domain
- API od Application (Infra tylko dla DI)

Sprawdza:
1. `.csproj` ProjectReference — czy nie ma zabronionych referencji
2. `using` statements — czy nie importuje z zabronionych warstw
3. Naming conventions — Controllers, Commands, Queries, Entities, Interfaces, DTOs, Services, Repositories
4. Entity purity — czy Domain entities nie mają atrybutów EF/JSON/Mongo, DbContext, ILogger

Reguły warstw:
```python
LAYER_RULES = {
    "Orbito.Domain": {
        "allowed": [],
        "forbidden": ["Orbito.Application", "Orbito.Infrastructure", "Orbito.API"],
    },
    "Orbito.Application": {
        "allowed": ["Orbito.Domain"],
        "forbidden": ["Orbito.Infrastructure", "Orbito.API"],
    },
    "Orbito.Infrastructure": {
        "allowed": ["Orbito.Domain", "Orbito.Application"],
        "forbidden": ["Orbito.API"],
    },
    "Orbito.API": {
        "allowed": ["Orbito.Application", "Orbito.Infrastructure"],
        "forbidden": ["Orbito.Domain"],
    },
}
```

Wyjątek: API → Domain import jest OK w plikach `DependencyInjection` i `Program`.

Severities: CRITICAL dla złamanych layer boundaries w csproj, MAJOR dla using violations, MINOR dla naming conventions.

Exit codes: 0 = clean, 1 = major issues, 2 = critical.

### scripts/build_warnings.py

Uruchamia `dotnet build --no-incremental -v q` na solution, parsuje stdout+stderr:
- Warnings: regex `(\S+\.cs)\((\d+),\d+\):\s*warning\s+(CS\d+):\s*(.+)`
- Errors: regex `(\S+\.cs)\((\d+),\d+\):\s*error\s+(CS\d+):\s*(.+)`

Grupuje warnings po kodzie CS, pokazuje top 3 wystąpienia per kod. Raportuje summary: BUILD SUCCESS/FAILED, error count, warning count, unique codes.

### scripts/code_metrics.py

Skanuje projekty: Orbito.API, Orbito.Application, Orbito.Domain, Orbito.Infrastructure, Orbito.Tests.

Per projekt: file count, line count, avg lines/file, top 5 largest files (flag >200 lines).

Complexity hotspots (pliki >200 linii): lines, method count (regex na public/private/protected + async Task/void/string etc.), dependency count (private readonly I-prefixed), try-catch count.

TODO/FIXME scan — szuka TODO, FIXME, HACK, XXX.

Backup file detection — *.bak, *.cs.bak, *.old.

### scripts/dependency_scan.py

Parsuje wszystkie `.csproj` — wyciąga PackageReference Include+Version.

Sprawdza version consistency — czy ten sam pakiet nie ma różnych wersji w różnych projektach.

Próbuje `dotnet list <sln> package --outdated` (timeout 60s, graceful fail jeśli brak dotnet CLI).

Flaguje security-sensitive packages: Stripe.net, JwtBearer, EF Core, Serilog — z notką dlaczego ważne.

### scripts/performance_scan.py

Skanuje pliki .cs (pomija bin/obj) pod kątem 10 anti-patternów:

1. **Sync-over-async** (CRITICAL): `.Result`, `.Wait()`, `.GetAwaiter().GetResult()` — pomija Tests
2. **Missing CancellationToken** (MAJOR): async Task bez CancellationToken w sygnaturze
3. **In-memory filtering** (MAJOR): ToList/ToArray a potem Where/Select/OrderBy w następnych 3 liniach
4. **Missing AsNoTracking** (MINOR): read queries w repozytoriach bez AsNoTracking — scope: repositories only
5. **SQL injection** (CRITICAL): string interpolation/concatenation w FromSqlRaw/ExecuteSqlRaw
6. **Task.Delay** (MAJOR): w nie-testowym kodzie — workaround na race condition
7. **new HttpClient()** (MAJOR): bezpośrednie tworzenie — socket exhaustion. Użyj IHttpClientFactory
8. **Unbounded query** (MAJOR): ToList bez Take/Skip/PageSize — scope: repositories only
9. **Lock in async** (MINOR): `lock(` — użyj SemaphoreSlim
10. **ConfigureAwait(false) missing** (SUGGESTION): w Infrastructure code — scope: infrastructure only

Każdy check ma: name, severity, pattern (regex), opcjonalnie negative_pattern (jeśli znalezione = OK), negative_context (broader scope), followup_pattern, exclude_patterns, scope filter.

### scripts/tenant_security_scan.py

NAJWAŻNIEJSZY skrypt — wykrywa cross-tenant data leaks.

5 skanerów:

1. **Repositories** — czy każdy Repository injectuje ITenantContext? Czy każda publiczna async metoda filtruje po TenantId? (pomija SaveChanges, Dispose, Commit)
2. **Query handlers** — czy IRequestHandler-y referencją ITenantContext? (pomija Admin-only)
3. **Controllers** — czy TenantId/ProviderId NIE pochodzi z FromBody/FromQuery (musi z claims). Czy [AllowAnonymous] nie jest na mutation endpoints (Post/Put/Delete/Patch)
4. **Cache keys** — czy operacje Cache.Set/Get zawierają TenantId w kluczu
5. **Background jobs** — czy *Job.cs pliki z data access (Repository/DbContext/SaveChanges) ustawiają tenant context

Severity: CRITICAL dla brakującego tenant filtering w repo/controllers/jobs, MAJOR dla handlers/cache.

### scripts/test_coverage_gaps.py

Szuka klas, które powinny mieć testy:
- *Handler.cs (HIGH priority)
- *Validator.cs (MEDIUM)
- *Service.cs (HIGH, pomija interfejsy)
- *Job.cs (HIGH)
- Entities/*.cs (MEDIUM)
- *Repository.cs (LOW, pomija interfejsy)

Szuka test files w `Orbito.Tests/` — *Tests.cs i *Test.cs. Matchuje po nazwie (FooHandler → FooHandlerTests, FooTests, itp.).

Raportuje gaps po kategorii z priority icons, covered count, coverage %.

---

## KROK 4: references/backend-checklist.md

Utwórz plik `references/backend-checklist.md` z heurystykami per phase. Sekcje:

### 1. Domain Layer

**1.1 Entities:** private setters, constructor invariants, business logic w entity nie w handlers, navigation properties jako IReadOnlyCollection<T>, domain events collection, brak infra concerns (EF/JSON attributes), IMustHaveTenant, equality by ID. Red flags: public set, business rules w handlers, entity z 20+ properties, entity bez metod (anemic), [JsonProperty]/[Column] na domain. Thresholds: max 15 properties, max 10 methods.

**1.2 Value Objects:** immutability (init/get only), structural equality (Equals/GetHashCode), self-validation w constructor, brak entity references. Red flags: string email zamiast Email VO, mutable properties, VO z Id.

**1.3 Enums:** explicit int values, brak magic strings/numbers, PascalCase. Red flags: Status=1,2,3 bez nazw, stringi "active"/"pending" w kodzie, enum z 15+ values.

**1.4 Events & Errors:** past tense (PaymentCompleted), tylko IDs + essential data, immutable (records), error codes + messages, pełny error catalog. Red flags: events z navigation properties, generic Exception, brakujące error types.

**1.5 Interfaces:** tylko repository interfaces w Domain, ISP compliance, zwracają entities nie DTOs, zero infra types.

### 2. Application Layer

**2.1-2.5 Handlers:** jeden handler = jedna operacja, handler nie woła innych handlers, FluentValidation przed logiką, CancellationToken propagowany, Result<T> zamiast exceptions, tenant filtering, idempotency na payment operations. Red flags: handler >100 linii, handler z >5 dependencies, brak validation, catch-all exception. Thresholds: max 80 linii, max 5 deps, 100% CancellationToken.

**2.6 Interfaces:** ISP (max 5 metod per interface), brak god interfaces, async methods z CancellationToken, result types zamiast void+exceptions. Red flags: IPaymentService z 15 metod, sync methods w async interface.

**2.7 Validators:** każdy Command ma Validator, business rule validation w handler NIE validator, custom validators wyextractowane, error messages user-friendly. Red flags: brak walidatora, walidator z business logic, generic error messages.

**2.8 Services:** SRP, brak direct DB access (przez repos), proper DI, async all the way.

**2.9 Background Jobs:** retry logic, error handling z logging, idempotency, tenant context set before data access, graceful cancellation.

**2.10 DTOs:** no business logic, flat structure, proper mapping, brak sensitive data w response DTOs.

**2.11 Behaviours:** logging behaviour, validation behaviour, transaction behaviour (correct order), performance monitoring.

### 3. Infrastructure

**3.1 DbContext:** multi-tenant query filters, SaveChanges auditing, connection resiliency, proper entity configurations (Fluent API > attributes).

**3.2 Repositories:** implementują domain interfaces, tenant filtering w KAŻDYM query, AsNoTracking na reads, proper pagination, Unit of Work pattern.

**3.3 Stripe:** webhook signature verification, idempotency keys, proper error mapping, retry logic, no secrets in code.

**3.4 Services:** proper abstractions, no leaked implementation details, error handling, timeout configuration.

**3.5 Background Jobs (Infra):** Hangfire/job configuration, retry policies, concurrent execution prevention, dead letter handling.

**3.6 DI:** all services registered, correct lifetimes (Scoped for tenant-context, Singleton for stateless), no missing registrations.

### 4. API

**4.1-4.2 Controllers:** thin controllers (delegate to MediatR), proper HTTP status codes, [Authorize] on all mutations, model binding validation, proper route naming. Red flags: business logic w controller, missing [Authorize], inconsistent response types.

**4.3 Middleware:** error handling middleware (global), tenant resolution middleware, idempotency middleware, Stripe webhook signature middleware. Order matters!

**4.4 Program.cs:** proper middleware order, CORS config, rate limiting, HTTPS enforcement, health checks registered.

**4.5 Health checks:** DB connectivity, Stripe API, external dependencies, custom checks for background job health.

### 5. Cross-Cutting

**5.1 Multi-Tenancy:** KAŻDY query filtruje po TenantId, global query filters w DbContext, tenant z auth claims NIGDY z request body, cache keys zawierają tenant, background jobs ustawiają tenant context.

**5.2 Error Handling:** Result<T> pattern wszędzie, no naked exceptions, proper error codes, GlobalExceptionHandler jako last resort.

**5.3 Logging:** structured logging (Serilog), correlation IDs, no sensitive data w logach (PII, tokens, passwords), proper log levels.

**5.4 Security:** JWT validation, CORS properly configured, rate limiting on auth endpoints, secrets w User Secrets/env vars (nie w appsettings), HTTPS enforced, security headers.

**5.5 Performance:** patrz skrypt performance_scan.py + async all the way, proper caching strategy, DB indexes on filtered columns, no N+1.

### 6. Tests

**6.1 Coverage:** każdy Handler/Service/Job ma test, coverage >80% na Application layer.

**6.2 Unit Tests:** AAA pattern, meaningful test names, one assert per test (or related group), proper mocking (no over-mocking), edge cases tested.

**6.3 Integration Tests:** realistic scenarios, proper test DB setup/teardown, test tenant isolation, no test interdependence.

**6.4 Failing Tests:** categorize failures (outdated test, real bug, flaky, env-dependent), prioritize real bugs.

---

## KROK 5: audit-progress.json

Utwórz plik z 38 phase entries, każdy w formacie:
```json
{
  "id": "X.Y",
  "phase": "PHASE N: Name",
  "block": "Block Name",
  "status": "pending",
  "score": null,
  "issues": { "critical": 0, "major": 0, "minor": 0, "suggestion": 0 },
  "date": null,
  "fixesApplied": false,
  "notes": []
}
```

Phases: 0.1-0.3, 1.1-1.5, 2.1-2.11, 3.1-3.6, 4.1-4.5, 5.1-5.5, 6.1-6.4.

Dodaj target i estFiles tam gdzie relevantne (patrz tabele phases wyżej). Dodaj notes np. "Uses tenant_security_scan.py — CRITICAL priority" dla 5.1.

## KROK 6: QUICKSTART.md

Krótki guide z:
1. Instalacja (cp do .claude/skills/)
2. Recommended audit order (8 sprintów)
3. Tips (--phase list, --fix, single file, splitting large phases)
4. File structure

## KROK 7: Finalizacja

1. `chmod +x scripts/*.py`
2. Przetestuj: `python3 scripts/code_metrics.py .` (powinno działać na dowolnym katalogu bez crashu)
3. Sprawdź czy SKILL.md ma poprawny YAML frontmatter
4. Potwierdź strukturę: `find .claude/skills/backend-audit -type f | sort`

WAŻNE: Wszystkie ścieżki w SKILL.md do skryptów powinny być RELATYWNE do lokalizacji skilla, np.:
```bash
python3 .claude/skills/backend-audit/scripts/build_warnings.py .
```

Ale Claude Code powinien sam rozwiązać ścieżkę na podstawie tego, gdzie skill się znajduje. W SKILL.md odwołuj się do `scripts/` i `references/` bez pełnej ścieżki — Claude Code zna working directory skilla.
```
