# Frontend Plan v6.0 - Orbito Platform

**Wersja**: 6.0 (Fresh Start)  
**Data**: 2025-11-29  
**Stack**: Next.js 15 + TypeScript Strict + Tailwind CSS + TanStack Query + Zustand  
**Czas realizacji**: 10-12 tygodni (MVP)  
**Status**: 🆕 Nowy start od zera

---

## 📋 Executive Summary

### Kluczowe Zmiany vs Poprzednia Wersja

| Obszar             | Było (v5.x)                 | Jest (v6.0)                               | Uzasadnienie                         |
| ------------------ | --------------------------- | ----------------------------------------- | ------------------------------------ |
| **TypeScript**     | Hybrid (allowJs: true)      | **Strict od dnia 0**                      | Type safety 1:1 z backendem          |
| **API Layer**      | Ręczne pisanie \*Api.ts     | **Automatyczna generacja (orval)**        | Eliminacja błędów, oszczędność czasu |
| **Architektura**   | Feature-based               | **Vertical Slices (lustro backendu)**     | Spójność z CQRS backend              |
| **Kolejność faz**  | Auth → Dashboard → Clients  | **Auth → Tenant Context → UI → Features** | Multi-tenancy jako fundament         |
| **Error Handling** | Try-catch w hookach         | **Centralny interceptor Result<T>**       | DRY, spójność                        |
| **Struktura**      | components/, hooks/, pages/ | **features/{domain}/**                    | Domeny biznesowe                     |

---

## 🏗️ Architektura Aplikacji

### Zasady Przewodnie

1. **Backend jest źródłem prawdy** - Frontend tylko odwzorowuje typy i struktury
2. **Tenant Context wszędzie** - Każda operacja wymaga kontekstu tenanta
3. **Fail fast** - TypeScript strict wyłapie błędy przy kompilacji
4. **DRY API** - Generowanie z Swaggera eliminuje duplikację

### Stack Technologiczny

```typescript
const stack = {
  framework: "Next.js 15.x (App Router)",
  language: "TypeScript 5.x (strict: true)",
  styling: {
    css: "Tailwind CSS 3.4",
    components: "shadcn/ui",
    icons: "lucide-react",
  },
  state: {
    server: "TanStack Query v5",
    client: "Zustand v5",
    forms: "React Hook Form v7",
  },
  validation: "Zod v3",
  api: {
    generator: "orval", // Automatyczna generacja z OpenAPI
    client: "axios",
    types: "Auto-generated from swagger.json",
  },
  auth: "NextAuth.js v5 (Auth.js)",
  testing: {
    unit: "Vitest + React Testing Library",
    e2e: "Playwright",
    types: "tsc --noEmit",
  },
};
```

### Struktura Katalogów (Vertical Slices)

```
orbito-frontend/
├── src/
│   ├── app/                      # Next.js App Router (tylko routing)
│   │   ├── (auth)/               # Public routes group
│   │   │   ├── login/page.tsx
│   │   │   ├── register/page.tsx
│   │   │   └── layout.tsx
│   │   ├── (dashboard)/          # Protected routes group
│   │   │   ├── layout.tsx        # Dashboard layout z TenantProvider
│   │   │   ├── page.tsx          # Dashboard home
│   │   │   ├── team/
│   │   │   ├── clients/
│   │   │   ├── plans/
│   │   │   ├── subscriptions/
│   │   │   ├── payments/
│   │   │   └── analytics/
│   │   ├── api/                  # API Routes (NextAuth)
│   │   │   └── auth/[...nextauth]/
│   │   ├── layout.tsx            # Root layout
│   │   └── page.tsx              # Landing page
│   │
│   ├── features/                 # 🎯 Vertical Slices (lustro backendu)
│   │   ├── auth/
│   │   │   ├── components/       # LoginForm, RegisterForm, UserMenu
│   │   │   ├── hooks/            # useAuth, useSession
│   │   │   ├── stores/           # authStore.ts
│   │   │   └── types/            # auth.types.ts (jeśli custom)
│   │   │
│   │   ├── tenant/               # 🆕 Tenant/Team Context
│   │   │   ├── components/       # TenantSwitcher, TenantGuard
│   │   │   ├── hooks/            # useTenant, useTeamContext
│   │   │   ├── stores/           # tenantStore.ts
│   │   │   └── providers/        # TenantProvider.tsx
│   │   │
│   │   ├── team/                 # Team Management
│   │   │   ├── components/       # MemberList, InviteDialog, RoleSelect
│   │   │   ├── hooks/            # useTeamMembers, useInvitations
│   │   │   └── views/            # TeamPage, MemberDetailPage
│   │   │
│   │   ├── clients/              # Client Management
│   │   │   ├── components/       # ClientTable, ClientForm, ClientCard
│   │   │   ├── hooks/            # useClients, useClient, useClientMutations
│   │   │   └── views/            # ClientsPage, ClientDetailPage
│   │   │
│   │   ├── plans/                # Subscription Plans
│   │   │   ├── components/       # PlanCard, PlanForm, PricingTable
│   │   │   ├── hooks/            # usePlans, usePlanMutations
│   │   │   └── views/            # PlansPage, PlanDetailPage
│   │   │
│   │   ├── subscriptions/        # Subscriptions
│   │   │   ├── components/       # SubscriptionCard, SubscriptionStatus
│   │   │   ├── hooks/            # useSubscriptions, useSubscriptionActions
│   │   │   └── views/            # SubscriptionsPage
│   │   │
│   │   ├── payments/             # Payments & Billing
│   │   │   ├── components/       # PaymentHistory, PaymentMethodForm
│   │   │   ├── hooks/            # usePayments, usePaymentMethods
│   │   │   └── views/            # PaymentsPage, BillingPage
│   │   │
│   │   └── analytics/            # Reports & Analytics
│   │       ├── components/       # RevenueChart, ClientStats
│   │       ├── hooks/            # useAnalytics, useReports
│   │       └── views/            # AnalyticsDashboard
│   │
│   ├── shared/                   # Współdzielone komponenty i utilities
│   │   ├── components/
│   │   │   ├── ui/               # shadcn/ui components
│   │   │   ├── layout/           # Header, Sidebar, Footer
│   │   │   ├── feedback/         # LoadingSpinner, ErrorBoundary, EmptyState
│   │   │   └── data-display/     # DataTable, Pagination, StatusBadge
│   │   ├── hooks/
│   │   │   ├── useDebounce.ts
│   │   │   ├── useLocalStorage.ts
│   │   │   └── useMediaQuery.ts
│   │   └── utils/
│   │       ├── formatters.ts     # formatCurrency, formatDate
│   │       ├── validators.ts     # Zod schemas
│   │       └── constants.ts
│   │
│   ├── core/                     # Infrastruktura aplikacji
│   │   ├── api/
│   │   │   ├── client.ts         # Axios instance
│   │   │   ├── interceptors.ts   # Result<T> handling, error mapping
│   │   │   └── generated/        # 🎯 Auto-generated by orval
│   │   │       ├── api.ts        # API functions
│   │   │       ├── model.ts      # TypeScript types (DTOs)
│   │   │       └── hooks.ts      # React Query hooks
│   │   ├── auth/
│   │   │   └── auth.config.ts    # NextAuth configuration
│   │   └── providers/
│   │       ├── QueryProvider.tsx
│   │       ├── AuthProvider.tsx
│   │       └── ThemeProvider.tsx
│   │
│   ├── types/                    # Globalne typy (rozszerzenia, utility types)
│   │   ├── next-auth.d.ts        # Rozszerzenie sesji NextAuth
│   │   └── globals.d.ts
│   │
│   └── middleware.ts             # Route protection, tenant validation
│
├── public/
├── orval.config.ts               # 🎯 Konfiguracja generatora API
├── tsconfig.json                 # strict: true
├── tailwind.config.ts
├── next.config.ts
└── package.json
```

---

## 🔧 Konfiguracja TypeScript (Strict Mode)

### tsconfig.json

```json
{
  "compilerOptions": {
    "target": "ES2022",
    "lib": ["ES2022", "DOM", "DOM.Iterable"],
    "jsx": "preserve",
    "module": "esnext",
    "moduleResolution": "bundler",
    "resolveJsonModule": true,

    // 🎯 STRICT MODE - bez kompromisów
    "strict": true,
    "noImplicitAny": true,
    "strictNullChecks": true,
    "strictFunctionTypes": true,
    "strictBindCallApply": true,
    "strictPropertyInitialization": true,
    "noImplicitThis": true,
    "alwaysStrict": true,

    // 🚫 Bez allowJs - tylko TypeScript
    "allowJs": false,

    "noEmit": true,
    "incremental": true,
    "esModuleInterop": true,
    "skipLibCheck": true,
    "forceConsistentCasingInFileNames": true,

    "paths": {
      "@/*": ["./src/*"],
      "@/features/*": ["./src/features/*"],
      "@/shared/*": ["./src/shared/*"],
      "@/core/*": ["./src/core/*"]
    },

    "plugins": [{ "name": "next" }]
  },
  "include": ["next-env.d.ts", "**/*.ts", "**/*.tsx", ".next/types/**/*.ts"],
  "exclude": ["node_modules"]
}
```

---

## 🔌 Automatyzacja API (orval)

### Dlaczego orval?

| Aspekt                     | Ręczne pisanie | orval        |
| -------------------------- | -------------- | ------------ |
| Czas                       | Dni/tygodnie   | Minuty       |
| Błędy typów                | Częste         | Niemożliwe   |
| Synchronizacja z backendem | Ręczna         | Automatyczna |
| React Query hooks          | Pisane ręcznie | Generowane   |
| Maintenance                | Wysoki         | Zerowy       |

### orval.config.ts

```typescript
import { defineConfig } from "orval";

