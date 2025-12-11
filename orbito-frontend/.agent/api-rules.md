# 🚨 KRYTYCZNE REGUŁY API - PRZECZYTAJ PRZED KAŻDYM BLOKIEM

> **Ten dokument jest OBOWIĄZKOWY.** Agent MUSI przestrzegać tych reguł.
> Złamanie którejkolwiek reguły = blok NIE JEST ukończony.

---

## 🚫 ABSOLUTNE ZAKAZY

### 1. ZERO HARDCODED DATA

```tsx
// ❌ ZABRONIONE - NIGDY TAK NIE RÓB
<p className="text-3xl font-bold">0</p>
<p>Total: $0.00</p>
const clients = []; // pusta tablica zamiast API

// ✅ WYMAGANE - ZAWSZE UŻYWAJ HOOKÓW API
const { data: clients, isLoading } = useGetApiClients();
<p className="text-3xl font-bold">{clients?.length ?? 0}</p>
```

### 2. ZERO MOCK FUNKCJI

```tsx
// ❌ ZABRONIONE
const handleSubmit = async (data) => {
  console.log("TODO: implement API call", data);
  toast.success("Saved!"); // KŁAMSTWO - nic nie zapisano
};

// ✅ WYMAGANE
const mutation = usePostApiClients();
const handleSubmit = async (data) => {
  await mutation.mutateAsync(data);
  toast.success("Saved!");
};
```

### 3. ZERO PLACEHOLDER COMMENTS

```tsx
// ❌ ZABRONIONE
// TODO: Add API call later
// TODO: Implement in next block
// Will be added when backend is ready

// ✅ WYMAGANE
// Jeśli endpoint nie istnieje - POWIEDZ MI i NIE KOŃCZ BLOKU
```

### 4. ZERO POMINIĘĆ AUTH

```tsx
// ❌ ZABRONIONE - zapomnienie o ustawieniu tokenu
axiosInstance.interceptors.request.use((config) => {
  // "będzie dodane później" - NIE!
  return config;
});

// ✅ WYMAGANE - auth interceptor MUSI być w bloku 1.1
axiosInstance.interceptors.request.use(async (config) => {
  if (typeof window !== "undefined") {
    const { getSession } = await import("next-auth/react");
    const session = await getSession();
    if (session?.accessToken) {
      config.headers.Authorization = `Bearer ${session.accessToken}`;
    }
  }
  return config;
});
```

---

## ✅ WYMAGANE WZORCE

### Pattern 1: Każdy komponent z danymi MUSI używać hooków Orval

```tsx
// src/features/clients/components/ClientsList.tsx
"use client";

import { useGetApiClients } from "@/core/api/generated/clients/clients";
import { Skeleton } from "@/shared/ui/skeleton";

export function ClientsList() {
  const { data: clients, isLoading, error } = useGetApiClients();

  if (isLoading) return <Skeleton className="h-40" />;
  if (error) return <div>Error: {error.message}</div>;
  if (!clients?.length) return <div>No clients found</div>;

  return (
    <ul>
      {clients.map((client) => (
        <li key={client.id}>{client.name}</li>
      ))}
    </ul>
  );
}
```

### Pattern 2: Każda mutacja MUSI używać hooków Orval

```tsx
// src/features/clients/components/CreateClientForm.tsx
"use client";

import { usePostApiClients } from "@/core/api/generated/clients/clients";
import { useQueryClient } from "@tanstack/react-query";

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

  return (/* form */);
}
```

### Pattern 3: Loading i Error states są OBOWIĄZKOWE

```tsx
// ❌ ZABRONIONE - brak obsługi loading/error
const { data } = useGetApiClients();
return <div>{data.map(...)}</div>; // CRASH jeśli data undefined

// ✅ WYMAGANE
const { data, isLoading, error } = useGetApiClients();

if (isLoading) return <ClientsListSkeleton />;
if (error) return <ErrorMessage error={error} />;
if (!data?.length) return <EmptyState message="No clients" />;

return <div>{data.map(...)}</div>;
```

---

## 🔍 WERYFIKACJA PO KAŻDYM BLOKU

### Checklist (WSZYSTKIE punkty muszą być ✅)

- [ ] **Network Tab**: Otwórz DevTools → Network → odśwież stronę
  - [ ] Widzę requesty do `/api/...` (nie tylko do `_next/`)
  - [ ] Requesty mają status `200` (nie `401` Unauthorized)
  - [ ] Requesty mają header `Authorization: Bearer ...`
- [ ] **Console**: Brak błędów w konsoli przeglądarki
  - [ ] Brak `TypeError: Cannot read property of undefined`
  - [ ] Brak `401 Unauthorized`
