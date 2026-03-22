---
name: backend-audit
description: Comprehensive backend audit for .NET 9 Clean Architecture projects (CQRS/MediatR, EF Core, Stripe). Use when the user wants to audit, review, or improve backend code quality. Supports phases like --phase 1.1 for incremental audits.
---

# Backend Audit Skill

Perform incremental, token-efficient audits of a .NET 9 Clean Architecture backend.
Each phase is designed to be run in a **single Claude Code session** without hitting token limits.

## CRITICAL: Token Management Rules

1. **ONE phase per session** — never run multiple phases in one conversation
2. **Read only the files relevant to the current phase** — do not explore the whole project
3. **After each phase, update the progress tracker** before ending the session
4. **If a phase has too many files** (>15), split into sub-phases (e.g., 2.1a, 2.1b)

## Quick Start

```bash
# Show all phases and their status
claude "run backend-audit --phase list"

# Audit a specific phase
claude "run backend-audit --phase 1.1"

# Audit AND auto-fix critical/major issues
claude "run backend-audit --phase 1.1 --fix"

# Audit a specific file only
claude "run backend-audit --path Orbito.Domain/Entities/Payment.cs"
```

---

## Audit Phases

### PHASE 0: Project Health Check (run first, always)

Quick automated scan — no manual review needed.

| Block | Target           | Est. Files     | Description                                     |
| ----- | ---------------- | -------------- | ----------------------------------------------- |
| 0.1   | Build & Warnings | 0 (build only) | `dotnet build` — collect all warnings           |
| 0.2   | Dependency Scan  | \*.csproj      | Outdated/vulnerable NuGet packages              |
| 0.3   | Code Metrics     | project-wide   | Lines of code, file counts, complexity overview |

### PHASE 1: Domain Layer (`Orbito.Domain/`)

| Block | Target                 | Est. Files | Description                                   |
| ----- | ---------------------- | ---------- | --------------------------------------------- |
| 1.1   | Entities               | ~14 files  | Rich domain models, encapsulation, invariants |
| 1.2   | Value Objects          | ~5 files   | Immutability, equality, validation            |
| 1.3   | Enums & Constants      | ~13 files  | Enum design, magic strings/numbers            |
| 1.4   | Domain Events & Errors | ~8 files   | Event design, error catalog completeness      |
| 1.5   | Domain Interfaces      | ~3 files   | Interface segregation, domain purity          |

### PHASE 2: Application Layer (`Orbito.Application/`)

| Block | Target                     | Est. Files     | Description                           |
| ----- | -------------------------- | -------------- | ------------------------------------- |
| 2.1   | Commands — Clients         | ~8 folders     | CQRS command handlers, validation     |
| 2.2   | Queries — Clients          | ~4 folders     | Query handlers, projection efficiency |
| 2.3   | Commands — Payments\*      | ~8 folders     | Payment command handlers              |
| 2.4   | Commands — Subscriptions\* | ~6 folders     | Subscription lifecycle handlers       |
| 2.5   | Commands — Other\*         | remaining      | Team, Provider, Plans handlers        |
| 2.6   | Interfaces Audit           | ~35 files      | Interface bloat, ISP violations       |
| 2.7   | Validators                 | all validators | FluentValidation rules completeness   |
| 2.8   | Services                   | ~10 files      | Application services, SRP             |
| 2.9   | Background Jobs            | ~4 files       | Job design, error handling, retry     |
| 2.10  | DTOs & Models              | ~15 files      | DTO design, mapping, Result<T>        |
| 2.11  | Behaviours & Pipeline      | ~3 files       | MediatR pipeline, cross-cutting       |

_Note: Scan `Orbito.Application/Features/` with `find` first to see the actual folder structure, then adjust blocks accordingly._

### PHASE 3: Infrastructure Layer (`Orbito.Infrastructure/`)

| Block | Target                  | Est. Files | Description                             |
| ----- | ----------------------- | ---------- | --------------------------------------- |
| 3.1   | DbContext & Config      | ~5 files   | EF Core config, conventions, indexes    |
| 3.2   | Repositories            | ~10 files  | Repository pattern, tenant filtering    |
| 3.3   | Stripe Integration      | ~6 files   | Payment gateway, webhook processor      |
| 3.4   | Services                | ~7 files   | Email, cache, transaction, user context |
| 3.5   | Background Jobs (Infra) | ~4 files   | Infra-level jobs, scheduling            |
| 3.6   | DI Registration         | 1 file     | DependencyInjection.cs completeness     |

### PHASE 4: API Layer (`Orbito.API/`)

| Block | Target               | Est. Files | Description                                         |
| ----- | -------------------- | ---------- | --------------------------------------------------- |
| 4.1   | Controllers (Part 1) | ~8 files   | Account, Clients, Providers, Team, Users, Portal    |
| 4.2   | Controllers (Part 2) | ~8 files   | Payment\*, Subscriptions, Plans, Analytics, Webhook |
| 4.3   | Middleware           | ~4 files   | Error handling, idempotency, tenant, Stripe sig     |
| 4.4   | Program.cs & Config  | ~3 files   | Startup, DI, pipeline order, settings               |
| 4.5   | Health Checks        | ~2 files   | Health check completeness                           |

