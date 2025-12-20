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
      onSuccess: async () => {
        // Invalidate all subscription queries with any parameters
        await queryClient.invalidateQueries({
          predicate: (query) =>
            Array.isArray(query.queryKey) && query.queryKey[0] === "/api/Subscriptions",
        });
        toast.success("Subscription created successfully!");
        router.push("/dashboard/subscriptions");
      },
      onError: (error: unknown) => {
        console.error("=== SUBSCRIPTION CREATE ERROR ===");
        console.error("Error:", error);

        // Type guard for Error objects
        const errorMessage = error instanceof Error ? error.message : "Unknown error";

        // Type guard for response errors (Axios-like structure)
        const responseError = error && typeof error === 'object' && 'response' in error ? error.response : null;
        const responseData = responseError && typeof responseError === 'object' && 'data' in responseError
          ? (responseError.data as Record<string, unknown>)
          : null;
        const detailMessage = responseData
          ? (responseData.detail as string) || (responseData.title as string) || errorMessage
          : errorMessage;

        console.error("Error message:", errorMessage);
        console.error("================================");

        toast.error(`Failed to create subscription: ${detailMessage}`);
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
      onSuccess: async () => {
        // Invalidate all subscription queries with any parameters
        await queryClient.invalidateQueries({
          predicate: (query) =>
            Array.isArray(query.queryKey) && query.queryKey[0] === "/api/Subscriptions",
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
      onSuccess: async () => {
        // Invalidate all subscription queries with any parameters
        await queryClient.invalidateQueries({
          predicate: (query) =>
            Array.isArray(query.queryKey) && query.queryKey[0] === "/api/Subscriptions",
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
      onSuccess: async () => {
        // Invalidate all subscription queries with any parameters
        await queryClient.invalidateQueries({
          predicate: (query) =>
            Array.isArray(query.queryKey) && query.queryKey[0] === "/api/Subscriptions",
        });
        toast.success("Subscription resumed successfully!");
      },
      onError: (error: Error) => {
        toast.error(`Failed to resume subscription: ${error.message}`);
      },
    },
  });
}
