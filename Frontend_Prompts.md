## 🔵 FAZA 0: Setup & Configuration (Tydzień 1)

### 0.1 Inicjalizacja Projektu

| #     | Zadanie                              | Priorytet | Status | Opis                                                                |
| ----- | ------------------------------------ | --------- | ------ | ------------------------------------------------------------------- |
| 0.1.1 | 🔴 Utworzenie projektu Next.js 15    | Krytyczne | ⬜     | `create-next-app` z TypeScript, Tailwind, App Router, src directory |
| 0.1.2 | 🔴 Konfiguracja tsconfig.json strict | Krytyczne | ⬜     | strict: true, allowJs: false, wszystkie strict\* opcje              |
| 0.1.3 | 🔴 Struktura katalogów               | Krytyczne | ⬜     | Utworzenie features/, shared/, core/ zgodnie z planem               |

**Blok 0.1 - Wymagania wejściowe**: Brak  
**Blok 0.1 - Rezultat**: Działający projekt Next.js z TypeScript strict

-------------------------------------------------------------------> PROMPT <---------------------------------------------------------------

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

KROK 1: Inicjalizacja Projektu (Zadanie 0.1.1)
Wygeneruj komendę (lub wykonaj ją, jeśli masz dostęp do terminala) `create-next-app` z następującymi flagami, aby uniknąć interaktywnych pytań:
--typescript
--tailwind
--eslint
--app
--src-dir
--import-alias "@/\*"
--use-npm (lub --use-pnpm/--use-yarn zależnie od preferencji)

KROK 2: Konfiguracja TypeScript (Zadanie 0.1.2)
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

KROK 3: Struktura Katalogów (Zadanie 0.1.3)
Wewnątrz katalogu `src/` usuń wszelki boilerplate (oprócz layout.tsx i page.tsx - wyczyść ich zawartość do minimum) i utwórz następującą strukturę katalogów:

- src/features/ (dla modułów funkcjonalnych)
- src/shared/ (dla komponentów UI, utils, hooks współdzielonych)
- src/core/ (dla konfiguracji, typów globalnych, stałych)

### OCZEKIWANY REZULTAT:

## Gotowy do uruchomienia projekt, który kompiluje się bez błędów, posiada pustą strukturę folderów zgodnie z architekturą features/shared/core oraz restrykcyjny config TS.

=============================================================================================================================================

### 0.2 Stylowanie i UI Kit

| #     | Zadanie                        | Priorytet | Status | Opis                                                                      |
| ----- | ------------------------------ | --------- | ------ | ------------------------------------------------------------------------- |
| 0.2.1 | 🔴 Konfiguracja Tailwind CSS   | Krytyczne | ⬜     | tailwind.config.ts z custom colors, fonts                                 |
| 0.2.2 | 🔴 Inicjalizacja shadcn/ui     | Krytyczne | ⬜     | `npx shadcn@latest init`, konfiguracja components.json                    |
| 0.2.3 | 🔴 Import bazowych komponentów | Krytyczne | ⬜     | Button, Input, Card, Dialog, DropdownMenu, Select, Badge, Skeleton, Toast |
| 0.2.4 | 🟡 Utility functions           | Ważne     | ⬜     | cn() helper, formatters (currency, date)                                  |

**Blok 0.2 - Wymagania wejściowe**: Blok 0.1  
**Blok 0.2 - Rezultat**: Gotowy UI kit z shadcn/ui
-------------------------------------------------------------------> PROMPT <---------------------------------------------------------------

Działaj jako Senior Frontend Developer.
Kontynuujemy pracę nad projektem Next.js 15. Twoim zadaniem jest wykonanie Fazy 0.2 (Stylowanie i UI Kit) zgodnie z planem.

### CEL GŁÓWNY:

Skonfigurować Tailwind CSS, zainicjować bibliotekę shadcn/ui dostosowaną do naszej struktury katalogów oraz dodać niezbędne funkcje pomocnicze.

### KONTEKST STRUKTURY (z Fazy 0.1):

Mamy strukturę: `src/features`, `src/shared`, `src/core`.
Chcemy, aby reużywalne komponenty UI (shadcn) trafiały do: `src/shared/ui`.
Chcemy, aby funkcje pomocnicze (utils/lib) trafiały do: `src/shared/lib`.

### KROKI DO WYKONANIA:

KROK 1: Inicjalizacja shadcn/ui (Zadanie 0.2.2)
Uruchom (lub zasymuluj konfigurację) `npx shadcn@latest init`.
Skonfiguruj plik `components.json` tak, aby odzwierciedlał poniższe ustawienia (nadpisz domyślne ścieżki):

- Style: Default
- Base Color: Slate
- CSS Variables: Yes
- Aliases -> components: "@/shared/ui"
- Aliases -> utils: "@/shared/lib/utils"
- Aliases -> ui: "@/shared/ui" (jeśli dostępne)
  _Upewnij się, że plik `globals.css` znajduje się w `src/app/globals.css` i zostanie zaktualizowany o zmienne CSS._

KROK 2: Instalacja komponentów bazowych (Zadanie 0.2.3)
Zainstaluj następujące komponenty za pomocą CLI shadcn:
`button`, `input`, `card`, `dialog`, `dropdown-menu`, `select`, `badge`, `skeleton`, `toast` (sonner lub standardowy toast).

KROK 3: Weryfikacja i Konfiguracja Tailwind (Zadanie 0.2.1)
Sprawdź plik `tailwind.config.ts`.

1. Upewnij się, że `content` obejmuje wszystkie nasze katalogi:
   - "./src/pages/\*_/_.{js,ts,jsx,tsx,mdx}"
   - "./src/components/\*_/_.{js,ts,jsx,tsx,mdx}"
   - "./src/app/\*_/_.{js,ts,jsx,tsx,mdx}"
   - "./src/features/\*_/_.{js,ts,jsx,tsx,mdx}" (BARDZO WAŻNE - dodaj to)
   - "./src/shared/\*_/_.{js,ts,jsx,tsx,mdx}" (BARDZO WAŻNE - dodaj to)
2. Jeśli shadcn nie dodał pluginu `tailwindcss-animate`, dodaj go.

KROK 4: Utility Functions (Zadanie 0.2.4)

1. Upewnij się, że funkcja `cn` (classNames) została wygenerowana w `src/shared/lib/utils.ts`.
2. W tym samym pliku (lub nowym `src/shared/lib/formatters.ts`) stwórz dwie funkcje:
   - `formatCurrency(amount: number, currency: string = 'PLN'): string` – formatująca walutę (użyj Intl.NumberFormat).
   - `formatDate(date: string | Date): string` – formatująca datę na polski format (np. 'DD.MM.YYYY').

### OCZEKIWANY REZULTAT:

Projekt z zainstalowanym shadcn/ui, gdzie komponenty (np. Button) znajdują się w `src/shared/ui/button.tsx`, a Tailwind poprawnie wykrywa klasy w folderach `features` i `shared`.

=============================================================================================================================================

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

-------------------------------------------------------------------> PROMPT <---------------------------------------------------------------
Działaj jako Senior Frontend Developer / API Architect.
Przechodzimy do Fazy 0.3 (API Layer Setup). Twoim celem jest zautomatyzowanie generowania warstwy API na podstawie definicji Swagger/OpenAPI.

### CEL GŁÓWNY:

Skonfigurować bibliotekę **Orval** do generowania typów i hooków React Query, oraz skonfigurować instancję **Axios** tak, aby automatycznie obsługiwała "rozpakowywanie" obiektów typu `Result<T>` zwracanych przez backend.

### KROKI DO WYKONANIA:

KROK 1: Instalacja Zależności (Zadanie 0.3.1)
Zainstaluj wymagane pakiety:

- `npm install axios @tanstack/react-query`
- `npm install -D orval`

KROK 2: Setup Klienta Axios - Custom Mutator (Zadanie 0.3.3 i 0.3.4)
Utwórz plik `src/core/api/client.ts`. To będzie nasz "Custom Mutator".
Wymagania:

1. Utworzenie instancji Axios z `baseURL` pobieranym z env.
2. **Krytyczne: Response Interceptor**. Backend zwraca `Result<T>`. Interceptor ma zwracać `response.data.data` w przypadku sukcesu lub rzucać błąd domenowy w przypadku porażki.
3. **Sygnatura funkcji**: Wyeksportuj funkcję `customInstance` (lub `apiClient`), która musi pasować do tego, jak Orval generuje kod.
   Zazwyczaj powinna wyglądać tak:
   `export const customInstance = <T>(config: AxiosRequestConfig, options?: AxiosRequestConfig): Promise<T> => { ... }`
   Upewnij się, że jest to funkcja generyczna zwracająca Promise.

KROK 3: Konfiguracja Orval (Zadanie 0.3.2)
Utwórz plik `orval.config.ts`. Skonfiguruj go:

- Input: URL do Swaggera.
- Output: `src/core/api/generated`, Client: `react-query`.
- **Override -> Mutator**: Wskaż na plik `src/core/api/client.ts` i funkcję `customInstance`.

KROK 4: Skrypty i Generowanie (Zadanie 0.3.5)
Dodaj skrypt `"api:generate": "orval"` i spróbuj uruchomić generowanie (nawet na mockowym URL, byle sprawdzić config).

### OCZEKIWANY REZULTAT:

Gotowa infrastruktura API, która poprawnie kompiluje się z wygenerowanym kodem Orvala.

=============================================================================================================================================

## 🔵 FAZA 1: Auth + Tenant Context (Tydzień 2-3)

### 1.1 NextAuth Configuration

| #     | Zadanie                         | Priorytet | Status | Opis                                                 |
| ----- | ------------------------------- | --------- | ------ | ---------------------------------------------------- |
| 1.1.1 | 🔴 Instalacja NextAuth v5       | Krytyczne | ⬜     | `npm install next-auth@beta`                         |
| 1.1.2 | 🔴 auth.config.ts               | Krytyczne | ⬜     | Credentials provider, JWT callback, session callback |
| 1.1.3 | 🔴 API route [...nextauth]      | Krytyczne | ⬜     | src/app/api/auth/[...nextauth]/route.ts              |
| 1.1.4 | 🔴 Rozszerzenie typów next-auth | Krytyczne | ⬜     | next-auth.d.ts z tenantId, teamRole, teamMemberId    |
| 1.1.5 | 🔴 AuthProvider wrapper         | Krytyczne | ⬜     | SessionProvider w root layout                        |

**Blok 1.1 - Wymagania wejściowe**: Faza 0  
**Blok 1.1 - Rezultat**: Działająca konfiguracja NextAuth
-------------------------------------------------------------------> PROMPT <---------------------------------------------------------------

Rola: Senior Frontend Developer (Next.js 15, TypeScript Strict) Projekt: Orbito Platform Frontend (v6.0) Kontekst: Faza 0 (Setup) została zakończona. Przechodzimy do Fazy 1.1. Cel: Implementacja konfiguracji NextAuth v5 oraz podstawowej infrastruktury autentykacji.

Twoim zadaniem jest wykonanie Bloku 1.1 z planu implementacji. Nie twórz jeszcze stron logowania (UI) ani middleware – skup się wyłącznie na konfiguracji, typach i API route.

📋 Zakres zadania (Blok 1.1)
Zrealizuj następujące punkty zgodnie z "Vertical Slices" i architekturą opisaną w dokumentacji:
Instalacja zależności: npm install next-auth@beta (v5).
Definicja Typów (Crucial): Rozszerzenie typów NextAuth o pola związane z Tenant Context (tenantId, teamRole, teamMemberId).
Konfiguracja Auth: Utworzenie pliku konfiguracyjnego z CredentialsProvider.
API Route: Utworzenie handlerów dla App Router.
Provider: Utworzenie AuthProvider (wrapper dla SessionProvider).

🏗️ Wymagania Techniczne i Struktura Plików
Wszystkie pliki muszą być zgodne z tsconfig.json (strict: true). Używaj poniższych ścieżek:

1. Typy Globalne
   Plik: src/types/next-auth.d.ts Wymagane rozszerzenie interfejsów User, Session i JWT. Muszą zawierać:
   id: string
   email: string
   role: "PlatformAdmin" | "Provider" | "Client" | "TeamMember"
   tenantId: string
   teamRole?: "Owner" | "Admin" | "Member"
   teamMemberId?: string
   accessToken: string

2. Konfiguracja Auth
   Plik: src/core/auth/auth.config.ts
   Skonfiguruj NextAuth używając CredentialsProvider.
   W funkcji authorize: na razie zamockuj zwracanie użytkownika (później podepniemy tu API z orval). Zwróć obiekt zgodny z nowym typem User (np. hardcoded success).
   Callbacki:
   jwt: Musi przepisywać dane z user do tokenu.
   session: Musi przepisywać dane z token do sesji (aby były dostępne w useSession).

3. API Route Handler
   Plik: src/app/api/auth/[...nextauth]/route.ts
   Eksportuj GET i POST używając handlerów z NextAuth.

4. Auth Provider
   Plik: src/core/providers/AuthProvider.tsx
   Komponent "use client".
   Wrapper na SessionProvider z next-auth/react.

