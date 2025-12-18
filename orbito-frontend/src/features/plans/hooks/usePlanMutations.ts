import { useRouter } from "next/navigation";
import { useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import {
  usePostApiSubscriptionPlans,
  usePutApiSubscriptionPlansId,
  useDeleteApiSubscriptionPlansId,
} from "@/core/api/generated/subscription-plans/subscription-plans";
import type {
  CreateSubscriptionPlanCommand,
  UpdateSubscriptionPlanCommand,
} from "@/core/api/generated/models";

/**
 * Hook for creating a subscription plan
 */
export function useCreatePlan() {
  const router = useRouter();
  const queryClient = useQueryClient();

  return usePostApiSubscriptionPlans({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: ["/api/SubscriptionPlans"],
        });
        toast.success("Plan created successfully!");
        router.push("/dashboard/plans");
      },
      onError: (error: Error) => {
        toast.error(error.message || "Failed to create plan");
      },
    },
  });
}

/**
 * Hook for updating a subscription plan
 */
export function useUpdatePlan(id: string) {
  const router = useRouter();
  const queryClient = useQueryClient();

  return usePutApiSubscriptionPlansId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: ["/api/SubscriptionPlans"],
        });
        queryClient.invalidateQueries({
          queryKey: [`/api/SubscriptionPlans/${id}`],
        });
        toast.success("Plan updated successfully!");
        router.push(`/dashboard/plans/${id}`);
      },
      onError: (error: Error) => {
        toast.error(error.message || "Failed to update plan");
      },
    },
  });
}

/**
 * Hook for deleting a subscription plan
 */
export function useDeletePlan() {
  const router = useRouter();
  const queryClient = useQueryClient();

  return useDeleteApiSubscriptionPlansId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: ["/api/SubscriptionPlans"],
        });
        toast.success("Plan deleted successfully!");
        router.push("/dashboard/plans");
      },
      onError: (error: Error) => {
        toast.error(error.message || "Failed to delete plan");
      },
    },
  });
}