### PHASE 5: Cross-Cutting Concerns

| Block | Target                     | Est. Files    | Description                        |
| ----- | -------------------------- | ------------- | ---------------------------------- |
| 5.1   | Multi-Tenancy Security     | grep-based    | TenantId filtering everywhere      |
| 5.2   | Error Handling & Result<T> | grep-based    | Consistent error flow              |
| 5.3   | Logging & Observability    | grep-based    | Structured logging quality         |
| 5.4   | Security Hardening         | config + code | Auth, CORS, rate limiting, secrets |
| 5.5   | Performance Patterns       | grep-based    | N+1, async, caching                |

### PHASE 6: Test Quality (`Orbito.Tests/`)

| Block | Target                     | Est. Files        | Description                        |
| ----- | -------------------------- | ----------------- | ---------------------------------- |
| 6.1   | Test Coverage Gaps         | project-wide scan | Missing test files for handlers    |
| 6.2   | Test Quality — Unit        | ~20 files sample  | Assert quality, mock abuse, naming |
| 6.3   | Test Quality — Integration | ~10 files         | Realistic scenarios, cleanup       |
| 6.4   | Failing Tests Triage       | test results      | Root cause of 15 failing tests     |

---

## Phase Execution Workflow

When a phase is triggered, follow these steps IN ORDER:

### Step 1: Scope Check

```bash
# Count files in the target area
find <target-path> -name "*.cs" | wc -l

# If > 15 files, tell the user to split into sub-phases
```

If there are more than 15 .cs files for a single block, **STOP and ask the user** which sub-set to audit first. Do NOT try to read them all.

### Step 2: Run Automated Scripts

Run the relevant script(s) from `scripts/` for the current phase:

```bash
# Phase 0
python3 .agents/skills/backend-audit-skill/scripts/build_warnings.py
python3 .agents/skills/backend-audit-skill/scripts/dependency_scan.py
python3 .agents/skills/backend-audit-skill/scripts/code_metrics.py

# Phase 1-4: Architecture checks
python3 .agents/skills/backend-audit-skill/scripts/architecture_check.py <layer-path>

# Phase 5.1: Multi-tenancy
python3 .agents/skills/backend-audit-skill/scripts/tenant_security_scan.py

# Phase 5.5: Performance
python3 .agents/skills/backend-audit-skill/scripts/performance_scan.py

# Phase 6.1: Coverage gaps
python3 .agents/skills/backend-audit-skill/scripts/test_coverage_gaps.py
```

### Step 3: Manual Code Review

Read each file in the phase scope. For each file, check against the relevant checklist section from [references/backend-checklist.md](references/backend-checklist.md).

Record issues as:

```
- **[SEVERITY]** [CATEGORY] — [FILE:LINE]
  Issue: [description]
  Fix: [concrete code change]
```

Severities:

- `CRITICAL` — Security vulnerability, data leak, crash in production
- `MAJOR` — Bug, incorrect behavior, architectural violation
- `MINOR` — Code smell, inconsistency, maintainability concern
- `SUGGESTION` — Improvement opportunity, not a defect

### Step 4: Report

```markdown
## Backend Audit Report — Phase [X.Y]: [Name]

### Summary

- Files audited: N
- Issues: X critical, Y major, Z minor, W suggestions
- Phase health: [A/B/C/D/F]

### Critical Issues

[list]

### Major Issues

[list]

### Minor Issues & Suggestions

[list]

### Recommendations

[prioritized action items]
```

Scoring:

- **A** (0 critical, 0-2 major) — Clean
- **B** (0 critical, 3-5 major) — Acceptable
- **C** (1-2 critical OR 6+ major) — Needs work
- **D** (3+ critical) — Significant issues
- **F** (5+ critical) — Requires rework

### Step 5: Update Progress

Update `.agents/skills/backend-audit-skill/audit-progress.json`:

```json
{
  "phase": "1.1",
  "status": "completed",
  "score": "B",
  "issues": { "critical": 0, "major": 3, "minor": 5, "suggestion": 2 },
  "date": "2025-XX-XX",
  "fixesApplied": false
}
```

### Step 6: Implement Fixes (if `--fix` flag)

If `--fix` is specified:

1. Fix all `CRITICAL` issues automatically
2. Fix all `MAJOR` issues automatically
3. Present `MINOR` issues and ask before fixing
4. Re-run automated scripts to verify
5. Run `dotnet build` to confirm no regressions

---

## Resources

### scripts/

- `build_warnings.py` — Runs `dotnet build` and parses warnings
- `dependency_scan.py` — Checks NuGet packages for updates/vulnerabilities
- `code_metrics.py` — File counts, line counts, complexity overview
- `architecture_check.py` — Layer dependency violations, naming conventions
- `tenant_security_scan.py` — Scans for missing TenantId filters
- `performance_scan.py` — Detects N+1, sync-over-async, missing cancellation tokens
- `test_coverage_gaps.py` — Finds handlers/services without corresponding test files

### references/

- `backend-checklist.md` — Detailed heuristics for each audit category (read per-phase)