5. Integracja z Root Layout
   Plik: src/app/layout.tsx
   Dodaj AuthProvider do głównego layoutu aplikacji.
   📝 Wytyczne Implementacyjne (Definition of Done)
   TypeScript Strict: Kod nie może zawierać any. Każdy typ musi być ściśle zdefiniowany.
   NextAuth v5: Używamy najnowszej wersji beta (zgodnej z Next.js 15). Pamiętaj, że konfiguracja w v5 często rozdzielana jest na auth.config.ts (niezależne od Node.js) i auth.ts, ale w tym kroku stwórz główną konfigurację, która zadziała w środowisku Node (API Routes).
   Bez UI: Nie twórz formularzy logowania w tym kroku.
   Mock Data: W authorize użyj prostego if-a sprawdzającego np. email admin@orbito.com / pass password, który zwróci pełny obiekt użytkownika z tenantId i rolami, abyśmy mogli testować kontekst w następnych krokach.

💻 Komendy do wykonania na start
Bash

npm install next-auth@beta

Oczekiwany rezultat
Po wykonaniu zadania aplikacja powinna się budować bez błędów, a endpoint /api/auth/session powinien być dostępny (choć zwracać null/empty przed zalogowaniem).

# Proszę o wygenerowanie kodu dla wyżej wymienionych plików.

=============================================================================================================================================

### 1.2 Tenant Context

| #     | Zadanie                  | Priorytet | Status | Opis                                              |
| ----- | ------------------------ | --------- | ------ | ------------------------------------------------- |
| 1.2.1 | 🔴 TenantProvider        | Krytyczne | ⬜     | Context z tenantId, teamRole, hasAccess()         |
| 1.2.2 | 🔴 useTenant hook        | Krytyczne | ⬜     | Custom hook do pobierania kontekstu               |
| 1.2.3 | 🔴 TenantGuard component | Krytyczne | ⬜     | Wrapper sprawdzający uprawnienia                  |
| 1.2.4 | 🟡 TenantSwitcher        | Ważne     | ⬜     | UI do przełączania tenantów (jeśli user ma wiele) |

**Blok 1.2 - Wymagania wejściowe**: Blok 1.1  
**Blok 1.2 - Rezultat**: Działający kontekst tenanta
-------------------------------------------------------------------> PROMPT <---------------------------------------------------------------

Działaj jako Senior Frontend Developer / System Architect. Zakończyliśmy Fazę 1.1 (NextAuth Configuration). Teraz przechodzimy do Fazy 1.2: Implementacja Tenant Context.

Jest to fundament architektury Multi-tenancy w naszej aplikacji SaaS. Wszystkie operacje w systemie (poza Auth) będą wymagały tego kontekstu.

CEL GŁÓWNY:
Stworzyć mechanizm React Context (TenantProvider), który na podstawie sesji zalogowanego użytkownika udostępnia w całej aplikacji informacje o aktualnym Tenancie i Roli użytkownika, oraz dostarcza mechanizmy kontroli dostępu (Guard).

ARCHITEKTURA I ŚCIEŻKI:
Zgodnie z zasadą "Vertical Slices", wszystkie pliki związane z tą domeną mają trafić do: src/features/tenant/

KROKI DO WYKONANIA:
KROK 1: TenantProvider i useTenant (Zadania 1.2.1, 1.2.2) Utwórz plik src/features/tenant/providers/TenantProvider.tsx. Wymagania:

Musi być komponentem "use client".
Musi korzystać z useSession z next-auth/react.

Zdefiniuj interfejs TenantContext zawierający:
tenantId: string | null
teamRole: "Owner" | "Admin" | "Member" | null (typ zaciągnij z typów globalnych jeśli możliwe lub zdefiniuj)
teamMemberId: string | null
isLoading: boolean
hasAccess: (requiredRoles: TeamRole[]) => boolean

Zaimplementuj TenantProvider uzupełniający te dane na podstawie obiektu session.user.
Wyeksportuj custom hook useTenant(), który rzuca błąd, jeśli zostanie użyty poza Providerem.

KROK 2: TenantGuard Component (Zadanie 1.2.3) Utwórz plik src/features/tenant/components/TenantGuard.tsx. Jest to wrapper (HOC/Wrapper Component) służący do ochrony widoków. Wymagania:

Propsy: children, requiredRoles? (tablica ról), fallback? (opcjonalny komponent renderowany przy braku dostępu).
Logika:

Jeśli isLoading -> return null (lub loader).
Jeśli brak tenantId -> przekieruj do /login (użyj useRouter).
Jeśli podano requiredRoles i użytkownik nie ma roli -> zwróć fallback (np. "Access Denied") lub przekieruj.
W przeciwnym razie -> render children.

KROK 3: TenantSwitcher UI (Zadanie 1.2.4) Utwórz plik src/features/tenant/components/TenantSwitcher.tsx. Na ten moment backend może nie obsługiwać wielu tenantów dla jednego usera, więc zrób wersję UI-first. Wymagania:

Użyj komponentów shadcn/ui (DropdownMenu, Button, Avatar).
Wyświetl aktualną nazwę tenanta/zespołu (możesz pobrać z sesji lub zamockować jeśli brak w typach).
Lista powinna zawierać opcję "Create Team" oraz listę dostępnych zespołów.

KROK 4: Integracja Globalna Poinstruuj mnie, gdzie w pliku src/app/(dashboard)/layout.tsx (lub src/app/layout.tsx) należy dodać <TenantProvider>, aby kontekst był dostępny dla wszystkich chronionych tras.

WYMAGANIA TECHNICZNE:
Strict Type Safety: Żadnych any. Jeśli typy sesji w next-auth nie są widoczne, załóż, że next-auth.d.ts z Fazy 1.1 jest poprawny i zaimportuj odpowiednie typy.

Fail Fast: useTenant musi rzucać error, gdy context jest undefined.
Optymalizacja: Użyj useMemo dla wartości kontekstu.

OCZEKIWANY REZULTAT:
Gotowe pliki w katalogu src/features/tenant/ oraz instrukcja, jak owinąć aplikację w TenantProvider.

=============================================================================================================================================

### 1.3 Auth Store i Middleware

| #     | Zadanie                | Priorytet | Status | Opis                                                   |
| ----- | ---------------------- | --------- | ------ | ------------------------------------------------------ |
| 1.3.1 | 🔴 authStore (Zustand) | Krytyczne | ⬜     | Store z user, isAuthenticated, login/logout actions    |
| 1.3.2 | 🔴 middleware.ts       | Krytyczne | ⬜     | Protected routes, redirect to login, tenant validation |
| 1.3.3 | 🟡 Auth sync           | Ważne     | ⬜     | Synchronizacja NextAuth session z Zustand store        |

**Blok 1.3 - Wymagania wejściowe**: Blok 1.1 (NextAuth), Blok 1.2 (Tenant)
**Blok 1.3 - Rezultat**: Pełna ochrona tras i zsynchronizowany stan klienta

-------------------------------------------------------------------> PROMPT <---------------------------------------------------------------
Działaj jako Senior Frontend Developer / Security Specialist.
Realizujemy Fazę 1.3: **Auth Store & Middleware**.

### CEL GŁÓWNY:

1. Wdrożyć `middleware.ts` chroniący trasy dashboardu, ale przepuszczający pliki statyczne.
2. Stworzyć `authStore` (Zustand).
3. Zsynchronizować sesję NextAuth z Zustandem.

### KROKI DO WYKONANIA:

KROK 1: Auth Store (Zadanie 1.3.1)
Utwórz `src/features/auth/stores/authStore.ts` używając Zustand v5.
Stan: `user`, `isAuthenticated`, `isLoading`. Typowanie strict.

KROK 2: Middleware (Zadanie 1.3.2)
Utwórz plik `src/middleware.ts`.
Wymagania:

- Zaimportuj `authConfig`.
- Logika: Przekieruj niezalogowanych z tras chronionych na `/login`, a zalogowanych z `/login` na `/dashboard`.
- **Wydajność (Matcher)**: Zamiast sprawdzać każdą ścieżkę w `if-ach`, zdefiniuj `config.matcher`, aby wykluczyć pliki statyczne i API Next.js.
  Wzór matchera: `['/((?!api|_next/static|_next/image|favicon.ico).*)']`. Dzięki temu middleware nie uruchomi się dla obrazków i zasobów, co przyspieszy aplikację.

KROK 3: Synchronizacja Stanu (Zadanie 1.3.3)
Zmodyfikuj `src/core/providers/AuthProvider.tsx`.
Użyj `useEffect` do aktualizacji `authStore` na podstawie sesji z `useSession`.
Ważne: Nie renderuj `children`, dopóki status sesji to "loading", aby uniknąć efektu migania (Flash of Unauthenticated Content).

### OCZEKIWANY REZULTAT:

Bezpieczny middleware, który nie blokuje zasobów statycznych, oraz store zsynchronizowany z sesją.

=========================================================================================================================================

### 1.4 Auth UI Pages

| #     | Zadanie                   | Priorytet | Status | Opis                                           |
| ----- | ------------------------- | --------- | ------ | ---------------------------------------------- |
| 1.4.1 | 🔴 Login page             | Krytyczne | ⬜     | /login - formularz z walidacją Zod             |
| 1.4.2 | 🔴 LoginForm component    | Krytyczne | ⬜     | React Hook Form, error handling, loading state |
| 1.4.3 | 🔴 Register page          | Krytyczne | ⬜     | /register - rejestracja providera              |
| 1.4.4 | 🔴 RegisterForm component | Krytyczne | ⬜     | Formularz z walidacją, role selection          |
| 1.4.5 | 🟡 Auth error page        | Ważne     | ⬜     | /auth/error - obsługa błędów auth              |

**Blok 1.4 - Wymagania wejściowe**: Blok 1.1 (NextAuth), Blok 1.3 (Store/Middleware)
**Blok 1.4 - Rezultat**: Działające, ostylowane strony logowania i rejestracji

-------------------------------------------------------------------> PROMPT <---------------------------------------------------------------

Działaj jako Senior Frontend Developer / UX Designer.
Mamy już działającą konfigurację NextAuth (backend) oraz AuthStore.
Przechodzimy do Fazy 1.4: **Implementacja UI logowania i rejestracji**.

### CEL GŁÓWNY:

Stworzyć estetyczne, w pełni funkcjonalne strony `/login` i `/register` przy użyciu komponentów `shadcn/ui`, biblioteki `react-hook-form` oraz walidacji `zod`.

### STRUKTURA KATALOGÓW:

- Komponenty formularzy: `src/features/auth/components/`
- Schematy walidacji: `src/features/auth/schemas.ts`
- Strony (Routing):
  - `src/app/(auth)/login/page.tsx`
  - `src/app/(auth)/register/page.tsx`
  - `src/app/(auth)/auth/error/page.tsx`
- Layout dla Auth: `src/app/(auth)/layout.tsx` (dla wycentrowania contentu)

### KROKI DO WYKONANIA:

KROK 1: Schematy Walidacji (Zod)
Utwórz plik `src/features/auth/schemas.ts`.
Zdefiniuj dwa schematy:

1. `LoginSchema`: email (wymagany, format email), password (min 1 znak).
2. `RegisterSchema`: email, password (min 8 znaków), confirmPassword (musi pasować do password), name (opcjonalnie).

KROK 2: LoginForm Component (Zadanie 1.4.2)
Utwórz `src/features/auth/components/LoginForm.tsx`.
Wymagania:

- Użyj `useForm` z resolverem `zodResolver`.
- UI: Karta (`Card`, `CardHeader`, `CardContent`) z `shadcn/ui`.
- Pola: Email, Password.
- Submit: Wywołaj `signIn("credentials", { ... })` z `next-auth/react`.
- Obsługa błędów: Jeśli `signIn` zwróci error, wyświetl go (np. używając `sonner/toast` lub alertu w formularzu).
- Loading state: Przycisk "Sign In" ma być zablokowany i pokazywać spinner podczas wysyłania.
- Link do rejestracji na dole.

KROK 3: RegisterForm Component (Zadanie 1.4.4)
Utwórz `src/features/auth/components/RegisterForm.tsx`.
Wymagania:

- Analogicznie do LoginForm (Card UI, React Hook Form, Zod).
- Pola: Name, Email, Password, Confirm Password.
- Submit: Na ten moment (MVP) tylko zamockuj wywołanie API (console.log) i po 1s przekieruj do `/login` z toastem sukcesu. (Prawdziwe API podepniemy, gdy backend udostępni endpoint rejestracji).
- Link do logowania na dole.

KROK 4: Strony i Layout (Zadania 1.4.1, 1.4.3, 1.4.5)

1. Utwórz `src/app/(auth)/layout.tsx`: Prosty layout centrujący zawartość (flex center, min-h-screen, tło slate-50).
2. Utwórz `src/app/(auth)/login/page.tsx`: Renderuje `LoginForm`.
3. Utwórz `src/app/(auth)/register/page.tsx`: Renderuje `RegisterForm`.
4. Utwórz `src/app/(auth)/auth/error/page.tsx`:
   - Pobierz parametr błędu z URL (searchParams).
   - Wyświetl prostą kartę błędu z komunikatem (np. "Configuration Error" lub "Access Denied") i przyciskiem "Back to Login".

### WYMAGANIA TECHNICZNE:

- **Styl**: Clean & Professional (B2B SaaS style).
- **Form Components**: Użyj komponentów formularzy z `shadcn/ui` (`Form`, `FormControl`, `FormField`, `FormItem`, `FormMessage`), aby walidacja wyglądała spójnie.
- **Interakcja**: Użyj `useTransition` lub `isSubmitting` dla płynnego UX.

### OCZEKIWANY REZULTAT:

Kod dla schematów, komponentów formularzy oraz stron w Next.js App Router.

=========================================================================================================================================

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

-------------------------------------------------------------------> PROMPT <---------------------------------------------------------------

Działaj jako Senior Frontend Developer / UI Specialist.
Mamy działającą autentykację. Przechodzimy do Fazy 2.1: **Implementacja Głównego Layoutu Dashboardu**.

### CEL GŁÓWNY:

Stworzyć responsywny layout aplikacji ("shell"), który będzie zawierał boczny pasek nawigacyjny (Sidebar), górny pasek (Header) oraz miejsce na zmienną zawartość (page content).

### STRUKTURA KATALOGÓW:

Komponenty layoutu mają trafić do: `src/shared/components/layout/`
Layout strony: `src/app/(dashboard)/layout.tsx`

### KROKI DO WYKONANIA:

KROK 1: Sidebar Component (Zadanie 2.1.2)
Utwórz `src/shared/components/layout/Sidebar.tsx` ("use client").
Wymagania:

- Zdefiniuj tablicę nawigacji: Dashboard, Team, Clients, Plans, Subscriptions, Payments, Analytics.
- Użyj ikon z `lucide-react` pasujących do każdej sekcji.
- Active State: Użyj hooka `usePathname`. Jeśli link pokrywa się z obecną ścieżką, nadaj mu inny styl (np. tło accent/text-accent-foreground).
- Styl: Fixed width (np. w-64) na desktopie, ukryty lub jako Drawer na mobile (użyj `Sheet` z shadcn/ui dla mobile trigger).
- Na dole sidebara: Prosty footer z wersją aplikacji.

KROK 2: UserMenu Component (Zadanie 2.1.4)
Utwórz `src/shared/components/layout/UserMenu.tsx` ("use client").
Wymagania:

- Pobierz dane użytkownika (name, email, avatar) z `useSession` (lub `useAuthStore` jeśli zsynchronizowany).
- Użyj `DropdownMenu` z `shadcn/ui`.
- Trigger: Avatar użytkownika (fallback to inicjały).
- Content:
  - Label z imieniem i e-mailem.
  - Separator.
  - Item "Profile" (link).
  - Item "Settings" (link).
  - Item "Log out" (wywołaj `signOut()` z next-auth).

KROK 3: Header Component (Zadanie 2.1.3)
Utwórz `src/shared/components/layout/Header.tsx`.
Wymagania:

- Flex container.
- Lewa strona: Mobile Menu Trigger (widoczny tylko na mobile) + Logo (lub Breadcrumbs).
- Prawa strona: `UserMenu` (z Kroku 2) + `TenantSwitcher` (który stworzyliśmy w Fazie 1.2 - zaimportuj go z `@/features/tenant/components/TenantSwitcher`).

KROK 4: Dashboard Layout (Zadanie 2.1.1)
Utwórz `src/app/(dashboard)/layout.tsx`.
Wymagania:

- Struktura HTML/CSS (flex lub grid):
  - Sidebar (fixed left na desktop).
  - Main Content Area (flex-1, po prawej od sidebara).
  - Header (sticky top wewnątrz Main Content Area lub nad nim - wg uznania dla dashboardów SaaS).
- Pamiętaj, aby owinąć `children` w odpowiedni kontener z paddingiem (np. `p-6`).
- Layout ma być responsywny.

### WYMAGANIA UI:

- Użyj klas Tailwind CSS.
- Zachowaj spójność z `shadcn/ui`.
- Kolorystyka: Tło dashboardu powinno być lekko szare (np. `bg-slate-50/50` lub `bg-muted/40`), a karty białe.

### OCZEKIWANY REZULTAT:

# Kod dla Sidebara, Headera, UserMenu oraz głównego Layoutu. Po wklejeniu kodu, po zalogowaniu powinienem widzieć pełny interfejs aplikacji z działającą nawigacją.

===============================================================================================================================================

### 2.2 Global State & Feedback

| #     | Zadanie                    | Priorytet | Status | Opis                                      |
| ----- | -------------------------- | --------- | ------ | ----------------------------------------- |
| 2.2.1 | 🔴 QueryProvider           | Krytyczne | ⬜     | TanStack Query provider w root layout     |
| 2.2.2 | 🔴 Global ErrorBoundary    | Krytyczne | ⬜     | Przechwytywanie błędów z user-friendly UI |
| 2.2.3 | 🔴 Suspense boundaries     | Krytyczne | ⬜     | Suspense z fallback loading w layout      |
| 2.2.4 | 🔴 Toast provider          | Krytyczne | ⬜     | Sonner setup dla notyfikacji              |
| 2.2.5 | 🟡 Loading skeleton system | Ważne     | ⬜     | Reusable skeletons dla list, cards, forms |

**Blok 2.2 - Wymagania wejściowe**: Blok 2.1 (Layout Dashboardu)
**Blok 2.2 - Rezultat**: Aplikacja obsługująca ładowanie danych, błędy i powiadomienia

-------------------------------------------------------------------> PROMPT <---------------------------------------------------------------

Działaj jako Senior Frontend Developer / UX Architect.
Mamy już szkielet aplikacji (Layout). Przechodzimy do Fazy 2.2: **Globalna obsługa stanów, błędów i feedbacku**.

Chcemy uniknąć sytuacji, w której użytkownik widzi biały ekran podczas ładowania lub błędu. Chcemy wykorzystać mechanizmy Next.js (App Router) takie jak `loading.tsx` i `error.tsx`.

### CEL GŁÓWNY:

Skonfigurować infrastrukturę React Query, globalną obsługę błędów, ekrany ładowania (Skeletons) oraz system powiadomień (Toasts).

### LOKALIZACJA PLIKÓW:

- Providery: `src/core/providers/`
- Komponenty UI feedbacku: `src/shared/components/feedback/`
- Pliki specjalne Next.js: `src/app/(dashboard)/error.tsx`, `src/app/(dashboard)/loading.tsx`

### KROKI DO WYKONANIA:

KROK 1: QueryProvider (Zadanie 2.2.1)
Upewnij się, że plik `src/core/providers/QueryProvider.tsx` istnieje (stworzony w fazie 0, ale zweryfikujmy go).
Wymagania:

- Komponent "use client".
- Tworzy instancję `QueryClient` (użyj `useState` by tworzyć go raz per sesja przeglądarki, tzw. singleton pattern w React).
- Wrappuje dzieci w `QueryClientProvider` oraz (opcjonalnie w dev mode) `ReactQueryDevtools`.

KROK 2: Toast Provider (Zadanie 2.2.4)
Zintegruj `sonner` (shadcn/ui).

- Dodaj komponent `<Toaster />` do głównego layoutu `src/app/layout.tsx` (lub stwórz wrapper w `src/core/providers/ToastProvider.tsx` jeśli wolisz izolację, i dodaj go do layoutu).
- Upewnij się, że style są zaimportowane.

KROK 3: Loading Skeletons (Zadanie 2.2.5)
Utwórz reużywalne komponenty w `src/shared/components/feedback/`.

1. `TableSkeleton`: Wyświetla atrapę tabeli (nagłówek + 5 wierszy). Użyj komponentu `Skeleton` z shadcn.
2. `CardSkeleton`: Wyświetla atrapę karty (np. dla statystyk).
3. `FormSkeleton`: Wyświetla atrapę formularza (kilka inputów + przycisk).

KROK 4: Pliki specjalne Dashboardu (Zadania 2.2.2, 2.2.3)
Wewnątrz katalogu `src/app/(dashboard)/` utwórz:

1. `loading.tsx`:

   - Importuje i wyświetla ogólny szkielet strony (możesz użyć prostego `TableSkeleton` jako domyślnego widoku lub stworzyć `DashboardSkeleton`).
   - Dzięki temu sidebar pozostanie widoczny, a tylko środek będzie się ładował.

2. `error.tsx`:
   - Komponent "use client".
   - Przyjmuje propsy `{ error, reset }`.
   - Wyświetla ładny UI błędu (np. ikonę Alertu, treść błędu i przycisk "Try again" wywołujący `reset()`).
   - Użyj komponentów shadcn (`Alert`, `Button`).

### INTEGRACJA W ROOT LAYOUT:

Poinstruuj mnie, jak zaktualizować `src/app/layout.tsx`, aby zawierał wszystkie providery w odpowiedniej kolejności (np. AuthProvider -> QueryProvider -> TooltipProvider itp.).

### OCZEKIWANY REZULTAT:

Gotowy kod providerów, komponentów `Skeleton`, oraz plików `error.tsx` i `loading.tsx` dla dashboardu. Po wykonaniu tego kroku aplikacja powinna "miękko" obsługiwać ładowanie i błędy.

======================================================================================================================================================

### 3.1 Team List & CRUD

| #     | Zadanie                 | Priorytet | Status | Opis                                                 |
| ----- | ----------------------- | --------- | ------ | ---------------------------------------------------- |
| 3.1.1 | 🔴 Team members page    | Krytyczne | ⬜     | /team - lista członków zespołu                       |
| 3.1.2 | 🔴 MemberCard component | Krytyczne | ⬜     | Karta członka z avatar, role, actions                |
| 3.1.3 | 🔴 MemberList component | Krytyczne | ⬜     | Grid/list view z sortowaniem                         |
| 3.1.4 | 🔴 Team hooks           | Krytyczne | ⬜     | useTeamMembers, useUpdateMemberRole, useRemoveMember |

**Blok 3.1 - Wymagania wejściowe**: Faza 2 (Layout & Global UI)
**Blok 3.1 - Rezultat**: Pierwszy działający moduł biznesowy (Lista Zespołu)

-------------------------------------------------------------------> PROMPT <---------------------------------------------------------------

Działaj jako Senior Frontend Developer.
Zakończyliśmy prace nad infrastrukturą (Layout, Auth, Global UI).
Przechodzimy do Fazy 3: **Team Management**. Implementujemy pierwszy "Vertical Slice" funkcjonalności biznesowej.

### CEL GŁÓWNY:

Stworzyć widok listy członków zespołu wraz z możliwością zarządzania nimi (edycja roli, usuwanie). Dane mają być pobierane z API (przy użyciu wygenerowanych hooków Orval/TanStack Query).

### STRUKTURA KATALOGÓW:

Wszystkie pliki funkcjonalne trafiają do domeny `team`:

- Komponenty: `src/features/team/components/`
- Hooki: `src/features/team/hooks/`
- Strona: `src/app/(dashboard)/team/page.tsx`

### KROKI DO WYKONANIA:

KROK 1: Team Hooks (Zadanie 3.1.4)
Utwórz plik `src/features/team/hooks/useTeam.ts` (lub rozdziel na pliki).
Wymagania:

1.  Zaimportuj wygenerowane hooki z `@/core/api/generated` (np. `useGetTeamMembers`, `useUpdateTeamMember`, `useDeleteTeamMember` - nazwy mogą się różnić zależnie od Swaggera, dostosuj je).
2.  Stwórz custom hooki, które upraszczają użycie w komponentach:
    - `useTeamMembers(tenantId: string)`: Zwraca listę, status loading i error.
    - `useUpdateMemberRole()`: Wrapper na mutację update'u.
    - `useRemoveMember()`: Wrapper na mutację usuwania (powinien automatycznie inwalidować query listy członków po sukcesie, używając `queryClient.invalidateQueries`).

KROK 2: MemberCard Component (Zadanie 3.1.2)
Utwórz `src/features/team/components/MemberCard.tsx`.
Wymagania:

- Props: `member` (typ z modelu API), `isCurrentUser` (boolean).
- UI: Komponent `Card` z shadcn/ui.
- Treść:
  - Header: `Avatar` z inicjałami użytkownika oraz imię i email.
  - Content: Badge z rolą (użyj różnych kolorów dla Owner/Admin/Member).
  - Footer/Actions: Przycisk "More" (ikona kropek) otwierający `DropdownMenu`.
    - Opcje: "Change Role", "Remove from Team".
    - Opcje powinny być zablokowane, jeśli `isCurrentUser` = true (nie można usunąć samego siebie w tym widoku).

KROK 3: MemberList Component (Zadanie 3.1.3)
Utwórz `src/features/team/components/MemberList.tsx`.
Wymagania:

- Pobiera dane używając `useTeamMembers` (pobierz `tenantId` z `useTenant()`).
- Obsługa stanów:
  - Loading: Wyświetl `Skeleton` (np. 3x `MemberCard` w stanie loading lub ogólny skeleton).
  - Error: Wyświetl komunikat błędu.
  - Empty: Wyświetl komunikat "No team members found" (lub użyj komponentu EmptyState jeśli istnieje).
- Layout: Grid responsywny (1 kolumna mobile, 2 tablet, 3 desktop).
- Renderuje `MemberCard` dla każdego elementu.

KROK 4: Team Page (Zadanie 3.1.1)
Utwórz `src/app/(dashboard)/team/page.tsx`.
Wymagania:

- Nagłówek strony: "Team Members" + podtytuł "Manage your team access and roles".
- Przycisk akcji: "Invite Member" (na razie tylko placeholder UI, podepniemy logikę w następnym bloku).
- Renderuje `MemberList`.
- Zabezpieczenie: Użyj `<TenantGuard requiredRoles={['Owner', 'Admin', 'Member']} />` (import z features/tenant), aby upewnić się, że user ma dostęp do tenanta.

### WYMAGANIA TECHNICZNE:

- **Next.js 15 Compatibility**: Pamiętaj, że w Next.js 15 `params` i `searchParams` w komponentach stron (`page.tsx`) są asynchroniczne. Nie używaj ich bezpośrednio.
  Przykład: `const params = await props.params; const id = params.id;`.
- **Strict Types**: Używaj typów wygenerowanych przez Orval.
- **React Query**: Pamiętaj o obsłudze kluczy `queryKey` przy inwalidacji danych.

### OCZEKIWANY REZULTAT:

Kod dla hooków, komponentów karty i listy oraz strony głównej zespołu.

==============================================================================================================================================================================================

### 3.2 Invitations

| #     | Zadanie                     | Priorytet | Status | Opis                                            |
| ----- | --------------------------- | --------- | ------ | ----------------------------------------------- |
| 3.2.1 | 🔴 InviteMemberDialog       | Krytyczne | ⬜     | Dialog z formularzem zaproszenia                |
| 3.2.2 | 🔴 useInviteMember hook     | Krytyczne | ⬜     | Mutation do wysyłania zaproszeń                 |
| 3.2.3 | 🔴 Accept invitation page   | Krytyczne | ⬜     | /team/accept?token=xxx - akceptacja zaproszenia |
| 3.2.4 | 🟡 Pending invitations list | Ważne     | ⬜     | Lista oczekujących zaproszeń z akcjami          |
| 3.2.5 | 🟡 Resend/Cancel invitation | Ważne     | ⬜     | Akcje na zaproszeniach                          |

**Blok 3.2 - Wymagania wejściowe**: Blok 3.1 (Team List)
**Blok 3.2 - Rezultat**: Możliwość zapraszania nowych osób i akceptowania zaproszeń

-------------------------------------------------------------------> PROMPT <---------------------------------------------------------------

Działaj jako Senior Frontend Developer.
Mamy już listę pracowników. Teraz realizujemy Fazę 3.2: **System Zaproszeń (Invitations)**.

### CEL GŁÓWNY:

Umożliwić wysyłanie zaproszeń e-mailowych do nowych członków zespołu, zarządzanie wysłanymi (ale niezaakceptowanymi) zaproszeniami oraz stworzyć stronę lądowania dla osoby akceptującej zaproszenie.

### LOKALIZACJA PLIKÓW:

- Feature: `src/features/team/`
- Komponenty: `src/features/team/components/`
- Hooki: `src/features/team/hooks/`
- Strona akceptacji: `src/app/(dashboard)/team/accept/page.tsx` (zakładamy, że user musi być zalogowany, by zaakceptować)

### KROKI DO WYKONANIA:

KROK 1: Invitation Hooks (Zadania 3.2.2, 3.2.5)
Rozbuduj `src/features/team/hooks/useTeam.ts` (lub stwórz `useInvitations.ts`).
Wymagania:

1.  Zaimportuj wygenerowane mutacje/query z `@/core/api/generated` (np. `useInviteMember`, `useGetInvitations`, `useResendInvitation`, `useCancelInvitation`).
2.  Stwórz custom hooki:
    - `usePendingInvitations(tenantId)`: Pobiera listę zaproszeń.
    - `useInviteMember()`: Wrapper na mutację wysyłania. Po sukcesie musi odświeżyć listę zaproszeń (`queryClient.invalidateQueries`).
    - `useResendInvitation()`: Ponowne wysłanie e-maila.
    - `useCancelInvitation()`: Usunięcie zaproszenia.

KROK 2: InviteMemberDialog (Zadanie 3.2.1)
Utwórz `src/features/team/components/InviteMemberDialog.tsx`.
Wymagania:

- Props: `open` (boolean), `onOpenChange` (func) - sterowane z zewnątrz lub użyj wzorca triggera.
- Formularz (`react-hook-form` + `zod`):
  - `email` (email, required).
  - `role` (select: Admin, Member).
- UI: `Dialog` z shadcn/ui.
- Submit: Wywołaj `useInviteMember`.
- UX: Po sukcesie zamknij modal i pokaż Toast "Invitation sent".

KROK 3: PendingInvitationsList (Zadanie 3.2.4)
Utwórz `src/features/team/components/PendingInvitationsList.tsx`.
Wymagania:

- Pobiera dane przez `usePendingInvitations`.
- Wyświetla tabelę lub listę kart (zależnie od mobile/desktop).
- Kolumny: Email, Role, Sent Date, Status (zawsze "Pending").
- Akcje (po prawej stronie wiersza):
  - Button/Icon "Resend" (wywołuje `useResendInvitation`).
  - Button/Icon "Revoke" (wywołuje `useCancelInvitation` z potwierdzeniem).
- Jeśli lista pusta -> nie renderuj nic (return null) lub pokaż komunikat "No pending invitations".

KROK 4: Aktualizacja Team Page
Zaktualizuj `src/app/(dashboard)/team/page.tsx`.

1.  Dodaj `PendingInvitationsList` pod `MemberList` (np. oddzielone nagłówkiem "Pending Invitations").
2.  Podepnij `InviteMemberDialog` pod przycisk "Invite Member" w nagłówku strony.

KROK 5: Accept Invitation Page (Zadanie 3.2.3)
Utwórz `src/app/(dashboard)/team/accept/page.tsx`.
Wymagania:

- Pobierz `token` z query params (`searchParams`).
- UI: Wyświetl kartę z nagłówkiem "Join Team".
- Treść: "You have been invited to join [Team Name]. Click below to accept."
- Button: "Accept Invitation".
- Logika: Kliknięcie wywołuje API (np. `POST /api/invitations/accept`).
- Po sukcesie: Przekieruj na `/dashboard` i pokaż Toast "Welcome to the team!".
- Error handling: Jeśli token wygasł, pokaż stosowny komunikat.

### WYMAGANIA TECHNICZNE:

- **Walidacja**: Schemat Zod dla formularza zaproszeń.
- **Optymistyczne UI**: Przyciski akcji (Resend, Revoke) powinny pokazywać stan ładowania.

### OCZEKIWANY REZULTAT:

Kod dla hooków zaproszeń, komponentu dialogu, listy oczekujących oraz strony akceptacji. Zaktualizowany kod `TeamPage` integrujący te elementy.

=============================================================================================================================================================================================

### 4A.1 Clients List

| #      | Zadanie                     | Priorytet | Status | Opis                                            |
| ------ | --------------------------- | --------- | ------ | ----------------------------------------------- |
| 4A.1.1 | 🔴 Clients page             | Krytyczne | ⬜     | /clients - główna strona z listą                |
| 4A.1.2 | 🔴 ClientTable component    | Krytyczne | ⬜     | Tabela z sortowaniem, paginacją                 |
| 4A.1.3 | 🔴 ClientCard component     | Krytyczne | ⬜     | Karta klienta dla grid view                     |
| 4A.1.4 | 🔴 View toggle (table/grid) | Krytyczne | ⬜     | Przełączanie widoku                             |
| 4A.1.5 | 🔴 Clients hooks            | Krytyczne | ⬜     | useClients, useClient - z wygenerowanych hooków |

**Blok 4A.1 - Wymagania wejściowe**: Faza 3 (Team Management zakończone)
**Blok 4A.1 - Rezultat**: Funkcjonalna lista klientów z obsługą widoków i paginacji

-------------------------------------------------------------------> PROMPT <---------------------------------------------------------------

Działaj jako Senior Frontend Developer.
Rozpoczynamy Fazę 4A: **Clients Management**. To kluczowa funkcjonalność CRM.

### CEL GŁÓWNY:

Stworzyć główny widok listy klientów (`/clients`) z możliwością przełączania między widokiem tabeli a widokiem siatki (Grid), obsługą paginacji i podstawowym sortowaniem.

### STRUKTURA KATALOGÓW:

- Feature: `src/features/clients/`
- Komponenty: `src/features/clients/components/`
- Hooki: `src/features/clients/hooks/`
- Strona: `src/app/(dashboard)/clients/page.tsx`

### KROKI DO WYKONANIA:

KROK 1: Clients Hooks (Zadanie 4A.1.5)
Utwórz `src/features/clients/hooks/useClients.ts`.
Wymagania:

1.  Zaimportuj `useGetClients` z wygenerowanego API.
2.  Stwórz wrapper `useClientsQuery(params)`, który obsługuje parametry: `page`, `pageSize`, `sortBy`, `sortDirection`, `search`.
3.  Hook powinien zwracać dane w formacie ułatwiającym paginację (items, totalCount, pageCount).
4.  Dodaj `keepPreviousData: true` (placeholderData w TanStack Query v5), aby uniknąć migania podczas zmiany stron.

KROK 2: ClientCard Component (Zadanie 4A.1.3)
Utwórz `src/features/clients/components/ClientCard.tsx`.
Wymagania:

- Reprezentuje pojedynczego klienta w widoku Grid.
- UI: `Card` z shadcn.
- Header: Avatar/Initials + Imię i Nazwisko.
- Content: E-mail, Telefon, Status (Badge: Active/Inactive).
- Footer: Przycisk "Manage" (link do szczegółów) lub Dropdown Menu z akcjami.

KROK 3: ClientTable Component (Zadanie 4A.1.2)
Utwórz `src/features/clients/components/ClientTable.tsx`.
Wymagania:

- UI: Komponent `Table` z shadcn.
- Kolumny: Name (z avatarem), Email, Status, Created At, Actions.
- Nagłówki kolumn powinny być klikalne dla sortowania (jeśli API to wspiera).
- Wiersz powinien być linkiem (lub zawierać przycisk) do `/clients/[id]`.

KROK 4: View Toggle & Toolbar (Zadanie 4A.1.4)
Utwórz `src/features/clients/components/ClientViewOptions.tsx`.
Wymagania:

- Komponent zawierający przełącznik widoku: Ikony `LayoutList` (Tabela) i `LayoutGrid` (Karty).
- Użyj `Tabs` lub `ToggleGroup` z shadcn.
- Komponent powinien przyjmować `viewMode` i `setViewMode`.

KROK 5: Clients Page (Zadanie 4A.1.1)
Utwórz `src/app/(dashboard)/clients/page.tsx`.
Wymagania:

- Zarządzanie stanem URL: Użyj hooka do synchronizacji stanu z URL (np. `page`, `view`).
- State: `viewMode` (domyślnie 'table').
- Layout:
  - Header: Tytuł "Clients", przycisk "Add Client" (placeholder), `ClientViewOptions`.
  - Content: Renderuje `ClientTable` LUB grid z `ClientCard` w zależności od `viewMode`.
  - Footer: Komponent paginacji (Previous / Page X of Y / Next).
- Loading State: Wyświetl `TableSkeleton` lub `GridSkeleton` (zależnie od widoku).

### WYMAGANIA TECHNICZNE:

- **Responsywność**: Na mobile zawsze wymuszaj widok Grid lub Card (tabela na telefonie jest nieczytelna).
- **Formatowanie**: Użyj helperów `formatDate` stworzonych w Fazie 0.
- **Typy**: Ścisłe typowanie `ClientDto`.

### OCZEKIWANY REZULTAT:

Kod dla hooków, komponentów (Card, Table, ViewOptions) oraz strony głównej klientów.

============================================================================================================================================

### 4A.2 Clients Search & Filters

| #      | Zadanie                    | Priorytet | Status | Opis                             |
| ------ | -------------------------- | --------- | ------ | -------------------------------- |
| 4A.2.1 | 🔴 ClientSearch component  | Krytyczne | ⬜     | Search input z debounce          |
| 4A.2.2 | 🔴 ClientFilters component | Krytyczne | ⬜     | Filtry: status, type, date range |
| 4A.2.3 | 🟡 useDebounce hook        | Ważne     | ⬜     | Reusable debounce hook           |
| 4A.2.4 | 🟡 Filter persistence      | Ważne     | ⬜     | Zapisywanie filtrów w URL params |

**Blok 4A.2 - Wymagania wejściowe**: Blok 4A.1 (Lista Klientów)
**Blok 4A.2 - Rezultat**: Zaawansowane wyszukiwanie i filtrowanie zsynchronizowane z URL

-------------------------------------------------------------------> PROMPT <---------------------------------------------------------------

Działaj jako Senior Frontend Developer.
Mamy działającą listę klientów. W Fazie 4A.2 dodajemy do niej **interaktywność: wyszukiwanie i filtrowanie**.

Chcemy, aby stan filtrów był odzwierciedlony w URL (URL Search Params), co pozwoli na łatwe udostępnianie linków do konkretnych wyników.

### CEL GŁÓWNY:

Zaimplementować wyszukiwarkę z mechanizmem debounce oraz zestaw filtrów, które aktualizują parametry URL, wymuszając odświeżenie listy klientów.

### LOKALIZACJA PLIKÓW:

- Shared Hooks: `src/shared/hooks/`
- Komponenty: `src/features/clients/components/`
- Integracja: `src/app/(dashboard)/clients/page.tsx`

### KROKI DO WYKONANIA:

KROK 1: useDebounce Hook (Zadanie 4A.2.3)
Utwórz plik `src/shared/hooks/useDebounce.ts`.
Wymagania:

- Generic hook przyjmujący `value` (T) i `delay` (number, default 500ms).
- Zwraca `debouncedValue`.
- Używa `useEffect` i `setTimeout` do opóźnienia aktualizacji wartości.

KROK 2: ClientSearch Component (Zadanie 4A.2.1)
Utwórz `src/features/clients/components/ClientSearch.tsx`.
Wymagania:

- UI: Input z ikoną lupki (shadcn/ui `Input`).
- Props: Brak (komponent powinien sam zarządzać swoim stanem w oparciu o URL).
- Logika:
  1. Pobierz obecną wartość `search` z URL (`useSearchParams`).
  2. Ustaw lokalny stan inputa.
  3. Użyj `useDebounce` na lokalnym stanie.
  4. W `useEffect` nasłuchuj zmian `debouncedValue` i aktualizuj URL używając `router.push` (lub `router.replace`) i `usePathname`.
  5. Jeśli wartość jest pusta, usuń parametr `search` z URL.

KROK 3: ClientFilters Component (Zadanie 4A.2.2)
Utwórz `src/features/clients/components/ClientFilters.tsx`.
Wymagania:

- UI: Zestaw dropdownów (shadcn `Select` lub `Popover` z `Command` dla multiselectu).
- Filtry do obsłużenia:
  - `Status` (Active, Inactive, Blocked).
  - `Type` (Individual, Business).
- Przycisk "Reset Filters" (widoczny tylko, gdy jakieś filtry są aktywne).
- Logika: Analogicznie do Search - pobierz stan z URL i aktualizuj URL po zmianie wyboru.

KROK 4: Integracja w Clients Page (Zadanie 4A.2.4)
Zaktualizuj `src/app/(dashboard)/clients/page.tsx`.
Wymagania:

1. Umieść `ClientSearch` i `ClientFilters` w sekcji nagłówka (toolbar).
2. Upewnij się, że hook `useClients` (stworzony w poprzednim bloku) pobiera parametry bezpośrednio z `searchParams` (Next.js Page props) lub przez `useSearchParams` hook, i przekazuje je do zapytania API.
   _Uwaga: W Next.js 15 App Router w komponencie Page `searchParams` może być Promise'm, więc obsłuż to odpowiednio lub użyj wersji "use client" dla komponentu wrappującego._

### WYMAGANIA TECHNICZNE:

- **UX**: Podczas pisania w wyszukiwarce URL powinien się zmieniać z opóźnieniem, ale Input powinien reagować natychmiast.
- **Paginacja**: Zmiana filtrów lub wyszukiwania powinna resetować paginację do strony 1 (`page=1`).

### OCZEKIWANY REZULTAT:

# Kod hooka `useDebounce`, komponentów `ClientSearch` i `ClientFilters` oraz zaktualizowany kod strony `ClientsPage`.

============================================================================================================================================================

### 4A.3 Client CRUD

| #      | Zadanie                  | Priorytet | Status | Opis                                              |
| ------ | ------------------------ | --------- | ------ | ------------------------------------------------- |
| 4A.3.1 | 🔴 ClientForm component  | Krytyczne | ⬜     | Formularz create/edit z Zod validation            |
| 4A.3.2 | 🔴 Create client page    | Krytyczne | ⬜     | /clients/new                                      |
| 4A.3.3 | 🔴 Client detail page    | Krytyczne | ⬜     | /clients/[id] - szczegóły klienta                 |
| 4A.3.4 | 🔴 Edit client page      | Krytyczne | ⬜     | /clients/[id]/edit                                |
| 4A.3.5 | 🔴 Delete confirmation   | Krytyczne | ⬜     | AlertDialog z potwierdzeniem usunięcia            |
| 4A.3.6 | 🟡 Client mutation hooks | Ważne     | ⬜     | useCreateClient, useUpdateClient, useDeleteClient |

**Blok 4A.3 - Wymagania wejściowe**: Blok 4A.1 (Clients Hooks)
**Blok 4A.3 - Rezultat**: Możliwość dodawania, edycji, podglądu i usuwania klientów

-------------------------------------------------------------------> PROMPT <---------------------------------------------------------------

Działaj jako Senior Frontend Developer.
Mamy już listę klientów. Teraz implementujemy pełny cykl życia danych: **CRUD (Create, Read, Update, Delete)**.

Chcemy wykorzystać potęgę React Hook Form i Zod do walidacji oraz React Query do zarządzania stanem serwera.

### CEL GŁÓWNY:

Stworzyć formularz klienta (wspólny dla tworzenia i edycji), podpiąć go pod widoki `/new` i `/[id]/edit`, stworzyć widok szczegółów `/[id]` oraz obsłużyć usuwanie rekordu.

### STRUKTURA PLIKÓW:

- Feature: `src/features/clients/`
- Strony:
  - `src/app/(dashboard)/clients/new/page.tsx`
  - `src/app/(dashboard)/clients/[id]/page.tsx`
  - `src/app/(dashboard)/clients/[id]/edit/page.tsx`

### KROKI DO WYKONANIA:

KROK 1: Client Schema & Hooks (Zadanie 4A.3.6)

1. W pliku `src/features/clients/schemas.ts` zdefiniuj `ClientFormSchema` przy użyciu Zod (fields: firstName, lastName, email, phone, type, status, notes).
2. Rozbuduj `src/features/clients/hooks/useClients.ts` o mutacje:
   - `useCreateClient()`: Po sukcesie: Toast "Client created" + `queryClient.invalidateQueries({ queryKey: ['clients'] })`.
   - `useUpdateClient()`: Po sukcesie: Toast "Client updated" + inwalidacja listy i szczegółów.
   - `useDeleteClient()`: Po sukcesie: Toast "Client deleted" + inwalidacja listy.

KROK 2: ClientForm Component (Zadanie 4A.3.1)
Utwórz `src/features/clients/components/ClientForm.tsx`.
Wymagania:

- Props: `initialData?` (ClientDto), `onSubmit` (handler), `isLoading` (boolean).
- Użyj `useForm` z `zodResolver(ClientFormSchema)`.
- Jeśli podano `initialData`, ustaw `defaultValues`.
- UI: Grid formularza (np. 2 kolumny). Pola tekstowe, Select dla Statusu i Typu, Textarea dla notatek.
- Error Handling: Wyświetl błędy walidacji pod polami.
- Actions: Przyciski "Cancel" (wraca do listy) i "Save Client" (submit).

KROK 3: Create Client Page (Zadanie 4A.3.2)
Utwórz `src/app/(dashboard)/clients/new/page.tsx`.
Wymagania:

- Nagłówek "New Client".
- Renderuje `ClientForm`.
- `onSubmit`: Wywołuje `useCreateClient`. Po sukcesie przekierowuje (`router.push`) do `/clients`.

KROK 4: Edit Client Page (Zadanie 4A.3.4)
Utwórz `src/app/(dashboard)/clients/[id]/edit/page.tsx`.
Wymagania:

- Pobiera ID z params.
- Pobiera dane klienta (`useClient(id)`).
- Loading state: Pokazuje spinner/skeleton.
- Renderuje `ClientForm` przekazując pobrane dane jako `initialData`.
- `onSubmit`: Wywołuje `useUpdateClient`.

KROK 5: Client Detail Page (Zadanie 4A.3.3)
Utwórz `src/app/(dashboard)/clients/[id]/page.tsx`.
Wymagania:

- **Async Params**: Pobierz ID używając `const { id } = await params`.
- Pobierz dane klienta (`useClient(id)`).
- Wyświetl nagłówek z danymi klienta i przyciski akcji (Edit, Delete).
- Pamiętaj, aby Delete wywoływał `AlertDialog`.

### WYMAGANIA TECHNICZNE:

- **Obsługa błędów API**: Jeśli backend zwróci błędy walidacji (400/422), formularz powinien je wyświetlić. Jeśli masz helper `applyServerErrors`, użyj go.
- **UX**: Formularz nie powinien pozwalać na ponowny submit w trakcie trwania requestu (`isSubmitting`).

### OCZEKIWANY REZULTAT:

Kod schematu Zod, kompletny komponent `ClientForm`, hooki mutacji oraz kod trzech podstron (New, Edit, Details).

========================================================================================================================================================

### 4B.1 Plans List

| #      | Zadanie               | Priorytet | Status | Opis                                  |
| ------ | --------------------- | --------- | ------ | ------------------------------------- |
| 4B.1.1 | 🔴 Plans page         | Krytyczne | ⬜     | /plans - lista planów subskrypcyjnych |
| 4B.1.2 | 🔴 PlanCard component | Krytyczne | ⬜     | Karta planu z ceną, features, status  |
| 4B.1.3 | 🔴 PlanGrid component | Krytyczne | ⬜     | Grid view planów                      |
| 4B.1.4 | 🔴 Popular badge      | Krytyczne | ⬜     | Badge dla popularnych planów          |
| 4B.1.5 | 🔴 Plans hooks        | Krytyczne | ⬜     | usePlans, usePlan                     |

**Blok 4B.1 - Wymagania wejściowe**: Faza 3 (Team Management zakończone)
**Blok 4B.1 - Rezultat**: Estetyczna lista (katalog) planów subskrypcyjnych

-------------------------------------------------------------------> PROMPT <---------------------------------------------------------------

Działaj jako Senior Frontend Developer / UI Designer.
Rozpoczynamy Fazę 4B: **Plans Management**. To katalog produktów/usług, które oferuje nasz użytkownik (Provider).

W przeciwieństwie do listy klientów (tabela), plany będą prezentowane w formie **Pricing Cards** (siatka kart), aby użytkownik łatwo widział strukturę swojej oferty.

### STRUKTURA PLIKÓW:

- Feature: `src/features/plans/`
- Komponenty: `src/features/plans/components/`
- Hooki: `src/features/plans/hooks/`
- Strona: `src/app/(dashboard)/plans/page.tsx`

### KROKI DO WYKONANIA:

KROK 1: Plans Hooks (Zadanie 4B.1.5)
Utwórz `src/features/plans/hooks/usePlans.ts`.
Wymagania:

- Zaimportuj `useGetPlans` z wygenerowanego API.
- Stwórz wrapper `usePlans(tenantId)`.
- Zazwyczaj planów jest mało (3-5), więc paginacja serwerowa może nie być konieczna, ale jeśli API ją wspiera, obsłuż ją (lub pobierz wszystkie i wyświetl).

KROK 2: PlanCard Component (Zadania 4B.1.2, 4B.1.4)
Utwórz `src/features/plans/components/PlanCard.tsx`.
Wymagania:

- UI: Karta stylizowana na element cennika.
- Props: `plan` (PlanDto).
- Header:
  - Nazwa planu (np. "Pro").
  - Jeśli `plan.isPopular` jest true -> wyświetl Badge "Most Popular" (np. w prawym górnym rogu, kolor primary).
- Content:
  - Cena: Użyj helpera `formatCurrency` (np. duża czcionka "99 PLN" + mniejsza "/ mc").
  - Status: Badge (Active/Archived).
  - Features: Lista punktowana cech planu (jeśli są dostępne w modelu danych).
- Footer:
  - Przycisk "Edit Plan" (link do edycji - na razie pusty href).
  - Przycisk/Menu "More" (deactivate, delete).

KROK 3: PlanGrid Component (Zadanie 4B.1.3)
Utwórz `src/features/plans/components/PlanGrid.tsx`.
Wymagania:

- Pobiera dane z `usePlans`.
- Obsługuje stan Loading (Skeleton kart) i Error.
- Layout: CSS Grid.
  - Mobile: 1 kolumna.
  - Tablet: 2 kolumny.
  - Desktop: 3 lub 4 kolumny.
- Renderuje `PlanCard` dla każdego elementu.
- Empty State: Jeśli brak planów, wyświetl zachętę "Create your first subscription plan".

KROK 4: Plans Page (Zadanie 4B.1.1)
Utwórz `src/app/(dashboard)/plans/page.tsx`.
Wymagania:

- Header: Tytuł "Subscription Plans" + Button "Create Plan".
- Content: Renderuje `PlanGrid`.

### WYMAGANIA TECHNICZNE:

- **Styling**: Karty powinny być równej wysokości (`h-full` w flex container).
- **Waluta**: Upewnij się, że cena jest sformatowana poprawnie dla waluty danego planu.
- **Dostępność**: Badge "Popular" powinien być dostępny dla czytników ekranowych.

### OCZEKIWANY REZULTAT:

# Kod dla hooka `usePlans`, komponentów `PlanCard` i `PlanGrid` oraz strony głównej planów.

### 4B.2 Plan CRUD

| #      | Zadanie                       | Priorytet  | Status | Opis                                            |
| ------ | ----------------------------- | ---------- | ------ | ----------------------------------------------- |
| 4B.2.1 | 🔴 PlanForm component         | Krytyczne  | ⬜     | Formularz z pricing, billing interval, features |
| 4B.2.2 | 🔴 Create plan page           | Krytyczne  | ⬜     | /plans/new                                      |
| 4B.2.3 | 🔴 Plan detail page           | Krytyczne  | ⬜     | /plans/[id]                                     |
| 4B.2.4 | 🔴 Edit plan page             | Krytyczne  | ⬜     | /plans/[id]/edit                                |
| 4B.2.5 | 🔴 Activate/Deactivate toggle | Krytyczne  | ⬜     | Zmiana statusu planu                            |
| 4B.2.6 | 🟡 Plan mutation hooks        | Ważne      | ⬜     | useCreatePlan, useUpdatePlan, useActivatePlan   |
| 4B.2.7 | 🟢 Plan preview               | Opcjonalne | ⬜     | Podgląd jak plan będzie wyglądał dla klienta    |

