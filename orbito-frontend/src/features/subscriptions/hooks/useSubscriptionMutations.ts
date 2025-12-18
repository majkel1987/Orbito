import { useQueryClient } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import {
  usePostApiSubscriptions,
  usePostApiSubscriptionsIdCancel,
  usePostApiSubscriptionsIdSuspend,
  usePostApiSubscriptionsIdResume,
} from "@/core/api/generated/subscriptions/subscriptions";

/**
 * Hook for creating a new subscription
 */
export function useCreateSubscription() {
  const queryClient = useQueryClient();
  const router = useRouter();

  return usePostApiSubscriptions({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ["api", "subscriptions"] });
        toast.success("Subscription created successfully!");
        router.push("/dashboard/subscriptions");
      },
      onError: (error: Error) => {
        toast.error(`Failed to create subscription: ${error.message}`);
      },
    },
  });
}

/**
 * Hook for canceling a subscription
 */
export function useCancelSubscription(subscriptionId: string) {
  const queryClient = useQueryClient();
  const router = useRouter();

  return usePostApiSubscriptionsIdCancel({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ["api", "subscriptions"] });
        queryClient.invalidateQueries({
          queryKey: ["api", "subscriptions", subscriptionId],
        });
        toast.success("Subscription canceled successfully!");
        router.push("/dashboard/subscriptions");
      },
      onError: (error: Error) => {
        toast.error(`Failed to cancel subscription: ${error.message}`);
      },
    },
  });
}

/**
 * Hook for suspending a subscription
 */
export function useSuspendSubscription(subscriptionId: string) {
  const queryClient = useQueryClient();

  return usePostApiSubscriptionsIdSuspend({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ["api", "subscriptions"] });
        queryClient.invalidateQueries({
          queryKey: ["api", "subscriptions", subscriptionId],
        });
        toast.success("Subscription suspended successfully!");
      },
      onError: (error: Error) => {
        toast.error(`Failed to suspend subscription: ${error.message}`);
      },
    },
  });
}

/**
 * Hook for resuming a subscription
 */
export function useResumeSubscription(subscriptionId: string) {
  const queryClient = useQueryClient();

  return usePostApiSubscriptionsIdResume({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ["api", "subscriptions"] });
        queryClient.invalidateQueries({
          queryKey: ["api", "subscriptions", subscriptionId],
        });
        toast.success("Subscription resumed successfully!");
      },
      onError: (error: Error) => {
        toast.error(`Failed to resume subscription: ${error.message}`);
      },
    },
  });
}
