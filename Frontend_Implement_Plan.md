# Frontend Implementation Plan v6.0 - Szczegółowa Lista Zadań

**Wersja**: 6.0 (Fresh Start)  
**Data**: 2025-11-29  
**Łączna liczba zadań**: ~95 zadań  
**Szacowany czas**: 10-12 tygodni (MVP)

---

## 📋 Instrukcja Użycia

- Każde zadanie oznacz jako ✅ po ukończeniu
- Zadania z 🔴 są **krytyczne** i blokują dalsze kroki
- Zadania z 🟡 są **ważne** ale nie blokujące
- Zadania z 🟢 można pominąć w MVP
- Każde zadanie to potencjalny osobny prompt dla agenta AI
- Zadania są pogrupowane w logiczne bloki do wykonania w jednej sesji

---

## 📊 Status Postępu

> **Ostatnia aktualizacja**: 2026-02-26 (na podstawie audytu kodu)

| Faza                        | Status           | Zadania | Ukończone        |
| --------------------------- | ---------------- | ------- | ---------------- |
| **FAZA 0: Setup**           | ✅ Ukończona     | 12      | 12/12            |
| **FAZA 1: Auth + Tenant**   | ✅ Ukończona     | 14      | 14/14            |
| **FAZA 2: Layout**          | ✅ Ukończona     | 10      | 10/10            |
| **FAZA 3: Team**            | ✅ Ukończona     | 9       | 9/9              |
| **FAZA 4A: Clients**        | ✅ Ukończona     | 14      | 14/14            |
| **FAZA 4B: Plans**          | ✅ Ukończona     | 12      | 12/12            |
| **FAZA 5: Subscriptions**   | ✅ Ukończona     | 10      | 10/10            |
| **FAZA 6: Payments**        | ✅ Ukończona     | 8       | 8/8              |
| **FAZA 7: Analytics**       | ✅ Ukończona     | 6       | 6/6              |
| **FAZA 8.1: Testing**       | ✅ Ukończona     | 5       | 5/5              |
| **FAZA 8.2: Polish**        | ⏳ Oczekuje      | 5       | 0/5              |
| **FAZA 9: Client Portal**   | ⏳ Oczekuje      | 5       | 0/5              |
| **ŁĄCZNIE**                 |                  | **100** | **90/100 (90%)** |

---

## 🔵 FAZA 0: Setup & Configuration (Tydzień 1)

### 0.1 Inicjalizacja Projektu

| #     | Zadanie                              | Priorytet | Status | Opis                                                                |
| ----- | ------------------------------------ | --------- | ------ | ------------------------------------------------------------------- |
| 0.1.1 | 🔴 Utworzenie projektu Next.js 15    | Krytyczne | ✅     | `create-next-app` z TypeScript, Tailwind, App Router, src directory |
| 0.1.2 | 🔴 Konfiguracja tsconfig.json strict | Krytyczne | ✅     | strict: true, allowJs: false, wszystkie strict\* opcje              |
| 0.1.3 | 🔴 Struktura katalogów               | Krytyczne | ✅     | Utworzenie features/, shared/, core/ zgodnie z planem               |

**Blok 0.1 - Wymagania wejściowe**: Brak  
**Blok 0.1 - Rezultat**: Działający projekt Next.js z TypeScript strict

---

### 0.2 Stylowanie i UI Kit

| #     | Zadanie                        | Priorytet | Status | Opis                                                                      |
| ----- | ------------------------------ | --------- | ------ | ------------------------------------------------------------------------- |
| 0.2.1 | 🔴 Konfiguracja Tailwind CSS   | Krytyczne | ✅     | tailwind.config.ts z custom colors, fonts                                 |
| 0.2.2 | 🔴 Inicjalizacja shadcn/ui     | Krytyczne | ✅     | `npx shadcn@latest init`, konfiguracja components.json                    |
| 0.2.3 | 🔴 Import bazowych komponentów | Krytyczne | ✅     | Button, Input, Card, Dialog, DropdownMenu, Select, Badge, Skeleton, Toast |
| 0.2.4 | 🟡 Utility functions           | Ważne     | ✅     | cn() helper, formatters (currency, date)                                  |

**Blok 0.2 - Wymagania wejściowe**: Blok 0.1  
**Blok 0.2 - Rezultat**: Gotowy UI kit z shadcn/ui