export default defineConfig({
  orbito: {
    input: {
      target: "http://localhost:5001/swagger/v1/swagger.json",
    },
    output: {
      mode: "tags-split",
      target: "./src/core/api/generated",
      schemas: "./src/core/api/generated/model",
      client: "react-query",
      httpClient: "axios",
      override: {
        mutator: {
          path: "./src/core/api/client.ts",
          name: "apiClient",
        },
        query: {
          useQuery: true,
          useMutation: true,
          signal: true,
        },
      },
    },
    hooks: {
      afterAllFilesWrite: "prettier --write",
    },
  },
});
```

### Workflow generowania

```bash
# Jednorazowo po zmianach w backendzie
npm run api:generate

# Script w package.json
{
  "scripts": {
    "api:generate": "orval --config orval.config.ts",
    "api:watch": "orval --config orval.config.ts --watch"
  }
}
```

### Co zostanie wygenerowane

```typescript
// src/core/api/generated/clients.ts
export const useGetClients = (params?: GetClientsParams) => {
  return useQuery({
    queryKey: getClientsQueryKey(params),
    queryFn: () => getClients(params),
  });
};

export const useCreateClient = () => {
  return useMutation({
    mutationFn: createClient,
  });
};

// src/core/api/generated/model/clientDto.ts
export interface ClientDto {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  // ... wszystkie pola z backendu
}
```

---

## 🔒 Obsługa Result<T> i Błędów Domenowych

### Centralny Interceptor

```typescript
// src/core/api/interceptors.ts
import axios, { AxiosError, AxiosResponse } from "axios";
import { toast } from "sonner";