- [ ] **UI**: Dane są wyświetlane
  - [ ] Widzę PRAWDZIWE dane z bazy (nie "0", nie puste listy)
  - [ ] Loading spinner pokazuje się podczas ładowania
  - [ ] Error message pokazuje się przy błędach

### Jak sprawdzić czy auth działa?

```bash
# 1. Zaloguj się w aplikacji
# 2. Otwórz DevTools → Network
# 3. Znajdź dowolny request do /api/
# 4. Sprawdź Request Headers:

Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

# Jeśli NIE MA tego headera = auth interceptor nie działa!
```

---

## 📋 ENDPOINTY API (Backend Orbito)

### Auth

- `POST /api/Account/login` → `{ token, user }`
- `POST /api/Account/register` → `{ token, user }`

### Clients

- `GET /api/Clients` → `PagedResult<ClientDto>`
- `GET /api/Clients/{id}` → `ClientDto`
- `POST /api/Clients` → `ClientDto`
- `PUT /api/Clients/{id}` → `ClientDto`
- `DELETE /api/Clients/{id}` → `void`

### Plans

- `GET /api/Plans` → `PlanDto[]`
- `GET /api/Plans/{id}` → `PlanDto`
- `POST /api/Plans` → `PlanDto`
- `PUT /api/Plans/{id}` → `PlanDto`
- `DELETE /api/Plans/{id}` → `void`

### Subscriptions

- `GET /api/Subscriptions` → `PagedResult<SubscriptionDto>`
- `GET /api/Subscriptions/{id}` → `SubscriptionDto`
- `POST /api/Subscriptions` → `SubscriptionDto`
- `PUT /api/Subscriptions/{id}/cancel` → `void`
- `PUT /api/Subscriptions/{id}/pause` → `void`

### Team

- `GET /api/Team` → `TeamMemberDto[]`
- `POST /api/Team/invite` → `InvitationDto`
- `DELETE /api/Team/{id}` → `void`

### Payments

- `GET /api/Payments` → `PagedResult<PaymentDto>`
- `GET /api/Payments/{id}` → `PaymentDto`

---

## ⚠️ CO ROBIĆ GDY ENDPOINT NIE ISTNIEJE?

1. **NIE twórz mock funkcji**
2. **NIE używaj hardcoded data**
3. **POWIEDZ MI** że endpoint nie istnieje w Swagger
4. **POCZEKAJ** na moją decyzję (może trzeba dodać w backendzie)

```tsx
// ❌ NIE RÓB TAK
const getStatistics = () => {
  // TODO: endpoint doesn't exist yet
  return { totalClients: 0, revenue: 0 };
};

// ✅ ZRÓB TAK
// STOP! Endpoint GET /api/Statistics nie istnieje w Swagger.
// Potrzebuję tego endpointu do wyświetlenia dashboardu.
// Opcje:
// 1. Dodać endpoint w backendzie
// 2. Użyć istniejących endpointów (np. GET /api/Clients count)
// 3. Pominąć tę sekcję na razie
//
// Która opcja wybierasz?
```

---

## 🔄 PRZYPOMNIENIE: Auth Interceptor

**KRYTYCZNE**: Auth interceptor MUSI być dodany w bloku 1.1!

Lokalizacja: `src/core/api/client.ts`

```typescript
import axios from "axios";

const axiosInstance = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL,
});

// Response interceptor - Result<T> unwrapping
axiosInstance.interceptors.response.use(
  (response) => {
    const result = response.data;
    if (result && typeof result === "object" && "isSuccess" in result) {
      if (result.isSuccess) {
        return { ...response, data: result.value };
      } else {
        return Promise.reject(new Error(result.error || "Unknown error"));
      }
    }
    return response;
  },
  (error) => Promise.reject(error)
);

// 🚨 AUTH INTERCEPTOR - KRYTYCZNE!
axiosInstance.interceptors.request.use(
  async (config) => {
    // Tylko w przeglądarce (nie podczas SSR)
    if (typeof window !== "undefined") {
      const { getSession } = await import("next-auth/react");
      const session = await getSession();
      if (session?.accessToken) {
        config.headers.Authorization = `Bearer ${session.accessToken}`;
      }
    }
    return config;
  },
  (error) => Promise.reject(error)
);

export default axiosInstance;
```

---

## 📝 PODSUMOWANIE

| Reguła             | Opis                                       |
| ------------------ | ------------------------------------------ |
| **NO MOCKS**       | Żadnych funkcji z `console.log("TODO")`    |
| **NO HARDCODE**    | Żadnych `0`, `[]`, `"placeholder"`         |
| **NO SKIP AUTH**   | Auth interceptor w bloku 1.1, nie później  |
| **ALWAYS VERIFY**  | Network tab + Console + UI po każdym bloku |
| **ASK IF MISSING** | Brak endpointu? Pytaj, nie mockuj          |
