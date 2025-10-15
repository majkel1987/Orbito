# Orbito Frontend - Szczegółowa Lista Zadań Implementacyjnych

## 📋 Instrukcja Użycia

- Każde zadanie oznacz jako ✅ po ukończeniu
- Zadania z 🔴 są krytyczne i blokują dalsze kroki
- Zadania z 🟡 są ważne ale nie blokujące
- Zadania z 🟢 można pominąć w MVP
- Używaj Claude Code 2 do implementacji każdego kroku

---

## FAZA 0: Setup & Foundation (Tydzień 1-2)

### Dzień 1-2: Inicjalizacja Projektu

#### 0.1 Setup Next.js 🔴

- [ ] Utwórz nowy projekt Next.js 14 z App Router (bez TypeScript)
- [ ] Wybierz: Tailwind CSS - Yes, ESLint - Yes, App Router - Yes
- [ ] Usuń niepotrzebne pliki startowe (przykładowy content)
- [ ] Sprawdź czy projekt się uruchamia na localhost:3000

#### 0.2 Struktura Katalogów 🔴

- [ ] Utwórz strukturę folderów zgodną z planem:
  - [ ] `src/app/` - dla Next.js App Router
  - [ ] `src/features/` - dla modułów funkcjonalnych
  - [ ] `src/core/` - dla logiki biznesowej
  - [ ] `src/shared/` - dla współdzielonych zasobów
- [ ] Utwórz `jsconfig.json` z path aliases (@/features, @/core, @/shared)

#### 0.3 Instalacja Podstawowych Zależności 🔴

- [ ] Zainstaluj core dependencies:
  - [ ] `zustand` - state management
  - [ ] `@tanstack/react-query` - server state
  - [ ] `axios` - HTTP client
  - [ ] `next-auth` - autentykacja
- [ ] Zainstaluj UI dependencies:
  - [ ] `lucide-react` - ikony
  - [ ] `clsx` - conditional classes
  - [ ] `tailwind-merge` - merge Tailwind classes

### Dzień 3-4: Konfiguracja Narzędzi

#### 0.4 Tailwind CSS Setup 🔴

- [ ] Skonfiguruj `tailwind.config.js`:
  - [ ] Dodaj custom colors pasujące do brandingu
  - [ ] Skonfiguruj spacing i typography
  - [ ] Dodaj dark mode support (class-based)
- [ ] Utwórz `globals.css` z custom utility classes

#### 0.5 shadcn/ui Installation 🔴

- [ ] Wykonaj `npx shadcn-ui@latest init`
- [ ] Wybierz style: Default
- [ ] Wybierz base color: Slate
- [ ] Sprawdź czy folder `components/ui` został utworzony

#### 0.6 ESLint & Prettier Setup 🟡

- [ ] Skonfiguruj `.eslintrc.json` dla Next.js
- [ ] Dodaj `.prettierrc` z regułami formatowania
- [ ] Dodaj skrypty w `package.json`:
  - [ ] `"lint": "next lint"`
  - [ ] `"format": "prettier --write ."`

### Dzień 5-6: Environment & API Client

#### 0.7 Environment Variables 🔴

- [ ] Utwórz `.env.local` z podstawowymi zmiennymi:
  - [ ] `NEXT_PUBLIC_API_URL` - URL backendu
  - [ ] `NEXTAUTH_URL` - URL aplikacji
  - [ ] `NEXTAUTH_SECRET` - secret dla NextAuth
  - [ ] `NEXT_PUBLIC_STRIPE_KEY` - publiczny klucz Stripe
- [ ] Utwórz `.env.example` jako template

#### 0.8 Axios Client Setup 🔴

- [ ] Utwórz `src/core/api/client.js`:
  - [ ] Skonfiguruj base URL z env
  - [ ] Ustaw default headers
  - [ ] Dodaj timeout (30s)
- [ ] Utwórz `src/core/api/interceptors.js`:
  - [ ] Request interceptor (będzie dodawać token później)
  - [ ] Response interceptor z error handling

#### 0.9 React Query Setup 🔴

- [ ] Utwórz `src/core/lib/react-query.js`:
  - [ ] Skonfiguruj QueryClient z default options
  - [ ] Ustaw staleTime i cacheTime
  - [ ] Skonfiguruj retry logic
- [ ] Dodaj QueryClientProvider do `app/layout.js`

### Dzień 7-8: Podstawowe Komponenty UI

#### 0.10 shadcn Components Import 🔴

