# Frontend Audit Checklist

Heurystyki i red flags do manualnego code review per phase.

---

## 1. Core Layer

### 1.1 API Client & Interceptors

**Dobre praktyki:**
- Axios instance z base URL z env vars
- Auth interceptor dodaje `Authorization: Bearer` header
- Response interceptor unwrappuje `Result<T>` pattern
- Error interceptor loguje i transformuje błędy
- Retry logic dla transient failures (5xx)
- Request/response typing

**Red flags:**
- ❌ Hardcoded base URL: `axios.create({ baseURL: "http://localhost:5000" })`
- ❌ Token pobierany z localStorage zamiast z session
- ❌ Brak error handling w interceptorach
- ❌ `any` type na response/error
- ❌ Console.log w production code

**Thresholds:**
- Max 100 linii w client.ts
- 100% typed requests/responses

### 1.2 Providers

**Dobre praktyki:**
- QueryClientProvider z sensownym staleTime/gcTime
- Proper error boundaries na poziomie provider
- SessionProvider (NextAuth) jako zewnętrzny wrapper
- StripeProvider lazy loaded (tylko gdzie potrzebny)
- Correct nesting order: Session → Query → Theme → App

**Red flags:**
- ❌ QueryClient tworzone w komponencie (new on every render)
- ❌ Brak Suspense fallback
- ❌ StripeProvider na globalnym poziomie (bloats bundle)
- ❌ Missing error boundaries

### 1.3 Auth Config & Types

**Dobre praktyki:**
- NextAuth config z właściwymi callbacks
- Session augmented z role, tenantId, providerId
- JWT strategy z proper expiry
- Secure cookie settings w production
- Type-safe session via `next-auth.d.ts`

**Red flags:**
- ❌ Secrets w kodzie zamiast env vars
- ❌ Brak session types augmentation
- ❌ `any` na session.user
- ❌ Missing CSRF protection

### 1.4 Middleware

**Dobre praktyki:**
- Role-based route protection
- Redirect do login dla unauthenticated
- Redirect Provider → /dashboard, Client → /portal
- Matcher excludes: _next, static, api
- Efficient session check (nie pełny DB lookup)

**Red flags:**
- ❌ Brak middleware protection — CRITICAL!
- ❌ Role check via API call w middleware (slow!)
- ❌ Hardcoded routes w middleware
- ❌ Missing matcher exclusions

---

## 2. Shared Layer

### 2.1 UI Components (shadcn/ui)

**Dobre praktyki:**
- CVA (Class Variance Authority) dla wariantów
- `cn()` dla łączenia klas
- ForwardRef dla input-like components
- Proper ARIA attributes
- No business logic (purely presentational)
- Accessible focus states

**Red flags:**
- ❌ Business logic w UI component
- ❌ Hardcoded colors zamiast CSS variables
- ❌ Missing `aria-label` na icon buttons
- ❌ No keyboard support
- ❌ Inline styles zamiast Tailwind

**Thresholds:**
- Max 100 LOC per UI component
- 0 business logic w `shared/ui/`

### 2.2 Layout Components

**Dobre praktyki:**
- Responsive design (mobile-first)
- Sidebar collapse na mobile
- Proper semantic HTML (nav, main, aside)
- Skip-to-content link
- Consistent spacing via design tokens

**Red flags:**
- ❌ Fixed widths breaking mobile
- ❌ `display: none` zamiast responsive
- ❌ Missing mobile nav
- ❌ No landmarks (main, nav, aside)

### 2.3 Shared Hooks & Utils

**Dobre praktyki:**
- Custom hooks z proper deps array
- Memoization gdzie appropriate
- Pure utils (no side effects)
- Edge case handling
- TypeScript overloads dla flexibility

**Red flags:**
- ❌ Hook bez deps array
- ❌ Missing cleanup w useEffect
- ❌ Mutating state directly
- ❌ `any` return types

### 2.4 Skeletons & Error Components

**Dobre praktyki:**
- Skeleton matching actual content shape
- ErrorBoundary z fallback UI
- Retry button na error states
- Consistent animation (pulse)
- Empty state z action CTA

**Red flags:**
- ❌ Generic "Loading..." text
- ❌ No error boundary
- ❌ Error bez actionable next step
- ❌ Skeleton nie pasuje do contentu

---

## 3. Features — Auth & Guards

### 3.1 Login/Register Forms

**Dobre praktyki:**
- react-hook-form + zod validation
- Loading state podczas submit
- Error display per field + general
- Redirect po successful auth
- "Remember me" / session persistence
- Password visibility toggle

**Red flags:**
- ❌ Form bez walidacji
- ❌ Submit bez loading state
- ❌ Redirect przed zakończeniem auth
- ❌ Password w URL/query params
- ❌ No rate limiting awareness

