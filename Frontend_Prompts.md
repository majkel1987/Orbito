# Orbito Frontend - Prompts dla Agentów AI

> **🚀 FRESH START - 2025-12-06**
>
> Frontend jest tworzony OD NOWA. Wszystkie bloki mają status `passes: false`.
> Zaczynamy od **Bloku 0.1** (Inicjalizacja Projektu).

> **INSTRUKCJA DLA AGENTÓW AI**
>
> Ten plik zawiera szczegółowe prompty dla każdego bloku implementacji.
> Każdy blok jest oznaczony markerami `<!-- BLOCK_START: X.X -->` i `<!-- BLOCK_END: X.X -->`.
>
> **Workflow:**
>
> 1. Znajdź swój blok używając markera z `feature_list.json` (pole `promptMarker`)
> 2. Przeczytaj CAŁY prompt przed rozpoczęciem pracy
> 3. Wykonaj DOKŁADNIE kroki opisane w promptcie
> 4. Na końcu przejdź przez CHECKLIST WERYFIKACJI
> 5. Zaktualizuj `feature_list.json` i `claude-progress.txt`

---

<!-- BLOCK_START: 0.1 -->

## 🔵 FAZA 0: Setup & Configuration (Tydzień 1)

### 0.1 Inicjalizacja Projektu

| #     | Zadanie                              | Priorytet | Status | Opis                                                                |
| ----- | ------------------------------------ | --------- | ------ | ------------------------------------------------------------------- |
| 0.1.1 | 🔴 Utworzenie projektu Next.js 15    | Krytyczne | ⬜     | `create-next-app` z TypeScript, Tailwind, App Router, src directory |
| 0.1.2 | 🔴 Konfiguracja tsconfig.json strict | Krytyczne | ⬜     | strict: true, allowJs: false, wszystkie strict\* opcje              |
| 0.1.3 | 🔴 Struktura katalogów               | Krytyczne | ⬜     | Utworzenie features/, shared/, core/ zgodnie z planem               |

**Blok 0.1 - Wymagania wejściowe**: Brak  
**Blok 0.1 - Rezultat**: Działający projekt Next.js z TypeScript strict

**📦 DEPENDENCIES:**

- Brak (pierwszy blok)

**⬅️ BLOKUJE:**

- Blok 0.2 (Stylowanie)
- Blok 0.3 (API Layer)

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer specjalizujący się w Next.js.
Twoim zadaniem jest wykonanie Fazy 0.1 (Inicjalizacja Projektu) z planu implementacji.

### CEL GŁÓWNY:

Zainicjowanie nowego projektu Next.js 15 z pełną konfiguracją TypeScript (tryb strict) oraz utworzenie specyficznej struktury katalogów.

### WYMAGANIA TECHNICZNE:

1. Framework: Next.js 15 (App Router).
2. Język: TypeScript.
3. Stylowanie: Tailwind CSS.
4. Katalog źródłowy: `src/` directory.
5. Import alias: `@/*`.

### KROKI DO WYKONANIA:

**KROK 1: Inicjalizacja Projektu (Zadanie 0.1.1)**
Wygeneruj komendę (lub wykonaj ją, jeśli masz dostęp do terminala) `create-next-app` z następującymi flagami, aby uniknąć interaktywnych pytań:

- --typescript
- --tailwind
- --eslint
- --app
- --src-dir
- --import-alias "@/\*"
- --use-npm (lub --use-pnpm/--use-yarn zależnie od preferencji)

**KROK 2: Konfiguracja TypeScript (Zadanie 0.1.2)**
Zaktualizuj plik `tsconfig.json`. Musi być maksymalnie restrykcyjny ("strict").
Upewnij się, że zawiera:

- "strict": true
- "allowJs": false
- "noImplicitAny": true
- "strictNullChecks": true
- "strictFunctionTypes": true
- "strictBindCallApply": true
- "strictPropertyInitialization": true
- "noImplicitThis": true
- "alwaysStrict": true

**KROK 3: Struktura Katalogów (Zadanie 0.1.3)**
Wewnątrz katalogu `src/` usuń wszelki boilerplate (oprócz layout.tsx i page.tsx - wyczyść ich zawartość do minimum) i utwórz następującą strukturę katalogów:

- src/features/ (dla modułów funkcjonalnych)
- src/shared/ (dla komponentów UI, utils, hooks współdzielonych)
- src/core/ (dla konfiguracji, typów globalnych, stałych)

### OCZEKIWANY REZULTAT:

Gotowy do uruchomienia projekt, który kompiluje się bez błędów, posiada pustą strukturę folderów zgodnie z architekturą features/shared/core oraz restrykcyjny config TS.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [x] Projekt utworzony komendą `create-next-app` z wszystkimi flagami
- [x] `tsconfig.json` zawiera wszystkie strict opcje
- [x] Struktura katalogów: `src/features/`, `src/shared/`, `src/core/`
- [x] `npm run dev` uruchamia się bez błędów
- [x] `npm run build` kompiluje się bez błędów
- [x] `npm run typecheck` (jeśli skonfigurowany) przechodzi
- [x] Git commit z opisowym message: `feat(setup): initialize Next.js 15 project with TypeScript strict`

<!-- BLOCK_END: 0.1 -->

---

<!-- BLOCK_START: 0.2 -->

### 0.2 Stylowanie i UI Kit

| #     | Zadanie                        | Priorytet | Status | Opis                                                                      |
| ----- | ------------------------------ | --------- | ------ | ------------------------------------------------------------------------- |
| 0.2.1 | 🔴 Konfiguracja Tailwind CSS   | Krytyczne | ✅     | tailwind.config.ts z custom colors, fonts                                 |
| 0.2.2 | 🔴 Inicjalizacja shadcn/ui     | Krytyczne | ✅     | `npx shadcn@latest init`, konfiguracja components.json                    |
| 0.2.3 | 🔴 Import bazowych komponentów | Krytyczne | ✅     | Button, Input, Card, Dialog, DropdownMenu, Select, Badge, Skeleton, Toast |
| 0.2.4 | 🟡 Utility functions           | Ważne     | ✅     | cn() helper, formatters (currency, date)                                  |

**Blok 0.2 - Wymagania wejściowe**: Blok 0.1  
**Blok 0.2 - Rezultat**: Gotowy UI kit z shadcn/ui

**📦 DEPENDENCIES:**

- ✅ Blok 0.1 (Inicjalizacja Projektu)

**⬅️ BLOKUJE:**

- Blok 1.1 (NextAuth Configuration)
- Wszystkie komponenty UI

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer.
Kontynuujemy pracę nad projektem Next.js 15. Twoim zadaniem jest wykonanie Fazy 0.2 (Stylowanie i UI Kit) zgodnie z planem.

### CEL GŁÓWNY:

Skonfigurować Tailwind CSS, zainicjować bibliotekę shadcn/ui dostosowaną do naszej struktury katalogów oraz dodać niezbędne funkcje pomocnicze.

### KONTEKST STRUKTURY (z Fazy 0.1):

Mamy strukturę: `src/features`, `src/shared`, `src/core`.
Chcemy, aby reużywalne komponenty UI (shadcn) trafiały do: `src/shared/ui`.
Chcemy, aby funkcje pomocnicze (utils/lib) trafiały do: `src/shared/lib`.

### KROKI DO WYKONANIA:

**KROK 1: Inicjalizacja shadcn/ui (Zadanie 0.2.2)**
Uruchom (lub zasymuluj konfigurację) `npx shadcn@latest init`.
Skonfiguruj plik `components.json` tak, aby odzwierciedlał poniższe ustawienia (nadpisz domyślne ścieżki):

- Style: Default
- Base Color: Slate
- CSS Variables: Yes
- Aliases -> components: "@/shared/ui"
- Aliases -> utils: "@/shared/lib/utils"
- Aliases -> ui: "@/shared/ui" (jeśli dostępne)

_Upewnij się, że plik `globals.css` znajduje się w `src/app/globals.css` i zostanie zaktualizowany o zmienne CSS._

**KROK 2: Instalacja komponentów bazowych (Zadanie 0.2.3)**
Zainstaluj następujące komponenty za pomocą CLI shadcn:
`button`, `input`, `card`, `dialog`, `dropdown-menu`, `select`, `badge`, `skeleton`, `toast` (sonner lub standardowy toast).

**KROK 3: Weryfikacja i Konfiguracja Tailwind (Zadanie 0.2.1)**
Sprawdź plik `tailwind.config.ts`.

1. Upewnij się, że `content` obejmuje wszystkie nasze katalogi:
   - "./src/pages/\*_/_.{js,ts,jsx,tsx,mdx}"
   - "./src/components/\*_/_.{js,ts,jsx,tsx,mdx}"
   - "./src/app/\*_/_.{js,ts,jsx,tsx,mdx}"
   - "./src/features/\*_/_.{js,ts,jsx,tsx,mdx}" (BARDZO WAŻNE - dodaj to)
   - "./src/shared/\*_/_.{js,ts,jsx,tsx,mdx}" (BARDZO WAŻNE - dodaj to)
2. Jeśli shadcn nie dodał pluginu `tailwindcss-animate`, dodaj go.

**KROK 4: Utility Functions (Zadanie 0.2.4)**

1. Upewnij się, że funkcja `cn` (classNames) została wygenerowana w `src/shared/lib/utils.ts`.
2. W tym samym pliku (lub nowym `src/shared/lib/formatters.ts`) stwórz dwie funkcje:
   - `formatCurrency(amount: number, currency: string = 'PLN'): string` — formatująca walutę (użyj Intl.NumberFormat).
   - `formatDate(date: string | Date): string` — formatująca datę na polski format (np. 'DD.MM.YYYY').

### OCZEKIWANY REZULTAT:

Projekt z zainstalowanym shadcn/ui, gdzie komponenty (np. Button) znajdują się w `src/shared/ui/button.tsx`, a Tailwind poprawnie wykrywa klasy w folderach `features` i `shared`.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [x] `components.json` skonfigurowany z aliasami do `src/shared/ui`
- [x] Wszystkie wymienione komponenty shadcn zainstalowane
- [x] `tailwind.config.ts` zawiera ścieżki do `features/` i `shared/` (Tailwind v4 auto-detection)
- [x] Funkcja `cn()` działa w `src/shared/lib/utils.ts`
- [x] `formatCurrency()` i `formatDate()` zaimplementowane
- [x] Import `<Button>` z `@/shared/ui/button` działa
- [x] `npm run dev` wyświetla stronę z poprawnym stylem
- [x] Git commit: `feat(ui): setup shadcn/ui and utility functions`

<!-- BLOCK_END: 0.2 -->

---

<!-- BLOCK_START: 0.3 -->

### 0.3 API Layer Setup

| #     | Zadanie                         | Priorytet | Status | Opis                                                        |
| ----- | ------------------------------- | --------- | ------ | ----------------------------------------------------------- |
| 0.3.1 | 🔴 Instalacja orval             | Krytyczne | ⬜     | `npm install -D orval`                                      |
| 0.3.2 | 🔴 Konfiguracja orval.config.ts | Krytyczne | ⬜     | Input: swagger.json, output: generated/, react-query client |
| 0.3.3 | 🔴 Axios client setup           | Krytyczne | ⬜     | src/core/api/client.ts - bazowa instancja z baseURL         |
| 0.3.4 | 🔴 Result<T> interceptor        | Krytyczne | ⬜     | Rozpakowywanie Result<T>, mapowanie błędów                  |
| 0.3.5 | 🔴 Pierwsze generowanie API     | Krytyczne | ⬜     | `npm run api:generate` - weryfikacja że działa              |

**Blok 0.3 - Wymagania wejściowe**: Blok 0.1, działający backend ze Swagger  
**Blok 0.3 - Rezultat**: Wygenerowane typy i hooki z backendu

**📦 DEPENDENCIES:**

- ✅ Blok 0.1 (Inicjalizacja Projektu)
- ✅ Backend API ze Swagger endpoint

**⬅️ BLOKUJE:**

- Wszystkie bloki korzystające z API
- Blok 1.1 (NextAuth - potrzebuje axios client)

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer / API Integration Specialist.
Kontynuujemy pracę nad projektem Next.js 15. Twoim zadaniem jest wykonanie Fazy 0.3 (API Layer Setup) zgodnie z planem.

### CEL GŁÓWNY:

Zautomatyzować generowanie typów TypeScript i hooków React Query na podstawie OpenAPI Specification (Swagger) z backendu.
Skonfigurować interceptory do obsługi Result<T> i zarządzania tokenami JWT.

### KONTEKST:

Backend API zwraca dane w formacie Result<T>:

```typescript
{
  isSuccess: boolean;
  value?: T;
  error?: string;
  errors?: string[];
}
```

### KROKI DO WYKONANIA:

**KROK 1: Instalacja Dependencies (Zadanie 0.3.1)**

```bash
npm install -D orval
npm install @tanstack/react-query axios
```

**KROK 2: Konfiguracja Orval (Zadanie 0.3.2)**
Utwórz plik `orval.config.ts` w głównym katalogu projektu.
Wymagania:

- Input: URL Swaggera backendu (np. `http://localhost:5000/swagger/v1/swagger.json`)
- Output: `src/core/api/generated/`
- Client: `react-query`
- Mode: `tags-split` (każdy tag API w osobnym pliku)
- Override axios instance: `@/core/api/client`

Przykładowa konfiguracja:

```typescript
import { defineConfig } from "orval";

export default defineConfig({
  orbito: {
    input: "http://localhost:5000/swagger/v1/swagger.json",
    output: {
      mode: "tags-split",
      target: "src/core/api/generated/endpoints.ts",
      schemas: "src/core/api/generated/models",
      client: "react-query",
      override: {
        mutator: {
          path: "src/core/api/client.ts",
          name: "customInstance",
        },
      },
    },
  },
});
```

**KROK 3: Axios Client Setup (Zadanie 0.3.3)**
Utwórz `src/core/api/client.ts`.
Wymagania:

- Bazowa instancja Axios z `baseURL` z zmiennej środowiskowej
- Interceptor request: Dodawanie Bearer token z NextAuth session
- Export funkcji `customInstance` dla Orval

**KROK 4: Result<T> Interceptor (Zadanie 0.3.4)**
W pliku `src/core/api/client.ts` dodaj response interceptor.
Wymagania:

- Jeśli response.data ma pole `isSuccess`:
  - `true` → zwróć `data.value`
  - `false` → rzuć error z `data.error` lub `data.errors`
- Mapowanie błędów HTTP (401, 403, 500) na user-friendly messages

**KROK 5: Package.json Scripts (Zadanie 0.3.5)**
Dodaj skrypt:

```json
{
  "scripts": {
    "api:generate": "orval --config orval.config.ts"
  }
}
```

Uruchom `npm run api:generate` i zweryfikuj, że pliki zostały wygenerowane w `src/core/api/generated/`.

### OCZEKIWANY REZULTAT:

Działający system generowania API z automatycznym rozpakowywaniem Result<T>, gotowy do użycia w hookach React Query.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [x] Orval zainstalowany i skonfigurowany
- [x] `orval.config.ts` zawiera poprawną konfigurację
- [x] `src/core/api/client.ts` z axios instance i interceptorami
- [x] Result<T> interceptor poprawnie rozpakkowuje odpowiedzi
- [x] `npm run api:generate` generuje pliki bez błędów
- [x] Wygenerowane typy w `src/core/api/generated/`
- [x] `.env.local` zawiera `NEXT_PUBLIC_API_URL`
- [x] Git commit: `feat(api): setup orval and API layer with Result<T> handling`

<!-- BLOCK_END: 0.3 -->

---

<!-- BLOCK_START: 1.1 -->

## 🔵 FAZA 1: Authentication & Tenant Context (Tydzień 2)

### 1.1 NextAuth Configuration

| #     | Zadanie                      | Priorytet | Status | Opis                                               |
| ----- | ---------------------------- | --------- | ------ | -------------------------------------------------- |
| 1.1.1 | 🔴 NextAuth setup            | Krytyczne | ✅     | app/api/auth/[...nextauth]/route.ts z JWT          |
| 1.1.2 | 🔴 Credentials provider      | Krytyczne | ✅     | Autoryzacja przez backend API /auth/login          |
| 1.1.3 | 🔴 JWT callbacks             | Krytyczne | ✅     | Dołączanie tenantId, role do tokena                |
| 1.1.4 | 🔴 Session types             | Krytyczne | ✅     | Rozszerzenie NextAuth types (tenantId, role, name) |
| 1.1.5 | 🟡 NEXTAUTH_SECRET generator | Ważne     | ✅     | Skrypt generowania bezpiecznego secret             |

**Blok 1.1 - Wymagania wejściowe**: Blok 0.3 (API Layer)  
**Blok 1.1 - Rezultat**: Działający system logowania z JWT

**📦 DEPENDENCIES:**

- ✅ Blok 0.3 (API Layer)
- ✅ Backend endpoint /auth/login

**⬅️ BLOKUJE:**

- Blok 1.2 (Auth Guards)
- Blok 1.3 (Tenant Store)
- Wszystkie chronione strony

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer / Auth Specialist.
Mamy działający API Layer. Przechodzimy do Fazy 1.1: **Konfiguracja NextAuth**.

### CEL GŁÓWNY:

Skonfigurować NextAuth.js z Credentials Providerem, który komunikuje się z naszym backendem i zapisuje w session: `tenantId`, `role`, `userId`, `name`.

### KONTEKST BACKENDU:

Backend ma endpoint `POST /api/auth/login` zwracający:

```typescript
{
  isSuccess: true,
  value: {
    token: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    user: {
      id: "uuid",
      email: "user@example.com",
      name: "John Doe",
      role: "Provider" | "Client" | "TeamMember",
      tenantId: "uuid"
    }
  }
}
```

### KROKI DO WYKONANIA:

**KROK 1: Instalacja NextAuth (Zadanie 1.1.1)**

```bash
npm install next-auth@beta
```

**KROK 2: Konfiguracja Route Handler (Zadanie 1.1.1)**
Utwórz `src/app/api/auth/[...nextauth]/route.ts`.
Wymagania:

- Import `NextAuth` z `next-auth`
- Użyj `CredentialsProvider`
- W funkcji `authorize()`:
  - Wyślij request do backend API `/auth/login`
  - Jeśli sukces → zwróć obiekt user z tokenem
  - Jeśli błąd → zwróć null

**KROK 3: JWT i Session Callbacks (Zadanie 1.1.3)**
W konfiguracji NextAuth dodaj callbacks:

```typescript
callbacks: {
  async jwt({ token, user }) {
    // Przy logowaniu user będzie dostępny
    if (user) {
      token.accessToken = user.token;
      token.userId = user.id;
      token.role = user.role;
      token.tenantId = user.tenantId;
      token.name = user.name;
    }
    return token;
  },
  async session({ session, token }) {
    session.accessToken = token.accessToken;
    session.user.id = token.userId;
    session.user.role = token.role;
    session.user.tenantId = token.tenantId;
    session.user.name = token.name;
    return session;
  },
}
```

**KROK 4: Type Definitions (Zadanie 1.1.4)**
Utwórz `src/core/types/next-auth.d.ts`:

```typescript
import { DefaultSession } from "next-auth";

declare module "next-auth" {
  interface Session {
    accessToken: string;
    user: {
      id: string;
      role: "Provider" | "Client" | "TeamMember" | "PlatformAdmin";
      tenantId: string;
      name: string;
    } & DefaultSession["user"];
  }

  interface User {
    token: string;
    id: string;
    role: string;
    tenantId: string;
    name: string;
  }
}

declare module "next-auth/jwt" {
  interface JWT {
    accessToken: string;
    userId: string;
    role: string;
    tenantId: string;
  }
}
```

**KROK 5: Environment Variables (Zadanie 1.1.5)**
W pliku `.env.local` dodaj:

```
NEXTAUTH_SECRET=wygeneruj-bezpieczny-secret-openssl-rand-base64-32
NEXTAUTH_URL=http://localhost:3000
```

### OCZEKIWANY REZULTAT:

Działający endpoint `/api/auth/signin` i możliwość wywołania `signIn('credentials', {...})` z formularza.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [x] NextAuth zainstalowany
- [x] Route handler w `app/api/auth/[...nextauth]/route.ts`
- [x] Credentials provider komunikuje się z backendem
- [x] JWT callback zapisuje token i dane user
- [x] Session callback wypełnia session.user
- [x] Type definitions w `next-auth.d.ts`
- [x] `.env.local` zawiera NEXTAUTH_SECRET i NEXTAUTH_URL
- [ ] Test: `signIn('credentials')` działa
- [x] Git commit: `feat(auth): configure NextAuth with JWT and tenant context`

<!-- BLOCK_END: 1.1 -->

---

<!-- BLOCK_START: 1.2 -->

### 1.2 Auth Guards & Middleware

| #     | Zadanie                  | Priorytet | Status | Opis                                             |
| ----- | ------------------------ | --------- | ------ | ------------------------------------------------ |
| 1.2.1 | 🔴 Next.js Middleware    | Krytyczne | ✅     | middleware.ts - ochrona tras /dashboard, /portal |
| 1.2.2 | 🔴 Role-based redirects  | Krytyczne | ✅     | Provider → /dashboard, Client → /portal          |
| 1.2.3 | 🔴 TenantGuard component | Krytyczne | ✅     | Client component weryfikujący tenantId w URL     |
| 1.2.4 | 🟡 useAuth hook          | Ważne     | ✅     | Custom hook opakowujący useSession z type safety |

**Blok 1.2 - Wymagania wejściowe**: Blok 1.1 (NextAuth)  
**Blok 1.2 - Rezultat**: Chronione trasy z weryfikacją tenant context

**📦 DEPENDENCIES:**

- ✅ Blok 1.1 (NextAuth Configuration)

**⬅️ BLOKUJE:**

- Wszystkie strony wymagające autentykacji
- Blok 2.1 (Dashboard Layout)

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer / Security Specialist.
Mamy działający NextAuth. Przechodzimy do Fazy 1.2: **Auth Guards & Middleware**.

### CEL GŁÓWNY:

Zabezpieczyć trasy aplikacji przed nieautoryzowanym dostępem oraz zaimplementować przekierowania oparte na rolach użytkownika.

### KONTEKST:

Aplikacja ma dwie główne strefy:

- `/dashboard/*` → dla Provider/TeamMember
- `/portal/*` → dla Client

### KROKI DO WYKONANIA:

**KROK 1: Next.js Middleware (Zadanie 1.2.1)**
Utwórz `src/middleware.ts`.
Wymagania:

- Import `NextResponse` i `getToken` z NextAuth
- Matcher: `/dashboard/:path*`, `/portal/:path*`
- Logika:
  - Brak tokena → redirect `/login`
  - Provider/TeamMember próbuje wejść na `/portal` → redirect `/dashboard`
  - Client próbuje wejść na `/dashboard` → redirect `/portal`

Przykładowy kod:

```typescript
import { NextResponse } from "next/server";
import { getToken } from "next-auth/jwt";
import type { NextRequest } from "next/server";

export async function middleware(request: NextRequest) {
  const token = await getToken({ req: request });
  const { pathname } = request.nextUrl;

  // Brak sesji - redirect do login
  if (!token) {
    const loginUrl = new URL("/login", request.url);
    loginUrl.searchParams.set("callbackUrl", pathname);
    return NextResponse.redirect(loginUrl);
  }

  // Role-based routing
  const role = token.role as string;

  if (pathname.startsWith("/dashboard") && role === "Client") {
    return NextResponse.redirect(new URL("/portal", request.url));
  }

  if (pathname.startsWith("/portal") && role !== "Client") {
    return NextResponse.redirect(new URL("/dashboard", request.url));
  }

  return NextResponse.next();
}

export const config = {
  matcher: ["/dashboard/:path*", "/portal/:path*"],
};
```

**KROK 2: TenantGuard Component (Zadanie 1.2.3)**
Utwórz `src/features/auth/components/TenantGuard.tsx`.
Wymagania:

- "use client"
- Weryfikuje czy `session.user.tenantId` zgadza się z `tenantId` w URL
- Jeśli nie → pokazuje error page lub redirect

**KROK 3: useAuth Hook (Zadanie 1.2.4)**
Utwórz `src/features/auth/hooks/useAuth.ts`.
Wymagania:

```typescript
import { useSession } from "next-auth/react";

export function useAuth() {
  const { data: session, status } = useSession();

  return {
    user: session?.user,
    isLoading: status === "loading",
    isAuthenticated: status === "authenticated",
    role: session?.user?.role,
    tenantId: session?.user?.tenantId,
  };
}
```

