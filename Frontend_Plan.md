# Plan Implementacji Frontendu dla Orbito Platform v4.0

**Wersja**: 4.0  
**Data**: 2025-01-14  
**Stack**: Next.js 14 + JavaScript + Tailwind CSS + TanStack Query + Zustand  
**Czas realizacji**: 24 tygodnie (6 miesięcy)

---

## 📊 Executive Summary

### Kluczowe Zmiany vs v3.0

| Obszar               | Było (v3.0)      | Jest (v4.0)              | Uzasadnienie                         |
| -------------------- | ---------------- | ------------------------ | ------------------------------------ |
| **Framework**        | Create React App | Next.js 14               | SSR, lepsze SEO, routing, middleware |
| **Czas realizacji**  | 12 tygodni       | 24 tygodnie              | Realistyczne podejście               |
| **State Management** | Context API      | Zustand + TanStack Query | Lepsza wydajność, DevTools           |
| **Bezpieczeństwo**   | localStorage JWT | httpOnly cookies         | Ochrona przed XSS                    |
| **Architektura**     | Component-based  | Feature-based            | Lepsza skalowalność                  |
| **API Integration**  | Manual           | OpenAPI Generator        | Type safety (w JSDoc)                |

### Statystyki Projektu

| Metryka                | Wartość                          |
| ---------------------- | -------------------------------- |
| **Fazy implementacji** | 8 faz                            |
| **MVP Timeline**       | 10 tygodni                       |
| **Full Release**       | 24 tygodnie                      |
| **Komponenty**         | ~80+                             |
| **Features**           | 12 modułów                       |
| **API Endpoints**      | 45+ (zsynchronizowane z backend) |
| **Test Coverage Goal** | 70%+                             |

---

## 🏗️ Architektura Aplikacji

### Stack Technologiczny

```javascript
// next.config.js
const config = {
  framework: "Next.js 14.2.x",
  language: "JavaScript (ES2024)",
  styling: {
    css: "Tailwind CSS 3.4",
    components: "shadcn/ui",
    icons: "lucide-react",
  },
  state: {
    server: "TanStack Query v5",
    client: "Zustand v4",
    forms: "React Hook Form v7",
  },
  validation: "Zod v3 (z JSDoc)",
  api: {
    client: "Axios v1.6",
    generation: "OpenAPI Generator",
  },
  auth: "NextAuth.js v4",
  testing: {
    unit: "Jest + React Testing Library",
    e2e: "Playwright",
    api: "MSW v2",
  },
};
```

### Struktura Katalogów (Feature-Based)

```
orbito-frontend/
├── src/
│   ├── app/                    # Next.js App Router
│   │   ├── (auth)/             # Auth group layout
│   │   │   ├── login/
│   │   │   ├── register/
│   │   │   └── setup/
│   │   ├── (dashboard)/        # Dashboard group layout
│   │   │   ├── layout.js
│   │   │   ├── page.js
│   │   │   └── loading.js
│   │   ├── api/                # API routes (BFF pattern)
│   │   │   ├── auth/[...nextauth]/
│   │   │   └── proxy/          # Backend proxy
│   │   └── layout.js           # Root layout
│   ├── features/               # Feature modules
│   │   ├── auth/
│   │   │   ├── api/            # API calls
│   │   │   ├── components/     # Feature components
│   │   │   ├── hooks/          # Feature hooks
│   │   │   ├── stores/         # Zustand stores
│   │   │   ├── utils/          # Utils
│   │   │   └── validators/     # Zod schemas
│   │   ├── clients/
│   │   ├── subscriptions/
│   │   ├── payments/
│   │   ├── analytics/
│   │   └── [other features]/
│   ├── core/                   # Core business logic
│   │   ├── api/
│   │   │   ├── client.js       # Axios instance
│   │   │   ├── interceptors.js # Request/Response interceptors
│   │   │   └── generated/      # OpenAPI generated
│   │   ├── config/
│   │   ├── constants/
│   │   └── security/
│   ├── shared/                 # Shared resources
│   │   ├── components/         # UI components
│   │   │   ├── ui/            # shadcn components
│   │   │   └── layouts/
│   │   ├── hooks/             # Global hooks
│   │   ├── utils/             # Global utils
│   │   └── lib/               # External lib configs
│   └── styles/
│       └── globals.css
├── public/
├── tests/
│   ├── unit/
│   ├── integration/
│   └── e2e/
├── .env.local
├── .env.production
├── next.config.js
├── package.json
├── jsconfig.json               # Path aliases
├── .eslintrc.json
└── playwright.config.js
```

