# PROMPT: FAZA 4 - Punkt 4.3 Plan List Page

## 📋 KONTEKST PROJEKTU

### Status Implementacji

- **Projekt**: Orbito Frontend (Next.js 15 + App Router)
- **Obecna Faza**: FAZA 4 - Plans & Subscriptions (W TRAKCIE)
- **Następny Krok**: Punkt 4.3 - Plan List Page (KRYTYCZNE)
- **Poprzednie Kroki**:
  - ✅ FAZA 0: Setup & Foundation (100%)
  - ✅ FAZA 1: Authentication (100%)
  - ✅ FAZA 2: Layout & Navigation (100%)
  - ✅ FAZA 2.5: Team Management (100%)
  - ✅ FAZA 3: Client Management (86% - 12/14 zadań)
  - ✅ FAZA 4.1: Plans API Service (UKOŃCZONE)
  - ✅ FAZA 4.2: Plans Hooks (UKOŃCZONE)

### Istniejące Struktury do Naśladowania

**Pattern z Client List Page (FAZA 3.3) - GŁÓWNY WZÓR:**

- `src/app/(dashboard)/clients/page.tsx` - strona z listą klientów
- PageHeader z "Create" button
- Search bar z debounce
- Filter dropdown
- Table view z pagination
- Loading/Error/Empty states
- Access guard (Provider/Admin)
- TypeScript implementation

**UWAGA**: Plans używa **Grid View** (cards) jako główny sposób wyświetlania, nie table jak Clients!

**Dostępne już z poprzednich punktów:**

- `src/features/plans/api/plansApi.ts` - wszystkie funkcje API (4.1)
- `src/features/plans/hooks/usePlans.ts` - wszystkie hooki (4.2)
- `src/types/plan.ts` - typy TypeScript (PlanDto, GetPlansParams, etc.)

### Folder Structure

```
src/
├── app/
│   └── (dashboard)/
│       └── plans/
│           └── page.tsx          # ⏳ DO UTWORZENIA (4.3)
└── features/
    └── plans/
        ├── api/
        │   └── plansApi.ts       # ✅ ISTNIEJE (4.1)
        ├── hooks/
        │   └── usePlans.ts       # ✅ ISTNIEJE (4.2)
        └── components/
            ├── PlanCard.tsx      # ⏳ DO UTWORZENIA (4.3)
            └── PlanGrid.tsx      # ⏳ DO UTWORZENIA (4.3)
```

---

## 🎯 ZADANIE: Punkt 4.3 - Plan List Page

### Opis Zadania

Stwórz kompletną stronę z listą planów subskrypcji wyświetlaną w formacie grid (karty planów), z możliwością przełączania między grid/list view, filtrowaniem, wyszukiwaniem i zarządzaniem planami.

### Wymagania Funkcjonalne

#### 1. Główny Plik: `src/app/(dashboard)/plans/page.tsx`

**Struktura strony:**

```tsx
import { useState } from "react";
import { useRouter } from "next/navigation";
import { Plus, Grid3x3, List } from "lucide-react";
import { Button } from "@/components/ui/button";
import { PageHeader } from "@/shared/components/PageHeader";
import { usePlans } from "@/features/plans/hooks/usePlans";
import { PlanGrid } from "@/features/plans/components/PlanGrid";
import { PlanList } from "@/features/plans/components/PlanList";
// ... inne importy
```

**Główne sekcje:**

1. **PageHeader** - nagłówek z "Create Plan" button
2. **Filters Bar** - search, status filter, view toggle
3. **Stats Cards** (opcjonalne) - quick stats
4. **Plan Grid/List** - główny content area
5. **Loading State** - skeleton loaders
6. **Error State** - error message z retry
7. **Empty State** - gdy brak planów

#### 2. Plan Card Component: `src/features/plans/components/PlanCard.tsx`

**Wymagania:**

```tsx
interface PlanCardProps {
  plan: PlanDto;
  onEdit?: (id: string) => void;
  onDelete?: (id: string) => void;
  onActivate?: (id: string) => void;
  onDeactivate?: (id: string) => void;
}

/**
 * Plan Card Component - wyświetla pojedynczy plan w formacie karty
 *
 * Features:
 * - Header: Plan name + Popular badge (jeśli isPopular)
 * - Price: Sformatowana cena z currency + billing interval
 * - Description: Plan description
 * - Features list: Wszystkie features z checkmarks
 * - Status badge: Active/Inactive
 * - Actions dropdown: Edit, Delete, Activate/Deactivate
 * - Hover effects: Shadow on hover
 * - Click to details: Całą kartę można kliknąć (navigate to detail)
 *
 * Design:
 * - Card z border
 * - Highlight dla popular plans (border-primary + badge)
 * - Green checkmark icons dla features
 * - Price jest najbardziej widoczny (duży font)
 * - Responsive (full width na mobile)
 */
```