### OCZEKIWANY REZULTAT:

Middleware chroniący trasy, komponenty guard oraz custom hook do łatwego dostępu do danych sesji.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [x] `middleware.ts` chroni `/dashboard` i `/portal`
- [x] Redirect do `/login` gdy brak sesji
- [x] Role-based routing działa poprawnie
- [x] TenantGuard weryfikuje tenant context
- [x] useAuth hook zwraca typed user data
- [x] Test: próba dostępu do `/dashboard` bez logowania
- [x] Test: Client nie może wejść na `/dashboard`
- [x] Git commit: `feat(auth): implement middleware and auth guards`

<!-- BLOCK_END: 1.2 -->

---

<!-- BLOCK_START: 1.3 -->

### 1.3 Tenant Context & Store

| #     | Zadanie                   | Priorytet | Status | Opis                                       |
| ----- | ------------------------- | --------- | ------ | ------------------------------------------ |
| 1.3.1 | 🔴 Zustand store          | Krytyczne | ⬜     | Tenant state (id, name, settings)          |
| 1.3.2 | 🔴 useTenant hook         | Krytyczne | ⬜     | Wrapper do łatwego dostępu                 |
| 1.3.3 | 🔴 TenantProvider wrapper | Krytyczne | ⬜     | Inicjalizacja store z session              |
| 1.3.4 | 🟡 TenantSwitcher         | Ważne     | ⬜     | Component do zmiany contextu (jeśli multi) |

**Blok 1.3 - Wymagania wejściowe**: Blok 1.1 (NextAuth)  
**Blok 1.3 - Rezultat**: Globalny state zarządzający kontekstem tenanta

**📦 DEPENDENCIES:**

- ✅ Blok 1.1 (NextAuth Configuration)

**⬅️ BLOKUJE:**

- Wszystkie features wymagające tenant context
- Blok 3.1 (Clients Management)

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer / State Management Specialist.
Mamy działający NextAuth z tenant context w session. Przechodzimy do Fazy 1.3: **Tenant Context & Store**.

### CEL GŁÓWNY:

Stworzyć globalny state (Zustand) do zarządzania aktywnym tenantem oraz komponent Provider inicjalizujący store danymi z session.

### KONTEKST:

Multi-tenancy wymaga, aby każda operacja była wykonywana w kontekście konkretnego tenanta (Provider).
Dane tenanta są dostępne w `session.user.tenantId`.

### KROKI DO WYKONANIA:

**KROK 1: Zustand Store (Zadanie 1.3.1)**
Utwórz `src/features/auth/store/tenant.store.ts`.
Wymagania:

```typescript
import { create } from "zustand";

interface TenantState {
  tenantId: string | null;
  tenantName: string | null;
  setTenant: (id: string, name: string) => void;
  clearTenant: () => void;
}

export const useTenantStore = create<TenantState>((set) => ({
  tenantId: null,
  tenantName: null,
  setTenant: (id, name) => set({ tenantId: id, tenantName: name }),
  clearTenant: () => set({ tenantId: null, tenantName: null }),
}));
```

**KROK 2: useTenant Hook (Zadanie 1.3.2)**
Utwórz `src/features/auth/hooks/useTenant.ts`.
Wymagania:

```typescript
import { useTenantStore } from "../store/tenant.store";

export function useTenant() {
  const { tenantId, tenantName, setTenant, clearTenant } = useTenantStore();

  return {
    tenantId,
    tenantName,
    setTenant,
    clearTenant,
    isReady: !!tenantId,
  };
}
```

**KROK 3: TenantProvider (Zadanie 1.3.3)**
Utwórz `src/features/auth/components/TenantProvider.tsx`.
Wymagania:

- "use client"
- Pobiera session przez `useSession()`
- Efekt: gdy session.user.tenantId zmienia się → wywołaj `setTenant()`
- Render: `{children}`

**KROK 4: TenantSwitcher Component (Zadanie 1.3.4)**
Utwórz `src/features/auth/components/TenantSwitcher.tsx`.
Wymagania:

- "use client"
- Wyświetla aktualnego tenanta (name)
- W przyszłości: dropdown do zmiany (jeśli user ma dostęp do wielu tenantów)
- MVP: Tylko display, bez switchowania

### OCZEKIWANY REZULTAT:

Działający Zustand store z tenant context oraz Provider inicjalizujący store przy logowaniu.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [ ] Zustand store w `tenant.store.ts`
- [ ] useTenant hook działa
- [ ] TenantProvider inicjalizuje store
- [ ] TenantSwitcher wyświetla tenant name
- [ ] Store aktualizuje się po logowaniu
- [ ] Store czyszczony przy wylogowaniu
- [ ] Test: useTenant() zwraca poprawne dane
- [ ] Git commit: `feat(auth): implement tenant context store with Zustand`

<!-- BLOCK_END: 1.3 -->

---

<!-- BLOCK_START: 1.4 -->

### 1.4 Login & Register Pages

| #     | Zadanie              | Priorytet | Status | Opis                                   |
| ----- | -------------------- | --------- | ------ | -------------------------------------- |
| 1.4.1 | 🔴 Auth layout       | Krytyczne | ⬜     | (auth)/layout.tsx - centered content   |
| 1.4.2 | 🔴 Login form schema | Krytyczne | ⬜     | Zod schema dla email + password        |
| 1.4.3 | 🔴 Login page        | Krytyczne | ⬜     | /login - React Hook Form + NextAuth    |
| 1.4.4 | 🔴 Register page     | Krytyczne | ⬜     | /register - mock API call              |
| 1.4.5 | 🟡 Auth error page   | Ważne     | ⬜     | /auth/error - wyświetlanie błędów auth |

**Blok 1.4 - Wymagania wejściowe**: Blok 1.1 (NextAuth), Blok 1.3 (Tenant Store)  
**Blok 1.4 - Rezultat**: Działające strony logowania i rejestracji

**📦 DEPENDENCIES:**

- ✅ Blok 0.2 (shadcn/ui components)
- ✅ Blok 1.1 (NextAuth)
- ✅ Blok 1.3 (Tenant Store)

**⬅️ BLOKUJE:**

- Wszystkie features (wymaga możliwości logowania)

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer / Forms Specialist.
Mamy działający NextAuth i tenant store. Przechodzimy do Fazy 1.4: **Login & Register Pages**.

### CEL GŁÓWNY:

Stworzyć profesjonalne strony logowania i rejestracji z walidacją formularzy (Zod + React Hook Form) oraz integracją z NextAuth.

### KONTEKST:

Używamy shadcn/ui do UI, React Hook Form do zarządzania stanem formularza, Zod do walidacji.

### KROKI DO WYKONANIA:

**KROK 1: Validation Schemas (Zadanie 1.4.2)**
Utwórz `src/features/auth/schemas/auth.schemas.ts`.
Wymagania:

```typescript
import { z } from "zod";

export const LoginSchema = z.object({
  email: z.string().email("Invalid email address"),
  password: z.string().min(8, "Password must be at least 8 characters"),
});

export const RegisterSchema = z
  .object({
    name: z.string().min(2, "Name must be at least 2 characters"),
    email: z.string().email("Invalid email address"),
    password: z.string().min(8, "Password must be at least 8 characters"),
    confirmPassword: z.string(),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: "Passwords don't match",
    path: ["confirmPassword"],
  });

export type LoginInput = z.infer<typeof LoginSchema>;
export type RegisterInput = z.infer<typeof RegisterSchema>;
```

**KROK 2: LoginForm Component (Zadanie 1.4.3)**
Utwórz `src/features/auth/components/LoginForm.tsx`.
Wymagania:

- "use client"
- React Hook Form z zodResolver
- shadcn/ui: Card, Input, Button
- Submit: `signIn('credentials', { email, password })`
- Loading state podczas logowania
- Error toast jeśli logowanie się nie powiedzie
- Link "Don't have an account? Register"

**KROK 3: RegisterForm Component (Zadanie 1.4.4)**
Utwórz `src/features/auth/components/RegisterForm.tsx`.
Wymagania:

- Analogicznie do LoginForm (Card UI, React Hook Form, Zod).
- Pola: Name, Email, Password, Confirm Password.
- Submit: Na ten moment (MVP) tylko zamockuj wywołanie API i po 1s przekieruj do `/login` z toastem sukcesu.
- Link do logowania na dole.

**KROK 4: Strony i Layout (Zadania 1.4.1, 1.4.3, 1.4.5)**

1. Utwórz `src/app/(auth)/layout.tsx`: Prosty layout centrujący zawartość.
2. Utwórz `src/app/(auth)/login/page.tsx`: Renderuje `LoginForm`.
3. Utwórz `src/app/(auth)/register/page.tsx`: Renderuje `RegisterForm`.
4. Utwórz `src/app/(auth)/auth/error/page.tsx`: Karta błędu z przyciskiem "Back to Login".

### OCZEKIWANY REZULTAT:

Kod dla schematów, komponentów formularzy oraz stron w Next.js App Router.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [ ] `LoginSchema` i `RegisterSchema` w Zod
- [ ] `LoginForm` z React Hook Form i shadcn/ui
- [ ] `RegisterForm` z walidacją
- [ ] Auth layout centruje content
- [ ] `/login` wyświetla formularz logowania
- [ ] `/register` wyświetla formularz rejestracji
- [ ] `/auth/error` wyświetla stronę błędu
- [ ] Logowanie działa z mock credentials
- [ ] Loading states podczas submit
- [ ] Error states przy błędnych danych
- [ ] Git commit: `feat(auth): implement login and register pages`

<!-- BLOCK_END: 1.4 -->

---

<!-- BLOCK_START: 2.1 -->

## 🔵 FAZA 2: Layout & Global UI (Tydzień 3-4)

### 2.1 Layout Components

| #     | Zadanie               | Priorytet  | Status | Opis                                               |
| ----- | --------------------- | ---------- | ------ | -------------------------------------------------- |
| 2.1.1 | 🔴 Dashboard layout   | Krytyczne  | ⬜     | (dashboard)/layout.tsx z Sidebar + Header          |
| 2.1.2 | 🔴 Sidebar component  | Krytyczne  | ⬜     | Nawigacja z ikonami, active state, role-based menu |
| 2.1.3 | 🔴 Header component   | Krytyczne  | ⬜     | Logo, UserMenu, notifications placeholder          |
| 2.1.4 | 🔴 UserMenu component | Krytyczne  | ⬜     | Dropdown z avatar, role badge, logout              |
| 2.1.5 | 🟢 Footer component   | Opcjonalne | ⬜     | Copyright, links                                   |

**Blok 2.1 - Wymagania wejściowe**: Faza 1 (Auth zakończone)  
**Blok 2.1 - Rezultat**: Gotowy szkielet aplikacji (Layout) z nawigacją

**📦 DEPENDENCIES:**

- ✅ Blok 1.1, 1.2, 1.3, 1.4 (cała Faza 1)

**⬅️ BLOKUJE:**

- Blok 2.2 (Global State)
- Wszystkie strony dashboard

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer / UI Specialist.
Mamy działającą autentykację. Przechodzimy do Fazy 2.1: **Implementacja Głównego Layoutu Dashboardu**.

### CEL GŁÓWNY:

Stworzyć responsywny layout aplikacji ("shell"), który będzie zawierał boczny pasek nawigacyjny (Sidebar), górny pasek (Header) oraz miejsce na zmienną zawartość (page content).

### STRUKTURA KATALOGÓW:

Komponenty layoutu mają trafić do: `src/shared/components/layout/`
Layout strony: `src/app/(dashboard)/layout.tsx`

### KROKI DO WYKONANIA:

**KROK 1: Sidebar Component (Zadanie 2.1.2)**
Utwórz `src/shared/components/layout/Sidebar.tsx` ("use client").
Wymagania:

- Zdefiniuj tablicę nawigacji: Dashboard, Team, Clients, Plans, Subscriptions, Payments, Analytics.
- Użyj ikon z `lucide-react` pasujących do każdej sekcji.
- Active State: Użyj hooka `usePathname`. Jeśli link pokrywa się z obecną ścieżką, nadaj mu inny styl.
- Styl: Fixed width (np. w-64) na desktopie, ukryty lub jako Drawer na mobile.

**KROK 2: UserMenu Component (Zadanie 2.1.4)**
Utwórz `src/shared/components/layout/UserMenu.tsx` ("use client").
Wymagania:

- Pobierz dane użytkownika z `useSession`.
- Użyj `DropdownMenu` z `shadcn/ui`.
- Trigger: Avatar użytkownika.
- Content: Label z imieniem, Items: Profile, Settings, Log out.

**KROK 3: Header Component (Zadanie 2.1.3)**
Utwórz `src/shared/components/layout/Header.tsx`.
Wymagania:

- Flex container.
- Lewa strona: Mobile Menu Trigger + Logo.
- Prawa strona: `UserMenu` + `TenantSwitcher`.

**KROK 4: Dashboard Layout (Zadanie 2.1.1)**
Utwórz `src/app/(dashboard)/layout.tsx`.
Wymagania:

- Struktura: Sidebar (fixed left) + Main Content Area.
- Header (sticky top).
- Layout ma być responsywny.

### OCZEKIWANY REZULTAT:

Kod dla Sidebara, Headera, UserMenu oraz głównego Layoutu.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [ ] Sidebar z nawigacją i active states
- [ ] UserMenu z dropdown
- [ ] Header z logo i user menu
- [ ] Dashboard layout łączy wszystko
- [ ] Responsywność na mobile
- [ ] Logout działa
- [ ] Git commit: `feat(layout): implement dashboard layout with sidebar and header`

<!-- BLOCK_END: 2.1 -->

---

<!-- BLOCK_START: 2.2 -->

### 2.2 Global State & Query Client

| #     | Zadanie                      | Priorytet | Status | Opis                                   |
| ----- | ---------------------------- | --------- | ------ | -------------------------------------- |
| 2.2.1 | 🔴 React Query setup         | Krytyczne | ⬜     | QueryClientProvider w layout           |
| 2.2.2 | 🔴 Error boundary            | Krytyczne | ⬜     | Global error handling component        |
| 2.2.3 | 🟡 Loading states            | Ważne     | ⬜     | Suspense boundaries, loading skeletons |
| 2.2.4 | 🟡 Toast notifications setup | Ważne     | ⬜     | Sonner provider, useToast wrapper      |

**Blok 2.2 - Wymagania wejściowe**: Blok 2.1 (Layout)  
**Blok 2.2 - Rezultat**: Globalny state management i UI feedback

**📦 DEPENDENCIES:**

- ✅ Blok 2.1 (Dashboard Layout)
- ✅ Blok 0.2 (shadcn/ui Toast)

**⬅️ BLOKUJE:**

- Wszystkie features używające React Query
- Blok 3.1 (Clients)

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer / State Management Specialist.
Mamy działający Layout. Przechodzimy do Fazy 2.2: **Global State & Query Client**.

### CEL GŁÓWNY:

Skonfigurować React Query (TanStack Query) oraz globalne mechanizmy obsługi błędów i powiadomień.

### KROKI DO WYKONANIA:

**KROK 1: Query Client Setup (Zadanie 2.2.1)**
Utwórz `src/core/providers/QueryProvider.tsx`.
Wymagania:

- "use client"
- Konfiguracja QueryClient z sensownymi defaultami:
  - `staleTime`: 60000 (1 min)
  - `retry`: 1
- Wrap children w `QueryClientProvider`

Dodaj Provider do głównego layout: `src/app/layout.tsx`.

**KROK 2: Error Boundary (Zadanie 2.2.2)**
Utwórz `src/shared/components/ErrorBoundary.tsx`.
Wymagania:

- Class component (Error Boundaries w React)
- Catch errors i wyświetl user-friendly message
- Button "Try Again" resetujący error state

**KROK 3: Loading Skeletons (Zadanie 2.2.3)**
Utwórz kilka reużywalnych komponentów skeleton:

- `src/shared/components/TableSkeleton.tsx`
- `src/shared/components/CardSkeleton.tsx`
- `src/shared/components/FormSkeleton.tsx`

Użyj `Skeleton` z shadcn/ui.

**KROK 4: Toast Setup (Zadanie 2.2.4)**

1. Sprawdź czy `Toaster` z Sonner jest dodany do layout
2. Utwórz `src/shared/hooks/useToast.ts`:

```typescript
import { toast as sonnerToast } from "sonner";

export function useToast() {
  return {
    success: (message: string) => sonnerToast.success(message),
    error: (message: string) => sonnerToast.error(message),
    info: (message: string) => sonnerToast.info(message),
  };
}
```

### OCZEKIWANY REZULTAT:

Konfiguracja React Query, Error Boundary, Loading Skeletons i Toast system.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [ ] QueryClientProvider w root layout
- [ ] QueryClient z sensownymi defaults
- [ ] ErrorBoundary łapie błędy
- [ ] Skeleton components gotowe
- [ ] useToast hook działa
- [ ] Toaster component w layout
- [ ] Test: useQuery() działa
- [ ] Git commit: `feat(core): setup React Query and global error handling`

<!-- BLOCK_END: 2.2 -->

---

<!-- BLOCK_START: 3.1 -->

## 🔵 FAZA 3: Shared Data Layer (Tydzień 4)

### 3.1 Clients Management Foundation

| #     | Zadanie                      | Priorytet | Status | Opis                                          |
| ----- | ---------------------------- | --------- | ------ | --------------------------------------------- |
| 3.1.1 | 🔴 Client types & validation | Krytyczne | ⬜     | Zod schemas dla Client CRUD                   |
| 3.1.2 | 🔴 useClients hooks          | Krytyczne | ⬜     | useQuery + useMutation dla clients            |
| 3.1.3 | 🔴 ClientsTable component    | Krytyczne | ⬜     | Tabela z sortowaniem, filtrowaniem, paginacją |
| 3.1.4 | 🔴 ClientForm component      | Krytyczne | ⬜     | Dialog z formularzem add/edit                 |
| 3.1.5 | 🔴 Clients list page         | Krytyczne | ⬜     | /dashboard/clients - integracja wszystkiego   |

**Blok 3.1 - Wymagania wejściowe**: Faza 2 (Layout & State)  
**Blok 3.1 - Rezultat**: Działający moduł zarządzania klientami

**📦 DEPENDENCIES:**

- ✅ Blok 2.1 (Dashboard Layout)
- ✅ Blok 2.2 (React Query)
- ✅ Blok 0.3 (API Generated)

**⬅️ BLOKUJE:**

- Blok 5.1 (Subscriptions - potrzebuje clients)

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer / Feature Developer.
Mamy gotowy Layout i State Management. Przechodzimy do Fazy 3.1: **Clients Management Foundation**.

### CEL GŁÓWNY:

Stworzyć kompletny moduł CRUD dla klientów z profesjonalną tabelą, formularzami i integracją z API.

### ARCHITEKTURA (Vertical Slice):

```
src/features/clients/
├── components/
│   ├── ClientsTable.tsx
│   ├── ClientForm.tsx
│   └── ClientActions.tsx
├── hooks/
│   └── useClients.ts
├── schemas/
│   └── client.schemas.ts
└── types/
    └── client.types.ts
```

### KROKI DO WYKONANIA:

**KROK 1: Types & Validation (Zadanie 3.1.1)**
Utwórz `src/features/clients/schemas/client.schemas.ts`.
Wymagania:

```typescript
import { z } from "zod";

export const ClientSchema = z.object({
  name: z.string().min(2, "Name must be at least 2 characters"),
  email: z.string().email("Invalid email address"),
  phone: z.string().optional(),
  company: z.string().optional(),
  notes: z.string().optional(),
});

export type ClientInput = z.infer<typeof ClientSchema>;
```

**KROK 2: API Hooks (Zadanie 3.1.2)**
Utwórz `src/features/clients/hooks/useClients.ts`.
Wymagania:

- Użyj wygenerowanych hooków z orval (lub stwórz wrapper)
- `useClients()`: Query pobierająca listę z filtrowaniem
- `useCreateClient()`: Mutation
- `useUpdateClient()`: Mutation
- `useDeleteClient()`: Mutation
- Wszystkie mutacje invalidują query cache

**KROK 3: ClientsTable (Zadanie 3.1.3)**
Utwórz `src/features/clients/components/ClientsTable.tsx`.
Wymagania:

- shadcn/ui Table
- Kolumny: Name, Email, Company, Status, Actions
- Sortowanie (client-side lub server-side)
- Filtrowanie po nazwie/email
- Pagination
- Actions dropdown: Edit, Delete

**KROK 4: ClientForm (Zadanie 3.1.4)**
Utwórz `src/features/clients/components/ClientForm.tsx`.
Wymagania:

- Dialog z formem
- React Hook Form + Zod validation
- Mode: "create" lub "edit"
- Submit → wywołanie odpowiedniej mutation
- Toast na sukces/błąd

**KROK 5: Clients Page (Zadanie 3.1.5)**
Utwórz `src/app/(dashboard)/dashboard/clients/page.tsx`.
Wymagania:

- Header z przyciskiem "Add Client"
- Statystyki (Total Clients, Active, Inactive)
- ClientsTable
- Search bar

### OCZEKIWANY REZULTAT:

Pełny CRUD dla klientów z profesjonalnym UI.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [ ] Client schemas z Zod validation
- [ ] useClients hooks działają
- [ ] ClientsTable wyświetla dane z API
- [ ] Sortowanie i filtrowanie działa
- [ ] ClientForm tworzy nowych klientów
- [ ] Edit client działa
- [ ] Delete client działa
- [ ] Toast notifications
- [ ] Loading states
- [ ] Error handling
- [ ] Git commit: `feat(clients): implement full CRUD with table and forms`

<!-- BLOCK_END: 3.1 -->

---

<!-- BLOCK_START: 3.2 -->

### 3.2 Plans Management Foundation

| #     | Zadanie                 | Priorytet | Status | Opis                                         |
| ----- | ----------------------- | --------- | ------ | -------------------------------------------- |
| 3.2.1 | 🔴 Plan types & schemas | Krytyczne | ⬜     | Zod schemas dla Plan CRUD                    |
| 3.2.2 | 🔴 usePlans hooks       | Krytyczne | ⬜     | useQuery + useMutation                       |
| 3.2.3 | 🔴 PlansGrid component  | Krytyczne | ⬜     | Card grid z cenami i features                |
| 3.2.4 | 🔴 PlanForm component   | Krytyczne | ⬜     | Dialog z multi-step form (details + pricing) |
| 3.2.5 | 🔴 Plans page           | Krytyczne | ⬜     | /dashboard/plans                             |

**Blok 3.2 - Wymagania wejściowe**: Blok 3.1 (Clients)  
**Blok 3.2 - Rezultat**: Moduł zarządzania planami subskrypcyjnymi

**📦 DEPENDENCIES:**

- ✅ Blok 2.1 (Dashboard Layout)
- ✅ Blok 2.2 (React Query)
- ✅ Blok 3.1 (Clients - używają planów)

**⬅️ BLOKUJE:**

- Blok 5.1 (Subscriptions - potrzebuje planów)

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer.
Przechodzimy do Fazy 3.2: **Plans Management Foundation**.

### CEL GŁÓWNY:

Stworzyć moduł zarządzania planami subskrypcyjnymi (Subscription Plans) z możliwością definiowania cen, interwałów rozliczeniowych i feature lists.

### ARCHITEKTURA:

```
src/features/plans/
├── components/
│   ├── PlansGrid.tsx
│   ├── PlanCard.tsx
│   ├── PlanForm.tsx
│   └── PlanFeatures.tsx
├── hooks/
│   └── usePlans.ts
└── schemas/
    └── plan.schemas.ts
```

### KROKI DO WYKONANIA:

**KROK 1: Schemas (Zadanie 3.2.1)**
Utwórz `src/features/plans/schemas/plan.schemas.ts`.
Wymagania:

```typescript
import { z } from "zod";

export const PlanSchema = z.object({
  name: z.string().min(2),
  description: z.string().optional(),
  price: z.number().min(0),
  interval: z.enum(["MONTHLY", "YEARLY", "QUARTERLY"]),
  features: z.array(z.string()),
  isActive: z.boolean().default(true),
});

export type PlanInput = z.infer<typeof PlanSchema>;
```

**KROK 2: API Hooks (Zadanie 3.2.2)**
Utwórz `src/features/plans/hooks/usePlans.ts`.
Analogicznie do useClients.

**KROK 3: PlansGrid & PlanCard (Zadanie 3.2.3)**
Utwórz komponenty wyświetlające plany w formie kart.
Wymagania:

- Grid layout (3 kolumny na desktop)
- Każda karta: Name, Price, Interval, Feature list
- Badge "Active/Inactive"
- Actions: Edit, Delete, Duplicate