---

### 0.3 API Layer Setup

| #     | Zadanie                         | Priorytet | Status | Opis                                                        |
| ----- | ------------------------------- | --------- | ------ | ----------------------------------------------------------- |
| 0.3.1 | 🔴 Instalacja orval             | Krytyczne | ✅     | `npm install -D orval`                                      |
| 0.3.2 | 🔴 Konfiguracja orval.config.ts | Krytyczne | ✅     | Input: swagger.json, output: generated/, react-query client |
| 0.3.3 | 🔴 Axios client setup           | Krytyczne | ✅     | src/core/api/client.ts - bazowa instancja z baseURL         |
| 0.3.4 | 🔴 Result<T> interceptor        | Krytyczne | ✅     | Rozpakowywanie Result<T>, mapowanie błędów                  |
| 0.3.5 | 🔴 Pierwsze generowanie API     | Krytyczne | ✅     | `npm run api:generate` - weryfikacja że działa              |

**Blok 0.3 - Wymagania wejściowe**: Blok 0.1, działający backend ze Swagger  
**Blok 0.3 - Rezultat**: Wygenerowane typy i hooki z backendu

---

## 🔵 FAZA 1: Auth + Tenant Context (Tydzień 2-3)

### 1.1 NextAuth Configuration

| #     | Zadanie                         | Priorytet | Status | Opis                                                 |
| ----- | ------------------------------- | --------- | ------ | ---------------------------------------------------- |
| 1.1.1 | 🔴 Instalacja NextAuth v5       | Krytyczne | ✅     | `npm install next-auth@beta`                         |
| 1.1.2 | 🔴 auth.config.ts               | Krytyczne | ✅     | Credentials provider, JWT callback, session callback |
| 1.1.3 | 🔴 API route [...nextauth]      | Krytyczne | ✅     | src/app/api/auth/[...nextauth]/route.ts              |
| 1.1.4 | 🔴 Rozszerzenie typów next-auth | Krytyczne | ✅     | next-auth.d.ts z tenantId, teamRole, teamMemberId    |
| 1.1.5 | 🔴 AuthProvider wrapper         | Krytyczne | ✅     | SessionProvider w root layout                        |

**Blok 1.1 - Wymagania wejściowe**: Faza 0  
**Blok 1.1 - Rezultat**: Działająca konfiguracja NextAuth

---

### 1.2 Tenant Context

| #     | Zadanie                  | Priorytet | Status | Opis                                              |
| ----- | ------------------------ | --------- | ------ | ------------------------------------------------- |
| 1.2.1 | 🔴 TenantProvider        | Krytyczne | ✅     | Context z tenantId, teamRole, hasAccess()         |
| 1.2.2 | 🔴 useTenant hook        | Krytyczne | ✅     | Custom hook do pobierania kontekstu               |
| 1.2.3 | 🔴 TenantGuard component | Krytyczne | ✅     | Wrapper sprawdzający uprawnienia                  |
| 1.2.4 | 🟡 TenantSwitcher        | Ważne     | ✅     | UI do przełączania tenantów (jeśli user ma wiele) |

**Blok 1.2 - Wymagania wejściowe**: Blok 1.1  
**Blok 1.2 - Rezultat**: Działający kontekst tenanta

---

### 1.3 Auth Store i Middleware

| #     | Zadanie                | Priorytet | Status | Opis                                                   |
| ----- | ---------------------- | --------- | ------ | ------------------------------------------------------ |
| 1.3.1 | 🔴 authStore (Zustand) | Krytyczne | ✅     | Store z user, isAuthenticated, login/logout actions    |
| 1.3.2 | 🔴 middleware.ts       | Krytyczne | ✅     | Protected routes, redirect to login, tenant validation |
| 1.3.3 | 🟡 Auth sync           | Ważne     | ✅     | Synchronizacja NextAuth session z Zustand store        |

**Blok 1.3 - Wymagania wejściowe**: Blok 1.1, 1.2  
**Blok 1.3 - Rezultat**: Ochrona tras i stan autentykacji

---

### 1.4 Auth UI Pages