**Layout karty:**

```
┌─────────────────────────────────┐
│ [Popular Badge]      [Actions] │  ← Header (conditional)
│                                 │
│ Plan Name                       │  ← Title (h3)
│ $29.99 / month                  │  ← Price (large, primary color)
│                                 │
│ Description text here...        │  ← Description
│                                 │
│ ✓ Feature 1                     │
│ ✓ Feature 2                     │  ← Features list
│ ✓ Feature 3                     │
│                                 │
│ [Active Badge]                  │  ← Status
└─────────────────────────────────┘
```

**Elementy:**

- **Popular Badge**: Jeśli `plan.isPopular === true`, pokaż badge "Popular" (badge variant="default")
- **Plan Name**: `<h3>` z plan.name
- **Price Display**: Użyj `formatPlanPrice(plan.price, plan.currency)` + `getBillingIntervalText(plan.billingInterval)`
- **Description**: `plan.description` (max 2-3 linie, truncate jeśli dłuższe)
- **Features**: Lista z checkmark icons (`<Check className="h-4 w-4 text-green-500" />`)
- **Status Badge**: Użyj `getPlanStatusBadge(plan.status)` z helper functions
- **Actions Dropdown**: Edit, Delete, Activate/Deactivate (conditional based on status)

**Przykład struktury:**

```tsx
export function PlanCard({
  plan,
  onEdit,
  onDelete,
  onActivate,
  onDeactivate,
}: PlanCardProps) {
  const router = useRouter();

  const handleCardClick = () => {
    router.push(`/plans/${plan.id}`);
  };

  return (
    <Card
      className={cn(
        "cursor-pointer transition-shadow hover:shadow-lg",
        plan.isPopular && "border-primary"
      )}
      onClick={handleCardClick}
    >
      <CardHeader>
        <div className="flex items-start justify-between">
          <div className="space-y-1">
            {plan.isPopular && <Badge variant="default">Popular</Badge>}
            <CardTitle>{plan.name}</CardTitle>
          </div>
          <DropdownMenu>{/* Actions dropdown */}</DropdownMenu>
        </div>
      </CardHeader>

      <CardContent>
        <div className="space-y-4">
          {/* Price */}
          <div className="text-3xl font-bold">
            {formatPlanPrice(plan.price, plan.currency)}
            <span className="text-sm font-normal text-muted-foreground ml-2">
              {getBillingIntervalText(plan.billingInterval)}
            </span>
          </div>

          {/* Description */}
          {plan.description && (
            <p className="text-sm text-muted-foreground line-clamp-2">
              {plan.description}
            </p>
          )}

          {/* Features */}
          <div className="space-y-2">
            {plan.features.map((feature, index) => (
              <div key={index} className="flex items-center gap-2">
                <Check className="h-4 w-4 text-green-500 shrink-0" />
                <span className="text-sm">{feature}</span>
              </div>
            ))}
          </div>

          {/* Status */}
          <div>
            <Badge variant={getPlanStatusBadge(plan.status).variant}>
              {getPlanStatusBadge(plan.status).label}
            </Badge>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
```

#### 3. Plan Grid Component: `src/features/plans/components/PlanGrid.tsx`

**Wymagania:**

```tsx
interface PlanGridProps {
  plans: PlanDto[];
  isLoading?: boolean;
  onEdit?: (id: string) => void;
  onDelete?: (id: string) => void;
  onActivate?: (id: string) => void;
  onDeactivate?: (id: string) => void;
}

/**
 * Plan Grid Component - wyświetla plany w grid layout
 *
 * Features:
 * - Responsive grid: 3 kolumny desktop, 2 tablet, 1 mobile
 * - Loading skeletons (6 cards)
 * - Empty state z message i "Create Plan" CTA
 * - Popular plans na początku (sorted by displayOrder)
 *
 * Grid Layout:
 * - Desktop (lg): 3 columns (grid-cols-3)
 * - Tablet (md): 2 columns (grid-cols-2)
 * - Mobile: 1 column (grid-cols-1)
 * - Gap: gap-6
 */
export function PlanGrid({ plans, isLoading, ...actions }: PlanGridProps) {
  if (isLoading) {
    return <PlanGridSkeleton />;
  }

  if (!plans || plans.length === 0) {
    return <EmptyPlansState />;
  }

  // Sort: popular first, then by displayOrder
  const sortedPlans = [...plans].sort((a, b) => {
    if (a.isPopular && !b.isPopular) return -1;
    if (!a.isPopular && b.isPopular) return 1;
    return (a.displayOrder ?? 999) - (b.displayOrder ?? 999);
  });

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      {sortedPlans.map((plan) => (
        <PlanCard key={plan.id} plan={plan} {...actions} />
      ))}
    </div>
  );
}
```