### Architektura Warstw

```
┌─────────────────────────────────────────────────┐
│             Next.js Pages/App Router            │
│          (SSR/SSG/ISR + API Routes)             │
└─────────────────────┬───────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────┐
│              Feature Modules                    │
│  (Self-contained features with own logic)       │
└─────────────────────┬───────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────┐
│           State Management Layer                │
│     (Zustand stores + TanStack Query)           │
└─────────────────────┬───────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────┐
│              API Client Layer                   │
│   (Axios + Interceptors + Error Handling)       │
└─────────────────────┬───────────────────────────┘
                      │
┌─────────────────────▼──────────────────────────-─┐
│            Backend API (.NET)                    │
│     (Multi-tenant + CQRS + Clean Arch)           │
└──────────────────────────────────────────────────┘
```

---

## 🔐 Bezpieczeństwo i Autentykacja

### NextAuth.js Configuration

```javascript
// src/app/api/auth/[...nextauth]/route.js
import NextAuth from "next-auth";
import CredentialsProvider from "next-auth/providers/credentials";
import { apiClient } from "@/core/api/client";

export const authOptions = {
  providers: [
    CredentialsProvider({
      name: "credentials",
      credentials: {
        email: { label: "Email", type: "email" },
        password: { label: "Password", type: "password" },
      },
      async authorize(credentials) {
        try {
          const response = await apiClient.post("/api/account/login", {
            email: credentials.email,
            password: credentials.password,
          });

          if (response.data.token) {
            // Decode JWT to get user info
            const user = {
              id: response.data.userId,
              email: response.data.email,
              name: response.data.name,
              role: response.data.role,
              tenantId: response.data.tenantId,
              accessToken: response.data.token,
            };
            return user;
          }
          return null;
        } catch (error) {
          console.error("Auth error:", error);
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
        token.userId = user.id;
      }
      return token;
    },
    async session({ session, token }) {
      session.accessToken = token.accessToken;
      session.user.role = token.role;
      session.user.tenantId = token.tenantId;
      session.user.id = token.userId;
      return session;
    },
  },
  pages: {
    signIn: "/login",
    error: "/auth/error",
  },
  session: {
    strategy: "jwt",
    maxAge: 30 * 60, // 30 minutes
  },
  cookies: {
    sessionToken: {
      name: `__Secure-orbito.session-token`,
      options: {
        httpOnly: true,
        sameSite: "lax",
        path: "/",
        secure: true, // tylko HTTPS w produkcji
      },
    },
  },
};

const handler = NextAuth(authOptions);
export { handler as GET, handler as POST };
```

### Multi-Tenancy Implementation

```javascript
// src/core/api/interceptors.js
import { getSession } from "next-auth/react";

export const setupInterceptors = (axiosInstance) => {
  // Request interceptor
  axiosInstance.interceptors.request.use(
    async (config) => {
      const session = await getSession();

      if (session?.accessToken) {
        config.headers.Authorization = `Bearer ${session.accessToken}`;
      }

      if (session?.user?.tenantId) {
        config.headers["X-Tenant-Id"] = session.user.tenantId;
      }

      // Idempotency key dla POST/PUT/DELETE
      if (["post", "put", "delete"].includes(config.method)) {
        config.headers["X-Idempotency-Key"] = crypto.randomUUID();
      }

      return config;
    },
    (error) => Promise.reject(error)
  );

  // Response interceptor
  axiosInstance.interceptors.response.use(
    (response) => response,
    async (error) => {
      if (error.response?.status === 401) {
        // Token wygasł - wyloguj
        await signOut({ callbackUrl: "/login" });
      }

      if (error.response?.status === 429) {
        // Rate limiting - pokaż toast
        showToast("Too many requests. Please wait a moment.");
      }

      return Promise.reject(error);
    }
  );
};
```

### Protected Routes Middleware