| #     | Zadanie                   | Priorytet | Status | Opis                                           |
| ----- | ------------------------- | --------- | ------ | ---------------------------------------------- |
| 1.4.1 | 🔴 Login page             | Krytyczne | ✅     | /login - formularz z walidacją Zod             |
| 1.4.2 | 🔴 LoginForm component    | Krytyczne | ✅     | React Hook Form, error handling, loading state |
| 1.4.3 | 🔴 Register page          | Krytyczne | ✅     | /register - rejestracja providera              |
| 1.4.4 | 🔴 RegisterForm component | Krytyczne | ✅     | Formularz z walidacją, role selection          |
| 1.4.5 | 🟡 Auth error page        | Ważne     | ✅     | /auth/error - obsługa błędów auth              |

**Blok 1.4 - Wymagania wejściowe**: Blok 1.1, 1.3  
**Blok 1.4 - Rezultat**: Działające strony logowania i rejestracji

---

## 🔵 FAZA 2: Layout & Global UI (Tydzień 3-4)

### 2.1 Layout Components

| #     | Zadanie               | Priorytet  | Status | Opis                                               |
| ----- | --------------------- | ---------- | ------ | -------------------------------------------------- |
| 2.1.1 | 🔴 Dashboard layout   | Krytyczne  | ✅     | (dashboard)/layout.tsx z Sidebar + Header          |
| 2.1.2 | 🔴 Sidebar component  | Krytyczne  | ✅     | Nawigacja z ikonami, active state, role-based menu |
| 2.1.3 | 🔴 Header component   | Krytyczne  | ✅     | Logo, UserMenu, notifications placeholder          |
| 2.1.4 | 🔴 UserMenu component | Krytyczne  | ✅     | Dropdown z avatar, role badge, logout              |
| 2.1.5 | 🟢 Footer component   | Opcjonalne | ✅     | DashboardShell - wrapper komponent layoutu         |

**Blok 2.1 - Wymagania wejściowe**: Faza 1  
**Blok 2.1 - Rezultat**: Podstawowy layout dashboard

---

### 2.2 Global State & Feedback

| #     | Zadanie                    | Priorytet | Status | Opis                                      |
| ----- | -------------------------- | --------- | ------ | ----------------------------------------- |
| 2.2.1 | 🔴 QueryProvider           | Krytyczne | ✅     | TanStack Query provider w root layout     |
| 2.2.2 | 🔴 Global ErrorBoundary    | Krytyczne | ✅     | Przechwytywanie błędów z user-friendly UI |
| 2.2.3 | 🔴 Suspense boundaries     | Krytyczne | ✅     | Suspense z fallback loading w layout      |
| 2.2.4 | 🔴 Toast provider          | Krytyczne | ✅     | Sonner setup dla notyfikacji              |
| 2.2.5 | 🟡 Loading skeleton system | Ważne     | ✅     | CardSkeleton, TableSkeleton, FormSkeleton |

**Blok 2.2 - Wymagania wejściowe**: Blok 2.1  
**Blok 2.2 - Rezultat**: Globalna obsługa stanów i błędów

---

## 🔵 FAZA 3: Team Management (Tydzień 4-5)

### 3.1 Team List & CRUD

| #     | Zadanie                 | Priorytet | Status | Opis                                                 |
| ----- | ----------------------- | --------- | ------ | ---------------------------------------------------- |
| 3.1.1 | 🔴 Team members page    | Krytyczne | ✅     | /team - lista członków zespołu                       |
| 3.1.2 | 🔴 MemberCard component | Krytyczne | ✅     | Karta członka z avatar, role, actions                |
| 3.1.3 | 🔴 MemberList component | Krytyczne | ✅     | TeamTable z sortowaniem i akcjami                    |
| 3.1.4 | 🔴 Team hooks           | Krytyczne | ✅     | useTeam, useInvitations - z hooków Orval             |

**Blok 3.1 - Wymagania wejściowe**: Faza 2  
**Blok 3.1 - Rezultat**: Wyświetlanie i zarządzanie członkami

---

### 3.2 Invitations