**Loading Skeleton:**

```tsx
function PlanGridSkeleton() {
  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      {Array.from({ length: 6 }).map((_, i) => (
        <Card key={i}>
          <CardHeader>
            <Skeleton className="h-6 w-3/4" />
          </CardHeader>
          <CardContent className="space-y-4">
            <Skeleton className="h-8 w-1/2" />
            <Skeleton className="h-4 w-full" />
            <Skeleton className="h-4 w-full" />
            <div className="space-y-2">
              <Skeleton className="h-4 w-full" />
              <Skeleton className="h-4 w-full" />
              <Skeleton className="h-4 w-3/4" />
            </div>
          </CardContent>
        </Card>
      ))}
    </div>
  );
}
```

**Empty State:**

```tsx
function EmptyPlansState() {
  const router = useRouter();

  return (
    <div className="flex flex-col items-center justify-center py-12 text-center">
      <div className="rounded-full bg-muted p-3 mb-4">
        <Grid3x3 className="h-8 w-8 text-muted-foreground" />
      </div>
      <h3 className="text-lg font-semibold mb-2">No plans found</h3>
      <p className="text-muted-foreground mb-4 max-w-sm">
        Get started by creating your first subscription plan.
      </p>
      <Button onClick={() => router.push("/plans/new")}>
        <Plus className="h-4 w-4 mr-2" />
        Create Plan
      </Button>
    </div>
  );
}
```

#### 4. Plan List Component (Opcjonalne): `src/features/plans/components/PlanList.tsx`

**Wymagania:**

Alternatywny widok listy (table view) dla użytkowników preferujących tabelę:

```tsx
interface PlanListProps {
  plans: PlanDto[];
  isLoading?: boolean;
  onEdit?: (id: string) => void;
  onDelete?: (id: string) => void;
  onActivate?: (id: string) => void;
  onDeactivate?: (id: string) => void;
}

/**
 * Plan List Component - wyświetla plany w table format
 *
 * Columns:
 * - Name (link to details) + Popular badge
 * - Price (formatted with currency)
 * - Billing Interval
 * - Status (badge)
 * - Actions (dropdown)
 *
 * Features:
 * - Table z hover effects
 * - Loading skeleton (5 rows)
 * - Empty state
 * - Responsive (scroll na mobile)
 */
```

**Struktura tabeli:**

```tsx
export function PlanList({ plans, isLoading, ...actions }: PlanListProps) {
  if (isLoading) {
    return <PlanListSkeleton />;
  }

  if (!plans || plans.length === 0) {
    return <EmptyPlansState />;
  }

  return (
    <div className="rounded-md border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Name</TableHead>
            <TableHead>Price</TableHead>
            <TableHead>Billing</TableHead>
            <TableHead>Status</TableHead>
            <TableHead className="text-right">Actions</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {plans.map((plan) => (
            <TableRow
              key={plan.id}
              className="cursor-pointer hover:bg-muted/50"
            >
              <TableCell>
                <div className="flex items-center gap-2">
                  <span
                    className="font-medium hover:underline"
                    onClick={() => router.push(`/plans/${plan.id}`)}
                  >
                    {plan.name}
                  </span>
                  {plan.isPopular && (
                    <Badge variant="default" className="text-xs">
                      Popular
                    </Badge>
                  )}
                </div>
              </TableCell>
              <TableCell>
                {formatPlanPrice(plan.price, plan.currency)}
              </TableCell>
              <TableCell>
                {getBillingIntervalText(plan.billingInterval)}
              </TableCell>
              <TableCell>
                <Badge variant={getPlanStatusBadge(plan.status).variant}>
                  {getPlanStatusBadge(plan.status).label}
                </Badge>
              </TableCell>
              <TableCell className="text-right">
                {/* Actions dropdown */}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}
```

#### 5. Main Page Implementation: `src/app/(dashboard)/plans/page.tsx`