**Blok 4B.2 - Wymagania wejściowe**: Blok 4B.1 (Lista Planów)
**Blok 4B.2 - Rezultat**: Pełne zarządzanie ofertą (tworzenie, edycja, zmiana statusu)

-------------------------------------------------------------------> PROMPT <---------------------------------------------------------------

Działaj jako Senior Frontend Developer.
Mamy widok listy planów. Teraz implementujemy logikę **CRUD dla Planów Subskrypcyjnych**.

To zadanie wymaga obsługi **dynamicznych formularzy** (lista features), ponieważ użytkownik może dodać dowolną liczbę cech do planu.

### CEL GŁÓWNY:

Stworzyć zaawansowany formularz dodawania/edycji planu z obsługą `useFieldArray` dla listy cech, oraz widoki szczegółów i edycji.

### STRUKTURA PLIKÓW:

- Feature: `src/features/plans/`
- Strony:
  - `src/app/(dashboard)/plans/new/page.tsx`
  - `src/app/(dashboard)/plans/[id]/page.tsx`
  - `src/app/(dashboard)/plans/[id]/edit/page.tsx`

### KROKI DO WYKONANIA:

KROK 1: Plan Schema & Hooks (Zadanie 4B.2.6)

1. W `src/features/plans/schemas.ts` zdefiniuj `PlanFormSchema`:
   - `name`: string (min 3 znaki).
   - `description`: string (optional).
   - `price`: number (min 0).
   - `currency`: string (default 'PLN' lub enum).
   - `interval`: enum ('month', 'year').
   - `features`: array of strings (min 1 element).
2. Rozbuduj `src/features/plans/hooks/usePlans.ts` o mutacje:
   - `useCreatePlan`, `useUpdatePlan`.
   - `useTogglePlanStatus`: Specjalna mutacja do zmiany pola `isActive` (lub `status`). Po zmianie inwaliduj listę planów.

KROK 2: PlanForm Component (Zadanie 4B.2.1)
Utwórz `src/features/plans/components/PlanForm.tsx`.
Wymagania:

- Użyj `useForm` oraz **`useFieldArray`** do zarządzania listą `features`.
- Sekcja "General": Name, Description.
- Sekcja "Pricing": Price (Input type number), Currency (Select), Interval (Select).
- Sekcja "Features":
  - Lista inputów z przyciskami "Remove" (ikona kosza) obok każdego.
  - Przycisk "+ Add Feature" pod listą.
- (Opcjonalnie - Zadanie 4B.2.7): Obok formularza wyświetl "Live Preview" używając komponentu `PlanCard` (stworzonego w poprzedniej fazie), zasilanego danymi z `form.watch()`.

KROK 3: Create & Edit Pages (Zadania 4B.2.2, 4B.2.4)

1. `src/app/(dashboard)/plans/new/page.tsx`:
   - Renderuje pusty `PlanForm`.
   - Submit tworzy plan i przekierowuje do `/plans`.
2. `src/app/(dashboard)/plans/[id]/edit/page.tsx`:
   - Pobiera dane (`usePlan(id)`).
   - Renderuje `PlanForm` z `initialData`.
   - Submit aktualizuje plan.

KROK 4: Plan Detail Page (Zadania 4B.2.3, 4B.2.5)
Utwórz `src/app/(dashboard)/plans/[id]/page.tsx`.
Wymagania:

- Header: Nazwa planu, Badges (Status, Interval), Cena.
- Actions:
  - Button "Edit" (link).
  - Toggle/Switch "Active Plan": Wywołuje `useTogglePlanStatus`. Zmiana powinna być natychmiastowa (optimistic UI lub loading state).
- Content:
  - Sekcja "Features List" (wylistowane cechy).
  - Statystyki (Placeholder): "Active Subscriptions using this plan: 0" (to dodamy w przyszłości).

### WYMAGANIA TECHNICZNE:

- **Dynamic Fields**: Upewnij się, że `useFieldArray` jest poprawnie zaimplementowane (klucze, dodawanie, usuwanie).
- **Preview**: Jeśli implementujesz podgląd, pamiętaj, że `PlanCard` oczekuje obiektu `PlanDto`, a formularz ma `PlanFormValues`. Może być potrzebne mapowanie typów w locie.

### OCZEKIWANY REZULTAT:

Kod schematu Zod, zaawansowanego komponentu `PlanForm` z dynamiczną listą cech, hooków mutacji oraz stron obsługujących proces.

==================================================================================================================================================================

### 5.1 Subscriptions List

| #     | Zadanie                        | Priorytet | Status | Opis                                          |
| ----- | ------------------------------ | --------- | ------ | --------------------------------------------- |
| 5.1.1 | 🔴 Subscriptions page          | Krytyczne | ⬜     | /subscriptions - lista wszystkich subskrypcji |
| 5.1.2 | 🔴 SubscriptionTable component | Krytyczne | ⬜     | Tabela z client, plan, status, actions        |
| 5.1.3 | 🔴 SubscriptionStatusBadge     | Krytyczne | ⬜     | Badge z kolorami dla statusów                 |
| 5.1.4 | 🔴 Subscription filters        | Krytyczne | ⬜     | Filtrowanie po status, plan, date             |
| 5.1.5 | 🔴 Subscriptions hooks         | Krytyczne | ⬜     | useSubscriptions, useSubscription             |

**Blok 5.1 - Wymagania wejściowe**: Faza 4A (Klienci) i 4B (Plany) - zakończone
**Blok 5.1 - Rezultat**: Centralny widok zarządzania subskrypcjami

-------------------------------------------------------------------> PROMPT <---------------------------------------------------------------

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

KROK 1: Subscriptions Hooks (Zadanie 5.1.5)
Utwórz `src/features/subscriptions/hooks/useSubscriptions.ts`.
Wymagania:

- Zaimportuj `useGetSubscriptions` z API.
- Stwórz wrapper obsługujący parametry: `page`, `pageSize`, `status`, `planId`, `search` (szukanie po nazwisku klienta).
- Upewnij się, że typ zwracany (DTO) zawiera zagnieżdżone dane o Kliencie (`clientName`, `clientEmail`) i Planie (`planName`, `price`), aby uniknąć problemu "N+1" po stronie frontendu.

KROK 2: SubscriptionStatusBadge (Zadanie 5.1.3)
Utwórz `src/features/subscriptions/components/SubscriptionStatusBadge.tsx`.
Wymagania:

- Props: `status` (Enum: Active, Canceled, PastDue, Trialing, Paused).
- UI: Komponent `Badge` z shadcn.
- Mapowanie kolorów (użyj klas Tailwind `bg-color-100 text-color-800` lub wariantów Badge):
  - Active -> Green (Success)
  - PastDue -> Red (Destructive)
  - Canceled -> Gray (Secondary)
  - Trialing -> Blue (Info)
  - Paused -> Orange/Yellow (Warning)

KROK 3: SubscriptionFilters (Zadanie 5.1.4)
Utwórz `src/features/subscriptions/components/SubscriptionFilters.tsx`.
Wymagania:

- Komponent "use client" zintegrowany z URL (podobnie jak w Clients).
- Filtry:
  - Status (Multi-select lub Select).
  - Plan (Select - dane pobierz używając hooka `usePlans` z features/plans - import cross-feature jest dozwolony w warstwie UI).

KROK 4: SubscriptionTable (Zadanie 5.1.2)
Utwórz `src/features/subscriptions/components/SubscriptionTable.tsx`.
Wymagania:

- Kolumny:
  1. Client (Avatar + Name + Email).
  2. Plan (Name + Price + Interval).
  3. Status (StatusBadge).
  4. Next Billing Date (sformatowana data).
  5. Actions (Dropdown: "View Details", "Cancel Subscription").
- Wiersz klikalny -> przenosi do `/subscriptions/[id]`.

KROK 5: Subscriptions Page (Zadanie 5.1.1)
Utwórz `src/app/(dashboard)/subscriptions/page.tsx`.
Wymagania:

- Header: Title "Subscriptions" + Button "Create Subscription" (link do `/subscriptions/new`).
- Toolbar: `SubscriptionFilters` + Search.
- Content: `SubscriptionTable`.
- Pagination: Komponent paginacji na dole.

### WYMAGANIA TECHNICZNE:

- **Formatowanie walut**: Użyj helpera `formatCurrency` dla ceny planu w tabeli.
- **Cross-Feature Imports**: Możesz importować `usePlans` w filtrach, ale unikaj cyklicznych zależności w logice biznesowej.
- **Data Display**: Daty ("Next Billing") sformatuj czytelnie (np. "Oct 24, 2025").

### OCZEKIWANY REZULTAT:

Kod dla hooków, komponentu Badge, Tabeli, Filtrów oraz głównej strony subskrypcji.

======================================================================================================================================================

### 5.2 Subscription Actions

| #     | Zadanie                      | Priorytet | Status | Opis                                     |
| ----- | ---------------------------- | --------- | ------ | ---------------------------------------- |
| 5.2.1 | 🔴 Create subscription flow  | Krytyczne | ⬜     | Wizard: wybór client → plan → confirm    |
| 5.2.2 | 🔴 Subscription detail page  | Krytyczne | ⬜     | /subscriptions/[id]                      |
| 5.2.3 | 🔴 Cancel subscription       | Krytyczne | ⬜     | Dialog z reason, immediate/end-of-period |
| 5.2.4 | 🟡 Pause/Resume subscription | Ważne     | ⬜     | Zawieszanie subskrypcji                  |
| 5.2.5 | 🟡 Change plan               | Ważne     | ⬜     | Upgrade/downgrade planu                  |

**Blok 5.2 - Wymagania wejściowe**: Blok 5.1 (Lista Subskrypcji)
**Blok 5.2 - Rezultat**: Możliwość tworzenia, edycji i anulowania subskrypcji

-------------------------------------------------------------------> PROMPT <---------------------------------------------------------------

Działaj jako Senior Frontend Developer / UX Specialist.
Mamy listę subskrypcji. Teraz implementujemy logikę biznesową zarządzania nimi.

Szczególnym wyzwaniem jest proces tworzenia subskrypcji, który zrealizujemy jako **Multi-step Wizard**, aby ułatwić użytkownikowi łączenie Klienta z Planem.

### CEL GŁÓWNY:

Stworzyć kreator nowej subskrypcji (Wizard), widok szczegółów oraz okna dialogowe do akcji krytycznych (Anulowanie, Zmiana Planu).

### STRUKTURA PLIKÓW:

- Feature: `src/features/subscriptions/`
- Strony:
  - `src/app/(dashboard)/subscriptions/new/page.tsx`
  - `src/app/(dashboard)/subscriptions/[id]/page.tsx`

### KROKI DO WYKONANIA:

KROK 1: Subscription Action Hooks (Zadania 5.2.3, 5.2.4, 5.2.5)
Rozbuduj `src/features/subscriptions/hooks/useSubscriptions.ts` (lub stwórz `useSubscriptionActions.ts`):

1. `useCreateSubscription()`: Przyjmuje { clientId, planId, startDate }.
2. `useCancelSubscription()`: Przyjmuje { subscriptionId, reason, cancelImmediately }.
3. `usePauseSubscription()` / `useResumeSubscription()`.
4. `useChangeSubscriptionPlan()`: Przyjmuje { subscriptionId, newPlanId }.
   _Pamiętaj o inwalidacji odpowiednich zapytań po sukcesie._

KROK 2: Create Subscription Wizard (Zadanie 5.2.1)
Utwórz zestaw komponentów w `src/features/subscriptions/components/wizard/`:

1. `CreateSubscriptionWizard.tsx`: Główny kontener zarządzający stanem (`step`: 1|2|3, `selectedClient`, `selectedPlan`).
2. `StepSelectClient.tsx`:
   - Wykorzystaj `ClientSearch` (z Fazy 4A) lub stwórz uproszczoną listę z wyszukiwaniem.
   - Po kliknięciu w klienta -> ustawia stan i przechodzi dalej.
3. `StepSelectPlan.tsx`:
   - Wyświetla plany (możesz użyć `PlanGrid` lub `PlanCard` z Fazy 4B, dodając prop `onSelect` i tryb "selectable").
4. `StepConfirm.tsx`:
   - Podsumowanie: Wybrany klient + Wybrany plan + Data startu (Date Picker, domyślnie today).
   - Przycisk "Create Subscription".

KROK 3: Subscription Detail Page (Zadanie 5.2.2)
Utwórz `src/app/(dashboard)/subscriptions/[id]/page.tsx`.
Wymagania:

- Header: ID Subskrypcji, StatusBadge, Data odnowienia.
- Sekcja "Customer": Karta z danymi klienta (link do profilu klienta).
- Sekcja "Current Plan": Karta z danymi planu (Nazwa, Cena, Interval).
- Sekcja "Actions" (Card lub osobny panel):
  - Przyciski: "Change Plan", "Pause Subscription" (jeśli aktywna), "Cancel Subscription" (czerwony).