| #     | Zadanie                     | Priorytet | Status | Opis                                            |
| ----- | --------------------------- | --------- | ------ | ----------------------------------------------- |
| 3.2.1 | 🔴 InviteMemberDialog       | Krytyczne | ✅     | Dialog z formularzem zaproszenia (TeamInviteForm) |
| 3.2.2 | 🔴 useInviteMember hook     | Krytyczne | ✅     | Mutation do wysyłania zaproszeń (useInvitations)  |
| 3.2.3 | 🔴 Accept invitation page   | Krytyczne | ✅     | /invite/[token] - akceptacja zaproszenia          |
| 3.2.4 | 🟡 Pending invitations list | Ważne     | ✅     | InvitationsList z listą oczekujących              |
| 3.2.5 | 🟡 Resend/Cancel invitation | Ważne     | ✅     | Akcje na zaproszeniach w InvitationsList          |

**Blok 3.2 - Wymagania wejściowe**: Blok 3.1  
**Blok 3.2 - Rezultat**: Pełny system zaproszeń

---

## 🔵 FAZA 4A: Clients Management (Tydzień 5-6)

### 4A.1 Clients List

| #      | Zadanie                     | Priorytet | Status | Opis                                            |
| ------ | --------------------------- | --------- | ------ | ----------------------------------------------- |
| 4A.1.1 | 🔴 Clients page             | Krytyczne | ✅     | /clients - główna strona z listą                |
| 4A.1.2 | 🔴 ClientTable component    | Krytyczne | ✅     | ClientsTable z sortowaniem, paginacją           |
| 4A.1.3 | 🔴 ClientCard component     | Krytyczne | ✅     | Karta klienta dla grid view                     |
| 4A.1.4 | 🔴 View toggle (table/grid) | Krytyczne | ✅     | Przełączanie widoku w ClientsTable              |
| 4A.1.5 | 🔴 Clients hooks            | Krytyczne | ✅     | useClients, useClientMutations - z Orval hooków |

**Blok 4A.1 - Wymagania wejściowe**: Faza 3  
**Blok 4A.1 - Rezultat**: Lista klientów z różnymi widokami

---

### 4A.2 Clients Search & Filters

| #      | Zadanie                    | Priorytet | Status | Opis                             |
| ------ | -------------------------- | --------- | ------ | -------------------------------- |
| 4A.2.1 | 🔴 ClientSearch component  | Krytyczne | ✅     | Search input z debounce w ClientsFilters |
| 4A.2.2 | 🔴 ClientFilters component | Krytyczne | ✅     | ClientsFilters z filtrami status, type   |
| 4A.2.3 | 🟡 useDebounce hook        | Ważne     | ✅     | Reusable debounce hook                   |
| 4A.2.4 | 🟡 Filter persistence      | Ważne     | ✅     | Zapisywanie filtrów w URL params         |

**Blok 4A.2 - Wymagania wejściowe**: Blok 4A.1  
**Blok 4A.2 - Rezultat**: Wyszukiwanie i filtrowanie klientów

---

### 4A.3 Client CRUD

| #      | Zadanie                  | Priorytet | Status | Opis                                              |
| ------ | ------------------------ | --------- | ------ | ------------------------------------------------- |
| 4A.3.1 | 🔴 ClientForm component  | Krytyczne | ✅     | Formularz create/edit z Zod validation            |
| 4A.3.2 | 🔴 Create client page    | Krytyczne | ✅     | /clients/new                                      |
| 4A.3.3 | 🔴 Client detail page    | Krytyczne | ✅     | /clients/[id] - szczegóły klienta                 |
| 4A.3.4 | 🔴 Edit client page      | Krytyczne | ✅     | /clients/[id]/edit                                |
| 4A.3.5 | 🔴 Delete confirmation   | Krytyczne | ✅     | /clients/[id]/delete - AlertDialog potwierdzenia  |
| 4A.3.6 | 🟡 Client mutation hooks | Ważne     | ✅     | useClientMutations - create, update, delete       |

**Blok 4A.3 - Wymagania wejściowe**: Blok 4A.1  
**Blok 4A.3 - Rezultat**: Pełne CRUD operacje na klientach

---

## 🔵 FAZA 4B: Plans Management (Tydzień 6-7)

### 4B.1 Plans List