interface ApiResult<T> {
  isSuccess: boolean;
  value?: T;
  error?: {
    code: string;
    message: string;
    details?: Record<string, string[]>;
  };
}

interface ProblemDetails {
  type: string;
  title: string;
  status: number;
  detail: string;
  errors?: Record<string, string[]>;
}

// Response interceptor - rozpakowuje Result<T>
apiClient.interceptors.response.use(
  (response: AxiosResponse<ApiResult<unknown>>) => {
    const data = response.data;

    // Jeśli backend zwraca Result<T>
    if ("isSuccess" in data) {
      if (data.isSuccess) {
        // Zwróć tylko wartość
        response.data = data.value;
        return response;
      } else {
        // Rzuć błąd domenowy
        const error = new DomainError(data.error!);
        return Promise.reject(error);
      }
    }

    return response;
  },
  (error: AxiosError<ProblemDetails>) => {
    // Obsługa błędów HTTP
    if (error.response?.data) {
      const problemDetails = error.response.data;

      // Mapowanie błędów walidacji na formularze
      if (error.response.status === 400 && problemDetails.errors) {
        const validationError = new ValidationError(problemDetails.errors);
        return Promise.reject(validationError);
      }

      // Błędy autoryzacji
      if (error.response.status === 401) {
        // Redirect do login
        window.location.href = "/login";
      }

      // Błędy biznesowe
      if (error.response.status === 422) {
        toast.error(problemDetails.detail);
      }
    }

    return Promise.reject(error);
  }
);

