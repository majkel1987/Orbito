# Prompt: Stwórz skill `frontend-audit` dla projektu Orbito

## Kontekst

Mam istniejący skill `backend-audit` w `.claude/skills/backend-audit/` — chcę stworzyć analogiczny skill `frontend-audit` dla frontendu mojej aplikacji SaaS (Orbito). Skill powinien mieć **identyczną strukturę i konwencje** co backend-audit, ale dostosowany do specyfiki frontendu.

## Lokalizacja

- **Skill**: `.claude/skills/frontend-audit/`
- **Frontend**: `orbito-frontend/` (root projektu frontendowego)

## Wzorzec: backend-audit

Backend-audit ma następującą strukturę — **odwzoruj ją 1:1**:

```
frontend-audit/
├── SKILL.md                    # Główny plik skilla (frontmatter + instrukcje)
├── QUICKSTART.md               # Krótki przewodnik użytkownika
├── audit-progress.json         # Tracking postępu (puste fazy, status: pending)
├── references/
│   └── frontend-checklist.md   # Heurystyki per faza (dobre praktyki + red flags + thresholds)
└── scripts/                    # Skrypty Python do automatycznych skanów
    ├── bundle_analysis.py
    ├── component_metrics.py
    ├── type_coverage.py
    ├── accessibility_scan.py
    ├── api_usage_audit.py
    ├── performance_scan.py
    └── test_coverage_gaps.py
```

## Tech Stack frontendu (WAŻNE — skill musi to znać)

| Kategoria | Technologia |
|-----------|------------|
| Framework | Next.js 16 (App Router, React 19) |
| Language | TypeScript 5 (strict mode) |
| Server State | TanStack React Query v5 |
| Client State | Zustand v5 |
| API Generation | **Orval** (generuje hooki z OpenAPI/Swagger) |
| Styling | Tailwind CSS 4 + shadcn/ui (new-york style) |
| Forms | React Hook Form + Zod 4 |
| Auth | NextAuth v5 (Credentials provider) |
| Icons | Lucide React |
| Toasts | Sonner |
| Charts | Recharts 3 |
| Dates | date-fns 4 |
| Payments | Stripe (@stripe/react-stripe-js) |
| Testing | Vitest 4 + Testing Library + Playwright |
| Linting | ESLint 9 (flat config) + jsx-a11y |
| Build analysis | @next/bundle-analyzer |

## Architektura frontendu

```
src/
├── app/                      # Next.js App Router
│   ├── (auth)/              # Login, Register (publiczne)
│   ├── (dashboard)/         # Panel Provider (TenantGuard)
│   ├── (portal)/            # Panel Client (PortalGuard)
│   └── (public)/            # Publiczne strony (invite)
├── core/                     # Infrastruktura
│   ├── api/
│   │   ├── client.ts        # Axios instance + interceptors (auth, Result<T> unwrap)
│   │   └── generated/       # 🤖 Orval hooks (NIE EDYTOWAĆ!)
│   ├── providers/           # QueryProvider, AuthInterceptorProvider, StripeProvider
│   └── types/               # next-auth.d.ts (augmentacja sesji)
├── features/                 # Vertical Slices (auth, clients, plans, subscriptions, payments, team, analytics, billing, client-portal)
│   └── {feature}/
│       ├── components/
│       ├── hooks/
│       ├── schemas/         # Zod schemas
│       └── types/
├── shared/                   # Reużywalne, bezstanowe
│   ├── ui/                  # shadcn/ui components
│   ├── components/          # layout/ (DashboardShell, Sidebar, Header), Pagination, ErrorBoundary, Skeletons
│   ├── hooks/               # useToast
│   └── lib/                 # utils.ts (cn()), formatters.ts
├── middleware.ts             # Auth + role-based routing
└── test/                     # Vitest setup + test utils
```

## Role użytkowników i routing

- **Provider/TeamMember** → `/dashboard/*` (TenantGuard)
- **Client** → `/portal/*` (PortalGuard)
- Middleware w `middleware.ts` wymusza routing na podstawie roli z JWT

## Kluczowe patterny w kodzie (skill MUSI je znać i audytować)

### 1. Obowiązkowy pattern dla komponentów z danymi
```tsx
"use client";
const { data, isLoading, error } = useGetApiXxx();
if (isLoading) return <Skeleton />;
if (error) return <ErrorMessage />;
if (!data?.length) return <EmptyState />;
return <RealDataUI data={data} />;
```

### 2. Pattern mutacji
```tsx
const mutation = usePostApiXxx({
  mutation: {
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [...] });
      toast.success("...");
    },
    onError: (error) => toast.error(error.message),
  },
});
```

### 3. Pattern formularzy
```tsx
const form = useForm({ resolver: zodResolver(Schema), defaultValues: {...} });
```

