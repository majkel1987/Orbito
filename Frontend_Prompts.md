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
> 1. 🚨 **PRZECZYTAJ `API_RULES.md` PRZED ROZPOCZĘCIEM!** (obowiązkowe dla bloków z API)
> 2. Znajdź swój blok używając markera z `feature_list.json` (pole `promptMarker`)
> 3. Przeczytaj CAŁY prompt przed rozpoczęciem pracy
> 4. Wykonuj DOKŁADNIE kroki opisane w promptcie
> 5. Na końcu przejdź przez CHECKLIST WERYFIKACJI
> 6. Zaktualizuj `feature_list.json` i `claude-progress.txt`

---

## 🚨 KRYTYCZNE ZASADY API

> **UWAGA**: Te zasady powstały po naprawie krytycznych bugów (2025-12-09).
> Ich ignorowanie prowadzi do 401 Unauthorized i hardcoded danych.

### ❌ ABSOLUTNIE ZABRONIONE:

1. **Hardcoded Data**: `const data = 0`, `const items = []`, `const user = { name: "Test" }`
2. **Mock Functions**: `console.log('TODO: call API')`, placeholder funkcje
3. **TODO Comments**: Zostawianie TODO bez implementacji
4. **Pomijanie Auth Interceptora**: W bloku 1.1 MUSISZ dodać auth interceptor do `client.ts`!

### ✅ ZAWSZE WYMAGANE:

1. **Import Hooków z Orval**: `import { useGetApiClients } from "@/core/api/generated/..."`
2. **Obsługa 3 Stanów**: loading (Skeleton), error (ErrorMessage), data (actual content)
3. **Weryfikacja Network Tab**: Przed `passes: true` sprawdź DevTools → 200 OK + Authorization header
4. **TypeScript**: Zero błędów, zero `any`, zero `@ts-ignore`

### 📖 Pełna Dokumentacja:

**Przed rozpoczęciem bloku który używa API - PRZECZYTAJ: [`API_RULES.md`](.agent/API_RULES.md)**

---

<!-- BLOCK_START: 0.1 -->

## 🔵 FAZA 0: Setup & Configuration (Tydzień 1)

### 0.1 Inicjalizacja Projektu

| #     | Zadanie                              | Priorytet | Status | Opis                                                                |
| ----- | ------------------------------------ | --------- | ------ | ------------------------------------------------------------------- |
| 0.1.1 | 🔴 Utworzenie projektu Next.js 15    | Krytyczne | ✅     | `create-next-app` z TypeScript, Tailwind, App Router, src directory |
| 0.1.2 | 🔴 Konfiguracja tsconfig.json strict | Krytyczne | ✅     | strict: true, allowJs: false, wszystkie strict\* opcje              |
| 0.1.3 | 🔴 Struktura katalogów               | Krytyczne | ✅     | Utworzenie features/, shared/, core/ zgodnie z planem               |

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
- [x] Git commit: `feat(ui): setup shadcn/ui with custom paths`

<!-- BLOCK_END: 0.2 -->

---

<!-- BLOCK_START: 0.3 -->

### 0.3 API Layer Setup

| #     | Zadanie                          | Priorytet | Status | Opis                                                 |
| ----- | -------------------------------- | --------- | ------ | ---------------------------------------------------- |
| 0.3.1 | 🔴 Konfiguracja Orval            | Krytyczne | ✅     | orval.config.ts z konfiguracją do generowania hooków |
| 0.3.2 | 🔴 Axios instance                | Krytyczne | ✅     | client.ts z baseURL i interceptorami                 |
| 0.3.3 | 🔴 Result<T> interceptor         | Krytyczne | ✅     | Response interceptor rozpakowujący Result<T>         |
| 0.3.4 | 🔴 Generowanie pierwszych hooków | Krytyczne | ✅     | npm run api:generate → pliki w core/api/generated    |
| 0.3.5 | 🟡 Typy pomocnicze               | Ważne     | ✅     | ApiError type, isApiError guard                      |

**Blok 0.3 - Wymagania wejściowe**: Blok 0.1  
**Blok 0.3 - Rezultat**: Działający system generowania API hooków

**📦 DEPENDENCIES:**

- ✅ Blok 0.1 (Inicjalizacja Projektu)

**⬅️ BLOKUJE:**

- Wszystkie bloki korzystające z API (1.1, 3.x, 4.x, 5.x, 6.x)

**⚠️ KRYTYCZNA UWAGA:**
Auth interceptor (dodający token JWT do nagłówków) będzie dodany w **Bloku 1.1**!
W tym bloku tworzymy tylko infrastrukturę bez autoryzacji.

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer specjalizujący się w integracji API.
Twoim zadaniem jest wykonanie Fazy 0.3 (API Layer Setup) z planu implementacji.

### CEL GŁÓWNY:

Skonfigurować Orval do automatycznego generowania React Query hooków z OpenAPI spec oraz przygotować Axios client z interceptorami obsługującymi nasz format odpowiedzi `Result<T>`.

### KONTEKST BACKENDU:

Backend zwraca odpowiedzi w formacie:

```json
{
  "isSuccess": true,
  "value": { ... },  // dane gdy sukces
  "error": null
}
// lub
{
  "isSuccess": false,
  "value": null,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "..."
  }
}
```

### KROKI DO WYKONANIA:

**KROK 1: Instalacja zależności**

```bash
npm install axios @tanstack/react-query
npm install -D orval
```

**KROK 2: Orval Config (Zadanie 0.3.1)**
Utwórz `orval.config.ts` w root projektu:

```typescript
import { defineConfig } from "orval";

export default defineConfig({
  orbito: {
    input: {
      target: "http://localhost:5211/swagger/v1/swagger.json",
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
          name: "customInstance",
        },
      },
    },
  },
});
```

**KROK 3: Axios Client (Zadanie 0.3.2, 0.3.3)**
Utwórz `src/core/api/client.ts`:

```typescript
import axios, { AxiosRequestConfig, AxiosResponse } from "axios";

const axiosInstance = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL || "http://localhost:5211",
  headers: {
    "Content-Type": "application/json",
  },
});

// Result<T> Response Interceptor
axiosInstance.interceptors.response.use(
  (response: AxiosResponse) => {
    // Backend zwraca { isSuccess, value, error }
    const data = response.data;
    if (data && typeof data === "object" && "isSuccess" in data) {
      if (data.isSuccess) {
        response.data = data.value;
      } else {
        return Promise.reject({
          code: data.error?.code || "UNKNOWN_ERROR",
          message: data.error?.message || "An error occurred",
        });
      }
    }
    return response;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// 🚨 AUTH INTERCEPTOR BĘDZIE DODANY W BLOKU 1.1!
// NIE ZOSTAWIAJ TODO - to jest celowe rozdzielenie bloków

export const customInstance = <T>(config: AxiosRequestConfig): Promise<T> => {
  const promise = axiosInstance(config).then((response) => response.data as T);
  return promise;
};

export default axiosInstance;
```

**KROK 4: npm script (Zadanie 0.3.4)**
Dodaj do `package.json`:

```json
{
  "scripts": {
    "api:generate": "orval"
  }
}
```

Uruchom `npm run api:generate` - powinny powstać pliki w `src/core/api/generated/`.

**KROK 5: Typy pomocnicze (Zadanie 0.3.5)**
Utwórz `src/core/api/types.ts`:

```typescript
export interface ApiError {
  code: string;
  message: string;
}

export function isApiError(error: unknown): error is ApiError {
  return (
    typeof error === "object" &&
    error !== null &&
    "code" in error &&
    "message" in error
  );
}
```

### OCZEKIWANY REZULTAT:

Po uruchomieniu `npm run api:generate`:

- Katalog `src/core/api/generated/` zawiera pliki z hookami (np. `clients.ts`, `plans.ts`)
- Katalog `src/core/api/generated/model/` zawiera typy DTO
- `customInstance` jest gotowy do użycia (bez auth - to będzie w 1.1)

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [x] `orval.config.ts` utworzony
- [x] `src/core/api/client.ts` z customInstance
- [x] Result<T> interceptor działa
- [x] `npm run api:generate` generuje hooki
- [x] Wygenerowane typy DTO (ClientDto, PlanDto, etc.)
- [x] ApiError type i isApiError guard
- [x] NIE MA auth interceptora (to jest w 1.1)
- [x] Git commit: `feat(api): setup Orval and Axios client with Result<T> handling`

<!-- BLOCK_END: 0.3 -->

---

<!-- BLOCK_START: 1.1 -->

## 🔵 FAZA 1: Auth & Tenant Context (Tydzień 2)

### 1.1 NextAuth Configuration

| #     | Zadanie                             | Priorytet | Status | Opis                                                      |
| ----- | ----------------------------------- | --------- | ------ | --------------------------------------------------------- |
| 1.1.1 | 🔴 NextAuth v5 setup                | Krytyczne | ✅     | auth.ts, route handler, next.config.js                    |
| 1.1.2 | 🔴 Credentials provider             | Krytyczne | ✅     | Login via POST /api/Account/login                         |
| 1.1.3 | 🔴 JWT callback z accessToken       | Krytyczne | ✅     | Zapisanie tokena JWT do sesji                             |
| 1.1.4 | 🔴 Session callback                 | Krytyczne | ✅     | Udostępnienie accessToken w useSession                    |
| 1.1.5 | 🔴 **AUTH INTERCEPTOR w client.ts** | Krytyczne | ✅     | 🚨 Interceptor dodający Authorization header do requestów |

**Blok 1.1 - Wymagania wejściowe**: Bloki 0.x  
**Blok 1.1 - Rezultat**: Działająca autoryzacja z tokenem JWT

**📦 DEPENDENCIES:**

- ✅ Blok 0.1 (Project Init)
- ✅ Blok 0.2 (UI Kit)
- ✅ Blok 0.3 (API Layer)

**⬅️ BLOKUJE:**

- Wszystkie bloki wymagające autoryzacji (1.2, 1.3, 1.4, 2.x, 3.x, 4.x, 5.x, 6.x)

**🚨 KRYTYCZNE:**
Ten blok MUSI dodać auth interceptor do `client.ts`! Bez tego WSZYSTKIE requesty do API zwrócą 401 Unauthorized!

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer specjalizujący się w Auth.
Twoim zadaniem jest wykonanie Fazy 1.1 (NextAuth Configuration) z planu implementacji.

### CEL GŁÓWNY:

Skonfigurować NextAuth v5 z Credentials provider i **DODAĆ AUTH INTERCEPTOR do Axios client**.

### API ENDPOINTS:

```
POST /api/Account/login
Body: { email: string, password: string }
Response (Result<T>): { token: string, user: { id, email, role, tenantId } }

POST /api/Account/register
Body: { email, password, firstName, lastName }
Response (Result<T>): { token: string, user: { ... } }
```

### KROKI DO WYKONANIA:

**KROK 1: Instalacja NextAuth v5**

```bash
npm install next-auth@beta
```

**KROK 2: auth.ts (Zadanie 1.1.1, 1.1.2)**
Utwórz `src/core/auth/auth.ts`:

```typescript
import NextAuth from "next-auth";
import Credentials from "next-auth/providers/credentials";
import { customInstance } from "@/core/api/client";

export const { handlers, signIn, signOut, auth } = NextAuth({
  providers: [
    Credentials({
      async authorize(credentials) {
        try {
          const response = await customInstance<{
            token: string;
            user: {
              id: string;
              email: string;
              role: string;
              tenantId: string;
            };
          }>({
            url: "/api/Account/login",
            method: "POST",
            data: {
              email: credentials.email,
              password: credentials.password,
            },
          });

          return {
            id: response.user.id,
            email: response.user.email,
            role: response.user.role,
            tenantId: response.user.tenantId,
            accessToken: response.token,
          };
        } catch {
          return null;
        }
      },
    }),
  ],
  callbacks: {
    async jwt({ token, user }) {
      if (user) {
        token.accessToken = user.accessToken;
        token.role = user.role;
        token.tenantId = user.tenantId;
      }
      return token;
    },
    async session({ session, token }) {
      session.accessToken = token.accessToken as string;
      session.user.role = token.role as string;
      session.user.tenantId = token.tenantId as string;
      return session;
    },
  },
  pages: {
    signIn: "/login",
  },
});
```

**KROK 3: Route Handler**
Utwórz `src/app/api/auth/[...nextauth]/route.ts`:

```typescript
import { handlers } from "@/core/auth/auth";
export const { GET, POST } = handlers;
```

**KROK 4: TypeScript types**
Utwórz `src/core/auth/next-auth.d.ts`:

```typescript
import "next-auth";

declare module "next-auth" {
  interface User {
    accessToken?: string;
    role?: string;
    tenantId?: string;
  }

  interface Session {
    accessToken?: string;
    user: User & {
      role?: string;
      tenantId?: string;
    };
  }
}

declare module "next-auth/jwt" {
  interface JWT {
    accessToken?: string;
    role?: string;
    tenantId?: string;
  }
}
```

**KROK 5: 🚨 AUTH INTERCEPTOR (Zadanie 1.1.5) - KRYTYCZNE!**
Zaktualizuj `src/core/api/client.ts`:

```typescript
import axios, { AxiosRequestConfig, AxiosResponse } from "axios";
import { getSession } from "next-auth/react";

const axiosInstance = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL || "http://localhost:5211",
  headers: {
    "Content-Type": "application/json",
  },
});

// 🚨 AUTH INTERCEPTOR - dodaje token do KAŻDEGO requestu
axiosInstance.interceptors.request.use(
  async (config) => {
    const session = await getSession();
    if (session?.accessToken) {
      config.headers.Authorization = `Bearer ${session.accessToken}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Result<T> Response Interceptor
axiosInstance.interceptors.response.use(
  (response: AxiosResponse) => {
    const data = response.data;
    if (data && typeof data === "object" && "isSuccess" in data) {
      if (data.isSuccess) {
        response.data = data.value;
      } else {
        return Promise.reject({
          code: data.error?.code || "UNKNOWN_ERROR",
          message: data.error?.message || "An error occurred",
        });
      }
    }
    return response;
  },
  (error) => Promise.reject(error)
);

export const customInstance = <T>(config: AxiosRequestConfig): Promise<T> => {
  return axiosInstance(config).then((response) => response.data as T);
};