KROK 4: Cancel Subscription Dialog (Zadanie 5.2.3)
Utwórz `src/features/subscriptions/components/CancelSubscriptionDialog.tsx`.
Wymagania:

- UI: `Dialog` (shadcn).
- Formularz:
  - `Reason`: Select (Too expensive, Switching provider, No longer needed, Other).
  - `Mode`: Radio Group ("End of current period" vs "Immediately").
- Warning: Wyświetl alert informujący o konsekwencjach (np. "Access will be revoked immediately").

KROK 5: Change Plan Dialog (Zadanie 5.2.5)
Utwórz `src/features/subscriptions/components/ChangePlanDialog.tsx`.
Wymagania:

- Wyświetla listę planów (z wykluczeniem obecnego).
- Po wybraniu wyświetla informację (jeśli dostępna z backendu) o zmianie ceny (proration).
- Confirm button: "Update Subscription".

### WYMAGANIA TECHNICZNE:

- **Reużywalność**: Staraj się nie kopiować kodu `PlanCard` czy `ClientSearch`. Jeśli musisz je dostosować (np. dodać tryb wyboru), zmodyfikuj oryginalne komponenty dodając opcjonalne propsy (np. `onClick`, `isSelectable`).
- **Wizard State**: Stan wizarda trzymaj lokalnie w komponencie rodzica (`useState` lub `useReducer`). Nie ma potrzeby wrzucać tego do globalnego store'a.

### OCZEKIWANY REZULTAT:

Kod wizarda (wszystkie kroki), strony szczegółów oraz dialogów akcji (Cancel, Change Plan).

=====================================================================================================================================================

### 6.1 Payment History

| #     | Zadanie                   | Priorytet | Status | Opis                                        |
| ----- | ------------------------- | --------- | ------ | ------------------------------------------- |
| 6.1.1 | 🔴 Payments page          | Krytyczne | ⬜     | /payments - historia płatności              |
| 6.1.2 | 🔴 PaymentTable component | Krytyczne | ⬜     | Tabela z amount, status, date, client       |
| 6.1.3 | 🔴 PaymentStatusBadge     | Krytyczne | ⬜     | Badge: Completed, Pending, Failed, Refunded |
| 6.1.4 | 🔴 Payment detail dialog  | Krytyczne | ⬜     | Szczegóły płatności w dialogu               |

**Blok 6.1 - Wymagania wejściowe**: Faza 5 (Subskrypcje)
**Blok 6.1 - Rezultat**: Przejrzysta historia transakcji finansowych

-------------------------------------------------------------------> PROMPT <---------------------------------------------------------------

Działaj jako Senior Frontend Developer / Fintech Specialist.
Rozpoczynamy Fazę 6: **Payments**.
Twoim zadaniem jest stworzenie widoku historii płatności. To moduł krytyczny dla zaufania użytkownika – dane finansowe muszą być prezentowane w sposób czytelny i bezbłędny.

### CEL GŁÓWNY:

Stworzyć tabelę historii płatności z możliwością filtrowania po statusie oraz podglądem szczegółów transakcji w oknie modalnym (Dialog).

### STRUKTURA PLIKÓW:

- Feature: `src/features/payments/`
- Komponenty: `src/features/payments/components/`
- Hooki: `src/features/payments/hooks/`
- Strona: `src/app/(dashboard)/payments/page.tsx`

### KROKI DO WYKONANIA:

KROK 1: Payment Hooks (Zadanie 6.1.4 - część logiczna)
Utwórz `src/features/payments/hooks/usePayments.ts`.
Wymagania:

- Zaimportuj `useGetPayments` z API.
- Stwórz wrapper obsługujący parametry: `page`, `pageSize`, `status` (Succeeded, Pending, Failed, Refunded), `clientId` (opcjonalnie do filtrowania per klient).
- Zwracane dane powinny zawierać: ID, Amount, Currency, Status, Date, Client Info (Name, Email), Payment Method Info (np. Brand: Visa, Last4: 4242).

KROK 2: PaymentStatusBadge (Zadanie 6.1.3)
Utwórz `src/features/payments/components/PaymentStatusBadge.tsx`.
Wymagania:

- Props: `status`.
- UI: `Badge` z shadcn.
- Kolory:
  - Succeeded / Completed -> Green (Success).
  - Pending / Processing -> Yellow/Blue.
  - Failed -> Red (Destructive) - to bardzo ważne, musi rzucać się w oczy.
  - Refunded -> Gray/Muted.

KROK 3: PaymentDetailDialog (Zadanie 6.1.4 - część UI)
Utwórz `src/features/payments/components/PaymentDetailDialog.tsx`.
Wymagania:

- Wyświetla szczegóły pojedynczej transakcji.
- Header: Kwota (duża czcionka) + StatusBadge.
- Content (Lista klucz-wartość):
  - Transaction ID (z przyciskiem "Copy").
  - Date & Time.
  - Customer (Link do profilu klienta).
  - Payment Method (np. ikona karty + \*\*\*\* 1234).
  - Invoice ID (jeśli dostępny).
  - Failure Reason (wyświetl TYLKO jeśli status to Failed, np. "Insufficient funds").

KROK 4: PaymentTable (Zadanie 6.1.2)
Utwórz `src/features/payments/components/PaymentTable.tsx`.
Wymagania:

- Kolumny:
  1. Amount (wyrównane do prawej, sformatowane helperem `formatCurrency`).
  2. Status.
  3. Client.
  4. Date (helper `formatDate`).
  5. Method (krótka info np. "Visa 4242").
  6. Actions ("View Details").
- Kliknięcie w "View Details" otwiera `PaymentDetailDialog`.

KROK 5: Payments Page (Zadanie 6.1.1)
Utwórz `src/app/(dashboard)/payments/page.tsx`.
Wymagania:

- Tytuł "Payment History".
- Toolbar: Prosty filtr statusu (Select) + ewentualnie wyszukiwarka.
- Content: `PaymentTable`.
- Pagination.

### WYMAGANIA TECHNICZNE:

- **Formatowanie Walut**: Absolutnie kluczowe jest użycie `formatCurrency(amount, currency)`. Upewnij się, że wiesz, w jakiej jednostce backend zwraca kwotę (zazwyczaj są to centy/grosze, więc może być potrzebne dzielenie przez 100 przed wyświetleniem, chyba że helper robi to sam - załóż standard, że API zwraca float lub helper obsługuje to poprawnie).
- **Bezpieczeństwo**: Nie wyświetlaj nigdy pełnych numerów kart, tylko `last4`.

### OCZEKIWANY REZULTAT:

Kod dla hooków, komponentów `Badge`, `Table`, `Dialog` oraz strony głównej płatności.

==================================================================================================================================================================

### 6.2 Payment Methods & Manual Payments

| #     | Zadanie                     | Priorytet | Status | Opis                                      |
| ----- | --------------------------- | --------- | ------ | ----------------------------------------- |
| 6.2.1 | 🔴 PaymentMethodForm        | Krytyczne | ⬜     | Formularz dodawania metody płatności      |
| 6.2.2 | 🔴 PaymentMethodList        | Krytyczne | ⬜     | Lista metod płatności klienta             |
| 6.2.3 | 🟡 Manual payment recording | Ważne     | ⬜     | Dialog do ręcznego wprowadzania płatności |
| 6.2.4 | 🟡 Refund dialog            | Ważne     | ⬜     | Dialog zwrotu z reason                    |

**Blok 6.2 - Wymagania wejściowe**: Blok 6.1 (Historia Płatności)
**Blok 6.2 - Rezultat**: Możliwość dodawania kart, rejestrowania przelewów i wykonywania zwrotów

-------------------------------------------------------------------> PROMPT <---------------------------------------------------------------
Działaj jako Senior Frontend Developer / Fintech Specialist.
Mamy historię płatności. Teraz dodajemy narzędzia operacyjne.

### UWAGA DOTYCZĄCA BEZPIECZEŃSTWA (PCI DSS):

W tej fazie **nie implementujemy** formularzy zbierających numery kart kredytowych. Jako SaaS musimy polegać na rozwiązaniach dostawcy (np. Stripe Elements / Payment Links), aby nie dotykać danych wrażliwych. W miejscu formularza karty stwórz Placeholder lub Integrację Mock.

### KROKI DO WYKONANIA:

KROK 1: Payment Operations Hooks (Zadanie 6.2.3, 6.2.4)
Rozbuduj `usePayments.ts` o mutacje:

1. `useCreatePaymentSession()`: Zamiast dodawać kartę bezpośrednio, backend zwróci URL do Stripe Checkout lub client_secret.
2. `useDeletePaymentMethod()`.
3. `useRecordManualPayment()`: Dla wpłat poza systemem (gotówka/przelew).

KROK 2: PaymentMethodList (Zadanie 6.2.2)
Utwórz `src/features/payments/components/PaymentMethodList.tsx`.
Wyświetla zamaskowane dane kart (Brand, \*\*\*\* 4242) pobrane z API.
Przycisk "Add Payment Method" powinien otwierać dialog z kroku 3.

KROK 3: AddPaymentMethod Dialog (Zadanie 6.2.1)
Utwórz `src/features/payments/components/AddPaymentMethodDialog.tsx`.
Wymagania:

- **Zamiast inputów na numer karty**: Wyświetl informację "You will be redirected to our secure payment provider" i przycisk "Proceed to Secure Checkout" LUB (jeśli preferujesz UI wewnątrz apki) stwórz kontener `<div id="stripe-elements-placeholder" />` z komentarzem, że tu zostanie wstrzyknięty iframe Stripe.
- Nie twórz inputów `Card Number`, `CVC` w czystym HTML/React!

KROK 4: ManualPaymentDialog & RefundDialog (Zadania 6.2.3, 6.2.4)
Zaimplementuj dialogi do ręcznego księgowania wpłat (dla płatności offline) oraz do zwrotów (Refund), zgodnie z wcześniejszym planem.

### OCZEKIWANY REZULTAT:

Bezpieczny interfejs zarządzania płatnościami, gotowy do integracji ze Stripe (bez ryzyka wycieku danych kart).

======================================================================================================================================================================

### 7.1 Analytics Dashboard

| #     | Zadanie                   | Priorytet  | Status | Opis                              |
| ----- | ------------------------- | ---------- | ------ | --------------------------------- |
| 7.1.1 | 🔴 Analytics page         | Krytyczne  | ⬜     | /analytics - główny dashboard     |
| 7.1.2 | 🔴 RevenueChart component | Krytyczne  | ⬜     | Wykres przychodów (recharts)      |
| 7.1.3 | 🔴 StatCards component    | Krytyczne  | ⬜     | MRR, ARR, Churn Rate, Active Subs |
| 7.1.4 | 🟡 DateRangePicker        | Ważne      | ⬜     | Wybór zakresu dat dla raportów    |
| 7.1.5 | 🟡 ClientGrowthChart      | Ważne      | ⬜     | Wykres wzrostu klientów           |
| 7.1.6 | 🟢 Export reports         | Opcjonalne | ⬜     | Export do CSV/Excel               |

**Blok 7.1 - Wymagania wejściowe**: Faza 6 (Płatności - mamy dane do analizy)
**Blok 7.1 - Rezultat**: Dashboard z wykresami i kluczowymi metrykami biznesowymi

-------------------------------------------------------------------> PROMPT <---------------------------------------------------------------

Działaj jako Senior Frontend Developer / Data Visualization Expert.
Wchodzimy w Fazę 7: **Analytics & Dashboard**.
Mamy w systemie dane o klientach, subskrypcjach i płatnościach. Teraz musimy je zagregować i wizualizować, aby dostarczyć użytkownikowi wartościowe insighty biznesowe.

Użyjemy biblioteki **`recharts`** do wykresów oraz `date-fns` do operacji na datach.

### CEL GŁÓWNY:

Stworzyć dashboard analityczny prezentujący kluczowe wskaźniki (KPI) takie jak MRR, ARR, Churn Rate oraz wizualizujący trendy przychodów i wzrostu bazy klientów.

### STRUKTURA PLIKÓW:

- Feature: `src/features/analytics/`
- Komponenty: `src/features/analytics/components/`
- Hooki: `src/features/analytics/hooks/`
- Strona: `src/app/(dashboard)/analytics/page.tsx`

### KROKI DO WYKONANIA:

KROK 1: Analytics Hooks (Zadanie 7.1.1 - logika)
Utwórz `src/features/analytics/hooks/useAnalytics.ts`.
Wymagania:

- Przyjmuje parametr `dateRange` ({ from: Date, to: Date }).
- Importuje `useGetAnalytics` (lub podobny endpoint) z API.
- Zwraca zagregowane dane:
  - `stats`: { mrr, arr, activeSubscriptions, churnRate, totalRevenue }.
  - `revenueHistory`: Tablica obiektów { date: string, amount: number } do wykresu.
  - `clientGrowth`: Tablica obiektów { date: string, totalClients: number, newClients: number }.

KROK 2: DateRangePicker (Zadanie 7.1.4)
Utwórz `src/features/analytics/components/DateRangePicker.tsx`.
Wymagania:

- UI: `Popover` + `Calendar` (shadcn/ui).
- Pozwala wybrać zakres dat (From - To).
- Posiada szybkie presety: "Last 7 days", "Last 30 days", "This Month", "Last Month", "This Year".

KROK 3: StatCards Component (Zadanie 7.1.3)
Utwórz `src/features/analytics/components/StatCards.tsx`.
Wymagania:

- Wyświetla grid 4 kart (Metric Cards).
- Każda karta zawiera:
  - Title (np. "Total Revenue").
  - Icon (lucide-react).
  - Value (duża czcionka, sformatowana waluta lub liczba).
  - Trend (opcjonalnie): np. "+12% from last month" (zielony) lub "-2%" (czerwony).

KROK 4: RevenueChart Component (Zadanie 7.1.2)
Utwórz `src/features/analytics/components/RevenueChart.tsx`.
Wymagania:

- Użyj `recharts`: `<ResponsiveContainer>`, `<AreaChart>`, `<XAxis>`, `<YAxis>`, `<Tooltip>`, `<Area>`.
- Oś X: Daty (sformatowane krótko, np. "Jan 21").
- Oś Y: Kwota.
- Tooltip: Musi formatować kwotę używając `formatCurrency`.
- Styl: Gradient pod linią wykresu (fill="url(#colorRevenue)").

KROK 5: ClientGrowthChart (Zadanie 7.1.5)
Utwórz `src/features/analytics/components/ClientGrowthChart.tsx`.
Wymagania:

- Użyj `recharts`: `<BarChart>` (słupkowy).
- Pokazuje liczbę nowych klientów w danym okresie.

KROK 6: Analytics Page (Zadanie 7.1.1, 7.1.6)
Utwórz `src/app/(dashboard)/analytics/page.tsx`.
Wymagania:

- State: `dateRange` (domyślnie "Last 30 days").
- Header: Title "Analytics" + `DateRangePicker` + Button "Export Report" (tylko UI lub prosta funkcja generująca CSV z danych wykresu).
- Content Layout:
  - Top: `StatCards` (przekaż dane z hooka).
  - Middle: Dwa duże wykresy obok siebie (lub jeden pod drugim na mobile): `RevenueChart` i `ClientGrowthChart`.
- Loading State: Szkielety kart i pusty kontener wykresów podczas ładowania.

### WYMAGANIA TECHNICZNE:

- **Recharts**: Upewnij się, że wykresy są responsywne (width="100%" w ResponsiveContainer).
- **Formatowanie**: Waluty i daty muszą być spójne z resztą aplikacji.

### OCZEKIWANY REZULTAT:

Kod dla hooka analitycznego, komponentu wyboru daty, kart statystyk, dwóch typów wykresów oraz strony spinającej całość.

===================================================================================================================================================

### 8.1 Testing

| #     | Zadanie             | Priorytet | Status | Opis                             |
| ----- | ------------------- | --------- | ------ | -------------------------------- |
| 8.1.1 | 🔴 Vitest setup     | Krytyczne | ⬜     | Konfiguracja unit tests          |
| 8.1.2 | 🔴 Component tests  | Krytyczne | ⬜     | Testy dla kluczowych komponentów |
| 8.1.3 | 🟡 Playwright setup | Ważne     | ⬜     | Konfiguracja E2E tests           |
| 8.1.4 | 🟡 E2E auth flow    | Ważne     | ⬜     | Test login/register flow         |
| 8.1.5 | 🟡 E2E client CRUD  | Ważne     | ⬜     | Test tworzenia/edycji klienta    |

**Blok 8.1 - Wymagania wejściowe**: Wszystkie poprzednie fazy (Aplikacja funkcjonalna)
**Blok 8.1 - Rezultat**: Skonfigurowane środowisko testowe i pokrycie krytycznych ścieżek

-------------------------------------------------------------------> PROMPT <---------------------------------------------------------------

Działaj jako Senior QA Engineer / SDET (Software Development Engineer in Test).
Mamy gotowe MVP aplikacji SaaS (Next.js 15). Teraz musimy wdrożyć automatyczne testy, aby zapewnić stabilność przed wdrożeniem produkcyjnym.

Podzielimy prace na dwie warstwy: **Unit/Component Testing** (Vitest) oraz **End-to-End Testing** (Playwright).

### STRUKTURA PLIKÓW:

- Unit Tests: Obok plików źródłowych (np. `Sidebar.test.tsx`) lub w `src/__tests__/`.
- E2E Tests: `e2e/`.
- Config: `vitest.config.ts`, `playwright.config.ts`.

### KROKI DO WYKONANIA:

KROK 1: Vitest Setup (Zadanie 8.1.1)

1. Zainstaluj zależności: `vitest`, `@testing-library/react`, `@vitejs/plugin-react`, `jsdom`, `@testing-library/dom`.
2. Utwórz `vitest.config.ts`:
   - Skonfiguruj środowisko `jsdom`.
   - Ustaw aliasy ścieżek (`@/*`) tak, aby pokrywały się z `tsconfig.json`.
   - Setup files: `src/test/setup.ts` (tutaj zaimportuj `@testing-library/jest-dom`).
3. Przygotuj helper `renderWithProviders` (w `src/test/utils.tsx`), który oplata testowany komponent w niezbędne providery: `QueryClientProvider` (z nowym klientem per test) i ewentualnie mock `SessionProvider`.

KROK 2: Component Tests (Zadanie 8.1.2)
Napisz przykładowe testy jednostkowe dla kluczowych elementów UI i Utils:

1. `src/shared/utils/formatters.test.ts`: Sprawdź czy `formatCurrency` i `formatDate` działają poprawnie dla skrajnych przypadków.
2. `src/features/payments/components/PaymentStatusBadge.test.tsx`: Sprawdź, czy renderuje poprawny tekst i klasę koloru w zależności od propa `status`.
3. `src/shared/components/layout/Sidebar.test.tsx`: Sprawdź, czy renderuje linki nawigacyjne. (Będziesz musiał zamockować `usePathname` z `next/navigation` - użyj `vi.mock`).

KROK 3: Playwright Setup (Zadanie 8.1.3)

1. Wygeneruj konfigurację dla Playwright (`playwright.config.ts`).
   - BaseURL: `http://localhost:3000`.
   - Ustaw nagrywanie śladów (trace) na "on-first-retry".
2. Dodaj skrypt do package.json: `"test:e2e": "playwright test"`.

KROK 4: E2E Critical Flows (Zadania 8.1.4, 8.1.5)
Utwórz plik `e2e/core-flows.spec.ts`.
Zaimplementuj dwa kluczowe scenariusze ("Happy Path"):

1. **Auth Flow**:
   - Wejdź na `/login`.
   - Wpisz poprawne dane (użyj danych testowych, np. z `.env.test`).
   - Kliknij "Sign In".
   - Oczekuj przekierowania na `/dashboard` (lub sprawdź obecność nagłówka "Dashboard").
2. **Client CRUD Flow**:
   - (Zakładając, że jesteś zalogowany - możesz użyć `test.use({ storageState: ... })` lub logować się w `beforeEach`).
   - Przejdź na `/clients/new`.
   - Wypełnij formularz (Imię, Email, Status).
   - Kliknij "Save".
   - Sprawdź, czy zostałeś przekierowany na `/clients` i czy nowy klient widnieje na liście.

### WYMAGANIA TECHNICZNE:

- **Mocking Next.js**: W testach Vitest pamiętaj, że komponenty używające `useRouter`, `usePathname` czy `useSearchParams` wysypią się bez mockowania. Przygotuj prosty mock w `setup.ts` lub w samym teście.
- **Izolacja**: Testy E2E nie powinny polegać na danych z poprzednich testów (w idealnym świecie resetujemy bazę, ale w tym kroku skupmy się na unikalnych nazwach klientów, np. `Client ${Date.now()}`, aby uniknąć konfliktów).

### OCZEKIWANY REZULTAT:

Pliki konfiguracyjne (`vitest.config.ts`, `playwright.config.ts`), plik setupu testów, helper `renderWithProviders` oraz kod przykładowych testów (Unit i E2E).

=================================================================================================================================================================

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

-------------------------------------------------------------------> PROMPT <---------------------------------------------------------------

Działaj jako Senior Frontend Developer / Tech Lead.
Aplikacja jest funkcjonalna i przetestowana. Przechodzimy do Fazy 8.2: **Polish & Optimization**.
Naszym celem jest upewnienie się, że kod jest najwyższej jakości, aplikacja jest dostępna dla wszystkich użytkowników (A11y) i zoptymalizowana pod kątem wydajności.

### CEL GŁÓWNY:

Przeprowadzić audyt kodu, skonfigurować narzędzia do analizy wydajności oraz przygotować dokumentację projektu.

### KROKI DO WYKONANIA:

KROK 1: TypeScript & Linting Strictness (Zadanie 8.2.1)

1. Sprawdź plik `tsconfig.json`. Upewnij się, że `noImplicitAny` jest na `true`.
2. Dodaj skrypt do `package.json`: `"type-check": "tsc --noEmit"`.
3. Skonfiguruj ESLint, aby wyłapywał typy `any`. W pliku `eslint.config.mjs` (lub .rc) dodaj regułę:
   - `@typescript-eslint/no-explicit-any`: "warn" (lub "error" dla strict mode).
   - `@typescript-eslint/no-unused-vars`: "error".

KROK 2: Accessibility (A11y) Setup (Zadanie 8.2.2)

1. Zainstaluj plugin `eslint-plugin-jsx-a11y`.
2. Dodaj go do konfiguracji ESLint, aby automatycznie wykrywał brakujące `alt` w obrazkach, brakujące `aria-label` w przyciskach (szczególnie tych z samą ikoną, np. w `Sidebar` czy `ActionsDropdown`).
3. Stwórz prosty dokument `A11Y_CHECKLIST.md` z punktami do ręcznego sprawdzenia:
   - Czy wszystkie formularze mają etykiety (`label`) powiązane z inputami?
   - Czy można poruszać się po stronie używając tylko klawisza TAB?
   - Czy focus jest widoczny na elementach aktywnych?

KROK 3: Performance Optimization (Zadanie 8.2.3)

1. Zainstaluj `@next/bundle-analyzer`.
2. Skonfiguruj `next.config.ts`, aby włączał analyzer zmienną środowiskową (np. `ANALYZE=true`).
3. Dodaj skrypt do `package.json`: `"analyze": "cross-env ANALYZE=true npm run build"`.
4. Stwórz komponent `Image` wrapper (opcjonalnie), który wymusza używanie `next/image` zamiast `<img>` w celu optymalizacji obrazów.

KROK 4: Documentation (Zadanie 8.2.5)
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

===================================================================================================================================================

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

-------------------------------------------------------------------> PROMPT <---------------------------------------------------------------

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

KROK 1: Portal Layout & Guard (Zadania 9.1.1, 9.1.2)

1. Utwórz `src/features/client-portal/components/PortalGuard.tsx`.
   - Działa analogicznie do `TenantGuard`, ale wymaga roli `Client`.
   - Jeśli user ma rolę `Provider` -> przekieruj na `/dashboard`.
   - Jeśli user nie jest zalogowany -> `/login`.
2. Utwórz `src/app/(portal)/layout.tsx`.
   - Prosty layout: Navbar na górze (Logo + UserMenu), wycentrowana zawartość (max-w-4xl).
   - Brak bocznego Sidebara.
   - Owiń `children` w `PortalGuard`.

KROK 2: Portal Hooks (Zadanie 9.1.3)
Utwórz `src/features/client-portal/hooks/usePortal.ts`.
Wymagania:

- `useMySubscriptions()`: Pobiera subskrypcje zalogowanego klienta (filtrowanie po stronie API dla zalogowanego usera).
- `useMyInvoices()`: Pobiera historię faktur.
- `usePortalAction()`: Wrapper na mutacje (np. `cancelSubscription`, `updatePaymentMethod`).

KROK 3: Client Dashboard (Zadanie 9.1.3)
Utwórz `src/app/(portal)/portal/page.tsx`.
Wymagania:

- **Sekcja "Current Plan"**: Wyświetla dużą kartę z aktywną subskrypcją (Plan Name, Price, Renewal Date, Status Badge).
- **Action Buttons**:
  - "Update Payment Method" (otwiera dialog - użyj placeholdera lub komponentu PaymentMethodForm z Fazy 6, ale dostosowanego do kontekstu klienta).
  - "Cancel Subscription" (otwiera dialog potwierdzenia).

KROK 4: Invoices List (Zadanie 9.1.4)
Utwórz `src/features/client-portal/components/ClientInvoicesList.tsx`.
Wymagania:

- Prosta tabela lub lista: Data, Kwota, Status, "Download PDF" (ikona).
- Dodaj ten komponent na dole Dashboardu (`page.tsx`).

### WYMAGANIA TECHNICZNE:

- **Next.js 15 Async Params**: Pamiętaj, że w plikach `page.tsx` propsy `params` i `searchParams` są asynchroniczne (Promise). Jeśli będziesz ich potrzebować, użyj `await`.
- **Reużywalność**: Możesz importować komponenty UI (Button, Card, Badge) z `shared/ui`.

### OCZEKIWANY REZULTAT:

Kod layoutu portalu, Guarda, hooków oraz strony głównej portalu z listą faktur.