```javascript
// src/middleware.js
import { withAuth } from "next-auth/middleware";

export default withAuth({
  callbacks: {
    authorized: ({ token, req }) => {
      const path = req.nextUrl.pathname;

      // Public routes
      if (
        path.startsWith("/login") ||
        path.startsWith("/register") ||
        path === "/"
      ) {
        return true;
      }

      // Authenticated routes
      if (!token) return false;

      // Admin only routes
      if (path.startsWith("/admin")) {
        return token.role === "PlatformAdmin";
      }

      // Provider routes
      if (path.startsWith("/provider")) {
        return ["Provider", "PlatformAdmin"].includes(token.role);
      }

      return true;
    },
  },
});

export const config = {
  matcher: ["/((?!api|_next/static|_next/image|favicon.ico).*)"],
};
```

---

## 📦 Feature Modules - Szczegółowy Opis

### 1. Auth Module (`src/features/auth/`)

```javascript
// src/features/auth/stores/authStore.js
import { create } from "zustand";
import { devtools } from "zustand/middleware";

export const useAuthStore = create(
  devtools(
    (set, get) => ({
      user: null,
      isLoading: false,
      error: null,

      // Actions
      setUser: (user) => set({ user }),
      setLoading: (isLoading) => set({ isLoading }),
      setError: (error) => set({ error }),

      // Computed
      isAuthenticated: () => !!get().user,
      hasRole: (role) => get().user?.role === role,
      isProvider: () =>
        ["Provider", "PlatformAdmin"].includes(get().user?.role),
      isAdmin: () => get().user?.role === "PlatformAdmin",

      // Clear
      logout: () => set({ user: null, error: null }),
    }),
    { name: "auth-store" }
  )
);
```

### 2. Clients Module (`src/features/clients/`)

```javascript
// src/features/clients/api/clientsApi.js
import { apiClient } from "@/core/api/client";

export const clientsApi = {
  // Queries
  getClients: async (params) => {
    const { data } = await apiClient.get("/api/clients", { params });
    return data;
  },

  getClientById: async (id) => {
    const { data } = await apiClient.get(`/api/clients/${id}`);
    return data;
  },

  searchClients: async (searchTerm, params) => {
    const { data } = await apiClient.get("/api/clients/search", {
      params: { searchTerm, ...params },
    });
    return data;
  },

  getClientStats: async () => {
    const { data } = await apiClient.get("/api/clients/stats");
    return data;
  },

  // Mutations
  createClient: async (clientData) => {
    const { data } = await apiClient.post("/api/clients", clientData);
    return data;
  },

  updateClient: async ({ id, ...clientData }) => {
    const { data } = await apiClient.put(`/api/clients/${id}`, clientData);
    return data;
  },

  deleteClient: async (id, hardDelete = false) => {
    const { data } = await apiClient.delete(`/api/clients/${id}`, {
      params: { hardDelete },
    });
    return data;
  },

  activateClient: async (id) => {
    const { data } = await apiClient.post(`/api/clients/${id}/activate`);
    return data;
  },

  deactivateClient: async (id) => {
    const { data } = await apiClient.post(`/api/clients/${id}/deactivate`);
    return data;
  },
};

// src/features/clients/hooks/useClients.js
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { clientsApi } from "../api/clientsApi";
import { toast } from "sonner";

export const useClients = (params) => {
  return useQuery({
    queryKey: ["clients", params],
    queryFn: () => clientsApi.getClients(params),
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
};

export const useClient = (id) => {
  return useQuery({
    queryKey: ["clients", id],
    queryFn: () => clientsApi.getClientById(id),
    enabled: !!id,
  });
};

export const useCreateClient = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: clientsApi.createClient,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["clients"] });
      toast.success("Klient został utworzony");
    },
    onError: (error) => {
      toast.error(
        error.response?.data?.message || "Błąd podczas tworzenia klienta"
      );
    },
  });
};
```

### 3. Payments Module (`src/features/payments/`)