export default axiosInstance;
```

### OCZEKIWANY REZULTAT:

- NextAuth v5 skonfigurowany
- Logowanie przez `/api/Account/login` działa
- Token JWT zapisany w sesji
- **KAŻDY request do API ma header `Authorization: Bearer ...`**

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [x] NextAuth v5 zainstalowany i skonfigurowany
- [x] Credentials provider wywołuje POST /api/Account/login
- [x] JWT callback zapisuje accessToken
- [x] Session callback udostępnia accessToken
- [x] 🚨 **AUTH INTERCEPTOR w client.ts DODANY!**
- [x] Sprawdź w Network tab: requesty mają header Authorization
- [x] Git commit: `feat(auth): setup NextAuth v5 with JWT and auth interceptor`

<!-- BLOCK_END: 1.1 -->

---

<!-- BLOCK_START: 1.2 -->

### 1.2 Tenant Context

| #     | Zadanie            | Priorytet | Status | Opis                                     |
| ----- | ------------------ | --------- | ------ | ---------------------------------------- |
| 1.2.1 | 🔴 TenantProvider  | Krytyczne | ✅     | React context z tenantId z sesji         |
| 1.2.2 | 🔴 useTenant hook  | Krytyczne | ✅     | Hook do odczytu tenant context           |
| 1.2.3 | 🔴 TenantGuard     | Krytyczne | ✅     | Component sprawdzający dostęp do tenanta |
| 1.2.4 | 🟡 Tenant switcher | Ważne     | ✅     | Dla admins: wybór tenanta (opcjonalne)   |

**Blok 1.2 - Wymagania wejściowe**: Blok 1.1  
**Blok 1.2 - Rezultat**: System multi-tenant context

**📦 DEPENDENCIES:**

- ✅ Blok 1.1 (NextAuth)

**⬅️ BLOKUJE:**

- Wszystkie komponenty wymagające tenantId

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer.
Twoim zadaniem jest wykonanie Fazy 1.2 (Tenant Context) z planu implementacji.

### CEL GŁÓWNY:

Stworzyć system kontekstu multi-tenant oparty na tenantId z sesji NextAuth.

### KROKI DO WYKONANIA:

**KROK 1: TenantProvider (Zadanie 1.2.1)**
Utwórz `src/core/tenant/TenantProvider.tsx`:

```typescript
"use client";

import { createContext, ReactNode } from "react";
import { useSession } from "next-auth/react";

interface TenantContextType {
  tenantId: string | null;
  isLoading: boolean;
}

export const TenantContext = createContext<TenantContextType>({
  tenantId: null,
  isLoading: true,
});

export function TenantProvider({ children }: { children: ReactNode }) {
  const { data: session, status } = useSession();

  return (
    <TenantContext.Provider
      value={{
        tenantId: session?.user?.tenantId ?? null,
        isLoading: status === "loading",
      }}
    >
      {children}
    </TenantContext.Provider>
  );
}
```

**KROK 2: useTenant hook (Zadanie 1.2.2)**
Utwórz `src/core/tenant/useTenant.ts`:

```typescript
"use client";

import { useContext } from "react";
import { TenantContext } from "./TenantProvider";

export function useTenant() {
  const context = useContext(TenantContext);
  if (!context) {
    throw new Error("useTenant must be used within TenantProvider");
  }
  return context;
}
```

**KROK 3: TenantGuard (Zadanie 1.2.3)**
Utwórz `src/core/tenant/TenantGuard.tsx`:

```typescript
"use client";

import { ReactNode } from "react";
import { useTenant } from "./useTenant";
import { redirect } from "next/navigation";

export function TenantGuard({ children }: { children: ReactNode }) {
  const { tenantId, isLoading } = useTenant();

  if (isLoading) {
    return <div>Loading...</div>;
  }

  if (!tenantId) {
    redirect("/login");
  }

  return <>{children}</>;
}
```

### OCZEKIWANY REZULTAT:

TenantProvider i useTenant hook działają, TenantGuard chroni strony.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [x] TenantProvider utworzony
- [x] useTenant() zwraca tenantId z sesji
- [x] TenantGuard przekierowuje nieautoryzowanych
- [x] Git commit: `feat(tenant): implement tenant context and guard`

<!-- BLOCK_END: 1.2 -->

---

<!-- BLOCK_START: 1.3 -->

### 1.3 Auth Store & Middleware

| #     | Zadanie               | Priorytet | Status | Opis                                      |
| ----- | --------------------- | --------- | ------ | ----------------------------------------- |
| 1.3.1 | 🔴 Zustand auth store | Krytyczne | ✅     | Stan user, isAuthenticated, actions       |
| 1.3.2 | 🔴 NextJS middleware  | Krytyczne | ✅     | Ochrona tras /dashboard/\*                |
| 1.3.3 | 🟡 Session sync       | Ważne     | ✅     | Synchronizacja Zustand z NextAuth session |

**Blok 1.3 - Wymagania wejściowe**: Blok 1.1  
**Blok 1.3 - Rezultat**: Middleware ochrony tras i auth store

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer.
Twoim zadaniem jest wykonanie Fazy 1.3 (Auth Store & Middleware).

### KROKI DO WYKONANIA:

**KROK 1: Middleware (Zadanie 1.3.2)**
Utwórz `src/middleware.ts`:

```typescript
import { auth } from "@/core/auth/auth";
import { NextResponse } from "next/server";

export default auth((req) => {
  const isLoggedIn = !!req.auth;
  const isOnDashboard = req.nextUrl.pathname.startsWith("/dashboard");
  const isOnAuth =
    req.nextUrl.pathname.startsWith("/login") ||
    req.nextUrl.pathname.startsWith("/register");

  if (isOnDashboard && !isLoggedIn) {
    return NextResponse.redirect(new URL("/login", req.url));
  }

  if (isOnAuth && isLoggedIn) {
    return NextResponse.redirect(new URL("/dashboard", req.url));
  }

  return NextResponse.next();
});

export const config = {
  matcher: ["/dashboard/:path*", "/login", "/register"],
};
```

**KROK 2: Auth Store (Zadanie 1.3.1)**
Utwórz `src/core/auth/useAuthStore.ts`:

```typescript
import { create } from "zustand";

interface AuthState {
  isAuthenticated: boolean;
  user: {
    id: string;
    email: string;
    role: string;
    tenantId: string;
  } | null;
  setAuth: (user: AuthState["user"]) => void;
  clearAuth: () => void;
}

export const useAuthStore = create<AuthState>((set) => ({
  isAuthenticated: false,
  user: null,
  setAuth: (user) => set({ isAuthenticated: true, user }),
  clearAuth: () => set({ isAuthenticated: false, user: null }),
}));
```

### OCZEKIWANY REZULTAT:

Middleware chroni /dashboard/\*, auth store gotowy do użycia.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [x] Middleware chroni /dashboard/\*
- [x] Niezalogowany user przekierowany do /login
- [x] Zalogowany user na /login przekierowany do /dashboard
- [x] Auth store z Zustand
- [x] Git commit: `feat(auth): add middleware and auth store`

<!-- BLOCK_END: 1.3 -->

---

<!-- BLOCK_START: 1.4 -->

### 1.4 Auth UI Pages

| #     | Zadanie                  | Priorytet | Status | Opis                                |
| ----- | ------------------------ | --------- | ------ | ----------------------------------- |
| 1.4.1 | 🔴 Login page            | Krytyczne | ✅     | /login z formularzem                |
| 1.4.2 | 🔴 Register page         | Krytyczne | ✅     | /register z formularzem             |
| 1.4.3 | 🔴 Auth forms components | Krytyczne | ✅     | LoginForm, RegisterForm z RHF + Zod |
| 1.4.4 | 🟡 Error handling        | Ważne     | ✅     | Toast notifications dla błędów      |
| 1.4.5 | 🟡 Loading states        | Ważne     | ✅     | Button loading podczas submit       |

**Blok 1.4 - Wymagania wejściowe**: Blok 1.1, 1.3  
**Blok 1.4 - Rezultat**: Działające strony logowania i rejestracji

**📦 DEPENDENCIES:**

- ✅ Blok 1.1 (NextAuth)
- ✅ Blok 1.3 (Middleware)

**API ENDPOINTS:**

```
POST /api/Account/login
POST /api/Account/register
```

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer.
Twoim zadaniem jest wykonanie Fazy 1.4 (Auth UI Pages).

### KROKI DO WYKONANIA:

**KROK 1: Auth Schemas**
Utwórz `src/features/auth/schemas/auth.schemas.ts`:

```typescript
import { z } from "zod";

export const LoginSchema = z.object({
  email: z.string().email("Invalid email"),
  password: z.string().min(1, "Password required"),
});

export const RegisterSchema = z.object({
  email: z.string().email("Invalid email"),
  password: z.string().min(8, "Min 8 characters"),
  firstName: z.string().min(2),
  lastName: z.string().min(2),
});
```

**KROK 2: Login Page**
Utwórz `src/app/(auth)/login/page.tsx` z LoginForm używającym:

- React Hook Form
- Zod validation
- signIn z NextAuth
- Toast dla błędów

**KROK 3: Register Page**
Analogicznie dla `/register`.

### OCZEKIWANY REZULTAT:

Działające formularze logowania i rejestracji z walidacją i obsługą błędów.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [x] Login form wysyła POST /api/Account/login
- [x] Register form wysyła POST /api/Account/register
- [x] Zod validation działa
- [x] Error toast dla błędnych danych
- [x] Loading state podczas submit
- [x] Redirect do /dashboard po sukcesie
- [x] Git commit: `feat(auth): implement login and register pages`

<!-- BLOCK_END: 1.4 -->

---

<!-- BLOCK_START: 2.1 -->

## 🔵 FAZA 2: Layout & Global UI (Tydzień 3)

### 2.1 Layout Components

| #     | Zadanie               | Priorytet | Status | Opis                                   |
| ----- | --------------------- | --------- | ------ | -------------------------------------- |
| 2.1.1 | 🔴 Dashboard layout   | Krytyczne | ✅     | Layout z sidebar i header              |
| 2.1.2 | 🔴 Sidebar component  | Krytyczne | ✅     | Nawigacja główna                       |
| 2.1.3 | 🔴 Header component   | Krytyczne | ✅     | Logo, search, notifications, user menu |
| 2.1.4 | 🔴 UserMenu component | Krytyczne | ✅     | Dropdown z danymi z sesji, logout      |
| 2.1.5 | 🟡 Mobile responsive  | Ważne     | ✅     | Hamburger menu na mobile               |

**Blok 2.1 - Wymagania wejściowe**: Faza 1  
**Blok 2.1 - Rezultat**: Gotowy layout dashboardu

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer.
Twoim zadaniem jest wykonanie Fazy 2.1 (Layout Components).

### CEL GŁÓWNY:

Stworzyć profesjonalny layout dashboardu z sidebar, header i user menu.

### KROKI DO WYKONANIA:

**KROK 1: Dashboard Layout**
Utwórz `src/app/(dashboard)/layout.tsx`:

```typescript
import { Sidebar } from "@/shared/components/layout/Sidebar";
import { Header } from "@/shared/components/layout/Header";
import { TenantProvider } from "@/core/tenant/TenantProvider";
import { SessionProvider } from "next-auth/react";

export default function DashboardLayout({ children }) {
  return (
    <SessionProvider>
      <TenantProvider>
        <div className="flex h-screen">
          <Sidebar />
          <div className="flex-1 flex flex-col">
            <Header />
            <main className="flex-1 overflow-auto p-6">{children}</main>
          </div>
        </div>
      </TenantProvider>
    </SessionProvider>
  );
}
```

**KROK 2: Sidebar**
Utwórz `src/shared/components/layout/Sidebar.tsx`:

- Logo
- Navigation items (Dashboard, Team, Clients, Plans, Subscriptions, Payments)
- Active state

**KROK 3: Header**
Utwórz `src/shared/components/layout/Header.tsx`:

- Search input
- Notifications bell
- UserMenu

**KROK 4: UserMenu**
Utwórz `src/shared/components/layout/UserMenu.tsx`:

- Avatar z danymi z sesji (NIE hardcoded!)
- Dropdown: Profile, Settings, Logout
- signOut z NextAuth

### OCZEKIWANY REZULTAT:

Profesjonalny dashboard layout.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [x] Dashboard layout z sidebar i header
- [x] Sidebar z nawigacją
- [x] UserMenu pokazuje dane z sesji (nie hardcoded)
- [x] Logout działa
- [x] Mobile responsive
- [x] Git commit: `feat(layout): implement dashboard layout with sidebar and header`

<!-- BLOCK_END: 2.1 -->

---

<!-- BLOCK_START: 2.2 -->

### 2.2 Global State & Feedback

| #     | Zadanie                | Priorytet | Status | Opis                          |
| ----- | ---------------------- | --------- | ------ | ----------------------------- |
| 2.2.1 | 🔴 QueryClientProvider | Krytyczne | ✅     | React Query provider w root   |
| 2.2.2 | 🔴 ErrorBoundary       | Krytyczne | ✅     | Global error catching         |
| 2.2.3 | 🔴 Loading Skeletons   | Krytyczne | ✅     | Reusable skeleton components  |
| 2.2.4 | 🔴 Toast setup         | Krytyczne | ✅     | Sonner toast provider         |
| 2.2.5 | 🟡 Suspense boundaries | Ważne     | ✅     | Suspense dla async components |

**Blok 2.2 - Wymagania wejściowe**: Blok 2.1  
**Blok 2.2 - Rezultat**: Global state management i feedback UI

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer.
Twoim zadaniem jest wykonanie Fazy 2.2 (Global State & Feedback).

### KROKI DO WYKONANIA:

**KROK 1: QueryClientProvider (Zadanie 2.2.1)**
Utwórz `src/core/providers/QueryProvider.tsx`:

```typescript
"use client";

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { useState, ReactNode } from "react";

export function QueryProvider({ children }: { children: ReactNode }) {
  const [queryClient] = useState(
    () =>
      new QueryClient({
        defaultOptions: {
          queries: {
            staleTime: 60 * 1000, // 1 minute
            retry: 1,
          },
        },
      })
  );

  return (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
}
```

**KROK 2: ErrorBoundary (Zadanie 2.2.2)**
Utwórz `src/shared/components/ErrorBoundary.tsx`.

**KROK 3: Loading Skeletons (Zadanie 2.2.3)**
Utwórz skeleton components:

- `TableSkeleton.tsx`
- `CardSkeleton.tsx`
- `FormSkeleton.tsx`

**KROK 4: Toast Setup (Zadanie 2.2.4)**
Dodaj Sonner Toaster do layout.

### OCZEKIWANY REZULTAT:

Global providers i feedback components gotowe.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [x] QueryClientProvider w root
- [x] ErrorBoundary łapie błędy
- [x] Skeleton components
- [x] Toast system działa
- [x] Git commit: `feat(core): setup React Query and global feedback`

<!-- BLOCK_END: 2.2 -->

---

<!-- BLOCK_START: 3.1 -->

## 🔵 FAZA 3: Team Management (Tydzień 4)

### 3.1 Team List & CRUD

| #     | Zadanie               | Priorytet | Status | Opis                                         |
| ----- | --------------------- | --------- | ------ | -------------------------------------------- |
| 3.1.1 | 🔴 Team types & hooks | Krytyczne | ✅     | Typy TeamMemberDto, hooki useGetApiTeam      |
| 3.1.2 | 🔴 TeamMembersTable   | Krytyczne | ✅     | Tabela członków zespołu                      |
| 3.1.3 | 🔴 Delete team member | Krytyczne | ✅     | Dialog potwierdzenia i DELETE /api/Team/{id} |
| 3.1.4 | 🔴 Team list page     | Krytyczne | ✅     | /dashboard/team z tabelą i akcjami           |

**Blok 3.1 - Wymagania wejściowe**: Faza 2 (Layout & State)  
**Blok 3.1 - Rezultat**: Działający moduł zarządzania zespołem

**📦 DEPENDENCIES:**

- ✅ Blok 2.1 (Dashboard Layout)
- ✅ Blok 2.2 (React Query)

**⬅️ BLOKUJE:**

- Blok 3.2 (Invitations)

**🚨 API ENDPOINTS (z feature_list.json):**

```
GET /api/Team → TeamMemberDto[]
DELETE /api/Team/{id} → void
```

**🚨 REQUIRED HOOKS:**

```typescript
useGetApiTeam();
useDeleteApiTeamId();
```

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer.
Twoim zadaniem jest wykonanie Fazy 3.1 (Team List & CRUD).

### CEL GŁÓWNY:

Stworzyć moduł zarządzania członkami zespołu z listą i możliwością usuwania.

### ARCHITEKTURA (Vertical Slice):

```
src/features/team/
├── components/
│   ├── TeamMembersTable.tsx
│   ├── TeamMemberRow.tsx
│   ├── DeleteMemberDialog.tsx
│   └── RoleBadge.tsx
├── hooks/
│   └── useTeamMembers.ts
└── types/
    └── team.types.ts
```

### KROKI DO WYKONANIA:

**KROK 1: Types & Hooks (Zadanie 3.1.1)**
Utwórz `src/features/team/hooks/useTeamMembers.ts`:

```typescript
import { useGetApiTeam, useDeleteApiTeamId } from "@/core/api/generated/team";
import { useQueryClient } from "@tanstack/react-query";

export function useTeamMembers() {
  const queryClient = useQueryClient();

  const { data, isLoading, error } = useGetApiTeam();

  const deleteMutation = useDeleteApiTeamId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ["/api/Team"] });
      },
    },
  });

  return {
    members: data ?? [],
    isLoading,
    error,
    deleteMember: deleteMutation.mutate,
    isDeleting: deleteMutation.isPending,
  };
}
```

**KROK 2: TeamMembersTable (Zadanie 3.1.2)**
Utwórz `src/features/team/components/TeamMembersTable.tsx`:

```typescript
"use client";

