---
name: backend-audit
description: >
  Comprehensive backend audit for .NET 9 Clean Architecture projects
  (CQRS/MediatR, EF Core, Stripe). Use when the user says "run backend-audit",
  "audit backend", "audit phase", or wants to review/improve backend code quality.
  Supports incremental phases like --phase 1.1.
---

# Backend Audit Skill

## Cel skilla

Audit backendu Orbito — .NET 9, Clean Architecture (Domain → Application → Infrastructure → API), CQRS z MediatR, EF Core, Stripe. Projekt jest multi-tenant (każdy Provider ma izolowane dane).

## Token Management — KRYTYCZNE ZASADY

1. **JEDEN phase na sesję** — nigdy nie rób wielu phases w jednej konwersacji
2. **Czytaj TYLKO pliki z bieżącego phase** — nie eksploruj całego projektu
3. **Po każdym phase aktualizuj audit-progress.json** zgodnie z nowymi zasadami (tylko nienaprawione issues)
4. **Jeśli phase ma >15 plików .cs** — podziel na sub-phases (np. 2.1a, 2.1b) i zapytaj użytkownika

**UWAGA:** Istniejące wpisy w `audit-progress.json` mogą zawierać informacje o naprawionych problemach (stary format). Przy kolejnych audytach **stosuj nowe zasady** — zapisuj tylko nienaprawione issues.

## Komendy

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

## Phases

**PHASE 0: Health Check (automatyczne skany)**

| Block | Target           | Skrypt                         |
| ----- | ---------------- | ------------------------------ |
| 0.1   | Build & Warnings | `scripts/build_warnings.py .`  |
| 0.2   | Dependency Scan  | `scripts/dependency_scan.py .` |
| 0.3   | Code Metrics     | `scripts/code_metrics.py .`    |

**PHASE 1: Domain Layer (`Orbito.Domain/`)**

| Block | Target                 | Est. Files |
| ----- | ---------------------- | ---------- |
| 1.1   | Entities               | ~14        |
| 1.2   | Value Objects          | ~5         |
| 1.3   | Enums & Constants      | ~13        |
| 1.4   | Domain Events & Errors | ~8         |
| 1.5   | Domain Interfaces      | ~3         |

**PHASE 2: Application Layer (`Orbito.Application/`)**

| Block | Target                                   | Est. Files     |
| ----- | ---------------------------------------- | -------------- |
| 2.1   | Commands — Clients                       | ~8             |
| 2.2   | Queries — Clients                        | ~4             |
| **2.3a** | **Payments — Commands Core** (CreatePaymentIntent, ProcessPayment, UpdatePaymentStatus) | ~10 |
| **2.3b** | **Payments — Commands Refunds & Retries** (Refund, Retry, BulkRetry, CancelRetry) | ~10 |
| **2.3c** | **Payments — Commands Webhooks & Stripe** (ProcessWebhook, UpdateFromWebhook, CreateStripeCustomer, SavePaymentMethod) | ~8 |
| **2.3d** | **Payments — Commands Validators** | ~6 |
| **2.3e** | **Payments — Queries Part 1** (GetPaymentById, GetAllPayments, GetPaymentsBySubscription, GetPaymentMethodsByClient) | ~10 |
| **2.3f** | **Payments — Queries Part 2** (Statistics, Trends, RevenueReport, FailedPayments, ScheduledRetries, FailureReasons) | ~15 |
| **2.3g** | **Payments — Root Validators** | ~5 |
| 2.4   | Commands — Subscriptions                 | ~6             |
| 2.5   | Commands — Other (Team, Provider, Plans) | ~10            |
| 2.6   | Interfaces                               | ~35 (PODZIEL!) |
| 2.7   | Validators                               | ~8             |
| 2.8   | Application Services                     | ~10            |
| 2.9   | Background Jobs                          | ~4             |
| **2.10a** | **DTOs & Models — Clients** | ~11 |
| **2.10b** | **DTOs & Models — Providers** | ~9 |
| **2.10c** | **DTOs & Models — TeamMembers & ProviderSubscriptions** | ~11 |
| **2.10d** | **DTOs & Models — Subscriptions** | ~12 |
| **2.10e** | **DTOs & Models — SubscriptionPlans** | ~12 |
| **2.10f** | **DTOs & Models — Payments Commands** | ~13 |
| **2.10g** | **DTOs & Models — Payments Queries** | ~14 |
| **2.10h** | **DTOs & Models — Mappers & Extensions** | ~8 |
| **2.10i** | **DTOs & Models — Common Configuration & Settings** | ~9 |
| **2.10j** | **DTOs & Models — Common Core Models** | ~10 |
| **2.10k** | **DTOs & Models — Common PaymentGateway Models** | ~9 |
| **2.10l** | **DTOs & Models — Common Authorization & Helpers** | ~8 |
| 2.11  | Behaviours & Pipeline                    | ~3             |

**PHASE 3: Infrastructure Layer (`Orbito.Infrastructure/`)**

| Block | Target                  | Est. Files |
| ----- | ----------------------- | ---------- |
| 3.1   | DbContext & EF Config   | ~5         |
| 3.2   | Repositories            | ~12        |
| 3.3   | Stripe Integration      | ~6         |
| 3.4   | Infrastructure Services | ~7         |
| 3.5   | Background Jobs (Infra) | ~4         |
| 3.6   | DI Registration         | 1          |

