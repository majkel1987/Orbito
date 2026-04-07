# Backend Audit Skill — Quick Start Guide

## Instalacja

Skill jest już zainstalowany w `.claude/skills/backend-audit/`.

## Jak używać

```bash
# Sprawdź status wszystkich phases
run backend-audit --phase list

# Uruchom konkretny phase
run backend-audit --phase 1.1

# Uruchom z automatycznymi fixami
run backend-audit --phase 1.1 --fix

# Audyt pojedynczego pliku
run backend-audit --path Orbito.Domain/Entities/Payment.cs
```

## Zalecana kolejność audytu (8 sprintów)

### Sprint 1: Health Check
- `--phase 0.1` Build & Warnings
- `--phase 0.2` Dependency Scan
- `--phase 0.3` Code Metrics

### Sprint 2: Domain Layer
- `--phase 1.1` Entities
- `--phase 1.2` Value Objects
- `--phase 1.3` Enums & Constants
- `--phase 1.4` Domain Events & Errors
- `--phase 1.5` Domain Interfaces

### Sprint 3: Application Layer (Clients)
- `--phase 2.1` Commands — Clients
- `--phase 2.2` Queries — Clients

### Sprint 3b: Application Layer (Payments — 7 sub-phases)
- `--phase 2.3a` Payments — Commands Core (CreatePaymentIntent, ProcessPayment, UpdatePaymentStatus)
- `--phase 2.3b` Payments — Commands Refunds & Retries (Refund, Retry, BulkRetry, CancelRetry)
- `--phase 2.3c` Payments — Commands Webhooks & Stripe (ProcessWebhook, UpdateFromWebhook, CreateStripeCustomer, SavePaymentMethod)
- `--phase 2.3d` Payments — Commands Validators
- `--phase 2.3e` Payments — Queries Part 1 (GetPaymentById, GetAllPayments, GetPaymentsBySubscription, GetPaymentMethodsByClient)
- `--phase 2.3f` Payments — Queries Part 2 (Statistics, Trends, RevenueReport, FailedPayments, ScheduledRetries, FailureReasons)
- `--phase 2.3g` Payments — Root Validators

### Sprint 3c: Application Layer (Other Commands)
- `--phase 2.4` Commands — Subscriptions
- `--phase 2.5` Commands — Other

### Sprint 4: Application Layer (Infrastructure)
- `--phase 2.6` Interfaces (UWAGA: duży phase, może wymagać podziału)
- `--phase 2.7` Validators
- `--phase 2.8` Application Services

### Sprint 5: Application Layer (Rest) + Infrastructure Start
- `--phase 2.9` Background Jobs
- `--phase 2.10` DTOs & Models
- `--phase 2.11` Behaviours & Pipeline
- `--phase 3.1` DbContext & EF Config

### Sprint 6: Infrastructure Layer
- `--phase 3.2` Repositories
- `--phase 3.3` Stripe Integration
- `--phase 3.4` Infrastructure Services
- `--phase 3.5` Background Jobs (Infra)
- `--phase 3.6` DI Registration

### Sprint 7: API Layer
- `--phase 4.1` Controllers (Core)
- `--phase 4.2` Controllers (Billing)
- `--phase 4.3` Middleware
- `--phase 4.4` Program.cs & Config
- `--phase 4.5` Health Checks

### Sprint 8: Cross-Cutting Concerns
- `--phase 5.1` Multi-Tenancy Security (KRYTYCZNE!)
- `--phase 5.2` Error Handling
- `--phase 5.3` Logging
- `--phase 5.4` Security Hardening
- `--phase 5.5` Performance Patterns

## Tips

1. **Jeden phase na sesję** — nie próbuj robić wielu phases w jednej konwersacji
2. **Używaj `--phase list`** — żeby sprawdzić postęp
3. **Duże phases (>15 plików)** — będą automatycznie podzielone na sub-phases
4. **Po każdym phase** — sprawdź `audit-progress.json` dla pełnej historii
5. **`--fix` flag** — automatycznie naprawia CRITICAL i MAJOR issues

## Struktura plików

```
.claude/skills/backend-audit/
├── SKILL.md                    # Główna definicja skilla
├── QUICKSTART.md               # Ten plik
├── audit-progress.json         # Tracking postępu
├── scripts/
│   ├── architecture_check.py   # Sprawdza Clean Architecture boundaries
│   ├── build_warnings.py       # Parsuje dotnet build warnings
│   ├── code_metrics.py         # Metryki kodu (LOC, complexity)
│   ├── dependency_scan.py      # Skanuje PackageReferences
│   ├── performance_scan.py     # Wykrywa performance anti-patterns
│   └── tenant_security_scan.py # Wykrywa cross-tenant data leaks
└── references/
    └── backend-checklist.md    # Heurystyki per phase
```

## Severity Levels

| Level | Opis | Przykład |
|-------|------|----------|
| CRITICAL | Security, data leak, crash | Brak tenant filtering w query |
| MAJOR | Bug, wrong behavior | Missing CancellationToken |
| MINOR | Code smell | Public setter on entity |
| SUGGESTION | Nice to have | Extract method refactoring |

## Scoring

| Grade | Criteria |
|-------|----------|
| A | 0 critical, 0-2 major |
| B | 0 critical, 3-5 major |
| C | 1-2 critical OR 6+ major |
| D | 3+ critical |
| F | 5+ critical |