import { useTeamMembers } from "../hooks/useTeamMembers";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/shared/ui/table";
import { Badge } from "@/shared/ui/badge";
import { Button } from "@/shared/ui/button";
import { Skeleton } from "@/shared/ui/skeleton";
import { Trash2 } from "lucide-react";

export function TeamMembersTable() {
  const { members, isLoading, error, deleteMember, isDeleting } =
    useTeamMembers();

  if (error) {
    return <div className="text-red-500">Error: {error.message}</div>;
  }

  if (isLoading) {
    return <TeamTableSkeleton />;
  }

  if (members.length === 0) {
    return (
      <div className="text-center py-8 text-muted-foreground">
        No team members yet
      </div>
    );
  }

  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Name</TableHead>
          <TableHead>Email</TableHead>
          <TableHead>Role</TableHead>
          <TableHead>Status</TableHead>
          <TableHead>Actions</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {members.map((member) => (
          <TableRow key={member.id}>
            <TableCell>
              {member.firstName} {member.lastName}
            </TableCell>
            <TableCell>{member.email}</TableCell>
            <TableCell>
              <Badge
                variant={member.role === "Owner" ? "default" : "secondary"}
              >
                {member.role}
              </Badge>
            </TableCell>
            <TableCell>
              <Badge variant={member.isActive ? "success" : "outline"}>
                {member.isActive ? "Active" : "Inactive"}
              </Badge>
            </TableCell>
            <TableCell>
              <Button
                variant="ghost"
                size="icon"
                onClick={() => deleteMember({ id: member.id })}
                disabled={isDeleting || member.role === "Owner"}
              >
                <Trash2 className="h-4 w-4" />
              </Button>
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}

function TeamTableSkeleton() {
  return (
    <div className="space-y-2">
      {[...Array(3)].map((_, i) => (
        <Skeleton key={i} className="h-12 w-full" />
      ))}
    </div>
  );
}
```

**KROK 3: Delete Dialog (Zadanie 3.1.3)**
Utwórz `src/features/team/components/DeleteMemberDialog.tsx`:

```typescript
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@/shared/ui/alert-dialog";

interface Props {
  memberName: string;
  onConfirm: () => void;
  children: React.ReactNode;
}

export function DeleteMemberDialog({ memberName, onConfirm, children }: Props) {
  return (
    <AlertDialog>
      <AlertDialogTrigger asChild>{children}</AlertDialogTrigger>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>Remove team member?</AlertDialogTitle>
          <AlertDialogDescription>
            Are you sure you want to remove {memberName} from the team? This
            action cannot be undone.
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel>Cancel</AlertDialogCancel>
          <AlertDialogAction onClick={onConfirm}>Remove</AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
```

**KROK 4: Team Page (Zadanie 3.1.4)**
Utwórz `src/app/(dashboard)/dashboard/team/page.tsx`:

```typescript
import { TeamMembersTable } from "@/features/team/components/TeamMembersTable";
import { Button } from "@/shared/ui/button";
import { UserPlus } from "lucide-react";
import Link from "next/link";

export default function TeamPage() {
  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Team</h1>
          <p className="text-muted-foreground">Manage your team members</p>
        </div>
        <Link href="/dashboard/team/invite">
          <Button>
            <UserPlus className="mr-2 h-4 w-4" />
            Invite Member
          </Button>
        </Link>
      </div>

      <TeamMembersTable />
    </div>
  );
}
```

### OCZEKIWANY REZULTAT:

Lista członków zespołu z danymi z API i możliwością usuwania.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [x] Network tab: GET /api/Team zwraca 200
- [x] Lista pokazuje PRAWDZIWE dane z bazy
- [x] Role badge wyświetla Owner/Admin/Member
- [x] Usunięcie członka wywołuje DELETE /api/Team/{id}
- [x] Po usunięciu lista się odświeża
- [x] Owner nie może być usunięty
- [x] Loading skeleton podczas ładowania
- [x] Error state dla błędów API
- [x] Empty state gdy brak członków
- [x] Git commit: `feat(team): implement team members list with CRUD`

<!-- BLOCK_END: 3.1 -->

---

<!-- BLOCK_START: 3.2 -->

### 3.2 Invitations

| #     | Zadanie                     | Priorytet | Status | Opis                                          |
| ----- | --------------------------- | --------- | ------ | --------------------------------------------- |
| 3.2.1 | 🔴 Invitation types & hooks | Krytyczne | ✅     | InvitationDto, usePostApiTeamInvite           |
| 3.2.2 | 🔴 InviteForm component     | Krytyczne | ✅     | Formularz zaproszenia (email, role)           |
| 3.2.3 | 🔴 InvitationsList          | Krytyczne | ✅     | Lista pending invitations                     |
| 3.2.4 | 🔴 Invite page              | Krytyczne | ✅     | /dashboard/team/invite                        |
| 3.2.5 | 🟡 Accept invitation page   | Ważne     | ✅     | /invite/[token] - publiczna strona akceptacji |

**Blok 3.2 - Wymagania wejściowe**: Blok 3.1  
**Blok 3.2 - Rezultat**: System zaproszeń do zespołu

**📦 DEPENDENCIES:**

- ✅ Blok 3.1 (Team List)

**⬅️ BLOKUJE:**

- Blok 4A.1 (Clients List)
- Blok 4B.1 (Plans List)

**🚨 API ENDPOINTS (z feature_list.json):**

```
POST /api/Team/invite → InvitationDto
GET /api/Team/invitations → InvitationDto[]
POST /api/Team/invitations/{token}/accept → void
```

**🚨 REQUIRED HOOKS:**

```typescript
usePostApiTeamInvite();
useGetApiTeamInvitations();
```

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer.
Twoim zadaniem jest wykonanie Fazy 3.2 (Invitations).

### CEL GŁÓWNY:

Stworzyć system zaproszeń do zespołu z formularzem wysyłania i listą oczekujących zaproszeń.

### ARCHITEKTURA:

```
src/features/team/
├── components/
│   ├── invitations/
│   │   ├── InviteForm.tsx
│   │   ├── InvitationsList.tsx
│   │   └── InvitationRow.tsx
├── hooks/
│   └── useInvitations.ts
└── schemas/
    └── invitation.schemas.ts
```

### KROKI DO WYKONANIA:

**KROK 1: Schemas (Zadanie 3.2.1)**
Utwórz `src/features/team/schemas/invitation.schemas.ts`:

```typescript
import { z } from "zod";

export const InviteSchema = z.object({
  email: z.string().email("Invalid email address"),
  role: z.enum(["Admin", "Member"]),
});

export type InviteInput = z.infer<typeof InviteSchema>;
```

**KROK 2: Hooks (Zadanie 3.2.1)**
Utwórz `src/features/team/hooks/useInvitations.ts`:

```typescript
import {
  useGetApiTeamInvitations,
  usePostApiTeamInvite,
} from "@/core/api/generated/team";
import { useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useInvitations() {
  const queryClient = useQueryClient();

  const { data, isLoading, error } = useGetApiTeamInvitations();

  const inviteMutation = usePostApiTeamInvite({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ["/api/Team/invitations"] });
        toast.success("Invitation sent!");
      },
      onError: (error) => {
        toast.error(error.message || "Failed to send invitation");
      },
    },
  });

  return {
    invitations: data ?? [],
    isLoading,
    error,
    sendInvite: inviteMutation.mutate,
    isSending: inviteMutation.isPending,
  };
}
```

**KROK 3: InviteForm (Zadanie 3.2.2)**
Utwórz `src/features/team/components/invitations/InviteForm.tsx`:

```typescript
"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { InviteSchema, InviteInput } from "../../schemas/invitation.schemas";
import { useInvitations } from "../../hooks/useInvitations";
import { Button } from "@/shared/ui/button";
import { Input } from "@/shared/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/ui/select";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/shared/ui/form";

export function InviteForm() {
  const { sendInvite, isSending } = useInvitations();

  const form = useForm<InviteInput>({
    resolver: zodResolver(InviteSchema),
    defaultValues: {
      email: "",
      role: "Member",
    },
  });

  function onSubmit(data: InviteInput) {
    sendInvite({ data });
    form.reset();
  }

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
        <FormField
          control={form.control}
          name="email"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Email</FormLabel>
              <FormControl>
                <Input placeholder="colleague@example.com" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="role"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Role</FormLabel>
              <Select onValueChange={field.onChange} defaultValue={field.value}>
                <FormControl>
                  <SelectTrigger>
                    <SelectValue placeholder="Select a role" />
                  </SelectTrigger>
                </FormControl>
                <SelectContent>
                  <SelectItem value="Admin">Admin</SelectItem>
                  <SelectItem value="Member">Member</SelectItem>
                </SelectContent>
              </Select>
              <FormMessage />
            </FormItem>
          )}
        />

        <Button type="submit" disabled={isSending}>
          {isSending ? "Sending..." : "Send Invitation"}
        </Button>
      </form>
    </Form>
  );
}
```

**KROK 4: InvitationsList (Zadanie 3.2.3)**
Utwórz `src/features/team/components/invitations/InvitationsList.tsx`:

```typescript
"use client";

import { useInvitations } from "../../hooks/useInvitations";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/shared/ui/table";
import { Badge } from "@/shared/ui/badge";
import { Skeleton } from "@/shared/ui/skeleton";
import { formatDate } from "@/shared/lib/formatters";