- [ ] Zaimportuj podstawowe komponenty:
  - [ ] `npx shadcn-ui@latest add button`
  - [ ] `npx shadcn-ui@latest add card`
  - [ ] `npx shadcn-ui@latest add input`
  - [ ] `npx shadcn-ui@latest add label`
  - [ ] `npx shadcn-ui@latest add toast`
  - [ ] `npx shadcn-ui@latest add dialog`
  - [ ] `npx shadcn-ui@latest add dropdown-menu`
  - [ ] `npx shadcn-ui@latest add skeleton`
  - [ ] `npx shadcn-ui@latest add alert`
  - [ ] `npx shadcn-ui@latest add badge`

#### 0.11 Custom Components 🟡

- [ ] Utwórz `src/shared/components/layouts/PageHeader.jsx`:
  - [ ] Props: title, subtitle, actions
  - [ ] Responsive design
- [ ] Utwórz `src/shared/components/ui/LoadingSpinner.jsx`:
  - [ ] Props: size, fullScreen
  - [ ] Z użyciem lucide-react icons
- [ ] Utwórz `src/shared/components/ui/ErrorMessage.jsx`:
  - [ ] Props: error, onRetry
  - [ ] Friendly error display

#### 0.12 Layout Components 🔴

- [ ] Utwórz `src/shared/components/layouts/MainLayout.jsx`:
  - [ ] Placeholder dla header i sidebar
  - [ ] Main content area z paddingiem
- [ ] Zaktualizuj `app/layout.js`:
  - [ ] Dodaj Toaster provider
  - [ ] Dodaj podstawowe meta tags

---

## FAZA 1: Authentication & Security (Tydzień 3-4)

### Dzień 9-10: NextAuth Setup

#### 1.1 NextAuth Configuration 🔴

- [ ] Utwórz `src/app/api/auth/[...nextauth]/route.js`:
  - [ ] Skonfiguruj Credentials Provider
  - [ ] Dodaj authorize function z API call do backendu
  - [ ] Skonfiguruj JWT callbacks
  - [ ] Dodaj session callback z user info
- [ ] Utwórz `src/core/lib/auth.js`:
  - [ ] Export authOptions dla reużywalności
  - [ ] Helper functions dla auth

#### 1.2 Auth Context & Provider 🔴

- [ ] Utwórz `src/features/auth/providers/AuthProvider.jsx`:
  - [ ] Wrap SessionProvider z next-auth
  - [ ] Dodaj do root layout
- [ ] Utwórz `src/features/auth/hooks/useAuth.js`:
  - [ ] Hook używający useSession
  - [ ] Helper methods (isAuthenticated, hasRole, etc.)

#### 1.3 Login Page 🔴

- [ ] Utwórz `src/app/(auth)/login/page.js`:
  - [ ] Formularz logowania (email, password)
  - [ ] Używaj React Hook Form
  - [ ] Walidacja po stronie klienta
  - [ ] Error handling z toast
  - [ ] Redirect po sukcesie
- [ ] Dodaj loading state podczas logowania
- [ ] Dodaj link do rejestracji

### Dzień 11-12: Registration & Protected Routes

#### 1.4 Registration Page 🔴

- [ ] Utwórz `src/app/(auth)/register/page.js`:
  - [ ] Formularz rejestracji dla Provider
  - [ ] Pola: firstName, lastName, email, password, confirmPassword, companyName
  - [ ] Password strength indicator
  - [ ] Terms acceptance checkbox
- [ ] Utwórz `src/features/auth/api/authApi.js`:
  - [ ] registerProvider function z API call
  - [ ] Error handling

#### 1.5 Middleware for Protected Routes 🔴

- [ ] Utwórz `src/middleware.js`:
  - [ ] Używaj withAuth z next-auth/middleware
  - [ ] Zdefiniuj public routes
  - [ ] Zdefiniuj role-based routes
  - [ ] Redirect logic dla unauthorized
- [ ] Skonfiguruj matcher dla middleware

#### 1.6 Setup Admin Page 🟡

- [ ] Utwórz `src/app/(auth)/setup/page.js`:
  - [ ] Formularz pierwszego admina
  - [ ] Check czy admin już istnieje
  - [ ] One-time setup flow
- [ ] Utwórz API call dla admin setup check

### Dzień 13-14: Auth Components & Guards

#### 1.7 Auth Store (Zustand) 🔴

- [ ] Utwórz `src/features/auth/stores/authStore.js`:
  - [ ] Stan: user, isLoading, error
  - [ ] Actions: setUser, clearUser, setError
  - [ ] Computed: isAuthenticated, hasRole
