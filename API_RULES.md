# 🚨 API Integration Rules - OBOWIĄZKOWE

> **PRZECZYTAJ TO PRZED ROZPOCZĘCIEM JAKIEGOKOLWIEK BLOKU!**
>
> Te zasady powstały po naprawie krytycznych bugów w integracji API.
> Ignorowanie ich prowadzi do 401 Unauthorized i hardcoded danych.

---

## ❌ ABSOLUTNIE ZABRONIONE

### 🚫 NIGDY nie używaj:

#### 1. Hardcoded Data
```typescript
// ❌ ŹLE - hardcoded wartości
const totalClients = 0;
const revenue = "$0";
const items = [];
const user = { name: "Test User" };

// ✅ DOBRZE - dane z API
const { data } = useGetApiClients();
const totalClients = data?.totalCount ?? 0;
```

#### 2. Mock Functions
```typescript
// ❌ ŹLE - mock funkcja
function getClients() {
  console.log('TODO: call API');
  return [];
}

// ✅ DOBRZE - hook z Orval
const { data } = useGetApiClients();
```

#### 3. Placeholder Components
```typescript
// ❌ ŹLE - pusty komponent
export default function ClientsPage() {
  return <h1>Clients</h1>;
}

// ✅ DOBRZE - pełna implementacja z API
export default function ClientsPage() {
  const { data, isLoading, error } = useGetApiClients();
  // ... obsługa stanów ...
}
```

#### 4. TODO Comments (bez implementacji)
```typescript
// ❌ ŹLE - TODO zostawione
// TODO: add auth interceptor
// TODO: call real API

// ✅ DOBRZE - implementacja od razu
axiosInstance.interceptors.request.use(async (config) => {
  // ... implementacja ...
});
```

---

## ✅ ZAWSZE WYMAGANE

### 1. Import Hooków z Orval

```typescript
// ✅ DOBRZE - import wygenerowanych hooków
import { useGetApiClients } from "@/core/api/generated/clients/clients";
import { usePostApiClients } from "@/core/api/generated/clients/clients";
import type { ClientDto } from "@/core/api/generated/models";
```

### 2. Obsługa Stanów

```typescript
// ✅ DOBRZE - wszystkie 3 stany obsłużone
const { data, isLoading, error } = useGetApiClients();

if (isLoading) {
  return <Skeleton />;
}

if (error) {
  return <ErrorMessage error={error} />;
}

return <DataTable data={data?.items ?? []} />;
```

### 3. Type Assertions (tylko dla Orval bugs)

```typescript
// ✅ OK - workaround dla Orval bug (customInstance<void>)
const { data } = useGetApiClients() as {
  data: ClientDtoPaginatedList | undefined;
  isLoading: boolean;
};

// ❌ ŹLE - ukrywanie prawdziwych błędów
const data = response as any;
```

### 4. Formatowanie Danych

```typescript
// ✅ DOBRZE - używaj utility functions
import { formatCurrency, formatDate } from "@/shared/lib/formatters";

<p>{formatCurrency(payment.amount, payment.currency)}</p>
<p>{formatDate(client.createdAt)}</p>
```

---

## 🔍 WERYFIKACJA API PRZED passes: true

### Checklist Obowiązkowy

Przed ustawieniem `"passes": true` w `feature_list.json`:

#### 1. Code Review
- [ ] Komponent importuje hooki z `@/core/api/generated` (sprawdź imports)
- [ ] ZERO hardcoded wartości: szukaj `0`, `"$0"`, `[]`, `"placeholder"`
- [ ] ZERO komentarzy TODO związanych z API
- [ ] Loading states używają `<Skeleton>` z shadcn/ui
- [ ] Error handling wyświetla `error.message`

#### 2. DevTools Network (KRYTYCZNE!)

**To jest NAJWAŻNIEJSZY krok - jeśli pominiesz, wprowadzisz bugi!**

1. Otwórz DevTools → Network tab
2. Zaloguj się do aplikacji
3. Nawiguj do strony która powinna wywoływać API
4. Sprawdź:

```
✅ MUSI BYĆ:
- Request URL: http://localhost:5211/api/Clients (lub inny endpoint)
- Request Headers:
  Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
- Status Code: 200 OK
- Response: { items: [...], totalCount: 5 }

❌ FAIL jeśli:
- Status: 401 Unauthorized → brak tokena JWT!
- Brak requestów do /api/* → komponent nie wywołuje API!
- Response: null lub undefined → endpoint nie istnieje!
```