export function InvitationsList() {
  const { invitations, isLoading, error } = useInvitations();

  if (error) {
    return <div className="text-red-500">Error: {error.message}</div>;
  }

  if (isLoading) {
    return <InvitationsListSkeleton />;
  }

  if (invitations.length === 0) {
    return (
      <div className="text-center py-8 text-muted-foreground">
        No pending invitations
      </div>
    );
  }

  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Email</TableHead>
          <TableHead>Role</TableHead>
          <TableHead>Sent</TableHead>
          <TableHead>Expires</TableHead>
          <TableHead>Status</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {invitations.map((invitation) => (
          <TableRow key={invitation.id}>
            <TableCell>{invitation.email}</TableCell>
            <TableCell>
              <Badge variant="outline">{invitation.role}</Badge>
            </TableCell>
            <TableCell>{formatDate(invitation.createdAt)}</TableCell>
            <TableCell>{formatDate(invitation.expiresAt)}</TableCell>
            <TableCell>
              <Badge
                variant={
                  invitation.status === "Pending" ? "warning" : "secondary"
                }
              >
                {invitation.status}
              </Badge>
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}

function InvitationsListSkeleton() {
  return (
    <div className="space-y-2">
      {[...Array(3)].map((_, i) => (
        <Skeleton key={i} className="h-12 w-full" />
      ))}
    </div>
  );
}
```

**KROK 5: Invite Page (Zadanie 3.2.4)**
Utwórz `src/app/(dashboard)/dashboard/team/invite/page.tsx`:

```typescript
import { InviteForm } from "@/features/team/components/invitations/InviteForm";
import { InvitationsList } from "@/features/team/components/invitations/InvitationsList";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/shared/ui/card";
import Link from "next/link";
import { Button } from "@/shared/ui/button";
import { ArrowLeft } from "lucide-react";

export default function InvitePage() {
  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Link href="/dashboard/team">
          <Button variant="ghost" size="icon">
            <ArrowLeft className="h-4 w-4" />
          </Button>
        </Link>
        <div>
          <h1 className="text-3xl font-bold">Invite Team Member</h1>
          <p className="text-muted-foreground">
            Send invitations to join your team
          </p>
        </div>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Send Invitation</CardTitle>
            <CardDescription>
              Enter the email address and role for the new team member
            </CardDescription>
          </CardHeader>
          <CardContent>
            <InviteForm />
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Pending Invitations</CardTitle>
            <CardDescription>
              Invitations waiting to be accepted
            </CardDescription>
          </CardHeader>
          <CardContent>
            <InvitationsList />
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
```

**KROK 6: Accept Invitation Page (Zadanie 3.2.5)**
Utwórz `src/app/(public)/invite/[token]/page.tsx`:

```typescript
"use client";

import { use, useState } from "react";
import { useRouter } from "next/navigation";
import { Button } from "@/shared/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/shared/ui/card";
import { customInstance } from "@/core/api/client";
import { toast } from "sonner";

interface Props {
  params: Promise<{ token: string }>;
}

export default function AcceptInvitationPage({ params }: Props) {
  const { token } = use(params);
  const router = useRouter();
  const [isAccepting, setIsAccepting] = useState(false);

  async function handleAccept() {
    setIsAccepting(true);
    try {
      await customInstance({
        url: `/api/Team/invitations/${token}/accept`,
        method: "POST",
      });
      toast.success("Invitation accepted! Please login to continue.");
      router.push("/login");
    } catch (error) {
      toast.error("Failed to accept invitation. It may have expired.");
    } finally {
      setIsAccepting(false);
    }
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50">
      <Card className="w-full max-w-md">
        <CardHeader>
          <CardTitle>Team Invitation</CardTitle>
          <CardDescription>
            You have been invited to join a team on Orbito Platform
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <p className="text-sm text-muted-foreground">
            Click the button below to accept this invitation and join the team.
          </p>
          <Button
            onClick={handleAccept}
            disabled={isAccepting}
            className="w-full"
          >
            {isAccepting ? "Accepting..." : "Accept Invitation"}
          </Button>
        </CardContent>
      </Card>
    </div>
  );
}
```

### OCZEKIWANY REZULTAT:

System zaproszeń z formularzem, listą pending invitations i stroną akceptacji.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [x] Network tab: POST /api/Team/invite zwraca 200/201
- [x] Network tab: GET /api/Team/invitations zwraca 200
- [x] Formularz wysyła zaproszenie z email i role
- [x] Lista pokazuje pending invitations z API
- [x] Toast notification po wysłaniu zaproszenia
- [x] Strona /invite/[token] akceptuje zaproszenie
- [x] Loading states podczas wysyłania/akceptowania
- [x] Error handling dla błędów API
- [x] Zod validation na formularzu
- [x] Git commit: `feat(team): implement invitations system`

<!-- BLOCK_END: 3.2 -->

---

<!-- BLOCK_START: 4A.1 -->

## 🔵 FAZA 4A: Clients Management (Tydzień 5) - PARALLEL z 4B

### 4A.1 Clients List

| #      | Zadanie                   | Priorytet | Status | Opis                        |
| ------ | ------------------------- | --------- | ------ | --------------------------- |
| 4A.1.1 | 🔴 Client types & hooks   | Krytyczne | ✅     | ClientDto, useGetApiClients |
| 4A.1.2 | 🔴 ClientsTable component | Krytyczne | ✅     | Tabela z paginacją          |
| 4A.1.3 | 🔴 Pagination component   | Krytyczne | ✅     | Reusable pagination         |
| 4A.1.4 | 🔴 Clients list page      | Krytyczne | ✅     | /dashboard/clients          |
| 4A.1.5 | 🔴 Empty & loading states | Krytyczne | ✅     | Skeleton, empty message     |

**Blok 4A.1 - Wymagania wejściowe**: Blok 3.1, 3.2  
**Blok 4A.1 - Rezultat**: Lista klientów z paginacją

**📦 DEPENDENCIES:**

- ✅ Blok 3.1 (Team List)
- ✅ Blok 3.2 (Invitations)

**⬅️ BLOKUJE:**

- Blok 4A.2 (Search & Filters)
- Blok 4A.3 (Client CRUD)

**🚨 API ENDPOINTS (z feature_list.json):**

```
GET /api/Clients → PagedResult<ClientDto>
GET /api/Clients?pageNumber=1&pageSize=10
```

**🚨 REQUIRED HOOKS:**

```typescript
useGetApiClients({ pageNumber, pageSize });
```

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer.
Twoim zadaniem jest wykonanie Fazy 4A.1 (Clients List).

### CEL GŁÓWNY:

Stworzyć listę klientów z paginacją pobierającą dane z API.

### ARCHITEKTURA:

```
src/features/clients/
├── components/
│   ├── ClientsTable.tsx
│   └── ClientRow.tsx
├── hooks/
│   └── useClients.ts
└── types/
    └── client.types.ts
```

### KROKI DO WYKONANIA:

**KROK 1: Hooks (Zadanie 4A.1.1)**
Utwórz `src/features/clients/hooks/useClients.ts`:

```typescript
import { useGetApiClients } from "@/core/api/generated/clients";
import { useState } from "react";

export function useClients() {
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);

  const { data, isLoading, error } = useGetApiClients({
    pageNumber: page,
    pageSize,
  });

  return {
    clients: data?.items ?? [],
    totalCount: data?.totalCount ?? 0,
    totalPages: data?.totalPages ?? 0,
    currentPage: page,
    pageSize,
    isLoading,
    error,
    setPage,
  };
}
```

**KROK 2: ClientsTable (Zadanie 4A.1.2)**
Utwórz `src/features/clients/components/ClientsTable.tsx`:

```typescript
"use client";

import { useClients } from "../hooks/useClients";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/shared/ui/table";
import { Badge } from "@/shared/ui/badge";
import { Skeleton } from "@/shared/ui/skeleton";
import { Pagination } from "@/shared/components/Pagination";
import Link from "next/link";

export function ClientsTable() {
  const { clients, isLoading, error, currentPage, totalPages, setPage } =
    useClients();

  if (error) {
    return <div className="text-red-500">Error: {error.message}</div>;
  }

  if (isLoading) {
    return <ClientsTableSkeleton />;
  }

  if (clients.length === 0) {
    return (
      <div className="text-center py-8 text-muted-foreground">
        No clients yet. Add your first client to get started.
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Name</TableHead>
            <TableHead>Email</TableHead>
            <TableHead>Company</TableHead>
            <TableHead>Status</TableHead>
            <TableHead>Actions</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {clients.map((client) => (
            <TableRow key={client.id}>
              <TableCell className="font-medium">{client.name}</TableCell>
              <TableCell>{client.email}</TableCell>
              <TableCell>{client.company || "-"}</TableCell>
              <TableCell>
                <Badge variant={client.isActive ? "success" : "secondary"}>
                  {client.isActive ? "Active" : "Inactive"}
                </Badge>
              </TableCell>
              <TableCell>
                <Link
                  href={`/dashboard/clients/${client.id}`}
                  className="text-sm text-blue-600 hover:underline"
                >
                  View
                </Link>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>

      {totalPages > 1 && (
        <Pagination
          currentPage={currentPage}
          totalPages={totalPages}
          onPageChange={setPage}
        />
      )}
    </div>
  );
}

function ClientsTableSkeleton() {
  return (
    <div className="space-y-2">
      {[...Array(5)].map((_, i) => (
        <Skeleton key={i} className="h-12 w-full" />
      ))}
    </div>
  );
}
```

**KROK 3: Pagination Component (Zadanie 4A.1.3)**
Utwórz `src/shared/components/Pagination.tsx`:

```typescript
import { Button } from "@/shared/ui/button";
import { ChevronLeft, ChevronRight } from "lucide-react";

interface Props {
  currentPage: number;
  totalPages: number;
  onPageChange: (page: number) => void;
}

export function Pagination({ currentPage, totalPages, onPageChange }: Props) {
  return (
    <div className="flex items-center justify-center gap-2">
      <Button
        variant="outline"
        size="sm"
        onClick={() => onPageChange(currentPage - 1)}
        disabled={currentPage <= 1}
      >
        <ChevronLeft className="h-4 w-4" />
        Previous
      </Button>

      <span className="text-sm text-muted-foreground">
        Page {currentPage} of {totalPages}
      </span>

      <Button
        variant="outline"
        size="sm"
        onClick={() => onPageChange(currentPage + 1)}
        disabled={currentPage >= totalPages}
      >
        Next
        <ChevronRight className="h-4 w-4" />
      </Button>
    </div>
  );
}
```

**KROK 4: Clients Page (Zadanie 4A.1.4)**
Utwórz `src/app/(dashboard)/dashboard/clients/page.tsx`:

```typescript
import { ClientsTable } from "@/features/clients/components/ClientsTable";
import { Button } from "@/shared/ui/button";
import { Plus } from "lucide-react";
import Link from "next/link";

export default function ClientsPage() {
  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Clients</h1>
          <p className="text-muted-foreground">Manage your clients</p>
        </div>
        <Link href="/dashboard/clients/new">
          <Button>
            <Plus className="mr-2 h-4 w-4" />
            Add Client
          </Button>
        </Link>
      </div>

      <ClientsTable />
    </div>
  );
}
```

### OCZEKIWANY REZULTAT:

Lista klientów z paginacją i prawdziwymi danymi z API.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [x] Network tab: GET /api/Clients zwraca 200
- [x] Lista pokazuje PRAWDZIWE dane z bazy
- [x] Paginacja zmienia pageNumber w request
- [x] ZERO hardcoded '0' lub pustych list
- [x] Loading skeleton podczas ładowania
- [x] Empty state gdy brak klientów
- [x] Error state dla błędów API
- [x] Git commit: `feat(clients): implement clients list with pagination`

<!-- BLOCK_END: 4A.1 -->

---

<!-- BLOCK_START: 4A.2 -->

### 4A.2 Clients Search & Filters

| #      | Zadanie          | Priorytet | Status | Opis                           |
| ------ | ---------------- | --------- | ------ | ------------------------------ |
| 4A.2.1 | 🔴 Search input  | Krytyczne | ✅     | Debounced search z query param |
| 4A.2.2 | 🔴 Status filter | Krytyczne | ✅     | Active/Inactive/All dropdown   |
| 4A.2.3 | 🔴 URL sync      | Krytyczne | ✅     | Filtry zapisywane w URL        |
| 4A.2.4 | 🔴 Clear filters | Krytyczne | ✅     | Reset button                   |

**Blok 4A.2 - Wymagania wejściowe**: Blok 4A.1  
**Blok 4A.2 - Rezultat**: Wyszukiwanie i filtry dla klientów

**🚨 API ENDPOINTS:**

```
GET /api/Clients?search=query&status=active
```

**🚨 REQUIRED HOOKS:**

```typescript
useGetApiClients({ search, status, pageNumber });
```

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer.
Twoim zadaniem jest wykonanie Fazy 4A.2 (Clients Search & Filters).

### CEL GŁÓWNY:

Dodać wyszukiwanie i filtry do listy klientów z synchronizacją URL.

### KROKI DO WYKONANIA:

**KROK 1: Rozszerzenie useClients hook**
Zaktualizuj `src/features/clients/hooks/useClients.ts` o obsługę search i status:

```typescript
import { useGetApiClients } from "@/core/api/generated/clients";
import { useState, useDeferredValue } from "react";
import { useSearchParams, useRouter } from "next/navigation";

export function useClients() {
  const searchParams = useSearchParams();
  const router = useRouter();

  const page = Number(searchParams.get("page")) || 1;
  const search = searchParams.get("search") || "";
  const status = searchParams.get("status") || "";

  const deferredSearch = useDeferredValue(search);

  const { data, isLoading, error } = useGetApiClients({
    pageNumber: page,
    pageSize: 10,
    search: deferredSearch || undefined,
    status: status || undefined,
  });

  function updateParams(updates: Record<string, string | undefined>) {
    const params = new URLSearchParams(searchParams.toString());
    Object.entries(updates).forEach(([key, value]) => {
      if (value) {
        params.set(key, value);
      } else {
        params.delete(key);
      }
    });
    // Reset to page 1 when filters change
    if (!updates.page) {
      params.set("page", "1");
    }
    router.push(`?${params.toString()}`);
  }

  return {
    clients: data?.items ?? [],
    totalCount: data?.totalCount ?? 0,
    totalPages: data?.totalPages ?? 0,
    currentPage: page,
    search,
    status,
    isLoading,
    error,
    setSearch: (value: string) => updateParams({ search: value || undefined }),
    setStatus: (value: string) => updateParams({ status: value || undefined }),
    setPage: (value: number) => updateParams({ page: String(value) }),
    clearFilters: () => router.push("?"),
  };
}
```

**KROK 2: ClientsFilters component**
Utwórz `src/features/clients/components/ClientsFilters.tsx`:

```typescript
"use client";

import { Input } from "@/shared/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/ui/select";
import { Button } from "@/shared/ui/button";
import { Search, X } from "lucide-react";

interface Props {
  search: string;
  status: string;
  onSearchChange: (value: string) => void;
  onStatusChange: (value: string) => void;
  onClear: () => void;
}

export function ClientsFilters({
  search,
  status,
  onSearchChange,
  onStatusChange,
  onClear,
}: Props) {
  const hasFilters = search || status;

  return (
    <div className="flex flex-col sm:flex-row gap-4">
      <div className="relative flex-1">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
        <Input
          placeholder="Search clients..."
          value={search}
          onChange={(e) => onSearchChange(e.target.value)}
          className="pl-9"
        />
      </div>

      <Select value={status} onValueChange={onStatusChange}>
        <SelectTrigger className="w-[180px]">
          <SelectValue placeholder="All statuses" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="">All statuses</SelectItem>
          <SelectItem value="active">Active</SelectItem>
          <SelectItem value="inactive">Inactive</SelectItem>
        </SelectContent>
      </Select>

      {hasFilters && (
        <Button variant="ghost" onClick={onClear}>
          <X className="mr-2 h-4 w-4" />
          Clear
        </Button>
      )}
    </div>
  );
}
```

**KROK 3: Integracja z Clients Page**
Zaktualizuj stronę aby używać filtrów.

### OCZEKIWANY REZULTAT:

Wyszukiwanie i filtry z synchronizacją URL.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [x] Wpisanie w search wysyła request z ?search=
- [x] Zmiana status filtruje po statusie
- [x] URL persistence - odświeżenie strony zachowuje filtry
- [x] Clear filters resetuje wszystko
- [x] Debounced search (nie wysyła requestu przy każdym znaku)
- [x] Git commit: `feat(clients): add search and filters with URL sync`

<!-- BLOCK_END: 4A.2 -->

---

<!-- BLOCK_START: 4A.3 -->

### 4A.3 Client CRUD

| #      | Zadanie                 | Priorytet | Status | Opis                         |
| ------ | ----------------------- | --------- | ------ | ---------------------------- |
| 4A.3.1 | 🔴 ClientForm component | Krytyczne | ✅     | Formularz create/edit        |
| 4A.3.2 | 🔴 Create client page   | Krytyczne | ✅     | /dashboard/clients/new       |
| 4A.3.3 | 🔴 Client detail page   | Krytyczne | ✅     | /dashboard/clients/[id]      |
| 4A.3.4 | 🔴 Edit client page     | Krytyczne | ✅     | /dashboard/clients/[id]/edit |
| 4A.3.5 | 🔴 Delete client        | Krytyczne | ✅     | Dialog z potwierdzeniem      |
| 4A.3.6 | 🔴 Toast notifications  | Krytyczne | ✅     | Success/error messages       |

**Blok 4A.3 - Wymagania wejściowe**: Blok 4A.1  
**Blok 4A.3 - Rezultat**: Pełny CRUD dla klientów

**🚨 API ENDPOINTS (z feature_list.json):**

```
POST /api/Clients → ClientDto
GET /api/Clients/{id} → ClientDto
PUT /api/Clients/{id} → ClientDto
DELETE /api/Clients/{id} → void
```

**🚨 REQUIRED HOOKS:**

```typescript
usePostApiClients();
useGetApiClientsId(id);
usePutApiClientsId();
useDeleteApiClientsId();
```

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer.
Twoim zadaniem jest wykonanie Fazy 4A.3 (Client CRUD).

### CEL GŁÓWNY:

Zaimplementować pełny CRUD (Create, Read, Update, Delete) dla klientów.

### KROKI DO WYKONANIA:

**KROK 1: Schemas**
Utwórz `src/features/clients/schemas/client.schemas.ts`:

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

**KROK 2: Client mutations hook**
Utwórz `src/features/clients/hooks/useClientMutations.ts`:

```typescript
import {
  usePostApiClients,
  usePutApiClientsId,
  useDeleteApiClientsId,
} from "@/core/api/generated/clients";
import { useQueryClient } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { toast } from "sonner";

export function useClientMutations() {
  const queryClient = useQueryClient();
  const router = useRouter();

  const createMutation = usePostApiClients({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ["/api/Clients"] });
        toast.success("Client created successfully");
        router.push("/dashboard/clients");
      },
      onError: (error) => {
        toast.error(error.message || "Failed to create client");
      },
    },
  });

  const updateMutation = usePutApiClientsId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ["/api/Clients"] });
        toast.success("Client updated successfully");
        router.push("/dashboard/clients");
      },
      onError: (error) => {
        toast.error(error.message || "Failed to update client");
      },
    },
  });

  const deleteMutation = useDeleteApiClientsId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ["/api/Clients"] });
        toast.success("Client deleted successfully");
        router.push("/dashboard/clients");
      },
      onError: (error) => {
        toast.error(error.message || "Failed to delete client");
      },
    },
  });

  return {
    createClient: createMutation.mutate,
    updateClient: updateMutation.mutate,
    deleteClient: deleteMutation.mutate,
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isDeleting: deleteMutation.isPending,
  };
}
```

**KROK 3: ClientForm component**
Utwórz `src/features/clients/components/ClientForm.tsx` z React Hook Form i Zod.

**KROK 4: Create/Edit/Detail pages**
Utwórz odpowiednie strony w `src/app/(dashboard)/dashboard/clients/`.

### OCZEKIWANY REZULTAT:

Pełny CRUD dla klientów z formularzami i toastami.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [x] POST /api/Clients tworzy klienta
- [x] GET /api/Clients/{id} pobiera szczegóły
- [x] PUT /api/Clients/{id} aktualizuje klienta
- [x] DELETE /api/Clients/{id} usuwa klienta
- [x] Formularz z Zod validation
- [x] Toast notifications dla sukcesu/błędu
- [x] Redirect po operacjach
- [x] Loading states na buttonach
- [x] Git commit: `feat(clients): implement full CRUD operations`

<!-- BLOCK_END: 4A.3 -->

---

<!-- BLOCK_START: 4B.1 -->

## 🔵 FAZA 4B: Plans Management (Tydzień 5) - PARALLEL z 4A

### 4B.1 Plans List

| #      | Zadanie                | Priorytet | Status | Opis                                    |
| ------ | ---------------------- | --------- | ------ | --------------------------------------- |
| 4B.1.1 | 🔴 Plan types & hooks  | Krytyczne | ✅     | PlanDto, useGetApiPlans                 |
| 4B.1.2 | 🔴 PlansGrid component | Krytyczne | ✅     | Grid z kartami planów                   |
| 4B.1.3 | 🔴 PlanCard component  | Krytyczne | ✅     | Karta z ceną, features, status          |
| 4B.1.4 | 🔴 Plans list page     | Krytyczne | ✅     | /dashboard/plans                        |
| 4B.1.5 | 🔴 formatCurrency      | Krytyczne | ✅     | Ceny formatowane przez formatCurrency() |

**Blok 4B.1 - Wymagania wejściowe**: Blok 3.1, 3.2  
**Blok 4B.1 - Rezultat**: Lista planów z kartami

**📦 DEPENDENCIES:**

- ✅ Blok 3.1 (Team List)
- ✅ Blok 3.2 (Invitations)

**⬅️ BLOKUJE:**

- Blok 4B.2 (Plan CRUD)

**🚨 API ENDPOINTS (z feature_list.json):**

```
GET /api/Plans → PlanDto[]
```

**🚨 REQUIRED HOOKS:**

```typescript
useGetApiPlans();
```

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer.
Twoim zadaniem jest wykonanie Fazy 4B.1 (Plans List).

### CEL GŁÓWNY:

Stworzyć listę planów subskrypcyjnych w formie kart z cenami i features.

### ARCHITEKTURA:

```
src/features/plans/
├── components/
│   ├── PlansGrid.tsx
│   ├── PlanCard.tsx
│   └── PlanFeatures.tsx
├── hooks/
│   └── usePlans.ts
└── types/
    └── plan.types.ts