```javascript
// src/features/payments/components/StripePaymentForm.jsx
import { loadStripe } from "@stripe/stripe-js";
import {
  Elements,
  CardElement,
  useStripe,
  useElements,
} from "@stripe/react-stripe-js";
import { useState } from "react";
import { useProcessPayment } from "../hooks/usePayments";

const stripePromise = loadStripe(process.env.NEXT_PUBLIC_STRIPE_KEY);

function PaymentForm({ amount, currency, subscriptionId, onSuccess }) {
  const stripe = useStripe();
  const elements = useElements();
  const [isProcessing, setIsProcessing] = useState(false);
  const processPayment = useProcessPayment();

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!stripe || !elements) return;

    setIsProcessing(true);

    try {
      // Create payment method
      const { error, paymentMethod } = await stripe.createPaymentMethod({
        type: "card",
        card: elements.getElement(CardElement),
      });

      if (error) throw error;

      // Process payment through backend
      const result = await processPayment.mutateAsync({
        subscriptionId,
        paymentMethodId: paymentMethod.id,
        amount,
        currency,
      });

      // Handle 3D Secure if required
      if (result.requiresAction) {
        const { error: confirmError } = await stripe.confirmCardPayment(
          result.clientSecret
        );

        if (confirmError) throw confirmError;
      }

      onSuccess?.(result);
    } catch (error) {
      console.error("Payment error:", error);
      toast.error(error.message || "Błąd płatności");
    } finally {
      setIsProcessing(false);
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <CardElement />
      <button type="submit" disabled={!stripe || isProcessing}>
        {isProcessing ? "Przetwarzanie..." : `Zapłać ${amount} ${currency}`}
      </button>
    </form>
  );
}

export default function StripePaymentForm(props) {
  return (
    <Elements stripe={stripePromise}>
      <PaymentForm {...props} />
    </Elements>
  );
}
```

### 4. Real-time Updates Module

```javascript
// src/features/realtime/hooks/useWebSocket.js
import { useEffect, useRef } from "react";
import { useSession } from "next-auth/react";

export const useWebSocket = (url, options = {}) => {
  const { data: session } = useSession();
  const ws = useRef(null);
  const reconnectTimeout = useRef(null);

  useEffect(() => {
    if (!session?.accessToken) return;

    const connect = () => {
      ws.current = new WebSocket(`${url}?token=${session.accessToken}`);

      ws.current.onopen = () => {
        console.log("WebSocket connected");
        options.onOpen?.();
      };

      ws.current.onmessage = (event) => {
        const data = JSON.parse(event.data);
        options.onMessage?.(data);
      };

      ws.current.onerror = (error) => {
        console.error("WebSocket error:", error);
        options.onError?.(error);
      };

      ws.current.onclose = () => {
        console.log("WebSocket disconnected");
        options.onClose?.();

        // Auto-reconnect after 5 seconds
        if (options.autoReconnect !== false) {
          reconnectTimeout.current = setTimeout(connect, 5000);
        }
      };
    };

    connect();

    return () => {
      clearTimeout(reconnectTimeout.current);
      ws.current?.close();
    };
  }, [session, url]);

  const send = (data) => {
    if (ws.current?.readyState === WebSocket.OPEN) {
      ws.current.send(JSON.stringify(data));
    }
  };

  return { send, ws: ws.current };
};
```

---

## 🚀 Fazy Implementacji (Realistyczny Timeline)

### **FAZA 0: Setup & Foundation (Tydzień 1-2)**

#### Zadania

- [ ] Inicjalizacja Next.js 14 z App Router
- [ ] Konfiguracja Tailwind CSS + shadcn/ui
- [ ] Setup ESLint + Prettier
- [ ] Konfiguracja path aliases
- [ ] Setup Zustand + TanStack Query
- [ ] Axios client z interceptorami
- [ ] NextAuth.js basic setup
- [ ] Podstawowe komponenty UI (10 komponentów)

#### Deliverables

- Działający projekt Next.js
- Skonfigurowane narzędzia deweloperskie
- Basic authentication flow
- 10 komponentów UI z shadcn

---

### **FAZA 1: Authentication & Security (Tydzień 3-4)**

#### Zadania

- [ ] Pełna konfiguracja NextAuth.js
- [ ] Login/Register pages
- [ ] Protected routes middleware
- [ ] Session management
- [ ] Role-based access control
- [ ] Multi-tenancy context
- [ ] Error boundary components
- [ ] Security headers

#### Deliverables

- Kompletny system autentykacji
- RBAC implementation
- Secure cookie management

---

### **FAZA 2: Layout & Navigation (Tydzień 5-6)**

#### Zadania

- [ ] Dashboard layouts (Provider, Client, Admin)
- [ ] Responsive sidebar
- [ ] Header z user menu
- [ ] Breadcrumbs system
- [ ] Loading states
- [ ] Error pages (404, 500)
- [ ] Theme system (light/dark przygotowanie)

#### Deliverables