- [ ] Synchronizuj z NextAuth session

#### 1.8 Protected Components 🔴

- [ ] Utwórz `src/features/auth/components/ProtectedRoute.jsx`:
  - [ ] Props: children, requiredRoles
  - [ ] Check authentication i role
  - [ ] Loading state
  - [ ] Redirect jeśli unauthorized
- [ ] Utwórz `src/features/auth/components/RoleGuard.jsx`:
  - [ ] Component-level authorization
  - [ ] Fallback UI dla unauthorized

#### 1.9 Auth UI Components 🟡

- [ ] Utwórz `src/features/auth/components/UserMenu.jsx`:
  - [ ] Dropdown z user info
  - [ ] Logout button
  - [ ] Link do profilu
- [ ] Utwórz `src/features/auth/components/LoginForm.jsx`:
  - [ ] Wydzielony komponent formularza
  - [ ] Reużywalny w różnych miejscach

#### 1.10 Logout Functionality 🔴

- [ ] Dodaj logout handler:
  - [ ] Call signOut z next-auth
  - [ ] Clear local state
  - [ ] Redirect do login
- [ ] Dodaj logout button do UserMenu

---

## FAZA 2: Layout & Navigation (Tydzień 5-6)

### Dzień 15-16: Main Layout Components

#### 2.1 Header Component 🔴

- [ ] Utwórz `src/shared/components/layouts/Header.jsx`:
  - [ ] Logo i nazwa aplikacji
  - [ ] UserMenu po prawej
  - [ ] Notification icon (placeholder)
  - [ ] Mobile menu toggle
  - [ ] Sticky positioning

#### 2.2 Sidebar Navigation 🔴

- [ ] Utwórz `src/shared/components/layouts/Sidebar.jsx`:
  - [ ] Navigation items z lucide icons
  - [ ] Active route highlighting
  - [ ] Role-based menu filtering
  - [ ] Collapsible na mobile
- [ ] Utwórz `src/core/config/navigation.js`:
  - [ ] Menu items configuration
  - [ ] Role permissions per item

#### 2.3 Dashboard Layout 🔴

- [ ] Utwórz `src/app/(dashboard)/layout.js`:
  - [ ] Integruj Header i Sidebar
  - [ ] Protected route wrapper
  - [ ] Responsive grid layout
- [ ] Dodaj transition animations

### Dzień 17-18: Dashboard Pages

#### 2.4 Main Dashboard 🔴

- [ ] Utwórz `src/app/(dashboard)/page.js`:
  - [ ] Welcome message z user name
  - [ ] Quick stats cards (placeholder)
  - [ ] Recent activity (placeholder)
  - [ ] Quick actions buttons
- [ ] Responsive grid layout

#### 2.5 Provider Dashboard 🟡

- [ ] Utwórz `src/app/(dashboard)/provider/dashboard/page.js`:
  - [ ] Provider-specific metrics
  - [ ] Client overview card
  - [ ] Revenue summary card
  - [ ] Active subscriptions count
- [ ] Role guard dla Provider only

#### 2.6 Admin Dashboard 🟢

- [ ] Utwórz `src/app/(dashboard)/admin/dashboard/page.js`:
  - [ ] Platform-wide statistics
  - [ ] Provider list summary
  - [ ] System health status
- [ ] Role guard dla PlatformAdmin only

### Dzień 19-20: Navigation Features

#### 2.7 Breadcrumbs System 🟡

- [ ] Utwórz `src/shared/components/navigation/Breadcrumbs.jsx`:
  - [ ] Auto-generate z route path
  - [ ] Clickable links
  - [ ] Home icon dla root
- [ ] Integruj w dashboard layout

#### 2.8 Loading States 🔴

- [ ] Utwórz loading.js dla każdej route group:
  - [ ] `app/(dashboard)/loading.js`
  - [ ] `app/(auth)/loading.js`
- [ ] Używaj Skeleton components

#### 2.9 Error Pages 🔴

- [ ] Utwórz `src/app/not-found.js`:
  - [ ] 404 page z friendly message
  - [ ] Link do dashboard
- [ ] Utwórz `src/app/error.js`:
  - [ ] Global error boundary
  - [ ] Retry button

#### 2.10 Mobile Navigation 🟡

- [ ] Dodaj mobile drawer do Sidebar:
  - [ ] Hamburger menu toggle
  - [ ] Overlay backdrop
  - [ ] Swipe gestures support
- [ ] Test responsive breakpoints

---