**Pełna struktura strony:**

```tsx
"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { Plus, Grid3x3, List, Search } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { PageHeader } from "@/shared/components/PageHeader";
import {
  usePlans,
  useDeletePlan,
  useActivatePlan,
  useDeactivatePlan,
} from "@/features/plans/hooks/usePlans";
import { PlanGrid } from "@/features/plans/components/PlanGrid";
import { PlanList } from "@/features/plans/components/PlanList";
import { useDebounce } from "@/shared/hooks/useDebounce";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import type { PlanStatus } from "@/types/plan";

export default function PlansPage() {
  const router = useRouter();

  // State
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState<PlanStatus | "all">("all");
  const [viewMode, setViewMode] = useState<"grid" | "list">("grid");
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [planToDelete, setPlanToDelete] = useState<string | null>(null);

  // Debounced search
  const debouncedSearch = useDebounce(searchTerm, 300);

  // Fetch plans
  const {
    data: plansData,
    isLoading,
    error,
    refetch,
  } = usePlans({
    searchTerm: debouncedSearch,
    status: statusFilter === "all" ? undefined : statusFilter,
  });

  // Mutations
  const { mutate: deletePlan, isPending: isDeleting } = useDeletePlan();
  const { mutate: activatePlan, isPending: isActivating } = useActivatePlan();
  const { mutate: deactivatePlan, isPending: isDeactivating } =
    useDeactivatePlan();

  // Handlers
  const handleEdit = (id: string) => {
    router.push(`/plans/${id}/edit`);
  };

  const handleDelete = (id: string) => {
    setPlanToDelete(id);
    setDeleteDialogOpen(true);
  };

  const confirmDelete = () => {
    if (planToDelete) {
      deletePlan(planToDelete, {
        onSuccess: () => {
          setDeleteDialogOpen(false);
          setPlanToDelete(null);
        },
      });
    }
  };

  const handleActivate = (id: string) => {
    activatePlan(id);
  };

  const handleDeactivate = (id: string) => {
    deactivatePlan(id);
  };

  // Error state
  if (error) {
    return (
      <div className="space-y-6">
        <PageHeader
          title="Subscription Plans"
          description="Manage your subscription plans and pricing"
        />
        <div className="flex flex-col items-center justify-center py-12">
          <p className="text-destructive mb-4">Failed to load plans</p>
          <Button onClick={() => refetch()}>Retry</Button>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <PageHeader
        title="Subscription Plans"
        description="Manage your subscription plans and pricing"
        action={
          <Button onClick={() => router.push("/plans/new")}>
            <Plus className="h-4 w-4 mr-2" />
            Create Plan
          </Button>
        }
      />

      {/* Filters Bar */}
      <div className="flex flex-col sm:flex-row gap-4">
        {/* Search */}
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input
            placeholder="Search plans..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="pl-10"
          />
        </div>

        {/* Status Filter */}
        <Select
          value={statusFilter}
          onValueChange={(value) =>
            setStatusFilter(value as PlanStatus | "all")
          }
        >
          <SelectTrigger className="w-full sm:w-[180px]">
            <SelectValue placeholder="Filter by status" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Plans</SelectItem>
            <SelectItem value="active">Active</SelectItem>
            <SelectItem value="inactive">Inactive</SelectItem>
            <SelectItem value="draft">Draft</SelectItem>
          </SelectContent>
        </Select>

        {/* View Toggle */}
        <div className="flex gap-2">
          <Button
            variant={viewMode === "grid" ? "default" : "outline"}
            size="icon"
            onClick={() => setViewMode("grid")}
          >
            <Grid3x3 className="h-4 w-4" />
          </Button>
          <Button
            variant={viewMode === "list" ? "default" : "outline"}
            size="icon"
            onClick={() => setViewMode("list")}
          >
            <List className="h-4 w-4" />
          </Button>
        </div>
      </div>

      {/* Content */}
      {viewMode === "grid" ? (
        <PlanGrid
          plans={plansData?.items || []}
          isLoading={isLoading}
          onEdit={handleEdit}
          onDelete={handleDelete}
          onActivate={handleActivate}
          onDeactivate={handleDeactivate}
        />
      ) : (
        <PlanList
          plans={plansData?.items || []}
          isLoading={isLoading}
          onEdit={handleEdit}
          onDelete={handleDelete}
          onActivate={handleActivate}
          onDeactivate={handleDeactivate}
        />
      )}

      {/* Delete Confirmation Dialog */}
      <AlertDialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Are you sure?</AlertDialogTitle>
            <AlertDialogDescription>
              This will delete the plan. This action cannot be undone. Active
              subscriptions using this plan will not be affected.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction
              onClick={confirmDelete}
              disabled={isDeleting}
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
            >
              {isDeleting ? "Deleting..." : "Delete"}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}
```