| #      | Zadanie               | Priorytet | Status | Opis                                  |
| ------ | --------------------- | --------- | ------ | ------------------------------------- |
| 4B.1.1 | 🔴 Plans page         | Krytyczne | ✅     | /plans - lista planów subskrypcyjnych |
| 4B.1.2 | 🔴 PlanCard component | Krytyczne | ✅     | Karta planu z ceną, features, status  |
| 4B.1.3 | 🔴 PlanGrid component | Krytyczne | ✅     | PlansGrid - grid view planów          |
| 4B.1.4 | 🔴 Popular badge      | Krytyczne | ✅     | Badge dla popularnych planów          |
| 4B.1.5 | 🔴 Plans hooks        | Krytyczne | ✅     | usePlans, usePlanMutations            |

**Blok 4B.1 - Wymagania wejściowe**: Faza 3  
**Blok 4B.1 - Rezultat**: Lista planów subskrypcyjnych

---

### 4B.2 Plan CRUD

| #      | Zadanie                       | Priorytet  | Status | Opis                                            |
| ------ | ----------------------------- | ---------- | ------ | ----------------------------------------------- |
| 4B.2.1 | 🔴 PlanForm component         | Krytyczne  | ✅     | Formularz z pricing, billing interval, features |
| 4B.2.2 | 🔴 Create plan page           | Krytyczne  | ✅     | /plans/new                                      |
| 4B.2.3 | 🔴 Plan detail page           | Krytyczne  | ✅     | /plans/[id]                                     |
| 4B.2.4 | 🔴 Edit plan page             | Krytyczne  | ✅     | /plans/[id]/edit                                |
| 4B.2.5 | 🔴 Activate/Deactivate toggle | Krytyczne  | ✅     | Zmiana statusu planu (przycisk w PlanCard)      |
| 4B.2.6 | 🟡 Plan mutation hooks        | Ważne      | ✅     | usePlanMutations - create, update, delete       |
| 4B.2.7 | 🟢 Plan preview               | Opcjonalne | ✅     | PlanCard jako podgląd z features                |

**Blok 4B.2 - Wymagania wejściowe**: Blok 4B.1  
**Blok 4B.2 - Rezultat**: Pełne zarządzanie planami

---

## 🔵 FAZA 5: Subscriptions (Tydzień 7-8)

### 5.1 Subscriptions List

| #     | Zadanie                        | Priorytet | Status | Opis                                          |
| ----- | ------------------------------ | --------- | ------ | --------------------------------------------- |
| 5.1.1 | 🔴 Subscriptions page          | Krytyczne | ✅     | /subscriptions - lista wszystkich subskrypcji |
| 5.1.2 | 🔴 SubscriptionTable component | Krytyczne | ✅     | SubscriptionsTable z client, plan, status     |
| 5.1.3 | 🔴 SubscriptionStatusBadge     | Krytyczne | ✅     | Badge z kolorami dla statusów                 |
| 5.1.4 | 🔴 Subscription filters        | Krytyczne | ✅     | Filtrowanie po searchTerm, status             |
| 5.1.5 | 🔴 Subscriptions hooks         | Krytyczne | ✅     | useSubscriptions, useSubscriptionMutations    |

**Blok 5.1 - Wymagania wejściowe**: Faza 4A, 4B  
**Blok 5.1 - Rezultat**: Lista subskrypcji

---

### 5.2 Subscription Actions

| #     | Zadanie                      | Priorytet | Status | Opis                                     |
| ----- | ---------------------------- | --------- | ------ | ---------------------------------------- |
| 5.2.1 | 🔴 Create subscription flow  | Krytyczne | ✅     | CreateSubscriptionWizard - client→plan→confirm |
| 5.2.2 | 🔴 Subscription detail page  | Krytyczne | ✅     | /subscriptions/[id] - SubscriptionDetail       |
| 5.2.3 | 🔴 Cancel subscription       | Krytyczne | ✅     | CancelSubscriptionDialog z reason              |
| 5.2.4 | 🟡 Pause/Resume subscription | Ważne     | ✅     | SuspendSubscriptionDialog + ResumeDialog       |
| 5.2.5 | 🟡 Change plan               | Ważne     | ✅     | Akcje w SubscriptionDetail (suspend/resume)    |

**Blok 5.2 - Wymagania wejściowe**: Blok 5.1  
**Blok 5.2 - Rezultat**: Pełne zarządzanie subskrypcjami

---

## 🔵 FAZA 6: Payments (Tydzień 8-9)

### 6.1 Payment History

