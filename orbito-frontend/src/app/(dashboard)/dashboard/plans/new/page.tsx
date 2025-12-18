"use client";

import { PlanForm } from "@/features/plans/components/PlanForm";
import { useCreatePlan } from "@/features/plans/hooks/usePlanMutations";
import {
  formInputToCreateCommand,
  type PlanFormInput,
} from "@/features/plans/schemas/plan.schemas";
import { Button } from "@/shared/ui/button";
import { ArrowLeft } from "lucide-react";
import Link from "next/link";

export default function CreatePlanPage() {
  const mutation = useCreatePlan();

  const handleSubmit = (data: PlanFormInput) => {
    const command = formInputToCreateCommand(data);
    mutation.mutate({ data: command });
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Link href="/dashboard/plans">
          <Button variant="ghost" size="icon">
            <ArrowLeft className="h-4 w-4" />
          </Button>
        </Link>
        <div>
          <h1 className="text-3xl font-bold">Create Plan</h1>
          <p className="text-muted-foreground">
            Add a new subscription plan for your clients
          </p>
        </div>
      </div>

      <div className="rounded-lg border bg-card p-6">
        <PlanForm onSubmit={handleSubmit} isSubmitting={mutation.isPending} />
      </div>
    </div>
  );
}