## FAZA 3: Client Management (Tydzień 7-10)

### Dzień 21-23: Client API Integration

#### 3.1 Client API Service 🔴

- [ ] Utwórz `src/features/clients/api/clientsApi.js`:
  - [ ] CRUD operations (create, read, update, delete)
  - [ ] Search endpoint
  - [ ] Stats endpoint
  - [ ] Activate/deactivate endpoints
- [ ] Error handling dla każdej metody

#### 3.2 Client React Query Hooks 🔴

- [ ] Utwórz `src/features/clients/hooks/useClients.js`:
  - [ ] useClients - lista z paginacją
  - [ ] useClient - pojedynczy klient
  - [ ] useCreateClient - tworzenie
  - [ ] useUpdateClient - aktualizacja
  - [ ] useDeleteClient - usuwanie
- [ ] Optimistic updates gdzie możliwe

#### 3.3 Client Store (Zustand) 🟡

- [ ] Utwórz `src/features/clients/stores/clientStore.js`:
  - [ ] Selected clients dla bulk operations
  - [ ] Filter i sort preferences
  - [ ] Search term
- [ ] Persist filters w localStorage

### Dzień 24-26: Client List Page

#### 3.4 Client List Page 🔴

- [ ] Utwórz `src/app/(dashboard)/clients/page.js`:
  - [ ] Tabela klientów
  - [ ] Pagination controls
  - [ ] Search bar z debounce
  - [ ] Filter dropdown (active/inactive)
  - [ ] Sort options
- [ ] Loading i error states

#### 3.5 Client Table Component 🔴

- [ ] Utwórz `src/features/clients/components/ClientTable.jsx`:
  - [ ] Kolumny: Name, Email, Status, Created, Actions
  - [ ] Sortable headers
  - [ ] Row selection checkboxes
  - [ ] Action buttons (View, Edit, Delete)
- [ ] Responsive - cards na mobile

#### 3.6 Client Search & Filters 🔴

- [ ] Utwórz `src/features/clients/components/ClientFilters.jsx`:
  - [ ] Search input z debounce (500ms)
  - [ ] Status filter dropdown
  - [ ] Date range picker (optional)
  - [ ] Clear filters button
- [ ] Synchronizuj z URL params

### Dzień 27-29: Client Details & Forms

#### 3.7 Client Detail Page 🔴

- [ ] Utwórz `src/app/(dashboard)/clients/[id]/page.js`:
  - [ ] Client info cards
  - [ ] Subscriptions list
  - [ ] Payment history
  - [ ] Activity timeline
  - [ ] Action buttons (Edit, Deactivate)
- [ ] Loading state z skeletons

#### 3.8 Client Form Component 🔴

- [ ] Utwórz `src/features/clients/components/ClientForm.jsx`:
  - [ ] Pola: companyName, email, taxId, billingAddress, contactPerson
  - [ ] React Hook Form integration
  - [ ] Walidacja z Zod
  - [ ] Submit i Cancel buttons
- [ ] Reużywalny dla Create i Edit

#### 3.9 Create Client Page 🔴

- [ ] Utwórz `src/app/(dashboard)/clients/new/page.js`:
  - [ ] Używaj ClientForm
  - [ ] Success redirect do detail page
  - [ ] Error handling z toast
- [ ] Breadcrumbs navigation

#### 3.10 Edit Client Page 🔴

- [ ] Utwórz `src/app/(dashboard)/clients/[id]/edit/page.js`:
  - [ ] Pre-populate form z client data
  - [ ] Loading state podczas fetch
  - [ ] Success redirect do detail
  - [ ] Cancel wraca do detail
- [ ] Optimistic update

### Dzień 30-32: Client Advanced Features

#### 3.11 Bulk Operations 🟡

- [ ] Dodaj bulk actions toolbar:
  - [ ] Show gdy selected > 0
  - [ ] Bulk activate/deactivate
  - [ ] Bulk delete z confirmation
- [ ] Progress indicator dla operacji

#### 3.12 Client Statistics 🟡

- [ ] Utwórz `src/features/clients/components/ClientStats.jsx`:
  - [ ] Total clients count
  - [ ] Active vs Inactive
  - [ ] Growth trend
  - [ ] Top clients by revenue
- [ ] Użyj w dashboard

#### 3.13 Export Functionality 🟢

- [ ] Dodaj export button do list page:
  - [ ] Export do CSV
  - [ ] Export do Excel
  - [ ] Respektuj current filters
- [ ] Progress indicator

#### 3.14 Client Activity Log 🟢