#### 6. Helper Functions (z types/plan.ts - już istnieją)

Użyj istniejących helper functions:

```typescript
import {
  formatPlanPrice,
  getBillingIntervalText,
  getPlanStatusBadge,
} from "@/types/plan";
```

#### 7. Required Imports

```tsx
// Next.js
import { useRouter } from "next/navigation";

// React
import { useState } from "react";

// Icons
import {
  Plus,
  Grid3x3,
  List,
  Search,
  Check,
  MoreVertical,
  Edit,
  Trash,
  PlayCircle,
  PauseCircle,
} from "lucide-react";

// UI Components
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";

// Shared Components
import { PageHeader } from "@/shared/components/PageHeader";

// Hooks
import {
  usePlans,
  useDeletePlan,
  useActivatePlan,
  useDeactivatePlan,
} from "@/features/plans/hooks/usePlans";
import { useDebounce } from "@/shared/hooks/useDebounce";

// Types
import type { PlanDto, PlanStatus } from "@/types/plan";

// Utils
import { cn } from "@/lib/utils";
import {
  formatPlanPrice,
  getBillingIntervalText,
  getPlanStatusBadge,
} from "@/types/plan";
```

---

## 📚 PRZYKŁADY IMPLEMENTACJI

### Przykład 1: PlanCard Component (Kompletny)

```tsx
"use client";

import { useRouter } from "next/navigation";
import {
  Check,
  MoreVertical,
  Edit,
  Trash,
  PlayCircle,
  PauseCircle,
} from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { cn } from "@/lib/utils";
import {
  formatPlanPrice,
  getBillingIntervalText,
  getPlanStatusBadge,
} from "@/types/plan";
import type { PlanDto } from "@/types/plan";

interface PlanCardProps {
  plan: PlanDto;
  onEdit?: (id: string) => void;
  onDelete?: (id: string) => void;
  onActivate?: (id: string) => void;
  onDeactivate?: (id: string) => void;
}

export function PlanCard({
  plan,
  onEdit,
  onDelete,
  onActivate,
  onDeactivate,
}: PlanCardProps) {
  const router = useRouter();
  const statusBadge = getPlanStatusBadge(plan.status);

  const handleCardClick = (e: React.MouseEvent) => {
    // Don't navigate if clicking on actions
    if ((e.target as HTMLElement).closest("[data-no-navigate]")) {
      return;
    }
    router.push(`/plans/${plan.id}`);
  };

  const handleAction = (e: React.MouseEvent, action: () => void) => {
    e.stopPropagation();
    action();
  };

  return (
    <Card
      className={cn(
        "cursor-pointer transition-all hover:shadow-lg",
        plan.isPopular && "border-primary ring-2 ring-primary/20"
      )}
      onClick={handleCardClick}
    >
      <CardHeader>
        <div className="flex items-start justify-between">
          <div className="space-y-2 flex-1">
            {plan.isPopular && (
              <Badge variant="default" className="mb-2">
                ⭐ Popular
              </Badge>
            )}
            <CardTitle className="text-xl">{plan.name}</CardTitle>
          </div>

          {/* Actions Dropdown */}
          <div data-no-navigate>
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="ghost" size="icon">
                  <MoreVertical className="h-4 w-4" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end">
                <DropdownMenuItem
                  onClick={(e) => handleAction(e, () => onEdit?.(plan.id))}
                >
                  <Edit className="h-4 w-4 mr-2" />
                  Edit
                </DropdownMenuItem>

                {plan.isActive ? (
                  <DropdownMenuItem
                    onClick={(e) =>
                      handleAction(e, () => onDeactivate?.(plan.id))
                    }
                  >
                    <PauseCircle className="h-4 w-4 mr-2" />
                    Deactivate
                  </DropdownMenuItem>
                ) : (
                  <DropdownMenuItem
                    onClick={(e) =>
                      handleAction(e, () => onActivate?.(plan.id))
                    }
                  >
                    <PlayCircle className="h-4 w-4 mr-2" />
                    Activate
                  </DropdownMenuItem>
                )}

                <DropdownMenuItem
                  onClick={(e) => handleAction(e, () => onDelete?.(plan.id))}
                  className="text-destructive"
                >
                  <Trash className="h-4 w-4 mr-2" />
                  Delete
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          </div>
        </div>
      </CardHeader>

      <CardContent>
        <div className="space-y-4">
          {/* Price */}
          <div>
            <div className="text-4xl font-bold text-primary">
              {formatPlanPrice(plan.price, plan.currency)}
            </div>
            <div className="text-sm text-muted-foreground mt-1">
              {getBillingIntervalText(plan.billingInterval)}
            </div>
          </div>

          {/* Description */}
          {plan.description && (
            <p className="text-sm text-muted-foreground line-clamp-2">
              {plan.description}
            </p>
          )}

          {/* Features */}
          <div className="space-y-2 pt-2 border-t">
            {plan.features.slice(0, 5).map((feature, index) => (
              <div key={index} className="flex items-start gap-2">
                <Check className="h-4 w-4 text-green-500 mt-0.5 shrink-0" />
                <span className="text-sm">{feature}</span>
              </div>
            ))}
            {plan.features.length > 5 && (
              <p className="text-xs text-muted-foreground pl-6">
                +{plan.features.length - 5} more features
              </p>
            )}
          </div>

          {/* Footer */}
          <div className="flex items-center justify-between pt-2 border-t">
            <Badge variant={statusBadge.variant}>{statusBadge.label}</Badge>

            {plan.maxUsers && (
              <span className="text-xs text-muted-foreground">
                Up to {plan.maxUsers} users
              </span>
            )}
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
```