**KROK 4: PlanForm (Zadanie 3.2.4)**
Multi-step form lub pojedynczy form z sekcjami:

- Section 1: Basic Info (Name, Description)
- Section 2: Pricing (Price, Interval)
- Section 3: Features (Dynamic list - add/remove)

**KROK 5: Plans Page (Zadanie 3.2.5)**
`src/app/(dashboard)/dashboard/plans/page.tsx`

- Header z "Create Plan"
- PlansGrid
- Stats: Total Plans, Active Plans

### OCZEKIWANY REZULTAT:

Działający CRUD dla planów subskrypcyjnych.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [ ] Plan schemas gotowe
- [ ] usePlans hooks działają
- [ ] PlansGrid wyświetla karty
- [ ] PlanForm tworzy/edytuje plany
- [ ] Feature list jest dynamiczna
- [ ] Delete plan działa
- [ ] Toast notifications
- [ ] Git commit: `feat(plans): implement subscription plans management`

<!-- BLOCK_END: 3.2 -->

---

<!-- BLOCK_START: 4A.1 -->

## 🔵 FAZA 4A: Clients Detail (Tydzień 5) - PARALLEL z 4B

### 4A.1 Client Detail Page

| #      | Zadanie                        | Priorytet | Status | Opis                                          |
| ------ | ------------------------------ | --------- | ------ | --------------------------------------------- |
| 4A.1.1 | 🔴 Client detail layout        | Krytyczne | ⬜     | /clients/[id] - tabs: Overview, Subscriptions |
| 4A.1.2 | 🔴 ClientHeader component      | Krytyczne | ⬜     | Name, email, status, edit button              |
| 4A.1.3 | 🔴 ClientOverview tab          | Krytyczne | ⬜     | Info cards, activity timeline                 |
| 4A.1.4 | 🔴 ClientSubscriptions tab     | Krytyczne | ⬜     | Lista subskrypcji tego klienta                |
| 4A.1.5 | 🟡 ClientActivityLog component | Ważne     | ⬜     | Timeline z historią zmian                     |

**Blok 4A.1 - Wymagania wejściowe**: Blok 3.1 (Clients List)  
**Blok 4A.1 - Rezultat**: Szczegółowy widok klienta z zakładkami

**📦 DEPENDENCIES:**

- ✅ Blok 3.1 (Clients Management)

**⬅️ BLOKUJE:**

- Blok 5.1 (Subscriptions - korzysta z client detail)

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer.
Przechodzimy do Fazy 4A.1: **Client Detail Page**.

### CEL GŁÓWNY:

Stworzyć szczegółowy widok pojedynczego klienta z zakładkami: Overview i Subscriptions.

### ARCHITEKTURA:

```
src/features/clients/
└── components/
    ├── detail/
    │   ├── ClientHeader.tsx
    │   ├── ClientOverview.tsx
    │   ├── ClientSubscriptions.tsx
    │   └── ClientActivityLog.tsx
```

### KROKI DO WYKONANIA:

**KROK 1: Dynamic Route (Zadanie 4A.1.1)**
Utwórz `src/app/(dashboard)/dashboard/clients/[id]/page.tsx`.
Wymagania:

- Async component (Next.js 15)
- `await params` do pobrania ID
- Tabs: Overview, Subscriptions, Activity

**KROK 2: ClientHeader (Zadanie 4A.1.2)**
Wymagania:

- Avatar (lub initials)
- Name + Email
- Status badge
- Edit button (otwiera ClientForm w trybie edit)

**KROK 3: ClientOverview (Zadanie 4A.1.3)**
Wymagania:

- Info cards: Total Subscriptions, Active Plans, Total Revenue
- Recent activity timeline (placeholder)

**KROK 4: ClientSubscriptions (Zadanie 4A.1.4)**
Wymagania:

- Tabela subskrypcji należących do tego klienta
- Kolumny: Plan, Status, Next Billing, Amount
- Action: View Subscription Details

**KROK 5: Activity Log (Zadanie 4A.1.5)**
Timeline component:

- "Created", "Updated", "Subscription Added", etc.
- Use shadcn/ui Timeline or custom

### OCZEKIWANY REZULTAT:

Szczegółowa strona klienta z nawigacją tabs.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [ ] `/clients/[id]` wyświetla client data
- [ ] Tabs działają
- [ ] ClientHeader pokazuje info
- [ ] Overview z statystykami
- [ ] ClientSubscriptions lista
- [ ] Activity log (może być placeholder)
- [ ] Edit client z tej strony działa
- [ ] Git commit: `feat(clients): implement client detail page with tabs`

<!-- BLOCK_END: 4A.1 -->

---

<!-- BLOCK_START: 4A.2 -->

### 4A.2 Bulk Operations & Advanced Filters

| #      | Zadanie              | Priorytet  | Status | Opis                                 |
| ------ | -------------------- | ---------- | ------ | ------------------------------------ |
| 4A.2.1 | 🟡 Multi-select rows | Ważne      | ⬜     | Checkboxes w tabeli                  |
| 4A.2.2 | 🟡 Bulk actions bar  | Ważne      | ⬜     | Delete, Export, Tag (gdy zaznaczone) |
| 4A.2.3 | 🟡 Advanced filters  | Ważne      | ⬜     | Status, Date range, Tags             |
| 4A.2.4 | 🟢 Export to CSV     | Opcjonalne | ⬜     | Eksport zaznaczonych lub wszystkich  |
| 4A.2.5 | 🟢 Import from CSV   | Opcjonalne | ⬜     | Bulk import klientów                 |

**Blok 4A.2 - Wymagania wejściowe**: Blok 4A.1 (Client Detail)  
**Blok 4A.2 - Rezultat**: Zaawansowane operacje na klientach

**📦 DEPENDENCIES:**

- ✅ Blok 3.1 (Clients Table)
- ✅ Blok 4A.1 (Client Detail)

**⬅️ BLOKUJE:**

- Opcjonalne usprawnienia UX

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer.
Przechodzimy do Fazy 4A.2: **Bulk Operations & Advanced Filters**.

### CEL GŁÓWNY:

Dodać zaawansowane funkcje do modułu klientów: multi-select, bulk delete, filtry, eksport/import.

### KROKI DO WYKONANIA:

**KROK 1: Multi-select (Zadanie 4A.2.1)**
W ClientsTable:

- Dodaj kolumnę checkbox (select all + individual)
- State: `selectedIds: string[]`

**KROK 2: Bulk Actions Bar (Zadanie 4A.2.2)**
Sticky bar na dole/górze tabeli gdy `selectedIds.length > 0`:

- Delete Selected
- Export Selected
- Close button (clear selection)

**KROK 3: Advanced Filters (Zadanie 4A.2.3)**
Utwórz `ClientsFilters.tsx`:

- Dropdown: Status (All, Active, Inactive)
- Date range picker
- Search bar enhancement

**KROK 4: Export CSV (Zadanie 4A.2.4)**
Funkcja konwertująca selected clients do CSV i trigger download.

**KROK 5: Import CSV (Zadanie 4A.2.5)**
Dialog z file upload:

- Parse CSV
- Validate rows
- Bulk create mutation

### OCZEKIWANY REZULTAT:

Zaawansowane operacje bulk w module clients.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [ ] Multi-select checkboxes działają
- [ ] Bulk delete działa
- [ ] Advanced filters filtrują
- [ ] Export CSV działa
- [ ] Import CSV parsuje i tworzy
- [ ] Toast confirmations
- [ ] Git commit: `feat(clients): add bulk operations and advanced filters`

<!-- BLOCK_END: 4A.2 -->

---

<!-- BLOCK_START: 4A.3 -->

### 4A.3 Client Notes & Tags

| #      | Zadanie           | Priorytet  | Status | Opis                             |
| ------ | ----------------- | ---------- | ------ | -------------------------------- |
| 4A.3.1 | 🟡 Notes system   | Ważne      | ⬜     | Add/Edit/Delete notes per client |
| 4A.3.2 | 🟡 Tags system    | Ważne      | ⬜     | Assign multiple tags to clients  |
| 4A.3.3 | 🟡 Filter by tags | Ważne      | ⬜     | Filter clients by assigned tags  |
| 4A.3.4 | 🟢 Notes timeline | Opcjonalne | ⬜     | Chronological notes view         |

**Blok 4A.3 - Wymagania wejściowe**: Blok 4A.1 (Client Detail)  
**Blok 4A.3 - Rezultat**: System notatek i tagów dla klientów

**📦 DEPENDENCIES:**

- ✅ Blok 4A.1 (Client Detail Page)

**⬅️ BLOKUJE:**

- Brak (opcjonalne feature)

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer.
Przechodzimy do Fazy 4A.3: **Client Notes & Tags**.

### CEL GŁÓWNY:

Dodać system notatek i tagów do klientów dla lepszej organizacji.

### KROKI DO WYKONANIA:

**KROK 1: Notes Component (Zadanie 4A.3.1)**
Utwórz `src/features/clients/components/ClientNotes.tsx`.
Wymagania:

- Textarea do dodawania nowej notatki
- Lista notatek z timestamp i author
- Edit/Delete dla każdej notatki

**KROK 2: Tags Component (Zadanie 4A.3.2)**
Utwórz `ClientTags.tsx`:

- Multi-select tag input (shadcn/ui Badge + Command)
- Create new tag on the fly
- Display assigned tags with remove option

**KROK 3: Tag Filters (Zadanie 4A.3.3)**
W ClientsFilters dodaj:

- Tag filter dropdown (multi-select)

**KROK 4: Notes Timeline (Zadanie 4A.3.4)**
Opcjonalny widok chronologiczny notatek w Activity tab.

### OCZEKIWANY REZULTAT:

System notatek i tagów zintegrowany z client detail.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [ ] Można dodać notatkę do klienta
- [ ] Można edytować/usuwać notatki
- [ ] Tagi można przypisywać
- [ ] Filter by tags działa
- [ ] UI przyjazny dla użytkownika
- [ ] Git commit: `feat(clients): add notes and tags system`

<!-- BLOCK_END: 4A.3 -->

---

<!-- BLOCK_START: 4B.1 -->

## 🔵 FAZA 4B: Plans Detail (Tydzień 5) - PARALLEL z 4A

### 4B.1 Plan Detail Page

| #      | Zadanie                 | Priorytet | Status | Opis                                      |
| ------ | ----------------------- | --------- | ------ | ----------------------------------------- |
| 4B.1.1 | 🔴 Plan detail layout   | Krytyczne | ⬜     | /plans/[id] - tabs: Overview, Subscribers |
| 4B.1.2 | 🔴 PlanHeader component | Krytyczne | ⬜     | Name, price, interval, status             |
| 4B.1.3 | 🔴 PlanOverview tab     | Krytyczne | ⬜     | Stats, features list, pricing tiers       |
| 4B.1.4 | 🔴 PlanSubscribers tab  | Krytyczne | ⬜     | Lista aktywnych subskrypcji tego planu    |
| 4B.1.5 | 🟡 Plan analytics       | Ważne     | ⬜     | Revenue chart, growth metrics             |

**Blok 4B.1 - Wymagania wejściowe**: Blok 3.2 (Plans List)  
**Blok 4B.1 - Rezultat**: Szczegółowy widok planu subskrypcyjnego

**📦 DEPENDENCIES:**

- ✅ Blok 3.2 (Plans Management)

**⬅️ BLOKUJE:**

- Blok 5.1 (Subscriptions)

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer.
Przechodzimy do Fazy 4B.1: **Plan Detail Page**.

### CEL GŁÓWNY:

Stworzyć szczegółowy widok planu subskrypcyjnego z zakładkami i statystykami.

### ARCHITEKTURA:

```
src/features/plans/
└── components/
    └── detail/
        ├── PlanHeader.tsx
        ├── PlanOverview.tsx
        ├── PlanSubscribers.tsx
        └── PlanAnalytics.tsx
```

### KROKI DO WYKONANIA:

**KROK 1: Dynamic Route (Zadanie 4B.1.1)**
`src/app/(dashboard)/dashboard/plans/[id]/page.tsx`
Tabs: Overview, Subscribers, Analytics

**KROK 2: PlanHeader (Zadanie 4B.1.2)**

- Plan name + description
- Price badge
- Interval badge
- Active/Inactive status
- Edit button

**KROK 3: PlanOverview (Zadanie 4B.1.3)**
Stats cards:

- Active Subscriptions
- Monthly Recurring Revenue (MRR)
- Average Customer Value
  Features list display

**KROK 4: PlanSubscribers (Zadanie 4B.1.4)**
Tabela subskrypcji:

- Client Name
- Status
- Start Date
- Next Billing

**KROK 5: Analytics (Zadanie 4B.1.5)**
Wykresy (Recharts):

- Subscriber growth over time
- Revenue trend

### OCZEKIWANY REZULTAT:

Kompleksowy widok planu z metrykami.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [ ] `/plans/[id]` działa
- [ ] PlanHeader wyświetla dane
- [ ] Overview z stats
- [ ] Subscribers lista
- [ ] Analytics (może być placeholder)
- [ ] Edit plan działa
- [ ] Git commit: `feat(plans): implement plan detail page with analytics`

<!-- BLOCK_END: 4B.1 -->

---

<!-- BLOCK_START: 4B.2 -->

### 4B.2 Plan Templates & Duplication

| #      | Zadanie            | Priorytet  | Status | Opis                             |
| ------ | ------------------ | ---------- | ------ | -------------------------------- |
| 4B.2.1 | 🟡 Duplicate plan  | Ważne      | ⬜     | Clone plan z wszystkimi settings |
| 4B.2.2 | 🟡 Plan templates  | Ważne      | ⬜     | Pre-defined starter templates    |
| 4B.2.3 | 🟡 Archive plan    | Ważne      | ⬜     | Soft delete (archived status)    |
| 4B.2.4 | 🟢 Plan versioning | Opcjonalne | ⬜     | Version history, rollback        |

**Blok 4B.2 - Wymagania wejściowe**: Blok 4B.1 (Plan Detail)  
**Blok 4B.2 - Rezultat**: Zaawansowane zarządzanie planami

**📦 DEPENDENCIES:**

- ✅ Blok 3.2 (Plans CRUD)
- ✅ Blok 4B.1 (Plan Detail)

**⬅️ BLOKUJE:**

- Brak (opcjonalne features)

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer.
Przechodzimy do Fazy 4B.2: **Plan Templates & Duplication**.

### CEL GŁÓWNY:

Ułatwić zarządzanie planami przez templates i klonowanie.

### KROKI DO WYKONANIA:

**KROK 1: Duplicate Feature (Zadanie 4B.2.1)**
W PlanCard/PlanDetail:

- Action "Duplicate"
- Kopiuje wszystkie dane planu
- Otwiera PlanForm z skopiowanymi danymi
- Prefix name z "Copy of"

**KROK 2: Templates (Zadanie 4B.2.2)**
Utwórz `src/features/plans/templates/planTemplates.ts`:

- Array gotowych templatek (Basic, Pro, Enterprise)
- Button "Use Template" w PlansPage
- Wypełnia form templatem

**KROK 3: Archive (Zadanie 4B.2.3)**
Zamiast delete → archive:

- Soft delete (isArchived flag)
- Archived plans nie pokazują się na głównej liście
- Osobna zakładka "Archived Plans"

**KROK 4: Versioning (Zadanie 4B.2.4)**
Opcjonalnie: historia zmian planu

- Zapisywanie snapshota przy każdej edycji
- View/Restore previous versions

### OCZEKIWANY REZULTAT:

Zaawansowane features zarządzania planami.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [ ] Duplicate plan działa
- [ ] Templates dostępne
- [ ] Archive zamiast delete
- [ ] Archived plans w osobnej zakładce
- [ ] Versioning (jeśli zaimplementowane)
- [ ] Git commit: `feat(plans): add duplication, templates, and archiving`

<!-- BLOCK_END: 4B.2 -->

---

<!-- BLOCK_START: 5.1 -->

## 🔵 FAZA 5: Subscriptions Core (Tydzień 6-7)

### 5.1 Subscription Lifecycle

| #     | Zadanie                         | Priorytet | Status | Opis                                                    |
| ----- | ------------------------------- | --------- | ------ | ------------------------------------------------------- |
| 5.1.1 | 🔴 Subscription types & schemas | Krytyczne | ⬜     | Zod schemas dla Subscription CRUD                       |
| 5.1.2 | 🔴 useSubscriptions hooks       | Krytyczne | ⬜     | useQuery + mutations                                    |
| 5.1.3 | 🔴 SubscriptionsTable           | Krytyczne | ⬜     | Tabela z filtrowaniem po status, plan, client           |
| 5.1.4 | 🔴 SubscriptionForm             | Krytyczne | ⬜     | Wizard: wybór klienta, planu, daty rozpoczęcia          |
| 5.1.5 | 🔴 Subscription status badges   | Krytyczne | ⬜     | Active, Paused, Cancelled, PendingCancellation, Expired |
| 5.1.6 | 🔴 Subscriptions list page      | Krytyczne | ⬜     | /dashboard/subscriptions                                |

**Blok 5.1 - Wymagania wejściowe**: Faza 4 (Clients & Plans gotowe)  
**Blok 5.1 - Rezultat**: Podstawowy CRUD subskrypcji

**📦 DEPENDENCIES:**

- ✅ Blok 3.1 (Clients)
- ✅ Blok 3.2 (Plans)
- ✅ Blok 4A.1 (Client Detail)
- ✅ Blok 4B.1 (Plan Detail)

**⬅️ BLOKUJE:**

- Blok 5.2 (Subscription Detail)
- Blok 6.1 (Payments)

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer / Business Logic Specialist.
Przechodzimy do Fazy 5.1: **Subscription Lifecycle**.

### CEL GŁÓWNY:

Zaimplementować kompletny system zarządzania subskrypcjami - serce aplikacji.

### ARCHITEKTURA:

```
src/features/subscriptions/
├── components/
│   ├── SubscriptionsTable.tsx
│   ├── SubscriptionForm.tsx
│   ├── SubscriptionStatusBadge.tsx
│   └── SubscriptionWizard.tsx
├── hooks/
│   └── useSubscriptions.ts
├── schemas/
│   └── subscription.schemas.ts
└── types/
    └── subscription.types.ts
```

### KROKI DO WYKONANIA:

**KROK 1: Schemas (Zadanie 5.1.1)**

```typescript
export const SubscriptionSchema = z.object({
  clientId: z.string().uuid(),
  planId: z.string().uuid(),
  startDate: z.date(),
  nextBillingDate: z.date().optional(),
  status: z.enum([
    "Active",
    "Paused",
    "Cancelled",
    "PendingCancellation",
    "Expired",
  ]),
  autoRenew: z.boolean().default(true),
});
```

**KROK 2: API Hooks (Zadanie 5.1.2)**
`useSubscriptions.ts`:

- List, Create, Update, Cancel, Pause, Resume

**KROK 3: Status Badges (Zadanie 5.1.5)**
Component wyświetlający kolorowy badge:

- Active → green
- Paused → yellow
- Cancelled → red
- PendingCancellation → orange
- Expired → gray

**KROK 4: SubscriptionForm/Wizard (Zadanie 5.1.4)**
Multi-step:

1. Wybór klienta (searchable select)
2. Wybór planu (cards)
3. Konfiguracja (start date, auto-renew)
4. Review & Create

**KROK 5: SubscriptionsTable (Zadanie 5.1.3)**
Kolumny:

- Client Name
- Plan Name
- Status
- Next Billing
- Amount
- Actions (View, Cancel, Pause)

**KROK 6: Subscriptions Page (Zadanie 5.1.6)**
`/dashboard/subscriptions`

- Stats: Total, Active, MRR
- Filters: Status, Plan, Client
- Table

### OCZEKIWANY REZULTAT:

Działający CRUD subskrypcji z lifecycle management.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [ ] Subscription schemas
- [ ] useSubscriptions hooks
- [ ] Status badges z kolorami
- [ ] SubscriptionWizard tworzy subskrypcje
- [ ] SubscriptionsTable wyświetla dane
- [ ] Filters działają
- [ ] Cancel/Pause/Resume działają
- [ ] Toast notifications
- [ ] Git commit: `feat(subscriptions): implement subscription lifecycle management`

<!-- BLOCK_END: 5.1 -->

---

<!-- BLOCK_START: 5.2 -->

### 5.2 Subscription Detail & Actions

| #     | Zadanie                       | Priorytet | Status | Opis                             |
| ----- | ----------------------------- | --------- | ------ | -------------------------------- |
| 5.2.1 | 🔴 Subscription detail page   | Krytyczne | ⬜     | /subscriptions/[id]              |
| 5.2.2 | 🔴 SubscriptionHeader         | Krytyczne | ⬜     | Client, Plan, Status, Actions    |
| 5.2.3 | 🔴 Billing history tab        | Krytyczne | ⬜     | Lista wszystkich płatności       |
| 5.2.4 | 🔴 Cancel subscription dialog | Krytyczne | ⬜     | Reason, immediate/end of period  |
| 5.2.5 | 🟡 Change plan flow           | Ważne     | ⬜     | Upgrade/Downgrade z proration    |
| 5.2.6 | 🟡 Pause/Resume subscription  | Ważne     | ⬜     | Temporary pause with resume date |

**Blok 5.2 - Wymagania wejściowe**: Blok 5.1 (Subscriptions List)  
**Blok 5.2 - Rezultat**: Szczegółowy widok subskrypcji z akcjami

**📦 DEPENDENCIES:**

- ✅ Blok 5.1 (Subscription Lifecycle)

**⬅️ BLOKUJE:**

- Blok 6.1 (Payments - korzysta z billing history)

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer.
Przechodzimy do Fazy 5.2: **Subscription Detail & Actions**.

### CEL GŁÓWNY:

Stworzyć szczegółowy widok subskrypcji z możliwością zarządzania lifecycle.

### ARCHITEKTURA:

```
src/features/subscriptions/
└── components/
    └── detail/
        ├── SubscriptionHeader.tsx
        ├── BillingHistory.tsx
        ├── CancelDialog.tsx
        ├── ChangePlanDialog.tsx
        └── PauseDialog.tsx
```

### KROKI DO WYKONANIA:

**KROK 1: Detail Page (Zadanie 5.2.1)**
`/dashboard/subscriptions/[id]/page.tsx`
Tabs:

- Overview
- Billing History
- Activity Log

**KROK 2: SubscriptionHeader (Zadanie 5.2.2)**

- Client info (link do client detail)
- Plan info (link do plan detail)
- Status badge
- Actions dropdown: Cancel, Pause, Change Plan

**KROK 3: Billing History (Zadanie 5.2.3)**
Tabela płatności:

- Date, Amount, Status, Invoice Link
- Download Invoice button

**KROK 4: Cancel Dialog (Zadanie 5.2.4)**
Dialog z:

- Reason select/textarea
- Radio: Cancel immediately / End of billing period
- Confirmation checkbox
- Submit → wywołanie cancelSubscription mutation

**KROK 5: Change Plan (Zadanie 5.2.5)**
Dialog:

- Lista dostępnych planów
- Pokazanie różnicy w cenie
- Proration calculation (backend)
- Effective date

**KROK 6: Pause/Resume (Zadanie 5.2.6)**
Dialog:

- Pause until date picker
- Resume button (jeśli paused)

### OCZEKIWANY REZULTAT:

Pełny lifecycle management dla pojedynczej subskrypcji.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [ ] `/subscriptions/[id]` działa
- [ ] Header z info i actions
- [ ] Billing history wyświetla płatności
- [ ] Cancel dialog działa
- [ ] Change plan działa
- [ ] Pause/Resume działa
- [ ] Wszystkie mutacje z toast
- [ ] Git commit: `feat(subscriptions): implement detail page and lifecycle actions`

<!-- BLOCK_END: 5.2 -->

---

<!-- BLOCK_START: 6.1 -->

## 🔵 FAZA 6: Payments Integration (Tydzień 7-8)

### 6.1 Stripe Integration

| #     | Zadanie                       | Priorytet | Status | Opis                                |
| ----- | ----------------------------- | --------- | ------ | ----------------------------------- |
| 6.1.1 | 🔴 Stripe client setup        | Krytyczne | ⬜     | loadStripe, Elements provider       |
| 6.1.2 | 🔴 Payment method form        | Krytyczne | ⬜     | CardElement z Stripe Elements       |
| 6.1.3 | 🔴 Setup Intent flow          | Krytyczne | ⬜     | Dodawanie karty bez płatności       |
| 6.1.4 | 🔴 Payment Intent flow        | Krytyczne | ⬜     | One-time payment                    |
| 6.1.5 | 🔴 Saved payment methods list | Krytyczne | ⬜     | Lista kart klienta, default, delete |
| 6.1.6 | 🟡 3D Secure handling         | Ważne     | ⬜     | Obsługa requires_action             |