- [ ] Utwórz `src/features/clients/components/ActivityTimeline.jsx`:
  - [ ] Show recent activities
  - [ ] Icons per activity type
  - [ ] Relative timestamps
- [ ] Lazy load starsze wpisy

---

## FAZA 4: Plans & Subscriptions (Tydzień 11-14)

### Dzień 33-35: Plans Management

#### 4.1 Plans API Service 🔴

- [ ] Utwórz `src/features/plans/api/plansApi.js`:
  - [ ] CRUD operations
  - [ ] Get active plans
  - [ ] Activate/deactivate
- [ ] Handle pricing w różnych walutach

#### 4.2 Plans Hooks 🔴

- [ ] Utwórz `src/features/plans/hooks/usePlans.js`:
  - [ ] usePlans - lista planów
  - [ ] useActivePlans - tylko aktywne
  - [ ] useCreatePlan, useUpdatePlan, useDeletePlan
- [ ] Cache active plans longer (10 min)

#### 4.3 Plan List Page 🔴

- [ ] Utwórz `src/app/(dashboard)/plans/page.js`:
  - [ ] Grid view z plan cards
  - [ ] Toggle list/grid view
  - [ ] Filter active/inactive
  - [ ] Create new button
- [ ] Pricing display z currency

### Dzień 36-38: Plan Details & Forms

#### 4.4 Plan Detail Page 🔴

- [ ] Utwórz `src/app/(dashboard)/plans/[id]/page.js`:
  - [ ] Plan information
  - [ ] Features list
  - [ ] Active subscriptions count
  - [ ] Edit/Delete actions
- [ ] Pricing history (optional)

#### 4.5 Plan Form Component 🔴

- [ ] Utwórz `src/features/plans/components/PlanForm.jsx`:
  - [ ] Name, description, price fields
  - [ ] Currency selector
  - [ ] Billing interval selector
  - [ ] Dynamic features list (add/remove)
  - [ ] Active toggle
- [ ] Price preview

#### 4.6 Create/Edit Plan Pages 🔴

- [ ] Utwórz `src/app/(dashboard)/plans/new/page.js`
- [ ] Utwórz `src/app/(dashboard)/plans/[id]/edit/page.js`
- [ ] Form validation
- [ ] Success handling

### Dzień 39-42: Subscriptions

#### 4.7 Subscriptions API Service 🔴

- [ ] Utwórz `src/features/subscriptions/api/subscriptionsApi.js`:
  - [ ] Create subscription
  - [ ] List subscriptions
  - [ ] Update subscription
  - [ ] Pause/Resume/Cancel operations
- [ ] Filter by status

#### 4.8 Subscription Hooks 🔴

- [ ] Utwórz `src/features/subscriptions/hooks/useSubscriptions.js`:
  - [ ] useSubscriptions z filters
  - [ ] useSubscription by ID
  - [ ] useCreateSubscription
  - [ ] Lifecycle operation hooks
- [ ] Real-time status updates

#### 4.9 Subscription List Page 🔴

- [ ] Utwórz `src/app/(dashboard)/subscriptions/page.js`:
  - [ ] Table view
  - [ ] Status badges
  - [ ] Filter by status/client/plan
  - [ ] Search functionality
- [ ] Bulk operations

### Dzień 43-45: Subscription Wizard

#### 4.10 Subscription Wizard Component 🔴

- [ ] Utwórz `src/features/subscriptions/components/SubscriptionWizard.jsx`:
  - [ ] Step 1: Select Client
  - [ ] Step 2: Choose Plan
  - [ ] Step 3: Payment Method
  - [ ] Step 4: Review & Confirm
- [ ] Progress indicator
- [ ] Back/Next navigation
- [ ] Validation per step

#### 4.11 Create Subscription Page 🔴

- [ ] Utwórz `src/app/(dashboard)/subscriptions/new/page.js`:
  - [ ] Używaj SubscriptionWizard
  - [ ] Handle wizard completion
  - [ ] Success redirect
- [ ] Cancel handling

#### 4.12 Subscription Detail Page 🔴

- [ ] Utwórz `src/app/(dashboard)/subscriptions/[id]/page.js`:
  - [ ] Subscription info cards
  - [ ] Payment history
  - [ ] Status timeline
  - [ ] Action buttons (Pause/Resume/Cancel)
- [ ] Next billing date display

#### 4.13 Subscription Actions 🔴

- [ ] Implementuj lifecycle actions:
  - [ ] Pause subscription z datą
  - [ ] Resume subscription
  - [ ] Cancel z reason
  - [ ] Change plan flow