### Przykład 2: PlanGrid Component z Loading i Empty State

```tsx
"use client";

import { useRouter } from "next/navigation";
import { Plus, Grid3x3 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { PlanCard } from "./PlanCard";
import type { PlanDto } from "@/types/plan";

interface PlanGridProps {
  plans: PlanDto[];
  isLoading?: boolean;
  onEdit?: (id: string) => void;
  onDelete?: (id: string) => void;
  onActivate?: (id: string) => void;
  onDeactivate?: (id: string) => void;
}

export function PlanGrid({
  plans,
  isLoading,
  onEdit,
  onDelete,
  onActivate,
  onDeactivate,
}: PlanGridProps) {
  if (isLoading) {
    return <PlanGridSkeleton />;
  }

  if (!plans || plans.length === 0) {
    return <EmptyPlansState />;
  }

  // Sort: popular first, then by displayOrder
  const sortedPlans = [...plans].sort((a, b) => {
    if (a.isPopular && !b.isPopular) return -1;
    if (!a.isPopular && b.isPopular) return 1;
    return (a.displayOrder ?? 999) - (b.displayOrder ?? 999);
  });

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      {sortedPlans.map((plan) => (
        <PlanCard
          key={plan.id}
          plan={plan}
          onEdit={onEdit}
          onDelete={onDelete}
          onActivate={onActivate}
          onDeactivate={onDeactivate}
        />
      ))}
    </div>
  );
}

function PlanGridSkeleton() {
  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      {Array.from({ length: 6 }).map((_, i) => (
        <Card key={i}>
          <CardHeader>
            <Skeleton className="h-6 w-3/4" />
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <Skeleton className="h-10 w-1/2 mb-2" />
              <Skeleton className="h-4 w-1/3" />
            </div>
            <Skeleton className="h-4 w-full" />
            <Skeleton className="h-4 w-full" />
            <div className="space-y-2 pt-2">
              <Skeleton className="h-4 w-full" />
              <Skeleton className="h-4 w-full" />
              <Skeleton className="h-4 w-3/4" />
            </div>
            <div className="pt-2">
              <Skeleton className="h-6 w-20" />
            </div>
          </CardContent>
        </Card>
      ))}
    </div>
  );
}

function EmptyPlansState() {
  const router = useRouter();

  return (
    <div className="flex flex-col items-center justify-center py-16 px-4 text-center">
      <div className="rounded-full bg-muted p-4 mb-4">
        <Grid3x3 className="h-10 w-10 text-muted-foreground" />
      </div>
      <h3 className="text-xl font-semibold mb-2">No subscription plans yet</h3>
      <p className="text-muted-foreground mb-6 max-w-md">
        Create your first subscription plan to start offering services to your
        clients. You can set up different pricing tiers and features.
      </p>
      <Button onClick={() => router.push("/plans/new")} size="lg">
        <Plus className="h-5 w-5 mr-2" />
        Create Your First Plan
      </Button>
    </div>
  );
}
```

### Przykład 3: Actions Dropdown z Conditional Items