// Custom Error Classes
export class DomainError extends Error {
  constructor(public error: { code: string; message: string }) {
    super(error.message);
    this.name = "DomainError";
  }
}

export class ValidationError extends Error {
  constructor(public errors: Record<string, string[]>) {
    super("Validation failed");
    this.name = "ValidationError";
  }
}
```

### Integracja z React Hook Form

```typescript
// src/shared/hooks/useFormWithValidation.ts
import { UseFormReturn, FieldValues } from "react-hook-form";
import { ValidationError } from "@/core/api/interceptors";

export function applyServerErrors<T extends FieldValues>(
  form: UseFormReturn<T>,
  error: unknown
) {
  if (error instanceof ValidationError) {
    Object.entries(error.errors).forEach(([field, messages]) => {
      form.setError(field as any, {
        type: "server",
        message: messages[0],
      });
    });
  }
}
```

---

## 🏢 Tenant Context (Fundament Multi-Tenancy)

### TenantProvider

```typescript
// src/features/tenant/providers/TenantProvider.tsx
"use client";

import { createContext, useContext, useEffect, useState } from "react";
import { useSession } from "next-auth/react";

interface TenantContext {
  tenantId: string | null;
  teamRole: "Owner" | "Admin" | "Member" | null;
  teamMemberId: string | null;
  isLoading: boolean;
  hasAccess: (requiredRole?: TeamRole[]) => boolean;
}

const TenantContext = createContext<TenantContext | null>(null);

export function TenantProvider({ children }: { children: React.ReactNode }) {
  const { data: session, status } = useSession();

  const value: TenantContext = {
    tenantId: session?.user?.tenantId ?? null,
    teamRole: session?.user?.teamRole ?? null,
    teamMemberId: session?.user?.teamMemberId ?? null,
    isLoading: status === "loading",
    hasAccess: (requiredRoles) => {
      if (!requiredRoles) return true;
      return requiredRoles.includes(session?.user?.teamRole!);
    },
  };

  return (
    <TenantContext.Provider value={value}>{children}</TenantContext.Provider>
  );
}

export function useTenant() {
  const context = useContext(TenantContext);
  if (!context) {
    throw new Error("useTenant must be used within TenantProvider");
  }
  return context;
}
```

### Rozszerzenie NextAuth Types

```typescript
// src/types/next-auth.d.ts
import "next-auth";

declare module "next-auth" {
  interface User {
    id: string;
    email: string;
    role: "PlatformAdmin" | "Provider" | "Client" | "TeamMember";
    tenantId: string;
    teamRole?: "Owner" | "Admin" | "Member";
    teamMemberId?: string;
  }

  interface Session {
    user: User;
    accessToken: string;
  }
}

