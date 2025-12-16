## ✅ NAPRAWIONE (2025-12-15)

## Error Type

Runtime TypeError

## Error Message

subscriptionsData?.filter is not a function

## Root Cause

Backend zwraca `PaginatedList<SubscriptionDto>` z polem `items`, ale frontend próbował użyć `.filter()` bezpośrednio na obiekcie paginacji zamiast na polu `items`.

## Rozwiązanie

1. Zmieniono typ danych `subscriptionsData` na:
   ```typescript
   data: { items: Array<{ status: string }>; totalCount: number } | undefined
   ```

2. Poprawiono dostęp do danych:
   ```typescript
   // Przed (❌):
   subscriptionsData?.filter((s) => s.status === "Active").length ?? 0

   // Po (✅):
   subscriptionsData?.items?.filter((s) => s.status === "Active").length ?? 0
   ```

3. Dodano obsługę błędów dla PaymentMetrics endpoint (który nie ma implementacji):
   ```typescript
   {statsError ? (
     <p className="text-sm text-muted-foreground">
       Stats temporarily unavailable
     </p>
   ) : (
     <p className="text-3xl font-bold">
       {formatCurrency(monthlyRevenue ?? 0, currency)}
     </p>
   )}
   ```

## Zmienione pliki

✅ [orbito-frontend/src/app/(dashboard)/dashboard/page.tsx](orbito-frontend/src/app/(dashboard)/dashboard/page.tsx)

## Status

✅ Dashboard teraz poprawnie wyświetla:
- Total Clients (z API)
- Active Subscriptions (z API - teraz działa!)
- Monthly Revenue (graceful degradation gdy endpoint nie działa)

## Stack Trace (dla referencji)

    at DashboardPage (file://C:/Users/Michał/source/repos/Orbito/orbito-frontend/.next/dev/static/chunks/_a40ff414._.js:1545:52)
    at Object.react_stack_bottom_frame (file://C:/Users/Michał/source/repos/Orbito/orbito-frontend/.next/dev/static/chunks/node_modules_next_dist_compiled_react-dom_1e674e59._.js:14816:24)
    at renderWithHooks (file://C:/Users/Michał/source/repos/Orbito/orbito-frontend/.next/dev/static/chunks/node_modules_next_dist_compiled_react-dom_1e674e59._.js:4645:24)
    at updateFunctionComponent (file://C:/Users/Michał/source/repos/Orbito/orbito-frontend/.next/dev/static/chunks/node_modules_next_dist_compiled_react-dom_1e674e59._.js:6106:21)
    at beginWork (file://C:/Users/Michał/source/repos/Orbito/orbito-frontend/.next/dev/static/chunks/node_modules_next_dist_compiled_react-dom_1e674e59._.js:6702:24)
    at runWithFiberInDEV (file://C:/Users/Michał/source/repos/Orbito/orbito-frontend/.next/dev/static/chunks/node_modules_next_dist_compiled_react-dom_1e674e59._.js:959:74)
    at performUnitOfWork (file://C:/Users/Michał/source/repos/Orbito/orbito-frontend/.next/dev/static/chunks/node_modules_next_dist_compiled_react-dom_1e674e59._.js:9556:97)
    at workLoopSync (file://C:/Users/Michał/source/repos/Orbito/orbito-frontend/.next/dev/static/chunks/node_modules_next_dist_compiled_react-dom_1e674e59._.js:9450:40)
    at renderRootSync (file://C:/Users/Michał/source/repos/Orbito/orbito-frontend/.next/dev/static/chunks/node_modules_next_dist_compiled_react-dom_1e674e59._.js:9434:13)
    at performWorkOnRoot (file://C:/Users/Michał/source/repos/Orbito/orbito-frontend/.next/dev/static/chunks/node_modules_next_dist_compiled_react-dom_1e674e59._.js:9099:47)
    at performSyncWorkOnRoot (file://C:/Users/Michał/source/repos/Orbito/orbito-frontend/.next/dev/static/chunks/node_modules_next_dist_compiled_react-dom_1e674e59._.js:10232:9)
    at flushSyncWorkAcrossRoots_impl (file://C:/Users/Michał/source/repos/Orbito/orbito-frontend/.next/dev/static/chunks/node_modules_next_dist_compiled_react-dom_1e674e59._.js:10148:316)
    at processRootScheduleInMicrotask (file://C:/Users/Michał/source/repos/Orbito/orbito-frontend/.next/dev/static/chunks/node_modules_next_dist_compiled_react-dom_1e674e59._.js:10169:106)
    at <unknown> (file://C:/Users/Michał/source/repos/Orbito/orbito-frontend/.next/dev/static/chunks/node_modules_next_dist_compiled_react-dom_1e674e59._.js:10243:158)
    at ClientPageRoot (file://C:/Users/Michał/source/repos/Orbito/orbito-frontend/.next/dev/static/chunks/node_modules_next_dist_094231d7._.js:2202:50)

Next.js version: 16.0.7 (Turbopack)