```tsx
<DropdownMenu>
  <DropdownMenuTrigger asChild>
    <Button variant="ghost" size="icon">
      <MoreVertical className="h-4 w-4" />
    </Button>
  </DropdownMenuTrigger>
  <DropdownMenuContent align="end">
    <DropdownMenuItem onClick={() => router.push(`/plans/${plan.id}`)}>
      View Details
    </DropdownMenuItem>

    <DropdownMenuItem onClick={() => onEdit?.(plan.id)}>
      <Edit className="h-4 w-4 mr-2" />
      Edit Plan
    </DropdownMenuItem>

    {plan.isActive ? (
      <DropdownMenuItem onClick={() => onDeactivate?.(plan.id)}>
        <PauseCircle className="h-4 w-4 mr-2" />
        Deactivate
      </DropdownMenuItem>
    ) : (
      <DropdownMenuItem onClick={() => onActivate?.(plan.id)}>
        <PlayCircle className="h-4 w-4 mr-2" />
        Activate
      </DropdownMenuItem>
    )}

    <DropdownMenuItem
      onClick={() => onDelete?.(plan.id)}
      className="text-destructive focus:text-destructive"
    >
      <Trash className="h-4 w-4 mr-2" />
      Delete Plan
    </DropdownMenuItem>
  </DropdownMenuContent>
</DropdownMenu>
```

---

## 🚨 WAŻNE UWAGI

### Grid Layout Requirements

- **Desktop (lg)**: 3 kolumny - `grid-cols-1 md:grid-cols-2 lg:grid-cols-3`
- **Tablet (md)**: 2 kolumny
- **Mobile**: 1 kolumna
- **Gap**: `gap-6` (1.5rem)

### Plan Card Design

- **Popular Plans**: Border primary + ring + badge
- **Price**: Największy element (text-4xl, font-bold, primary color)
- **Features**: Max 5 widocznych, reszta "+X more features"
- **Hover**: Shadow-lg transition
- **Click**: Navigate to detail page
- **Actions**: Stop propagation, dropdown menu

### View Toggle

- Grid view (domyślny): Cards w grid layout
- List view (opcjonalny): Table format
- Toggle buttons w filters bar
- Persist wybór w state (opcjonalnie localStorage)

### Empty State

- Pokaż gdy `plans.length === 0`
- Friendly message
- "Create Plan" CTA button
- Icon (Grid3x3)

### Loading State

- Skeleton cards (6 kart)
- Zachowaj grid layout
- Match card structure

### Error Handling

```tsx
if (error) {
  return (
    <div className="space-y-6">
      <PageHeader title="Subscription Plans" />
      <div className="flex flex-col items-center justify-center py-12">
        <p className="text-destructive mb-4">
          Failed to load plans: {error.message}
        </p>
        <Button onClick={() => refetch()}>Retry</Button>
      </div>
    </div>
  );
}
```

### Access Control

- Strona dostępna tylko dla Provider/Admin
- Conditional fetching w usePlans hooku
- Middleware już sprawdza uprawnienia

### Delete Confirmation

```tsx
<AlertDialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
  <AlertDialogContent>
    <AlertDialogHeader>
      <AlertDialogTitle>Delete Plan?</AlertDialogTitle>
      <AlertDialogDescription>
        This will permanently delete the plan "{planName}". Active subscriptions
        will not be affected.
      </AlertDialogDescription>
    </AlertDialogHeader>
    <AlertDialogFooter>
      <AlertDialogCancel>Cancel</AlertDialogCancel>
      <AlertDialogAction
        onClick={confirmDelete}
        disabled={isDeleting}
        className="bg-destructive"
      >
        {isDeleting ? "Deleting..." : "Delete Plan"}
      </AlertDialogAction>
    </AlertDialogFooter>
  </AlertDialogContent>
</AlertDialog>
```

---

## 💡 DODATKOWE WSKAZÓWKI

### Testing Strategy

Po implementacji przetestuj:

- [ ] Grid view wyświetla karty w 3 kolumnach (desktop)
- [ ] Popular plans są na początku
- [ ] Sorting po displayOrder działa
- [ ] Search filtruje plany poprawnie (debounce 300ms)
- [ ] Status filter działa (all/active/inactive/draft)
- [ ] View toggle przełącza grid/list
- [ ] Click na kartę nawiguje do detail page
- [ ] Actions dropdown działa (edit/delete/activate/deactivate)
- [ ] Delete confirmation dialog pokazuje się
- [ ] Loading skeleton wyświetla 6 kart
- [ ] Empty state pokazuje się gdy brak planów
- [ ] Error state z retry button działa
- [ ] Responsive design (mobile/tablet/desktop)
- [ ] Price formatting z currency jest poprawny
- [ ] Features list skraca się po 5 elementach

