# CLAUDE.md - Instrukcje dla AI Agent

> This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

---

## 🌐 Zasady Ogólne

- **Język odpowiedzi**: Polski
- **Język kodu**: Angielski (kod, komentarze, commit messages)
- **Podejście**: Human-in-the-loop - jeśli zadanie jest niejasne, zadawaj pytania
- **Jakość**: Zero `TODO`, placeholderów, zakomentowanego kodu w finalnej implementacji

---

## 📁 Pliki do przeczytania NA POCZĄTKU każdej sesji

| Priorytet | Plik                         | Opis                                 |
| --------- | ---------------------------- | ------------------------------------ |
| 🚨 **1**  | `.agent/API_RULES.md`        | Krytyczne reguły API - OBOWIĄZKOWE!  |
| 2         | `.agent/feature_list.json`   | Lista bloków, znajdź `passes: false` |
| 3         | `.agent/claude-progress.txt` | Kontekst sesji + KNOWN BUGS          |
| 4         | `Frontend_Prompts.md`        | Szczegółowe prompty dla bloków       |

---

## 🚫 ABSOLUTNE ZAKAZY (API Rules)

```typescript
// ❌ NIGDY TAK NIE RÓB:
<p>0</p>; // hardcoded data
const data = []; // empty array instead of API
console.log("TODO: implement"); // mock function
// TODO: add later                    // placeholder comment
const handleSubmit = () => {
  // fake success
  toast.success("Saved!"); // KŁAMSTWO - nic nie zapisano
};

// ✅ ZAWSZE TAK:
const { data, isLoading, error } = useGetApiClients();
if (isLoading) return <Skeleton />;
if (error) return <ErrorMessage error={error} />;
if (!data?.length) return <EmptyState />;
return <ClientsList clients={data} />;
```

---

## ✅ Wymagany Pattern dla komponentów z danymi

```typescript
"use client";

import { useGetApiClients } from "@/core/api/generated/clients/clients";
import { Skeleton } from "@/shared/ui/skeleton";

export function ClientsList() {
  const { data, isLoading, error } = useGetApiClients();

  // 1. Loading state - OBOWIĄZKOWY
  if (isLoading) return <Skeleton className="h-40" />;

  // 2. Error state - OBOWIĄZKOWY
  if (error) return <div className="text-red-500">Error: {error.message}</div>;

  // 3. Empty state - OBOWIĄZKOWY
  if (!data?.length) return <div>No clients found</div>;

  // 4. Success state - prawdziwe dane
  return (
    <ul>
      {data.map((client) => (
        <li key={client.id}>{client.name}</li>
      ))}
    </ul>
  );
}
```

---

## ✅ Wymagany Pattern dla mutacji

```typescript
"use client";

import { usePostApiClients } from "@/core/api/generated/clients/clients";
import { useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function CreateClientForm() {
  const queryClient = useQueryClient();

  const mutation = usePostApiClients({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ["api", "clients"] });
        toast.success("Client created!");
      },
      onError: (error) => {
        toast.error(error.message);
      },
    },
  });

  const onSubmit = async (data: CreateClientCommand) => {
    await mutation.mutateAsync(data);
  };

  return (/* form with react-hook-form */);
}
```

---

## 🔍 Weryfikacja po każdym bloku

### 1. Network Tab (DevTools → Network)

- [ ] Widzę requesty do `/api/...`
- [ ] Status `200` (nie `401 Unauthorized`)
- [ ] Header `Authorization: Bearer ...` jest obecny

### 2. Console (DevTools → Console)

- [ ] Brak `TypeError: Cannot read property of undefined`
- [ ] Brak `401` errors

### 3. UI

- [ ] Dane są PRAWDZIWE (nie "0", nie puste listy)
- [ ] Loading spinner podczas ładowania
- [ ] Error message przy błędach

---

## 🗂️ Architektura Projektu

### Frontend (Next.js 15) - Vertical Slices

```
src/
├── app/                      # Next.js App Router
│   ├── (auth)/              # Login, Register (publiczne)
│   ├── (dashboard)/         # Panel Provider (wymaga TenantGuard)
│   └── (portal)/            # Panel Client (wymaga PortalGuard)
├── core/                     # Infrastruktura aplikacji
│   ├── api/
│   │   ├── client.ts        # Axios + interceptors (auth, Result<T>)
│   │   └── generated/       # 🤖 Orval hooks (DO NOT EDIT!)
│   ├── auth/                # NextAuth config
│   └── providers/           # QueryProvider, etc.
├── features/                 # Domeny biznesowe (Vertical Slices)
│   ├── auth/                # components/, hooks/, schemas.ts
│   ├── clients/
│   ├── plans/
│   ├── subscriptions/
│   ├── payments/
│   └── team/
└── shared/                   # Reużywalne, bezstanowe
    ├── ui/                  # shadcn/ui components
    ├── lib/                 # utils.ts, formatters.ts
    └── hooks/               # shared hooks
```

### Backend (.NET 9) - Clean Architecture

- **Warstwy**: API → Application → Domain ← Infrastructure
- **Pattern**: CQRS + MediatR (Commands/Queries)
- **Security**: Strict Multi-tenancy (`ITenantContext`)

---

## 🖥️ Frontend Tech Stack

| Kategoria      | Technologia                |
| -------------- | -------------------------- |
| Framework      | Next.js 15 (App Router)    |
| Language       | TypeScript **Strict Mode** |
| Server State   | TanStack Query v5          |
| Client State   | Zustand v5                 |
| API Generation | **Orval** (z Swagger)      |
| Styling        | Tailwind CSS + shadcn/ui   |
| Forms          | React Hook Form + Zod      |
| Auth           | NextAuth v5 (Credentials)  |