### 3.2 TenantGuard & PortalGuard — CRITICAL SECURITY

**Dobre praktyki:**
- Session check na mount
- Loading state podczas verification
- Redirect do login jeśli no session
- Redirect do proper portal based on role
- No flash of protected content

**Red flags:**
- ❌ Render protected content before auth check — CRITICAL!
- ❌ Guard tylko na client, no middleware — CRITICAL!
- ❌ Race condition: content visible then redirect
- ❌ Missing loading skeleton during auth check

**Thresholds:**
- 100% protected routes mają Guard + middleware
- 0 flash of protected content

### 3.3 Session Management

**Dobre praktyki:**
- Token refresh przed expiry
- Logout cleanup (clear cache, redirect)
- Session sync across tabs
- Handle 401 globally (redirect to login)

**Red flags:**
- ❌ No token refresh = forced re-login
- ❌ Stale data after logout (cache not cleared)
- ❌ 401 not handled globally
- ❌ Session desync between tabs

---

## 4. Features — Business Domains

### 4.1-4.8 Feature Components (WSZYSTKIE muszą spełniać!)

**Dobre praktyki:**
- TYLKO Orval hooks do API calls
- Loading state (Skeleton)
- Error state (ErrorMessage + retry)
- Empty state (EmptyState + action)
- Mutation invaliduje relevant queries
- Form walidacja z Zod
- Toast feedback na success/error

**Red flags — CRITICAL:**
- ❌ Hardcoded data (`<p>0</p>`, `const data = []`)
- ❌ Raw fetch/axios zamiast Orval hooks
- ❌ Brak loading state
- ❌ Brak error state
- ❌ Brak empty state
- ❌ Mutation bez cache invalidation
- ❌ Toast bez prawdziwej operacji (fake success)
- ❌ `// TODO: implement` comments
- ❌ `console.log("TODO")`

**Thresholds:**
- 100% komponentów z danymi ma loading/error/empty states
- 0% hardcoded data (ZERO TOLERANCE)
- 0% raw fetch/axios (TYLKO Orval)
- 100% mutacji invalidują cache
- Max 200 LOC per component
- Max 8 props per component
- Max 7 hooks per component

### 4.4 Payments Feature (dodatkowe wymagania)

**Dobre praktyki:**
- Stripe Elements z proper styling
- PCI compliance (no card data w state)
- PaymentIntent flow z confirmation
- Proper error handling (decline, 3DS)
- Idempotency na payment operations

**Red flags:**
- ❌ Card data stored w React state — CRITICAL!
- ❌ PaymentIntent created on every render
- ❌ Missing 3DS handling
- ❌ No idempotency key

---

## 5. App Router & Pages

### 5.1 Route Structure & Layouts

**Dobre praktyki:**
- Route groups: `(auth)`, `(dashboard)`, `(portal)`, `(public)`
- Shared layout per group
- Proper metadata export
- Parallel routes gdzie needed
- Loading.tsx per route segment

**Red flags:**
- ❌ Flat route structure (no grouping)
- ❌ Missing layouts (duplicated wrappers)
- ❌ No metadata/SEO

### 5.2 Page Components

**Dobre praktyki:**
- Async params pattern (Next.js 15+):
  ```tsx
  export default async function Page({ params }: { params: Promise<{ id: string }> }) {
    const { id } = await params;
  }
  ```
- Server components gdzie możliwe
- "use client" tylko gdy needed
- Suspense boundaries

**Red flags:**
- ❌ `params.id` bez await — TypeError!
- ❌ "use client" na wszystkim
- ❌ Data fetching w client component (SSR benefit lost)

### 5.3 Error Pages & Loading States

**Dobre praktyki:**
- `error.tsx` z reset button
- `loading.tsx` z skeleton
- `not-found.tsx` z navigation
- Global error boundary w layout

**Red flags:**
- ❌ Missing error.tsx (unhandled crashes)
- ❌ Missing loading.tsx (no feedback)
- ❌ Generic 404 (no helpful navigation)

### 5.4 Root Layout & Globals

**Dobre praktyki:**
- Correct provider nesting order
- Font optimization (next/font)
- CSS variables w globals.css
- Dark mode support via class strategy
- Proper lang attribute

**Red flags:**
- ❌ Wrong provider order (Session must be outside Query)
- ❌ Font imports blocking render
- ❌ Hardcoded colors w globals (not themeable)
- ❌ Missing html lang

---

## 6. Cross-Cutting Concerns

### 6.1 Performance

**Dobre praktyki:**
- React Query z sensowne staleTime (5min+)
- useMemo/useCallback dla expensive ops
- Dynamic imports dla heavy components
- Image optimization (next/image)
- Code splitting per route
- Virtualization dla long lists

