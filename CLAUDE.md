# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

# Zasady Pracy dla Orbito Platform

## 🌐 Zasady Ogólne i Komunikacja

- **Język**: Zawsze odpowiadaj po polsku. Kod, komentarze w kodzie i commit messages pisz po angielsku.
- **Podejście**: Human-in-the-loop. Jeśli zadanie jest niejasne, zadawaj pytania.
- **Jakość**: Nie zostawiaj `TODO`, placeholderów ani zakomentowanego kodu w finalnej implementacji.

## 🏗️ Architektura Systemu

### Backend (.NET 9) - Clean Architecture

- **Warstwy**: API → Application → Domain ← Infrastructure
- **Pattern**: CQRS + MediatR (Commands/Queries)
- **Security**: Strict Multi-tenancy (`ITenantContext`)

### Frontend (Next.js 15) - Vertical Slices

- **Warstwy**: App (Routing) → Features (Domain) → Core/Shared
- **Pattern**: API-First (Generated Hooks via Orval)
- **Security**: TenantGuard (Provider) + PortalGuard (Client)

---

## 🖥️ Frontend Guidelines (Next.js 15)

### Tech Stack

- **Framework**: Next.js 15 (App Router)
- **Language**: TypeScript (**Strict Mode**: `noImplicitAny`, `strictNullChecks`, `no-explicit-any`)
- **State**: TanStack Query v5 (Server State) + Zustand v5 (Client State)
- **Styling**: Tailwind CSS + shadcn/ui
- **Validation**: Zod + React Hook Form

### Struktura Katalogów (Vertical Slices)

Organizujemy kod według domen biznesowych (features), a nie typów plików.

```

src/
├── app/                  \# Routing, Layouts, Route Groups
│   ├── (auth)/           \# Login, Register
│   ├── (dashboard)/      \# Provider Area (wymaga TenantGuard)
│   └── (portal)/         \# Client Area (wymaga PortalGuard)
├── core/                 \# Infrastruktura Aplikacji
│   ├── api/generated/    \# 🤖 AUTO-GENERATED Orval Hooks (Source of Truth)
│   ├── auth/             \# NextAuth Config
│   └── providers/        \# Global Providers
├── shared/               \# Reużywalne, bezstanowe UI (shadcn) i Utils
└── features/             \# Domeny biznesowe (Lustro backendu)
├── auth/
├── tenant/
├── clients/
├── team/
├── subscriptions/
└── payments/
\# Wewnątrz feature: components/, hooks/, schemas.ts

```

### 🔌 Data Fetching & API (Orval)

**KRYTYCZNE:** Nie pisz ręcznie klientów API, axiosa ani `fetch`.

1. **Generator**: Używamy **Orval** do generowania typów i hooków z OpenAPI.
2. **Importy**: Importuj gotowe hooki z `@/core/api/generated` (np. `useGetClients`, `useCreateInvoice`).
3. **Typy**: Używaj wygenerowanych DTO (np. `ClientDto`, `CreatePaymentCommand`).
4. **Interceptor**: Globalny interceptor w `src/core/api/client.ts` automatycznie obsługuje backendowy `Result<T>` – frontend otrzymuje od razu `value` lub rzuca wyjątek domenowy.

### Next.js 15 Specifics

- **Async Params**: Pamiętaj, że `params` i `searchParams` w `page.tsx` są **Promise'ami**.
  - ✅ DOBRZE: `const { id } = await params;`
  - ❌ ŹLE: `const id = params.id;`
- **Server vs Client**: Domyślnie używaj Server Components. Dodawaj `"use client"` tylko gdy potrzebujesz interaktywności (hooki, event listenery).

---

## ⚙️ Backend Guidelines (.NET 9)

### Standardy Kodowania

- **C# 13**: Nullable Reference Types włączone.
- **Namespaces**: File-scoped (`namespace Orbito.Application;`).
- **Records**: Używaj `record` dla DTOs i Commands/Queries.
- **Result Pattern**: Wszystkie handlery zwracają `Result<T>`.