---

## 📌 Data Fetching (Orval) - KRYTYCZNE

**NIE pisz ręcznie klientów API, axiosa ani `fetch`!**

1. **Hooki są generowane**: `npm run api:generate`
2. **Importuj z**: `@/core/api/generated/`
3. **Typy DTO**: Używaj wygenerowanych (`ClientDto`, `CreatePaymentCommand`)
4. **Interceptor**: `src/core/api/client.ts` obsługuje:
   - Auth header (`Authorization: Bearer`)
   - Result<T> unwrapping (backend pattern)

```typescript
// ✅ DOBRZE - używaj wygenerowanych hooków
import {
  useGetApiClients,
  usePostApiClients,
} from "@/core/api/generated/clients/clients";

// ❌ ŹLE - nie pisz ręcznie
const response = await axios.get("/api/clients"); // ZABRONIONE
```

---

## ⚡ Next.js 15 Specifics

### Async Params

```typescript
// ✅ DOBRZE - params to Promise
export default async function Page({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  return <ClientDetail id={id} />;
}

// ❌ ŹLE
const id = params.id; // TypeError!
```

### Server vs Client Components

- **Domyślnie**: Server Components
- **Dodaj `"use client"`** tylko gdy: hooks, event listeners, browser APIs

---

## 🎨 shadcn/ui Guidelines

1. **Extend, Don't Rebuild**: Rozszerzaj istniejące komponenty
2. **No Business Logic**: Komponenty w `shared/ui` są "głupie" (prezentacyjne)
3. **CVA dla wariantów**: Używaj Class Variance Authority
4. **cn() dla klas**: Łącz klasy przez utility

```typescript
import { cva, type VariantProps } from "class-variance-authority";
import { cn } from "@/shared/lib/utils";

const buttonVariants = cva("inline-flex items-center...", {
  variants: {
    variant: { default: "...", destructive: "..." },
    size: { sm: "...", lg: "..." },
  },
});

export interface ButtonProps
  extends React.ButtonHTMLAttributes<HTMLButtonElement>,
    VariantProps<typeof buttonVariants> {}

export function Button({ className, variant, size, ...props }: ButtonProps) {
  return (
    <button
      className={cn(buttonVariants({ variant, size }), className)}
      {...props}
    />
  );
}
```

---

## 🔒 Security Patterns

### Frontend

- **TenantGuard**: Chroni `/dashboard/*` (tylko Provider)
- **PortalGuard**: Chroni `/portal/*` (tylko Client)
- **Auth Interceptor**: Dodaje `Bearer` token do każdego requestu

### Backend (dla kontekstu)

```csharp
// ✅ DOBRE - zawsze filtruj po tenant
var tenantId = _tenantContext.CurrentTenantId;
var clients = await _repo.GetClientsForTenantAsync(tenantId, ct);

// ❌ ZŁE - security risk!
var clients = await _repo.GetAllAsync();
```

---

## 🧪 Testowanie

### Frontend

- **Unit**: Vitest + React Testing Library
- **E2E**: Playwright (krytyczne ścieżki)
- **Mocking**: Mockuj hooki Orvala, nie sieć

### Backend

- **Framework**: xUnit + FluentAssertions + Moq
- **Coverage**: >95% dla Domain/Application

---

## 📝 Komendy

```bash
# Development
npm run dev              # Start dev server (localhost:3000)
npm run typecheck        # TypeScript check
npm run lint             # ESLint

# API
npm run api:generate     # Regenerate hooks from Swagger

# Build
npm run build            # Production build
npm run start            # Start production server
```

---

## 🔄 Agent Workflow

### 1. Na początku sesji

```
1. Przeczytaj .agent/API_RULES.md
2. Sprawdź .agent/feature_list.json → znajdź passes: false
3. Sprawdź apiEndpoints i requiredHooks dla bloku
4. Otwórz Frontend_Prompts.md → znajdź BLOCK_START: X.X
```

### 2. Podczas pracy

```
- Wykonuj DOKŁADNIE kroki z promptu
- Używaj TYLKO hooków z @/core/api/generated/
- Testuj w przeglądarce po każdej zmianie
- Git commit po znaczących zmianach
```

### 3. Po zakończeniu bloku

```
1. npm run typecheck && npm run lint
2. Weryfikuj w DevTools (Network + Console + UI)
3. Zmień passes: true w feature_list.json
4. Dodaj wpis do claude-progress.txt
5. Git commit: feat(scope): description
```

---

## ❓ Endpoint nie istnieje?

**NIE MOCKUJ!** Zamiast tego:

1. Powiedz że endpoint nie istnieje w Swagger
2. Zaproponuj opcje:
   - Dodać endpoint w backendzie
   - Użyć istniejącego endpointu
   - Pominąć tę sekcję
3. Poczekaj na decyzję użytkownika

---

## 💡 Pro Tips

1. **Jeden blok = jedna sesja** - nie próbuj robić wielu bloków naraz
2. **Zawsze sprawdzaj dependencies** - blok 4A.1 wymaga ukończenia 3.x
3. **Git commit po każdym bloku** - czysta historia, łatwy rollback
4. **Network tab > Pretty UI** - prawdziwe dane są ważniejsze niż wygląd

---

> **Pamiętaj: Prawdziwe dane z API > Ładny UI z zerami** 🎯
