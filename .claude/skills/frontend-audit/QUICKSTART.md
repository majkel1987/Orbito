# Frontend Audit Skill — Quick Start Guide

## Instalacja

Skill jest już zainstalowany w `.claude/skills/frontend-audit/`.

## Jak używać

```bash
# Sprawdź status wszystkich phases
run frontend-audit --phase list

# Uruchom konkretny phase
run frontend-audit --phase 1.1

# Uruchom z automatycznymi fixami
run frontend-audit --phase 1.1 --fix

# Audyt konkretnej ścieżki
run frontend-audit --path src/features/clients/
```

## Zalecana kolejność audytu (8 sprintów)

### Sprint 1: Health Check
- `--phase 0.1` Build & TypeScript errors
- `--phase 0.2` Lint & Accessibility warnings
- `--phase 0.3` Bundle size analysis
- `--phase 0.4` Dependency audit

### Sprint 2: Core Layer
- `--phase 1.1` API Client & Interceptors
- `--phase 1.2` Providers
- `--phase 1.3` Auth Config & Types
- `--phase 1.4` Middleware

### Sprint 3: Shared Layer
- `--phase 2.1` UI Components (shadcn/ui)
- `--phase 2.2` Layout Components
- `--phase 2.3` Shared Hooks & Utils
- `--phase 2.4` Skeletons & Error Components

### Sprint 4: Auth & Guards
- `--phase 3.1` Login/Register Forms
- `--phase 3.2` TenantGuard & PortalGuard (KRYTYCZNE!)
- `--phase 3.3` Session Management

### Sprint 5: Business Features (Part 1)
- `--phase 4.1` Clients Feature
- `--phase 4.2` Plans Feature
- `--phase 4.3` Subscriptions Feature
- `--phase 4.4` Payments Feature (+ Stripe!)

### Sprint 6: Business Features (Part 2)
- `--phase 4.5` Team Feature
- `--phase 4.6` Analytics Feature
- `--phase 4.7` Billing Feature
- `--phase 4.8` Client Portal Feature

### Sprint 7: App Router & Pages
- `--phase 5.1` Route Structure & Layouts
- `--phase 5.2` Page Components
- `--phase 5.3` Error Pages & Loading States
- `--phase 5.4` Root Layout & Globals

### Sprint 8: Cross-Cutting & Tests
- `--phase 6.1` Performance
- `--phase 6.2` Accessibility (KRYTYCZNE!)
- `--phase 6.3` Security
- `--phase 6.4` Error Handling Consistency
- `--phase 6.5` i18n Readiness
- `--phase 7.1` Unit Test Coverage
- `--phase 7.2` Test Quality
- `--phase 7.3` E2E Tests
- `--phase 7.4` Test Infrastructure

## Tips

1. **Jeden phase na sesję** — nie próbuj robić wielu phases w jednej konwersacji
2. **Używaj `--phase list`** — żeby sprawdzić postęp
3. **Duże phases (>15 plików)** — będą automatycznie podzielone na sub-phases
4. **Po każdym phase** — sprawdź `audit-progress.json` dla pełnej historii
5. **`--fix` flag** — automatycznie naprawia CRITICAL i MAJOR issues
6. **`src/core/api/generated/`** — NIGDY nie edytuj (auto-generowane)

## Struktura plików

```
.claude/skills/frontend-audit/
├── SKILL.md                      # Główna definicja skilla
├── QUICKSTART.md                 # Ten plik
├── audit-progress.json           # Tracking postępu
├── scripts/
│   ├── bundle_analysis.py        # Analizuje rozmiar bundla
│   ├── component_metrics.py      # Metryki komponentów (LOC, props, hooks)
│   ├── type_coverage.py          # Wykrywa `any`, `as`, `@ts-ignore`
│   ├── accessibility_scan.py     # Skanuje a11y issues
│   ├── api_usage_audit.py        # Sprawdza użycie hooków Orval
│   ├── performance_scan.py       # Wykrywa performance anti-patterns
│   └── test_coverage_gaps.py     # Znajduje brakujące testy
└── references/
    └── frontend-checklist.md     # Heurystyki per phase
```

## Severity Levels

| Level | Opis | Przykład |
|-------|------|----------|
| CRITICAL | Security, hardcoded data, auth bypass | Brak TenantGuard na dashboard route |
| MAJOR | Missing states, wrong API usage | Komponent bez loading/error state |
| MINOR | Code smell, inconsistency | Brak useMemo na ciężkiej operacji |
| SUGGESTION | Nice to have | Extract component refactoring |

## Scoring

| Grade | Criteria |
|-------|----------|
| A | 0 critical, 0-2 major |
| B | 0 critical, 3-5 major |
| C | 1-2 critical OR 6+ major |
| D | 3+ critical |
| F | 5+ critical |

## Krytyczne patterny do sprawdzania

### Komponenty z danymi MUSZĄ mieć:

```tsx
// ✅ DOBRZE
const { data, isLoading, error } = useGetApiClients();
if (isLoading) return <Skeleton />;
if (error) return <ErrorMessage error={error} />;
if (!data?.length) return <EmptyState />;
return <ClientsList clients={data} />;

// ❌ ŹLE - brak stanów
const { data } = useGetApiClients();
return <ClientsList clients={data} />; // CRASH jeśli data undefined!
```

### Mutacje MUSZĄ invalidować cache:

```tsx
// ✅ DOBRZE
const mutation = usePostApiClients({
  mutation: {
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["api", "clients"] });
      toast.success("Client created!");
    },
  },
});

// ❌ ŹLE - brak invalidacji = stale data
const mutation = usePostApiClients();
```

### API calls TYLKO przez Orval:

```tsx
// ✅ DOBRZE
import { useGetApiClients } from "@/core/api/generated/clients/clients";

// ❌ ŹLE - ZERO TOLERANCE
const response = await fetch("/api/clients");
const response = await axios.get("/api/clients");
```

## Thresholds

| Metryka | Limit |
|---------|-------|
| LOC per komponent | max 200 |
| Props per komponent | max 8 |
| Hooks per komponent | max 7 |
| Zagnieżdżenie JSX | max 5 poziomów |
| Initial JS bundle per route | max 300KB |
| TypeScript strict compliance | min 90% (no `any`) |
| Loading/error/empty states | 100% komponentów z danymi |
| Hardcoded data | 0% (ZERO TOLERANCE) |
| Raw fetch/axios | 0% (TYLKO Orval hooki) |