**Blok 6.1 - Wymagania wejściowe**: Faza 5 (Subscriptions)  
**Blok 6.1 - Rezultat**: Integracja z Stripe dla płatności

**📦 DEPENDENCIES:**

- ✅ Blok 5.1 (Subscriptions)
- ✅ Backend Stripe integration

**⬅️ BLOKUJE:**

- Blok 6.2 (Payment History)
- Blok 7.1 (Invoices)

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer / Payments Specialist.
Przechodzimy do Fazy 6.1: **Stripe Integration**.

### CEL GŁÓWNY:

Zintegrować Stripe Elements do przyjmowania płatności i zarządzania metodami płatności.

### KONTEKST:

Backend dostarcza endpointy:

- POST /payments/setup-intent → clientSecret
- POST /payments/payment-intent → clientSecret
- GET /payments/methods → lista saved cards
- DELETE /payments/methods/{id}

### KROKI DO WYKONANIA:

**KROK 1: Stripe Setup (Zadanie 6.1.1)**

```bash
npm install @stripe/stripe-js @stripe/react-stripe-js
```

Utwórz `src/core/providers/StripeProvider.tsx`:

```typescript
import { loadStripe } from "@stripe/stripe-js";
import { Elements } from "@stripe/react-stripe-js";

const stripePromise = loadStripe(
  process.env.NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY!
);

export function StripeProvider({ children }: { children: React.ReactNode }) {
  return <Elements stripe={stripePromise}>{children}</Elements>;
}
```

Dodaj do layout.

**KROK 2: Payment Method Form (Zadanie 6.1.2)**
Utwórz `src/features/payments/components/PaymentMethodForm.tsx`.
Wymagania:

- Use `CardElement` from Stripe
- Submit → wywołanie backend `/setup-intent`
- Confirmation ze Stripe

**KROK 3: Setup Intent Flow (Zadanie 6.1.3)**
Hook `useSetupIntent()`:

- Pobiera clientSecret z backendu
- Używa `stripe.confirmCardSetup()`
- Obsługuje sukces/błąd

**KROK 4: Payment Intent (Zadanie 6.1.4)**
Analogicznie dla one-time payments.

**KROK 5: Saved Methods List (Zadanie 6.1.5)**
Component `PaymentMethodsList.tsx`:

- Wyświetla listę kart (last4, brand, exp)
- Default badge
- Delete button

**KROK 6: 3DS Handling (Zadanie 6.1.6)**
Obsługa `requires_action`:

- `stripe.confirmCardPayment()` z redirect

### OCZEKIWANY REZULTAT:

Działająca integracja Stripe do płatności.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [ ] Stripe Elements załadowane
- [ ] PaymentMethodForm działa
- [ ] Można dodać kartę
- [ ] Lista saved methods
- [ ] Delete payment method działa
- [ ] 3DS redirect działa
- [ ] Error handling
- [ ] Git commit: `feat(payments): integrate Stripe payment processing`

<!-- BLOCK_END: 6.1 -->

---

<!-- BLOCK_START: 6.2 -->

### 6.2 Payment History & Invoices

| #     | Zadanie                  | Priorytet | Status | Opis                                 |
| ----- | ------------------------ | --------- | ------ | ------------------------------------ |
| 6.2.1 | 🔴 Payments table        | Krytyczne | ⬜     | Historia wszystkich płatności        |
| 6.2.2 | 🔴 Payment detail modal  | Krytyczne | ⬜     | Szczegóły transakcji                 |
| 6.2.3 | 🔴 Payment status badges | Krytyczne | ⬜     | Succeeded, Failed, Pending, Refunded |
| 6.2.4 | 🟡 Refund dialog         | Ważne     | ⬜     | Partial/Full refund                  |
| 6.2.5 | 🟡 Failed payments retry | Ważne     | ⬜     | Retry failed payment                 |

**Blok 6.2 - Wymagania wejściowe**: Blok 6.1 (Stripe Integration)  
**Blok 6.2 - Rezultat**: Historia i zarządzanie płatnościami

**📦 DEPENDENCIES:**

- ✅ Blok 6.1 (Stripe Integration)

**⬅️ BLOKUJE:**

- Blok 7.1 (Invoices)

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer.
Przechodzimy do Fazy 6.2: **Payment History & Invoices**.

### CEL GŁÓWNY:

Stworzyć kompletny moduł historii płatności z możliwością zwrotów i retry.

### KROKI DO WYKONANIA:

**KROK 1: Payments Table (Zadanie 6.2.1)**
`src/features/payments/components/PaymentsTable.tsx`
Kolumny:

- Date, Client, Subscription, Amount, Status, Actions

**KROK 2: Payment Detail (Zadanie 6.2.2)**
Modal/Drawer z szczegółami:

- Transaction ID (Stripe)
- Payment method details
- Timeline (Attempted, Succeeded/Failed)
- Refund history

**KROK 3: Status Badges (Zadanie 6.2.3)**
Component z kolorami dla każdego statusu.

**KROK 4: Refund Dialog (Zadanie 6.2.4)**
Dialog:

- Reason
- Amount (full/partial)
- Confirmation
- Wywołanie `/payments/refund`

**KROK 5: Retry Failed (Zadanie 6.2.5)**
Button "Retry Payment":

- Tworzy nowy payment intent
- Próbuje z saved payment method

### OCZEKIWANY REZULTAT:

Pełne zarządzanie historią płatności.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [ ] PaymentsTable wyświetla wszystkie płatności
- [ ] Payment detail modal działa
- [ ] Status badges
- [ ] Refund działa
- [ ] Retry failed payments
- [ ] Filters (date range, status)
- [ ] Git commit: `feat(payments): implement payment history and refunds`

<!-- BLOCK_END: 6.2 -->

---

<!-- BLOCK_START: 7.1 -->

## 🔵 FAZA 7: Invoicing & Reporting (Tydzień 8)

### 7.1 Invoice Generation

| #     | Zadanie                  | Priorytet | Status | Opis                                 |
| ----- | ------------------------ | --------- | ------ | ------------------------------------ |
| 7.1.1 | 🔴 Invoice types         | Krytyczne | ⬜     | Generated types z backendu           |
| 7.1.2 | 🔴 Invoices table        | Krytyczne | ⬜     | Lista faktur z filtrowaniem          |
| 7.1.3 | 🔴 Invoice preview       | Krytyczne | ⬜     | Modal z podglądem faktury (PDF/HTML) |
| 7.1.4 | 🔴 Download PDF          | Krytyczne | ⬜     | Generowanie i download z backendu    |
| 7.1.5 | 🟡 Send invoice by email | Ważne     | ⬜     | Wysyłka na email klienta             |
| 7.1.6 | 🟡 Invoice templates     | Ważne     | ⬜     | Customizacja wyglądu faktury         |

**Blok 7.1 - Wymagania wejściowe**: Blok 6.2 (Payments)  
**Blok 7.1 - Rezultat**: System generowania i wysyłki faktur

**📦 DEPENDENCIES:**

- ✅ Blok 6.2 (Payment History)
- ✅ Backend Invoice generation

**⬅️ BLOKUJE:**

- Blok 8.1 (Analytics)

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer.
Przechodzimy do Fazy 7.1: **Invoice Generation**.

### CEL GŁÓWNY:

Zaimplementować system faktur - podgląd, download, wysyłka email.

### KROKI DO WYKONANIA:

**KROK 1: Invoices Table (Zadanie 7.1.2)**
`src/features/invoices/components/InvoicesTable.tsx`
Kolumny:

- Invoice Number, Client, Date, Amount, Status, Actions

**KROK 2: Invoice Preview (Zadanie 7.1.3)**
Dialog/Modal:

- Embed PDF viewer lub render HTML preview
- Backend endpoint: GET `/invoices/{id}/preview`

**KROK 3: Download PDF (Zadanie 7.1.4)**
Button "Download":

- Wywołanie GET `/invoices/{id}/pdf`
- Trigger file download

**KROK 4: Send Email (Zadanie 7.1.5)**
Dialog:

- Recipient email (pre-filled from client)
- Subject, Message
- POST `/invoices/{id}/send`

**KROK 5: Templates (Zadanie 7.1.6)**
Opcjonalnie: wybór template dla faktury.

### OCZEKIWANY REZULTAT:

Kompletny system fakturowania.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [ ] InvoicesTable wyświetla faktury
- [ ] Preview działa (PDF lub HTML)
- [ ] Download PDF działa
- [ ] Send email działa
- [ ] Filters (date, status, client)
- [ ] Git commit: `feat(invoices): implement invoice generation and delivery`

<!-- BLOCK_END: 7.1 -->

---

<!-- BLOCK_START: 8.1 -->

## 🔵 FAZA 8: Testing & Polish (Tydzień 9)

### 8.1 Testing Infrastructure

| #     | Zadanie                   | Priorytet | Status | Opis                                     |
| ----- | ------------------------- | --------- | ------ | ---------------------------------------- |
| 8.1.1 | 🔴 Vitest setup           | Krytyczne | ⬜     | Konfiguracja test runnera                |
| 8.1.2 | 🔴 React Testing Library  | Krytyczne | ⬜     | Setup dla komponentów                    |
| 8.1.3 | 🔴 Unit tests - hooks     | Krytyczne | ⬜     | Testy dla custom hooks                   |
| 8.1.4 | 🔴 Unit tests - utils     | Krytyczne | ⬜     | Testy dla formatters, helpers            |
| 8.1.5 | 🟡 Component tests        | Ważne     | ⬜     | Testy dla kluczowych komponentów         |
| 8.1.6 | 🟡 E2E setup (Playwright) | Ważne     | ⬜     | Happy paths (login, create subscription) |

**Blok 8.1 - Wymagania wejściowe**: Wszystkie features zakończone  
**Blok 8.1 - Rezultat**: Kompleksowe testy

**📦 DEPENDENCIES:**

- ✅ Wszystkie poprzednie bloki (features gotowe)

**⬅️ BLOKUJE:**

- Blok 8.2 (Polish)

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer / QA Engineer.
Przechodzimy do Fazy 8.1: **Testing Infrastructure**.

### CEL GŁÓWNY:

Skonfigurować narzędzia testowe i napisać testy dla krytycznych części aplikacji.

### KROKI DO WYKONANIA:

**KROK 1: Vitest Setup (Zadanie 8.1.1)**

```bash
npm install -D vitest @vitejs/plugin-react jsdom
```

Konfiguracja `vitest.config.ts`:

```typescript
import { defineConfig } from "vitest/config";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [react()],
  test: {
    environment: "jsdom",
    globals: true,
  },
});
```

**KROK 2: React Testing Library (Zadanie 8.1.2)**

```bash
npm install -D @testing-library/react @testing-library/jest-dom @testing-library/user-event
```

**KROK 3: Hook Tests (Zadanie 8.1.3)**
Testy dla:

- `useAuth()`
- `useTenant()`
- `useClients()`

**KROK 4: Utils Tests (Zadanie 8.1.4)**
Testy dla:

- `formatCurrency()`
- `formatDate()`
- `cn()`

**KROK 5: Component Tests (Zadanie 8.1.5)**
Testy dla:

- LoginForm
- ClientsTable
- SubscriptionForm

**KROK 6: E2E Setup (Zadanie 8.1.6)**

```bash
npm install -D @playwright/test
```

Napisz testy dla:

- Login flow
- Create subscription flow

### OCZEKIWANY REZULTAT:

Działające testy dla krytycznych części.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [ ] Vitest działa
- [ ] Hook tests przechodzą
- [ ] Utils tests przechodzą
- [ ] Component tests przechodzą
- [ ] E2E tests przechodzą
- [ ] CI/CD integration (opcjonalne)
- [ ] Git commit: `test: setup testing infrastructure and core tests`

<!-- BLOCK_END: 8.1 -->

---

<!-- BLOCK_START: 8.2 -->

### 8.2 Polish & Optimization