| #     | Zadanie                   | Priorytet | Status | Opis                                        |
| ----- | ------------------------- | --------- | ------ | ------------------------------------------- |
| 6.1.1 | 🔴 Payments page          | Krytyczne | ✅     | /payments - historia płatności              |
| 6.1.2 | 🔴 PaymentTable component | Krytyczne | ✅     | PaymentsTable z amount, status, date        |
| 6.1.3 | 🔴 PaymentStatusBadge     | Krytyczne | ✅     | Badge: Completed, Pending, Failed, Refunded |
| 6.1.4 | 🔴 Payment detail dialog  | Krytyczne | ✅     | PaymentDetail - szczegóły płatności         |

**Blok 6.1 - Wymagania wejściowe**: Faza 5  
**Blok 6.1 - Rezultat**: Historia płatności

---

### 6.2 Payment Methods & Manual Payments

| #     | Zadanie                     | Priorytet | Status | Opis                                      |
| ----- | --------------------------- | --------- | ------ | ----------------------------------------- |
| 6.2.1 | 🔴 PaymentMethodForm        | Krytyczne | ✅     | AddPaymentMethodDialog (PCI DSS compliant)    |
| 6.2.2 | 🔴 PaymentMethodList        | Krytyczne | ✅     | PaymentMethodList - lista metod klienta       |
| 6.2.3 | 🟡 Manual payment recording | Ważne     | ✅     | ProcessPaymentForm - ręczne wprowadzanie      |
| 6.2.4 | 🟡 Refund dialog            | Ważne     | ✅     | RefundPaymentDialog - zwrot z reason          |

**Blok 6.2 - Wymagania wejściowe**: Blok 6.1  
**Blok 6.2 - Rezultat**: Zarządzanie metodami płatności

---

## 🔵 FAZA 7: Analytics & Dashboard (Tydzień 9-10)

### 7.1 Analytics Dashboard

| #     | Zadanie                   | Priorytet  | Status | Opis                              |
| ----- | ------------------------- | ---------- | ------ | --------------------------------- |
| 7.1.1 | 🔴 Analytics page         | Krytyczne  | ✅     | /analytics - główny dashboard         |
| 7.1.2 | 🔴 RevenueChart component | Krytyczne  | ✅     | RevenueChart (recharts)               |
| 7.1.3 | 🔴 StatCards component    | Krytyczne  | ✅     | StatCards - MRR, ARR, Churn, Subs     |
| 7.1.4 | 🟡 DateRangePicker        | Ważne      | ✅     | DateRangePicker - zakres dat          |
| 7.1.5 | 🟡 ClientGrowthChart      | Ważne      | ✅     | ClientGrowthChart - wzrost klientów   |
| 7.1.6 | 🟢 Export reports         | Opcjonalne | ✅     | Export do CSV (zaimplementowany)      |

**Blok 7.1 - Wymagania wejściowe**: Faza 6  
**Blok 7.1 - Rezultat**: Dashboard analityczny

---

## 🔵 FAZA 8: Testing & Polish (Tydzień 10-11)

### 8.1 Testing

| #     | Zadanie             | Priorytet | Status | Opis                             |
| ----- | ------------------- | --------- | ------ | -------------------------------- |
| 8.1.1 | 🔴 Vitest setup     | Krytyczne | ✅     | vitest.config.ts - konfiguracja unit tests    |
| 8.1.2 | 🔴 Component tests  | Krytyczne | ✅     | Testy: PaymentStatusBadge, PaymentsTable, Sidebar, formatters |
| 8.1.3 | 🟡 Playwright setup | Ważne     | ✅     | playwright.config.ts - E2E tests              |
| 8.1.4 | 🟡 E2E auth flow    | Ważne     | ✅     | core-flows.spec.ts - E2E flows                |
| 8.1.5 | 🟡 E2E client CRUD  | Ważne     | ✅     | E2E testy w e2e/core-flows.spec.ts            |

**Blok 8.1 - Wymagania wejściowe**: Wszystkie poprzednie fazy  
**Blok 8.1 - Rezultat**: Testy automatyczne

---

### 8.2 Polish & Optimization