### 4. URL State Management
- Filtry i paginacja trzymane w URL search params (useSearchParams + useRouter)

### 5. ABSOLUTNE ZAKAZY (z API_RULES.md)
- ❌ Hardcoded dane (np. `<p>0</p>`)
- ❌ Puste tablice zamiast danych z API
- ❌ Mock funkcje, `console.log("TODO")`
- ❌ Komentarze `// TODO: add later`
- ❌ Fake success (toast bez prawdziwej operacji)
- ❌ Ręczne `fetch()` lub `axios.get()` — TYLKO hooki Orval

## Fazy audytu (ZAPROPONUJ — wzorując się na backend-audit)

Zaproponuj **7 faz** z sub-fazami, pokrywając:

### PHASE 0: Health Check (automatyczne skrypty)
- 0.1: Build & TypeScript errors (`npm run typecheck`, `npm run build`)
- 0.2: Lint & Accessibility warnings (`npm run lint`, jsx-a11y)
- 0.3: Bundle size analysis (`@next/bundle-analyzer`)
- 0.4: Dependency audit (`npm audit`, outdated packages)

### PHASE 1: Core Layer (`src/core/`)
- 1.1: API Client & Interceptors (`client.ts` — error handling, Result<T> unwrap, auth header)
- 1.2: Providers (QueryProvider, AuthInterceptorProvider, StripeProvider — konfiguracja, error boundaries)
- 1.3: Auth config & types (next-auth setup, session augmentation, token refresh)
- 1.4: Middleware (route protection, role checks, redirect logic)

### PHASE 2: Shared Layer (`src/shared/`)
- 2.1: UI Components (shadcn/ui — zgodność z CVA, accessibility, no business logic)
- 2.2: Layout Components (DashboardShell, Sidebar, Header — responsywność, mobile)
- 2.3: Shared Hooks & Utils (useToast, formatters, cn() — testowanie, edge cases)
- 2.4: Skeletons & Error Components (loading states consistency, ErrorBoundary coverage)

### PHASE 3: Features — Auth & Guards (`src/features/auth/`)
- 3.1: Login/Register forms (walidacja, error handling, redirect po auth)
- 3.2: TenantGuard & PortalGuard (security, race conditions, loading states)
- 3.3: Session management (token refresh, expiry handling, logout cleanup)

### PHASE 4: Features — Business Domains (podziel na sub-fazy per feature!)
- 4.1: Clients feature (components, hooks, schemas, types)
- 4.2: Plans feature
- 4.3: Subscriptions feature
- 4.4: Payments feature (+ Stripe integration!)
- 4.5: Team feature
- 4.6: Analytics feature (charts, data visualization)
- 4.7: Billing feature (TrialBanner, SubscriptionExpiredOverlay)
- 4.8: Client Portal feature

### PHASE 5: App Router & Pages (`src/app/`)
- 5.1: Route structure & layouts (route groups, nested layouts, metadata)
- 5.2: Page components (async params pattern Next.js 15+, server vs client)
- 5.3: Error pages & loading states (error.tsx, loading.tsx, not-found.tsx)
- 5.4: Root layout & globals (providers nesting order, CSS variables, fonts)

### PHASE 6: Cross-Cutting Concerns
- 6.1: Performance (React Query caching, re-renders, memo usage, suspense)
- 6.2: Accessibility (ARIA, keyboard nav, focus management, color contrast)
- 6.3: Security (XSS prevention, auth token handling, CSRF, env vars exposure)
- 6.4: Error handling consistency (error boundaries, toast patterns, retry logic)
- 6.5: Internationalization readiness (hardcoded strings audit, locale handling)

### PHASE 7: Testing
- 7.1: Unit test coverage (components, hooks, utils — gaps analysis)
- 7.2: Test quality (assertions, edge cases, mocking patterns)
- 7.3: E2E tests (critical paths: login → CRUD → logout)
- 7.4: Test infrastructure (setup, fixtures, CI integration)

## Wymagania dla SKILL.md

1. **Frontmatter**: `name: frontend-audit`, `description:` z triggerami ("run frontend-audit", "audit frontend", "audit phase X")
2. **Token Management**: ONE phase per session, max 15 plików per sub-faza (auto-split jeśli więcej), update progress po każdej fazie
3. **Komendy**:
   - `run frontend-audit --phase list` — pokaż fazy i status
   - `run frontend-audit --phase 1.1` — uruchom konkretną fazę
   - `run frontend-audit --phase 1.1 --fix` — audyt + automatyczne poprawki
   - `run frontend-audit --path src/features/clients/` — audytuj konkretną ścieżkę