**PHASE 4: API Layer (`Orbito.API/`)**

| Block | Target                         | Est. Files |
| ----- | ------------------------------ | ---------- |
| 4.1   | Controllers (Part 1 — Core)    | ~6         |
| 4.2   | Controllers (Part 2 — Billing) | ~9         |
| 4.3   | Middleware                     | ~4         |
| 4.4   | Program.cs & Config            | ~4         |
| 4.5   | Health Checks                  | ~2         |

**PHASE 5: Cross-Cutting Concerns**

| Block | Target                     | Skrypt                              |
| ----- | -------------------------- | ----------------------------------- |
| 5.1   | Multi-Tenancy Security     | `scripts/tenant_security_scan.py .` |
| 5.2   | Error Handling & Result<T> | grep scan                           |
| 5.3   | Logging & Observability    | grep scan                           |
| 5.4   | Security Hardening         | config + auth                       |
| 5.5   | Performance Patterns       | `scripts/performance_scan.py .`     |

## Workflow wykonania phase

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

**Krok 4: Podsumowanie wyników**

Wyświetl użytkownikowi:
- Liczba audytowanych plików
- Zliczone problemy (critical, major, minor, suggestion)
- Health score phase (A/B/C/D/F)
- Lista znalezionych issues

**UWAGA: NIE twórz plików .md z raportami**, chyba że użytkownik wyraźnie o to poprosi.

Scoring:

- **A** (0 critical, 0-2 major) — Clean
- **B** (0 critical, 3-5 major) — Acceptable
- **C** (1-2 critical OR 6+ major) — Needs work
- **D** (3+ critical) — Significant issues
- **F** (5+ critical) — Requires rework

**Krok 5: Fixy (jeśli --fix)**
Jeśli flaga `--fix`:

1. Napraw WSZYSTKIE `CRITICAL`
2. Napraw WSZYSTKIE `MAJOR`
3. Zapytaj przed naprawą `MINOR`
4. Ponownie uruchom skrypty automatyczne
5. Uruchom `dotnet build` żeby potwierdzić brak regresji

**Krok 6: Aktualizuj audit-progress.json — KRYTYCZNE!**

Po zakończeniu phase (z fixem lub bez) **ZAWSZE aktualizuj** `audit-progress.json`:

1. Zmień `status` na `"completed"`
2. Wpisz `score` (A/B/C/D/F)
3. Ustaw `issues: { critical, major, minor, suggestion }` — **TYLKO nienaprawione problemy**
4. Dodaj `date` (format: "YYYY-MM-DD")
5. Ustaw `fixesApplied: true` jeśli użyto flagi `--fix`
6. W `notes[]` zapisz:
   - Liczbę audytowanych plików
   - **TYLKO nienaprawione problemy** z dokładną lokalizacją (plik:linia)
   - **NIE zapisuj** informacji o naprawionych problemach

**UWAGA KRYTYCZNA:** W sekcji `issues` i `notes` mają znajdować się **WYŁĄCZNIE nienaprawione błędy**. Nie zapisuj tutaj problemów, które zostały naprawione.

**Przykład prawidłowego wpisu:**

```json
{
  "id": "2.1",
  "phase": "PHASE 2: Application Layer",
  "block": "Commands — Clients",
  "target": "Orbito.Application/Clients/Commands/",
  "estFiles": 8,
  "status": "completed",
  "score": "B",
  "issues": { "critical": 0, "major": 2, "minor": 1, "suggestion": 0 },
  "date": "2026-03-31",
  "fixesApplied": true,
  "notes": [
    "8 command handler files audited",
    "UNFIXED MAJOR: CreateClientCommandHandler.cs:45 - Missing tenant validation in handler",
    "UNFIXED MAJOR: UpdateClientCommandHandler.cs:67 - Race condition in concurrent updates",
    "UNFIXED MINOR: DeleteClientCommandHandler.cs:23 - Missing XML documentation"
  ]
}
```

**Przykład ZŁEGO wpisu (NIE TAK!):**

```json
{
  "notes": [
    "8 files audited",
    "FIXED: Added tenant validation",  // ❌ NIE ZAPISUJ naprawionych issues!
    "FIXED: Resolved race condition",  // ❌ NIE ZAPISUJ naprawionych issues!
    "GOOD: All handlers use Result<T>",  // ❌ NIE ZAPISUJ pozytywnych komentarzy!
    "UNFIXED: Missing XML docs"  // ✅ TO jest OK
  ]
}
```

## Komenda --phase list

Gdy użytkownik wpisze `--phase list`:

1. Przeczytaj `audit-progress.json`
2. Wyświetl tabelkę ze statusem każdego phase (pending/completed/in-progress)
3. Pokaż overall progress (X/N phases completed)

## Znane problemy projektu

- `Orbito.Infrastructure/Persistance/` — literówka w nazwie folderu (powinno być Persistence)
- Istnieją OBA foldery `Persistance/` i `Persistence/` — do konsolidacji
- Istnieje plik `PaymentRepository.cs.bak` — do usunięcia