#### 3. TypeScript & Linting

```bash
npm run typecheck  # MUSI: zero błędów
npm run lint       # MUSI: zero warnings
```

#### 4. Visual Testing

- [ ] Loading spinner pojawia się przy pierwszym ładowaniu
- [ ] Dane wyświetlają się po załadowaniu (NIE "0" ani puste)
- [ ] Error state działa (wyłącz backend i sprawdź)
- [ ] Formatowanie jest poprawne (daty w formacie DD.MM.YYYY, kwoty z PLN)

---

## 🚨 Specjalne Przypadki

### Auth Interceptor (Blok 1.1)

**KRYTYCZNE**: Auth interceptor MUSI być dodany w bloku 1.1 (NextAuth Configuration).

```typescript
// src/core/api/client.ts
import { getSession } from "next-auth/react";

axiosInstance.interceptors.request.use(async (config) => {
  if (typeof window !== "undefined") {
    const session = await getSession();
    if (session?.accessToken) {
      config.headers.Authorization = `Bearer ${session.accessToken}`;
    }
  }
  return config;
});
```

**Weryfikacja**: Po implementacji, KAŻDY request do /api/* MUSI mieć header `Authorization: Bearer ...`

### Brakujący Endpoint

```typescript
// ❌ ŹLE - mockujesz endpoint
const mockAnalytics = { revenue: 0 };

// ✅ DOBRZE - pytasz o endpoint
// "Endpoint GET /api/Analytics/dashboard nie istnieje w Swagger.
// Czy mam:
// 1. Użyć innego endpointu?
// 2. Poczekać na implementację backendu?
// 3. Pominąć tę funkcjonalność?"
```

### Orval Type Generation Bug

Jeśli Orval generuje `customInstance<void>`:

```typescript
// Backend issue - brak ProducesResponseType attribute
// Workaround:
const { data } = useGetApiClients() as {
  data: ClientDtoPaginatedList | undefined;
};

// ZGŁOŚ to jako backend issue!
```

---

## 📋 Przykładowe Implementacje

### ✅ Poprawny Komponent

```typescript
"use client";

import { useGetApiClients } from "@/core/api/generated/clients/clients";
import { Card, CardHeader, CardTitle, CardContent } from "@/shared/ui/card";
import { Skeleton } from "@/shared/ui/skeleton";
import type { ClientDtoPaginatedList } from "@/core/api/generated/models";

export default function ClientsPage() {
  const { data, isLoading, error } = useGetApiClients({
    pageNumber: 1,
    pageSize: 50,
  }) as {
    data: ClientDtoPaginatedList | undefined;
    isLoading: boolean;
    error: Error | null;
  };

  if (error) {
    return (
      <Card>
        <CardContent className="pt-6">
          <p className="text-destructive">Failed to load: {error.message}</p>
        </CardContent>
      </Card>
    );
  }

  return (
    <div>
      <h1>Clients ({data?.totalCount ?? 0})</h1>
      {isLoading ? (
        <Skeleton className="h-12 w-full" />
      ) : (
        <ul>
          {data?.items?.map((client) => (
            <li key={client.id}>{client.fullName}</li>
          ))}
        </ul>
      )}
    </div>
  );
}
```

### ❌ Błędny Komponent (DO NOT DO THIS!)

```typescript
export default function ClientsPage() {
  // ❌ hardcoded data
  const totalClients = 0;
  const clients = [];

  return (
    <div>
      <h1>Clients ({totalClients})</h1>
      {/* ❌ brak loading state */}
      {/* ❌ brak error handling */}
      <ul>
        {clients.map((c) => (
          <li>{c.name}</li>
        ))}
      </ul>
    </div>
  );
}
```

---

## 🎯 Quick Reference

| Pytanie | Odpowiedź |
|---------|-----------|
| Czy mogę użyć `const data = []`? | ❌ NIE - tylko API |
| Czy mogę zostawić TODO? | ❌ NIE - implementuj lub pytaj |
| Endpoint nie istnieje - co robić? | ✅ PYTAJ - nie mockuj |
| Orval wygenerował `void` - co robić? | ✅ Type assertion + zgłoś backend issue |
| Jak sprawdzić czy API działa? | ✅ DevTools Network → 200 OK + Authorization header |
| Czy mogę pominąć error handling? | ❌ NIE - zawsze obsługuj error |

---

**Remember**: Jeśli nie widzisz requestów w Network tab lub widzisz 401 - STOP i napraw to zanim ustawisz passes: true!