### Performance Tips

```typescript
// Memoize sorted plans
const sortedPlans = useMemo(() => {
  return [...plans].sort((a, b) => {
    if (a.isPopular && !b.isPopular) return -1;
    if (!a.isPopular && b.isPopular) return 1;
    return (a.displayOrder ?? 999) - (b.displayOrder ?? 999);
  });
}, [plans]);
```

### Accessibility

- [ ] Keyboard navigation działa (Tab, Enter)
- [ ] Focus indicators są widoczne
- [ ] Screen reader support (ARIA labels)
- [ ] Card click area jest wystarczająco duży
- [ ] Actions dropdown keyboard accessible

### Future Enhancements (Nice to Have)

- [ ] Drag & drop reordering (displayOrder)
- [ ] Bulk operations (select multiple, activate/deactivate)
- [ ] Export plans to CSV
- [ ] Duplicate plan functionality
- [ ] Plan comparison view
- [ ] Quick stats cards (total plans, active plans, total revenue)
- [ ] Filter by currency
- [ ] Filter by billing interval
- [ ] Save view preference (grid/list) to localStorage

### Integration Checklist

Po ukończeniu 4.3, w kolejnych punktach:

- [ ] 4.4 - Plan Detail Page (navigate z PlanCard)
- [ ] 4.5 - Plan Form Component (używany w Create/Edit)
- [ ] 4.6 - Create/Edit Plan Pages (używają PlanForm)

---

## 📞 POMOC

### Troubleshooting Common Issues

**Problem**: Grid nie jest responsive

```css
/* Sprawdź czy używasz poprawnych breakpoints */
grid-cols-1 md:grid-cols-2 lg:grid-cols-3
```

**Problem**: Popular badge nie pokazuje się

```tsx
// Sprawdź czy plan.isPopular === true (boolean)
{
  plan.isPopular && <Badge variant="default">Popular</Badge>;
}
```

**Problem**: Click na Actions nie działa (nawiguje do detail)

```tsx
// Użyj data-no-navigate attribute
<div data-no-navigate>
  <DropdownMenu>...</DropdownMenu>
</div>;

// I w handleCardClick:
if ((e.target as HTMLElement).closest("[data-no-navigate]")) {
  return;
}
```

**Problem**: Features list jest za długa

```tsx
// Ogranicz do 5 features
{plan.features.slice(0, 5).map(...)}

// Pokaż informację o pozostałych
{plan.features.length > 5 && (
  <p className="text-xs">+{plan.features.length - 5} more</p>
)}
```

**Problem**: Price formatting nie działa

```tsx
// Użyj helper function z types/plan.ts
import { formatPlanPrice } from "@/types/plan";
{
  formatPlanPrice(plan.price, plan.currency);
}
```

**Problem**: Empty state nie pokazuje się

```tsx
// Sprawdź kolejność warunków
if (isLoading) return <Skeleton />;
if (!plans || plans.length === 0) return <EmptyState />;
```

---

**Wersja**: 1.0  
**Data**: 2025-11-03  
**Autor**: Orbito Development Team  
**Status**: ✅ **UKOŃCZONE (2025-11-03)**  
**Priorytet**: 🔴 KRYTYCZNE (Blocking dla FAZY 4.4-4.6) ✅ **ROZWIĄZANE**  
**Estimated Time**: 6-8 godzin  
**Actual Time**: ~6 godzin  
**Dependencies**:

- ✅ 4.1 Plans API Service (UKOŃCZONE)
- ✅ 4.2 Plans Hooks (UKOŃCZONE)

**✅ IMPLEMENTACJA UKOŃCZONA (2025-11-03):**

- ✅ Plans List Page (`src/app/(dashboard)/plans/page.tsx`) - kompletna implementacja
- ✅ PlanCard component (`src/features/plans/components/PlanCard.tsx`) - karta planu
- ✅ PlanGrid component (`src/features/plans/components/PlanGrid.tsx`) - grid layout
- ✅ PlanList component (`src/features/plans/components/PlanList.tsx`) - table view
- ✅ Wszystkie wymagane funkcje zaimplementowane
- ✅ TypeScript types zdefiniowane
- ✅ Responsive design zoptymalizowany