### 🔒 KRYTYCZNE: Repository Security Pattern

**Zasada Zero Trust**: Wszystkie operacje na danych wrażliwych muszą być izolowane per Tenant.

1. **ITenantContext**: Repozytoria muszą wstrzykiwać ten serwis.
2. **Explicit Filtering**:

   ```csharp
   // ✅ DOBRE - Query Handlers
   var tenantId = _tenantContext.CurrentTenantId;
   var payment = await _paymentRepository.GetByIdForClientAsync(paymentId, clientId, ct);
   var subs = await _repo.GetActiveSubscriptionsForTenantAsync(tenantId, ct);

   // ❌ ZŁE (ZABRONIONE)
   var payment = await _repo.GetByIdAsync(id); // Security Risk!
   ```

````

3.  **Background Jobs**: Iteruj po `TenantId` i używaj metod `ForTenant`.
4.  **Webhooks**: Używaj `UnsafeAsync` TYLKO po weryfikacji sygnatury Stripe, a następnie ręcznie sprawdź `TenantId` w encji.

### CQRS Implementation

```csharp
// Command
public record CreateProviderCommand(string Name, string Email) : IRequest<Result<Guid>>;

// Query
public record GetProviderQuery(Guid Id) : IRequest<Result<ProviderDto>>;

// Validator
public class CreateProviderValidator : AbstractValidator<CreateProviderCommand> { ... }
```

### Rate Limiting & Limits

Używaj `ISecurityLimitService` do sprawdzania limitów biznesowych (np. max payment methods per client) przed wykonaniem akcji.

-----

## 🧪 Testowanie

### Backend

  - **Framework**: xUnit + FluentAssertions + Moq.
  - **Coverage**: \> 95% dla Domain/Application.
  - **Kategorie**: `[Trait("Category", "Unit")]`, `[Trait("Category", "Integration")]`.

### Frontend

  - **Unit**: Vitest + React Testing Library (dla komponentów i utility functions).
  - **E2E**: Playwright (dla krytycznych ścieżek: Login, Checkout, CRUD).
  - **Mocking**: W testach unitowych mockuj hooki Orvala, nie API sieciowe.

-----

# shadcn/ui Component Builder Assistant

*Stosuj te zasady, gdy jesteś proszony o tworzenie lub modyfikację generycznych komponentów UI.*

## Core Responsibilities

  - **Tech Stack**: React, TypeScript, Tailwind CSS, Radix UI, shadcn/ui.
  - **Architecture**:
      - Używaj `forwardRef` dla interaktywnych komponentów.
      - Używaj **CVA** (Class Variance Authority) do wariantów stylów.
      - Używaj `cn()` utility do łączenia klas.
  - **Accessibility**: Strict WCAG 2.1 AA compliance (ARIA labels, keyboard nav, focus states).

## Implementation Rules

1.  **Extend, Don't Rebuild**: Rozszerzaj istniejące komponenty `shadcn/ui` zamiast pisać od zera.
2.  **No Business Logic**: Komponenty w `shared/ui` muszą być "głupie" (prezentacyjne). Logika biznesowa trafia do `features/`.
3.  **Props Interface**: Zawsze definiuj pełne interfejsy TypeScript rozszerzające natywne atrybuty HTML.

<!-- end list -->

```typescript
// Example Pattern
import { cva, type VariantProps } from "class-variance-authority"
import { cn } from "@/shared/lib/utils"

const badgeVariants = cva("inline-flex items-center...", {
  variants: { variant: { default: "...", destructive: "..." } }
})

export interface BadgeProps extends React.HTMLAttributes<HTMLDivElement>,
  VariantProps<typeof badgeVariants> {}

function Badge({ className, variant, ...props }: BadgeProps) {
  return <div className={cn(badgeVariants({ variant }), className)} {...props} />
}
```

```
```
````
