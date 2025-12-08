# Analiza SPA Behavior - Orbito Frontend

## ✅ Konfiguracja SPA (Single Page Application)

### 1. Client Components - Layout nie przeładowuje się

Wszystkie komponenty layoutu są oznaczone jako `"use client"`:

- ✅ `DashboardShell.tsx` - "use client"
- ✅ `Sidebar.tsx` - "use client" + używa `usePathname` hook
- ✅ `Header.tsx` - "use client"
- ✅ `UserMenu.tsx` - "use client"

**Rezultat**: Layout (Sidebar + Header) **NIE będzie się przeładowywał** podczas nawigacji między stronami.

### 2. Nawigacja używa Next.js Link

Sidebar używa komponentu `Link` z `next/link`:

```tsx
import Link from "next/link";

<Link href="/dashboard/clients">Clients</Link>
```

**Rezultat**: Nawigacja jest **client-side** - bez pełnego przeładowania strony.

### 3. Poprawna struktura routingu

```
src/app/
├── (dashboard)/
│   ├── layout.tsx          # Dashboard layout (SessionProvider + TenantProvider + DashboardShell)
│   └── dashboard/
│       ├── page.tsx        # /dashboard
│       ├── team/
│       │   └── page.tsx    # /dashboard/team
│       ├── clients/
│       │   └── page.tsx    # /dashboard/clients
│       ├── plans/
│       │   └── page.tsx    # /dashboard/plans
│       ├── subscriptions/
│       │   └── page.tsx    # /dashboard/subscriptions
│       ├── payments/
│       │   └── page.tsx    # /dashboard/payments
│       └── analytics/
│           └── page.tsx    # /dashboard/analytics
```

**Uwaga**: Route group `(dashboard)` NIE dodaje segmentu do URL.

### 4. Server Components dla stron

Strony dashboard (`page.tsx`) są Server Components (domyślnie):

- ✅ Pozwala na server-side data fetching
- ✅ Mniejszy bundle JavaScript
- ✅ Podczas nawigacji Next.js wykonuje tylko RSC request (nie pełne przeładowanie)

## 🎯 Jak to działa w praktyce

1. **Pierwsze załadowanie** (`/login` → `/dashboard`):
   - Pełne załadowanie HTML + CSS + JS
   - Sidebar i Header renderują się po stronie klienta

2. **Nawigacja między stronami** (`/dashboard` → `/dashboard/clients`):
   - ❌ **NIE** przeładowuje się cała strona
   - ❌ **NIE** przeładowuje się Sidebar
   - ❌ **NIE** przeładowuje się Header
   - ✅ **TYLKO** zmienia się zawartość `<main>` (children)
   - ✅ Next.js wykonuje RSC request (fetch danych dla nowej strony)
   - ✅ Active state w Sidebar aktualizuje się (dzięki `usePathname`)

3. **Zachowanie przycisków**:
   - ✅ Kliknięcie w link w Sidebar → client-side navigation
   - ✅ Brak "białego mignięcia" ekranu
   - ✅ Płynne przejście między stronami

## 🧪 Jak przetestować

### Test 1: Sprawdzenie czy layout się nie przeładowuje

1. Otwórz DevTools → Console
2. W konsoli wpisz:
   ```js
   window.layoutRenderCount = 0;
   ```
3. Dodaj `console.log` w `DashboardShell.tsx`:
   ```tsx
   export function DashboardShell({ children }: DashboardShellProps) {
     React.useEffect(() => {
       console.log('🔄 DashboardShell RENDERED');
     }, []);
     // ...
   }
   ```
4. Klikaj między stronami dashboard
5. Sprawdź konsolę - `DashboardShell RENDERED` powinno pojawić się **tylko raz**

### Test 2: Sprawdzenie Network tab

1. Otwórz DevTools → Network
2. Zaznacz "Preserve log"
3. Kliknij w link w Sidebar (np. `/dashboard/clients`)
4. Sprawdź Network:
   - ❌ **NIE** powinno być request do pełnego HTML dokumentu
   - ✅ **POWINIEN** być request do RSC payload (text/x-component)

### Test 3: Visual test

1. Dodaj animację do Sidebar:
   ```css
   .sidebar {
     animation: pulse 2s infinite;
   }
   ```
2. Klikaj między stronami
3. Animacja **nie powinna się resetować** - oznacza to, że Sidebar nie jest re-renderowany

## 📊 Rezultat

✅ **Aplikacja działa jako prawdziwa SPA**
✅ **Layout nie przeładowuje się**
✅ **Nawigacja jest płynna i szybka**
✅ **Active states w Sidebar działają poprawnie**

## 🔍 Potencjalne problemy

1. **Jeśli layout się przeładowuje**:
   - Sprawdź czy wszystkie komponenty layoutu mają `"use client"`
   - Sprawdź czy używasz `Link` z `next/link` (nie `<a href>`)

2. **Jeśli active states nie działają**:
   - Sprawdź czy `usePathname` jest w client component
   - Sprawdź logikę `isActive` w Sidebar

3. **Jeśli dane nie aktualizują się**:
   - Sprawdź czy używasz React Query / TanStack Query
   - Sprawdź czy cache jest invalidowany po mutacjach

## 📝 Następne kroki

1. Dodać React Query Provider (Blok 2.2)
2. Dodać error boundaries
3. Dodać loading states (Suspense)
4. Zaimplementować rzeczywiste strony z danymi z API
