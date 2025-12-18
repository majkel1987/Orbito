"use client";

import { use, useEffect } from "react";
import { useRouter } from "next/navigation";
import { useGetApiSubscriptionPlansId } from "@/core/api/generated/subscription-plans/subscription-plans";
import type { SubscriptionPlanDto } from "@/features/plans/types/plan.types";
import { useDeletePlan } from "@/features/plans/hooks/usePlanMutations";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/shared/ui/alert-dialog";
import { Skeleton } from "@/shared/ui/skeleton";

export default function DeletePlanPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = use(params);
  const router = useRouter();

  const { data, isLoading, error } = useGetApiSubscriptionPlansId(id) as {
    data: SubscriptionPlanDto | undefined;
    isLoading: boolean;
    error: Error | null;
  };

  const mutation = useDeletePlan();

  // Auto-open dialog when component mounts
  useEffect(() => {
    if (!isLoading && !data && !error) {
      router.push("/dashboard/plans");
    }
  }, [isLoading, data, error, router]);

  const handleDelete = () => {
    mutation.mutate({ id });
  };

  const handleCancel = () => {
    router.push(`/dashboard/plans/${id}`);
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <Skeleton className="h-64 w-96" />
      </div>
    );
  }

  if (error || !data) {
    return (
      <AlertDialog open={true}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Error</AlertDialogTitle>
            <AlertDialogDescription>
              {error?.message || "Plan not found"}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogAction onClick={() => router.push("/dashboard/plans")}>
              OK
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    );
  }

  return (
    <AlertDialog open={true}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>Delete Plan</AlertDialogTitle>
          <AlertDialogDescription>
            Are you sure you want to delete <strong>{data.name}</strong>? This
            action cannot be undone.
            {data.activeSubscriptionsCount &&
              data.activeSubscriptionsCount > 0 && (
                <>
                  <br />
                  <br />
                  <span className="text-destructive font-medium">
                    Warning: This plan has {data.activeSubscriptionsCount}{" "}
                    active subscription(s). Deleting it may affect those
                    subscriptions.
                  </span>
                </>
              )}
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel onClick={handleCancel}>Cancel</AlertDialogCancel>
          <AlertDialogAction
            onClick={handleDelete}
            className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
            disabled={mutation.isPending}
          >
            {mutation.isPending ? "Deleting..." : "Delete"}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