```

### KROKI DO WYKONANIA:

**KROK 1: Hooks (Zadanie 4B.1.1)**
Utwórz `src/features/plans/hooks/usePlans.ts`:

```typescript
import { useGetApiPlans } from "@/core/api/generated/plans";

export function usePlans() {
  const { data, isLoading, error } = useGetApiPlans();

  return {
    plans: data ?? [],
    isLoading,
    error,
  };
}
```

**KROK 2: PlanCard (Zadanie 4B.1.3)**
Utwórz `src/features/plans/components/PlanCard.tsx`:

```typescript
import {
  Card,
  CardContent,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/shared/ui/card";
import { Badge } from "@/shared/ui/badge";
import { Button } from "@/shared/ui/button";
import { Check } from "lucide-react";
import { formatCurrency } from "@/shared/lib/formatters";
import Link from "next/link";

interface PlanCardProps {
  plan: {
    id: string;
    name: string;
    description?: string;
    price: number;
    currency: string;
    interval: string;
    features: string[];
    isActive: boolean;
  };
}

export function PlanCard({ plan }: PlanCardProps) {
  return (
    <Card className={!plan.isActive ? "opacity-60" : ""}>
      <CardHeader>
        <div className="flex items-center justify-between">
          <CardTitle>{plan.name}</CardTitle>
          <Badge variant={plan.isActive ? "default" : "secondary"}>
            {plan.isActive ? "Active" : "Inactive"}
          </Badge>
        </div>
        {plan.description && (
          <p className="text-sm text-muted-foreground">{plan.description}</p>
        )}
      </CardHeader>
      <CardContent className="space-y-4">
        <div>
          <span className="text-3xl font-bold">
            {formatCurrency(plan.price, plan.currency)}
          </span>
          <span className="text-muted-foreground">
            /{plan.interval.toLowerCase()}
          </span>
        </div>

        <ul className="space-y-2">
          {plan.features.map((feature, index) => (
            <li key={index} className="flex items-center gap-2">
              <Check className="h-4 w-4 text-green-500" />
              <span className="text-sm">{feature}</span>
            </li>
          ))}
        </ul>
      </CardContent>
      <CardFooter className="flex gap-2">
        <Link href={`/dashboard/plans/${plan.id}`} className="flex-1">
          <Button variant="outline" className="w-full">
            View
          </Button>
        </Link>
        <Link href={`/dashboard/plans/${plan.id}/edit`} className="flex-1">
          <Button className="w-full">Edit</Button>
        </Link>
      </CardFooter>
    </Card>
  );
}
```

**KROK 3: PlansGrid (Zadanie 4B.1.2)**
Utwórz `src/features/plans/components/PlansGrid.tsx`:

```typescript
"use client";

import { usePlans } from "../hooks/usePlans";
import { PlanCard } from "./PlanCard";
import { Skeleton } from "@/shared/ui/skeleton";

export function PlansGrid() {
  const { plans, isLoading, error } = usePlans();

  if (error) {
    return <div className="text-red-500">Error: {error.message}</div>;
  }

  if (isLoading) {
    return <PlansGridSkeleton />;
  }

  if (plans.length === 0) {
    return (
      <div className="text-center py-8 text-muted-foreground">
        No plans yet. Create your first subscription plan to get started.
      </div>
    );
  }

  return (
    <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
      {plans.map((plan) => (
        <PlanCard key={plan.id} plan={plan} />
      ))}
    </div>
  );
}

function PlansGridSkeleton() {
  return (
    <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
      {[...Array(3)].map((_, i) => (
        <Skeleton key={i} className="h-[400px]" />
      ))}
    </div>
  );
}
```

**KROK 4: Plans Page (Zadanie 4B.1.4)**
Utwórz `src/app/(dashboard)/dashboard/plans/page.tsx`:

```typescript
import { PlansGrid } from "@/features/plans/components/PlansGrid";
import { Button } from "@/shared/ui/button";
import { Plus } from "lucide-react";
import Link from "next/link";

export default function PlansPage() {
  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Plans</h1>
          <p className="text-muted-foreground">
            Manage your subscription plans
          </p>
        </div>
        <Link href="/dashboard/plans/new">
          <Button>
            <Plus className="mr-2 h-4 w-4" />
            Create Plan
          </Button>
        </Link>
      </div>

      <PlansGrid />
    </div>
  );
}
```

### OCZEKIWANY REZULTAT:

Grid kart z planami subskrypcyjnymi i prawdziwymi danymi z API.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [x] Network tab: GET /api/Plans zwraca 200
- [x] Karty planów pokazują prawdziwe dane
- [x] Ceny formatowane przez formatCurrency()
- [x] Features lista wyświetla się poprawnie
- [x] Active/Inactive badge działa
- [x] Loading skeleton podczas ładowania
- [x] Empty state gdy brak planów
- [x] Error state dla błędów API
- [x] Git commit: `feat(plans): implement plans list with cards`

<!-- BLOCK_END: 4B.1 -->

---

<!-- BLOCK_START: 4B.2 -->

### 4B.2 Plan CRUD

| #      | Zadanie               | Priorytet | Status | Opis                                   |
| ------ | --------------------- | --------- | ------ | -------------------------------------- |
| 4B.2.1 | 🔴 Plan schemas       | Krytyczne | ✅     | Zod schemas dla Plan                   |
| 4B.2.2 | 🔴 PlanForm component | Krytyczne | ✅     | Formularz create/edit z features array |
| 4B.2.3 | 🔴 useFieldArray      | Krytyczne | ✅     | Dynamic features list                  |
| 4B.2.4 | 🔴 Create plan page   | Krytyczne | ✅     | /dashboard/plans/new                   |
| 4B.2.5 | 🔴 Plan detail page   | Krytyczne | ✅     | /dashboard/plans/[id]                  |
| 4B.2.6 | 🔴 Edit plan page     | Krytyczne | ✅     | /dashboard/plans/[id]/edit             |
| 4B.2.7 | 🔴 Delete plan        | Krytyczne | ✅     | Dialog z potwierdzeniem                |

**Blok 4B.2 - Wymagania wejściowe**: Blok 4B.1  
**Blok 4B.2 - Rezultat**: Pełny CRUD dla planów

**🚨 API ENDPOINTS (z feature_list.json):**

```
POST /api/Plans → PlanDto
GET /api/Plans/{id} → PlanDto
PUT /api/Plans/{id} → PlanDto
DELETE /api/Plans/{id} → void
```

**🚨 REQUIRED HOOKS:**

```typescript
usePostApiPlans();
useGetApiPlansId(id);
usePutApiPlansId();
useDeleteApiPlansId();
```

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer.
Twoim zadaniem jest wykonanie Fazy 4B.2 (Plan CRUD).

### CEL GŁÓWNY:

Zaimplementować pełny CRUD dla planów z dynamic features list używając useFieldArray.

### KROKI DO WYKONANIA:

**KROK 1: Schemas (Zadanie 4B.2.1)**
Utwórz `src/features/plans/schemas/plan.schemas.ts`:

```typescript
import { z } from "zod";

export const PlanSchema = z.object({
  name: z.string().min(2, "Name must be at least 2 characters"),
  description: z.string().optional(),
  price: z.number().min(0, "Price must be positive"),
  currency: z.string().default("PLN"),
  interval: z.enum(["Monthly", "Quarterly", "Yearly"]),
  features: z
    .array(
      z.object({
        value: z.string().min(1, "Feature cannot be empty"),
      })
    )
    .min(1, "At least one feature required"),
  isActive: z.boolean().default(true),
});

export type PlanInput = z.infer<typeof PlanSchema>;
```

**KROK 2: Plan mutations hook**
Utwórz `src/features/plans/hooks/usePlanMutations.ts` analogicznie do klientów.

**KROK 3: PlanForm with useFieldArray (Zadanie 4B.2.2, 4B.2.3)**
Utwórz `src/features/plans/components/PlanForm.tsx`:

```typescript
"use client";

import { useForm, useFieldArray } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { PlanSchema, PlanInput } from "../schemas/plan.schemas";
import { Button } from "@/shared/ui/button";
import { Input } from "@/shared/ui/input";
import { Textarea } from "@/shared/ui/textarea";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/ui/select";
import { Switch } from "@/shared/ui/switch";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/shared/ui/form";
import { Plus, Trash2 } from "lucide-react";

interface Props {
  defaultValues?: Partial<PlanInput>;
  onSubmit: (data: PlanInput) => void;
  isSubmitting?: boolean;
}