declare module "next-auth/jwt" {
  interface JWT {
    id: string;
    role: string;
    tenantId: string;
    teamRole?: string;
    teamMemberId?: string;
    accessToken: string;
  }
}
```

---

## 📊 Fazy Implementacji (Nowa Kolejność)

### Przegląd Faz

| Faza  | Nazwa                 | Czas         | Zależności |
| ----- | --------------------- | ------------ | ---------- |
| **0** | Setup & Configuration | 1 tydzień    | -          |
| **1** | Auth + Tenant Context | 1.5 tygodnia | Faza 0     |
| **2** | Layout & Global UI    | 1 tydzień    | Faza 1     |
| **3** | Team Management       | 1 tydzień    | Faza 2     |
| **4** | Clients & Plans       | 2 tygodnie   | Faza 3     |
| **5** | Subscriptions         | 1.5 tygodnia | Faza 4     |
| **6** | Payments              | 1.5 tygodnia | Faza 5     |
| **7** | Analytics & Dashboard | 1 tydzień    | Faza 6     |
| **8** | Testing & Polish      | 1 tydzień    | Wszystkie  |
| **9** | Client Portal         | 1 tydzień    | Faza 1 i 5 |

**Łącznie: 11.5 tygodnia (MVP)**

### Faza 0: Setup & Configuration

**Cel**: Przygotowanie fundamentów projektu

- Inicjalizacja Next.js 15 z TypeScript strict
- Konfiguracja Tailwind CSS + shadcn/ui
- Setup orval (OpenAPI Generator)
- Konfiguracja ESLint + Prettier
- Struktura katalogów
- Axios client z interceptorami

### Faza 1: Auth + Tenant Context

**Cel**: Bezpieczna autentykacja z kontekstem tenanta

- NextAuth v5 configuration
- JWT token handling z claims (tenantId, teamRole)
- TenantProvider + useTenant hook
- Protected routes middleware
- Auth store (Zustand)
- Login/Register pages
- Session management

### Faza 2: Layout & Global UI

**Cel**: Spójny interfejs z obsługą stanów globalnych

- Dashboard layout (Sidebar, Header)
- Suspense boundaries
- Global ErrorBoundary
- Loading states (skeletons)
- Toast notifications (sonner)
- Theme support (opcjonalnie)
- Responsive design

### Faza 3: Team Management

**Cel**: Zarządzanie członkami zespołu providera

- Team members list
- Invite member dialog
- Role management
- Accept invitation page
- Remove member
- Team activity log (opcjonalnie)

### Faza 4: Clients & Plans (Równolegle)

**Cel**: Core business features

**Clients:**

- Clients list (table + grid view)
- Client form (create/edit)
- Client detail page
- Client search + filters
- Bulk operations
- Client statistics

**Plans:**

- Plans list
- Plan form (pricing, features)
- Plan detail page
- Plan activation/deactivation
- Popular plans badge

### Faza 5: Subscriptions

**Cel**: Zarządzanie subskrypcjami klientów

- Subscriptions list
- Create subscription flow
- Subscription status management
- Cancel/pause subscription
- Subscription history
- Renewal handling

### Faza 6: Payments

**Cel**: Obsługa płatności z Stripe

- Payment history
- Payment methods (CRUD)
- Manual payment recording
- Refund handling
- Idempotency keys w nagłówkach
- Invoice generation

### Faza 7: Analytics & Dashboard

**Cel**: Metryki i raporty

- Revenue dashboard
- Client analytics
- Subscription metrics
- Export reports (CSV/Excel)
- Date range filters
- Charts (recharts)

### Faza 8: Testing & Polish

**Cel**: Jakość i finalizacja

- Unit tests (Vitest)
- E2E tests (Playwright)
- Performance optimization
- Accessibility audit
- Documentation
- Bug fixes

---

## 🎯 MVP Scope

### Must Have (MVP)

- ✅ Authentication (login/register/logout)
- ✅ Tenant Context
- ✅ Team Management (basic)
- ✅ Client CRUD
- ✅ Plan CRUD
- ✅ Basic subscription creation
- ✅ Simple payment processing
- ✅ Basic dashboard

### Nice to Have (Post-MVP)

- Advanced analytics
- Bulk operations
- Export/Import
- Webhook management UI
- Audit logs viewer
- Real-time notifications

---

## 📝 Definition of Done

### Dla każdego zadania

- [ ] Kod w TypeScript strict (brak any, pełne typy)
- [ ] Komponenty używają wygenerowanych typów z orval
- [ ] Loading states zaimplementowane
- [ ] Error handling zaimplementowany
- [ ] Responsive design (mobile-first)
- [ ] Accessibility basics (keyboard nav, aria labels)
- [ ] Tenant context weryfikowany
- [ ] Code review przeprowadzony

### Dla każdej fazy

- [ ] Wszystkie zadania ukończone
- [ ] Integracja z backendem przetestowana
- [ ] TypeScript build bez błędów (`tsc --noEmit`)
- [ ] ESLint bez błędów
- [ ] Dokumentacja zaktualizowana

---

## 🚀 Quick Start

```bash
# 1. Inicjalizacja projektu
npx create-next-app@latest orbito-frontend \
  --typescript \
  --tailwind \
  --eslint \
  --app \
  --src-dir \
  --import-alias "@/*"

cd orbito-frontend

# 2. Instalacja zależności
npm install zustand @tanstack/react-query axios next-auth@beta \
  lucide-react clsx tailwind-merge sonner zod react-hook-form \
  @hookform/resolvers

# 3. Instalacja orval (API generator)
npm install -D orval

# 4. shadcn/ui setup
npx shadcn@latest init

# 5. Generowanie API z backendu
npm run api:generate

# 6. Development
npm run dev
```

---

**Wersja**: 6.0  
**Data utworzenia**: 2025-11-29  
**Autor**: Projekt Orbito  
**Status**: 🆕 Nowy plan - do implementacji