- [ ] Confirmation modals

#### 4.14 Subscription Metrics 🟢

- [ ] Utwórz dashboard widgets:
  - [ ] Active subscriptions count
  - [ ] MRR calculation
  - [ ] Churn rate
  - [ ] Growth trend
- [ ] Real-time updates

---

## FAZA 5: Payment System (Tydzień 15-18)

### Dzień 46-48: Payment Integration Setup

#### 5.1 Stripe Setup 🔴

- [ ] Zainstaluj `@stripe/stripe-js` i `@stripe/react-stripe-js`
- [ ] Utwórz `src/core/lib/stripe.js`:
  - [ ] Initialize Stripe z public key
  - [ ] Export dla reużycia
- [ ] Test połączenie z Stripe

#### 5.2 Payment API Service 🔴

- [ ] Utwórz `src/features/payments/api/paymentsApi.js`:
  - [ ] Process payment
  - [ ] Get payment history
  - [ ] Refund payment
  - [ ] Retry payment
- [ ] Handle Stripe webhooks

#### 5.3 Payment Hooks 🔴

- [ ] Utwórz `src/features/payments/hooks/usePayments.js`:
  - [ ] usePayments - lista
  - [ ] useProcessPayment
  - [ ] useRefundPayment
  - [ ] useRetryPayment
- [ ] Error handling

### Dzień 49-51: Payment Forms

#### 5.4 Stripe Payment Form 🔴

- [ ] Utwórz `src/features/payments/components/StripePaymentForm.jsx`:
  - [ ] Card element z Stripe
  - [ ] Billing address fields
  - [ ] Save payment method checkbox
  - [ ] 3D Secure handling
- [ ] Loading states

#### 5.5 Payment Methods Management 🔴

- [ ] Utwórz `src/features/payments/components/PaymentMethodsList.jsx`:
  - [ ] List saved cards
  - [ ] Default indicator
  - [ ] Remove button
  - [ ] Add new button
- [ ] Card brand icons

#### 5.6 Add Payment Method Modal 🔴

- [ ] Utwórz `src/features/payments/components/AddPaymentMethodModal.jsx`:
  - [ ] Stripe card element
  - [ ] Set as default checkbox
  - [ ] Save functionality
- [ ] Success feedback

### Dzień 52-54: Payment History & Processing

#### 5.7 Payment List Page 🔴

- [ ] Utwórz `src/app/(dashboard)/payments/page.js`:
  - [ ] Payment history table
  - [ ] Status badges
  - [ ] Amount formatting
  - [ ] Filter by status/date
  - [ ] Export functionality
- [ ] Pagination

#### 5.8 Payment Detail Page 🔴

- [ ] Utwórz `src/app/(dashboard)/payments/[id]/page.js`:
  - [ ] Payment info
  - [ ] Transaction timeline
  - [ ] Related subscription
  - [ ] Refund button
- [ ] Receipt download

#### 5.9 Process Payment Flow 🔴

- [ ] Utwórz payment processing flow:
  - [ ] Select payment method
  - [ ] Confirm amount
  - [ ] Process with Stripe
  - [ ] Handle success/failure
  - [ ] Update UI optimistically
- [ ] Retry logic

### Dzień 55-57: Refunds & Advanced Features

#### 5.10 Refund Modal 🔴

- [ ] Utwórz `src/features/payments/components/RefundModal.jsx`:
  - [ ] Amount input (max = original)
  - [ ] Reason textarea
  - [ ] Partial/Full refund toggle
  - [ ] Confirmation step
- [ ] Success handling

#### 5.11 Failed Payments Page 🟡

- [ ] Utwórz `src/app/(dashboard)/payments/failed/page.js`:
  - [ ] List failed payments
  - [ ] Retry buttons
  - [ ] Bulk retry
  - [ ] Schedule retry
- [ ] Error reasons display

#### 5.12 Payment Notifications 🟡

- [ ] Implementuj payment notifications:
  - [ ] Success toast
  - [ ] Failure alert
  - [ ] Email notification trigger
  - [ ] In-app notifications
- [ ] Notification preferences

#### 5.13 Invoice Generation 🟢

- [ ] Utwórz invoice components:
  - [ ] Invoice preview
  - [ ] PDF generation
  - [ ] Email invoice
  - [ ] Download button
- [ ] Invoice template

#### 5.14 Payment Analytics Widget 🟢

- [ ] Utwórz payment widgets:
  - [ ] Today's revenue
  - [ ] Success rate
  - [ ] Average transaction
  - [ ] Payment methods breakdown
