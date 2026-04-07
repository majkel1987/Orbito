---
name: frontend-audit
description: >
  Kompleksowy audyt frontendu dla Next.js 16 + React 19 + TypeScript.
  Używaj gdy użytkownik mówi "run frontend-audit", "audit frontend", "audit phase X",
  lub chce przejrzeć/poprawić jakość kodu frontendowego.
  Obsługuje przyrostowe fazy jak --phase 1.1.
---

# Frontend Audit Skill

## Cel skilla

Audyt frontendu Orbito — Next.js 16 (App Router), React 19, TypeScript strict, TanStack Query v5, Zustand v5, Orval (generowane hooki API), Tailwind CSS 4 + shadcn/ui, React Hook Form + Zod 4.

## Tech Stack

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

## Token Management — KRYTYCZNE ZASADY

1. **JEDEN phase na sesję** — nigdy nie rób wielu phases w jednej konwersacji
2. **Czytaj TYLKO pliki z bieżącego phase** — nie eksploruj całego projektu
3. **Po każdym phase aktualizuj audit-progress.json**
4. **Jeśli phase ma >15 plików .ts/.tsx** — podziel na sub-phases (np. 4.1a, 4.1b) i zapytaj użytkownika

## Komendy

```bash
# Lista wszystkich phases i ich status
run frontend-audit --phase list

# Audyt konkretnego phase
run frontend-audit --phase 1.1

# Audyt z auto-fixem krytycznych/major issues
run frontend-audit --phase 1.1 --fix

# Audyt konkretnej ścieżki
run frontend-audit --path src/features/clients/
```

## Phases

**PHASE 0: Health Check (automatyczne skany)**

| Block | Target | Skrypt |
|-------|--------|--------|
| 0.1 | Build & TypeScript | `npm run typecheck && npm run build` |
| 0.2 | Lint & Accessibility | `npm run lint` |
| 0.3 | Bundle Size | `scripts/bundle_analysis.py` |
| 0.4 | Dependency Audit | `npm audit` |

**PHASE 1: Core Layer (`src/core/`)**

| Block | Target | Est. Files |
|-------|--------|------------|
| 1.1 | API Client & Interceptors | ~3 |
| 1.2 | Providers | ~5 |
| 1.3 | Auth Config & Types | ~4 |
| 1.4 | Middleware | ~2 |

**PHASE 2: Shared Layer (`src/shared/`)**

| Block | Target | Est. Files |
|-------|--------|------------|
| 2.1 | UI Components (shadcn/ui) | ~20 |
| 2.2 | Layout Components | ~6 |
| 2.3 | Shared Hooks & Utils | ~8 |
| 2.4 | Skeletons & Error Components | ~5 |

**PHASE 3: Features — Auth & Guards (`src/features/auth/`)**

| Block | Target | Est. Files |
|-------|--------|------------|
| 3.1 | Login/Register Forms | ~8 |
| 3.2 | TenantGuard & PortalGuard | ~4 |
| 3.3 | Session Management | ~3 |

**PHASE 4: Features — Business Domains**

| Block | Target | Est. Files |
|-------|--------|------------|
| 4.1 | Clients Feature | ~15 |
| 4.2 | Plans Feature | ~10 |
| 4.3 | Subscriptions Feature | ~15 |
| 4.4 | Payments Feature (+ Stripe!) | ~20 |
| 4.5 | Team Feature | ~12 |
| 4.6 | Analytics Feature | ~10 |
| 4.7 | Billing Feature | ~8 |
| 4.8 | Client Portal Feature | ~15 |

**PHASE 5: App Router & Pages (`src/app/`)**

| Block | Target | Est. Files |
|-------|--------|------------|
| 5.1 | Route Structure & Layouts | ~10 |
| 5.2 | Page Components | ~20 |
| 5.3 | Error Pages & Loading States | ~8 |
| 5.4 | Root Layout & Globals | ~4 |

**PHASE 6: Cross-Cutting Concerns**

