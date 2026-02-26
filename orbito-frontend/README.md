# Orbito Frontend

> Subscription Management Platform — Provider Dashboard & Client Portal

![Next.js](https://img.shields.io/badge/Next.js-16-black?logo=next.js)
![TypeScript](https://img.shields.io/badge/TypeScript-5-blue?logo=typescript)
![TanStack Query](https://img.shields.io/badge/TanStack_Query-v5-red)
![shadcn/ui](https://img.shields.io/badge/shadcn%2Fui-latest-black)
![Tailwind CSS](https://img.shields.io/badge/Tailwind_CSS-v4-38bdf8?logo=tailwindcss)

---

## Prerequisites

| Requirement | Version  |
|-------------|----------|
| Node.js     | >= 20.x  |
| npm         | >= 10.x  |
| Backend API | Running on `http://localhost:5000` |

---

## Getting Started

### 1. Install dependencies

```bash
npm install
```

### 2. Configure environment

Create a `.env.local` file in the project root:

```env
# Backend API base URL
NEXT_PUBLIC_API_URL=http://localhost:5000

# NextAuth secret (any secure random string)
AUTH_SECRET=your-secret-here

# NextAuth URL (for callbacks)
NEXTAUTH_URL=http://localhost:3000
```

### 3. Generate API hooks (requires running backend)

```bash
npm run api:generate
```

### 4. Start the development server

```bash
npm run dev
```

Open [http://localhost:3000](http://localhost:3000) in your browser.

---

## Project Structure

```
orbito-frontend/
├── src/
│   ├── app/                        # Next.js 15 App Router
│   │   ├── (auth)/                 # Public auth pages (login, register)
│   │   ├── (dashboard)/            # Provider panel (TenantGuard protected)
│   │   │   ├── dashboard/          # Main dashboard with analytics
│   │   │   ├── clients/            # Client management
│   │   │   ├── plans/              # Subscription plans management
│   │   │   ├── subscriptions/      # Subscriptions management
│   │   │   ├── payments/           # Payment history & processing
│   │   │   ├── team/               # Team members & invitations
│   │   │   └── analytics/          # Charts and statistics
│   │   └── (public)/               # Public invitation acceptance
│   ├── core/                       # Application infrastructure
│   │   ├── api/
│   │   │   ├── client.ts           # Axios instance + auth & Result<T> interceptors
│   │   │   └── generated/          # Auto-generated Orval hooks (DO NOT EDIT)
│   │   ├── auth/                   # NextAuth v5 configuration
│   │   └── providers/              # React Query provider, theme provider
│   ├── features/                   # Vertical slice business domains
│   │   ├── auth/                   # Login/register forms, auth hooks
│   │   ├── clients/                # Client CRUD, search, filters
│   │   ├── plans/                  # Plan CRUD with dynamic features list
│   │   ├── subscriptions/          # Subscription lifecycle management
│   │   ├── payments/               # Payment history, processing, refunds
│   │   ├── analytics/              # Dashboard stats, charts, CSV export
│   │   └── team/                   # Team members, invitation flow
│   └── shared/                     # Reusable, stateless utilities
│       ├── ui/                     # shadcn/ui components
│       ├── components/             # Shared app components (layout, guards)
│       ├── lib/                    # Formatters, utils (cn, formatCurrency...)
│       └── hooks/                  # Shared hooks (usePagination, useDebounce)
├── A11Y_CHECKLIST.md               # Accessibility audit checklist
├── eslint.config.mjs               # ESLint flat config (A11y + TS strict)
├── next.config.ts                  # Next.js + bundle analyzer config
├── orval.config.ts                 # Orval code generation config
└── tailwind.config.ts              # Tailwind CSS configuration
```

---

## Scripts

| Script         | Command                 | Description                                    |
|----------------|-------------------------|------------------------------------------------|
| `dev`          | `npm run dev`           | Start development server (localhost:3000)      |
| `build`        | `npm run build`         | Production build                               |
| `start`        | `npm run start`         | Start production server                        |
| `typecheck`    | `npm run typecheck`     | TypeScript type checking (no emit)             |
| `lint`         | `npm run lint`          | ESLint with A11y + TypeScript rules            |
| `api:generate` | `npm run api:generate`  | Regenerate Orval hooks from Swagger spec       |
| `test`         | `npm run test`          | Run Vitest unit tests                          |
| `test:watch`   | `npm run test:watch`    | Vitest in watch mode                           |
| `test:e2e`     | `npm run test:e2e`      | Playwright end-to-end tests                    |
| `analyze`      | `npm run analyze`       | Build with bundle size analyzer                |

---

## Environment Variables

| Variable                | Required | Description                                         |
|-------------------------|----------|-----------------------------------------------------|
| `NEXT_PUBLIC_API_URL`   | Yes      | Backend API base URL (e.g. `http://localhost:5000`) |
| `AUTH_SECRET`           | Yes      | NextAuth secret for JWT signing                     |
| `NEXTAUTH_URL`          | Yes      | Full URL of the frontend app                        |
| `ANALYZE`               | No       | Set to `"true"` to enable bundle analyzer           |

---

## Tech Stack

| Category        | Technology                   | Version  |
|-----------------|------------------------------|----------|
| Framework       | Next.js (App Router)         | 16.x     |
| Language        | TypeScript (Strict Mode)     | 5.x      |
| Server State    | TanStack Query               | v5       |
| Client State    | Zustand                      | v5       |
| API Generation  | Orval (from OpenAPI/Swagger) | 7.x      |
| Styling         | Tailwind CSS v4              | 4.x      |
| UI Components   | shadcn/ui + Radix UI         | latest   |
| Forms           | React Hook Form + Zod        | 7.x / 4.x |
| Auth            | NextAuth v5 (Credentials)    | beta.30  |
| Charts          | Recharts                     | 3.x      |
| Testing (Unit)  | Vitest + React Testing Library | 4.x    |
| Testing (E2E)   | Playwright                   | 1.x      |

---

## API Integration

This project uses **Orval** to auto-generate type-safe React Query hooks from the backend's OpenAPI/Swagger specification.

Never write manual fetch/axios calls. Always use generated hooks:

```typescript
// Correct — use generated hooks
import { useGetApiClients } from "@/core/api/generated/clients/clients";

const { data, isLoading, error } = useGetApiClients();

// Forbidden — manual API calls
const response = await axios.get("/api/clients");
```

To regenerate hooks after backend changes:

```bash
# Backend must be running at NEXT_PUBLIC_API_URL
npm run api:generate
```

---

## Architecture: Vertical Slices

Each feature domain is self-contained in `src/features/<domain>/`:

```
features/clients/
├── components/        # UI components (ClientsList, ClientForm...)
├── hooks/             # Domain-specific hooks
└── schemas.ts         # Zod validation schemas
```

Shared utilities live in `src/shared/` and have no business logic.

---

## Security

- **TenantGuard** — Protects `/dashboard/*` routes (Provider role only)
- **PortalGuard** — Protects `/portal/*` routes (Client role only)
- **Auth Interceptor** — Automatically injects `Authorization: Bearer <token>` on all API requests
- **Multi-tenancy** — All API calls are scoped to the authenticated tenant (enforced server-side)

---

## Accessibility

See [A11Y_CHECKLIST.md](./A11Y_CHECKLIST.md) for the full audit checklist.

Automated A11y linting is enabled via `eslint-plugin-jsx-a11y` (recommended ruleset).

---

## Known Issues

- `billingPeriodType` returned as string `"1 Monthly"` from backend — workaround: `parseBillingPeriod()` in `features/plans/`
- Backend filter `activeOnly=false` returns active clients instead of inactive (backend bug)
- `GET /api/Subscriptions` OpenAPI spec lacks `SubscriptionDto` type — custom type defined in frontend