- [ ] Real-time updates

---

## FAZA 6: Analytics & Reports (Tydzień 19-21)

### Dzień 58-60: Analytics Setup

#### 6.1 Analytics API Service 🔴

- [ ] Utwórz `src/features/analytics/api/analyticsApi.js`:
  - [ ] Get payment statistics
  - [ ] Get revenue reports
  - [ ] Get payment trends
  - [ ] Get failure reasons
- [ ] Date range parameters

#### 6.2 Chart Components Setup 🔴

- [ ] Zainstaluj `recharts`
- [ ] Utwórz `src/features/analytics/components/charts/`:
  - [ ] LineChart wrapper
  - [ ] BarChart wrapper
  - [ ] PieChart wrapper
  - [ ] AreaChart wrapper
- [ ] Responsive containers

#### 6.3 Analytics Hooks 🔴

- [ ] Utwórz `src/features/analytics/hooks/useAnalytics.js`:
  - [ ] usePaymentStats
  - [ ] useRevenueReport
  - [ ] usePaymentTrends
- [ ] Cache strategy (5 min)

### Dzień 61-63: Dashboard & Reports

#### 6.4 Analytics Dashboard 🔴

- [ ] Utwórz `src/app/(dashboard)/analytics/page.js`:
  - [ ] KPI cards grid
  - [ ] Revenue chart
  - [ ] Payment status breakdown
  - [ ] Date range picker
- [ ] Export functionality

#### 6.5 Revenue Report Page 🔴

- [ ] Utwórz `src/app/(dashboard)/analytics/revenue/page.js`:
  - [ ] MRR/ARR metrics
  - [ ] Growth trends
  - [ ] Currency breakdown
  - [ ] Comparison periods
- [ ] Drill-down capability

#### 6.6 Payment Statistics Page 🔴

- [ ] Utwórz `src/app/(dashboard)/analytics/payments/page.js`:
  - [ ] Success/failure rates
  - [ ] Processing times
  - [ ] Payment methods usage
  - [ ] Failure reasons chart
- [ ] Time period selector

#### 6.7 Custom Reports Builder 🟢

- [ ] Utwórz report builder:
  - [ ] Metric selector
  - [ ] Grouping options
  - [ ] Filter builder
  - [ ] Chart type selector
  - [ ] Save report template
- [ ] Schedule reports

#### 6.8 Export & Sharing 🟡

- [ ] Implementuj export options:
  - [ ] Export to PDF
  - [ ] Export to Excel
  - [ ] Email report
  - [ ] Share link generation
- [ ] Schedule automated reports

---

## FAZA 7: Testing & Optimization (Tydzień 22-24)

### Dzień 64-66: Unit Testing

#### 7.1 Testing Setup 🔴

- [ ] Skonfiguruj Jest
- [ ] Skonfiguruj React Testing Library
- [ ] Setup MSW dla API mocking
- [ ] Coverage reporter config

#### 7.2 Component Tests 🔴

- [ ] Napisz testy dla:
  - [ ] Auth components (Login, Register)
  - [ ] Client components (Table, Form)
  - [ ] Payment components (Form, List)
  - [ ] Shared UI components
- [ ] Min 70% coverage

#### 7.3 Hook Tests 🔴

- [ ] Napisz testy dla custom hooks:
  - [ ] useAuth
  - [ ] useClients
  - [ ] usePayments
  - [ ] useAnalytics
- [ ] Mock API responses

### Dzień 67-69: Integration & E2E Testing

#### 7.4 Integration Tests 🔴

- [ ] Test critical user flows:
  - [ ] Login → Dashboard
  - [ ] Create Client → View Client
  - [ ] Create Subscription flow
  - [ ] Payment processing flow
- [ ] API integration tests

#### 7.5 E2E Testing Setup 🟡

- [ ] Skonfiguruj Playwright
- [ ] Napisz E2E testy dla:
  - [ ] Authentication flow
  - [ ] Client CRUD operations
  - [ ] Payment flow
  - [ ] Critical user journeys
- [ ] Cross-browser testing

#### 7.6 Performance Testing 🟡

- [ ] Lighthouse audits:
  - [ ] Performance score > 90
  - [ ] Accessibility score > 95
  - [ ] Best practices > 95
  - [ ] SEO score > 90
- [ ] Bundle size analysis

### Dzień 70-72: Optimization

#### 7.7 Performance Optimization 🔴