- Kompletny system layoutów
- Nawigacja działająca
- Responsive design

---

### **FAZA 3: Client Management (Tydzień 7-10)**

#### Zadania

- [ ] Client list z paginacją i filtrowaniem
- [ ] Client detail page
- [ ] Create/Edit client forms
- [ ] Client search z debounce
- [ ] Client statistics dashboard
- [ ] Bulk operations
- [ ] Export to Excel
- [ ] Client activity timeline

#### Deliverables

- Pełny CRUD dla klientów
- Advanced search i filtering
- Statistics dashboard

---

### **FAZA 4: Plans & Subscriptions (Tydzień 11-14)**

#### Zadania

- [ ] Plan management (CRUD)
- [ ] Plan templates
- [ ] Subscription wizard (multi-step)
- [ ] Subscription lifecycle management
- [ ] Subscription timeline
- [ ] Plan comparison tool
- [ ] Upgrade/downgrade flow
- [ ] Cancellation flow

#### Deliverables

- Plan management kompletny
- Subscription wizard działający
- Lifecycle operations

---

### **FAZA 5: Payment System (Tydzień 15-18)**

#### Zadania

- [ ] Stripe integration
- [ ] Payment form z 3D Secure
- [ ] Payment methods management
- [ ] Payment history
- [ ] Invoice generation
- [ ] Refund processing
- [ ] Failed payment retry
- [ ] Payment notifications

#### Deliverables

- Pełna integracja Stripe
- Payment processing flow
- Invoice system

---

### **FAZA 6: Analytics & Reports (Tydzień 19-21)**

#### Zadania

- [ ] Revenue dashboard
- [ ] Payment analytics
- [ ] Client analytics
- [ ] Subscription metrics
- [ ] Custom reports builder
- [ ] Export functionality
- [ ] Charts z Recharts
- [ ] Real-time metrics

#### Deliverables

- Kompletny system analityk
- Custom reports
- Real-time dashboards

---

### **FAZA 7: Testing & Optimization (Tydzień 22-24)**

#### Zadania

- [ ] Unit tests (70% coverage)
- [ ] Integration tests
- [ ] E2E tests z Playwright
- [ ] Performance optimization
- [ ] SEO optimization
- [ ] Accessibility audit
- [ ] Security audit
- [ ] Load testing

#### Deliverables

- 70%+ test coverage
- Performance < 3s load time
- WCAG 2.1 AA compliance
- Security audit passed

---

## 🔄 Synchronizacja z Backendem

### OpenAPI Integration

```javascript
// package.json scripts
{
  "scripts": {
    "generate-api": "openapi-generator-cli generate -i http://localhost:5000/swagger/v1/swagger.json -g javascript -o ./src/core/api/generated",
    "generate-api:watch": "nodemon --watch ../backend/swagger.json --exec npm run generate-api"
  }
}
```

### API Client z JSDoc Types

```javascript
// src/core/api/client.js
import axios from "axios";

/**
 * @typedef {Object} ApiResponse
 * @property {boolean} success
 * @property {any} data
 * @property {string} message
 * @property {Object} errors
 */

/**
 * @typedef {Object} PaginatedResponse
 * @property {Array} items
 * @property {number} pageNumber
 * @property {number} pageSize
 * @property {number} totalPages
 * @property {number} totalCount
 */

const apiClient = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000",
  timeout: 30000,
  headers: {
    "Content-Type": "application/json",
  },
});

// Error handler
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    // Standardized error format
    const standardError = {
      message: error.response?.data?.message || "Wystąpił błąd",
      status: error.response?.status,
      errors: error.response?.data?.errors || {},
      code: error.response?.data?.code,
    };

    return Promise.reject(standardError);
  }
);

export { apiClient };
```

### Health Checks Integration

```javascript
// src/features/monitoring/hooks/useHealthChecks.js
import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/core/api/client";

export const useHealthChecks = () => {
  return useQuery({
    queryKey: ["health-checks"],
    queryFn: async () => {
      const response = await apiClient.get("/health");
      return response.data;
    },
    refetchInterval: 30000, // Check every 30 seconds
    retry: false,
  });
};

// Component usage
export function SystemStatus() {
  const { data: health, isLoading } = useHealthChecks();

  if (isLoading) return <Skeleton />;

  return (
    <div className="grid grid-cols-3 gap-4">
      <StatusCard
        title="API"
        status={health?.status}
        details={health?.results?.api}
      />
      <StatusCard
        title="Stripe"
        status={health?.results?.stripe?.status}
        responseTime={health?.results?.stripe?.data?.responseTime}
      />
      <StatusCard title="Database" status={health?.results?.database?.status} />
    </div>
  );
}
```