| Block | Target | Skrypt/Narzędzie |
|-------|--------|-----------------|
| 6.1 | Performance | `scripts/performance_scan.py` |
| 6.2 | Accessibility | `scripts/accessibility_scan.py` |
| 6.3 | Security | grep scan |
| 6.4 | Error Handling Consistency | grep scan |
| 6.5 | i18n Readiness | grep scan |

**PHASE 7: Testing**

| Block | Target | Skrypt |
|-------|--------|--------|
| 7.1 | Unit Test Coverage | `scripts/test_coverage_gaps.py` |
| 7.2 | Test Quality | sample review |
| 7.3 | E2E Tests | Playwright review |
| 7.4 | Test Infrastructure | setup review |

## Kluczowe Patterny — AUDYT MUSI JE SPRAWDZAĆ

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

### 5. ABSOLUTNE ZAKAZY (ZERO TOLERANCE)

- ❌ Hardcoded dane (np. `<p>0</p>`)
- ❌ Puste tablice zamiast danych z API
- ❌ Mock funkcje, `console.log("TODO")`
- ❌ Komentarze `// TODO: add later`
- ❌ Fake success (toast bez prawdziwej operacji)
- ❌ Ręczne `fetch()` lub `axios.get()` — TYLKO hooki Orval

## Workflow wykonania phase

Gdy użytkownik uruchamia phase, wykonaj PO KOLEI:

**Krok 1: Scope Check**

```bash
find <target-path> -name "*.ts" -o -name "*.tsx" | grep -v node_modules | grep -v .next | wc -l
```

Jeśli >15 plików — STOP, zapytaj użytkownika o podział.

**Krok 2: Uruchom skrypty automatyczne**

Odpowiedni skrypt z `scripts/` dla danego phase. Skrypty przyjmują ścieżkę do frontend root jako argument.

**Krok 3: Manualny code review**

Przeczytaj każdy plik w scope danego phase. Sprawdź heurystyki z `references/frontend-checklist.md` (czytaj TYLKO sekcję dla bieżącego phase).

Zapisuj issues w formacie:

```
- **[SEVERITY]** [CATEGORY] — [FILE:LINE]
  Issue: [opis]
  Fix: [konkretna zmiana kodu]
```

Severity:
- `CRITICAL` — luka bezpieczeństwa, hardcoded dane, brak auth check
- `MAJOR` — brak loading/error state, złe API usage, accessibility fail
- `MINOR` — code smell, niespójność, brak optymalizacji
- `SUGGESTION` — propozycja ulepszenia

**Krok 4: Raport**

```markdown
## Frontend Audit Report — Phase [X.Y]: [Name]

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
5. Uruchom `npm run typecheck && npm run build` żeby potwierdzić brak regresji

## Komenda --phase list

Gdy użytkownik wpisze `--phase list`:
1. Przeczytaj `audit-progress.json`
2. Wyświetl tabelkę ze statusem każdego phase (pending/completed/in-progress)
3. Pokaż overall progress (X/N phases completed)

## Znane problemy projektu (Known Workarounds)

- **Result<T> unwrapping** — backend zwraca `{ isSuccess, value, error }`, interceptor unwrappuje
- **clients→items mapping** — niektóre endpointy zwracają `items` zamiast bezpośredniej tablicy
- **billingPeriod string→enum** — mapowanie stringów na enum w walidacji
- **CustomInstance<void> type assertion bug** — workaround w kliencie Axios

## Ważne uwagi

1. **`src/core/api/generated/`** — NIGDY nie audytuj tego katalogu (auto-generowany przez Orval). Ale audytuj CZY jest używany poprawnie w features.
2. **`src/shared/ui/`** — audytuj tylko czy nie zawierają business logic, czy są zgodne z CVA pattern.
3. **Multi-tenancy** — TenantGuard, PortalGuard, middleware role checks — to jest CRITICAL security area.
4. **Język raportów**: Polski.