export function PlanForm({ defaultValues, onSubmit, isSubmitting }: Props) {
  const form = useForm<PlanInput>({
    resolver: zodResolver(PlanSchema),
    defaultValues: {
      name: "",
      description: "",
      price: 0,
      currency: "PLN",
      interval: "Monthly",
      features: [{ value: "" }],
      isActive: true,
      ...defaultValues,
    },
  });

  const { fields, append, remove } = useFieldArray({
    control: form.control,
    name: "features",
  });

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
        {/* Basic fields */}
        <FormField
          control={form.control}
          name="name"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Name</FormLabel>
              <FormControl>
                <Input placeholder="Basic Plan" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="grid gap-4 md:grid-cols-3">
          <FormField
            control={form.control}
            name="price"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Price</FormLabel>
                <FormControl>
                  <Input
                    type="number"
                    step="0.01"
                    {...field}
                    onChange={(e) => field.onChange(parseFloat(e.target.value))}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="currency"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Currency</FormLabel>
                <Select
                  onValueChange={field.onChange}
                  defaultValue={field.value}
                >
                  <FormControl>
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                  </FormControl>
                  <SelectContent>
                    <SelectItem value="PLN">PLN</SelectItem>
                    <SelectItem value="EUR">EUR</SelectItem>
                    <SelectItem value="USD">USD</SelectItem>
                  </SelectContent>
                </Select>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="interval"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Billing Interval</FormLabel>
                <Select
                  onValueChange={field.onChange}
                  defaultValue={field.value}
                >
                  <FormControl>
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                  </FormControl>
                  <SelectContent>
                    <SelectItem value="Monthly">Monthly</SelectItem>
                    <SelectItem value="Quarterly">Quarterly</SelectItem>
                    <SelectItem value="Yearly">Yearly</SelectItem>
                  </SelectContent>
                </Select>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        {/* Features with useFieldArray */}
        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <FormLabel>Features</FormLabel>
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={() => append({ value: "" })}
            >
              <Plus className="mr-2 h-4 w-4" />
              Add Feature
            </Button>
          </div>

          {fields.map((field, index) => (
            <div key={field.id} className="flex gap-2">
              <FormField
                control={form.control}
                name={`features.${index}.value`}
                render={({ field }) => (
                  <FormItem className="flex-1">
                    <FormControl>
                      <Input placeholder="Feature description" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              {fields.length > 1 && (
                <Button
                  type="button"
                  variant="ghost"
                  size="icon"
                  onClick={() => remove(index)}
                >
                  <Trash2 className="h-4 w-4" />
                </Button>
              )}
            </div>
          ))}
        </div>

        <FormField
          control={form.control}
          name="isActive"
          render={({ field }) => (
            <FormItem className="flex items-center gap-2">
              <FormControl>
                <Switch
                  checked={field.value}
                  onCheckedChange={field.onChange}
                />
              </FormControl>
              <FormLabel className="!mt-0">Active</FormLabel>
            </FormItem>
          )}
        />

        <Button type="submit" disabled={isSubmitting}>
          {isSubmitting ? "Saving..." : "Save Plan"}
        </Button>
      </form>
    </Form>
  );
}
```

**KROK 4: Create/Edit/Detail pages**
Utwórz odpowiednie strony w `src/app/(dashboard)/dashboard/plans/`.

### OCZEKIWANY REZULTAT:

Pełny CRUD dla planów z dynamic features list.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [x] Utworzenie planu z features działa
- [x] Edycja planu: GET → formularz → PUT
- [x] useFieldArray dodaje/usuwa features
- [x] Delete plan z dialogiem
- [x] Zod validation działa
- [x] Toast notifications
- [x] Loading states
- [x] Git commit: `feat(plans): implement full CRUD with dynamic features`

<!-- BLOCK_END: 4B.2 -->

# BRAKUJĄCE BLOKI DO DODANIA DO Frontend_Prompts.md

> **INSTRUKCJA**: Poniższe bloki należy dodać do pliku `Frontend_Prompts.md`
> bezpośrednio PO bloku `<!-- BLOCK_END: 4B.2 -->` i PRZED sekcją `## 📊 PODSUMOWANIE BLOKÓW`.

---

<!-- BLOCK_START: 5.1 -->

## 🔵 FAZA 5: Subscriptions Management (Tydzień 6)

### 5.1 Subscriptions List

| #     | Zadanie                        | Priorytet | Status | Opis                                          |
| ----- | ------------------------------ | --------- | ------ | --------------------------------------------- |
| 5.1.1 | 🔴 Subscriptions page          | Krytyczne | ✅     | /subscriptions - lista wszystkich subskrypcji |
| 5.1.2 | 🔴 SubscriptionTable component | Krytyczne | ✅     | Tabela z client, plan, status, actions        |
| 5.1.3 | 🔴 SubscriptionStatusBadge     | Krytyczne | ✅     | Badge z kolorami dla statusów                 |
| 5.1.4 | 🔴 Subscription filters        | Krytyczne | ✅     | Filtrowanie po status, plan, date             |
| 5.1.5 | 🔴 Subscriptions hooks         | Krytyczne | ✅     | useSubscriptions, useSubscription             |

**Blok 5.1 - Wymagania wejściowe**: Faza 4A (Klienci) i 4B (Plany) - zakończone  
**Blok 5.1 - Rezultat**: Centralny widok zarządzania subskrypcjami

**📦 DEPENDENCIES:**

- ✅ Blok 4A.x (Clients Management)
- ✅ Blok 4B.x (Plans Management)

**⬅️ BLOKUJE:**

- Blok 5.2 (Subscription Actions)
- Blok 6.x (Payments)

**🚨 API ENDPOINTS:**

```
GET /api/Subscriptions - lista subskrypcji z paginacją i filtrami
GET /api/Subscriptions/{id} - szczegóły subskrypcji
```

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer.
Rozpoczynamy Fazę 5: **Subscriptions Management**.
Jest to moduł łączący domeny Klientów i Planów.

### CEL GŁÓWNY:

Stworzyć widok listy subskrypcji (`/subscriptions`), który pozwala monitorować przychody, statusy płatności i cykle rozliczeniowe klientów.

### STRUKTURA PLIKÓW:

- Feature: `src/features/subscriptions/`
- Komponenty: `src/features/subscriptions/components/`
- Hooki: `src/features/subscriptions/hooks/`
- Strona: `src/app/(dashboard)/subscriptions/page.tsx`

### KROKI DO WYKONANIA:

**KROK 1: Subscriptions Hooks (Zadanie 5.1.5)**
Utwórz `src/features/subscriptions/hooks/useSubscriptions.ts`.
Wymagania:

- Zaimportuj `useGetSubscriptions` z API.
- Stwórz wrapper obsługujący parametry: `page`, `pageSize`, `status`, `planId`, `search` (szukanie po nazwisku klienta).
- Upewnij się, że typ zwracany (DTO) zawiera zagnieżdżone dane o Kliencie (`clientName`, `clientEmail`) i Planie (`planName`, `price`).

**KROK 2: SubscriptionStatusBadge (Zadanie 5.1.3)**
Utwórz `src/features/subscriptions/components/SubscriptionStatusBadge.tsx`.
Wymagania:

- Props: `status` (Enum: Active, Canceled, PastDue, Trialing, Paused).
- UI: Komponent `Badge` z shadcn.
- Mapowanie kolorów:
  - Active -> Green (Success)
  - PastDue -> Red (Destructive)
  - Canceled -> Gray (Secondary)
  - Trialing -> Blue (Info)
  - Paused -> Orange/Yellow (Warning)

**KROK 3: SubscriptionFilters (Zadanie 5.1.4)**
Utwórz `src/features/subscriptions/components/SubscriptionFilters.tsx`.
Wymagania:

- Komponent "use client" zintegrowany z URL.
- Filtry: Status (Multi-select), Plan (Select - dane z `usePlans`).

**KROK 4: SubscriptionTable (Zadanie 5.1.2)**
Utwórz `src/features/subscriptions/components/SubscriptionTable.tsx`.
Wymagania:

- Kolumny: Client (Avatar + Name + Email), Plan (Name + Price + Interval), Status (StatusBadge), Next Billing Date, Actions (Dropdown).
- Wiersz klikalny -> przenosi do `/subscriptions/[id]`.

**KROK 5: Subscriptions Page (Zadanie 5.1.1)**
Utwórz `src/app/(dashboard)/subscriptions/page.tsx`.
Wymagania:

- Header: Title "Subscriptions" + Button "Create Subscription".
- Toolbar: `SubscriptionFilters` + Search.
- Content: `SubscriptionTable`.
- Pagination.

### OCZEKIWANY REZULTAT:

Kod dla hooków, komponentu Badge, Tabeli, Filtrów oraz głównej strony subskrypcji.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [x] useSubscriptions hook importuje z @/core/api/generated
- [x] SubscriptionStatusBadge mapuje wszystkie statusy
- [x] Filtry działają z URL params
- [x] Tabela wyświetla dane z API (nie hardcoded!)
- [x] Paginacja działa
- [x] Loading states z Skeleton
- [x] Network tab: GET /api/Subscriptions z Authorization header
- [x] TypeScript: zero błędów
- [x] Git commit: `feat(subscriptions): implement subscriptions list with filters`

<!-- BLOCK_END: 5.1 -->

---

<!-- BLOCK_START: 5.2 -->

### 5.2 Subscription Actions

| #     | Zadanie                      | Priorytet | Status | Opis                                     |
| ----- | ---------------------------- | --------- | ------ | ---------------------------------------- |
| 5.2.1 | 🔴 Create subscription flow  | Krytyczne | ✅     | Wizard: wybór client → plan → confirm    |
| 5.2.2 | 🔴 Subscription detail page  | Krytyczne | ✅     | /subscriptions/[id]                      |
| 5.2.3 | 🔴 Cancel subscription       | Krytyczne | ✅     | Dialog z reason, immediate/end-of-period |
| 5.2.4 | 🟡 Pause/Resume subscription | Ważne     | ✅     | Zawieszanie subskrypcji                  |
| 5.2.5 | 🟡 Change plan               | Ważne     | ✅     | Upgrade/downgrade planu                  |

**Blok 5.2 - Wymagania wejściowe**: Blok 5.1 (Lista Subskrypcji)  
**Blok 5.2 - Rezultat**: Możliwość tworzenia, edycji i anulowania subskrypcji

**📦 DEPENDENCIES:**

- ✅ Blok 5.1 (Subscriptions List)

**⬅️ BLOKUJE:**

- Blok 6.x (Payments)

**🚨 API ENDPOINTS:**

```
POST /api/Subscriptions - tworzenie subskrypcji
PUT /api/Subscriptions/{id} - aktualizacja
POST /api/Subscriptions/{id}/cancel - anulowanie
POST /api/Subscriptions/{id}/pause - pauza
POST /api/Subscriptions/{id}/resume - wznowienie
```

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer / UX Specialist.
Mamy listę subskrypcji. Teraz implementujemy logikę biznesową zarządzania nimi.

Szczególnym wyzwaniem jest proces tworzenia subskrypcji, który zrealizujemy jako **Multi-step Wizard**.

### CEL GŁÓWNY:

Stworzyć kreator nowej subskrypcji (Wizard), widok szczegółów oraz okna dialogowe do akcji krytycznych (Anulowanie, Zmiana Planu).

### STRUKTURA PLIKÓW:

- Feature: `src/features/subscriptions/`
- Strony:
  - `src/app/(dashboard)/subscriptions/new/page.tsx`
  - `src/app/(dashboard)/subscriptions/[id]/page.tsx`

### KROKI DO WYKONANIA:

**KROK 1: Subscription Action Hooks (Zadania 5.2.3, 5.2.4, 5.2.5)**
Rozbuduj `src/features/subscriptions/hooks/useSubscriptions.ts`:

1. `useCreateSubscription()`: Przyjmuje { clientId, planId, startDate }.
2. `useCancelSubscription()`: Przyjmuje { subscriptionId, reason, cancelImmediately }.
3. `usePauseSubscription()` / `useResumeSubscription()`.
4. `useChangeSubscriptionPlan()`: Przyjmuje { subscriptionId, newPlanId }.

**KROK 2: Create Subscription Wizard (Zadanie 5.2.1)**
Utwórz komponenty w `src/features/subscriptions/components/wizard/`:

1. `CreateSubscriptionWizard.tsx`: Główny kontener z stanem (`step`: 1|2|3).
2. `StepSelectClient.tsx`: Lista klientów z wyszukiwaniem.
3. `StepSelectPlan.tsx`: Wyświetla plany (użyj `PlanCard` z tryb "selectable").
4. `StepConfirm.tsx`: Podsumowanie + Date Picker + przycisk "Create Subscription".

**KROK 3: Subscription Detail Page (Zadanie 5.2.2)**
Utwórz `src/app/(dashboard)/subscriptions/[id]/page.tsx`.
Wymagania:

- Header: ID Subskrypcji, StatusBadge, Data odnowienia.
- Sekcja "Customer": Karta z danymi klienta.
- Sekcja "Current Plan": Karta z danymi planu.
- Sekcja "Actions": "Change Plan", "Pause Subscription", "Cancel Subscription".

**KROK 4: Cancel Subscription Dialog (Zadanie 5.2.3)**
Utwórz `src/features/subscriptions/components/CancelSubscriptionDialog.tsx`.
Wymagania:

- `Reason`: Select (Too expensive, Switching provider, No longer needed, Other).
- `Mode`: Radio Group ("End of current period" vs "Immediately").
- Warning alert informujący o konsekwencjach.

**KROK 5: Change Plan Dialog (Zadanie 5.2.5)**
Utwórz `src/features/subscriptions/components/ChangePlanDialog.tsx`.
Wymagania:

- Wyświetla listę planów (z wykluczeniem obecnego).
- Informacja o zmianie ceny (proration).
- Confirm button: "Update Subscription".

### OCZEKIWANY REZULTAT:

Kod wizarda (wszystkie kroki), strony szczegółów oraz dialogów akcji.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [x] Wizard: 3 kroki działają (Client → Plan → Confirm)
- [x] POST /api/Subscriptions tworzy subskrypcję
- [x] Detail page wyświetla dane z GET /api/Subscriptions/{id}
- [x] Cancel dialog wysyła POST /api/Subscriptions/{id}/cancel
- [x] Change plan dialog działa
- [x] Loading states na wszystkich mutacjach
- [x] Toast notifications po akcjach
- [x] Git commit: `feat(subscriptions): implement subscription wizard and actions`

<!-- BLOCK_END: 5.2 -->

---

<!-- BLOCK_START: 6.1 -->

## 🔵 FAZA 6: Payments (Tydzień 7)

### 6.1 Payment History

| #     | Zadanie                   | Priorytet | Status | Opis                                        |
| ----- | ------------------------- | --------- | ------ | ------------------------------------------- |
| 6.1.1 | 🔴 Payments page          | Krytyczne | ✅     | /payments - historia płatności              |
| 6.1.2 | 🔴 PaymentTable component | Krytyczne | ✅     | Tabela z amount, status, date, client       |
| 6.1.3 | 🔴 PaymentStatusBadge     | Krytyczne | ✅     | Badge: Completed, Pending, Failed, Refunded |
| 6.1.4 | 🔴 Payment detail dialog  | Krytyczne | ✅     | Szczegóły płatności w dialogu               |

**Blok 6.1 - Wymagania wejściowe**: Faza 5 (Subskrypcje)  
**Blok 6.1 - Rezultat**: Przejrzysta historia transakcji finansowych

**📦 DEPENDENCIES:**

- ✅ Blok 5.x (Subscriptions)

**⬅️ BLOKUJE:**

- Blok 6.2 (Payment Methods)
- Blok 7.1 (Analytics)

**🚨 API ENDPOINTS:**

```
GET /api/Payments - lista płatności z paginacją
GET /api/Payments/{id} - szczegóły płatności
```

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer / Fintech Specialist.
Rozpoczynamy Fazę 6: **Payments**.
To moduł krytyczny dla zaufania użytkownika – dane finansowe muszą być prezentowane w sposób czytelny i bezbłędny.

### CEL GŁÓWNY:

Stworzyć tabelę historii płatności z możliwością filtrowania po statusie oraz podglądem szczegółów transakcji w oknie modalnym.

### STRUKTURA PLIKÓW:

- Feature: `src/features/payments/`
- Komponenty: `src/features/payments/components/`
- Hooki: `src/features/payments/hooks/`
- Strona: `src/app/(dashboard)/payments/page.tsx`

### KROKI DO WYKONANIA:

**KROK 1: Payment Hooks**
Utwórz `src/features/payments/hooks/usePayments.ts`.
Wymagania:

- Zaimportuj `useGetPayments` z API.
- Stwórz wrapper obsługujący parametry: `page`, `pageSize`, `status`, `clientId`.
- Zwracane dane: ID, Amount, Currency, Status, Date, Client Info, Payment Method Info.

**KROK 2: PaymentStatusBadge (Zadanie 6.1.3)**
Utwórz `src/features/payments/components/PaymentStatusBadge.tsx`.
Wymagania:

- Kolory:
  - Succeeded / Completed -> Green (Success)
  - Pending / Processing -> Yellow/Blue
  - Failed -> Red (Destructive) - MUSI rzucać się w oczy
  - Refunded -> Gray/Muted

**KROK 3: PaymentDetailDialog (Zadanie 6.1.4)**
Utwórz `src/features/payments/components/PaymentDetailDialog.tsx`.
Wymagania:

- Header: Kwota (duża czcionka) + StatusBadge.
- Content: Transaction ID (z przyciskiem "Copy"), Date & Time, Customer, Payment Method, Invoice ID, Failure Reason (tylko dla Failed).

**KROK 4: PaymentTable (Zadanie 6.1.2)**
Utwórz `src/features/payments/components/PaymentTable.tsx`.
Wymagania:

- Kolumny: Amount (formatCurrency), Status, Client, Date, Method, Actions ("View Details").
- Kliknięcie w "View Details" otwiera `PaymentDetailDialog`.

**KROK 5: Payments Page (Zadanie 6.1.1)**
Utwórz `src/app/(dashboard)/payments/page.tsx`.
Wymagania:

- Tytuł "Payment History".
- Toolbar: Filtr statusu + wyszukiwarka.
- Content: `PaymentTable`.
- Pagination.

### WYMAGANIA TECHNICZNE:

- **Formatowanie Walut**: Użyj `formatCurrency(amount, currency)`.
- **Bezpieczeństwo**: Nie wyświetlaj nigdy pełnych numerów kart, tylko `last4`.

### OCZEKIWANY REZULTAT:

Kod dla hooków, komponentów Badge, Table, Dialog oraz strony głównej płatności.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [x] usePayments hook importuje z @/core/api/generated
- [x] PaymentStatusBadge: Failed status wyróżniony czerwonym
- [x] PaymentTable wyświetla dane z API
- [x] PaymentDetailDialog pokazuje szczegóły
- [x] Kwoty sformatowane przez formatCurrency
- [x] Numery kart: tylko last4 (np. \*\*\*\* 4242)
- [x] Network tab: GET /api/Payments z Authorization header
- [x] Git commit: `feat(payments): implement payment history and detail view`

<!-- BLOCK_END: 6.1 -->

---

<!-- BLOCK_START: 6.2 -->

### 6.2 Payment Methods & Manual Payments

| #     | Zadanie                     | Priorytet | Status | Opis                                      |
| ----- | --------------------------- | --------- | ------ | ----------------------------------------- |
| 6.2.1 | 🔴 PaymentMethodForm        | Krytyczne | ✅     | Formularz dodawania metody płatności      |
| 6.2.2 | 🔴 PaymentMethodList        | Krytyczne | ✅     | Lista metod płatności klienta             |
| 6.2.3 | 🟡 Manual payment recording | Ważne     | ✅     | Dialog do ręcznego wprowadzania płatności |
| 6.2.4 | 🟡 Refund dialog            | Ważne     | ✅     | Dialog zwrotu z reason                    |

**Blok 6.2 - Wymagania wejściowe**: Blok 6.1 (Historia Płatności)  
**Blok 6.2 - Rezultat**: Możliwość dodawania kart, rejestrowania przelewów i wykonywania zwrotów

**📦 DEPENDENCIES:**

- ✅ Blok 6.1 (Payment History)

**⬅️ BLOKUJE:**

- Blok 7.1 (Analytics)

**⚠️ UWAGA PCI DSS:**
Nie implementujemy formularzy zbierających numery kart kredytowych. Polegamy na Stripe Elements.

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer / Fintech Specialist.
Mamy historię płatności. Teraz dodajemy narzędzia operacyjne.

### UWAGA DOTYCZĄCA BEZPIECZEŃSTWA (PCI DSS):

**NIE implementujemy** formularzy zbierających numery kart kredytowych. Jako SaaS musimy polegać na rozwiązaniach dostawcy (Stripe Elements / Payment Links).

### KROKI DO WYKONANIA:

**KROK 1: Payment Operations Hooks**
Rozbuduj `usePayments.ts` o mutacje:

1. `useCreatePaymentSession()`: Backend zwróci URL do Stripe Checkout.
2. `useDeletePaymentMethod()`.
3. `useRecordManualPayment()`: Dla wpłat poza systemem (gotówka/przelew).
4. `useRefundPayment()`: Dla zwrotów.

**KROK 2: PaymentMethodList (Zadanie 6.2.2)**
Utwórz `src/features/payments/components/PaymentMethodList.tsx`.
Wyświetla zamaskowane dane kart (Brand, \*\*\*\* 4242) pobrane z API.
Przycisk "Add Payment Method" otwiera dialog z kroku 3.

**KROK 3: AddPaymentMethod Dialog (Zadanie 6.2.1)**
Utwórz `src/features/payments/components/AddPaymentMethodDialog.tsx`.
Wymagania:

- **ZAMIAST inputów na numer karty**: Wyświetl informację "You will be redirected to our secure payment provider" i przycisk "Proceed to Secure Checkout".
- LUB stwórz kontener `<div id="stripe-elements-placeholder" />` na iframe Stripe.
- **NIE TWÓRZ** inputów `Card Number`, `CVC` w czystym HTML/React!

**KROK 4: ManualPaymentDialog (Zadanie 6.2.3)**
Utwórz `src/features/payments/components/ManualPaymentDialog.tsx`.
Wymagania:

- Formularz: Amount, Client (Select), Description, Payment Method (Cash, Bank Transfer, Check).
- Submit wywołuje `useRecordManualPayment`.

**KROK 5: RefundDialog (Zadanie 6.2.4)**
Utwórz `src/features/payments/components/RefundDialog.tsx`.
Wymagania:

- Formularz: Amount (domyślnie pełna kwota), Reason (Select).
- Warning alert o nieodwracalności.
- Submit wywołuje `useRefundPayment`.

### OCZEKIWANY REZULTAT:

Bezpieczny interfejs zarządzania płatnościami, gotowy do integracji ze Stripe.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [x] PaymentMethodList wyświetla karty z API
- [x] AddPaymentMethod: ZERO inputów na dane karty!
- [x] ManualPaymentDialog wysyła POST do API
- [x] RefundDialog wysyła POST do API
- [x] Wszystkie mutacje mają loading states
- [x] Toast notifications po akcjach
- [x] Git commit: `feat(payments): implement payment methods and manual payments`

<!-- BLOCK_END: 6.2 -->

---

<!-- BLOCK_START: 7.1 -->

## 🔵 FAZA 7: Analytics Dashboard (Tydzień 8)

### 7.1 Analytics Dashboard

| #     | Zadanie                   | Priorytet  | Status | Opis                           |
| ----- | ------------------------- | ---------- | ------ | ------------------------------ |
| 7.1.1 | 🔴 Analytics page         | Krytyczne  | ✅     | /analytics - główny dashboard  |
| 7.1.2 | 🔴 RevenueChart component | Krytyczne  | ✅     | Wykres przychodów (recharts)   |
| 7.1.3 | 🔴 StatCards component    | Krytyczne  | ✅     | Karty MRR, ARR, Churn, Clients |
| 7.1.4 | 🔴 DateRangePicker        | Krytyczne  | ✅     | Wybór zakresu dat              |
| 7.1.5 | 🟡 ClientGrowthChart      | Ważne      | ✅     | Wykres wzrostu klientów        |
| 7.1.6 | 🟢 Export to CSV          | Opcjonalne | ✅     | Eksport danych do CSV          |

**Blok 7.1 - Wymagania wejściowe**: Faza 6 (Payments)  
**Blok 7.1 - Rezultat**: Dashboard analityczny z wykresami i KPI

**📦 DEPENDENCIES:**

- ✅ Blok 6.x (Payments)

**⬅️ BLOKUJE:**

- Blok 8.1 (Testing)

**🚨 API ENDPOINTS:**

```
GET /api/Analytics - zagregowane dane z parametrem dateRange
GET /api/Analytics/revenue - historia przychodów
GET /api/Analytics/clients - historia klientów
```

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer / Data Visualization Specialist.
Realizujemy Fazę 7: **Analytics Dashboard**.
To "heart of the SaaS" – miejsce, gdzie właściciel widzi, jak radzi sobie jego biznes.

### CEL GŁÓWNY:

Stworzyć dashboard analityczny z kartami KPI oraz interaktywnymi wykresami pokazującymi przychody i wzrost klientów w czasie.

### STRUKTURA PLIKÓW:

- Feature: `src/features/analytics/`
- Komponenty: `src/features/analytics/components/`
- Hooki: `src/features/analytics/hooks/`
- Strona: `src/app/(dashboard)/analytics/page.tsx`

### KROKI DO WYKONANIA:

**KROK 1: Analytics Hooks**
Utwórz `src/features/analytics/hooks/useAnalytics.ts`.
Wymagania:

- Przyjmuje parametr `dateRange` ({ from: Date, to: Date }).
- Importuje `useGetAnalytics` z API.
- Zwraca:
  - `stats`: { mrr, arr, activeSubscriptions, churnRate, totalRevenue }
  - `revenueHistory`: Tablica { date: string, amount: number }
  - `clientGrowth`: Tablica { date: string, totalClients: number, newClients: number }

**KROK 2: DateRangePicker (Zadanie 7.1.4)**
Utwórz `src/features/analytics/components/DateRangePicker.tsx`.
Wymagania:

- UI: `Popover` + `Calendar` (shadcn/ui).
- Szybkie presety: "Last 7 days", "Last 30 days", "This Month", "Last Month", "This Year".

**KROK 3: StatCards Component (Zadanie 7.1.3)**
Utwórz `src/features/analytics/components/StatCards.tsx`.
Wymagania:

- Grid 4 kart (Metric Cards).
- Każda karta: Title, Icon (lucide-react), Value (formatCurrency lub liczba), Trend (opcjonalnie: "+12% from last month").

**KROK 4: RevenueChart Component (Zadanie 7.1.2)**
Utwórz `src/features/analytics/components/RevenueChart.tsx`.
Wymagania:

- Użyj `recharts`: `<ResponsiveContainer>`, `<AreaChart>`, `<XAxis>`, `<YAxis>`, `<Tooltip>`, `<Area>`.
- Oś X: Daty (sformatowane krótko, np. "Jan 21").
- Oś Y: Kwota.
- Tooltip: formatCurrency.
- Styl: Gradient pod linią wykresu.

**KROK 5: ClientGrowthChart (Zadanie 7.1.5)**
Utwórz `src/features/analytics/components/ClientGrowthChart.tsx`.
Wymagania:

- Użyj `recharts`: `<BarChart>` (słupkowy).
- Pokazuje liczbę nowych klientów w danym okresie.

**KROK 6: Analytics Page (Zadanie 7.1.1, 7.1.6)**
Utwórz `src/app/(dashboard)/analytics/page.tsx`.
Wymagania:

- State: `dateRange` (domyślnie "Last 30 days").
- Header: Title "Analytics" + `DateRangePicker` + Button "Export Report" (generuje CSV).
- Content Layout:
  - Top: `StatCards`
  - Middle: Dwa wykresy obok siebie: `RevenueChart` i `ClientGrowthChart`
- Loading State: Szkielety kart i puste kontenery wykresów.

### WYMAGANIA TECHNICZNE:

- **Recharts**: Wykresy muszą być responsywne (width="100%" w ResponsiveContainer).
- **Formatowanie**: Waluty i daty spójne z resztą aplikacji.

### OCZEKIWANY REZULTAT:

Kod dla hooka analitycznego, komponentu wyboru daty, kart statystyk, dwóch typów wykresów oraz strony spinającej całość.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [x] useAnalytics hook importuje z @/core/api/generated
- [x] DateRangePicker działa z presetami
- [x] StatCards wyświetlają dane z API
- [x] RevenueChart: AreaChart z gradientem
- [x] ClientGrowthChart: BarChart
- [x] Wykresy responsywne (ResponsiveContainer)
- [x] Export CSV działa
- [x] Loading states z Skeleton
- [x] Network tab: GET /api/Analytics z Authorization header
- [x] Git commit: `feat(analytics): implement analytics dashboard with charts`

<!-- BLOCK_END: 7.1 -->

---

<!-- BLOCK_START: 8.1 -->

## 🔵 FAZA 8: Testing & Polish (Tydzień 9)

### 8.1 Testing

| #     | Zadanie             | Priorytet | Status | Opis                             |
| ----- | ------------------- | --------- | ------ | -------------------------------- |
| 8.1.1 | 🔴 Vitest setup     | Krytyczne | ✅     | Konfiguracja unit tests          |
| 8.1.2 | 🔴 Component tests  | Krytyczne | ✅     | Testy dla kluczowych komponentów |
| 8.1.3 | 🟡 Playwright setup | Ważne     | ✅     | Konfiguracja E2E tests           |
| 8.1.4 | 🟡 E2E auth flow    | Ważne     | ✅     | Test login/register flow         |
| 8.1.5 | 🟡 E2E client CRUD  | Ważne     | ✅     | Test tworzenia/edycji klienta    |

**Blok 8.1 - Wymagania wejściowe**: Wszystkie poprzednie fazy (Aplikacja funkcjonalna)  
**Blok 8.1 - Rezultat**: Skonfigurowane środowisko testowe i pokrycie krytycznych ścieżek

**📦 DEPENDENCIES:**

- ✅ Blok 7.1 (Analytics)

**⬅️ BLOKUJE:**

- Blok 8.2 (Polish & Optimization)

---

### 🤖 PROMPT

Działaj jako Senior QA Engineer / SDET (Software Development Engineer in Test).
Mamy gotowe MVP aplikacji SaaS (Next.js 15). Teraz musimy wdrożyć automatyczne testy.

Podzielimy prace na dwie warstwy: **Unit/Component Testing** (Vitest) oraz **End-to-End Testing** (Playwright).

### STRUKTURA PLIKÓW:

- Unit Tests: Obok plików źródłowych (np. `Sidebar.test.tsx`) lub w `src/__tests__/`
- E2E Tests: `e2e/`
- Config: `vitest.config.ts`, `playwright.config.ts`

### KROKI DO WYKONANIA:

**KROK 1: Vitest Setup (Zadanie 8.1.1)**

1. Zainstaluj: `vitest`, `@testing-library/react`, `@vitejs/plugin-react`, `jsdom`, `@testing-library/dom`.
2. Utwórz `vitest.config.ts`:
   - Środowisko `jsdom`.
   - Aliasy ścieżek (`@/*`) zgodne z `tsconfig.json`.
   - Setup files: `src/test/setup.ts` (importuj `@testing-library/jest-dom`).
3. Przygotuj helper `renderWithProviders` (w `src/test/utils.tsx`).

**KROK 2: Component Tests (Zadanie 8.1.2)**
Napisz przykładowe testy:

1. `src/shared/utils/formatters.test.ts`: Sprawdź `formatCurrency` i `formatDate`.
2. `src/features/payments/components/PaymentStatusBadge.test.tsx`: Sprawdź renderowanie dla różnych statusów.
3. `src/shared/components/layout/Sidebar.test.tsx`: Sprawdź renderowanie linków nawigacyjnych.

**KROK 3: Playwright Setup (Zadanie 8.1.3)**

1. Wygeneruj `playwright.config.ts`.
   - BaseURL: `http://localhost:3000`.
   - Trace: "on-first-retry".
2. Dodaj skrypt: `"test:e2e": "playwright test"`.

**KROK 4: E2E Critical Flows (Zadania 8.1.4, 8.1.5)**
Utwórz `e2e/core-flows.spec.ts`:

1. **Auth Flow**: Wejdź na `/login` → Wpisz dane → Kliknij "Sign In" → Oczekuj `/dashboard`.
2. **Client CRUD Flow**: Przejdź na `/clients/new` → Wypełnij formularz → Save → Sprawdź listę.

### WYMAGANIA TECHNICZNE:

- **Mocking Next.js**: Mockuj `useRouter`, `usePathname`, `useSearchParams` w testach Vitest.
- **Izolacja**: Testy E2E używają unikalnych nazw (np. `Client ${Date.now()}`).

### OCZEKIWANY REZULTAT:

Pliki konfiguracyjne, helper `renderWithProviders` oraz kod przykładowych testów.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [x] vitest.config.ts skonfigurowany
- [x] renderWithProviders helper działa
- [x] formatters.test.ts przechodzi
- [x] PaymentStatusBadge.test.tsx przechodzi
- [x] Sidebar.test.tsx przechodzi (z mockami)
- [x] playwright.config.ts skonfigurowany
- [x] e2e/core-flows.spec.ts: Auth flow przechodzi
- [x] e2e/core-flows.spec.ts: Client CRUD przechodzi
- [x] npm run test: zero błędów
- [x] npm run test:e2e: zero błędów
- [x] Git commit: `test(setup): configure Vitest and Playwright with initial tests`

<!-- BLOCK_END: 8.1 -->

---

<!-- BLOCK_START: 8.2 -->

### 8.2 Polish & Optimization

| #     | Zadanie                | Priorytet  | Status | Opis                                    |
| ----- | ---------------------- | ---------- | ------ | --------------------------------------- |
| 8.2.1 | 🔴 TypeScript audit    | Krytyczne  | ✅     | Weryfikacja brak any, pełne typy        |
| 8.2.2 | 🔴 Accessibility audit | Krytyczne  | ✅     | Keyboard nav, aria labels, focus states |
| 8.2.3 | 🟡 Performance audit   | Ważne      | ✅     | Lighthouse, bundle analysis             |
| 8.2.4 | 🟡 Mobile responsive   | Ważne      | ✅     | Testowanie na różnych rozdzielczościach |
| 8.2.5 | 🟢 Documentation       | Opcjonalne | ✅     | README, component docs                  |

**Blok 8.2 - Wymagania wejściowe**: Blok 8.1 (Testy)  
**Blok 8.2 - Rezultat**: Aplikacja gotowa do wdrożenia (Production Ready)

**📦 DEPENDENCIES:**

- ✅ Blok 8.1 (Testing)

**⬅️ BLOKUJE:**

- Blok 9.1 (Client Portal)

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer / Tech Lead.
Aplikacja jest funkcjonalna i przetestowana. Przechodzimy do Fazy 8.2: **Polish & Optimization**.
Naszym celem jest upewnienie się, że kod jest najwyższej jakości, aplikacja jest dostępna (A11y) i zoptymalizowana.

### CEL GŁÓWNY:

Przeprowadzić audyt kodu, skonfigurować narzędzia do analizy wydajności oraz przygotować dokumentację projektu.

### KROKI DO WYKONANIA:

**KROK 1: TypeScript & Linting Strictness (Zadanie 8.2.1)**

1. Sprawdź `tsconfig.json`: `noImplicitAny: true`.
2. Dodaj skrypt: `"type-check": "tsc --noEmit"`.
3. Skonfiguruj ESLint:
   - `@typescript-eslint/no-explicit-any`: "warn" (lub "error")
   - `@typescript-eslint/no-unused-vars`: "error"

**KROK 2: Accessibility (A11y) Setup (Zadanie 8.2.2)**

1. Zainstaluj `eslint-plugin-jsx-a11y`.
2. Dodaj do ESLint config.
3. Stwórz `A11Y_CHECKLIST.md`:
   - Czy wszystkie formularze mają etykiety (`label`) powiązane z inputami?
   - Czy można poruszać się po stronie używając tylko klawisza TAB?
   - Czy focus jest widoczny na elementach aktywnych?

**KROK 3: Performance Optimization (Zadanie 8.2.3)**

1. Zainstaluj `@next/bundle-analyzer`.
2. Skonfiguruj `next.config.ts` z warunkowym włączaniem analyzera.
3. Dodaj skrypt: `"analyze": "cross-env ANALYZE=true npm run build"`.

**KROK 4: Documentation (Zadanie 8.2.5)**
Napisz profesjonalny `README.md`:

- **Title & Badges** (Status, Tech Stack)
- **Prerequisites** (Node.js version, npm/pnpm)
- **Getting Started** (install, env setup, run dev)
- **Project Structure** (Features/Core/Shared)
- **Scripts** (dev, build, start, lint, test, api:generate)
- **Environment Variables** (lista kluczy z opisem)

### WYMAGANIA TECHNICZNE:

- **Next.js Config**: Bundle analyzer warunkowo (zmienną środowiskową).
- **ESLint**: Kompatybilny z "Flat Config" (ESLint 2024/2025 standard).

### OCZEKIWANY REZULTAT:

Zaktualizowane pliki konfiguracyjne, checklista dostępności oraz gotowy README.md.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [x] npm run type-check: zero błędów
- [x] npm run lint: zero warnings (lub tylko akceptowalne)
- [x] eslint-plugin-jsx-a11y skonfigurowany
- [x] A11Y_CHECKLIST.md utworzony
- [x] Bundle analyzer działa z ANALYZE=true
- [x] README.md kompletny i profesjonalny
- [x] Wszystkie strony responsywne na mobile
- [x] Lighthouse score > 80 dla Performance
- [x] Git commit: `chore(polish): typescript audit, a11y setup, documentation`

<!-- BLOCK_END: 8.2 -->

---

<!-- BLOCK_START: 9.1 -->

## 🔵 FAZA 9: Client Portal (Tydzień 10)

### 9.1 Client Portal Foundation

| #     | Zadanie             | Priorytet | Status | Opis                                       |
| ----- | ------------------- | --------- | ------ | ------------------------------------------ |
| 9.1.1 | 🔴 Portal Layout    | Krytyczne | ✅     | Osobny layout dla /portal (bez sidebara)   |
| 9.1.2 | 🔴 Portal Guard     | Krytyczne | ✅     | Ochrona tras tylko dla roli 'Client'       |
| 9.1.3 | 🔴 Portal Dashboard | Krytyczne | ✅     | /portal - podsumowanie subskrypcji         |
| 9.1.4 | 🟡 Invoices list    | Ważne     | ✅     | Lista faktur do pobrania (PDF)             |
| 9.1.5 | 🟡 Billing Settings | Ważne     | ⬜     | Zarządzanie kartą i anulowanie (poza scope)|

**Blok 9.1 - Wymagania wejściowe**: Faza 1 (Auth), Faza 5 (Subskrypcje)  
**Blok 9.1 - Rezultat**: Działający portal samoobsługowy dla końcowego klienta

**📦 DEPENDENCIES:**

- ✅ Blok 1.x (Auth)
- ✅ Blok 5.x (Subscriptions)
- ✅ Blok 8.2 (Polish)

**⬅️ BLOKUJE:**

- Nic (ostatni blok)

**🚨 API ENDPOINTS:**

```
GET /api/Portal/subscriptions - subskrypcje zalogowanego klienta
GET /api/Portal/invoices - faktury zalogowanego klienta
POST /api/Portal/cancel-subscription - anulowanie subskrypcji
PUT /api/Portal/payment-method - aktualizacja metody płatności
```

---

### 🤖 PROMPT

Działaj jako Senior Frontend Developer / System Architect.
Realizujemy Fazę 9: **Client Portal**.
Jest to osobna część aplikacji przeznaczona dla klientów końcowych (nie dla Providerów). Musi być prosta, przejrzysta i skupiona na samoobsłudze płatności.

### CEL GŁÓWNY:

Stworzyć wydzieloną strefę `/portal` z własnym layoutem, dostępną tylko dla użytkowników o roli `Client`. Użytkownik ma tam widzieć swoje aktywne subskrypcje, historię faktur i móc zarządzać metodami płatności.

### ARCHITEKTURA I LOKALIZACJA PLIKÓW:

Używamy **Route Groups** do separacji layoutów:

- Routing: `src/app/(portal)/layout.tsx`, `src/app/(portal)/portal/page.tsx`
- Feature: `src/features/client-portal/` (nowy vertical slice)
- Komponenty: `src/features/client-portal/components/`
- Hooki: `src/features/client-portal/hooks/`

### KROKI DO WYKONANIA:

**KROK 1: Portal Layout & Guard (Zadania 9.1.1, 9.1.2)**

1. Utwórz `src/features/client-portal/components/PortalGuard.tsx`:
   - Działa analogicznie do `TenantGuard`, ale wymaga roli `Client`.
   - Jeśli user ma rolę `Provider` -> przekieruj na `/dashboard`.
   - Jeśli user nie jest zalogowany -> `/login`.
2. Utwórz `src/app/(portal)/layout.tsx`:
   - Prosty layout: Navbar na górze (Logo + UserMenu), wycentrowana zawartość (max-w-4xl).
   - Brak bocznego Sidebara.
   - Owiń `children` w `PortalGuard`.

**KROK 2: Portal Hooks (Zadanie 9.1.3)**
Utwórz `src/features/client-portal/hooks/usePortal.ts`.
Wymagania:

- `useMySubscriptions()`: Pobiera subskrypcje zalogowanego klienta.
- `useMyInvoices()`: Pobiera historię faktur.
- `usePortalAction()`: Wrapper na mutacje (cancelSubscription, updatePaymentMethod).

**KROK 3: Client Dashboard (Zadanie 9.1.3)**
Utwórz `src/app/(portal)/portal/page.tsx`.
Wymagania:

- **Sekcja "Current Plan"**: Duża karta z aktywną subskrypcją (Plan Name, Price, Renewal Date, Status Badge).
- **Action Buttons**:
  - "Update Payment Method" (otwiera dialog).
  - "Cancel Subscription" (otwiera dialog potwierdzenia).

**KROK 4: Invoices List (Zadanie 9.1.4)**
Utwórz `src/features/client-portal/components/ClientInvoicesList.tsx`.
Wymagania:

- Prosta tabela lub lista: Data, Kwota, Status, "Download PDF" (ikona).
- Dodaj komponent na dole Dashboardu.

### WYMAGANIA TECHNICZNE:

- **Next.js 15 Async Params**: Propsy `params` i `searchParams` są asynchroniczne (Promise).
- **Reużywalność**: Importuj komponenty UI (Button, Card, Badge) z `shared/ui`.

### OCZEKIWANY REZULTAT:

Kod layoutu portalu, Guarda, hooków oraz strony głównej portalu z listą faktur.

---

### ✅ CHECKLIST WERYFIKACJI (przed oznaczeniem jako DONE):

- [x] PortalGuard chroni /portal/\* dla roli Client
- [x] Provider przekierowany na /dashboard
- [x] Portal layout bez sidebara, prosty navbar
- [x] useMySubscriptions pobiera dane z API
- [x] useMyInvoices pobiera dane z API
- [x] Current Plan card wyświetla dane z API
- [x] Invoices list z akcją Download PDF
- [ ] Cancel Subscription dialog działa (scope poza blokiem 9.1)
- [ ] Update Payment Method dialog działa (scope poza blokiem 9.1)
- [x] Mobile responsive
- [x] Network tab: requesty z Authorization header
- [x] Git commit: `feat(portal): implement client portal with subscriptions and invoices`

<!-- BLOCK_END: 9.1 -->

---

## 📊 PODSUMOWANIE BLOKÓW (ZAKTUALIZOWANE)

> **🚀 AKTUALIZACJA** - Pełna struktura zgodna z feature_list.json

| Blok | Faza    | Nazwa                        | Status | Dependencies  |
| ---- | ------- | ---------------------------- | ------ | ------------- |
| 0.1  | FAZA 0  | Inicjalizacja Projektu       | ✅     | -             |
| 0.2  | FAZA 0  | Stylowanie i UI Kit          | ✅     | 0.1           |
| 0.3  | FAZA 0  | API Layer Setup              | ✅     | 0.1           |
| 1.1  | FAZA 1  | NextAuth Configuration       | ✅     | 0.x           |
| 1.2  | FAZA 1  | Tenant Context               | ✅     | 1.1           |
| 1.3  | FAZA 1  | Auth Store & Middleware      | ✅     | 1.1           |
| 1.4  | FAZA 1  | Auth UI Pages                | ✅     | 1.1, 1.3      |
| 2.1  | FAZA 2  | Layout Components            | ✅     | 1.x           |
| 2.2  | FAZA 2  | Global State & Feedback      | ✅     | 2.1           |
| 3.1  | FAZA 3  | Team List & CRUD             | ✅     | 2.x           |
| 3.2  | FAZA 3  | Invitations                  | ⬜     | 3.1           |
| 4A.1 | FAZA 4A | Clients List                 | ⬜     | 3.x           |
| 4A.2 | FAZA 4A | Clients Search & Filters     | ⬜     | 4A.1          |
| 4A.3 | FAZA 4A | Client CRUD                  | ⬜     | 4A.1          |
| 4B.1 | FAZA 4B | Plans List                   | ⬜     | 3.x           |
| 4B.2 | FAZA 4B | Plan CRUD                    | ⬜     | 4B.1          |
| 5.1  | FAZA 5  | **Subscriptions List**       | ⬜     | 4A.x, 4B.x    |
| 5.2  | FAZA 5  | **Subscription Actions**     | ⬜     | 5.1           |
| 6.1  | FAZA 6  | **Payment History**          | ⬜     | 5.x           |
| 6.2  | FAZA 6  | **Payment Methods**          | ⬜     | 6.1           |
| 7.1  | FAZA 7  | **Analytics Dashboard**      | ⬜     | 6.x           |
| 8.1  | FAZA 8  | **Testing**                  | ⬜     | 7.1           |
| 8.2  | FAZA 8  | **Polish & Optimization**    | ⬜     | 8.1           |
| 9.1  | FAZA 9  | **Client Portal Foundation** | ⬜     | 1.1, 5.x, 8.2 |

---

_Ostatnia aktualizacja: 2025-12-19_  
_Wersja: 8.0 (pełna wersja ze wszystkimi blokami)_

---

## 📋 UNIVERSAL API VERIFICATION CHECKLIST

> **Użyj tej checklisty dla WSZYSTKICH bloków które zawierają `apiEndpoints` lub `requiredHooks` w feature_list.json**

### 🚨 PRZED ustawieniem `"passes": true`:

#### 1. Code Review

- [ ] Komponent importuje hooki z `@/core/api/generated/` (NIE mock data)
- [ ] ZERO hardcoded wartości: `0`, `"$0"`, `[]`, `"placeholder"`, `{ test: true }`
- [ ] ZERO komentarzy TODO dotyczących API
- [ ] Loading states używają `<Skeleton>` z shadcn/ui
- [ ] Error handling wyświetla `error.message`
- [ ] Wszystkie hooki z `requiredHooks` są zaimplementowane

#### 2. DevTools Network Verification (KRYTYCZNE!)

**To jest NAJWAŻNIEJSZY krok - bez tego wprowadzasz bugi!**

```bash
# 1. Uruchom aplikację
npm run dev

# 2. Otwórz przeglądarkę: http://localhost:3000
# 3. Otwórz DevTools → Network tab
# 4. Zaloguj się do aplikacji
# 5. Nawiguj do strony z funkcjonalnością z tego bloku
```

**Sprawdź dla KAŻDEGO endpointu z `apiEndpoints`:**

✅ **MUSI BYĆ:**

```
Request URL: http://localhost:5211/api/[endpoint]
Request Method: GET/POST/PUT/DELETE
Request Headers:
  Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Status: 200 OK
Response: { ... } (prawdziwe dane z backendu)
```

❌ **FAIL jeśli:**

- Status: `401 Unauthorized` → Brak tokena JWT lub auth interceptor nie działa!
- Brak requestów do `/api/*` → Komponent nie wywołuje API!
- Response: `null` lub `undefined` → Endpoint nie istnieje!
- Hardcoded dane w UI → Nie używasz `data` z hooka!

#### 3. TypeScript & Linting

```bash
npm run typecheck  # MUSI: zero błędów
npm run lint       # MUSI: zero warnings
```

#### 4. Visual Testing

- [ ] Loading spinner pojawia się przy pierwszym ładowaniu
- [ ] Dane wyświetlają się po załadowaniu (NIE "0" ani puste listy)
- [ ] Error state działa (wyłącz backend na chwilę i sprawdź)
- [ ] Formatowanie danych poprawne:
  - [ ] Daty: DD.MM.YYYY (użyj `formatDate()`)
  - [ ] Kwoty: PLN 1 234,56 (użyj `formatCurrency()`)
  - [ ] Statusy: Badge z odpowiednim wariantem

#### 5. Git Commit

```bash
git add .
git commit -m "feat(scope): description

- Implementacja [lista funkcjonalności]
- Wszystkie API calls używają wygenerowanych hooków
- Loading states i error handling
- TypeScript strict: zero błędów

🤖 Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>"
```

---

**Remember**: Jedna minuta weryfikacji Network tab = uniknięcie godzin debugowania później!
