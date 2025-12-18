"use client";

import { usePlans } from "../hooks/usePlans";
import { PlanCard } from "./PlanCard";
import { Skeleton } from "@/shared/ui/skeleton";

export function PlansGrid() {
  const { plans, isLoading, error } = usePlans();

  if (isLoading) {
    return <PlansGridSkeleton />;
  }

  if (error) {
    return (
      <div className="rounded-lg border border-destructive bg-destructive/10 p-4">
        <p className="text-sm text-destructive">
          Error loading plans: {error.message}
        </p>
      </div>
    );
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