---

## 🧪 Testing Strategy

### Unit Tests z Jest

```javascript
// src/features/clients/hooks/__tests__/useClients.test.js
import { renderHook, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { useClients } from "../useClients";
import { server } from "@/tests/mocks/server";
import { rest } from "msw";

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
    },
  });

  return ({ children }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
};

describe("useClients", () => {
  it("should fetch clients successfully", async () => {
    const { result } = renderHook(
      () => useClients({ pageNumber: 1, pageSize: 10 }),
      { wrapper: createWrapper() }
    );

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(result.current.data).toHaveProperty("items");
    expect(result.current.data.items).toHaveLength(10);
  });

  it("should handle errors", async () => {
    server.use(
      rest.get("/api/clients", (req, res, ctx) => {
        return res(ctx.status(500), ctx.json({ message: "Server error" }));
      })
    );

    const { result } = renderHook(
      () => useClients({ pageNumber: 1, pageSize: 10 }),
      { wrapper: createWrapper() }
    );

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });

    expect(result.current.error).toHaveProperty("message", "Server error");
  });
});
```

### E2E Tests z Playwright

```javascript
// tests/e2e/auth.spec.js
import { test, expect } from "@playwright/test";

test.describe("Authentication", () => {
  test("should login successfully", async ({ page }) => {
    await page.goto("/login");

    // Fill login form
    await page.fill('[name="email"]', "test@orbito.com");
    await page.fill('[name="password"]', "Test123!");

    // Submit form
    await page.click('[type="submit"]');

    // Wait for redirect
    await page.waitForURL("/dashboard");

    // Check if logged in
    expect(page.url()).toContain("/dashboard");
    await expect(page.locator('[data-testid="user-menu"]')).toBeVisible();
  });

  test("should handle invalid credentials", async ({ page }) => {
    await page.goto("/login");

    await page.fill('[name="email"]', "invalid@email.com");
    await page.fill('[name="password"]', "wrong");

    await page.click('[type="submit"]');

    // Should show error
    await expect(page.locator('[role="alert"]')).toContainText(
      "Invalid credentials"
    );
  });
});
```

---

## 🎯 MVP Scope (10 tygodni)

### Must Have (MVP)

- [ ] Authentication (login/register/logout)
- [ ] Basic dashboard
- [ ] Client CRUD
- [ ] Plan management
- [ ] Basic subscription creation
- [ ] Simple payment processing
- [ ] Basic reports

### Nice to Have (Post-MVP)

- [ ] Advanced analytics
- [ ] Bulk operations
- [ ] Export/Import
- [ ] Webhook management UI
- [ ] Audit logs viewer
- [ ] Advanced search
- [ ] Real-time notifications

### Future Features (v2)

- [ ] Mobile app (React Native)
- [ ] AI-powered insights
- [ ] Custom workflows
- [ ] API for third-party integrations
- [ ] White-label support

---

## 📊 Monitoring & Performance

### Performance Metrics

```javascript
// src/core/monitoring/performance.js
export const measurePerformance = () => {
  if (typeof window === "undefined") return;

  // Web Vitals
  if (window.performance?.getEntriesByType) {
    const navigation = performance.getEntriesByType("navigation")[0];

    const metrics = {
      // Time to First Byte
      ttfb: navigation.responseStart - navigation.fetchStart,

      // DOM Content Loaded
      domContentLoaded:
        navigation.domContentLoadedEventEnd - navigation.fetchStart,

      // Load Complete
      loadComplete: navigation.loadEventEnd - navigation.fetchStart,

      // First Contentful Paint
      fcp: performance.getEntriesByName("first-contentful-paint")[0]?.startTime,

      // Largest Contentful Paint
      lcp: 0, // Will be updated by PerformanceObserver
    };

    // Send to analytics
    if (window.gtag) {
      window.gtag("event", "page_performance", metrics);
    }
  }
};

// Next.js integration
// src/app/layout.js
import { GoogleAnalytics } from "@next/third-parties/google";

export default function RootLayout({ children }) {
  return (
    <html>
      <body>
        {children}
        <GoogleAnalytics gaId="G-XXXXXXXXXX" />
      </body>
    </html>
  );
}
```

