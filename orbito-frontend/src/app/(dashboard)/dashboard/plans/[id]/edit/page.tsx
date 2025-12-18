"use client";

import { use } from "react";
import { useGetApiSubscriptionPlansId } from "@/core/api/generated/subscription-plans/subscription-plans";
import type { SubscriptionPlanDto } from "@/features/plans/types/plan.types";
import {
  parseBillingPeriod,
  parsePlanFeatures,
} from "@/features/plans/types/plan.types";
import { PlanForm } from "@/features/plans/components/PlanForm";
import { useUpdatePlan } from "@/features/plans/hooks/usePlanMutations";
import {
  formInputToUpdateCommand,
  type PlanFormInput,
  enumTypeToInterval,
} from "@/features/plans/schemas/plan.schemas";
import type { BillingPeriodType } from "@/core/api/generated/models/billingPeriodType";
import { Button } from "@/shared/ui/button";
import { Skeleton } from "@/shared/ui/skeleton";
import { ArrowLeft } from "lucide-react";
import Link from "next/link";

export default function EditPlanPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = use(params);

  const { data, isLoading, error } = useGetApiSubscriptionPlansId(id) as {
    data: SubscriptionPlanDto | undefined;
    isLoading: boolean;
    error: Error | null;
  };

  const mutation = useUpdatePlan(id);

  if (isLoading) {
    return <EditPlanSkeleton />;
  }

  if (error) {
    return (
      <div className="rounded-lg border border-destructive bg-destructive/10 p-4">
        <p className="text-sm text-destructive">
          Error loading plan: {error.message}
        </p>
      </div>
    );
  }

  if (!data) {
    return (
      <div className="text-center py-8 text-muted-foreground">Plan not found</div>
    );
  }

  const features = parsePlanFeatures(data.featuresJson);
  const billingPeriod = parseBillingPeriod(data.billingPeriod);

  // Konwertuj backend data na form format
  const defaultValues: Partial<PlanFormInput> = {
    name: data.name,
    description: data.description || "",
    amount: data.amount,
    currency: data.currency,
    // Backend zwraca string "1 Monthly" - musimy wyodrębnić enum i skonwertować
    interval:
      billingPeriod.label === "Monthly"
        ? "Monthly"
        : billingPeriod.label === "Yearly"
          ? "Yearly"
          : "Quarterly",
    trialPeriodDays: data.trialPeriodDays,
    features: features.map((f) => ({ value: f })),
    isActive: data.isActive,
    isPublic: data.isPublic,
    sortOrder: data.sortOrder,
  };

  const handleSubmit = (formData: PlanFormInput) => {
    const command = formInputToUpdateCommand(id, formData);
    mutation.mutate({ id, data: command });
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Link href={`/dashboard/plans/${id}`}>
          <Button variant="ghost" size="icon">
            <ArrowLeft className="h-4 w-4" />
          </Button>
        </Link>
        <div>
          <h1 className="text-3xl font-bold">Edit Plan</h1>
          <p className="text-muted-foreground">Update {data.name}</p>
        </div>
      </div>

      <div className="rounded-lg border bg-card p-6">
        <PlanForm
          defaultValues={defaultValues}
          onSubmit={handleSubmit}
          isSubmitting={mutation.isPending}
        />
      </div>
    </div>
  );
}

function EditPlanSkeleton() {
  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Skeleton className="h-10 w-10" />
        <div className="space-y-2 flex-1">
          <Skeleton className="h-8 w-48" />
          <Skeleton className="h-4 w-64" />
        </div>
      </div>
      <Skeleton className="h-[600px]" />
    </div>
  );
}