| #     | Zadanie                | Priorytet  | Status | Opis                                    |
| ----- | ---------------------- | ---------- | ------ | --------------------------------------- |
| 8.2.1 | 🔴 TypeScript audit    | Krytyczne  | ⬜     | Weryfikacja brak any, pełne typy        |
| 8.2.2 | 🔴 Accessibility audit | Krytyczne  | ⬜     | Keyboard nav, aria labels, focus states |
| 8.2.3 | 🟡 Performance audit   | Ważne      | ⬜     | Lighthouse, bundle analysis             |
| 8.2.4 | 🟡 Mobile responsive   | Ważne      | ⬜     | Testowanie na różnych rozdzielczościach |
| 8.2.5 | 🟢 Documentation       | Opcjonalne | ⬜     | README, component docs                  |

**Blok 8.2 - Wymagania wejściowe**: Blok 8.1  
**Blok 8.2 - Rezultat**: Production-ready aplikacja

### 9.1 Client Portal Foundation

| #     | Zadanie             | Priorytet | Status | Opis                                       |
| ----- | ------------------- | --------- | ------ | ------------------------------------------ |
| 9.1.1 | 🔴 Portal Layout    | Krytyczne | ⬜     | Osobny layout dla /portal (bez sidebara)   |
| 9.1.2 | 🔴 Portal Guard     | Krytyczne | ⬜     | Ochrona tras tylko dla roli 'Client'       |
| 9.1.3 | 🔴 Portal Dashboard | Krytyczne | ⬜     | /portal - podsumowanie subskrypcji         |
| 9.1.4 | 🟡 Invoices list    | Ważne     | ⬜     | Lista faktur do pobrania (PDF)             |
| 9.1.5 | 🟡 Billing Settings | Ważne     | ⬜     | Zarządzanie kartą i anulowanie subskrypcji |

**Blok 9.1 - Wymagania wejściowe**: Faza 1 (Auth), Faza 5 (Subskrypcje)
**Blok 9.1 - Rezultat**: Działający portal samoobsługowy dla końcowego klienta

---

## 📝 Szablony Zadań dla Agentów

### Format opisu zadania (do promptu)

```markdown
## Zadanie: [Numer] [Nazwa]

### Kontekst

- Projekt: Orbito Frontend (Next.js 15, TypeScript strict)
- Faza: [X]
- Zależności: [poprzednie zadania]

### Cel

[Krótki opis co ma być osiągnięte]

### Wymagania

1. [Wymaganie 1]
2. [Wymaganie 2]
3. ...

### Pliki do utworzenia/modyfikacji

- [ ] src/features/[feature]/components/[Component].tsx
- [ ] ...

### Akceptacja

- [ ] TypeScript bez błędów (tsc --noEmit)
- [ ] Komponenty używają wygenerowanych typów z @/core/api/generated
- [ ] Loading state zaimplementowany
- [ ] Error handling zaimplementowany
```

---

## 🔗 Zależności Między Zadaniami

```
FAZA 0 (Setup)
    ↓
FAZA 1 (Auth + Tenant)
    ↓
FAZA 2 (Layout)
    ↓
FAZA 3 (Team)
    ↓
    ├─→ FAZA 4A (Clients) ──┐
    │                        │
    └─→ FAZA 4B (Plans) ────┤
                             ↓
                    FAZA 5 (Subscriptions)
                             ↓
                    FAZA 6 (Payments)
                             ↓
                    FAZA 7 (Analytics)
                             ↓
                    FAZA 8 (Testing)
```

---

## ⚡ Quick Reference - Komendy

```bash
# Generowanie API z Swaggera (po zmianach w backendzie)
npm run api:generate

# Sprawdzenie typów TypeScript
npm run type-check

# Development
npm run dev

# Build
npm run build

# Testy
npm run test
npm run test:e2e

# Linting
npm run lint
npm run lint:fix
```

---

## 📊 Metryki Sukcesu MVP

| Metryka             | Target                                         |
| ------------------- | ---------------------------------------------- |
| TypeScript coverage | 100% (strict)                                  |
| Wygenerowane typy   | 100% z orval                                   |
| ESLint errors       | 0                                              |
| Core features       | 100% (Clients, Plans, Subscriptions, Payments) |
| Auth + Tenant       | 100%                                           |
| Basic Analytics     | 80%+                                           |
| Mobile responsive   | 90%+                                           |

---

**Wersja**: 6.0  
**Data utworzenia**: 2025-11-29  
**Łączne zadania**: ~95  
**Status**: 🆕 Do implementacji