**Red flags:**
- ❌ Re-render na każde keystroke
- ❌ Inline objects/arrays jako props (new ref every render)
- ❌ Missing key na lists
- ❌ Import całego lodash (`import _ from "lodash"`)
- ❌ No lazy loading na route-level
- ❌ `staleTime: 0` (refetch on every focus)

**Thresholds:**
- Max 300KB initial JS bundle per route
- Max 3s LCP (Largest Contentful Paint)
- Max 100ms INP (Interaction to Next Paint)

### 6.2 Accessibility

**Dobre praktyki:**
- Semantic HTML (button, nav, main)
- ARIA labels na icon buttons
- Focus management na modals
- Skip links
- Color contrast ratios (WCAG AA)
- Keyboard navigation

**Red flags:**
- ❌ `<div onClick>` zamiast `<button>`
- ❌ Missing alt on images
- ❌ Missing htmlFor on labels
- ❌ Color-only indication (no icon/text)
- ❌ Focus trap not implemented on modals
- ❌ Missing :focus-visible styles

**Thresholds:**
- 100% images mają alt
- 100% interactive elements są keyboard accessible
- Color contrast minimum 4.5:1

### 6.3 Security

**Dobre praktyki:**
- No secrets w client code
- Sanitize user input (dangerouslySetInnerHTML)
- CSRF tokens on mutations
- Auth token in httpOnly cookie (not localStorage)
- Environment variables validation

**Red flags:**
- ❌ NEXT_PUBLIC_* z secrets — CRITICAL!
- ❌ localStorage for auth tokens
- ❌ dangerouslySetInnerHTML z user input
- ❌ No input sanitization

### 6.4 Error Handling Consistency

**Dobre praktyki:**
- Global error boundary
- Toast na mutation errors
- Inline errors na form fields
- Retry buttons na network errors
- Sentry/error reporting

**Red flags:**
- ❌ Swallowed errors (catch with no action)
- ❌ Inconsistent error UI
- ❌ No error reporting
- ❌ Generic "Something went wrong"

### 6.5 i18n Readiness

**Dobre praktyki:**
- No hardcoded user-facing strings
- Date/number formatowanie via Intl
- RTL consideration
- Locale-aware sorting

**Red flags:**
- ❌ Hardcoded strings: "Submit", "Cancel", "No results"
- ❌ `new Date().toLocaleDateString()` bez locale
- ❌ String concatenation dla plurals

---

## 7. Testing

### 7.1 Unit Test Coverage

**Targets:**
- >80% coverage na hooks i utils
- Każdy component z logiką ma test
- Edge cases tested (empty, error, loading)

**Red flags:**
- ❌ Component bez testów
- ❌ Only happy path tested
- ❌ No mock dla API calls

### 7.2 Test Quality

**Dobre praktyki:**
- Testing Library queries (getByRole > getByTestId)
- User-centric assertions
- Mock API calls (MSW lub Orval mocks)
- AAA pattern (Arrange, Act, Assert)
- Meaningful test names

**Red flags:**
- ❌ Implementation testing (testing internal state)
- ❌ `getByTestId` everywhere
- ❌ Snapshot tests bez review
- ❌ Tests depend on each other

### 7.3 E2E Tests

**Dobre praktyki:**
- Critical user journeys tested
- Login → CRUD → Logout flows
- Cross-browser testing
- Visual regression tests
- Proper test isolation (clean state)

**Red flags:**
- ❌ No E2E tests
- ❌ Flaky tests (pass/fail randomly)
- ❌ Tests share state
- ❌ Hardcoded waits (`page.waitForTimeout(5000)`)

### 7.4 Test Infrastructure

**Dobre praktyki:**
- Vitest config z coverage
- Playwright config z multiple browsers
- CI integration
- Test database/fixtures
- Proper mocking setup

**Red flags:**
- ❌ No CI integration
- ❌ Tests only run locally
- ❌ No coverage reporting
- ❌ Missing test utilities

---

## Thresholds Summary

| Metryka | Limit | Severity |
|---------|-------|----------|
| LOC per component | max 200 | MINOR |
| Props per component | max 8 | MINOR |
| Hooks per component | max 7 | MINOR |
| JSX nesting depth | max 5 levels | MINOR |
| Initial JS bundle per route | max 300KB | MAJOR |
| TypeScript `any` usage | max 10% | MAJOR |
| Components with data: loading state | 100% | MAJOR |
| Components with data: error state | 100% | MAJOR |
| Components with data: empty state | 100% | MAJOR |
| Hardcoded data | 0% | CRITICAL |
| Raw fetch/axios (not Orval) | 0% | CRITICAL |
| Protected routes without Guard | 0% | CRITICAL |
| Images without alt | 0% | MAJOR |
| Interactive elements keyboard accessible | 100% | MAJOR |