4. **Workflow** (6 kroków jak w backend):
   1. Scope check (policz pliki .ts/.tsx, auto-split)
   2. Uruchom skrypty automatyczne
   3. Manual code review z heurystykami z checklist
   4. Wygeneruj raport (format issue jak w backend)
   5. Update audit-progress.json
   6. Apply fixes (jeśli --fix)
5. **Issue format**: `[SEVERITY] [CATEGORY] — [FILE:LINE]` + Issue + Fix
6. **Severity levels**: CRITICAL, MAJOR, MINOR, SUGGESTION
7. **Grading**: A-F (jak w backend)

## Wymagania dla skryptów Python

Każdy skrypt powinien:
- Przyjmować `--path` (domyślnie `src/`)
- Zwracać JSON z wynikami
- Być uruchamialny standalone (`python scripts/xxx.py --path src/features/clients/`)

### Opisy skryptów:

1. **bundle_analysis.py** — Analizuje rozmiar bundla, szuka: dużych importów, brak tree-shaking, duplikatów bibliotek, importy z `node_modules` które powinny być code-split
2. **component_metrics.py** — Mierzy: LOC per komponent (max 200), ilość props (max 8), głębokość zagnieżdżenia JSX (max 5), ilość hooków per komponent (max 7), re-export patterns
3. **type_coverage.py** — Szuka: `any` usage, `as` type assertions, `@ts-ignore`/`@ts-expect-error`, brak typów na props, implicit `any` w event handlers
4. **accessibility_scan.py** — Sprawdza: brak `alt` na img, brak `aria-label` na interaktywnych elementach, brak `htmlFor` na label, color contrast (hardcoded colors vs CSS vars), keyboard navigation patterns
5. **api_usage_audit.py** — KRYTYCZNY! Sprawdza: czy używane są TYLKO hooki Orval (nie raw fetch/axios), czy KAŻDY komponent z danymi ma loading/error/empty state, czy mutacje invalidują cache, czy nie ma hardcoded danych
6. **performance_scan.py** — Sprawdza: brak `useMemo`/`useCallback` przy ciężkich operacjach, inline object/array props (re-render triggers), brak `key` na listach, duże re-renders (komponent z wieloma hookami), brak lazy loading na route-level
7. **test_coverage_gaps.py** — Analizuje: które komponenty/hooki/utils nie mają testów, ratio testów do kodu, brak testów dla edge cases (error, empty, loading)

## Wymagania dla frontend-checklist.md

Dla każdej fazy zdefiniuj:
- **Dobre praktyki** (co POWINNO być)
- **Red flags** (co jest BŁĘDEM)
- **Thresholds** (limity numeryczne)

Przykładowe thresholds:
- Max 200 LOC per komponent
- Max 8 props per komponent
- Max 7 hooków per komponent
- Max 5 poziomów zagnieżdżenia JSX
- Max 3 odpowiedzialności per komponent (SRP)
- 100% komponentów z danymi musi mieć loading/error/empty states
- 0% hardcoded danych (ZERO TOLERANCE)
- 0% raw fetch/axios (TYLKO Orval hooki)
- Max 300KB initial JS bundle per route
- Min 90% TypeScript strict compliance (no `any`)

## Wymagania dla audit-progress.json

Identyczna struktura jak w backend-audit, ale z fazami frontendowymi:

```json
{
  "projectName": "Orbito Frontend",
  "lastUpdated": "",
  "phases": [
    {
      "id": "0.1",
      "phase": "PHASE 0: Health Check",
      "block": "Build & TypeScript",
      "target": "orbito-frontend/",
      "script": "scripts/bundle_analysis.py",
      "status": "pending",
      "score": null,
      "issues": { "critical": 0, "major": 0, "minor": 0, "suggestion": 0 },
      "date": null,
      "fixesApplied": false,
      "notes": []
    }
  ]
}
```

Wypełnij WSZYSTKIE fazy i sub-fazy z sekcji "Fazy audytu" powyżej (0.1-0.4, 1.1-1.4, 2.1-2.4, 3.1-3.3, 4.1-4.8, 5.1-5.4, 6.1-6.5, 7.1-7.4) — każda jako osobny obiekt w tablicy `phases`.

## Dodatkowe uwagi

1. **Orval generated code** (`src/core/api/generated/`) — NIGDY nie audytuj tego katalogu (jest auto-generowany). Ale audytuj CZY jest używany poprawnie w features.
2. **shadcn/ui components** (`src/shared/ui/`) — audytuj tylko czy nie zawierają business logic, czy są zgodne z CVA pattern.
3. **Known backend workarounds** — skill powinien wiedzieć o: Result<T> unwrapping, `clients→items` mapping, billingPeriod string→enum, CustomInstance<void> type assertion bug.
4. **Multi-tenancy na froncie** — TenantGuard, PortalGuard, middleware role checks — to jest CRITICAL security area.
5. **Język raportów**: Polski (jak w CLAUDE.md projektu).