| #     | Zadanie                | Priorytet  | Status | Opis                                    |
| ----- | ---------------------- | ---------- | ------ | --------------------------------------- |
| 8.2.1 | 🔴 TypeScript audit    | Krytyczne  | ⬜     | Weryfikacja brak any, pełne typy        |
| 8.2.2 | 🔴 Accessibility audit | Krytyczne  | ⬜     | Keyboard nav, aria labels, focus states |
| 8.2.3 | 🟡 Performance audit   | Ważne      | ⬜     | Lighthouse, bundle analysis             |
| 8.2.4 | 🟡 Mobile responsive   | Ważne      | ⬜     | Testowanie na różnych rozdzielczościach |
| 8.2.5 | 🟢 Documentation       | Opcjonalne | ⬜     | README, component docs                  |

**Blok 8.2 - Wymagania wejściowe**: Blok 8.1 (Testy)  
**Blok 8.2 - Rezultat**: Aplikacja gotowa do wdrożenia (Production Ready)

**📦 DEPENDENCIES:**

- ✅ Blok 8.1 (Testing)

**⬅️ BLOKUJE:**

- Production deployment

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer / Tech Lead.
Aplikacja jest funkcjonalna i przetestowana. Przechodzimy do Fazy 8.2: **Polish & Optimization**.
Naszym celem jest upewnienie się, że kod jest najwyższej jakości, aplikacja jest dostępna dla wszystkich użytkowników (A11y) i zoptymalizowana pod kątem wydajności.

### CEL GŁÓWNY:

Przeprowadzić audyt kodu, skonfigurować narzędzia do analizy wydajności oraz przygotować dokumentację projektu.

### KROKI DO WYKONANIA:

**KROK 1: TypeScript & Linting Strictness (Zadanie 8.2.1)**

1. Sprawdź plik `tsconfig.json`. Upewnij się, że `noImplicitAny` jest na `true`.
2. Dodaj skrypt do `package.json`: `"type-check": "tsc --noEmit"`.
3. Skonfiguruj ESLint, aby wyłapywał typy `any`. W pliku `eslint.config.mjs` (lub .rc) dodaj regułę:
   - `@typescript-eslint/no-explicit-any`: "warn" (lub "error" dla strict mode).
   - `@typescript-eslint/no-unused-vars`: "error".

**KROK 2: Accessibility (A11y) Setup (Zadanie 8.2.2)**

1. Zainstaluj plugin `eslint-plugin-jsx-a11y`.
2. Dodaj go do konfiguracji ESLint, aby automatycznie wykrywał brakujące `alt` w obrazkach, brakujące `aria-label` w przyciskach (szczególnie tych z samą ikoną, np. w `Sidebar` czy `ActionsDropdown`).
3. Stwórz prosty dokument `A11Y_CHECKLIST.md` z punktami do ręcznego sprawdzenia:
   - Czy wszystkie formularze mają etykiety (`label`) powiązane z inputami?
   - Czy można poruszać się po stronie używając tylko klawisza TAB?
   - Czy focus jest widoczny na elementach aktywnych?

**KROK 3: Performance Optimization (Zadanie 8.2.3)**

1. Zainstaluj `@next/bundle-analyzer`.
2. Skonfiguruj `next.config.ts`, aby włączał analyzer zmienną środowiskową (np. `ANALYZE=true`).
3. Dodaj skrypt do `package.json`: `"analyze": "cross-env ANALYZE=true npm run build"`.
4. Stwórz komponent `Image` wrapper (opcjonalnie), który wymusza używanie `next/image` zamiast `<img>` w celu optymalizacji obrazów.

**KROK 4: Documentation (Zadanie 8.2.5)**
Napisz profesjonalny plik `README.md` dla projektu.
Struktura:

- **Title & Badges** (Status, Tech Stack).
- **Prerequisites** (Node.js version, npm/pnpm).
- **Getting Started** (instrukcja krok po kroku: install, env setup, run dev).
- **Project Structure** (wyjaśnienie architektury Features/Core/Shared).
- **Scripts** (wyjaśnienie komend: dev, build, start, lint, test, api:generate).
- **Environment Variables** (lista wymaganych zmiennych w `.env` - bez wartości, tylko klucze i opis).

### WYMAGANIA TECHNICZNE:

- **Next.js Config**: Upewnij się, że konfiguracja bundle analyzera nie psuje standardowego buildu (powinna działać tylko warunkowo).
- **ESLint**: Konfiguracja powinna być kompatybilna z "Flat Config" (nowy standard ESLint w 2024/2025).

### OCZEKIWANY REZULTAT:

Zaktualizowane pliki konfiguracyjne (`package.json`, `next.config.ts`, `eslint.config.mjs`), checklista dostępności oraz gotowy plik `README.md`.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [ ] TypeScript strict mode bez błędów
- [ ] ESLint bez warnings
- [ ] A11y checklist przeprowadzona
- [ ] Bundle analyzer skonfigurowany
- [ ] Lighthouse score > 90
- [ ] Responsive na mobile/tablet/desktop
- [ ] README.md kompletny
- [ ] Git commit: `chore: polish codebase and add documentation`

<!-- BLOCK_END: 8.2 -->

---

<!-- BLOCK_START: 9.1 -->

## 🔵 FAZA 9: Client Portal (Tydzień 10 - Opcjonalne)

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

**📦 DEPENDENCIES:**

- ✅ Blok 1.1, 1.2 (Auth)
- ✅ Blok 5.1 (Subscriptions)

**⬅️ BLOKUJE:**

- Brak (ostatnia faza, opcjonalna)

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer / System Architect.
Realizujemy Fazę 9: **Client Portal**.
Jest to osobna część aplikacji przeznaczona dla klientów końcowych (nie dla Providerów). Musi być prosta, przejrzysta i skupiona na samoobsłudze płatności.

### CEL GŁÓWNY:

Stworzyć wydzieloną strefę `/portal` z własnym layoutem, dostępną tylko dla użytkowników o roli `Client`. Użytkownik ma tam widzieć swoje aktywne subskrypcje, historię faktur i móc zarządzać metodami płatności.

### ARCHITEKTURA I LOKALIZACJA PLIKÓW:

Używamy **Route Groups** do separacji layoutów, aby panel klienta nie dziedziczył Dashboard Layoutu providera:

- Routing: `src/app/(portal)/layout.tsx`, `src/app/(portal)/portal/page.tsx`
- Feature: `src/features/client-portal/` (nowy vertical slice)
- Komponenty: `src/features/client-portal/components/`
- Hooki: `src/features/client-portal/hooks/`

### KROKI DO WYKONANIA:

**KROK 1: Portal Layout & Guard (Zadania 9.1.1, 9.1.2)**

1. Utwórz `src/features/client-portal/components/PortalGuard.tsx`.
   - Działa analogicznie do `TenantGuard`, ale wymaga roli `Client`.
   - Jeśli user ma rolę `Provider` -> przekieruj na `/dashboard`.
   - Jeśli user nie jest zalogowany -> `/login`.
2. Utwórz `src/app/(portal)/layout.tsx`.
   - Prosty layout: Navbar na górze (Logo + UserMenu), wycentrowana zawartość (max-w-4xl).
   - Brak bocznego Sidebara.
   - Owiń `children` w `PortalGuard`.

**KROK 2: Portal Hooks (Zadanie 9.1.3)**
Utwórz `src/features/client-portal/hooks/usePortal.ts`.
Wymagania:

- `useMySubscriptions()`: Pobiera subskrypcje zalogowanego klienta (filtrowanie po stronie API dla zalogowanego usera).
- `useMyInvoices()`: Pobiera historię faktur.
- `usePortalAction()`: Wrapper na mutacje (np. `cancelSubscription`, `updatePaymentMethod`).

**KROK 3: Client Dashboard (Zadanie 9.1.3)**
Utwórz `src/app/(portal)/portal/page.tsx`.
Wymagania:

- **Sekcja "Current Plan"**: Wyświetla dużą kartę z aktywną subskrypcją (Plan Name, Price, Renewal Date, Status Badge).
- **Action Buttons**:
  - "Update Payment Method" (otwiera dialog - użyj placeholdera lub komponentu PaymentMethodForm z Fazy 6, ale dostosowanego do kontekstu klienta).
  - "Cancel Subscription" (otwiera dialog potwierdzenia).

**KROK 4: Invoices List (Zadanie 9.1.4)**
Utwórz `src/features/client-portal/components/ClientInvoicesList.tsx`.
Wymagania:

- Prosta tabela lub lista: Data, Kwota, Status, "Download PDF" (ikona).
- Dodaj ten komponent na dole Dashboardu (`page.tsx`).

### WYMAGANIA TECHNICZNE:

- **Next.js 15 Async Params**: Pamiętaj, że w plikach `page.tsx` propsy `params` i `searchParams` są asynchroniczne (Promise). Jeśli będziesz ich potrzebować, użyj `await`.
- **Reużywalność**: Możesz importować komponenty UI (Button, Card, Badge) z `shared/ui`.

### OCZEKIWANY REZULTAT:

Kod layoutu portalu, Guarda, hooków oraz strony głównej portalu z listą faktur.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [ ] Portal layout osobny od dashboard
- [ ] PortalGuard blokuje Provider/TeamMember
- [ ] `/portal` wyświetla dashboard klienta
- [ ] Current Plan card z informacjami
- [ ] Update payment method dialog
- [ ] Cancel subscription dialog
- [ ] Invoices list wyświetla faktury
- [ ] Download PDF działa
- [ ] Responsywność
- [ ] Git commit: `feat(portal): implement client self-service portal`

<!-- BLOCK_END: 9.1 -->

---

## 📝 SZABLON DLA NOWYCH BLOKÓW

Użyj tego szablonu przy dodawaniu nowych bloków:

```markdown
<!-- BLOCK_START: X.X -->

### X.X Nazwa Bloku

| #     | Zadanie      | Priorytet | Status | Opis |
| ----- | ------------ | --------- | ------ | ---- |
| X.X.1 | 🔴 Zadanie 1 | Krytyczne | ⬜     | Opis |

**Blok X.X - Wymagania wejściowe**: [poprzednie bloki]  
**Blok X.X - Rezultat**: [co będzie gotowe]

**📦 DEPENDENCIES:**

- ✅ Blok Y.Y (nazwa)

**⬅️ BLOKUJE:**

- Blok Z.Z (nazwa)

---

### 🤖 PROMPT

[Szczegółowy prompt dla agenta]

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [ ] Punkt 1
- [ ] Punkt 2
- [ ] Git commit: `feat(scope): description`

<!-- BLOCK_END: X.X -->
```

---

## 📊 PODSUMOWANIE BLOKÓW

> **🚀 FRESH START** - Wszystkie bloki do wykonania od nowa

| Blok | Faza    | Status | Dependencies |
| ---- | ------- | ------ | ------------ |
| 0.1  | FAZA 0  | ⬜     | -            |
| 0.2  | FAZA 0  | ⬜     | 0.1          |
| 0.3  | FAZA 0  | ⬜     | 0.1          |
| 1.1  | FAZA 1  | ⬜     | 0.x          |
| 1.2  | FAZA 1  | ⬜     | 1.1          |
| 1.3  | FAZA 1  | ⬜     | 1.1          |
| 1.4  | FAZA 1  | ⬜     | 1.1, 1.3     |
| 2.1  | FAZA 2  | ⬜     | 1.x          |
| 2.2  | FAZA 2  | ⬜     | 2.1          |
| 3.1  | FAZA 3  | ⬜     | 2.x          |
| 3.2  | FAZA 3  | ⬜     | 3.1          |
| 4A.1 | FAZA 4A | ⬜     | 3.x          |
| 4A.2 | FAZA 4A | ⬜     | 4A.1         |
| 4A.3 | FAZA 4A | ⬜     | 4A.1         |
| 4B.1 | FAZA 4B | ⬜     | 3.x          |
| 4B.2 | FAZA 4B | ⬜     | 4B.1         |
| 5.1  | FAZA 5  | ⬜     | 4A.x, 4B.x   |
| 5.2  | FAZA 5  | ⬜     | 5.1          |
| 6.1  | FAZA 6  | ⬜     | 5.x          |
| 6.2  | FAZA 6  | ⬜     | 6.1          |
| 7.1  | FAZA 7  | ⬜     | 6.x          |
| 8.1  | FAZA 8  | ⬜     | 7.1          |
| 8.2  | FAZA 8  | ⬜     | 8.1          |
| 9.1  | FAZA 9  | ⬜     | 1.1, 5.x     |

---

_Ostatnia aktualizacja: 2025-12-06_  
_Wersja: 6.1 (z markerami dla agentów AI)_
