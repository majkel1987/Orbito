Project Structure - Orbito Platform
Architektura Systemu
Clean Architecture + DDD + CQRS (Backend) & Vertical Slices (Frontend)

Backend (.NET 9)
(Bez zmian - Backend jest źródłem prawdy dla Frontendu)

Orbito.API/
├── Controllers/ # REST endpoints, BaseController pattern
├── Middleware/ # IdempotencyMiddleware, ErrorHandling
├── Filters/ # Authorization, Validation
└── Program.cs # App configuration, Serilog, CORS

Orbito.Application/
├── Common/
│ ├── Behaviours/ # LoggingBehaviour, ValidationBehaviour
│ ├── Interfaces/ # Service contracts
│ ├── Models/ # DTOs, ErrorResponse, Result<T>
│ └── Constants/ # ValidationConstants, CacheConstants
├── Features/ # CQRS per aggregate
│ ├── [Feature]/
│ │ ├── Commands/ # Create, Update, Delete + Handlers
│ │ ├── Queries/ # Get, List, Search + Handlers
│ │ └── Validators/ # FluentValidation rules
└── Services/ # Application services

Orbito.Domain/
├── Entities/ # Rich domain models
├── ValueObjects/ # TenantId, Money, Email, IdempotencyKey
├── Enums/ # TeamMemberRole, ClientType, PaymentStatus
├── Repositories/ # Repository interfaces
└── Services/ # Domain services

Orbito.Infrastructure/
├── Data/
│ ├── Configurations/ # EF Core mappings
│ ├── Migrations/ # Database migrations
│ └── ApplicationDbContext.cs
├── Repositories/ # Repository implementations with ITenantContext
├── Services/ # External services (Stripe, Email)
├── Authorization/ # Custom handlers (ProviderTeamHandler)
└── DependencyInjection.cs
Frontend (Next.js 15 + TypeScript Strict)
Architektura oparta na Vertical Slices z automatyczną generacją API.

orbito-frontend/
├── src/
│ ├── app/ # Next.js App Router (Routing only)
│ │ ├── (auth)/ # Public routes (Login/Register)
│ │ │ ├── login/
│ │ │ ├── register/
│ │ │ └── layout.tsx # Centered layout
│ │ ├── (dashboard)/ # Provider Protected Area
│ │ │ ├── layout.tsx # Sidebar + TenantGuard
│ │ │ ├── page.tsx # Dashboard home
│ │ │ ├── team/ # Team management routes
│ │ │ ├── clients/ # Client CRM routes
│ │ │ ├── plans/ # Service plans routes
│ │ │ ├── subscriptions/ # Subscription management
│ │ │ ├── payments/ # Billing history & settings
│ │ │ └── analytics/ # Charts & Reports
│ │ ├── (portal)/ # End-Client Protected Area
│ │ │ ├── layout.tsx # Simple Topnav Layout + PortalGuard
│ │ │ └── portal/ # Client Self-Service Dashboard
│ │ └── api/ # Next.js API Routes (NextAuth)
│ │ └── auth/[...nextauth]/
│ │
│ ├── features/ # 🎯 Vertical Slices (Domain Logic)
│ │ ├── auth/
│ │ │ ├── components/ # LoginForm, RegisterForm
│ │ │ └── stores/ # authStore (Zustand)
│ │ ├── tenant/ # Multi-tenancy context
│ │ │ ├── components/ # TenantSwitcher, TenantGuard
│ │ │ └── providers/ # TenantProvider
│ │ ├── team/ # Team domain
│ │ ├── clients/ # Clients domain
│ │ ├── plans/ # Plans domain
│ │ ├── subscriptions/ # Subscriptions domain
│ │ ├── payments/ # Payments domain
│ │ ├── analytics/ # Analytics domain
│ │ └── client-portal/ # Specific logic for Client Portal
│ │ # Wewnątrz każdego feature'a:
│ │ # ├── components/ # Feature-specific UI
│ │ # ├── hooks/ # Logic & Data fetching wrappers
│ │ # └── schemas.ts # Zod validation schemas
│ │
│ ├── shared/ # Cross-cutting concerns
│ │ ├── ui/ # shadcn/ui components (Button, Card...)
│ │ ├── components/ # Layout (Sidebar, Header), Feedback
│ │ ├── hooks/ # useDebounce, useMediaQuery
│ │ └── lib/ # utils (cn), formatters
│ │
│ ├── core/ # Application Infrastructure
│ │ ├── api/
│ │ │ ├── client.ts # Axios instance + Interceptors (Result<T>)
│ │ │ └── generated/ # 🤖 AUTOMATICALLY GENERATED (Orval)
│ │ │ ├── api.ts # Endpoints & React Query Hooks
│ │ │ └── model.ts # TypeScript DTOs
│ │ ├── auth/ # NextAuth config
│ │ └── providers/ # QueryProvider, AuthProvider, ThemeProvider
│ │
│ ├── types/ # Global overrides (next-auth.d.ts)
│ └── middleware.ts # Route protection & matcher config
│
├── orval.config.ts # API Generator config
├── tailwind.config.ts
├── tsconfig.json # strict: true
└── package.json

Database Schema
Core Tables
Tenancy: Providers (Teams), TeamMembers

CRM: Clients (Customers)

Product: SubscriptionPlans

Billing: Subscriptions, Payments, PaymentMethods, Invoices

System: ActivityLogs, WebhookEvents

Key Relationships
Provider 1:N Clients

Provider 1:N TeamMembers

Provider 1:N SubscriptionPlans

Client 1:N Subscriptions

Subscription 1:N Payments

Client 1:N PaymentMethods (stored via Stripe reference)

Security Layers
Authentication: NextAuth v5 (JWT with Refresh Tokens)

Authorization: Role-Based Access Control (Owner/Admin/Member/Client)

Frontend Guard: TenantGuard (Provider) & PortalGuard (Client)

API Security: Repository-level ITenantContext isolation

Data Protection: No raw credit card data handled (Stripe Elements/PCI DSS)

Validation: Zod (Frontend) + FluentValidation (Backend)

Integration Points
API Communication: Orval (Auto-generated Axios + TanStack Query hooks)

Payments: Stripe (Checkout/Elements & Webhooks)

State Management: Zustand (Client state) + React Query (Server state)

UI System: shadcn/ui + Tailwind CSS

Development Patterns
API First: Frontend is generated from Backend Swagger/OpenAPI.

Strict Typing: No any, strict null checks, shared DTOs.

Component Colocation: UI components live inside their specific features/ unless truly generic.

Error Handling: Centralized Axios interceptor for Result<T> pattern.

Async Safety: Handling Next.js 15 async params and searchParams.