---

## 🚨 Risk Mitigation

### Identified Risks & Solutions

| Risk                            | Probability | Impact   | Mitigation Strategy                             |
| ------------------------------- | ----------- | -------- | ----------------------------------------------- |
| **Backend API changes**         | Medium      | High     | OpenAPI generation, versioning, contract tests  |
| **Performance issues**          | Medium      | High     | Code splitting, lazy loading, CDN, monitoring   |
| **Security vulnerabilities**    | Low         | Critical | Regular audits, dependency updates, CSP headers |
| **Browser compatibility**       | Low         | Medium   | Transpiling, polyfills, testing matrix          |
| **State management complexity** | Medium      | Medium   | Clear patterns, documentation, code reviews     |
| **Stripe integration issues**   | Low         | High     | Sandbox testing, error handling, fallbacks      |

---

## ✅ Definition of Done

### Feature Checklist

- [ ] Kod napisany i przetestowany lokalnie
- [ ] Unit testy (min. 70% coverage)
- [ ] Integration tests dla critical paths
- [ ] Code review przeprowadzony
- [ ] Dokumentacja zaktualizowana
- [ ] Responsive design sprawdzony
- [ ] Accessibility sprawdzone (keyboard nav + screen reader)
- [ ] Performance metrics w normie (LCP < 2.5s)
- [ ] Security headers configured
- [ ] Error handling implemented
- [ ] Loading states added
- [ ] Deployed to staging

---

## 📝 Development Guidelines

### Code Style

```javascript
// ✅ Good - Clear naming, proper error handling
export const useClient = (clientId) => {
  const { data, error, isLoading } = useQuery({
    queryKey: ["clients", clientId],
    queryFn: () => clientsApi.getById(clientId),
    enabled: !!clientId,
    retry: 2,
    staleTime: 5 * 60 * 1000,
  });

  return {
    client: data,
    error,
    isLoading,
    isError: !!error,
  };
};

// ❌ Bad - Poor naming, no error handling
export const useC = (id) => {
  const q = useQuery(["c", id], () => fetch(`/api/c/${id}`));
  return q.data;
};
```

### Component Pattern

```javascript
// ✅ Preferred - Composition, clear props
export function ClientCard({ client, onEdit, onDelete }) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>{client.name}</CardTitle>
      </CardHeader>
      <CardContent>
        <ClientInfo client={client} />
      </CardContent>
      <CardFooter>
        <Button onClick={() => onEdit(client.id)}>Edit</Button>
        <Button variant="destructive" onClick={() => onDelete(client.id)}>
          Delete
        </Button>
      </CardFooter>
    </Card>
  );
}

// ❌ Avoid - Too many responsibilities
export function ClientCardBad({ clientId }) {
  const client = useClient(clientId);
  const updateClient = useUpdateClient();
  const deleteClient = useDeleteClient();
  // ... too much logic in one component
}
```

---

## 🎯 Success Metrics

### Technical KPIs

- **Performance**: LCP < 2.5s, FID < 100ms, CLS < 0.1
- **Test Coverage**: > 70% overall
- **Bundle Size**: < 300KB initial JS (gzipped)
- **Accessibility**: WCAG 2.1 AA compliant
- **SEO**: Lighthouse score > 90

### Business KPIs

- **User Adoption**: 80% of users using new features
- **Error Rate**: < 0.1% of requests
- **Page Load Time**: < 3s on 3G
- **User Satisfaction**: > 4.5/5 rating

---

## 📞 Support & Resources

### Documentation

- [Next.js Docs](https://nextjs.org/docs)
- [Tailwind CSS](https://tailwindcss.com/docs)
- [shadcn/ui](https://ui.shadcn.com)
- [TanStack Query](https://tanstack.com/query)
- [Zustand](https://zustand-demo.pmnd.rs)

### Internal Resources

- API Documentation: `/swagger`
- Design System: Figma link
- Backend Repository: GitHub link
- CI/CD Pipeline: GitHub Actions

---

**Status**: ✅ **READY FOR IMPLEMENTATION**  
**Version**: 4.0  
**Updated**: 2025-01-14  
**Review**: After Phase 2 completion (Week 6)

---