- [ ] Implementuj code splitting:
  - [ ] Lazy load routes
  - [ ] Dynamic imports dla heavy components
  - [ ] Optimize images (next/image)
- [ ] Implement React.memo gdzie potrzebne

#### 7.8 SEO Optimization 🟡

- [ ] Dodaj metadata dla każdej strony
- [ ] Implementuj structured data
- [ ] Sitemap generation
- [ ] Robots.txt
- [ ] Open Graph tags

#### 7.9 Accessibility Improvements 🔴

- [ ] ARIA labels dla wszystkich interaktywnych elementów
- [ ] Keyboard navigation testing
- [ ] Screen reader testing
- [ ] Color contrast verification
- [ ] Focus management

#### 7.10 Security Audit 🔴

- [ ] Sprawdź dependencies (npm audit)
- [ ] Content Security Policy headers
- [ ] XSS protection verification
- [ ] CSRF token implementation check
- [ ] Rate limiting testing

#### 7.11 Error Handling Enhancement 🟡

- [ ] Global error boundary improvements
- [ ] Sentry integration setup
- [ ] Error logging strategy
- [ ] User-friendly error messages
- [ ] Retry mechanisms

#### 7.12 Documentation 🟢

- [ ] README.md z setup instructions
- [ ] API documentation
- [ ] Component storybook (optional)
- [ ] Deployment guide
- [ ] Contributing guidelines

---

## FAZA 8: Deployment & Monitoring (Bonus)

### Production Preparation

#### 8.1 Environment Setup 🔴

- [ ] Production environment variables
- [ ] API URLs configuration
- [ ] Feature flags setup
- [ ] Error tracking (Sentry)

#### 8.2 Build Optimization 🔴

- [ ] Production build testing
- [ ] Bundle analysis
- [ ] Asset optimization
- [ ] CDN configuration

#### 8.3 CI/CD Pipeline 🟡

- [ ] GitHub Actions setup
- [ ] Automated testing
- [ ] Build verification
- [ ] Deployment automation

#### 8.4 Monitoring Setup 🟡

- [ ] Google Analytics
- [ ] Performance monitoring
- [ ] Error tracking
- [ ] Uptime monitoring

#### 8.5 Deployment 🔴

- [ ] Vercel/Netlify setup
- [ ] Domain configuration
- [ ] SSL certificates
- [ ] Production deployment

---

## 📋 Post-MVP Features (Backlog)

### Nice to Have

- [ ] Dark mode implementation
- [ ] Multi-language support (i18n)
- [ ] Advanced search with filters
- [ ] Bulk import/export
- [ ] Email templates editor
- [ ] Webhook management UI
- [ ] API keys management
- [ ] Two-factor authentication
- [ ] Activity audit logs viewer
- [ ] Real-time notifications (WebSocket)
- [ ] Custom dashboard widgets
- [ ] White-label configuration
- [ ] Mobile app (React Native)
- [ ] Advanced permission system
- [ ] Workflow automation

---

## ✅ Definition of Done - Checklist per Feature

Każda funkcjonalność powinna spełniać:

- [ ] Kod działa lokalnie bez błędów
- [ ] Responsive design (mobile, tablet, desktop)
- [ ] Loading states zaimplementowane
- [ ] Error handling z user-friendly messages
- [ ] Form validation działa poprawnie
- [ ] Success feedback (toast/redirect)
- [ ] Testy napisane (unit lub integration)
- [ ] Accessibility sprawdzone (keyboard nav)
- [ ] Cross-browser testing (Chrome, Firefox, Safari)
- [ ] Code review przeprowadzony
- [ ] Dokumentacja zaktualizowana
- [ ] Deployed na staging

---

## 🔥 Quick Start Commands

```bash
# Inicjalizacja projektu
npx create-next-app@14 orbito-frontend --js --tailwind --app --no-typescript
cd orbito-frontend

# Instalacja podstawowych dependencies
npm install zustand @tanstack/react-query axios next-auth lucide-react clsx tailwind-merge

# shadcn/ui setup
npx shadcn-ui@latest init

# Importowanie komponentów
npx shadcn-ui@latest add button card input label toast dialog dropdown-menu skeleton alert badge

# Development
npm run dev

# Testing
npm test
npm run test:coverage

# Build
npm run build
npm run start

# Linting & Formatting
npm run lint
npm run format
```

---

**Status**: 📋 **READY FOR IMPLEMENTATION**  
**Total Tasks**: ~280  
**Estimated Time**: 24 weeks  
**MVP Timeline**: 10 weeks (do końca Fazy 3)

---
